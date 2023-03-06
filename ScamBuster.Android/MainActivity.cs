using System;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Android.Content;
using AndroidX.Core.App;
using Android.Provider;
using Android;
using ScamBuster.Droid.Services;

namespace ScamBuster.Droid
{
    [Activity(Label = "ScamBuster", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static int REQUEST_CODE = 9999;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            StartActivityForResult(new Intent(Settings.ActionManageOverlayPermission), REQUEST_CODE);
            StartService(new Intent(this, typeof(FloatingNotifier)));
            ActivityCompat.RequestPermissions(this, new string[] { "android.permission.READ_PHONE_STATE" }, 123);
            StartActivity(new Intent("android.settings.ACTION_NOTIFICATION_LISTENER_SETTINGS"));
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
			global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
			LoadApplication(new App());
		}

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            if (requestCode == 123 && grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                StartService(new Intent(this, typeof(PhoneCallListener)));
        }
    }
}