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