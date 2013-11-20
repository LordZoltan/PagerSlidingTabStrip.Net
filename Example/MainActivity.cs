using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Example
{
	[Activity(Label = "PagerSlidingTabStrip (.Net)", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : FragmentActivity
	{
		public class MyPagerAdapter : FragmentPagerAdapter
		{
			private Android.Support.V4.App.FragmentManager SupportFragmentManager;

			public MyPagerAdapter(Android.Support.V4.App.FragmentManager SupportFragmentManager)
				: base(SupportFragmentManager)
			{
				// TODO: Complete member initialization
				this.SupportFragmentManager = SupportFragmentManager;
			}

			private static readonly string[] Titles = { "Categories", "Home", "Top Paid", "Top Free", "Top Grossing", "Top New Paid",
																	"Top New Free", "Trending" };

			public override Android.Support.V4.App.Fragment GetItem(int position)
			{
				return new SuperAwesomeCardFragment(position);
			}

			public override int Count
			{
				get { return Titles.Length; }
			}

			public override Java.Lang.ICharSequence GetPageTitleFormatted(int position)
			{
				return new Java.Lang.String(Titles[position]);
			}
		}

		private Handler _handler = new Handler();
		private PagerSlidingTabStrip.PagerSlidingTabStrip _tabs;
		private ViewPager _pager;
		private MyPagerAdapter _adapter;

		private Drawable _oldBackground = null;
		private Color _currentColor = Color.Argb(0xff, 0x66, 0x66, 0x66);

		private class DrawableCallback : Java.Lang.Object, Drawable.ICallback
		{
			MainActivity _activity;

			public DrawableCallback(MainActivity activity)
			{
				_activity = activity;
			}

			#region ICallback Members

			public void InvalidateDrawable(Drawable who)
			{
				_activity.ActionBar.SetBackgroundDrawable(who);
			}

			public void ScheduleDrawable(Drawable who, Java.Lang.IRunnable what, long when)
			{
				_activity._handler.PostAtTime(what, when);
			}

			public void UnscheduleDrawable(Drawable who, Java.Lang.IRunnable what)
			{
				_activity._handler.RemoveCallbacks(what);
			}

			#endregion
		}

		private DrawableCallback _drawableCallback;

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			
			SetContentView(Resource.Layout.activity_main);
			_drawableCallback = new DrawableCallback(this);
			_tabs = FindViewById<PagerSlidingTabStrip.PagerSlidingTabStrip>(Resource.Id.tabs);
			_pager = FindViewById<ViewPager>(Resource.Id.pager);
			_adapter = new MyPagerAdapter(SupportFragmentManager);

			_pager.Adapter = _adapter;
			int pageMargin = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 4, Resources.DisplayMetrics);
			_pager.PageMargin = pageMargin;

			_tabs.SetViewPager(_pager);

			ChangeColor(_currentColor);
		}

		public override bool OnCreateOptionsMenu(IMenu menu)
		{
			MenuInflater.Inflate(Resource.Menu.main, menu);
			return true;
		}

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			switch (item.ItemId)
			{
				case Resource.Id.action_contact:
					{
						QuickContactFragment dialog = new QuickContactFragment();
						dialog.Show(SupportFragmentManager, "QuickContactFragment");
						return true;
					}
			}

			return base.OnOptionsItemSelected(item);
		}

		private void ChangeColor(Color newColor)
		{
			_tabs.IndicatorColor = newColor;

			// change ActionBar color just if an ActionBar is available
			if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Honeycomb)
			{

				Drawable colorDrawable = new ColorDrawable(newColor);
				Drawable bottomDrawable = Resources.GetDrawable(Resource.Drawable.actionbar_bottom);
				LayerDrawable ld = new LayerDrawable(new Drawable[] { colorDrawable, bottomDrawable });

				if (_oldBackground == null)
				{

					if ((int)Build.VERSION.SdkInt < 17)
					{
						ld.Callback = _drawableCallback;
					}
					else
					{
						ActionBar.SetBackgroundDrawable(ld);
					}
				}
				else
				{
					TransitionDrawable td = new TransitionDrawable(new Drawable[] { _oldBackground, ld });

					// workaround for broken ActionBarContainer drawable handling on
					// pre-API 17 builds
					// https://github.com/android/platform_frameworks_base/commit/a7cc06d82e45918c37429a59b14545c6a57db4e4
					if ((int)Build.VERSION.SdkInt < 17)
					{
						td.Callback = _drawableCallback;
					}
					else
					{
						ActionBar.SetBackgroundDrawable(td);
					}

					td.StartTransition(200);

				}

				_oldBackground = ld;

				// http://stackoverflow.com/questions/11002691/actionbar-setbackgrounddrawable-nulling-background-from-thread-handler
				ActionBar.SetDisplayShowTitleEnabled(false);
				ActionBar.SetDisplayShowTitleEnabled(true);
			}

			_currentColor = newColor;
		}

		[Java.Interop.Export]
		public void onColorClicked(View v)
		{
			Color color = Color.ParseColor(v.Tag.ToString());
			ChangeColor(color);
		}

		protected override void OnSaveInstanceState(Bundle outState)
		{
			base.OnSaveInstanceState(outState);
			outState.PutInt("color", _currentColor.ToArgb());
		}

		protected override void OnRestoreInstanceState(Bundle savedInstanceState)
		{
			base.OnRestoreInstanceState(savedInstanceState);
			_currentColor = new Color(savedInstanceState.GetInt("color"));
		}
	}
}