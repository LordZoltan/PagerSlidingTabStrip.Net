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
	/// Interface for an adapter that wants complete control over the views that are
	/// created for each of its tabs.
	/// 
	/// Note that when an Adapter supports this interface, the tab strip will still control 
	/// the padding and width of each of your tabs
	/// </summary>
	public interface ITabProvider
	{
		/// <summary>
		/// Fired whenever a tab is updated in any way that could affect it's display.  E.g., 
		/// text might have changed, or a progress bar might be visible. Etc.
		/// 
		/// An implementation that surfaces this event correctly will typically be firing it
		/// in its implementation of <see cref="UpdateTab"/>.
		/// </summary>
		event EventHandler<TabUpdateEventArgs> TabUpdated;

		/// <summary>
		/// Raised by the tab provider when it is aware of an underlying data change that will
		/// require a tab being updated through a call to UpdateTab.  A PagerSlidingTabStrip
		/// instance will subscribe to this event, find the correct view for the tab in it's
		/// view hierarchy, and then pass it to <see cref="UpdateTab"/>.
		/// </summary>
		event EventHandler<TabUpdateEventArgs> TabUpdateRequired;

		/// <summary>
		/// Call this to trigger a particular tab to be updated in response to a data change.
		/// 
		/// The correct way to implement this would simply be to raise the TabUpdateRequired event,
		/// which should then be handled by the PagerSlidingTabStrip, which in turn will find the 
		/// view to be updated and then call <see cref="UpdateTab"/>.
		/// </summary>
		/// <param name="position">The position of the tab to be updated.</param>
		/// <param name="hint">A hint string to be passed eventually to <see cref="UpdateTab"/></param>
		void RequestTabUpdate(int position, string hint = null);

		/// <summary>
		/// Called to get the view for a particular tab, either by creating a view or recycling an old one.
		/// Note - you do not have to handle the padding, background, focusability or click event for your view - 
		/// this is handled by the tab strip itself through a container control.
		/// </summary>
		/// <param name="owner">The tab strip that is requesting the view (allows access to its styled attributes and the current <see cref="Context"/></param>
		/// <param name="root">The view that will be the root for the view you pass back - typically a FrameLayout.</param>
		/// <param name="position">The position of the tab to be created.</param>
		/// <param name="recycled">A previous view that could possibly be recycled.  To indicate that this view
		/// can simply be re-bound, just return this view.</param>
		/// <returns>A view for the tab. MUST NOT RETURN NULL.</returns>
		View GetTab(PagerSlidingTabStrip owner, ViewGroup root, int position, View recycled = null);
		/// <summary>
		/// Binds the tab view - i.e. sets any text, calculates the visibility of more complex items etc.
		/// First time this is called for a tab is just after it's created or recycled by a call to <see cref="GetTab"/>.
		/// 
		/// Note that this is only designed to be called by a PagerSlidingTabStrip.  If you simply wish to trigger a tab update
		/// in response to data/state being changed, then 
		/// </summary>
		/// <param name="view">The view to be bound.</param>
		/// <param name="owner">The tab strip that the view belongs to.</param>
		/// <param name="position">The position of the tab being updated.</param>
		/// <param name="hint">An optional string providing an implementation-specific hint for the part(s)
		/// of the view that should be updated.</param>
		void UpdateTab(View view, PagerSlidingTabStrip owner, int position, string hint = null);
		/// <summary>
		/// This is called to give the provider a chance to sync any styles defined on the passed PagerSlidingTabStrip
		/// (e.g. text styles, layout etc) to the tabs it's created.
		/// 
		/// Called just after a tab is created, but also whenever a style changes on the tab strip itself.
		/// </summary>
		/// <param name="view"></param>
		/// <param name="owner"></param>
		/// <param name="position"></param>
		void UpdateTabStyle(View view, PagerSlidingTabStrip owner, int position);
	}
}