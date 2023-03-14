using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using Java.Lang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScamBuster.Droid
{
    public class ViewPagerAdapter : FragmentPagerAdapter
    {
        List<AndroidX.Fragment.App.Fragment> fragments { get; set; }
        List<string> fragmentNames { get; set; }
        public ViewPagerAdapter(AndroidX.Fragment.App.FragmentManager fragmentManager) : base(fragmentManager)
        {
            fragments = new List<AndroidX.Fragment.App.Fragment>();
            fragmentNames = new List<string>();
        }
        public void AddFragment(AndroidX.Fragment.App.Fragment fragment,string name)
        {
            fragments.Add(fragment);
            fragmentNames.Add(name);
        }
        public override int Count
        {
            get
            {
                return fragments.Count;
            }
        }
        public override AndroidX.Fragment.App.Fragment GetItem(int position)
        {
            return fragments[position];
        }
        public override ICharSequence GetPageTitleFormatted(int position)
        {
            switch(position)
            {
                case 0:
                    return new Java.Lang.String("Calls");
                case 1:
                    return new Java.Lang.String("Phone Numbers");
                case 2:
                    return new Java.Lang.String("Links");
            }
            return base.GetPageTitleFormatted(position);
        }
    }
}