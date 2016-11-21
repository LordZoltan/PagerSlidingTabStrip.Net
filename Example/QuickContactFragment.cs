using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Example
{
	public class QuickContactFragment : Android.Support.V4.App.DialogFragment
	{
		private PagerSlidingTabStrip.PagerSlidingTabStrip _tabs;
		private ViewPager _pager;
		private ContactPagerAdapter _adapter;

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			if (Dialog != null)
			{
				Dialog.Window.RequestFeature(WindowFeatures.NoTitle);
				Dialog.Window.SetBackgroundDrawableResource(Android.Resource.Color.Transparent);
			}

			View root = inflater.Inflate(Resource.Layout.fragment_quick_contact, container, false);
			_tabs = root.FindViewById<PagerSlidingTabStrip.PagerSlidingTabStrip>(Resource.Id.tabs);
			_pager = root.FindViewById<ViewPager>(Resource.Id.pager);
			_adapter = new ContactPagerAdapter(Activity);

			_pager.Adapter = _adapter;
			_tabs.SetViewPager(_pager);
			return root;
		}

		public override void OnStart()
		{
			base.OnStart();

			if (Dialog != null)
			{
				int fullWidth = Dialog.Window.Attributes.Width;
				if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.HoneycombMr2)
				{
					Display display = Activity.WindowManager.DefaultDisplay;
					Point size = new Point();
					display.GetSize(size);
					fullWidth = size.X;
				}
				else
				{
					Display display = Activity.WindowManager.DefaultDisplay;
					fullWidth = display.Width;
				}

				int padding = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 24, Resources.DisplayMetrics);

				int w = fullWidth - padding;
				int h = Dialog.Window.Attributes.Height;

				Dialog.Window.SetLayout(w, h);
			}
		}

		public class ContactPagerAdapter : Android.Support.V4.View.PagerAdapter, PagerSlidingTabStrip.PagerSlidingTabStrip.IIconTabProvider
		{
			private int[] ICONS = { Resource.Drawable.ic_launcher_gplus, Resource.Drawable.ic_launcher_gmail,
				Resource.Drawable.ic_launcher_gmaps, Resource.Drawable.ic_launcher_chrome };

			private Activity _activity;

			public ContactPagerAdapter(Activity activity) {
				_activity = activity;
			}

			public override int Count
			{
				get { return ICONS.Length; }
			}

			#region IIconTabProvider Members

			public int GetPageIconResId(int position)
			{
				return ICONS[position];
			}

			#endregion

			public override Java.Lang.Object InstantiateItem(ViewGroup container, int position)
			{	TextView v = new TextView(_activity);
				v.SetBackgroundResource(Resource.Color.background_window);
				v.Text = string.Format("PAGE {0}", position + 1);
				int padding = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 16, _activity.Resources.DisplayMetrics);
				v.SetPadding(padding, padding, padding, padding);
				v.Gravity = GravityFlags.Center;
				container.AddView(v, 0);
				return v;
			}

			public override void DestroyItem(ViewGroup container, int position, Java.Lang.Object @object)
			{
				container.RemoveView((View)@object);
			}

			public override bool IsViewFromObject(View view, Java.Lang.Object @object)
			{
				return view == (View)@object;
			}
		}

	}
}