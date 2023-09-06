﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
using Uno.UI.DataBinding;
using Uno.UI;
using Windows.Foundation;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Animation;
using Uno.UI.Xaml;
using Windows.UI.Xaml.Media;

#if __ANDROID__
using Android.Widget;
using Android.Views;
using ViewGroup = Android.Views.ViewGroup;
#elif __IOS__
using Windows.UI.Xaml.Media;
using UIKit;
using ViewGroup = UIKit.UIView;
#elif __MACOS__
using Windows.UI.Xaml.Media;
using AppKit;
using ViewGroup = AppKit.NSView;
#else
using ViewGroup = Windows.UI.Xaml.UIElement;
#endif

namespace Windows.UI.Xaml.Controls
{
	public partial class ItemsPresenter : FrameworkElement, IScrollSnapPointsInfo
	{
		internal ContentControl FooterContentControl { get; private set; }

		internal ContentControl HeaderContentControl { get; private set; }

		private Orientation Orientation => (Panel as Panel)?.InternalOrientation ?? Orientation.Horizontal;

		public object Header
		{
			get => GetHeaderValue();
			set => SetHeaderValue(value);
		}

		private static object GetHeaderDefaultValue() => null;

		[GeneratedDependencyProperty(ChangedCallback = true)]
		public static DependencyProperty HeaderProperty { get; } = CreateHeaderProperty();

		private void OnHeaderChanged(DependencyPropertyChangedEventArgs args)
		{
			if (HeaderContentControl is { })
			{
				HeaderContentControl.Content = args.NewValue;
			}
		}

		public object Footer
		{
			get => GetFooterValue();
			set => SetFooterValue(value);
		}

		private static object GetFooterDefaultValue() => null;

		[GeneratedDependencyProperty(ChangedCallback = true)]
		public static DependencyProperty FooterProperty { get; } = CreateFooterProperty();

		private void OnFooterChanged(DependencyPropertyChangedEventArgs args)
		{
			if (FooterContentControl is { })
			{
				FooterContentControl.Content = args.NewValue;
			}
		}

		private static DataTemplate GetHeaderTemplateDefaultValue() => null;

		public DataTemplate HeaderTemplate
		{
			get => GetHeaderTemplateValue();
			set => SetHeaderTemplateValue(value);
		}

		[GeneratedDependencyProperty(ChangedCallback = true)]
		public static DependencyProperty HeaderTemplateProperty { get; } = CreateHeaderTemplateProperty();

		private void OnHeaderTemplateChanged(DependencyPropertyChangedEventArgs args)
		{
			if (HeaderContentControl is { })
			{
				HeaderContentControl.ContentTemplate = (DataTemplate)args.NewValue;
			}
		}

		public DataTemplate FooterTemplate
		{
			get => GetFooterTemplateValue();
			set => SetFooterTemplateValue(value);
		}

		private static DataTemplate GetFooterTemplateDefaultValue() => null;

		[GeneratedDependencyProperty(ChangedCallback = true)]
		public static DependencyProperty FooterTemplateProperty { get; } = CreateFooterTemplateProperty();

		private void OnFooterTemplateChanged(DependencyPropertyChangedEventArgs args)
		{
			if (FooterContentControl is { })
			{
				FooterContentControl.ContentTemplate = (DataTemplate)args.NewValue;
			}
		}

		public TransitionCollection HeaderTransitions
		{
			get => GetHeaderTransitionsValue();
			set => SetHeaderTransitionsValue(value);
		}

		private static TransitionCollection GetHeaderTransitionsDefaultValue() => new TransitionCollection();

		[GeneratedDependencyProperty(ChangedCallback = true)]
		public static DependencyProperty HeaderTransitionsProperty { get; } = CreateHeaderTransitionsProperty();

		private void OnHeaderTransitionsChanged(DependencyPropertyChangedEventArgs args)
		{
			if (HeaderContentControl is { })
			{
				HeaderContentControl.Transitions = (TransitionCollection)args.NewValue;
			}
		}

		public TransitionCollection FooterTransitions
		{
			get => GetFooterTransitionsValue();
			set => SetFooterTransitionsValue(value);
		}

		private static TransitionCollection GetFooterTransitionsDefaultValue() => new TransitionCollection();

		[GeneratedDependencyProperty(ChangedCallback = true)]
		public static DependencyProperty FooterTransitionsProperty { get; } = CreateFooterTransitionsProperty();

		private void OnFooterTransitionsChanged(DependencyPropertyChangedEventArgs args)
		{
			if (FooterContentControl is { })
			{
				FooterContentControl.Transitions = (TransitionCollection)args.NewValue;
			}
		}

		protected internal override void OnTemplatedParentChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnTemplatedParentChanged(e);

			if (TemplatedParent is ItemsControl itemsControl && IsLoaded)
			{
				itemsControl.SetItemsPresenter(this);
			}
		}

		private protected override void OnLoaded()
		{
			base.OnLoaded();

			if (TemplatedParent is ItemsControl itemsControl && IsLoaded)
			{
				itemsControl.SetItemsPresenter(this);
			}
		}

		public ItemsPresenter()
		{

			// A content presenter does not propagate its own templated
			// parent. The content's TemplatedParent has already been set by the
			// content presenter to its own templated parent.

			//TODO TEMPLATED PARENT
			// PropagateTemplatedParent = false;
		}

		#region Padding DependencyProperty

		public Thickness Padding
		{
			get => (Thickness)GetValue(PaddingProperty);
			set => SetValue(PaddingProperty, value);
		}

		public static DependencyProperty PaddingProperty { get; } =
			DependencyProperty.Register(
				"Padding",
				typeof(Thickness),
				typeof(ItemsPresenter),
				new FrameworkPropertyMetadata(
					(Thickness)Thickness.Empty,
					(s, e) => ((ItemsPresenter)s)?.OnPaddingChanged((Thickness)e.OldValue, (Thickness)e.NewValue)
				)
			);

		private void OnPaddingChanged(Thickness oldValue, Thickness newValue)
		{
			this.InvalidateMeasure();
			PropagateLayoutValues();
		}

		#endregion

		internal override void OnPropertyChanged2(DependencyPropertyChangedEventArgs args)
		{
			base.OnPropertyChanged2(args);

			if (args.Property == FrameworkElement.MinHeightProperty || args.Property == FrameworkElement.MinWidthProperty)
			{
				PropagateLayoutValues();
			}
		}

		/// <summary>
		/// Indicates whether the ItemsPresenter is actually enclosed in the scrollable area of the
		/// ItemsControl (or derived type). This is always true on Windows, but on Uno it's not the case
		/// for controls which delegate to a native implementation (eg <see cref="ListViewBase"/>).
		/// </summary>
		private bool IsWithinScrollableArea =>
#if XAMARIN && !__MACOS__
			!(_itemsPanel is NativeListViewBase);
#else
			true;
#endif

		private Thickness AppliedPadding =>
			IsWithinScrollableArea ?
				Padding :
				Thickness.Empty;

		protected override bool IsSimpleLayout => true;

		private ViewGroup _itemsPanel;

		internal ViewGroup Panel => _itemsPanel;

		private IScrollSnapPointsInfo SnapPointsProvider => Panel as IScrollSnapPointsInfo;

		public bool AreHorizontalSnapPointsRegular => SnapPointsProvider?.AreHorizontalSnapPointsRegular ?? false;

		public bool AreVerticalSnapPointsRegular => SnapPointsProvider?.AreVerticalSnapPointsRegular ?? false;

		internal void SetItemsPanel(ViewGroup panel)
		{
			if (_itemsPanel == panel)
			{
				return;
			}

			// This is only called after (or while) the header and footer are created and added to the visual tree.
			global::System.Diagnostics.Debug.Assert(HeaderContentControl is { });

			if (_itemsPanel is { })
			{
				VisualTreeHelper.RemoveView(this, _itemsPanel);
			}

			_itemsPanel = panel;

			if (_itemsPanel != null)
			{
				VisualTreeHelper.AddView(this, _itemsPanel, 1);

				PropagateLayoutValues();
			}

			this.InvalidateMeasure();
		}

		internal void LoadChildren(ViewGroup panel)
		{
			if (HeaderContentControl is null)
			{
				HeaderContentControl = new ContentControl
				{
					Content = Header,
					ContentTemplate = HeaderTemplate,
					ContentTransitions = HeaderTransitions,
					VerticalContentAlignment = VerticalAlignment.Stretch,
					HorizontalContentAlignment = HorizontalAlignment.Stretch,
					IsTabStop = false
				};

				VisualTreeHelper.AddChild(this, HeaderContentControl);
			}

			SetItemsPanel(panel);

			if (FooterContentControl is null)
			{
				FooterContentControl = new ContentControl
				{
					Content = Footer,
					ContentTemplate = FooterTemplate,
					ContentTransitions = FooterTransitions,
					VerticalContentAlignment = VerticalAlignment.Stretch,
					HorizontalContentAlignment = HorizontalAlignment.Stretch,
					IsTabStop = false
				};

				VisualTreeHelper.AddChild(this, FooterContentControl);
			}
		}

		private void PropagateLayoutValues()
		{
#if XAMARIN && !__MACOS__
			var asListViewBase = _itemsPanel as NativeListViewBase;
			if (asListViewBase != null)
			{
				asListViewBase.Padding = Padding;
				asListViewBase.ItemsPresenterMinWidth = MinWidth;
				asListViewBase.ItemsPresenterMinHeight = MinHeight;
			}
#endif
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			// Most of this is inspired by StackPanel's ArrangeOverride

			var isHorizontal = Orientation == Orientation.Horizontal;
			var padding = AppliedPadding;

			var childRect = new Rect(new Point(padding.Left, padding.Top), default(Size));
			var previousChildSize = 0.0;

			var collection = new[]
			{
				HeaderContentControl,
				_itemsPanel,
				FooterContentControl
			};

			for (var i = 0; i < 3; i++)
			{
				var view = collection[i];
				var desiredChildSize = GetElementDesiredSize(view);

				if (isHorizontal)
				{
					childRect.X += previousChildSize;

					childRect.Width = desiredChildSize.Width;
					childRect.Height = Math.Max(finalSize.Height, desiredChildSize.Height);

					if (view == _itemsPanel)
					{
						// the panel should stretch to a width big enough such that the footer is at the very right
						childRect.Width = childRect.Width.AtLeast(finalSize.Width - GetElementDesiredSize(FooterContentControl).Width - childRect.X);
					}

					previousChildSize = childRect.Width;
				}
				else
				{
					childRect.Y += previousChildSize;

					childRect.Height = desiredChildSize.Height;
					childRect.Width = Math.Max(finalSize.Width, desiredChildSize.Width);

					if (view == _itemsPanel)
					{
						// the panel should stretch to a height big enough such that the footer is at the very bottom
						childRect.Height = childRect.Height.AtLeast(finalSize.Height - GetElementDesiredSize(FooterContentControl).Height - childRect.Y);
					}

					previousChildSize = childRect.Height;
				}

				ArrangeElement(view, childRect);
			}

			return finalSize;
		}

		protected override Size MeasureOverride(Size size)
		{
			// Most of this is inspired by StackPanel's MeasureOverride

			var padding = AppliedPadding;

			var unpaddedSize = new Size(
				size.Width - padding.Left - padding.Right,
				size.Height - padding.Top - padding.Bottom
			);

			var isHorizontal = ((Panel as Panel)?.InternalOrientation ?? Orientation.Horizontal) == Orientation.Horizontal;

			var desiredSize = default(Size);

			var collection = new[]
			{
				HeaderContentControl,
				_itemsPanel,
				FooterContentControl
			};

			for (var i = 0; i < 3; i++)
			{
				var view = collection[i];

				var availableSize = unpaddedSize;
				if (view != _itemsPanel)
				{
					// On Windows, every child gets an infinite length along the orientation dimension
					// except the panel, which doesn't get this treatment.
					if (isHorizontal)
					{
						availableSize.Width = float.PositiveInfinity;
					}
					else
					{
						availableSize.Height = float.PositiveInfinity;
					}
				}

				var measuredSize = MeasureElement(view, unpaddedSize);

				if (isHorizontal)
				{
					desiredSize.Width += measuredSize.Width;
					desiredSize.Height = Math.Max(desiredSize.Height, measuredSize.Height);
				}
				else
				{
					desiredSize.Width = Math.Max(desiredSize.Width, measuredSize.Width);
					desiredSize.Height += measuredSize.Height;
				}
			}

			return new Size(
				desiredSize.Width + padding.Left + padding.Right,
				desiredSize.Height + padding.Top + padding.Bottom
			);
		}

		public IReadOnlyList<float> GetIrregularSnapPoints(Orientation orientation, SnapPointsAlignment alignment)
		{
			if (orientation != Orientation || SnapPointsProvider is null || HeaderContentControl is null || FooterContentControl is null)
			{
				return null;
			}

			var result = new List<float>(2 + VisualTreeHelper.GetViewGroupChildrenCount(_itemsPanel));

			var panelSnapPoints = SnapPointsProvider.GetIrregularSnapPoints(orientation, alignment);

			var hasHeader = Header != null || HeaderTemplate != null;
			var hasFooter = Footer != null || FooterTemplate != null;

			var headerRect = LayoutInformation.GetLayoutSlot(HeaderContentControl);
			var footerRect = LayoutInformation.GetLayoutSlot(FooterContentControl);

			var panelStretch = Orientation == Orientation.Horizontal ?
				footerRect.Left - headerRect.Right - GetElementDesiredSize(_itemsPanel).Width :
				footerRect.Top - headerRect.Bottom - GetElementDesiredSize(_itemsPanel).Height;

			if (orientation == Orientation.Horizontal)
			{
				switch (alignment)
				{
					case SnapPointsAlignment.Near:
						if (hasHeader)
						{
							result.Add((float)headerRect.Left);
						}
						result.AddRange(panelSnapPoints.Select(i => i + (float)headerRect.Right));
						if (hasFooter)
						{
							result.Add((float)(footerRect.Left - panelStretch));
						}
						break;
					case SnapPointsAlignment.Center:
						if (hasHeader)
						{
							result.Add((float)headerRect.GetMidX());
						}
						result.AddRange(panelSnapPoints.Select(i => i + (float)headerRect.Right));
						if (hasFooter)
						{
							result.Add((float)(footerRect.GetMidX() - panelStretch));
						}
						break;
					case SnapPointsAlignment.Far:
						if (hasHeader)
						{
							result.Add((float)headerRect.Right);
						}
						result.AddRange(panelSnapPoints.Select(i => i + (float)headerRect.Right));
						if (hasFooter)
						{
							result.Add((float)(footerRect.Right - panelStretch));
						}
						break;
				}
			}
			else
			{
				switch (alignment)
				{
					case SnapPointsAlignment.Near:
						if (hasHeader)
						{
							result.Add((float)headerRect.Top);
						}
						result.AddRange(panelSnapPoints.Select(i => i + (float)headerRect.Bottom));
						if (hasFooter)
						{
							result.Add((float)(footerRect.Top - panelStretch));
						}
						break;
					case SnapPointsAlignment.Center:
						if (hasHeader)
						{
							result.Add((float)headerRect.GetMidY());
						}
						result.AddRange(panelSnapPoints.Select(i => i + (float)headerRect.Bottom));
						if (hasFooter)
						{
							result.Add((float)(footerRect.GetMidY() - panelStretch));
						}
						break;
					case SnapPointsAlignment.Far:
						if (hasHeader)
						{
							result.Add((float)headerRect.Bottom);
						}
						result.AddRange(panelSnapPoints.Select(i => i + (float)headerRect.Bottom));
						if (hasFooter)
						{
							result.Add((float)(footerRect.Bottom - panelStretch));
						}
						break;
				}
			}

			return result;
		}

		public float GetRegularSnapPoints(Orientation orientation, SnapPointsAlignment alignment, out float offset) => throw new NotSupportedException("Regular snap points are not supported.");

		internal override bool CanHaveChildren() => true;

		internal static double OffsetToIndex(double offset) => Math.Max(0, offset - 2);

		internal static double IndexToOffset(int index) => index >= 0 ? index + 2 : 0;
	}
}
