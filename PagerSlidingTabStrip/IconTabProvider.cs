using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;

namespace PagerSlidingTabStrip
{
	/// <summary>
	/// Default implementation of an ITabProvider that uses icons instead of text.
	/// 
	/// Use of this provider is triggered automatically if the PagerAdapter given to a
	/// PagerSlidingTabStrip implements its IIconTabProvider nested interface.
	/// </summary>
	public class IconTabProvider : TabProviderBase
	{
		private readonly Context _context;

		/// <summary>
		/// Gets the context for resources etc.
		/// </summary>
		protected Context Context
		{
			get { return _context; }
		}

		private readonly PagerAdapter _adapter;

		/// <summary>
		/// Gets the adapter.
		/// </summary>
		/// <value>
		/// The adapter.
		/// </value>
		protected PagerAdapter Adapter
		{
			get { return _adapter; }
		}

		/// <summary>
		/// Gets the icon provider for this instance.
		/// 
		/// The base implementation defaults to grabbing the instance from the 
		/// <see cref="Adapter"/> via a cast.  Override to provide a different
		/// way to fetch the icon provider.
		/// </summary>
		protected virtual PagerSlidingTabStrip.IIconTabProvider IconProvider
		{
			get 
			{
				return _adapter as PagerSlidingTabStrip.IIconTabProvider;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TextTabProvider"/> class.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="adapter">The adapter.</param>
		public IconTabProvider(Context context, PagerAdapter adapter)
		{
			_context = context;
			_adapter = adapter;
		}

		/// <summary>
		/// Gets the tab.
		/// </summary>
		/// <param name="owner">The owner.</param>
		/// <param name="root">The root.</param>
		/// <param name="position">The position.</param>
		/// <param name="recycled">The recycled.</param>
		/// <returns></returns>
		public override View GetTab(PagerSlidingTabStrip owner, ViewGroup root, int position, View recycled = null)
		{
			ImageView tab = recycled as ImageView;

			//ImageButton tab = recycled as ImageButton;
			if(recycled != null)
				return tab;

			tab = new ImageView(Context);
			tab.SetPadding(0, 0, 0, 0);
			tab.LayoutParameters = new FrameLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent);

			return tab;
		}

		/// <summary>
		/// Updates the tab.
		/// </summary>
		/// <param name="view">The view.</param>
		/// <param name="owner">The owner.</param>
		/// <param name="position">The position.</param>
		/// <param name="hint">The hint.</param>
		/// <exception cref="System.InvalidOperationException">No IconProvider is available.</exception>
		public override void UpdateTab(View view, PagerSlidingTabStrip owner, int position, string hint = null)
		{
			ImageView v = view as ImageView;
			if (v == null)
				return;
			var iconProvider = IconProvider;
			
			if(iconProvider == null)
				throw new InvalidOperationException("No IconProvider is available.");
			v.SetImageResource(iconProvider.GetPageIconResId(position));
		}

		/// <summary>
		/// Updates the tab style.
		/// </summary>
		/// <param name="view">The view.</param>
		/// <param name="owner">The owner.</param>
		/// <param name="position">The position.</param>
		public override void UpdateTabStyle(View view, PagerSlidingTabStrip owner, int position)
		{
			//don't need to inherit any styling here
		}
	}
}