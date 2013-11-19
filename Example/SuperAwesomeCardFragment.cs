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

namespace Example
{
	public class SuperAwesomeCardFragment : Android.Support.V4.App.Fragment
	{
		private int _position;

		public SuperAwesomeCardFragment()
		{

		}

		public SuperAwesomeCardFragment(int position)
		{
			_position = position;
		}

		public override void OnCreate(Android.OS.Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			if (savedInstanceState != null)
			{
				_position = savedInstanceState.GetInt("position", 0);
			}
		}

		public override Android.Views.View OnCreateView(Android.Views.LayoutInflater inflater, Android.Views.ViewGroup container, Android.OS.Bundle savedInstanceState)
		{
			ViewGroup.LayoutParams layoutParams = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
			FrameLayout fl = new FrameLayout(Activity);
			FrameLayout.LayoutParams vLayoutParams = new FrameLayout.LayoutParams(layoutParams);
			fl.LayoutParameters = layoutParams;
			int margin = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 8, Resources.DisplayMetrics);
			vLayoutParams.SetMargins(margin, margin, margin, margin);
			TextView v = new TextView(Activity);
			v.LayoutParameters = vLayoutParams;
			v.Gravity = GravityFlags.Center;
			v.SetBackgroundResource(Resource.Drawable.background_card);
			v.Text = string.Format("CARD {0}", _position + 1);
			fl.AddView(v);
			return fl;
		}

		public override void OnSaveInstanceState(Android.OS.Bundle outState)
		{
			base.OnSaveInstanceState(outState);
			outState.PutInt("position", _position);
		}
	}
}