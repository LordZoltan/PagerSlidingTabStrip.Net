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

		private LinearLayout.LayoutParams _defaultTabLayoutParams;
		private LinearLayout.LayoutParams _expandedTabLayoutParams;

		//public Android.Support.V4.View.ViewPager.IOnPageChangeListener _delegatePageListener;

		private LinearLayout _tabsContainer;
		private ViewPager _pager;

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
		private bool _textAllCaps = true;
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

		private static int[] ATTRS = new int[] {
			Android.Resource.Attribute.TextSize,
			Android.Resource.Attribute.TextColor
		};

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
			_textAllCaps = a.GetBoolean(Resource.Styleable.PagerSlidingTabStrip_textAllCaps, _textAllCaps);

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

		/// <summary>
		/// Sets the view pager for this instance.
		/// </summary>
		/// <param name="pager">The pager.</param>
		/// <exception cref="System.ArgumentException">ViewPager does not have adapter instance.;pager</exception>
		public void SetViewPager(ViewPager pager)
		{
			this._pager = pager;

			if (pager.Adapter == null)
			{
				throw new ArgumentException("ViewPager does not have adapter instance.", "pager");
			}
			pager.Adapter.RegisterDataSetObserver(new PagerAdapterDataSetObserver(this));
			pager.PageScrolled += pager_PageScrolled;
			pager.PageScrollStateChanged += pager_PageScrollStateChanged;
			pager.PageSelected += pager_PageSelected;

			NotifyDataSetChanged();
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

		//public void SetOnPageChangeListener(Android.Support.V4.View.ViewPager.IOnPageChangeListener listener)
		//{ //TODO: Change this to use events instead
		//	this._delegatePageListener = listener;
		//}



		/// <summary>
		/// Used to tell this instance that the underlying tabs have changed.
		/// </summary>
		public void NotifyDataSetChanged()
		{	

			_tabsContainer.RemoveAllViews();
			var adapter = _pager.Adapter;
			_tabCount = adapter.Count;
			IIconTabProvider iconAdapter = adapter as IIconTabProvider;
			if (iconAdapter != null)
			{
				for (int i = 0; i < _tabCount; i++)
				{
					AddIconTab(i, iconAdapter.GetPageIconResId(i));
				}
			}
			else
			{
				for (int i = 0; i < _tabCount; i++)
				{
					AddTextTab(i, adapter.GetPageTitle(i));
				}
			}

			_checkedTabWidths = false;
			UpdateTabStyles();

			if (!_globalLayoutSubscribed)
			{ 
				ViewTreeObserver.GlobalLayout += ViewTreeObserver_GlobalLayout;
				_globalLayoutSubscribed = true;
			}
			_shouldObserve = true;
			RequestLayout();
			Invalidate();
		}

		private bool _shouldObserve = false;
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

		private void AddTextTab(int position, String title)
		{
			TextView tab = new TextView(Context);

			tab.SetText(title, TextView.BufferType.Normal);
			tab.Focusable = true;
			tab.Gravity = GravityFlags.Center;
			tab.SetSingleLine();

			tab.Click += (o, e) =>
			{
				_pager.SetCurrentItem(position, true);
			};

			_tabsContainer.AddView(tab);

		}

		private void AddIconTab(int position, int resId)
		{

			ImageButton tab = new ImageButton(Context);
			tab.Focusable = true;
			tab.SetImageResource(resId);

			tab.Click += (o, e) =>
			{
				_pager.SetCurrentItem(position, true);
			};

			_tabsContainer.AddView(tab);
		}

		private void UpdateTabStyles()
		{

			for (int i = 0; i < _tabCount; i++)
			{

				View v = _tabsContainer.GetChildAt(i);

				v.LayoutParameters = _defaultTabLayoutParams;
				v.SetBackgroundResource(_tabBackgroundResId);
				//if (_shouldExpand)
				//{
				//	v.SetPadding(0, 0, 0, 0);
				//}
				//else
				//{
					v.SetPadding(_tabPadding, 0, _tabPadding, 0);
				//}

				if (v is TextView)
				{

					TextView tab = (TextView)v;
					tab.SetTextSize(ComplexUnitType.Px, _tabTextSize);
					tab.SetTypeface(_tabTypeface, _tabTypefaceStyle);
					tab.SetTextColor(_tabTextColor);

					// if you compare to the java version, it branches based on the running version.
					// we can't do that as our available APIs are limited to the minimum SDK.
					if (_textAllCaps)
					{
						tab.SetText(tab.Text.ToUpper(), TextView.BufferType.Normal);
					}
				}
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="widthMeasureSpec">horizontal space requirements as imposed by the parent.
		/// The requirements are encoded with
		/// <c><see cref="T:Android.Views.View+MeasureSpec" /></c>.</param>
		/// <param name="heightMeasureSpec">vertical space requirements as imposed by the parent.
		/// The requirements are encoded with
		/// <c><see cref="T:Android.Views.View+MeasureSpec" /></c>.</param>
		/// <since version="Added in API level 1" />
		///   <altmember cref="P:Android.Views.View.MeasuredWidth" />
		///   <altmember cref="P:Android.Views.View.MeasuredHeight" />
		///   <altmember cref="M:Android.Views.View.SetMeasuredDimension(System.Int32, System.Int32)" />
		///   <altmember cref="M:Android.Views.View.get_SuggestedMinimumHeight" />
		///   <altmember cref="M:Android.Views.View.get_SuggestedMinimumWidth" />
		///   <altmember cref="M:Android.Views.View.MeasureSpec.GetMode(System.Int32)" />
		///   <altmember cref="M:Android.Views.View.MeasureSpec.GetSize(System.Int32)" />
		/// <remarks>
		///   <para tool="javadoc-to-mdoc" />
		///   <para tool="javadoc-to-mdoc">
		/// Measure the view and its content to determine the measured width and the
		/// measured height. This method is invoked by <c><see cref="M:Android.Views.View.Measure(System.Int32, System.Int32)" /></c> and
		/// should be overriden by subclasses to provide accurate and efficient
		/// measurement of their contents.
		///   </para>
		///   <para tool="javadoc-to-mdoc">
		///   <i>CONTRACT:</i> When overriding this method, you
		///   <i>must</i> call <c><see cref="M:Android.Views.View.SetMeasuredDimension(System.Int32, System.Int32)" /></c> to store the
		/// measured width and height of this view. Failure to do so will trigger an
		///   <c>IllegalStateException</c>, thrown by
		///   <c><see cref="M:Android.Views.View.Measure(System.Int32, System.Int32)" /></c>. Calling the superclass'
		///   <c><see cref="M:Android.Views.View.OnMeasure(System.Int32, System.Int32)" /></c> is a valid use.
		///   </para>
		///   <para tool="javadoc-to-mdoc">
		/// The base class implementation of measure defaults to the background size,
		/// unless a larger size is allowed by the MeasureSpec. Subclasses should
		/// override <c><see cref="M:Android.Views.View.OnMeasure(System.Int32, System.Int32)" /></c> to provide better measurements of
		/// their content.
		///   </para>
		///   <para tool="javadoc-to-mdoc">
		/// If this method is overridden, it is the subclass's responsibility to make
		/// sure the measured height and width are at least the view's minimum height
		/// and width (<c><see cref="M:Android.Views.View.get_SuggestedMinimumHeight" /></c> and
		///   <c><see cref="M:Android.Views.View.get_SuggestedMinimumWidth" /></c>).
		///   </para>
		///   <para tool="javadoc-to-mdoc">
		///   <format type="text/html">
		///   <a href="http://developer.android.com/reference/android/view/View.html#onMeasure(int, int)" target="_blank">[Android Documentation]</a>
		///   </format>
		///   </para>
		/// </remarks>
		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			base.OnMeasure(widthMeasureSpec, heightMeasureSpec);

			if (!_shouldExpand || MeasureSpec.GetMode(widthMeasureSpec) == MeasureSpecMode.Unspecified)
			{
				Android.Util.Log.Info("PagerSlidingTabStrip", "Leaving OnMeasure as _shouldExpand is false or widthMeasureSpec is Unspecified");
				return;
			}

			int myWidth = MeasuredWidth;
			int childWidth = 0;
			Android.Util.Log.Info("PagerSlidingTabStrip", string.Format("_tabCount is {0} in OnMeasure", _tabCount));
			for (int i = 0; i < _tabCount; i++)
			{
				childWidth += _tabsContainer.GetChildAt(i).MeasuredWidth;
			}

			Android.Util.Log.Info("PagerSlidingTabStrip", string.Format("childWidth is {0}, _checkedTabWidths is {1}, myWidth is {2}", childWidth, _checkedTabWidths, myWidth));

			if (!_checkedTabWidths && childWidth > 0 && myWidth > 0)
			{
				if (childWidth <= myWidth)
				{
					Android.Util.Log.Info("PagerSlidingTabStrip", string.Format("measured childWidth less than myWidth - setting all children to expanded"));
					for (int i = 0; i < _tabCount; i++)
					{
						var v = _tabsContainer.GetChildAt(i);
						v.LayoutParameters = _expandedTabLayoutParams;
						//v.Measure(widthMeasureSpec, heightMeasureSpec);
					}
				}
				else
				{
					Android.Util.Log.Info("PagerSlidingTabStrip", string.Format("measured childWidth greater than myWidth - setting all children to default"));
					for (int i = 0; i < _tabCount; i++)
					{
						var v = _tabsContainer.GetChildAt(i);
						v.LayoutParameters = _defaultTabLayoutParams;
						//v.Measure(widthMeasureSpec, heightMeasureSpec);
					}
				}
				Android.Util.Log.Info("PagerSlidingTabStrip", string.Format("Calling base OnMeasure again and setting _checkedTabWidths to true"));
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
		/// Implement this to do your drawing.
		/// </summary>
		/// <param name="canvas">the canvas on which the background will be drawn</param>
		/// <since version="Added in API level 1" />
		/// <remarks>
		///   <para tool="javadoc-to-mdoc">Implement this to do your drawing.</para>
		///   <para tool="javadoc-to-mdoc">
		///   <format type="text/html">
		///   <a href="http://developer.android.com/reference/android/view/View.html#onDraw(android.graphics.Canvas)" target="_blank">[Android Documentation]</a>
		///   </format>
		///   </para>
		/// </remarks>
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
		/// Sets the <see cref="IndicatorColor"/> to a color resource from its ID.
		/// </summary>
		/// <param name="resId">The res id.</param>
		public void SetIndicatorColor(int resId)
		{
			IndicatorColor = Resources.GetColor(resId);
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
		/// Sets the <see cref="UnderlineColor"/> to a color resource from its ID.
		/// </summary>
		/// <param name="resId">The res id.</param>
		public void SetUnderlineColor(int resId)
		{
			UnderlineColor = Resources.GetColor(resId);
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
		/// Sets the <see cref="DividerColor"/> to a color resource from its ID.
		/// </summary>
		/// <param name="resId">The res id.</param>
		public void SetDividerColor(int resId)
		{
			DividerColor = Resources.GetColor(resId);
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
		/// Gets or sets the scroll offset.
		/// 
		/// Note from Andras Zoltan - in truth I'm not entirely sure why you'd want to set this.
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
		/// Gets or sets a value indicating whether [should expand].
		/// </summary>
		/// <value>
		///   <c>true</c> if [should expand]; otherwise, <c>false</c>.
		/// </value>
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
		public bool TextAllCaps
		{
			get
			{
				return _textAllCaps;
			}
			set
			{
				_textAllCaps = value;
				//TODO: call something here to force a redraw?
				UpdateTabStyles();
			}
		}

		/// <summary>
		/// Gets or sets the size, in dps, of the text.
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
		/// Sets the <see cref="TextColor"/> to a color resource, from its ID.
		/// </summary>
		/// <param name="resId">The res id.</param>
		public void SetTextColor(int resId)
		{
			this._tabTextColor = Resources.GetColor(resId);
			UpdateTabStyles();
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

		/// <summary>
		/// Hook allowing a view to re-apply a representation of its internal state that had previously
		/// been generated by <c><see cref="M:Android.Views.View.OnSaveInstanceState" /></c>.
		/// </summary>
		/// <param name="state">The frozen state that had previously been returned by
		/// <c><see cref="M:Android.Views.View.OnSaveInstanceState" /></c>.</param>
		/// <since version="Added in API level 1" />
		///   <altmember cref="M:Android.Views.View.OnSaveInstanceState" />
		/// <remarks>
		///   <para tool="javadoc-to-mdoc">Hook allowing a view to re-apply a representation of its internal state that had previously
		/// been generated by <c><see cref="M:Android.Views.View.OnSaveInstanceState" /></c>. This function will never be called with a
		/// null state.</para>
		///   <para tool="javadoc-to-mdoc">
		///   <format type="text/html">
		///   <a href="http://developer.android.com/reference/android/view/View.html#onRestoreInstanceState(android.os.Parcelable)" target="_blank">[Android Documentation]</a>
		///   </format>
		///   </para>
		/// </remarks>
		protected override void OnRestoreInstanceState(IParcelable state)
		{
			SavedState savedState = (SavedState)state;
			base.OnRestoreInstanceState(savedState.SuperState);
			_currentPosition = savedState.CurrentPosition;
			RequestLayout();
		}

		/// <summary>
		/// Hook allowing a view to generate a representation of its internal state
		/// that can later be used to create a new instance with that same state.
		/// </summary>
		/// <returns>
		/// To be added.
		/// </returns>
		/// <since version="Added in API level 1" />
		///   <altmember cref="M:Android.Views.View.OnRestoreInstanceState(Android.OS.IParcelable)" />
		///   <altmember cref="P:Android.Views.View.SaveEnabled" />
		/// <remarks>
		///   <para tool="javadoc-to-mdoc">Hook allowing a view to generate a representation of its internal state
		/// that can later be used to create a new instance with that same state.
		/// This state should only contain information that is not persistent or can
		/// not be reconstructed later. For example, you will never store your
		/// current position on screen because that will be computed again when a
		/// new instance of the view is placed in its view hierarchy.
		///   </para>
		///   <para tool="javadoc-to-mdoc">
		/// Some examples of things you may store here: the current cursor position
		/// in a text view (but usually not the text itself since that is stored in a
		/// content provider or other persistent storage), the currently selected
		/// item in a list view.</para>
		///   <para tool="javadoc-to-mdoc">
		///   <format type="text/html">
		///   <a href="http://developer.android.com/reference/android/view/View.html#onSaveInstanceState()" target="_blank">[Android Documentation]</a>
		///   </format>
		///   </para>
		/// </remarks>
		protected override IParcelable OnSaveInstanceState()
		{
			IParcelable superState = base.OnSaveInstanceState();
			SavedState savedState = new SavedState(superState) { CurrentPosition = _currentPosition };
			return savedState;
		}

		/// <summary>
		/// The state saved by an instance of PagerSlidingTabStrip during orientation changes etc.
		/// </summary>
		protected class SavedState : BaseSavedState
		{
			/// <summary>
			/// Gets or sets the current position.
			/// </summary>
			/// <value>
			/// The current position.
			/// </value>
			public int CurrentPosition
			{
				get;
				set;
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="SavedState"/> class.
			/// </summary>
			/// <param name="superState">State of the super.</param>
			public SavedState(IParcelable superState)
				: base(superState)
			{

			}

			private SavedState(Parcel source)
				: base(source)
			{
				CurrentPosition = source.ReadInt();
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="SavedState"/> class.
			/// </summary>
			/// <param name="javaReference">The java reference.</param>
			/// <param name="transfer">The transfer.</param>
			protected SavedState(IntPtr javaReference, JniHandleOwnership transfer)
				: base(javaReference, transfer)
			{

			}

			/// <summary>
			/// Flatten this object in to a Parcel.
			/// </summary>
			/// <param name="dest">The Parcel in which the object should be written.</param>
			/// <param name="flags">Additional flags about how the object should be written.
			/// May be 0 or <c><see cref="F:Android.OS.Parcelable.ParcelableWriteReturnValue" /></c>.</param>
			/// <since version="Added in API level 1" />
			/// <remarks>
			///   <para tool="javadoc-to-mdoc">Flatten this object in to a Parcel.</para>
			///   <para tool="javadoc-to-mdoc">
			///   <format type="text/html">
			///   <a href="http://developer.android.com/reference/android/view/AbsSavedState.html#writeToParcel(android.os.Parcel, int)" target="_blank">[Android Documentation]</a>
			///   </format>
			///   </para>
			/// </remarks>
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
					return new SavedState(source);
				}

				public Java.Lang.Object[] NewArray(int size)
				{
					return new SavedState[size];
				}

				#endregion
			}
		}
	}
}