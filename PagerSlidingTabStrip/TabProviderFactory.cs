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
using Android.Views;
using Android.Widget;

namespace PagerSlidingTabStrip
{
	/// <summary>
	/// Default tab provider factory class.
	///
	/// This one supports all the standard functionality: using the ITabProvider
	/// implementation from an adapter if it supports it; otherwise falling back to
	/// an IconTabProvider provider or TextTabProvider.
	/// 
	/// If you're looking to customise the behaviour, you can either inherit from this
	/// class, or you might want to look at <see cref="CustomTabProviderFactory"/>,
	/// which allows you to supply callbacks for a custom provider, the icon provider and
	/// the text provider.
	/// </summary>
	public class TabProviderFactory
	{
		/// <summary>
		/// Creates a tab provider to be used for the given adapter.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="adapter">The adapter.</param>
		/// <returns>
		/// Must return an ITabProvider instance.
		/// </returns>
		public virtual ITabProvider CreateTabProvider(Context context, Android.Support.V4.View.PagerAdapter adapter)
		{
			ITabProvider tabProvider = adapter as ITabProvider;

			if (tabProvider != null)
			{
				return tabProvider;
			}
			else
			{
				//if the adapter supports the IIconTabProvider interface, then create
				//an instance of the IconTabProvider, otherwise use the TextTabProvider.
				if (adapter is PagerSlidingTabStrip.IIconTabProvider)
					return CreateIconTabProvider(context, adapter);
				else
					return CreateTextTabProvider(context, adapter);
			}
		}

		public virtual ITabProvider CreateTextTabProvider(Context context, PagerAdapter adapter)
		{
			return new TextTabProvider(context, adapter);
		}

		public virtual ITabProvider CreateIconTabProvider(Context context, PagerAdapter adapter)
		{
			return new IconTabProvider(context, adapter);
		}
	}
}