/*
 * Copyright (C) 2013 Andras Zoltan (@RealLordZoltan)
 * Java (original) version Copyright (C) 2013 Andreas Stuetz <andreas.stuetz@gmail.com>
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

/* This is basically a straight port of Andreas' excellent PagerSlidingTabStrip found
 * at https://github.com/astuetz/PagerSlidingTabStrip
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Database;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.View;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Interop;
using Java.Util;

namespace PagerSlidingTabStrip
{
	/// <summary>
	/// A title strip to be used with a view pager that scrolls horizontally, has touchable tabs for page selection,
	/// and has a dynamic page indicator.
	/// 
	/// You can customise the color of the background, the bottom border, the indicator and the text, as well as 
	/// the height of the control, tabs and the underline.
	/// 
	/// All-in-all, it's just miles more flexible than any of the 'stock' pager title strips, and is supported on 
	/// Android 2.2+ devices.
	/// </summary>
	public class PagerSlidingTabStrip : HorizontalScrollView
	{
		#region nested types

		/// <summary>
		/// Interface for an adapter that wants to display icons in tabs instead of text.
		/// </summary>
		public interface IIconTabProvider
		{
			/// <summary>
			/// Gets the resource ID to be used for the icon for the given page.
			/// </summary>
			/// <param name="position">The position.</param>
			int GetPageIconResId(int position);
		}

		private class PagerAdapterDataSetObserver : DataSetObserver
		{
			private readonly PagerSlidingTabStrip TabStrip;

			public PagerAdapterDataSetObserver(PagerSlidingTabStrip tabStrip)
			{
				TabStrip = tabStrip;
			}

			protected PagerAdapterDataSetObserver(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }

			public override void OnChanged()
			{
				TabStrip.NotifyDataSetChanged();
			}

			public override void OnInvalidated()
			{
				TabStrip.NotifyDataSetChanged();
			}
		}

		#endregion

		#region fields

		private LinearLayout.LayoutParams _defaultTabLayoutParams;
		private LinearLayout.LayoutParams _expandedTabLayoutParams;
		private LinearLayout _tabsContainer;
		private ViewPager _pager;
		private PagerAdapterDataSetObserver _observer;
		private PagerAdapter _adapter;
		private TabProviderFactory _tabProviderFactory;
		/// <summary>
		/// This is created by the _tabProviderFactory 
		/// </summary>
		private ITabProvider _tabProvider;
		private Lazy<ITabProvider> _textTabProvider;
		private Lazy<ITabProvider> _iconTabProvider;

		private int _tabCount;
		private int _currentPosition = 0;
		private float _currentPositionOffset = 0f;
		private Paint _rectPaint;
		private Paint _dividerPaint;
		private bool _checkedTabWidths = false;
		private Color _indicatorColor = Color.Argb(0xFF, 0x66, 0x66, 0x66);
		private Color _underlineColor = Color.Argb(0x1A, 0x00, 0x00, 0x00);
		private Color _dividerColor = Color.Argb(0x1A, 0x00, 0x00, 0x00);
		private bool _shouldExpand = false;
		private bool _tabTextAllCaps = true;
		private bool _globalLayoutSubscribed = false;
		private int _scrollOffset = 52;
		private int _indicatorHeight = 8;
		private int _underlineHeight = 2;
		private int _dividerPadding = 12;
		private int _tabPadding = 24;
		private int _dividerWidth = 1;
		private int _tabTextSize = 12;
		private Color _tabTextColor = Color.Argb(0xFF, 0x66, 0x66, 0x66);
		private Typeface _tabTypeface = null;
		private TypefaceStyle _tabTypefaceStyle = Typeface.DefaultBold.Style;
		private int _lastScrollX = 0;
		private int _tabBackgroundResId = Resource.Drawable.pagerslidingtabstrip_background_tab;
		private bool _shouldObserve = false;
		private bool _inNotifyDataSetChanged;

		#endregion

		#region properties
		private static readonly TabProviderFactory _defaultTabProviderFactory = new TabProviderFactory();
		/// <summary>
		/// Used as the default <see cref="ITabProviderFactory"/> if you don't set a custom one in <see cref="SetViewPager"/>
		/// </summary>
		public static TabProviderFactory DefaultTabProviderFactory
		{
			get
			{
				return _defaultTabProviderFactory;
			}
		}

		/// <summary>
		/// The tab provider factory used by this instance to get a tab provider to be used to manage all
		/// the tabs that are displayed in this control.  Defaults to <see cref="DefaultTabProviderFactory"/>.
		/// 
		/// Note that more complex implementations of ITabProvider can utilise this factory to help them to create
		/// non-homogenous tab lists (e.g. mixing icon and text tabs with those that have more complex layouts).
		/// </summary>
		public TabProviderFactory TabProviderFactory
		{
			get
			{
				return _tabProviderFactory ?? DefaultTabProviderFactory;
			}
		}


		/// <summary>
		/// Gets a reference to the default TextTabProvider that can be used to help manage how text is displayed 
		/// in your custom tab layout.  The underlying instance is created by the current <see cref="TabProviderFactory"/>
		/// the first time you use it.
		/// 
		/// Most of the time, this will be an instance of the <see cref="TextTabProvider"/> type.
		/// </summary>
		public ITabProvider DefaultTextTabProvider
		{
			get
			{
				if (_adapter == null || _textTabProvider == null)
					return null;
				return _textTabProvider.Value;
			}
		}

		/// <summary>
		/// Gets a reference to the default provider that can be used to help manage how a tab icon is
		/// displayed in your custom layout.  The underlying instance is created by the current <see cref="TabProviderFactory"/>
		/// the first time you use it.
		/// 
		/// Most of the time, this will be an instance of the <see cref="IconTabProvider"/> type.
		/// </summary>
		public ITabProvider DefaultIconTabProvider
		{
			get
			{
				if (_adapter == null || _iconTabProvider == null)
					return null;
				return _iconTabProvider.Value;
			}
		}

		/// <summary>
		/// Gets the adapter that is providing view pages but potentially also
		/// tabs.
		/// </summary>
		/// <value>
		/// The adapter.
		/// </value>
		protected PagerAdapter Adapter
		{
			get
			{
				return _adapter;
			}
		}

		/// <summary>
		/// Gets or sets the color of the selected tab indicator.
		/// </summary>
		/// <value>
		/// The color of the indicator.
		/// </value>
		public Color IndicatorColor
		{
			get
			{
				return this._indicatorColor;
			}
			set
			{
				this._indicatorColor = value;
				Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets the height of the indicator, in DPs
		/// </summary>
		/// <value>
		/// The height of the indicator.
		/// </value>
		public int IndicatorHeight
		{
			get
			{
				return this._indicatorHeight;
			}
			set
			{
				this._indicatorHeight = value;
				Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets the color of the underline drawn across the bottom of the whole tab strip.
		/// </summary>
		/// <value>
		/// The color of the underline.
		/// </value>
		public Color UnderlineColor
		{
			get
			{
				return this._underlineColor;
			}
			set
			{
				this._underlineColor = value;
				Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets the color of the divider drawn between each tab.
		/// </summary>
		/// <value>
		/// The color of the divider.
		/// </value>
		public Color DividerColor
		{
			get
			{
				return this._dividerColor;
			}
			set
			{
				this._dividerColor = value;
				Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets the height of the underline drawn across the bottom of the tab strip, in DPs.
		/// </summary>
		/// <value>
		/// The height of the underline.
		/// </value>
		public int UnderlineHeight
		{
			get
			{
				return _underlineHeight;
			}
			set
			{
				_underlineHeight = value;
				Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets the padding, in DPs, either side of the dividers.
		/// </summary>
		/// <value>
		/// The divider padding.
		/// </value>
		public int DividerPadding
		{
			get
			{
				return _dividerPadding;
			}
			set
			{
				_dividerPadding = value;
				Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets the scroll offset, in pixels, used when scrolling the control view
		/// to the tab for the currently selected page.
		/// </summary>
		/// <value>
		/// The scroll offset.
		/// </value>
		public int ScrollOffset
		{
			get
			{
				return _scrollOffset;
			}
			set
			{
				_scrollOffset = value;
				Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether tabs should be resized to fill
		/// the whole control if they are too small, collectively, to fill it.
		/// </summary>
		public bool ShouldExpand
		{
			get
			{
				return _shouldExpand;
			}
			set
			{
				_shouldExpand = value;
				Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the text in the tabs should be all capitals.
		/// </summary>
		public bool TabTextAllCaps
		{
			get
            {
				return _tabTextAllCaps;
			}
			set
			{
				_tabTextAllCaps = value;
				//TODO: call something here to force a redraw?
				UpdateTabStyles();
			}
		}

		/// <summary>
		/// Gets or sets the size, in pixels, of the text.
		/// </summary>
		/// <value>
		/// The size of the text.
		/// </value>
		public int TextSize
		{
			get
			{
				return _tabTextSize;
			}
			set
			{
				_tabTextSize = value;
				UpdateTabStyles();
			}
		}

		/// <summary>
		/// Gets or sets the color of the text displayed in the tabs.
		/// </summary>
		/// <value>
		/// The color of the text.
		/// </value>
		public Color TextColor
		{
			get
			{
				return _tabTextColor;
			}
			set
			{
				_tabTextColor = value;
				UpdateTabStyles();
			}
		}

		/// <summary>
		/// Gets the typeface used to draw the tab text.
		/// </summary>
		/// <value>
		/// The typeface.
		/// </value>
		public Typeface Typeface
		{
			get
			{
				return _tabTypeface;
			}
		}

		/// <summary>
		/// Gets the typeface style used to draw the tab text.
		/// </summary>
		/// <value>
		/// The typeface style.
		/// </value>
		public TypefaceStyle TypefaceStyle
		{
			get
			{
				return _tabTypefaceStyle;
			}
		}

		/// <summary>
		/// Gets or sets the tab background resource ID.
		/// </summary>
		public int TabBackground
		{
			get
			{
				return _tabBackgroundResId;
			}
			set
			{
				_tabBackgroundResId = value;
				UpdateTabStyles();
			}
		}

		/// <summary>
		/// Gets or sets the padding either side of each tab.
		/// </summary>
		/// <value>
		/// The tab padding left right.
		/// </value>
		public int TabPaddingLeftRight
		{
			get
			{
				return _tabPadding;
			}
			set
			{
				_tabPadding = value;
				UpdateTabStyles();
			}
		}

		#endregion

		#region events

		/// <summary>
		/// Raised when a page is selected.  Subscribe to this instead of the associated ViewPager's
		/// PageSelected event.
		/// </summary>
		public event EventHandler<ViewPager.PageSelectedEventArgs> PageSelected;
		/// <summary>
		/// Raised when the pager's scroll state changes.  Subscribe to this instead of the associated
		/// ViewPager's PageScrollStateChanged event.
		/// </summary>
		public event EventHandler<ViewPager.PageScrollStateChangedEventArgs> PageScrollStateChanged;
		/// <summary>
		/// Raised when the pager scrolls.  Subscribe to this instead of the associated ViewPager's
		/// PageScrolled event.
		/// </summary>
		public event EventHandler<ViewPager.PageScrolledEventArgs> PageScrolled;

		#endregion

		private static int[] ATTRS = new int[] {
			Android.Resource.Attribute.TextSize,
			Android.Resource.Attribute.TextColor
		};

		#region construction

		/// <summary>
		/// Initializes a new instance of the <see cref="PagerSlidingTabStrip"/> class.
		/// </summary>
		/// <param name="context">The context.</param>
		public PagerSlidingTabStrip(Context context) : this(context, null) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="PagerSlidingTabStrip"/> class.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="attrs">The attrs.</param>
		public PagerSlidingTabStrip(Context context, IAttributeSet attrs) : this(context, attrs, 0) { }


		/// <summary>
		/// Initializes a new instance of the <see cref="PagerSlidingTabStrip"/> class.
		/// </summary>
		/// <param name="javaReference">The java reference.</param>
		/// <param name="transfer">The transfer.</param>
		protected PagerSlidingTabStrip(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="PagerSlidingTabStrip"/> class.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="attrs">The attributes from xml.</param>
		/// <param name="defStyle">The default style.</param>
		public PagerSlidingTabStrip(Context context, IAttributeSet attrs, int defStyle)
			: base(context, attrs, defStyle)
		{
			HorizontalScrollBarEnabled = false;
			FillViewport = true;
			SetWillNotDraw(false);
			_tabsContainer = new LinearLayout(context);
			_tabsContainer.Orientation = Android.Widget.Orientation.Horizontal;
			_tabsContainer.LayoutParameters = new ViewGroup.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);
			AddView(_tabsContainer);

			DisplayMetrics dm = Resources.DisplayMetrics;
			_scrollOffset = Convert.ToInt32(TypedValue.ApplyDimension(ComplexUnitType.Dip, _scrollOffset, dm));
			_indicatorHeight = Convert.ToInt32(TypedValue.ApplyDimension(ComplexUnitType.Dip, _indicatorHeight, dm));
			_underlineHeight = Convert.ToInt32(TypedValue.ApplyDimension(ComplexUnitType.Dip, _underlineHeight, dm));
			_dividerPadding = Convert.ToInt32(TypedValue.ApplyDimension(ComplexUnitType.Dip, _dividerPadding, dm));
			_tabPadding = Convert.ToInt32(TypedValue.ApplyDimension(ComplexUnitType.Dip, _tabPadding, dm));
			_dividerWidth = Convert.ToInt32(TypedValue.ApplyDimension(ComplexUnitType.Dip, _dividerWidth, dm));
			_tabTextSize = Convert.ToInt32(TypedValue.ApplyDimension(ComplexUnitType.Dip, _tabTextSize, dm));

			// get system attrs (android:textSize and android:textColor)

			TypedArray a = context.ObtainStyledAttributes(attrs, ATTRS);

			_tabTextSize = a.GetDimensionPixelSize(0, _tabTextSize);
			_tabTextColor = a.GetColor(1, _tabTextColor);

			a.Recycle();

			// get custom attrs

			a = context.ObtainStyledAttributes(attrs, Resource.Styleable.PagerSlidingTabStrip);

			_indicatorColor = a.GetColor(Resource.Styleable.PagerSlidingTabStrip_indicatorColor, _indicatorColor);
			_underlineColor = a.GetColor(Resource.Styleable.PagerSlidingTabStrip_underlineColor, _underlineColor);
			_dividerColor = a.GetColor(Resource.Styleable.PagerSlidingTabStrip_dividerColor, _dividerColor);
			_indicatorHeight = a.GetDimensionPixelSize(Resource.Styleable.PagerSlidingTabStrip_indicatorHeight, _indicatorHeight);
			_underlineHeight = a.GetDimensionPixelSize(Resource.Styleable.PagerSlidingTabStrip_underlineHeight, _underlineHeight);
			_dividerPadding = a.GetDimensionPixelSize(Resource.Styleable.PagerSlidingTabStrip_dividerPadding, _dividerPadding);
			_tabPadding = a.GetDimensionPixelSize(Resource.Styleable.PagerSlidingTabStrip_tabPaddingLeftRight, _tabPadding);
			_tabBackgroundResId = a.GetResourceId(Resource.Styleable.PagerSlidingTabStrip_tabBackground, _tabBackgroundResId);
			_shouldExpand = a.GetBoolean(Resource.Styleable.PagerSlidingTabStrip_shouldExpand, _shouldExpand);
			_scrollOffset = a.GetDimensionPixelSize(Resource.Styleable.PagerSlidingTabStrip_scrollOffset, _scrollOffset);
			_tabTextAllCaps = a.GetBoolean(Resource.Styleable.PagerSlidingTabStrip_tabTextAllCaps, _tabTextAllCaps);

			a.Recycle();

			_rectPaint = new Paint();
			_rectPaint.AntiAlias = true;
			_rectPaint.SetStyle(Android.Graphics.Paint.Style.Fill);

			_dividerPaint = new Paint();
			_dividerPaint.AntiAlias = true;
			_dividerPaint.StrokeWidth = _dividerWidth;

			_defaultTabLayoutParams = new LinearLayout.LayoutParams(LayoutParams.WrapContent, LayoutParams.MatchParent);
			_expandedTabLayoutParams = new LinearLayout.LayoutParams(0, LayoutParams.MatchParent, 1.0f);
		}

		#endregion

		/// <summary>
		/// Sets the view pager for this instance, from this the control inherits the PagerAdapter and then
		/// derives the <see cref="ITabProvider"/> - unless you also pass a custom <see cref="TabProviderFactory"/>
		/// in the <paramref name="tabProviderFactory"/> .
		/// </summary>
		/// <param name="pager">The pager.</param>
		/// <param name="tabProviderFactory">The factory to use to select the correct tab provider for the given
		/// pager, and equally as a factory for more complex tab providers to delegate to if they want to reuse
		/// .  This also then is set on the .</param>
		/// <exception cref="System.ArgumentException">ViewPager does not have adapter instance.;pager</exception>
		public void SetViewPager(ViewPager pager, TabProviderFactory tabProviderFactory = null)
		{
			if (pager == null)
				return;
			if (pager.Adapter == null)
			{
				throw new ArgumentException("ViewPager does not have adapter instance.", "pager");
			}

			//the property returns the default factory if set to null here.
			_tabProviderFactory = tabProviderFactory;

			//changes made here to be more tolerant of being set to the same pager or to a pager with
			//the same adapter, or to the same pager with a different adapter.
			if (_pager != pager)
			{
				if (_pager != null)
				{
					pager.PageScrolled -= pager_PageScrolled;
					pager.PageScrollStateChanged -= pager_PageScrollStateChanged;
					pager.PageSelected -= pager_PageSelected;
				}
				this._pager = pager;
				pager.PageScrolled += pager_PageScrolled;
				pager.PageScrollStateChanged += pager_PageScrollStateChanged;
				pager.PageSelected += pager_PageSelected;
			}

			if (_adapter != pager.Adapter)
			{
				if (_adapter != null && _observer != null)
				{
					_adapter.UnregisterDataSetObserver(_observer);
				}

				_adapter = pager.Adapter;
				_tabProviderFactory = tabProviderFactory;
				//re-create the Lazys for the default text and tab providers
				_textTabProvider = new Lazy<ITabProvider>(() => TabProviderFactory.CreateTextTabProvider(Context, _adapter));
				_iconTabProvider = new Lazy<ITabProvider>(() => TabProviderFactory.CreateIconTabProvider(Context, _adapter));
				//avoid recycling any previous views, because we've changed adapters.
				_tabsContainer.RemoveAllViews();

				var newProvider = TabProviderFactory.CreateTabProvider(Context, _adapter);
				if (newProvider != _tabProvider && _tabProvider != null)
				{
					//good housekeeping
					_tabProvider.TabUpdated -= _tabProvider_TabUpdated;
					_tabProvider.TabUpdateRequired -= _tabProvider_TabUpdateRequired;
				}

				_tabProvider = newProvider;
				_tabProvider.TabUpdated += _tabProvider_TabUpdated;
				_tabProvider.TabUpdateRequired += _tabProvider_TabUpdateRequired;

				if (_observer == null)
					_observer = new PagerAdapterDataSetObserver(this);

				_adapter.RegisterDataSetObserver(_observer);
			}


			NotifyDataSetChanged();
		}

		void _tabProvider_TabUpdated(object sender, TabUpdateEventArgs e)
		{
			//TODO: Consider a 'StartTabUpdates' and 'EndTabUpdates' method - *this* event handler would
			//not request layout or invalidate after StartTabUpdates and before EndTabUpdates, instead keeping count
			//of each of the tabs that raise the event.  
			//When EndTabUpdates is called, if one ore tabs fired that event, then a re-layout and redraw will be
			//requested.

			//don't request layout and redraw if NotifyDataSetChanged is currently being called.
			//This is because in that method we call the tab provider's UpdateTab method, which *should*
			//raise this method if the implementation has been done properly.
			if (_inNotifyDataSetChanged)
			{
				return;
			}
			RequestLayout();
			Invalidate();
		}

		/// <summary>
		/// Call this to force a tab to update it's UI and, optionally, have the tab control re-layout
		/// and redraw.
		/// </summary>
		/// <param name="position">The position of the tab to be updated.</param>
		/// <param name="hint">Optional hint to be passed to the underlying <see cref="ITabProvider"/>'s 
		/// <see cref="ITabProvider.UpdateTab"/> method.</param>
		public void UpdateTab(int position, string hint = null)
		{
			if (position >= _tabCount)
				return;

			var container = _tabsContainer.GetChildAt(position) as FrameLayout;
			if (container == null || container.ChildCount != 1)
				return;
			//instruct the provider to do the update
			_tabProvider.UpdateTab(container.GetChildAt(0), this, position, hint);
			//if the provider deems an update necessary, it should then raise the TabUpdated event, 
			//triggering a re-layout and Invalidate here
		}

		void _tabProvider_TabUpdateRequired(object sender, TabUpdateEventArgs e)
		{
			UpdateTab(e.Position, e.Hint);
		}

		void pager_PageSelected(object sender, ViewPager.PageSelectedEventArgs e)
		{
			var evt = PageSelected;
			if (evt != null)
			{
				evt(this, e);
			}
		}

		void pager_PageScrollStateChanged(object sender, ViewPager.PageScrollStateChangedEventArgs e)
		{
			if (e.State == ViewPager.ScrollStateIdle)
			{
				//fix the position offset to 0 and the current position to the
				//page currently at rest - fixes a couple of bugs not seen in the Java version
				//whereby scrolling to the right would eventually see the tab to the right
				//of the current selected one underlined once the pager settles, but as soon
				//you move the pager it would draw correctly again.
				//The other bug was similar, but occurred when the user scrolled a page a little
				//bit left or right, then let go - the pager would bounce back to where it was,
				//and the indicator would suddenly jump to the wrong tab until you started moving
				//the pager again.
				_currentPositionOffset = 0.0f;
				_currentPosition = _pager.CurrentItem;
				ScrollToChild(_pager.CurrentItem, 0);
			}

			var evt = PageScrollStateChanged;
			if (evt != null)
			{
				evt(this, e);
			}
		}

		void pager_PageScrolled(object sender, ViewPager.PageScrolledEventArgs e)
		{
			_currentPosition = e.Position;
			_currentPositionOffset = e.PositionOffset;

			ScrollToChild(e.Position, (int)(e.PositionOffset * _tabsContainer.GetChildAt(e.Position).Width));

			Invalidate();

			var evt = PageScrolled;
			if (evt != null)
			{
				evt(this, e);
			}
		}


		/// <summary>
		/// Used to tell this instance that the underlying tabs have changed.  In general you won't actually call 
		/// this method directly, but instead will rely on data set updates, <see cref="ITabProvider"/> events 
		/// or the <see cref="UpdateTab"/> method to notify about updates - as those mechanisms are generally
		/// generate less workload.
		/// </summary>
		public void NotifyDataSetChanged()
		{
			_inNotifyDataSetChanged = true;
			int currentViewCount = _tabsContainer.ChildCount;
			_tabCount = _adapter.Count;
			int viewsToAddCount = _tabCount - currentViewCount;
			_checkedTabWidths = false;

			//means we already have too many views.
			if (viewsToAddCount < 0)
			{
#if DEBUG
				Android.Util.Log.Info("PagerSlidingTabStrip", string.Format("Need to delete {0} tabs as the number of tabs has reduced", Math.Abs(viewsToAddCount)));
#endif
				while (viewsToAddCount++ != 0)
				{
					_tabsContainer.RemoveViewAt(_tabCount);
				}
			}

			FrameLayout tabContainer;
			View toRecycle;
			View newView = null;

			for (int i = 0; i < _tabCount; i++)
			{
				if (i < _tabsContainer.ChildCount)
				{
					tabContainer = _tabsContainer.GetChildAt(i) as FrameLayout;

					if (tabContainer != null)
					{
#if DEBUG
					Android.Util.Log.Info("PagerSlidingTabStrip", "Found old tab FrameLayout, looking to recycle its current child");
#endif
						//the upshot of this is that is another component starts mucking about with our shizzle
						//and inserting other types of views along with our tabs, this algorithm is going to 
						//break and tabs won't be created properly.
						if (tabContainer.ChildCount == 1)
						{
							toRecycle = tabContainer.GetChildAt(0);
							newView = _tabProvider.GetTab(this, tabContainer, i, toRecycle);

							if (newView != toRecycle)
							{
#if DEBUG
								Android.Util.Log.Info("PagerSlidingTabStrip", "Old tab not recycled by ITabProvider implementation - adding new tab");
#endif
								tabContainer.RemoveViewAt(0);
								tabContainer.AddView(newView);
							}
						}
					}
				}
				else
				{
#if DEBUG
					Android.Util.Log.Info("PagerSlidingTabStrip", "Creating brand new FrameLayout for tab and its content");
#endif
					tabContainer = new FrameLayout(Context);
					//tabContainer.LayoutParameters = _defaultTabLayoutParams;
					newView = _tabProvider.GetTab(this, tabContainer, i);
					tabContainer.AddView(newView);
					AddTabClick(tabContainer, i);
					_tabsContainer.AddView(tabContainer);
				}

				if (newView != null)
					_tabProvider.UpdateTab(newView, this, i);
			}

			UpdateTabStyles();

			if (!_globalLayoutSubscribed)
			{
				ViewTreeObserver.GlobalLayout += ViewTreeObserver_GlobalLayout;
				_globalLayoutSubscribed = true;
			}
			_shouldObserve = true;
			_inNotifyDataSetChanged = false;
			RequestLayout();
			Invalidate();
		}

		private void AddTabClick(View v, int position)
		{
			v.Click += (o, e) =>
			{
				SetCurrentItem(position, true);
			};
		}

		/// <summary>
		/// Sets the current item in both the view pager and the tab control.  This is a 
		/// wrapper for calling the underlying adapter's SetCurrentItem if the caller does
		/// not have access to that object, however note that even if you do, you should 
		/// use this method to set the position because otherwise the tab might not update.
		/// </summary>
		/// <param name="position">The tab position to be set as the current item.</param>
		/// <param name="smoothScroll">if set to <c>true</c> [smooth scroll].</param>
		public void SetCurrentItem(int position, bool smoothScroll)
		{
			_currentPosition = position;
			_currentPositionOffset = 0;
			_pager.SetCurrentItem(position, smoothScroll);
		}

		void ViewTreeObserver_GlobalLayout(object sender, EventArgs e)
		{
			//altered from Java version to use a flag because unsubscribing throws a NotSupportedException
			//in either some or all circumstances.
			if (_shouldObserve)
			{
				_currentPosition = _pager.CurrentItem;
				_currentPositionOffset = 0.0f;
				if ((_pager.Adapter.Count - 1) < _currentPosition)
				{
					_currentPosition = 0;
					_pager.SetCurrentItem(0, false);
				}

				ScrollToChild(_currentPosition, 0);

				_shouldObserve = false;
			}
		}

		private void UpdateTabStyles()
		{
			for (int i = 0; i < _tabCount; i++)
			{
				View v = _tabsContainer.GetChildAt(i);

				v.LayoutParameters = _defaultTabLayoutParams;
				v.SetBackgroundResource(_tabBackgroundResId);
				v.SetPadding(_tabPadding, 0, _tabPadding, 0);

				FrameLayout vLayout = v as FrameLayout;
				if (vLayout != null && vLayout.ChildCount == 1)
				{
					//the first and only child of the framelayout is the 
					//view that was created by the tab provider - fetch it.
					v = vLayout.GetChildAt(0);
					_tabProvider.UpdateTabStyle(v, this, i);
				}
			}
		}

		/// <summary>
		/// Implementation of the base method.  Tabs are measure in here to see if they overflow
		/// or not.  If not, and ShouldExpand is true, then their layout is changed so that they
		/// are all given equal share of the total width of the container.  This calculation is 
		/// performed once and only repeated if a change occurs in the tabs that could affect layout.
		/// </summary>
		/// <param name="widthMeasureSpec"></param>
		/// <param name="heightMeasureSpec"></param>
		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			base.OnMeasure(widthMeasureSpec, heightMeasureSpec);

			if (!_shouldExpand || MeasureSpec.GetMode(widthMeasureSpec) == MeasureSpecMode.Unspecified)
			{
				return;
			}

			int myWidth = MeasuredWidth;
			int childWidth = 0;
			for (int i = 0; i < _tabCount; i++)
			{
				childWidth += _tabsContainer.GetChildAt(i).MeasuredWidth;
			}

			if (!_checkedTabWidths && childWidth > 0 && myWidth > 0)
			{
				if (childWidth <= myWidth)
				{
					for (int i = 0; i < _tabCount; i++)
					{
						var v = _tabsContainer.GetChildAt(i);
						v.LayoutParameters = _expandedTabLayoutParams;
					}
				}
				else
				{
					for (int i = 0; i < _tabCount; i++)
					{
						var v = _tabsContainer.GetChildAt(i);
						v.LayoutParameters = _defaultTabLayoutParams;
					}
				}
				//re-measure now as we've potentially altered the widths of the child tabs
				base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
				_checkedTabWidths = true;
			}
		}

		private void ScrollToChild(int position, int offset)
		{

			if (_tabCount == 0)
			{
				return;
			}

			int newScrollX = _tabsContainer.GetChildAt(position).Left + offset;

			if (position > 0 || offset > 0)
			{
				newScrollX -= _scrollOffset;
			}

			if (newScrollX != _lastScrollX)
			{
				_lastScrollX = newScrollX;
				ScrollTo(newScrollX, 0);
			}

		}

		/// <summary>
		/// Overriden to perform the custom drawing required by this control - dividers, control underline and tab dividers.
		/// </summary>
		/// <param name="canvas">the canvas on which the background will be drawn</param>
		protected override void OnDraw(Canvas canvas)
		{
			base.OnDraw(canvas);

			if (IsInEditMode || _tabCount == 0)
			{
				//Log(LogPriority.Info, "Exiting OnDraw early");
				return;
			}

			int height = Height;

			// draw indicator line

			_rectPaint.Color = _indicatorColor;

			// default: line below current tab
			View currentTab = _tabsContainer.GetChildAt(_currentPosition);
			float lineLeft = currentTab.Left;
			float lineRight = currentTab.Right;
			// if there is an offset, start interpolating left and right coordinates between current and next tab

			if (_currentPositionOffset > 0f && _currentPosition < _tabCount - 1)
			{
				View nextTab = _tabsContainer.GetChildAt(_currentPosition + 1);
				float nextTabLeft = nextTab.Left;
				float nextTabRight = nextTab.Right;

				lineLeft = (_currentPositionOffset * nextTabLeft + (1f - _currentPositionOffset) * lineLeft);
				lineRight = (_currentPositionOffset * nextTabRight + (1f - _currentPositionOffset) * lineRight);
			}

			canvas.DrawRect(lineLeft, height - _indicatorHeight, lineRight, height, _rectPaint);

			// draw underliner
			_rectPaint.Color = _underlineColor;
			canvas.DrawRect(0, height - _underlineHeight, _tabsContainer.Width, height, _rectPaint);

			// draw divider

			_dividerPaint.Color = _dividerColor;
			for (int i = 0; i < _tabCount - 1; i++)
			{
				View tab = _tabsContainer.GetChildAt(i);
				canvas.DrawLine(tab.Right, _dividerPadding, tab.Right, height - _dividerPadding, _dividerPaint);
			}
		}



		/// <summary>
		/// Sets the <see cref="IndicatorColor"/> to a color resource from its ID.
		/// </summary>
		/// <param name="resId">The res id.</param>
		public void SetIndicatorColor(int resId)
		{
			IndicatorColor = Resources.GetColor(resId);
		}

		/// <summary>
		/// Sets the <see cref="UnderlineColor"/> to a color resource from its ID.
		/// </summary>
		/// <param name="resId">The res id.</param>
		public void SetUnderlineColor(int resId)
		{
			UnderlineColor = Resources.GetColor(resId);
		}

		/// <summary>
		/// Sets the <see cref="DividerColor"/> to a color resource from its ID.
		/// </summary>
		/// <param name="resId">The res id.</param>
		public void SetDividerColor(int resId)
		{
			DividerColor = Resources.GetColor(resId);
		}

		/// <summary>
		/// Sets the <see cref="TextColor"/> to a color resource, from its ID.
		/// </summary>
		/// <param name="resId">The res id.</param>
		public void SetTextColor(int resId)
		{
			this._tabTextColor = Resources.GetColor(resId);
			UpdateTabStyles();
		}

		/// <summary>
		/// Sets the typeface and style used to draw the tab text.
		/// 
		/// Please note - if the current adapter is an <see cref="IIconTabProvider"/> this will
		/// have no effect.
		/// </summary>
		/// <param name="typeface">The typeface.</param>
		/// <param name="style">The style.</param>
		public void SetTypeface(Typeface typeface, TypefaceStyle style)
		{
			this._tabTypeface = typeface;
			this._tabTypefaceStyle = style;
			UpdateTabStyles();
		}

		/// <summary>
		/// Override of <see cref="Android.Views.View.OnSaveInstanceState"/>.
		/// </summary>
		/// <param name="state">The state that was previously saved by <see cref="OnSaveInstanceState"/></param>
		protected override void OnRestoreInstanceState(IParcelable state)
		{
			//tried doing this with a nested state class, but Android would not have it.  Must have implemented
			//it incorrectly.  So using a bundle seemed the most logical solution.
			Bundle bundle = state as Bundle;
			if (bundle != null)
			{
				IParcelable superState = bundle.GetParcelable("base") as IParcelable;
				if (superState != null)
					base.OnRestoreInstanceState(superState);
				_currentPosition = bundle.GetInt("currentPosition", 0);
			}

			RequestLayout();
		}

		/// <summary>
		/// Override of <see cref="Android.Views.View.OnSaveInstanceState"/>.  Creates a <see cref="PagerSlidingTabStripState"/> instance with
		/// the current position, encompassing any base saved state, and returns it.
		/// </summary>
		protected override IParcelable OnSaveInstanceState()
		{
			//see notes in OnRestoreInstanceState about using Bundle.
			var superState = base.OnSaveInstanceState();
			Bundle state = new Bundle();
			state.PutParcelable("base", superState);
			state.PutInt("currentPosition", _currentPosition);
			return state;
		}

		/// <summary>
		/// The state saved by an instance of PagerSlidingTabStrip during orientation changes etc.
		/// </summary>
		public class PagerSlidingTabStripState : BaseSavedState
		{
			/// <summary>
			/// Gets or sets the current position.
			/// </summary>
			/// <value>
			/// The current position.
			/// </value>
			public int CurrentPosition { get; set; }

			/// <summary>
			/// Initializes a new instance of the <see cref="PagerSlidingTabStripState"/> class.
			/// </summary>
			/// <param name="superState">State of the super.</param>
			public PagerSlidingTabStripState(IParcelable superState)
				: base(superState)
			{

			}

			public PagerSlidingTabStripState(Parcel source)
				: base(source)
			{
				CurrentPosition = source.ReadInt();
			}

			/// <summary>
			/// Implementation of AbsSavedState.WriteToParcel
			/// 
			/// This is overriden to 
			/// </summary>
			/// <param name="dest">The Parcel in which the object should be written.</param>
			/// <param name="flags">Additional flags about how the object should be written.
			/// May be 0 or <c><see cref="F:Android.OS.Parcelable.ParcelableWriteReturnValue" /></c>.</param>
			public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
			{
				base.WriteToParcel(dest, flags);
				dest.WriteInt(CurrentPosition);
			}

			[ExportField("CREATOR")]
			static SavedStateCreator InitializeCreator()
			{
				return new SavedStateCreator();
			}

			class SavedStateCreator : Java.Lang.Object, IParcelableCreator
			{

				#region IParcelableCreator Members

				public Java.Lang.Object CreateFromParcel(Parcel source)
				{
					return new PagerSlidingTabStripState(source);
				}

				public Java.Lang.Object[] NewArray(int size)
				{
					return new PagerSlidingTabStripState[size];
				}

				#endregion
			}
		}
	}
}