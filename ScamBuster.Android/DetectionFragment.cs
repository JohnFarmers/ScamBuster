using Android.OS;
using Android.Views;
using AndroidX.ViewPager.Widget;
using System;
using Google.Android.Material.Tabs;
using Java.IO;

namespace ScamBuster.Droid.Resources
{
    public class DetectionFragment : AndroidX.Fragment.App.Fragment
    {
        View view;
        ViewPager viewPager;
        TabLayout tabLayout;
        ChatFragment chatFragment = new ChatFragment();
        PhoneFragment phoneFragment = new PhoneFragment();
        LinkFragment  linkFragment = new LinkFragment();
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            view = inflater.Inflate(Resource.Layout.Detection_Layout, container, false);
            SetUpViewPager();
            return view;
        }
        private void SetUpViewPager()
        {
            ViewPagerAdapter viewPagerAdapter = new ViewPagerAdapter(ChildFragmentManager);
            tabLayout = view.FindViewById<TabLayout>(Resource.Id.tabLayout1);
            viewPager = view.FindViewById<ViewPager>(Resource.Id.viewPager1);
            viewPagerAdapter.AddFragment(chatFragment,"Chats");
            viewPagerAdapter.AddFragment(phoneFragment,"PhoneNumbers");
            viewPagerAdapter.AddFragment(linkFragment,"Links");
            viewPager.OffscreenPageLimit = 3;
            viewPager.Adapter = viewPagerAdapter;
            tabLayout.SetupWithViewPager(viewPager);
        } 
    }
}