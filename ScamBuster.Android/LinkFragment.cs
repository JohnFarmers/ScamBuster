using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScamBuster.Droid
{
    public class LinkFragment : AndroidX.Fragment.App.Fragment
    {
        View view;
        ListView LinkList;
        public static List<String> LinkListItems = new List<String>();
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            view = inflater.Inflate(Resource.Layout.Links_Fragment, container, false);
            LinkList = view.FindViewById<ListView>(Resource.Id.LinkListView);
            ArrayAdapter<String> arrayAdapter = new ArrayAdapter<String>(Context, Resource.Layout.List, Resource.Id.ListText, LinkListItems.ToArray());
            LinkList.SetAdapter(arrayAdapter);
            return view;
        }
    }
}