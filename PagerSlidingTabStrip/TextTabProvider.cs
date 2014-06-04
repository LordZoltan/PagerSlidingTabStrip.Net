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
using Android.Support.V4.View;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace PagerSlidingTabStrip
{
	/// <summary>
	/// A standard implementation of the <see cref="ITabProvider"/> interface that provides a simple
	/// TextView instance for each tab.
	/// </summary>
	public class TextTabProvider : TabProviderBase
	{
		//resource ID of the layout to be used for the textview for the tab.
		private readonly int _textTabResourceID;

		private readonly Context _context;
		/// <summary>
		/// Gets the context for resources etc.
		/// </summary>
		protected Context Context
		{
			get { return _context; }
		}

		private readonly PagerAdapter _adapter;

		/// <summary>
		/// Gets the adapter.
		/// </summary>
		/// <value>
		/// The adapter.
		/// </value>
		protected PagerAdapter Adapter
		{
			get { return _adapter; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TextTabProvider"/> class.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="adapter">The adapter.</param>
		/// <param name="textTabResourceID">The ID of the layout resource to be used to inflate the text view.
		/// If not supplied or equal to null, then the built in resource pagerslidingtabstrip_texttab will be used.</param>
		public TextTabProvider(Context context, PagerAdapter adapter, int? textTabResourceID = null)
		{
			_context = context;
			_adapter = adapter;
			_textTabResourceID = textTabResourceID ?? Resource.Layout.pagerslidingtabstrip_texttab;
		}

		public override View GetTab(PagerSlidingTabStrip owner, ViewGroup root, int position, View recycled = null)
		{
			TextView tRecycled = recycled as TextView;
			if (tRecycled != null)
			{
				//TODO: should this method reset the gravity and singleline flag on the recycled view?
				//after all, there's no guarantee that the recycled view was originally constructed by this
				//provider.

				return recycled;
			}
			//var p = new FrameLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
			//p.Gravity = GravityFlags.Center;
			LayoutInflater inflater = (LayoutInflater)Context.GetSystemService(Context.LayoutInflaterService);
			TextView tab = (TextView)inflater.Inflate(_textTabResourceID, root, false);// new TextView(Context);
			return tab;
		}

		public override void UpdateTab(View view, PagerSlidingTabStrip owner, int position, string hint = null)
		{
			TextView v = view as TextView;
			if (v == null)
				return;

			var s = _adapter.GetPageTitle(position);
			if (owner.TabTextAllCaps)
				s = (s ?? "").ToUpper();
			v.SetText(s, TextView.BufferType.Normal);
			OnTabUpdated(position);
		}

		public override void UpdateTabStyle(View view, PagerSlidingTabStrip owner, int position)
		{
			TextView v = view as TextView;
			v.SetTextSize(ComplexUnitType.Px, owner.TextSize);
			v.SetTypeface(owner.Typeface, owner.TypefaceStyle);
			v.SetTextColor(owner.TextColor);
		}
	}
}