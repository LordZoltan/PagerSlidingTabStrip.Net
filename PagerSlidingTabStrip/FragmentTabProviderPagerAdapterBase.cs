/*
 * Copyright (C) 2013 Andras Zoltan (@RealLordZoltan)
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

namespace PagerSlidingTabStrip
{
	/// <summary>
	/// This class provides a starting implementation of ITabProvider alongside
	/// the FragmentPagerAdapter class for you to use to expose custom tabs for your
	/// fragments in one go.
	/// </summary>
	public abstract class FragmentTabProviderPagerAdapterBase : FragmentPagerAdapter, ITabProvider
	{
		private Func<PagerSlidingTabStrip, ITabProvider> _getFallBackProvider;

		private static readonly Func<PagerSlidingTabStrip, ITabProvider> GetDefaultTextProvider = p => p.DefaultTextTabProvider;
		private static readonly Func<PagerSlidingTabStrip, ITabProvider> GetDefaultIconProvider = p => p.DefaultIconTabProvider;

		/// <summary>
		/// Initializes a new instance of the <see cref="FragmentTabProviderPagerAdapterBase"/> class.
		/// </summary>
		/// <param name="fragmentManager">The fragment manager.</param>
		protected FragmentTabProviderPagerAdapterBase(FragmentManager fragmentManager)
			: base(fragmentManager)
		{
			if (this is PagerSlidingTabStrip.IIconTabProvider)
				_getFallBackProvider = GetDefaultIconProvider;
			else
				_getFallBackProvider = GetDefaultTextProvider;
		}

		/// <summary>
		/// Called to raise the TabUpdateRequired event.
		/// </summary>
		/// <param name="position"></param>
		/// <param name="hint"></param>
		protected virtual void OnTabUpdateRequired(int position, string hint = null)
		{
			var evt = TabUpdateRequired;
			if (evt != null)
				evt(this, new TabUpdateEventArgs(position, hint));
		}

		/// <summary>
		/// Called to raise the TabUpdated event.
		/// </summary>
		/// <param name="position"></param>
		/// <param name="hint"></param>
		protected virtual void OnTabUpdated(int position, string hint = null)
		{
			var evt = TabUpdated;
			if (evt != null)
				evt(this, new TabUpdateEventArgs(position, hint));
		}

		#region ITabProvider Members

		/// <summary>
		/// Fired whenever a tab is updated in any way that could affect it's display.  E.g.,
		/// text might have changed, or a progress bar might be visible. Etc.
		/// An implementation that surfaces this event correctly will typically be firing it
		/// in its implementation of <see cref="UpdateTab" />.
		/// </summary>
		public event EventHandler<TabUpdateEventArgs> TabUpdated;

		/// <summary>
		/// Raised by the tab provider when it is aware of an underlying data change that will
		/// require a tab being updated through a call to UpdateTab.  A PagerSlidingTabStrip
		/// instance will subscribe to this event, find the correct view for the tab in it's
		/// view hierarchy, and then pass it to <see cref="UpdateTab" />.
		/// </summary>
		public event EventHandler<TabUpdateEventArgs> TabUpdateRequired;

		/// <summary>
		/// This is a hook for external components to trigger a tab update when some data has changed
		/// that will affect its contents or size.
		/// 
		/// The base implementation simply raises the TabUpdateRequired event, which is handled by PagerSlidingTabStrip
		/// such that it finds the tab's target view and pushes it through to <see cref="UpdateTab"/>
		/// </summary>
		/// <param name="position">The position of the tab to be updated.</param>
		/// <param name="hint">An optional hint to be eventually passed to the <see cref="UpdateTab"/>
		/// method.</param>
		public virtual void RequestTabUpdate(int position, string hint = null)
		{
			OnTabUpdateRequired(position, hint);
		}


		View ITabProvider.GetTab(PagerSlidingTabStrip owner, ViewGroup root, int position, View recycled)
		{
			var toReturn = GetTab(owner, root, position, recycled);
			if (toReturn != null)
				return toReturn;

			return _getFallBackProvider(owner).GetTab(owner, root, position, recycled);
		}

		void ITabProvider.UpdateTab(View view, PagerSlidingTabStrip owner, int position, string hint)
		{
			if (!UpdateTab(view, owner, position, hint))
			{
				_getFallBackProvider(owner).UpdateTab(view, owner, position, hint);
			}
			OnTabUpdated(position, hint);
		}

		void ITabProvider.UpdateTabStyle(View view, PagerSlidingTabStrip owner, int position)
		{
			if (!UpdateTabStyle(view, owner, position))
				_getFallBackProvider(owner).UpdateTabStyle(view, owner, position);
		}

		#endregion

		/// <summary>
		/// Called by the underlying implementation of <see cref="ITabProvider"/> to get your custom view for a 
		/// given tab.  To have the default implementation kick in (which creates text tabs by default or icon tabs
		/// if this instance also implements IIconTabProvider), simply return null.
		/// 
		/// Note - you do not have to handle the standard padding, background, focusability or click event for your view -
		/// this is handled by the tab strip itself through a container control (the root).
		/// </summary>
		/// <param name="owner">The tab strip that is requesting the view (allows access to its styled attributes and the current <see cref="Context" /></param>
		/// <param name="root">The view that will be the root for the view you pass back - typically a FrameLayout.</param>
		/// <param name="position">The position of the tab to be created.</param>
		/// <param name="recycled">A previous view that could possibly be recycled.  To indicate that this view
		/// can simply be re-bound, just return this view.</param>
		/// <returns>
		/// A view for the tab. If you return null, a default will be created (see summary).
		/// </returns>
		protected abstract View GetTab(PagerSlidingTabStrip owner, ViewGroup root, int position, View recycled = null);

		/// <summary>
		/// Called by the underlying implementation of <see cref="ITabProvider"/> to allow you to perform a custom
		/// action to update the UI of the given tab (in response to text changes etc).  If you return false, 
		/// then the default implementation will fall back to either the TextTabProvider or IconTabProvider, 
		/// depending on whether the View is a TextView or if this instance also implements the <see cref="IIconTabProvider"/>.
		/// If neither of these two conditions are met for fall back, then no update is performed.
		/// 
		/// Note that the OnTabUpdated method is called after this method finishes - unless you return false and
		/// no fall back behaviour could be selected.
		/// </summary>
		/// <param name="view">The view to be updated.</param>
		/// <param name="owner">The tab strip that owns this tab.</param>
		/// <param name="position">The tab position.</param>
		/// <param name="hint">An optional string that provides an implementation-specific hint as to the nature
		/// of the data that has changed.</param>
		/// <returns>True if you successfully handle the update.  Return false to fall back to default behaviour.</returns>
		protected abstract bool UpdateTab(View view, PagerSlidingTabStrip owner, int position, string hint = null);

		/// <summary>
		/// Called by the underlying implementation of <see cref="ITabProvider"/> to give you the chance to apply any styles
		/// defined on the PagerSlidingTabStrip to your tab's UI components (e.g. Text styles, colours etc).  Return true from this
		/// method to prevent the default functionality, which is to fallback to either the text or icon providers.
		/// </summary>
		/// <param name="view">The view to be styled.</param>
		/// <param name="owner">The tab strip that owns this tab.</param>
		/// <param name="position">The tab position.</param>
		/// <returns>True if all styling has been done, or false if you want the default behaviour to kick in.</returns>
		protected abstract bool UpdateTabStyle(View view, PagerSlidingTabStrip owner, int position);
	}
}