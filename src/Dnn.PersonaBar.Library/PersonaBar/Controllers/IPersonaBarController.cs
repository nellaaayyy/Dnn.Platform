﻿// DotNetNuke® - http://www.dnnsoftware.com
//
// Copyright (c) 2002-2016, DNN Corp.
// All rights reserved.

using Dnn.PersonaBar.Library.PersonaBar.Model;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Library.PersonaBar.Controllers
{
    /// <summary>
    /// Interface responsible to manage the PersonaBar structure by User's Roles and Sku
    /// </summary>
    public interface IPersonaBarController
    {
        /// <summary>
        /// Gets the menu structure of the persona bar (for both mobile and desktop)
        /// </summary>
        /// <param name="portalSettings"></param>
        /// <param name="userInfo">the user that will be used to filter the menu</param>
        /// <returns>Persona bar menu structure for the user</returns>
        PersonaBarMenu GetMenu(PortalSettings portalSettings, UserInfo userInfo);

        /// <summary>
        /// Whether the menu item is visible.
        /// </summary>
        /// <param name="portalSettings">Portal Settings.</param>
        /// <param name="user">User Info.</param>
        /// <param name="menuItem">The menu item.</param>
        /// <returns></returns>
        bool IsVisible(PortalSettings portalSettings, UserInfo user, MenuItem menuItem);
    }
}
