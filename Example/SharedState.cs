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

namespace Example
{
	/// <summary>
	/// Use this as the container for the inprogress flag for each tab in MainActivity
	/// 
	/// In truth, storing the state for a fragment's tab inside the fragment itself is a problem,
	/// because the tab provider doesn't have access to the fragment when its updating the view.
	/// 
	/// Thus, you can use some solution where any state you use to control a tab's UI is accessible
	/// from both the fragment itself (if indeed it needs to update or read it in any way) and the 
	/// tab provider.
	/// 
	/// Plus you need a solution that is resilient to views being recreated due to orientation changes.
	/// 
	/// This class presents a method of doing this - an event is used to notify of changes to the
	/// booleans in the array.
	/// </summary>
	public static class SharedState
	{
		public static event EventHandler<int> InProgressChanged;

		private static bool[] _inProgress;

		public static int Count
		{
			get
			{
				return _inProgress != null ? _inProgress.Length : 0;
			}
			set
			{
				var currentCount = Count;
				if (value != currentCount)
				{
					if (value > 0)
						_inProgress = new bool[value];
					else
						_inProgress = null;
				}
			}
		}

		public static bool GetInProgress(int position)
		{
			if (position >= Count)
				throw new IndexOutOfRangeException();
			return _inProgress[position];
		}
		public static void SetInProgress(int position, bool value)
		{
			if (position >= Count)
				return;

			if (_inProgress[position] != value)
			{
				_inProgress[position] = value;
				var evt = InProgressChanged;
				if (evt != null)
					evt(null, position);
			}
		}
	}
}