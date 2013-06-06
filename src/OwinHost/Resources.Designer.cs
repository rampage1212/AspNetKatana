﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18033
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OwinHost {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("OwinHost.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to Unexpected command line argument &apos;{0}&apos;.
        /// </summary>
        internal static string CommandException_UnexpectedCommandLineArgument {
            get {
                return ResourceManager.GetString("CommandException_UnexpectedCommandLineArgument", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unknown switch type.
        /// </summary>
        internal static string CommandException_UnknownSwitchType {
            get {
                return ResourceManager.GetString("CommandException_UnknownSwitchType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Formats include [Namespace.TypeName[.MethodName][, Assembly]] and [Assembly]. MethodName and Assembly are optional. The default value is determined by finding a class where TypeName is &apos;Startup&apos;, MethodName is &apos;Configuration&apos;, and Namespace and Assembly are the same..
        /// </summary>
        internal static string ProgramOutput_AppStartupDescription {
            get {
                return ResourceManager.GetString("ProgramOutput_AppStartupDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Names a specific application entry point..
        /// </summary>
        internal static string ProgramOutput_AppStartupParameter {
            get {
                return ResourceManager.GetString("ProgramOutput_AppStartupParameter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Loads an assembly to provide custom startup control..
        /// </summary>
        internal static string ProgramOutput_BootOption {
            get {
                return ResourceManager.GetString("ProgramOutput_BootOption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Command line error - {0}.
        /// </summary>
        internal static string ProgramOutput_CommandLineError {
            get {
                return ResourceManager.GetString("ProgramOutput_CommandLineError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Specifies the target directory of the application..
        /// </summary>
        internal static string ProgramOutput_DirectoryOption {
            get {
                return ResourceManager.GetString("ProgramOutput_DirectoryOption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Environment Variables:.
        /// </summary>
        internal static string ProgramOutput_EnvironmentVariablesHeader {
            get {
                return ResourceManager.GetString("ProgramOutput_EnvironmentVariablesHeader", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Example: OwinHost --port 5000 HelloWorld.Startup.
        /// </summary>
        internal static string ProgramOutput_Example {
            get {
                return ResourceManager.GetString("ProgramOutput_Example", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Runs a web application on an http server.
        /// </summary>
        internal static string ProgramOutput_Intro {
            get {
                return ResourceManager.GetString("ProgramOutput_Intro", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Options:.
        /// </summary>
        internal static string ProgramOutput_Options {
            get {
                return ResourceManager.GetString("ProgramOutput_Options", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Writes any trace data to the given FILE. Default is stderr..
        /// </summary>
        internal static string ProgramOutput_OutputOption {
            get {
                return ResourceManager.GetString("ProgramOutput_OutputOption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Parameters:.
        /// </summary>
        internal static string ProgramOutput_ParametersHeader {
            get {
                return ResourceManager.GetString("ProgramOutput_ParametersHeader", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Changes the default TCP port to listen on when the --port and --url options are not provided..
        /// </summary>
        internal static string ProgramOutput_PortEnvironmentDescription {
            get {
                return ResourceManager.GetString("ProgramOutput_PortEnvironmentDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Which localhost TCP port to listen on if --url is not provided. The default is http://localhost:5000/..
        /// </summary>
        internal static string ProgramOutput_PortOption {
            get {
                return ResourceManager.GetString("ProgramOutput_PortOption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Press ctrl+c again to terminate.
        /// </summary>
        internal static string ProgramOutput_PressCtrlCToTerminate {
            get {
                return ResourceManager.GetString("ProgramOutput_PressCtrlCToTerminate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Changes the default server factory TYPE to use when the --server option is not provided..
        /// </summary>
        internal static string ProgramOutput_ServerEnvironmentDescription {
            get {
                return ResourceManager.GetString("ProgramOutput_ServerEnvironmentDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Load the specified server factory TYPE or assembly. The default is &apos;Microsoft.Owin.Host.HttpListener&apos;..
        /// </summary>
        internal static string ProgramOutput_ServerOption {
            get {
                return ResourceManager.GetString("ProgramOutput_ServerOption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The settings file that contains service and setting overrides. Additional settings will be loaded from the AppSettings section of the app&apos;s config file..
        /// </summary>
        internal static string ProgramOutput_SettingsOption {
            get {
                return ResourceManager.GetString("ProgramOutput_SettingsOption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error:  {0}{1}  {2}.
        /// </summary>
        internal static string ProgramOutput_SimpleErrorMessage {
            get {
                return ResourceManager.GetString("ProgramOutput_SimpleErrorMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Which uri prefix to listen on. This option may be used multiple times. Format is &apos;&lt;scheme&gt;://&lt;host&gt;[:&lt;port&gt;]&lt;path&gt;/&apos;..
        /// </summary>
        internal static string ProgramOutput_UriOption {
            get {
                return ResourceManager.GetString("ProgramOutput_UriOption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Usage:.
        /// </summary>
        internal static string ProgramOutput_Usage {
            get {
                return ResourceManager.GetString("ProgramOutput_Usage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to OwinHost [options] [&lt;application&gt;].
        /// </summary>
        internal static string ProgramOutput_UsageTemplate {
            get {
                return ResourceManager.GetString("ProgramOutput_UsageTemplate", resourceCulture);
            }
        }
    }
}
