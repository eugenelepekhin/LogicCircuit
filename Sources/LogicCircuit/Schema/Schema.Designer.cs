﻿//-----------------------------------------------------------------------------
//
//	This code was generated by a ResourceWrapper.Generator Version 4.0.0.0.
//
//	Changes to this file may cause incorrect behavior and will be lost if
//	the code is regenerated.
//
//-----------------------------------------------------------------------------

namespace LogicCircuit {
	using System;
	using System.Diagnostics;
	using System.Globalization;
	using System.Runtime.CompilerServices;
	using System.ComponentModel;
	using System.Resources;
	using System.Windows;

	/// <summary>
	/// A strongly-typed resource class, for looking up localized strings, etc.
	/// </summary>
	// This class was auto-generated.
	// To add or remove a member, edit your .ResX file then rerun MsBuild,
	// or rebuild your VS project.
	[DebuggerNonUserCodeAttribute()]
	[CompilerGeneratedAttribute()]
	internal static class Schema {

		/// <summary>
		/// Overrides the current thread's CurrentUICulture property for all
		/// resource lookups using this strongly typed resource class.
		/// </summary>
		[EditorBrowsableAttribute(EditorBrowsableState.Advanced)]
		public static CultureInfo Culture { get; set; }

		/// <summary>
		/// Used for formating of the resource strings. Usually same as CultureInfo.CurrentCulture.
		/// </summary>
		[EditorBrowsableAttribute(EditorBrowsableState.Advanced)]
		public static CultureInfo FormatCulture { get; set; }

		/// <summary>
		/// Gets FlowDirection for current culture.
		/// </summary>
		public static FlowDirection FlowDirection {
			get {
				bool isRightToLeft;
				if(Schema.Culture != null && Schema.Culture.TextInfo != null) {
					isRightToLeft = Schema.Culture.TextInfo.IsRightToLeft;
				} else if(CultureInfo.CurrentUICulture != null && CultureInfo.CurrentUICulture.TextInfo != null) {
					isRightToLeft = CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft;
				} else if(CultureInfo.CurrentCulture != null && CultureInfo.CurrentCulture.TextInfo != null) {
					isRightToLeft = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft;
				} else {
					isRightToLeft = false;
				}
				return isRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
			}
		}

		private static ResourceManager resourceManager;

		/// <summary>
		/// Returns the cached ResourceManager instance used by this class.
		/// </summary>
		[EditorBrowsableAttribute(EditorBrowsableState.Advanced)]
		public static ResourceManager ResourceManager {
			get {
				if(resourceManager == null) {
					resourceManager = new ResourceManager("LogicCircuit.Schema.Schema", typeof(Schema).Assembly);
				}
				return resourceManager;
			}
		}

		/// <summary>
		/// Looks up a localized string similar to content of the file: "convertfrom.1.0.0.2.xslt".
		/// </summary>
 		public static System.String ConvertFrom_1_0_0_2 {
			get { return (System.String)ResourceManager.GetObject("ConvertFrom_1_0_0_2", Culture); }
		}

		/// <summary>
		/// Looks up a localized string similar to content of the file: "convertfrom.1.0.0.3.xslt".
		/// </summary>
 		public static System.String ConvertFrom_1_0_0_3 {
			get { return (System.String)ResourceManager.GetObject("ConvertFrom_1_0_0_3", Culture); }
		}

		/// <summary>
		/// Looks up a localized string similar to content of the file: "convertfrom.2.0.0.1.xslt".
		/// </summary>
 		public static System.String ConvertFrom_2_0_0_1 {
			get { return (System.String)ResourceManager.GetObject("ConvertFrom_2_0_0_1", Culture); }
		}

		/// <summary>
		/// Looks up a localized string similar to content of the file: "convertfrom.2.0.0.10.xslt".
		/// </summary>
 		public static System.String ConvertFrom_2_0_0_10 {
			get { return (System.String)ResourceManager.GetObject("ConvertFrom_2_0_0_10", Culture); }
		}

		/// <summary>
		/// Looks up a localized string similar to content of the file: "ConvertFrom.2.0.0.11.xslt".
		/// </summary>
 		public static System.String ConvertFrom_2_0_0_11 {
			get { return (System.String)ResourceManager.GetObject("ConvertFrom_2_0_0_11", Culture); }
		}

		/// <summary>
		/// Looks up a localized string similar to content of the file: "ConvertFrom.2.0.0.12.xslt".
		/// </summary>
 		public static System.String ConvertFrom_2_0_0_12 {
			get { return (System.String)ResourceManager.GetObject("ConvertFrom_2_0_0_12", Culture); }
		}

		/// <summary>
		/// Looks up a localized string similar to content of the file: "convertfrom.2.0.0.2.xslt".
		/// </summary>
 		public static System.String ConvertFrom_2_0_0_2 {
			get { return (System.String)ResourceManager.GetObject("ConvertFrom_2_0_0_2", Culture); }
		}

		/// <summary>
		/// Looks up a localized string similar to content of the file: "convertfrom.2.0.0.3.xslt".
		/// </summary>
 		public static System.String ConvertFrom_2_0_0_3 {
			get { return (System.String)ResourceManager.GetObject("ConvertFrom_2_0_0_3", Culture); }
		}

		/// <summary>
		/// Looks up a localized string similar to content of the file: "convertfrom.2.0.0.4.xslt".
		/// </summary>
 		public static System.String ConvertFrom_2_0_0_4 {
			get { return (System.String)ResourceManager.GetObject("ConvertFrom_2_0_0_4", Culture); }
		}

		/// <summary>
		/// Looks up a localized string similar to content of the file: "convertfrom.2.0.0.5.xslt".
		/// </summary>
 		public static System.String ConvertFrom_2_0_0_5 {
			get { return (System.String)ResourceManager.GetObject("ConvertFrom_2_0_0_5", Culture); }
		}

		/// <summary>
		/// Looks up a localized string similar to content of the file: "convertfrom.2.0.0.6.xslt".
		/// </summary>
 		public static System.String ConvertFrom_2_0_0_6 {
			get { return (System.String)ResourceManager.GetObject("ConvertFrom_2_0_0_6", Culture); }
		}

		/// <summary>
		/// Looks up a localized string similar to content of the file: "convertfrom.2.0.0.7.xslt".
		/// </summary>
 		public static System.String ConvertFrom_2_0_0_7 {
			get { return (System.String)ResourceManager.GetObject("ConvertFrom_2_0_0_7", Culture); }
		}

		/// <summary>
		/// Looks up a localized string similar to content of the file: "convertfrom.2.0.0.8.xslt".
		/// </summary>
 		public static System.String ConvertFrom_2_0_0_8 {
			get { return (System.String)ResourceManager.GetObject("ConvertFrom_2_0_0_8", Culture); }
		}

		/// <summary>
		/// Looks up a localized string similar to content of the file: "convertfrom.2.0.0.9.xslt".
		/// </summary>
 		public static System.String ConvertFrom_2_0_0_9 {
			get { return (System.String)ResourceManager.GetObject("ConvertFrom_2_0_0_9", Culture); }
		}

		/// <summary>
		/// Looks up a localized string similar to content of the file: "convertsettings.xslt".
		/// </summary>
 		public static System.String ConvertSettings {
			get { return (System.String)ResourceManager.GetObject("ConvertSettings", Culture); }
		}

		/// <summary>
		/// Looks up a localized string similar to content of the file: "empty.xml".
		/// </summary>
 		public static System.String Empty {
			get { return (System.String)ResourceManager.GetObject("Empty", Culture); }
		}
	}
}
