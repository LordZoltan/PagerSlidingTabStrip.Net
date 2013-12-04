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
	/// A TabProviderFactory that can be customised through callbacks, eliminating 
	/// the need to inherit a new class for basic 
	/// customisations.
	/// </summary>
	public class CustomTabProviderFactory : TabProviderFactory
	{
		/// <summary>
		/// Delegate type for the callback used to create a tab provider.
		/// 
		/// If a callback returns null, then the <see cref="CustomTabProviderFactory"/> will use
		/// it's own default behaviour.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="adapter"></param>
		/// <returns></returns>
		public delegate ITabProvider FactoryCallback(Context context, PagerAdapter adapter);

		private readonly FactoryCallback _customTabProviderCallback;

		/// <summary>
		/// If not null, then this is executed by the <see cref="CreateTabProvider"/> override 
		/// to create a tab provider for the given context and adapter using custom logic.
		/// 
		/// If this is null or returns null, then the standard behaviour applies, which is to
		/// see if the adapter itself implements the ITabProvider interface, and if not then
		/// to fallback to either an icon provider or text provider.
		/// </summary>
		public FactoryCallback CustomTabProviderCallback
		{
			get { return _customTabProviderCallback; }
		}

		private readonly FactoryCallback _textTabProviderCallback;

		/// <summary>
		/// If not null, then this is executed by the <see cref="CreateTextTabProvider"/> override
		/// to give you a chance to create a custom tab provider to be used for basic text tabs.
		/// 
		/// If the method returns null, then the base implementation of the method will be used,
		/// which simply creates an instance of <see cref="TextTabProvider"/>.
		/// 
		/// The most common reason you would supply this callback on construction is so you can customise
		/// the standard <see cref="TextTabProvider"/>.
		/// </summary>
		public FactoryCallback TextTabProviderCallback
		{
			get { return _textTabProviderCallback; }
		}

		private readonly FactoryCallback _iconTabProviderCallback;

		/// <summary>
		/// If not null, then is is executed by the <see cref="CreateIconTabProvider"/> override
		/// to give you a chance to create a custom tab provider to be used for icon tabs.
		/// 
		/// If the method returns null, then the base implementation of the method will be used,
		/// which simply creates and instance <see cref="IconTabProvider"/>.
		/// 
		/// The most common reason you would supply this callback on construction is so you can customise
		/// the standard <see cref="IconTabProvider"/>.
		/// </summary>
		public FactoryCallback IconTabProviderCallback
		{
			get { return _iconTabProviderCallback; }
		} 


		/// <summary>
		/// Initializes a new instance of the <see cref="CustomTabProviderFactory"/> class.
		/// </summary>
		/// <param name="customTabProviderCallback">If supplied, this callback is used before any other
		/// logic to give you the chance to create a particular <see cref="ITabProvider"/> for a given adapter.  If this
		/// returns null, then the factory will check the adapter for its own ITabProvider implementation.</param>
		/// <param name="textTabProviderCallback">If supplied, this callback will be used to create the provider to 
		/// be used for straightforward text tabs (e.g. a <see cref="TextTabProvider"/>).  This is used most
		/// commonly to change the text layout resource that's used for the text tab.</param>
		/// <param name="iconTabProviderCallback">If supplied, this callback will be used to create the provider
		/// to be used for Icon tabs (e.g. a <see cref="IconTabProvider"/>.</param>
		public CustomTabProviderFactory(FactoryCallback customTabProviderCallback = null,
			FactoryCallback textTabProviderCallback = null,
			FactoryCallback iconTabProviderCallback = null)
		{
			_customTabProviderCallback = customTabProviderCallback;
			_textTabProviderCallback = textTabProviderCallback;
			_iconTabProviderCallback = iconTabProviderCallback;
		}


		/// <summary>
		/// Overrides the base implementation to first check if a custom provider is to be created (with a 
		/// call to the <see cref="CustomTabProviderCallback"/>) and, if not, the base implementation is called.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="adapter">The adapter.</param>
		/// <returns>
		/// Must return an ITabProvider instance.
		/// </returns>
		public override ITabProvider CreateTabProvider(Context context, PagerAdapter adapter)
		{
			ITabProvider toReturn = null;
			if (CustomTabProviderCallback != null)
				toReturn = CustomTabProviderCallback(context, adapter);

			return toReturn ?? base.CreateTabProvider(context, adapter);
		}

		/// <summary>
		/// Overrides the base method to use <see cref="TextTabProviderCallback"/> if it is not null.
		/// 
		/// If it is null, or if the callback returns null, then the default implementation will be 
		/// used, which simply returns a new default instance of the <see cref="TextTabProvider"/>.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="adapter">The adapter.</param>
		/// <returns></returns>
		public override ITabProvider CreateTextTabProvider(Context context, PagerAdapter adapter)
		{
			ITabProvider toReturn = null;
			if (_textTabProviderCallback != null)
				toReturn = _textTabProviderCallback(context, adapter);
			return toReturn ?? base.CreateTextTabProvider(context, adapter);
		}

		/// <summary>
		/// Overrides the base method to use <see cref="IconTabProviderCallback"/> if it is not null.
		/// 
		/// If it is null, or if the callback returns null, then the default implementation will be
		/// used, which simply returns a new default instance of the <see cref="IconTabProvider"/>.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="adapter">The adapter.</param>
		/// <returns></returns>
		public override ITabProvider CreateIconTabProvider(Context context, PagerAdapter adapter)
		{
			ITabProvider toReturn = null;
			if (_iconTabProviderCallback != null)
				toReturn = _iconTabProviderCallback(context, adapter);
			return toReturn ?? base.CreateIconTabProvider(context, adapter);
		}
	}

}