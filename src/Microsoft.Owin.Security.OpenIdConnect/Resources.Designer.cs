//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microsoft.Owin.Security.OpenIdConnect {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Microsoft.Owin.Security.OpenIdConnect.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to BackchannelTimeout cannot be less or equal to TimeSpan.Zero..
        /// </summary>
        internal static string ArgsException_BackchallelLessThanZero {
            get {
                return ResourceManager.GetString("ArgsException_BackchallelLessThanZero", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &quot;OpenIdConnectMessage.Error was not null, indicating an error. Error: &apos;{0}&apos;. Error_Description (may be empty): &apos;{1}&apos;. Error_Uri (may be empty): &apos;{2}&apos;.&quot;.
        /// </summary>
        internal static string Exception_OpenIdConnectMessageError {
            get {
                return ResourceManager.GetString("Exception_OpenIdConnectMessageError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to OIDC_20001: The query string for Logout is not a well formed URI. The runtime cannot redirect. Redirect uri: &apos;{0}&apos;..
        /// </summary>
        internal static string Exception_RedirectUri_LogoutQueryString_IsNotWellFormed {
            get {
                return ResourceManager.GetString("Exception_RedirectUri_LogoutQueryString_IsNotWellFormed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An ICertificateValidator cannot be specified at the same time as an HttpMessageHandler unless it is a WebRequestHandler..
        /// </summary>
        internal static string Exception_ValidatorHandlerMismatch {
            get {
                return ResourceManager.GetString("Exception_ValidatorHandlerMismatch", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to validate the &apos;id_token&apos;, no suitable ISecurityTokenValidator was found for: &apos;{0}&apos;.&quot;.
        /// </summary>
        internal static string UnableToValidateToken {
            get {
                return ResourceManager.GetString("UnableToValidateToken", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The Validated Security Token must be of type JwtSecurityToken, but instead its type is: &apos;{0}&apos;..
        /// </summary>
        internal static string ValidatedSecurityTokenNotJwt {
            get {
                return ResourceManager.GetString("ValidatedSecurityTokenNotJwt", resourceCulture);
            }
        }
    }
}
