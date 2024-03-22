﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ULDKClient.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("ULDKClient.Properties.Resources", typeof(Resources).Assembly);
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
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot add parcel to graphics layer or/and to feature class..
        /// </summary>
        public static string ADD_PARCEL_FAILED {
            get {
                return ResourceManager.GetString("ADD_PARCEL_FAILED", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to fetch commune and region data from a remote location. Check the Internet access and whitelist connection to *.github.com. Verify the logs in &lt;project path/GUKIK folder&gt;..
        /// </summary>
        public static string COMMUNE_REGION_ERROR {
            get {
                return ResourceManager.GetString("COMMUNE_REGION_ERROR", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot save the parcel in the feature class or add it to the graphics layer..
        /// </summary>
        public static string PARCEL_CANNOT_SAVE_GL_FC_ERROR {
            get {
                return ResourceManager.GetString("PARCEL_CANNOT_SAVE_GL_FC_ERROR", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot find the parcel with the provided id. Please check if it is correct..
        /// </summary>
        public static string PARCEL_ID_NOT_EXIST_ERROR {
            get {
                return ResourceManager.GetString("PARCEL_ID_NOT_EXIST_ERROR", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please draw a line whose length is less than 1 kilometer. Trying to be graceful to ULDK servers....
        /// </summary>
        public static string SKETCH_LINE_LENGTH_OVER_LIMIT {
            get {
                return ResourceManager.GetString("SKETCH_LINE_LENGTH_OVER_LIMIT", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please draw a polygon whose area is less than 1 square kilometer (1 km x1 km). Trying to be graceful to ULDK servers....
        /// </summary>
        public static string SKETCH_POLYGON_AREA_OVER_LIMIT {
            get {
                return ResourceManager.GetString("SKETCH_POLYGON_AREA_OVER_LIMIT", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Status:.
        /// </summary>
        public static string UI_CURSOR_STATUS {
            get {
                return ResourceManager.GetString("UI_CURSOR_STATUS", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Full id:.
        /// </summary>
        public static string UI_EX_FIND_BY_FULL_ID {
            get {
                return ResourceManager.GetString("UI_EX_FIND_BY_FULL_ID", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Find a parcel by its official number:.
        /// </summary>
        public static string UI_EX_FIND_BY_ID {
            get {
                return ResourceManager.GetString("UI_EX_FIND_BY_ID", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Select a commune:.
        /// </summary>
        public static string UI_EX_FIND_BY_ID_COMMUNE {
            get {
                return ResourceManager.GetString("UI_EX_FIND_BY_ID_COMMUNE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Select a region:.
        /// </summary>
        public static string UI_EX_FIND_BY_ID_REGION {
            get {
                return ResourceManager.GetString("UI_EX_FIND_BY_ID_REGION", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Type in parcel id:.
        /// </summary>
        public static string UI_EX_FIND_BY_PARCEL_ID {
            get {
                return ResourceManager.GetString("UI_EX_FIND_BY_PARCEL_ID", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Show in Geoportal.
        /// </summary>
        public static string UI_EX_FIND_BY_SHOW_GEOPORTAL {
            get {
                return ResourceManager.GetString("UI_EX_FIND_BY_SHOW_GEOPORTAL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Show on the Map.
        /// </summary>
        public static string UI_EX_FIND_BY_SHOW_MAP {
            get {
                return ResourceManager.GetString("UI_EX_FIND_BY_SHOW_MAP", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Find a parcel by drawing a shape:.
        /// </summary>
        public static string UI_EX_FIND_BY_SKETCH {
            get {
                return ResourceManager.GetString("UI_EX_FIND_BY_SKETCH", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Point    .
        /// </summary>
        public static string UI_EX_FIND_BY_SKETCH_POINT {
            get {
                return ResourceManager.GetString("UI_EX_FIND_BY_SKETCH_POINT", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Polygon.
        /// </summary>
        public static string UI_EX_FIND_BY_SKETCH_POLYGON {
            get {
                return ResourceManager.GetString("UI_EX_FIND_BY_SKETCH_POLYGON", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Polyline.
        /// </summary>
        public static string UI_EX_FIND_BY_SKETCH_POLYLINE {
            get {
                return ResourceManager.GetString("UI_EX_FIND_BY_SKETCH_POLYLINE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ULDK is a web service hosted by the Head Office of Geodesy and Cartography (GUGiK). Its spatial coverage is limited to Poland only..
        /// </summary>
        public static string UI_HEADING {
            get {
                return ResourceManager.GetString("UI_HEADING", resourceCulture);
            }
        }
    }
}
