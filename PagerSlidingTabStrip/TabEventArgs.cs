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
	/// Base class of arguments for events pertaining to tabs exposed by an ITabProvider.
	/// </summary>
	public class TabEventArgs : EventArgs
	{
		private readonly int _position;

		/// <summary>
		/// Gets the position of the tab that's changed
		/// </summary>
		public int Position
		{
			get { return _position; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TabEventArgs"/> class.
		/// </summary>
		/// <param name="position">The position.</param>
		public TabEventArgs(int position)
		{
			_position = position;
		}
	}
}