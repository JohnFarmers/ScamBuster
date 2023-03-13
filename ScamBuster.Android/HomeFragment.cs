using Android.OS;
using Android.Views;
using Android.Widget;
using System;

namespace ScamBuster.Droid
{
    public class HomeFragment : AndroidX.Fragment.App.Fragment
    {
        View view;
        ImageView imageView;
        TextView textView1, textView2;
        RelativeLayout relativeLayout;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            view = inflater.Inflate(Resource.Layout.Home_Layout, container, false);
            imageView = view.FindViewById<ImageView>(Resource.Id.imageView1);
            textView1 = view.FindViewById<TextView>(Resource.Id.textView1);
            textView2 = view.FindViewById<TextView>(Resource.Id.textView2);
            relativeLayout = view.FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);
            Console.WriteLine(MainActivity.isProtected);
            if(MainActivity.isProtected ==  false)
            {
                imageView.SetImageResource(Resource.Drawable.status_notready);
                textView1.SetText(Resource.String.notready);
                textView2.SetText(Resource.String.notready_desc);
                relativeLayout.SetBackgroundResource(Resource.Drawable.gradient_background_red);
                textView1.SetTextColor(Resources.GetColor(Resource.Color.DarkRed));
                textView2.SetTextColor(Resources.GetColor(Resource.Color.DarkRed));
            }
            else {
                imageView.SetImageResource(Resource.Drawable.status_ready);
                textView1.SetText(Resource.String.main);
                textView2.SetText(Resource.String.protected_desc);
                relativeLayout.SetBackgroundResource(Resource.Drawable.gradient_background_green);
                textView1.SetTextColor(Resources.GetColor(Resource.Color.BrightGreen));
                textView2.SetTextColor(Resources.GetColor(Resource.Color.BrightGreen));
            }
            return view;
        }
    }
}