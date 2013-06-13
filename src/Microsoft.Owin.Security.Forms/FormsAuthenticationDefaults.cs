﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Owin.Security.Forms
{
    /// <summary>
    /// Default values related to cookie-based authentication middleware
    /// </summary>
    public static class FormsAuthenticationDefaults
    {
        /// <summary>
        /// The default value used for FormsAuthenticationOptions.AuthenticationType
        /// </summary>
        public const string AuthenticationType = "Forms";

        /// <summary>
        /// The AuthenticationType used specifically by the UseApplicationSignInCookie extension method.
        /// </summary>
        public const string ApplicationAuthenticationType = "Application";

        /// <summary>
        /// The AuthenticationType used specifically by the UseExternalSignInCookie extension method.
        /// </summary>
        public const string ExternalAuthenticationType = "External";

        /// <summary>
        /// The prefix used to provide a default FormsAuthenticationOptions.CookieName
        /// </summary>
        public const string CookiePrefix = ".AspNet.";

        /// <summary>
        /// The default value used by UseApplicationSignInCookie for the
        /// FormsAuthenticationOptions.LoginPath
        /// </summary>
        public const string LoginPath = "/Account/Login";

        /// <summary>
        /// The default value used by UseApplicationSignInCookie for the
        /// FormsAuthenticationOptions.LogoutPath
        /// </summary>
        public const string LogoutPath = "/Account/Logout";

        /// <summary>
        /// The default value of the FormsAuthenticationOptions.ReturnUrlParameter
        /// </summary>
        public const string ReturnUrlParameter = "ReturnUrl";
    }
}
