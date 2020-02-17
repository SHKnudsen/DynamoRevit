﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DynamoRevitVersionSelector.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("DynamoRevitVersionSelector.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap dynamo_32x32 {
            get {
                object obj = ResourceManager.GetObject("dynamo_32x32", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Dynamo Version.
        /// </summary>
        internal static string DynamoVersions {
            get {
                return ResourceManager.GetString("DynamoVersions", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Dynamo Version Selection.
        /// </summary>
        internal static string DynamoVersionSelection {
            get {
                return ResourceManager.GetString("DynamoVersionSelection", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Dynamo {0}.
        /// </summary>
        internal static string DynamoVersionText {
            get {
                return ResourceManager.GetString("DynamoVersionText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Launches Dynamo {0}, to launch a different version of Dynamo you may need to restart Revit.
        /// </summary>
        internal static string DynamoVersionTooltip {
            get {
                return ResourceManager.GetString("DynamoVersionTooltip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Dynamo version {0} will be loaded after Revit restart..
        /// </summary>
        internal static string RestartMessage {
            get {
                return ResourceManager.GetString("RestartMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Multiple version of Dynamo installation found on this system.
        ///Please select the Dynamo version you wish to run..
        /// </summary>
        internal static string VersionSelectionContent {
            get {
                return ResourceManager.GetString("VersionSelectionContent", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Visual Programming.
        /// </summary>
        internal static string VisualProgramming {
            get {
                return ResourceManager.GetString("VisualProgramming", resourceCulture);
            }
        }
    }
}
