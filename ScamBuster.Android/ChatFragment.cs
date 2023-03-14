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
    public class ChatFragment : AndroidX.Fragment.App.Fragment
    {
        View view;
        ListView ChatList;
        public static List<String> ChatListItems = new List<String>();
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            view = inflater.Inflate(Resource.Layout.Chats_Fragment, container, false);
            ChatListItems.Add("Chat");
            ChatList = view.FindViewById<ListView>(Resource.Id.ChatlistView1);
            ArrayAdapter<String> arrayAdapter = new ArrayAdapter<String>(Context, Resource.Layout.List, Resource.Id.ListText, ChatListItems.ToArray());
            ChatList.SetAdapter(arrayAdapter);
            return view;
        }
    }
}