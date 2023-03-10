using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Util.Zip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScamBuster.Droid
{
    public class HomeFragment : AndroidX.Fragment.App.Fragment
    {
        View view;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            view = inflater.Inflate(Resource.Layout.Home_Layout, container, false);
            return view;
        }
    }
}