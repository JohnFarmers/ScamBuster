using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScamBuster.Droid.Services
{
    [Service]
    public class FloatingNotifier : Service, View.IOnTouchListener
    {
        public static FloatingNotifier instance;
        private IWindowManager windowManager;
        private WindowManagerLayoutParams layoutParams;
        private View floatingView;
        private int initialX;
        private int initialY;
        private float initialTouchX;
        private float initialTouchY;
        private View textContainer;
        private TextView textView;
        private const int notifyDuration = 3000;

        public override void OnCreate()
        {
            base.OnCreate();
            instance = this;
            floatingView = LayoutInflater.From(this).Inflate(Resource.Layout.FloatingNotifier, null);
            layoutParams = new WindowManagerLayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent, WindowManagerTypes.ApplicationOverlay, WindowManagerFlags.NotFocusable, Format.Translucent)
            { Gravity = GravityFlags.Left | GravityFlags.Top };
            windowManager = GetSystemService(WindowService).JavaCast<IWindowManager>();
            windowManager.AddView(floatingView, layoutParams);
            SetTouchListener();
            textContainer = floatingView.FindViewById(Resource.Id.flyout);
            textView = (TextView)textContainer.FindViewById(Resource.Id.notifiedText);
            System.Diagnostics.Debug.WriteLine("FloatingNotifier Initialized!");
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (floatingView != null)
                windowManager.RemoveView(floatingView);
            if (instance == this)
                instance = null;
        }

		public bool OnTouch(View view, MotionEvent motion)
		{
            switch (motion.Action)
            {
                case MotionEventActions.Down:
                    initialX = layoutParams.X;
                    initialY = layoutParams.Y;
                    initialTouchX = motion.RawX;
                    initialTouchY = motion.RawY;
                    return true;
                case MotionEventActions.Move:
                    layoutParams.X = initialX + (int)(motion.RawX - initialTouchX);
                    layoutParams.Y = initialY + (int)(motion.RawY - initialTouchY);
                    windowManager.UpdateViewLayout(floatingView, layoutParams);
                    return true;
            }
            return false;
        }

        public void ShowCheckingLink(bool show)
        {
			textContainer.Visibility = show ? ViewStates.Visible : ViewStates.Invisible;
            textView.Text = show ? "Checking URL safety..." : string.Empty;
		}

        public async void NotifyDangerLevel(double percent)
		{
            textContainer.Visibility = ViewStates.Visible;
            textView.Text = percent >= 50 ? $"BE CAREFUL! The recent message has {percent}% danger level!" : $"SAFE! The recent message has {percent}% danger level, but ALWAY STAY CAUTIOUS!";
            await Task.Delay(notifyDuration);
            textContainer.Visibility = ViewStates.Invisible;
            textView.Text = string.Empty;
        }

		public async void NotifyDangerURL()
		{
			textContainer.Visibility = ViewStates.Visible;
			textView.Text = "BE CAREFUL! The recent message contains DANGEROUS URL!";
			await Task.Delay(notifyDuration);
			textContainer.Visibility = ViewStates.Invisible;
			textView.Text = string.Empty;
		}

        public async void NotifyPhoneNumberSafety(bool safe)
        {
            textContainer.Visibility = ViewStates.Visible;
            textView.Text = safe ? "SAFE! The incoming number does not have any record of being dangerous, but ALWAY STAY CAUTIOUS!" : "BE CAREFUL! The incoming number has a record of being DANGER!";
            await Task.Delay(notifyDuration);
            textContainer.Visibility = ViewStates.Invisible;
            textView.Text = string.Empty;
        }

		public async void Notify(object text)
        {
            textContainer.Visibility = ViewStates.Visible;
            textView.Text = text.ToString();
            await Task.Delay(3000);
            textContainer.Visibility = ViewStates.Invisible;
            textView.Text = string.Empty;
        }

        private void SetTouchListener()
        {
            var rootContainer = floatingView.FindViewById<RelativeLayout>(Resource.Id.root);
            rootContainer.SetOnTouchListener(this);
        }
    }
}