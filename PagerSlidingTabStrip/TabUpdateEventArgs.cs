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
	/// Event arguments object for the <see cref="ITabProvider.TabUpdated"/> and 
	/// <see cref="ITabProvider.TabUpdateRequired"/> events.
	/// </summary>
	public class TabUpdateEventArgs : TabEventArgs 
	{
		private readonly string _hint;

		/// <summary>
		/// Gets the update hint.
		/// </summary>
		/// <value>
		/// The hint.
		/// </value>
		public string Hint
		{
			get { return _hint; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TabUpdateEventArgs"/> class.
		/// </summary>
		/// <param name="position">The position.</param>
		public TabUpdateEventArgs(int position, string hint = null)
			: base(position)
		{
			_hint = hint;
		}
	}
}