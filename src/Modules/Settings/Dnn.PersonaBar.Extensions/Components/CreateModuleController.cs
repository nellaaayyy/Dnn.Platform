﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Dnn.PersonaBar.Extensions.Components.Dto;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Framework;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Installer;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins.Controls;

namespace Dnn.PersonaBar.Extensions.Components
{
    public class CreateModuleController : ServiceLocator<ICreateModuleController, CreateModuleController>, ICreateModuleController
    {
        protected override Func<ICreateModuleController> GetFactory()
        {
            return () => new CreateModuleController();
        }

        public bool CreateModule(CreateModuleDto createModuleDto, out string newPageUrl, out string errorMessage)
        {
            errorMessage = string.Empty;
            newPageUrl = string.Empty;
            switch (createModuleDto.Type)
            {
                case CreateModuleType.New:
                    errorMessage = CreateNewModule(createModuleDto, out newPageUrl);
                    break;
                case CreateModuleType.Control:
                    errorMessage = CreateModuleFromControl(createModuleDto, out newPageUrl);
                    break;
                case CreateModuleType.Manifest:
                    errorMessage = CreateModuleFromManifest(createModuleDto, out newPageUrl);
                    break;
            }

            return string.IsNullOrEmpty(errorMessage);
        }

        private string CreateNewModule(CreateModuleDto createModuleDto, out string newPageUrl)
        {
            newPageUrl = string.Empty;
            if (string.IsNullOrEmpty(createModuleDto.ModuleFolder))
            {
                return "NoModuleFolder";
            }

            if (string.IsNullOrEmpty(createModuleDto.Language))
            {
                return "LanguageError";
            }

            //remove spaces so file is created correctly
            var controlSrc = createModuleDto.FileName.Replace(" ", "");
            if (InvalidFilename(controlSrc))
            {
                return "InvalidFilename";
            }

            if (String.IsNullOrEmpty(controlSrc))
            {
                return "MissingControl";
            }
            if (String.IsNullOrEmpty(createModuleDto.ModuleName))
            {
                return "MissingFriendlyname";
            }
            if (!controlSrc.EndsWith(".ascx"))
            {
                controlSrc += ".ascx";
            }

            var uniqueName = true;
            foreach (var package in PackageController.Instance.GetExtensionPackages(Null.NullInteger))
            {
                if (package.Name.Equals(createModuleDto.ModuleName, StringComparison.InvariantCultureIgnoreCase) 
                    || package.FriendlyName.Equals(createModuleDto.ModuleName, StringComparison.InvariantCultureIgnoreCase))
                {
                    uniqueName = false;
                    break;
                }
            }

            if (!uniqueName)
            {
                return "NonuniqueName";
            }
            //First create the control
            createModuleDto.FileName = controlSrc;
            var message = CreateControl(createModuleDto);
            if (string.IsNullOrEmpty(message))
            {
                //Next import the control
                message = CreateModuleFromControl(createModuleDto, out newPageUrl);
            }

            return message;
        }

        private string CreateModuleFromControl(CreateModuleDto createModuleDto, out string newPageUrl)
        {
            newPageUrl = string.Empty;
            if (string.IsNullOrEmpty(createModuleDto.FileName))
            {
                return "NoControl";
            }

            try
            {
                string folder = PathUtils.Instance.RemoveTrailingSlash(GetSourceFolder(createModuleDto));
                string friendlyName = createModuleDto.ModuleName;
                string name = createModuleDto.ModuleName;
                string moduleControl = "DesktopModules/" + folder + "/" + createModuleDto.FileName;

                var packageInfo = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p =>
                                    p.Name.Equals(createModuleDto.ModuleName, StringComparison.InvariantCultureIgnoreCase)
                                     || p.FriendlyName.Equals(createModuleDto.ModuleName, StringComparison.InvariantCultureIgnoreCase));
                if (packageInfo != null)
                {

                    return "NonuniqueName";
                }

                var package = new PackageInfo
                {
                    Name = name,
                    FriendlyName = friendlyName,
                    Description = createModuleDto.Description,
                    Version = new Version(1, 0, 0),
                    PackageType = "Module",
                    License = Util.PACKAGE_NoLicense
                };

                //Save Package
                PackageController.Instance.SaveExtensionPackage(package);

                var objDesktopModule = new DesktopModuleInfo
                {
                    DesktopModuleID = Null.NullInteger,
                    ModuleName = name,
                    FolderName = folder,
                    FriendlyName = friendlyName,
                    Description = createModuleDto.Description,
                    IsPremium = false,
                    IsAdmin = false,
                    Version = "01.00.00",
                    BusinessControllerClass = "",
                    CompatibleVersions = "",
                    Dependencies = "",
                    Permissions = "",
                    PackageID = package.PackageID
                };

                objDesktopModule.DesktopModuleID = DesktopModuleController.SaveDesktopModule(objDesktopModule, false, true);

                //Add module to all portals
                DesktopModuleController.AddDesktopModuleToPortals(objDesktopModule.DesktopModuleID);

                //Save module definition
                var moduleDefinition = new ModuleDefinitionInfo();

                moduleDefinition.ModuleDefID = Null.NullInteger;
                moduleDefinition.DesktopModuleID = objDesktopModule.DesktopModuleID;
                moduleDefinition.FriendlyName = friendlyName;
                moduleDefinition.DefaultCacheTime = 0;

                moduleDefinition.ModuleDefID = ModuleDefinitionController.SaveModuleDefinition(moduleDefinition, false, true);

                //Save module control
                var objModuleControl = new ModuleControlInfo();

                objModuleControl.ModuleControlID = Null.NullInteger;
                objModuleControl.ModuleDefID = moduleDefinition.ModuleDefID;
                objModuleControl.ControlKey = "";
                objModuleControl.ControlSrc = moduleControl;
                objModuleControl.ControlTitle = "";
                objModuleControl.ControlType = SecurityAccessLevel.View;
                objModuleControl.HelpURL = "";
                objModuleControl.IconFile = "";
                objModuleControl.ViewOrder = 0;
                objModuleControl.SupportsPartialRendering = false;

                ModuleControlController.AddModuleControl(objModuleControl);

                if (createModuleDto.AddPage)
                {
                    newPageUrl = CreateNewPage(moduleDefinition);
                }

                return string.Empty;
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
                return "CreateModuleFailed";
            }
        }

        private string CreateModuleFromManifest(CreateModuleDto createModuleDto, out string newPageUrl)
        {
            newPageUrl = string.Empty;
            if (string.IsNullOrEmpty(createModuleDto.Manifest))
            {
                return "MissingManifest";
            }

            try
            {
                var folder = PathUtils.Instance.RemoveTrailingSlash(GetSourceFolder(createModuleDto));
                var manifest = Path.Combine(Globals.ApplicationMapPath, "~/DesktopModules/" + folder + "/" + createModuleDto.Manifest);
                var installer = new Installer(manifest, Globals.ApplicationMapPath, true);

                if (installer.IsValid)
                {
                    installer.InstallerInfo.Log.Logs.Clear();
                    installer.Install();

                    if (installer.IsValid)
                    {
                        if (createModuleDto.AddPage)
                        {
                            var desktopModule =
                                DesktopModuleController.GetDesktopModuleByPackageID(installer.InstallerInfo.PackageID);
                            if (desktopModule != null && desktopModule.ModuleDefinitions.Count > 0)
                            {
                                foreach (var kvp in desktopModule.ModuleDefinitions)
                                {
                                    var moduleDefinition = kvp.Value;

                                    newPageUrl = CreateNewPage(moduleDefinition);
                                    break;
                                }
                            }
                        }

                        return string.Empty;
                    }
                    else
                    {
                        return "InstallError";
                    }
                }
                else
                {
                    return "InstallError";
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
                return "CreateModuleFailed";
            }
        }

        private string CreateNewPage(ModuleDefinitionInfo moduleDefinition)
        {
            if (PortalSettings.Current == null)
            {
                return string.Empty;
            }

            var portalId = PortalSettings.Current.PortalId;
            var tabName = "Test " + moduleDefinition.FriendlyName + " Page";
            var tabPath = Globals.GenerateTabPath(Null.NullInteger, tabName);
            var tabId = TabController.GetTabByTabPath(portalId, tabPath, Null.NullString);
            if (tabId == Null.NullInteger)
            {
                //Create a new page
                var newTab = new TabInfo();
                newTab.TabName = tabName;
                newTab.ParentId = Null.NullInteger;
                newTab.PortalID = portalId;
                newTab.IsVisible = true;
                newTab.TabID = TabController.Instance.AddTabBefore(newTab, PortalSettings.Current.AdminTabId);
                var objModule = new ModuleInfo();
                objModule.Initialize(portalId);
                objModule.PortalID = portalId;
                objModule.TabID = newTab.TabID;
                objModule.ModuleOrder = Null.NullInteger;
                objModule.ModuleTitle = moduleDefinition.FriendlyName;
                objModule.PaneName = Globals.glbDefaultPane;
                objModule.ModuleDefID = moduleDefinition.ModuleDefID;
                objModule.InheritViewPermissions = true;
                objModule.AllTabs = false;
                ModuleController.Instance.AddModule(objModule);

                return Globals.NavigateURL(newTab.TabID);
            }

            return string.Empty;
        }

        private static bool InvalidFilename(string fileName)
        {
            var invalidFilenameChars = RegexUtils.GetCachedRegex("[" + Regex.Escape(new string(Path.GetInvalidFileNameChars())) + "]");
            return invalidFilenameChars.IsMatch(fileName);
        }

        private string CreateControl(CreateModuleDto createModuleDto)
        {
            var folder = PathUtils.Instance.RemoveTrailingSlash(GetSourceFolder(createModuleDto));
            var className = GetClassName(createModuleDto);
            var moduleControlPath = Path.Combine(Globals.ApplicationMapPath, "DesktopModules/" + folder + "/" + createModuleDto.FileName);
            var message = Null.NullString;

            var source = string.Format(LoadControlTemplate(), createModuleDto.Language, className);

            //reset attributes
            if (File.Exists(moduleControlPath))
            {
                message = "FileExists";
            }
            else
            {
                using (var stream = File.CreateText(moduleControlPath))
                {
                    stream.WriteLine(source);
                }
            }
            return message;
        }

        private string LoadControlTemplate()
        {
            var personaBarFolder = Library.Constants.PersonaBarRelativePath.Replace("~/", "");
            var filePath = Path.Combine(Globals.ApplicationMapPath, personaBarFolder, "data/ModuleControlTemplate.resources");
            return File.ReadAllText(filePath, Encoding.UTF8);
        }

        private string GetSourceFolder(CreateModuleDto createModuleDto)
        {
            var folder = Null.NullString;
            if (!string.IsNullOrEmpty(createModuleDto.OwnerFolder))
            {
                folder += createModuleDto.OwnerFolder + "/";
            }
            if (!string.IsNullOrEmpty(createModuleDto.ModuleFolder))
            {
                folder += createModuleDto.ModuleFolder + "/";
            }
            return folder;
        }

        private string GetClassName(CreateModuleDto createModuleDto)
        {
            var className = Null.NullString;
            if (!String.IsNullOrEmpty(createModuleDto.OwnerFolder))
            {
                className += createModuleDto.OwnerFolder + ".";
            }
            if (!String.IsNullOrEmpty(createModuleDto.ModuleFolder))
            {
                className += createModuleDto.ModuleFolder;
            }
            //return class and remove any spaces that might appear in folder structure
            return className.Replace(" ", "");
        }
    }
}