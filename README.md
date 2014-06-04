PagerSlidingTabStrip.Net
========================

*Want a full overview of the control, why it exists, and how to customise it more? [Go to the Wiki!](https://github.com/LordZoltan/PagerSlidingTabStrip.Net/wiki)*

An enhanced tab strip for view pagers, offering control over tab background, highlight colour, divider colour and
much more.  The tabs can scroll in their own right, and the indicator slides smoothly between tabs as pages are changed,
in the same way as is seen in the Play Store(tm) version 4.4.22.

This is a port of the Java project of (almost) the same name by Andreas Stütz [here][1].

The example application is, also, a straight port as well - showing the same features (now with some additions).

Using Nuget?
------------

This project is available as a ready-rolled Nuget package (for Android 2.2+ projects) as [PagerSlidingTabStrip.Net on Nuget.Org](https://www.nuget.org/packages/PagerSlidingTabStrip.Net/).

1.2.x release breaking change
-----------------------------

James Ottoway very kindly followed up [Issue #1](https://github.com/LordZoltan/PagerSlidingTabStrip.Net/issues/1), which involved changing a couple of the attributes supported
by the control.  In the process the project was changed to reference the newer Xamarin.Android.Support.v4 reference library (via nuget) rather than the old Mono.Android.Support.v4
reference library.

These are both breaking changes - the first is simply a case of renaming attributes in your layout files.  The second might not hit you at all, but is a
concern if you are using another binary which references the older Mono.Android.Support.v4 library.  You'll either need a different version of that library
that references the newer support library wrapper, or get hold of the source for the offending binary and build it yourself to use the correct one.

Example
-------

See Andreas' demo, available on the Play Store (see [the project page][1]), for an accurate representation of what you can achieve with this library.

The Example project in the `sample/` folder builds an app that's functionally the same:

***Note - since these screenshots were done, the app has been updated with an extra set of menu options that demonstrate
how to change the number of tabs dynamically***

![Screenshot 1](https://lh3.googleusercontent.com/-FD9ojqMXcXQ/UouTWmzifII/AAAAAAAAAJw/8Ay-30fethQ/w306-h544-no/psts1.jpg)
&nbsp;![Screenshot 2](https://lh5.googleusercontent.com/-NIUMGzKWNaY/UouTW5JLOnI/AAAAAAAAAJQ/dbBgmlAOui4/w306-h544-no/psts2.jpg)

And showing icons instead of text:

![Screenshot 3](https://lh6.googleusercontent.com/-vSntr39cEF0/UouY1r8ViBI/AAAAAAAAAKA/8Igj2dzuerE/w306-h544-no/psts3.jpg)

[1]:https://github.com/astuetz/PagerSlidingTabStrip

Usage
=====

 1. Include the `PagerSlidingTabStrip` in your view.  this should usually be placed adjacent to the `ViewPager` it represents:

		<pagerslidingtabstrip.PagerSlidingTabStrip
			android:id="@+id/tabs"
			android:layout_width="match_parent"
			android:layout_height="48dip" />
 
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

With contributions from
=======================

 * [James Ottoway](https://github.com/jamesottaway)

*Many thanks!*

Port Notes
==========

I've tried where possible to translate Java paradigms to .Net paradigms in a similar way to Xamarin's own methodology.  However,
my main purpose was to get it working, so there might be some things I've missed.

I developed this port specifically for another project I was working on at the time - and as a result I have added features to this library
that were not present in the original.

Originally developed by
=======================

 * Andreas Stuetz - <andreas.stuetz@gmail.com>
