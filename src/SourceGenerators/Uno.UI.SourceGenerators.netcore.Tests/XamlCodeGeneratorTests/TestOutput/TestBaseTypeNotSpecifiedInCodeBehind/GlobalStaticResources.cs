﻿// <autogenerated />
#if __WASM__
#error invalid internal source generator state. The __WASM__ DefineConstant was not propagated properly.
#endif
namespace MyProject
{
	/// <summary>
	/// Contains all the static resources defined for the application
	/// </summary>
	public sealed partial class GlobalStaticResources
	{
		static bool _initialized;
		private static bool _stylesRegistered;
		private static bool _dictionariesRegistered;
		internal static global::Uno.UI.Xaml.XamlParseContext __ParseContext_ {get; } = new global::Uno.UI.Xaml.XamlParseContext()
		{
			AssemblyName = "TestProject",
		}
		;

		static GlobalStaticResources()
		{
			Initialize();
		}
		public static void Initialize()
		{
			if (!_initialized)
			{
				_initialized = true;
				global::Uno.UI.GlobalStaticResources.Initialize();
				global::Uno.UI.GlobalStaticResources.RegisterDefaultStyles();
				global::Uno.UI.GlobalStaticResources.RegisterResourceDictionariesBySource();
			}
		}
		public static void RegisterDefaultStyles()
		{
			if(!_stylesRegistered)
			{
				_stylesRegistered = true;
				RegisterDefaultStyles_MainPage_600e5db99cb0a5c835d2c4e1905b1bab();
				RegisterDefaultStyles_UserControl1_d5e1d0db68719e100793d3b723971209();
			}
		}
		// Register ResourceDictionaries using ms-appx:/// syntax, this is called for external resources
		public static void RegisterResourceDictionariesBySource()
		{
			if(!_dictionariesRegistered)
			{
				_dictionariesRegistered = true;
			}
		}
		// Register ResourceDictionaries using ms-resource:/// syntax, this is called for local resources
		internal static void RegisterResourceDictionariesBySourceLocal()
		{
		}
		static partial void RegisterDefaultStyles_MainPage_600e5db99cb0a5c835d2c4e1905b1bab();
		static partial void RegisterDefaultStyles_UserControl1_d5e1d0db68719e100793d3b723971209();
		[global::System.Obsolete("This method is provided for binary backward compatibility. It will always return null.")]
		[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
		public static object FindResource(string name) => null;
		
	}
}
