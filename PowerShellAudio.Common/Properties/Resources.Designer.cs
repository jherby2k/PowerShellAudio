﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PowerShellAudio.Properties {
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("PowerShellAudio.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to The file &apos;{0}&apos; already exists.
        /// </summary>
        internal static string CoverArtFileExistsError {
            get {
                return ResourceManager.GetString("CoverArtFileExistsError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Only bitmap, JPEG and PNG images are supported as cover art.
        /// </summary>
        internal static string CoverArtUnsupportedImageFormat {
            get {
                return ResourceManager.GetString("CoverArtUnsupportedImageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &apos;{0}&apos; is not a valid day of the month.
        /// </summary>
        internal static string MetadataDictionaryDayError {
            get {
                return ResourceManager.GetString("MetadataDictionaryDayError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &apos;{0}&apos; is not a valid month.
        /// </summary>
        internal static string MetadataDictionaryMonthError {
            get {
                return ResourceManager.GetString("MetadataDictionaryMonthError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &apos;{0}&apos; is not a valid year.
        /// </summary>
        internal static string MetadataDictionaryYearError {
            get {
                return ResourceManager.GetString("MetadataDictionaryYearError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The Key and/or Value property should not be null or empty.
        /// </summary>
        internal static string SettingsDictionaryItemError {
            get {
                return ResourceManager.GetString("SettingsDictionaryItemError", resourceCulture);
            }
        }
    }
}
