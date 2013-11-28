using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using PagerSlidingTabStrip;

namespace Example
{
	public class SuperAwesomeCardFragment : Android.Support.V4.App.Fragment
	{
		private int _position;
		private bool _enableProgressButton;

		//this uses an event to show one way to get a fragment to notify its host that its title needs to change in some way
		public event EventHandler<int> ChangeTitleRequested;

		//the progress bar on/off change is done alternatively through a direct back-reference to the ITabProvider when the
		//MainActivity's MyPageAdapter2 is used, to show another way of doing it.

		public SuperAwesomeCardFragment()
		{
			Android.Util.Log.Info("SuperAwesomeCardFragment", "Default constructor called.");
		}

		protected SuperAwesomeCardFragment(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) {
			Android.Util.Log.Info("SuperAwesomeCardFragment", "Protected constructor called.");
		}

		internal void Configure(int position, bool enableProgressButton = false)
		{
			Android.Util.Log.Info("SuperAwesomeCardFragment", string.Format("Configure called with position {0}, enableProgressButton: {1}", position, enableProgressButton));
			_position = position;
			_enableProgressButton = enableProgressButton;
			//reset this event
			ChangeTitleRequested = null;
		}

		public override void OnCreate(Android.OS.Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			Android.Util.Log.Info("SuperAwesomeCardFragment", string.Format("OnCreate called"));
			if (savedInstanceState != null)
			{
				_position = savedInstanceState.GetInt("position", 0);
				_enableProgressButton = savedInstanceState.GetBoolean("enableProgressButton", false);
			}
			if (this.View != null)
			{
				Android.Util.Log.Info("SuperAwesomeCardFragment", string.Format(string.Format("View already present for position {0}", _position)));
			}
		}

		public override Android.Views.View OnCreateView(Android.Views.LayoutInflater inflater, Android.Views.ViewGroup container, Android.OS.Bundle savedInstanceState)
		{
			Android.Util.Log.Info("SuperAwesomeCardFragment", string.Format("CreateView being called for position {0}", _position));
			var view = inflater.Inflate(Resource.Layout.SuperAwesomeCardFragment, container, false);
			var v = view.FindViewById<TextView>(Android.Resource.Id.Text1);

			if (_enableProgressButton)
			{
				var button = view.FindViewById<Button>(Resource.Id.btnToggleProgress);
				button.Click += btnToggleProgress_Clicked;
				button.Enabled = true;
				button.Dispose();
			}
			else
			{
				var button = view.FindViewById<Button>(Resource.Id.btnToggleProgress);
				button.Click -= btnToggleProgress_Clicked;
				button.Enabled = false;
				button.Dispose();
			}

			var button2 = view.FindViewById<Button>(Resource.Id.btnChangeTitle);
			//shows one way of signifying that something has to change in the tabs
			//using a public event on the fragment - getting this right is tricky, though,
			//as you have to ensure you remain subscribed during orientation changes and
			//fragment recycling.

			button2.Click -= btnChangeTitle_Clicked;
			button2.Click += btnChangeTitle_Clicked;
			button2.Dispose();

			v.Text = string.Format("CARD {0}", _position + 1);
			v.Dispose();

			return view;
		}

		public override void OnSaveInstanceState(Android.OS.Bundle outState)
		{
			base.OnSaveInstanceState(outState);
			outState.PutInt("position", _position);
			outState.PutBoolean("enableProgressButton", _enableProgressButton);
		}

		public void btnChangeTitle_Clicked(object sender, EventArgs e)
		{
			var evt = ChangeTitleRequested;
			if (evt != null)
				evt(this, _position);
		}

		public void btnToggleProgress_Clicked(object sender, EventArgs e)
		{
			//the static event fired by SharedState 
			SharedState.SetInProgress(_position, !SharedState.GetInProgress(_position));
		}
	}
}