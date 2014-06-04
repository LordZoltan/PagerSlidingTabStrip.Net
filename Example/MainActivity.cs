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
				_count = SharedState.Count != 0 ? SharedState.Count : Titles.Length;
				_titles = new string[Titles.Length];
				Array.Copy(Titles, _titles, Titles.Length);
				if (_count != SharedState.Count)
					SharedState.Count = _count;
			}

			protected internal static readonly string[] Titles = { "Categories", "Home", "Top Paid", "Top Free", "Top Grossing", "Top New Paid",
																	"Top New Free", "Trending" };

			protected internal static readonly string[] Titles2 = Titles.Select(s => s + " (Alt)").ToArray();

			protected internal readonly string[] _titles;

			protected virtual SuperAwesomeCardFragment CreateAwesomeFragment(int position)
			{
				return new SuperAwesomeCardFragment();
			}

			public override Android.Support.V4.App.Fragment GetItem(int position)
			{
				Android.Util.Log.Info("MyPagerAdapter", string.Format("GetItem being called for position {0}", position));
				var toReturn = CreateAwesomeFragment(position);
				return toReturn;
			}

			public override Java.Lang.Object InstantiateItem(ViewGroup container, int position)
			{
				Android.Util.Log.Info("MyPagerAdapter", string.Format("InstantiateItem being called for position {0}", position));
				var result = base.InstantiateItem(container, position);
				SuperAwesomeCardFragment frag = result as SuperAwesomeCardFragment;
				if (frag != null)
				{
					Configure(frag, position);
					frag.ChangeTitleRequested += toReturn_ChangeTitleRequested;
				}
				return result;
			}

			protected virtual void Configure(SuperAwesomeCardFragment frag, int position)
			{
				frag.Configure(position, false);
			}

			void toReturn_ChangeTitleRequested(object sender, int e)
			{
				ChangeTitle(e);
			}

			private int _count;
			public override int Count
			{
				get { return _count; }
			}

			public override Java.Lang.ICharSequence GetPageTitleFormatted(int position)
			{
				return new Java.Lang.String(_titles[position]);
			}

			/// <summary>
			/// used to demonstrate how the control can respond to tabs being added and removed.
			/// </summary>
			/// <param name="count"></param>
			public void SetCount(int count)
			{
				if (count < 0 || count > Titles.Length)
					return;

				_count = count;
				SharedState.Count = count;
				NotifyDataSetChanged();
			}

			public virtual void ChangeTitle(int position)
			{
				if (_titles[position] == Titles[position])
				{
					_titles[position] = Titles2[position];
				}
				else
				{
					_titles[position] = Titles[position];
				}
				//this one has to do it this way because 
				NotifyDataSetChanged();
			}
		}

		/// <summary>
		/// This adapter also exposes SuperAwesomCardFragment, but this time with richer tabs, by implementing
		/// ITabProvider directly.
		/// The tab is a layout that contains both a TextView and a ProgressBar, with an additional button
		/// being available on the fragment to toggle the visibility of the progress bar.  This is handled
		/// with a combination of a static object (SharedState) that exposes an event, and a standard implementation
		/// of RequestTabUpdate.
		/// 
		/// The way that Fragments are created and configured in both these adapters is worth taking note of in 
		/// general application scenarios - by using a lazy-initialised approach through both GetItem and InstantiateItem
		/// you can help ensure that your fragments work correctly even as device orientation changes.
		/// 
		/// Note - a better way to implement this adapter would be to use the FragmentTabProviderPagerAdapterBase,
		/// however, because of the way that I have adapter switching here, I haven't been able to change it easily.
		/// 
		/// I still wanted MyPagerAdapter not to be an ITabProvider implementation to show how easy simple tabs are.
		/// </summary>
		public class MyPagerAdapter2 : MyPagerAdapter, PagerSlidingTabStrip.ITabProvider
		{
			private PagerSlidingTabStrip.TextTabProvider _textTabProvider;

			public MyPagerAdapter2(Context context, Android.Support.V4.App.FragmentManager SupportFragmentManager)
				: base(SupportFragmentManager)
			{
				//encapsulate a TextTabProvider for setting the text style in the textview.
				_textTabProvider = new PagerSlidingTabStrip.TextTabProvider(context, this);
				SharedState.InProgressChanged += SharedState_InProgressChanged;
			}

			void SharedState_InProgressChanged(object sender, int e)
			{
				RequestTabUpdate(e);
			}

			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);
				//have to unsubscribe from the static event otherwise it'll keep firing.
				if (disposing)
				{
					SharedState.InProgressChanged -= SharedState_InProgressChanged;
					_textTabProvider = null;
				}
			}

			protected override SuperAwesomeCardFragment CreateAwesomeFragment(int position)
			{
				//we rely on the override of Instantiate item to configure the fragment
				return new SuperAwesomeCardFragment();
			}

			protected override void Configure(SuperAwesomeCardFragment frag, int position)
			{
				frag.Configure(position, true);
			}

			public override Java.Lang.ICharSequence GetPageTitleFormatted(int position)
			{
				return new Java.Lang.String(_titles[position]);
			}

			public override void ChangeTitle(int position)
			{
				//notice here that we call OnTabupdateRequired at the end
				//instead of NotifyDataSetChanged (which the base class calls) 
				// - this can be done because we implement the ITabProvider interface
				// and yields a more efficient tab UI as the existing view is updated rather
				// than being thrown away then recreated.
				if (_titles[position] == Titles[position])
				{
					_titles[position] = Titles2[position];
				}
				else
				{
					_titles[position] = Titles[position];
				}
				OnTabUpdateRequired(position);
			}

			#region ITabProvider Members

			public event EventHandler<PagerSlidingTabStrip.TabUpdateEventArgs> TabUpdated;

			public event EventHandler<PagerSlidingTabStrip.TabUpdateEventArgs> TabUpdateRequired;

			public void RequestTabUpdate(int position, string hint = null)
			{
				OnTabUpdateRequired(position, hint);
			}

			public View GetTab(PagerSlidingTabStrip.PagerSlidingTabStrip owner, ViewGroup root, int position, View recycled = null)
			{
				//TODO: you intend to add events to the ITabProvider interface to 
				//fire if a tab is knowingly updated in such a way that will affect it's size
				//You also intend to add a public method on the TabStrip itself - either for one tab,
				//all tabs or most likely both of these. Doing for all will probably just mean calling
				//requestlayout, then invalidate

				//what we're saying here is that any view that's previously been inflated is fine to be re-used so long as it's re-bound
				if (recycled != null)
					return recycled;
				
				var inflater = (LayoutInflater)owner.Context.GetSystemService(LayoutInflaterService);
				var view = inflater.Inflate(Resource.Layout.custom_tab, root, false) as ViewGroup;

				return view;
			}

			public void UpdateTab(View view, PagerSlidingTabStrip.PagerSlidingTabStrip owner, int position, string hint = null)
			{
				ProgressBar bar = view.FindViewById<ProgressBar>(Resource.Id.tab_progress);
				TextView textView = view.FindViewById<TextView>(Resource.Id.tab_text);

				textView.Text = owner.TabTextAllCaps ? _titles[position].ToUpper() : _titles[position];

				if (SharedState.GetInProgress(position))
				{
					bar.Visibility = ViewStates.Visible;
				}
				else
				{
					bar.Visibility = ViewStates.Gone;
				}

				bar.Dispose();
				textView.Dispose();

				OnTabUpdated(position);
			}

			public void UpdateTabStyle(View view, PagerSlidingTabStrip.PagerSlidingTabStrip owner, int position)
			{
				TextView textView = view.FindViewById<TextView>(Resource.Id.tab_text);
				if (textView != null)
					_textTabProvider.UpdateTabStyle(textView, owner, position);
			}

			#endregion

			private void OnTabUpdated(int position)
			{
				var evt = TabUpdated;
				if (evt != null)
					evt(this, new PagerSlidingTabStrip.TabUpdateEventArgs(position));
			}

			private void OnTabUpdateRequired(int position, string hint = null)
			{
				var evt = TabUpdateRequired;
				if (evt != null)
					evt(this, new PagerSlidingTabStrip.TabUpdateEventArgs(position, hint));
			}
		}

		private Handler _handler = new Handler();
		private PagerSlidingTabStrip.PagerSlidingTabStrip _tabs;
		private ViewPager _pager;
		private MyPagerAdapter _adapter;
		private bool _useAdapter2;

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

			if (bundle != null)
			{
				_useAdapter2 = bundle.GetBoolean("useAdapter2", false);
				var colorInt = bundle.GetInt("color", -1);
				if(colorInt != -1)
					_currentColor = new Color(colorInt);
			}

			_drawableCallback = new DrawableCallback(this);
			_tabs = FindViewById<PagerSlidingTabStrip.PagerSlidingTabStrip>(Resource.Id.tabs);
			_pager = FindViewById<ViewPager>(Resource.Id.pager);
			
			int pageMargin = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 4, Resources.DisplayMetrics);
			_pager.PageMargin = pageMargin;
			//_pager.Adapter = _adapter;

			InitAdapter();

			//_tabs.SetViewPager(_pager);

			ChangeColor(_currentColor);
		}

		private void InitAdapter(){
			_pager.Adapter = null;
			var oldAdapter = _adapter;
			_adapter = _useAdapter2 ? new MyPagerAdapter2(this, SupportFragmentManager) : new MyPagerAdapter(SupportFragmentManager);
			_pager.Adapter = _adapter;
			_tabs.SetViewPager(_pager);
			//have to dispose it after we've set the view pager, otherwise an error occurs because we've dumped out
			//the Java Reference.
			if (oldAdapter != null)
			{
				oldAdapter.Dispose();
			}
		}

		public override bool OnPrepareOptionsMenu(IMenu menu)
		{
			base.OnPrepareOptionsMenu(menu);

			var item = menu.FindItem(Resource.Id.action_changeadapter);
			if (item != null)
			{
				if (_useAdapter2)
					item.SetTitle("Switch to simple adapter");
				else
					item.SetTitle("Switch to ITabProvider adapter");
			}

			return true;
		}

		public override bool OnCreateOptionsMenu(IMenu menu)
		{
			base.OnCreateOptionsMenu(menu);

			MenuInflater.Inflate(Resource.Menu.main, menu);
			return true;
		}

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			switch (item.ItemId)
			{
				case Resource.Id.action_changeadapter:
					{
						_useAdapter2 = !_useAdapter2;
						InitAdapter();
						_adapter.NotifyDataSetChanged();
						return true;
					}
				case Resource.Id.action_contact:
					{
						QuickContactFragment dialog = new QuickContactFragment();
						dialog.Show(SupportFragmentManager, "QuickContactFragment");
						return true;
					}
				case Resource.Id.action_settabsone:
					{
						_adapter.SetCount(1);
						return true;
					}
				case Resource.Id.action_settabstwo:
					{
						_adapter.SetCount(2);
						return true;
					}
				case Resource.Id.action_settabsthree:
					{
						_adapter.SetCount(3);
						return true;
					}
				case Resource.Id.action_settabsfull:
					{
						_adapter.SetCount(MyPagerAdapter.Titles.Length);
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
			outState.PutBoolean("useAdapter2", _useAdapter2);
		}

		protected override void OnRestoreInstanceState(Bundle savedInstanceState)
		{
			if (savedInstanceState == null)
			{
				base.OnRestoreInstanceState(savedInstanceState);
				return;
			}

			base.OnRestoreInstanceState(savedInstanceState);
			_currentColor = new Color(savedInstanceState.GetInt("color"));
			_useAdapter2 = savedInstanceState.GetBoolean("useAdapter2", false);
		}
	}
}