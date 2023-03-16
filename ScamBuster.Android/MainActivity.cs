using System;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Android.Content;
using AndroidX.Core.App;
using Android.Provider;
using ScamBuster.Droid.Services;
using Google.Android.Material.BottomNavigation;
using ScamBuster.Droid.Resources;

namespace ScamBuster.Droid
{
    [Activity(Label = "ScamBuster", Icon = "@drawable/icon_scambuster", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public const int notifierRequestCode = 1111;
        public const int nlServiceRequestCode = 2222;
        public const int phoneNumberRequestCode = 3333;
        public static BottomNavigationView bottomnavigation;
        public static bool isProtected = true;
        public static bool isNLservice = false;
        public static bool isPCL = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
			global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            InitializeService();
            InitalizeHomePage();
		}

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            if (requestCode == phoneNumberRequestCode && grantResults.Length > 0 && grantResults[0] == Permission.Granted)
            {
                StartService(new Intent(this, typeof(PhoneCallListener)));
                isPCL = true;
            }
            else isPCL = false;
            isProtected = isPCL && isNLservice;
            LoadFragment(Resource.Id.Home);
        }

		private void InitializeService()
		{
			isPCL = false;
			StartActivityForResult(new Intent(Settings.ActionManageOverlayPermission), notifierRequestCode);
			SetContentView(Resource.Layout.Main);
			StartService(new Intent(this, typeof(FloatingNotifier)));
			ActivityCompat.RequestPermissions(this, new string[] { "android.permission.READ_PHONE_STATE" }, phoneNumberRequestCode);
			StartActivityForResult(new Intent(Settings.ActionNotificationListenerSettings), nlServiceRequestCode);
			StartService(new Intent(this, typeof(NLService)));
		}

		private void InitalizeHomePage()
        {
			SetContentView(Resource.Layout.Main);
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
                case Resource.Id.report:
                    ReportFragment reportFragment = new ReportFragment();
                    frag.Replace(Resource.Id.content_frame, reportFragment);
                    Console.WriteLine("Changed Report!");
                    break;
                case Resource.Id.disclaimer:
                    DisclaimerFragment disclaimerFragment = new DisclaimerFragment();
                    frag.Replace(Resource.Id.content_frame, disclaimerFragment);
                    Console.WriteLine("disclaimer!"); 
                    break;
            }
            frag.Commit();
        }

    }
}