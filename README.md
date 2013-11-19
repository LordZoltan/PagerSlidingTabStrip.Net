PagerSlidingTabStrip.Net
========================

Port of Java project of the same name by Andreas Stütz [here][1].

The example application is, also, a straight port as well - showing the same features.

Using Nuget?
------------

This project is available as a ready-rolled Nuget package (for Android 2.2+ projects) as [PagerSlidingTabStrip.Net on Nuget.Org](https://www.nuget.org/packages/PagerSlidingTabStrip.Net/).

See Andreas' demo, available on the Play Store (see [the project page][1]), for an accurate representation of what you can achieve with this library.

The Example project in the `sample/` folder builds an app that's functionally the same:

![Screenshot 1](https://lh3.googleusercontent.com/-FD9ojqMXcXQ/UouTWmzifII/AAAAAAAAAJw/8Ay-30fethQ/w306-h544-no/psts1.jpg)
![Screenshot 2](https://lh5.googleusercontent.com/-NIUMGzKWNaY/UouTW5JLOnI/AAAAAAAAAJQ/dbBgmlAOui4/w306-h544-no/psts2.jpg)

And showing icons instead of text:

![Screenshot 3](https://lh6.googleusercontent.com/-vSntr39cEF0/UouY1r8ViBI/AAAAAAAAAKA/8Igj2dzuerE/w306-h544-no/psts3.jpg)

[1]:https://github.com/astuetz/PagerSlidingTabStrip

Usage
=====

 1. Include the `PagerSlidingTabStrip` in your view.  this should usually be placed adjacent to the `ViewPager` it represents:

		&lt;pagerslidingtabstrip.PagerSlidingTabStrip
			android:id="@+id/tabs"
			android:layout_width="match_parent"
			android:layout_height="48dip" /&gt;
 
 2. In your `OnCreate`/`OnCreateView`(fragment) bind the widget to the `ViewPager` (note this is taken from the Example).

		_tabs = FindViewById<PagerSlidingTabStrip.PagerSlidingTabStrip>(Resource.Id.tabs);
		_pager = FindViewById<ViewPager>(Resource.Id.pager);
		_adapter = new MyPagerAdapter(SupportFragmentManager);

		// Set the pager with an adapter:
		_pager.Adapter = _adapter;

		//Set the pager to the tabs control:
		_tabs.SetViewPager(_pager);

 3. *(Optional)* If you use the `PageScrolled`/`PageScrollStateChanged`/`PageSelected` events from your view pager, you should 
now subscribe instead to the same events on the `_tabs` object (it proxies the events from the pager).

Ported by
=========

 * Andras Zoltan (@RealLordZoltan)

I've tried where possible to translate Java paradigms to .Net paradigms in a similar way to Xamarin's own methodology.  However,
my main purpose was to get it working, so there might be some things I've missed.

I might add features to this library as required by another project that I'm working on - but only in a way that would enhance 
other apps of course.

Originally developed by
=======================

 * Andreas Stuetz - <andreas.stuetz@gmail.com>