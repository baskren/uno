#nullable enable

using Android.App;
using Android.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using Uno.Extensions;
using Uno;
using Uno.UI;
using System.Linq;
using Android.OS;
using Microsoft.UI.Xaml.Media;
using Windows.UI.Text;
using Uno.Foundation.Logging;
using Windows.Storage;
using Uno.UI.Xaml.Media;
using System.Text.Json;
using System.Text.Json.Serialization;
using Uno.Helpers;
using static Android.Graphics.ImageDecoder;


namespace Microsoft.UI.Xaml
{
	internal partial class FontHelper
	{
		private static bool _assetsListed;
		private static readonly string DefaultFontFamilyName = "sans-serif";

		internal static Typeface? FontFamilyToTypeFace(FontFamily? fontFamily, FontWeight fontWeight, FontStyle fontStyle, FontStretch fontStretch)
		{
			var italic = fontStyle is FontStyle.Italic or FontStyle.Oblique;
			var entry = new FontFamilyToTypeFaceDictionary.Entry(fontFamily?.Source, fontWeight, italic, fontStretch);

			if (!_fontFamilyToTypeFaceDictionary.TryGetValue(entry, out var typeFace))
			{
				typeFace = InternalFontFamilyToTypeFace(fontFamily, fontWeight, italic, fontStretch);
				_fontFamilyToTypeFaceDictionary.Add(entry, typeFace);
			}

			return typeFace;
		}

		internal static Typeface? InternalFontFamilyToTypeFace(FontFamily? fontFamily, FontWeight fontWeight, bool italic, FontStretch fontStretch)
		{
			if (fontFamily?.Source == null || fontFamily.Equals(FontFamily.Default))
			{
				fontFamily = GetDefaultFontFamily(fontWeight);
			}

			Typeface? typeface;

			try
			{
				if (typeof(FontHelper).Log().IsEnabled(Uno.Foundation.Logging.LogLevel.Debug))
				{
					typeof(FontHelper).Log().Debug($"Searching for font [{fontFamily.Source}]");
				}

				// If there's a ".", we assume there's an extension and that it's a font file path.
				if (fontFamily.Source.Contains("."))
				{
					var source = fontFamily.Source;

					if (source.StartsWith("ms-appdata:///", StringComparison.OrdinalIgnoreCase))
					{
						if (TryLoadFromAppData(fontWeight.Weight, fontStretch, italic, source, out typeface))
						{
							return typeface;
						}
						else
						{
							throw new InvalidOperationException($"Unable to find [{fontFamily.Source}] from the application's local data.");
						}
					}

					source = source.TrimStart("ms-appx://", ignoreCase: true);

					if (!TryLoadFromAppX(fontWeight.Weight, fontStretch, italic, source, out typeface))
					{
						// The lookup used to be performed without the assets folder, even if its required to specify it
						// with UWP. Keep this behavior for backward compatibility.
						var legacySource = source.TrimStart("/assets/", ignoreCase: true);

						// The path for AndroidAssets is not encoded, unlike assets processed by the RetargetAssets tool.
						if (!TryLoadFromAppX(fontWeight.Weight, fontStretch, italic, legacySource, out typeface, encodePath: false))
						{
							throw new InvalidOperationException($"Unable to find [{fontFamily.Source}] from the application's assets.");
						}
					}
				}
				else
				{
					var style = TypefaceStyleHelper.GetTypefaceStyle(italic, fontWeight);
					typeface = Android.Graphics.Typeface.Create(fontFamily.Source, style);
					if (typeface is not null && typeface.Weight != fontWeight.Weight)
					{
						typeface = Android.Graphics.Typeface.Create(typeface, fontWeight.Weight, italic);
					}
				}

				return typeface;
			}
			catch (Exception e)
			{
				if (typeof(FontHelper).Log().IsEnabled(Uno.Foundation.Logging.LogLevel.Error))
				{
					typeof(FontHelper).Log().Error("Unable to find font", e);

					if (typeof(FontHelper).Log().IsEnabled(LogLevel.Warning))
					{
						if (!_assetsListed)
						{
							_assetsListed = true;

							var allAssets = AssetsHelper.AllAssets.JoinBy("\r\n");
							typeof(FontHelper).Log().Warn($"List of available assets: {allAssets}");
						}
					}
				}

				return null;
			}
		}

		private static string FontStretchToPercentage(FontStretch fontStretch)
		{
			return fontStretch switch
			{
				FontStretch.UltraCondensed => "50",
				FontStretch.ExtraCondensed => "62.5",
				FontStretch.Condensed => "75",
				FontStretch.SemiCondensed => "87.5",
				FontStretch.Normal => "100",
				FontStretch.SemiExpanded => "112.5",
				FontStretch.Expanded => "125",
				FontStretch.ExtraExpanded => "150",
				FontStretch.UltraExpanded => "200",
				_ => "100",
			};
		}

		private static bool TryLoadFromAppData(int weight, FontStretch stretch, bool italic, string source, out Typeface? typeface)
		{
			source = FontFamilyHelper.RemoveHashFamilyName(source);
			typeface = null;
			Uri? sourceUri = null;
			try
			{
				sourceUri = new Uri(source);
			}
			catch (Exception)
			{
				return false;
			}

			if (sourceUri.Scheme != "ms-appdata")
				return false;

			if (typeof(FontHelper).Log().IsEnabled(Uno.Foundation.Logging.LogLevel.Debug))
			{
				typeof(FontHelper).Log().Debug($"Searching for font as asset [{source}]");
			}

			string localPath = AppDataUriEvaluator.ToPath(sourceUri);
			return TryLoadFromLocalPath(weight, stretch, italic, source, localPath, out typeface);
		}

		private static bool TryLoadFromAppX(int weight, FontStretch stretch, bool italic, string source, out Typeface? typeface, bool encodePath = true)
		{
			source = FontFamilyHelper.RemoveHashFamilyName(source);

			if (typeof(FontHelper).Log().IsEnabled(Uno.Foundation.Logging.LogLevel.Debug))
			{
				typeof(FontHelper).Log().Debug($"Searching for font as asset [{source}]");
			}

			var encodedSource = encodePath
				? AndroidResourceNameEncoder.EncodeFileSystemPath(source, prefix: "")
				: source;

			// We need to lookup assets manually, as assets are stored this way by android, but windows
			// is case insensitive.
			string localPath = AssetsHelper.FindAssetFile(encodedSource);
			return TryLoadFromLocalPath(weight, stretch, italic, source, localPath, out typeface);
		}

		private static bool TryLoadFromLocalPath(int weight, FontStretch stretch, bool italic, string source, string localPath, out Typeface? typeface)
		{
			if (localPath != null)
			{
				var builder = new Android.Graphics.Typeface.Builder(Android.App.Application.Context.Assets!, localPath);
				// NOTE: We are unable to use 'ital' axis here. If that axis doesn't exist in the
				// font file, italic will break badly. However, if it exists, we will render "reasonable" (but not ideal) italic text.
				builder.SetFontVariationSettings($"'wght' {weight}, 'wdth' {FontStretchToPercentage(stretch)}");
				typeface = builder.Build();

				if (typeface is not null)
				{
					if (typeface.Weight != weight || typeface.IsItalic != italic)
					{
						typeface = Typeface.Create(typeface, weight, italic);
					}

					return true;
				}
				else
				{
					if (typeof(FontHelper).Log().IsEnabled(Uno.Foundation.Logging.LogLevel.Debug))
					{
						typeof(FontHelper).Log().Debug($"Font [{source}] could not be created from asset [{localPath}]");
					}
				}
			}
			else
			{
				if (typeof(FontHelper).Log().IsEnabled(Uno.Foundation.Logging.LogLevel.Debug))
				{
					typeof(FontHelper).Log().Debug($"Font [{source}] could not be found in app assets using [{localPath}]");
				}
			}

			typeface = null;
			return false;
		}

		private static FontFamily GetDefaultFontFamily(FontWeight fontWeight)
		{
			string fontVariant = string.Empty;

			if (fontWeight == FontWeights.Light
				|| fontWeight == FontWeights.UltraLight
				|| fontWeight == FontWeights.ExtraLight)
			{
				fontVariant = "-light";
			}
			else if (fontWeight == FontWeights.Thin
					|| fontWeight == FontWeights.SemiLight)
			{
				fontVariant = "-thin";
			}
			else if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
			{
				if (fontWeight == FontWeights.Medium)
				{
					fontVariant = "-medium";
				}
				else if (fontWeight == FontWeights.Black
						|| fontWeight == FontWeights.UltraBlack
						|| fontWeight == FontWeights.ExtraBlack)
				{
					fontVariant = "-black";
				}
			}

			return new FontFamily(DefaultFontFamilyName + fontVariant);
		}

		/// <summary>
		/// Get the ratio dictated by the user-specified text size in Accessibility
		/// </summary>
		/// <returns></returns>
		public static double GetFontRatio()
		{
			return ViewHelper.FontScale / ViewHelper.Scale;
		}
	}
}
