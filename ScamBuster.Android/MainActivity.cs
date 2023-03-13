﻿using System;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Android.Content;
using AndroidX.Core.App;
using Android.Provider;
using Android;
using ScamBuster.Droid.Services;
using Google.Android.Material.BottomNavigation;
using ScamBuster.Droid.Resources;
using Android.Widget;
using Android.Views;
using AndroidX.Fragment.App;

namespace ScamBuster.Droid
{
    [Activity(Label = "ScamBuster", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static int REQUEST_CODE = 9999;
        public static BottomNavigationView bottomnavigation;
        public static bool isProtected = true;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
			global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            //LoadApplication(new App());
            InitializeService();
            InitalizeHomePage();
		}

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            if (requestCode == 123 && grantResults.Length > 0 && grantResults[0] == Permission.Granted)
            {
                StartService(new Intent(this, typeof(PhoneCallListener)));
                isProtected = true;
            }
            else isProtected = false;
        }

        private void InitializeService()
        {
			StartActivityForResult(new Intent(Settings.ActionManageOverlayPermission), REQUEST_CODE);
			SetContentView(Resource.Layout.Main);
			StartService(new Intent(this, typeof(FloatingNotifier)));
			ActivityCompat.RequestPermissions(this, new string[] { "android.permission.READ_PHONE_STATE" }, 123);
			StartActivity(new Intent("android.settings.ACTION_NOTIFICATION_LISTENER_SETTINGS"));
		}

        private void InitalizeHomePage()
        {
			bottomnavigation = (BottomNavigationView)FindViewById(Resource.Id.bottomNavigationView1);
			bottomnavigation.NavigationItemSelected += NavigationItemSelected;
			LoadFragment(Resource.Id.Home);
		}

        public void NavigationItemSelected(object sender, BottomNavigationView.NavigationItemSelectedEventArgs e)
        {
            LoadFragment(e.Item.ItemId);
        }

        public void LoadFragment(int id)
        {
            var frag = SupportFragmentManager.BeginTransaction();
            switch (id)
            {
                case Resource.Id.Home:
                    HomeFragment homeFragment = new HomeFragment();
                    frag.Replace(Resource.Id.content_frame, homeFragment);
                    Console.WriteLine("Changed Home!");
                    break;
                case Resource.Id.detection:
                    DetectionFragment detectionFragment = new DetectionFragment();
                    frag.Replace(Resource.Id.content_frame, detectionFragment);
                    Console.WriteLine("Changed Detection!");
                    break;
            }
            frag.Commit();
        }

    }
}