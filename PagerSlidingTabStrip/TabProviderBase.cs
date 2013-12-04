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
using Android.Views;
using Android.Widget;

namespace PagerSlidingTabStrip
{
	/// <summary>
	/// Base class for your tab providers if you're not implementing it directly on your PagerAdapter.
	/// </summary>
	public abstract class TabProviderBase : ITabProvider
	{
		/// <summary>
		/// Raises the TabUpdated event
		/// </summary>
		/// <param name="position"></param>
		protected void OnTabUpdated(int position)
		{
			var evt = TabUpdated;
			if (evt != null)
				evt(this, new TabUpdateEventArgs(position));
		}

		/// <summary>
		/// Raises the TabUpdateRequired event
		/// </summary>
		/// <param name="position"></param>
		/// <param name="hint"></param>
		protected void OnTabUpdateRequired(int position, string hint = null)
		{
			var evt = TabUpdateRequired;
			if (evt != null)
				evt(this, new TabUpdateEventArgs(position, hint));
		}

		#region ITabProvider Members

		/// <summary>
		/// Raised by the tab provider when it is aware of an underlying data change that will
		/// required a tab being updated through a call to UpdateTab.
		/// </summary>
		public event EventHandler<TabUpdateEventArgs> TabUpdateRequired;

		/// <summary>
		/// Fired whenever the UI of a tab is updated in any way that could affect it's display.  E.g.,
		/// text might have changed, or a progress bar might be visible. Etc.
		/// An implementation that surfaces this event correctly will typically be firing it
		/// in its implementation of <see cref="UpdateTab" />.
		/// </summary>
		public event EventHandler<TabUpdateEventArgs> TabUpdated;

		/// <summary>
		/// Called to get the view for a particular tab, either by creating a view or recycling an old one.
		/// Note - you do not have to handle the padding, background, focusability or click event for your view -
		/// this is handled by the tab strip itself through a container control.
		/// </summary>
		/// <param name="owner">The tab strip that is requesting the view (allows access to its styled attributes and the current <see cref="Context" /></param>
		/// <param name="root">The view that will be the root for the view you pass back - typically a FrameLayout.</param>
		/// <param name="position">The position of the tab to be created.</param>
		/// <param name="recycled">A previous view that could possibly be recycled.  To indicate that this view
		/// can simply be re-bound, just return this view.</param>
		/// <returns>
		/// A view for the tab. MUST NOT RETURN NULL.
		/// </returns>
		public abstract View GetTab(PagerSlidingTabStrip owner, ViewGroup root, int position, View recycled = null);

		/// <summary>
		/// Binds the tab view - i.e. sets any text, calculates the visibility of more complex items etc.
		/// First time this is called for a tab is just after it's created or recycled by a call to <see cref="GetTab" />.
		/// </summary>
		/// <param name="view">The view to be bound.</param>
		/// <param name="owner">The tab strip that the view belongs to.</param>
		/// <param name="position">The position of the tab being updated.</param>
		/// <param name="hint">An optional string providing an implementation-specific hint for the part(s)
		/// of the view that should be updated.</param>
		public abstract void UpdateTab(View view, PagerSlidingTabStrip owner, int position, string hint = null);

		/// <summary>
		/// This is called to give the provider a chance to sync any styles defined on the passed PagerSlidingTabStrip
		/// (e.g. text styles, layout etc) to the tabs it's created.
		/// Called just after a tab is created, but also whenever a style changes on the tab strip itself.
		/// </summary>
		/// <param name="view"></param>
		/// <param name="owner"></param>
		/// <param name="position"></param>
		public abstract void UpdateTabStyle(View view, PagerSlidingTabStrip owner, int position);

		/// <summary>
		/// Call this to trigger a particular tab to be updated in response to a data change.
		/// The correct way to implement this would simply be to raise the TabUpdateRequired event,
		/// which should then be handled by the PagerSlidingTabStrip, which in turn will find the
		/// view to be updated and then call <see cref="UpdateTab" />.
		/// </summary>
		/// <param name="position">The position of the tab to be updated.</param>
		/// <param name="hint">A hint string to be passed eventually to <see cref="UpdateTab" /></param>
		public virtual void RequestTabUpdate(int position, string hint = null)
		{
			OnTabUpdateRequired(position, hint);
		}

		#endregion
	}
}