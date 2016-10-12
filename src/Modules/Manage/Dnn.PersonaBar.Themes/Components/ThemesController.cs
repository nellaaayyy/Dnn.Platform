﻿#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

#region Usings



#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using Dnn.PersonaBar.Themes.Components.DTO;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Installer.Log;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins;
using DotNetNuke.UI.Skins.Controls;
using Image = System.Drawing.Image;

namespace Dnn.PersonaBar.Themes.Components
{
    public class ThemesController : ServiceLocator<IThemesController, ThemesController>, IThemesController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ThemesController));

        private static readonly IList<string> ImageExtensions = new List<string>() {".jpg", ".png"};

        protected override Func<IThemesController> GetFactory()
        {
            return () => new ThemesController();
        }

        #region Public Methods

        /// <summary>
        /// Get Skins.
        /// </summary>
        /// <param name="portalSettings"></param>
        /// <param name="level">portal level or host level.</param>
        /// <returns></returns>
        public IList<ThemeInfo> GetLayouts(PortalSettings portalSettings, ThemeLevel level)
        {
            var themes = new List<ThemeInfo>();
            if ((level & ThemeLevel.Site) == ThemeLevel.Site)
            {
                themes.AddRange(GetThemes(ThemeType.Skin, Path.Combine(portalSettings.HomeSystemDirectoryMapPath, SkinController.RootSkin)));
            }

            if ((level & ThemeLevel.Global) == ThemeLevel.Global)
            {
                themes.AddRange(GetThemes(ThemeType.Skin, Path.Combine(Globals.HostMapPath, SkinController.RootSkin)));
            }

            return themes;
        }

        /// <summary>
        /// Get Containers.
        /// </summary>
        /// <param name="portalSettings"></param>
        /// <param name="level">portal level or host level.</param>
        /// <returns></returns>
        public IList<ThemeInfo> GetContainers(PortalSettings portalSettings, ThemeLevel level)
        {
            var themes = new List<ThemeInfo>();
            if ((level & ThemeLevel.Site) == ThemeLevel.Site)
            {
                themes.AddRange(GetThemes(ThemeType.Container, Path.Combine(portalSettings.HomeSystemDirectoryMapPath, SkinController.RootContainer)));
            }

            if ((level & ThemeLevel.Global) == ThemeLevel.Global)
            {
                themes.AddRange(GetThemes(ThemeType.Container, Path.Combine(Globals.HostMapPath, SkinController.RootContainer)));
            }

            return themes;
        }

        /// <summary>
        /// get skin files in the skin.
        /// </summary>
        /// <param name="portalSettings"></param>
        /// <param name="theme"></param>
        /// <returns></returns>
        public IList<ThemeFileInfo> GetThemeFiles(PortalSettings portalSettings, ThemeInfo theme)
        {
            var themePath = Path.Combine(Globals.ApplicationMapPath, theme.Path);
            var themeFiles = new List<ThemeFileInfo>();

            if (Directory.Exists(themePath))
            {
                bool fallbackSkin;
                string strRootSkin;
                if (theme.Type == ThemeType.Skin)
                {
                    strRootSkin = SkinController.RootSkin.ToLower();
                    fallbackSkin = IsFallbackSkin(themePath);
                }
                else
                {
                    strRootSkin = SkinController.RootContainer.ToLower();
                    fallbackSkin = IsFallbackContainer(themePath);
                }

                var strSkinType = themePath.IndexOf(Globals.HostMapPath, StringComparison.InvariantCultureIgnoreCase) != -1 ? "G" : "L";

                var canDeleteSkin = SkinController.CanDeleteSkin(themePath, portalSettings.HomeDirectoryMapPath);
                var arrFiles = Directory.GetFiles(themePath, "*.ascx");

                foreach (var strFile in arrFiles)
                {
                    var file = strFile.ToLowerInvariant();

                    var themeFile = new ThemeFileInfo();

                    var imagePath = string.Empty;
                    foreach (var ext in ImageExtensions)
                    {
                        var path = Path.ChangeExtension(file, ext);
                        if (File.Exists(path))
                        {
                            imagePath = path;
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(imagePath))
                    {
                        themeFile.Thumbnail = CreateThumbnail(imagePath);
                    }


                    themeFile.Name = Path.GetFileNameWithoutExtension(file);

                    var strUrl = file.Substring(strFile.IndexOf("\\" + strRootSkin + "\\", StringComparison.InvariantCultureIgnoreCase))
                        .Replace(".ascx", "")
                        .Replace("\\", "/");

                    themeFile.Path = "[" + strSkinType + "]" + strUrl;
                    themeFile.CanDelete = (UserController.Instance.GetCurrentUserInfo().IsSuperUser || strSkinType == "L")
                                          && (!fallbackSkin && canDeleteSkin);

                    themeFiles.Add(themeFile);
                }
            }

            return themeFiles;
        }

        /// <summary>
        /// update portal skin.
        /// </summary>
        /// <param name="portalId">portal id.</param>
        /// <param name="themeFile">skin info.</param>
        /// <param name="scope">change skin or container.</param>
        public void ApplyTheme(int portalId, ThemeFileInfo themeFile, ApplyThemeScope scope)
        {
            switch (themeFile.Type)
            {
                case ThemeType.Container:
                    if ((scope & ApplyThemeScope.Site) == ApplyThemeScope.Site)
                    {
                        SkinController.SetSkin(SkinController.RootContainer, portalId, SkinType.Portal, themeFile.Path);
                    }

                    if ((scope & ApplyThemeScope.Edit) == ApplyThemeScope.Edit)
                    {
                        SkinController.SetSkin(SkinController.RootContainer, portalId, SkinType.Admin, themeFile.Path);
                    }
                    break;
                case ThemeType.Skin:
                    if ((scope & ApplyThemeScope.Site) == ApplyThemeScope.Site)
                    {
                        SkinController.SetSkin(SkinController.RootSkin, portalId, SkinType.Portal, themeFile.Path);
                    }
                    if ((scope & ApplyThemeScope.Edit) == ApplyThemeScope.Edit)
                    {
                        SkinController.SetSkin(SkinController.RootSkin, portalId, SkinType.Admin, themeFile.Path);
                    }
                    DataCache.ClearPortalCache(portalId, true);
                    break;
            }
        }

        /// <summary>
        /// delete a skin or container.
        /// </summary>
        /// <param name="portalSettings"></param>
        /// <param name="themeFile"></param>
        public void DeleteTheme(PortalSettings portalSettings, ThemeFileInfo themeFile)
        {
            var themePath = SkinController.FormatSkinSrc(themeFile.Path, portalSettings);
            var user = UserController.Instance.GetCurrentUserInfo();

            if (!user.IsSuperUser && themePath.IndexOf("\\portals\\_default\\", StringComparison.InvariantCultureIgnoreCase) != Null.NullInteger)
            {
                throw new SecurityException("NoPermission");
            }

            File.Delete(Path.Combine(Globals.ApplicationMapPath, themePath));
            DataCache.ClearPortalCache(portalSettings.PortalId, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalSettings"></param>
        /// <param name="theme"></param>
        public void DeleteThemePackage(PortalSettings portalSettings, ThemeInfo theme)
        {
            var themePath = SkinController.FormatSkinSrc(theme.Path, portalSettings);
            var user = UserController.Instance.GetCurrentUserInfo();

            if (!user.IsSuperUser  && themePath.IndexOf("\\portals\\_default\\", StringComparison.InvariantCultureIgnoreCase) != Null.NullInteger)
            {
                throw new SecurityException("NoPermission");
            }

            if (theme.Type == ThemeType.Skin)
            {
                var skinPackage = SkinController.GetSkinPackage(portalSettings.PortalId, theme.PackageName, "Skin");
                if (skinPackage != null)
                {
                    throw new InvalidOperationException("UsePackageUninstall");
                }

                if (Directory.Exists(themePath))
                {
                    Globals.DeleteFolderRecursive(themePath);
                }
                if (Directory.Exists(themePath.Replace("\\" + SkinController.RootSkin.ToLower() + "\\", "\\" + SkinController.RootContainer + "\\")))
                {
                    Globals.DeleteFolderRecursive(themePath.Replace("\\" + SkinController.RootSkin.ToLower() + "\\", "\\" + SkinController.RootContainer + "\\"));
                }
            }
            else if (theme.Type == ThemeType.Container)
            {
                var skinPackage = SkinController.GetSkinPackage(portalSettings.PortalId, theme.PackageName, "Container");
                if (skinPackage != null)
                {
                    throw new InvalidOperationException("UsePackageUninstall");
                }

                if (Directory.Exists(themePath))
                {
                    Globals.DeleteFolderRecursive(themePath);
                }
            }
        }

        /// <summary>
        /// Update Theme Attributes.
        /// </summary>
        /// <param name="portalSettings"></param>
        /// <param name="updateTheme"></param>
        public void UpdateTheme(PortalSettings portalSettings, UpdateThemeInfo updateTheme)
        {
            var themePath = SkinController.FormatSkinSrc(updateTheme.Path, portalSettings);

            var objStreamReader = File.OpenText(themePath);
            var strSkin = objStreamReader.ReadToEnd();
            objStreamReader.Close();
            var strTag = "<dnn:" + updateTheme.Token + " runat=\"server\" id=\"dnn" + updateTheme.Token + "\"";
            var intOpenTag = strSkin.IndexOf(strTag);
            if (intOpenTag != -1)
            {
                var intCloseTag = strSkin.IndexOf(" />", intOpenTag);
                var strAttribute = updateTheme.Key;
                var intStartAttribute = strSkin.IndexOf(strAttribute, intOpenTag);
                string strValue = updateTheme.Value;
                if (intStartAttribute != -1 && intStartAttribute < intCloseTag)
                {
                    //remove attribute
                    var intEndAttribute = strSkin.IndexOf("\" ", intStartAttribute);
                    strSkin = strSkin.Substring(0, intStartAttribute) + strSkin.Substring(intEndAttribute + 2);
                }
                //add attribute
                strSkin = strSkin.Insert(intOpenTag + strTag.Length, " " + strAttribute + "=\"" + strValue + "\"");

                File.SetAttributes(themePath, FileAttributes.Normal);
                var objStream = File.CreateText(themePath);
                objStream.WriteLine(strSkin);
                objStream.Close();

                UpdateManifest(portalSettings, updateTheme);
            }
        }

        /// <summary>
        /// Parse skin package.
        /// </summary>
        /// <param name="portalSettings"></param>
        /// <param name="theme"></param>
        /// <param name="parseType"></param>
        public void ParseTheme(PortalSettings portalSettings, ThemeInfo theme, ParseType parseType)
        {
            var strRootPath = Null.NullString;
            switch (theme.Level)
            {
                case ThemeLevel.Global: //global
                    strRootPath = Globals.HostMapPath;
                    break;
                case ThemeLevel.Site: //local
                    strRootPath = portalSettings.HomeDirectoryMapPath;
                    break;
            }
            var objSkinFiles = new SkinFileProcessor(strRootPath, theme.Type == ThemeType.Container ? SkinController.RootContainer : SkinController.RootSkin, theme.PackageName);
            var arrSkinFiles = new ArrayList();

            var strFolder = Path.Combine(Globals.ApplicationMapPath, theme.Path);
            if (Directory.Exists(strFolder))
            {
                var arrFiles = Directory.GetFiles(strFolder);
                foreach (var strFile in arrFiles)
                {
                    switch (Path.GetExtension(strFile))
                    {
                        case ".htm":
                        case ".html":
                        case ".css":
                            if (strFile.ToLower().IndexOf(Globals.glbAboutPage.ToLower()) < 0)
                            {
                                arrSkinFiles.Add(strFile);
                            }
                            break;
                        case ".ascx":
                            if (File.Exists(strFile.Replace(".ascx", ".htm")) == false && File.Exists(strFile.Replace(".ascx", ".html")) == false)
                            {
                                arrSkinFiles.Add(strFile);
                            }
                            break;
                    }
                }
            }
            switch (parseType)
            {
                case ParseType.Localized: //localized
                    objSkinFiles.ProcessList(arrSkinFiles, SkinParser.Localized);
                    break;
                case ParseType.Portable: //portable
                    objSkinFiles.ProcessList(arrSkinFiles, SkinParser.Portable);
                    break;
            }
        }

        #endregion

        #region Private Methods

        private void UpdateManifest(PortalSettings portalSettings, UpdateThemeInfo updateTheme)
        {
            var themePath = SkinController.FormatSkinSrc(updateTheme.Path, portalSettings);
            if (File.Exists(themePath.Replace(".ascx", ".htm")))
            {
                var strFile = themePath.Replace(".ascx", ".xml");
                if (File.Exists(strFile) == false)
                {
                    strFile = strFile.Replace(Path.GetFileName(strFile), "skin.xml");
                }
                XmlDocument xmlDoc = null;
                try
                {
                    xmlDoc = new XmlDocument();
                    xmlDoc.Load(strFile);
                }
                catch
                {
                    xmlDoc.InnerXml = "<Objects></Objects>";
                }
                var xmlToken = xmlDoc.DocumentElement.SelectSingleNode("descendant::Object[Token='[" + updateTheme.Token + "]']");
                if (xmlToken == null)
                {
                    //add token
                    string strToken = "<Token>[" + updateTheme.Token + "]</Token><Settings></Settings>";
                    xmlToken = xmlDoc.CreateElement("Object");
                    xmlToken.InnerXml = strToken;
                    xmlDoc.SelectSingleNode("Objects").AppendChild(xmlToken);
                    xmlToken = xmlDoc.DocumentElement.SelectSingleNode("descendant::Object[Token='[" + updateTheme.Token + "]']");
                }
                var strValue = updateTheme.Value;

                var blnUpdate = false;
                foreach (XmlNode xmlSetting in xmlToken.SelectNodes(".//Settings/Setting"))
                {
                    if (xmlSetting.SelectSingleNode("Name").InnerText == updateTheme.Key)
                    {
                        xmlSetting.SelectSingleNode("Value").InnerText = strValue;
                        blnUpdate = true;
                    }
                }
                if (blnUpdate == false)
                {
                    var strSetting = "<Name>" + updateTheme.Key + "</Name><Value>" + strValue + "</Value>";
                    XmlNode xmlSetting = xmlDoc.CreateElement("Setting");
                    xmlSetting.InnerXml = strSetting;
                    xmlToken.SelectSingleNode("Settings").AppendChild(xmlSetting);
                }
                try
                {
                    if (File.Exists(strFile))
                    {
                        File.SetAttributes(strFile, FileAttributes.Normal);
                    }
                    var objStream = File.CreateText(strFile);
                    var strXML = xmlDoc.InnerXml;
                    strXML = strXML.Replace("><", ">" + Environment.NewLine + "<");
                    objStream.WriteLine(strXML);
                    objStream.Close();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }

        }

        private static IList<ThemeInfo> GetThemes(ThemeType type, string strRoot)
        {
            var themes = new List<ThemeInfo>();
            if (Directory.Exists(strRoot))
            {
                foreach (var strFolder in Directory.GetDirectories(strRoot))
                {
                    var strName = strFolder.Substring(strFolder.LastIndexOf("\\") + 1);
                    if (strName != "_default")
                    {
                        var themePath = strFolder.Replace(Globals.ApplicationMapPath, "").ToLowerInvariant();
                        var isFallback = type == ThemeType.Skin ? IsFallbackSkin(themePath) : IsFallbackContainer(themePath);
                        var canDelete = !isFallback && SkinController.CanDeleteSkin(strFolder, PortalSettings.Current.HomeDirectoryMapPath);

                        themes.Add(new ThemeInfo()
                        {
                            PackageName = strName,
                            Type = type,
                            Path = themePath,
                            CanDelete = canDelete
                        });
                    }
                }
            }

            return themes;
        }

        private static string CreateThumbnail(string strImage)
        {
            var blnCreate = true;

            var strThumbnail = strImage.Replace(Path.GetFileName(strImage), "thumbnail_" + Path.GetFileName(strImage));

            //check if image has changed
            if (File.Exists(strThumbnail))
            {
                if (File.GetLastWriteTime(strThumbnail) == File.GetLastWriteTime(strImage))
                {
                    blnCreate = false;
                }
            }
            if (blnCreate)
            {
                const int intSize = 150; //size of the thumbnail 
                Image objImage;
                try
                {
                    objImage = Image.FromFile(strImage);

                    //scale the image to prevent distortion
                    int intHeight;
                    int intWidth;
                    double dblScale;
                    if (objImage.Height > objImage.Width)
                    {
                        //The height was larger, so scale the width 
                        dblScale = (double)intSize / objImage.Height;
                        intHeight = intSize;
                        intWidth = Convert.ToInt32(objImage.Width * dblScale);
                    }
                    else
                    {
                        //The width was larger, so scale the height 
                        dblScale = (double)intSize / objImage.Width;
                        intWidth = intSize;
                        intHeight = Convert.ToInt32(objImage.Height * dblScale);
                    }

                    //create the thumbnail image
                    var objThumbnail = objImage.GetThumbnailImage(intWidth, intHeight, null, IntPtr.Zero);

                    //delete the old file ( if it exists )
                    if (File.Exists(strThumbnail))
                    {
                        File.Delete(strThumbnail);
                    }

                    //save the thumbnail image 
                    objThumbnail.Save(strThumbnail, objImage.RawFormat);

                    //set the file attributes
                    File.SetAttributes(strThumbnail, FileAttributes.Normal);
                    File.SetLastWriteTime(strThumbnail, File.GetLastWriteTime(strImage));

                    //tidy up
                    objImage.Dispose();
                    objThumbnail.Dispose();
                }
                catch (Exception ex) //problem creating thumbnail
                {
                    Logger.Error(ex);
                }
            }

            strThumbnail = Globals.ApplicationPath + "/" + strThumbnail.Substring(strThumbnail.IndexOf("portals\\"));
            strThumbnail = strThumbnail.Replace("\\", "/");

            //return thumbnail filename
            return strThumbnail;
        }

        private string GetSkinPath(string type, string root, string name)
        {
            var strPath = Null.NullString;
            switch (type)
            {
                case "G":  //global
                    strPath = Globals.HostPath + root + "/" + name;
                    break;
                case "L": //local
                    strPath = PortalSettings.Current.HomeDirectory + root + "/" + name;
                    break;
            }
            return strPath;
        }

        private static bool IsFallbackContainer(string skinPath)
        {
            var strDefaultContainerPath = (Globals.HostMapPath + SkinController.RootContainer + SkinDefaults.GetSkinDefaults(SkinDefaultType.SkinInfo).Folder).Replace("/", "\\");
            if (strDefaultContainerPath.EndsWith("\\"))
            {
                strDefaultContainerPath = strDefaultContainerPath.Substring(0, strDefaultContainerPath.Length - 1);
            }
            return skinPath.IndexOf(strDefaultContainerPath, StringComparison.CurrentCultureIgnoreCase) != -1;
        }

        private static bool IsFallbackSkin(string skinPath)
        {
            var strDefaultSkinPath = (Globals.HostMapPath + SkinController.RootSkin + SkinDefaults.GetSkinDefaults(SkinDefaultType.SkinInfo).Folder).Replace("/", "\\");
            if (strDefaultSkinPath.EndsWith("\\"))
            {
                strDefaultSkinPath = strDefaultSkinPath.Substring(0, strDefaultSkinPath.Length - 1);
            }
            return skinPath.ToLowerInvariant() == strDefaultSkinPath.ToLowerInvariant();
        }

        #endregion
    }
}