// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Code Analysis results, point to "Suppress Message", and click 
// "In Suppression File".
// You do not need to add suppressions to this file manually.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId = "chash", Scope = "resource", Target = "Microsoft.Owin.Security.OpenIdConnect.Resources.resources", Justification = "following OpenIdConnect specification naming")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId = "idtoken", Scope = "resource", Target = "Microsoft.Owin.Security.OpenIdConnect.Resources.resources", Justification = "following OpenIdConnect specification naming")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Scope = "member", Target = "Microsoft.Owin.Security.OpenIdConnect.OpenIdConnectAuthenticationOptions.#Redirect_Uri", Justification = "following OpenIdConnect specification naming")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Logout", Scope = "member", Target = "Microsoft.Owin.Security.OpenIdConnect.OpenIdConnectAuthenticationOptions.#Post_Logout_Redirect_Uri", Justification = "following OpenIdConnect specification naming")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Scope = "member", Target = "Microsoft.Owin.Security.OpenIdConnect.OpenIdConnectAuthenticationOptions.#Post_Logout_Redirect_Uri", Justification = "users do not need to set a uri")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Microsoft.Owin.Logging.LoggerExtensions.WriteWarning(Microsoft.Owin.Logging.ILogger,System.String,System.String[])", Scope = "member", Target = "Microsoft.Owin.Security.OpenIdConnect.OpenIdConnectAuthenticationHandler.#RetrieveNonce(Microsoft.IdentityModel.Protocols.OpenIdConnectMessage)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Scope = "member", Target = "Microsoft.Owin.Security.OpenIdConnect.OpenIdConnectAuthenticationHandler.#DeleteExpiredNonceCookies()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Microsoft.Owin.Logging.LoggerExtensions.WriteError(Microsoft.Owin.Logging.ILogger,System.String)", Scope = "member", Target = "Microsoft.Owin.Security.OpenIdConnect.OpenIdConnectAuthenticationHandler.#DeleteExpiredNonceCookies()")]
