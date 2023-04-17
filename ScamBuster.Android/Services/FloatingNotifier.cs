using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Threading.Tasks;

namespace ScamBuster.Droid.Services
{
    [Service]
    public class FloatingNotifier : Service, View.IOnTouchListener
    {
        public static FloatingNotifier instance;
        public View floatingView;
        private IWindowManager windowManager;
        private WindowManagerLayoutParams layoutParams;
        private int initialX;
        private int initialY;
        private float initialTouchX;
        private float initialTouchY;
        private View textContainer;
        private TextView textView;
        private const int notifyDuration = 3000;
        private int clickCount;


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
                case MotionEventActions.Up:
					if (clickCount < 1)
					{
						TimeSpan tt = new TimeSpan(0, 0, 0, 0, 500);
						Xamarin.Forms.Device.StartTimer(tt, HandleDoubleTap);
					}
					clickCount++;
					return true;
            }
            return false;
        }

        private bool HandleDoubleTap()
        {
            if (clickCount > 1)
                SetVisibility(ViewStates.Invisible);
			clickCount = 0;
			return false;
		}

        public void ShowCheckingLink(bool show)
        {
            SetVisibility(ViewStates.Visible);
			textContainer.Visibility = show ? ViewStates.Visible : ViewStates.Invisible;
            textView.Text = show ? Resources.GetString(Resource.String.checking_url) : string.Empty;
		}

        public async void NotifyDangerLevel(double percent)
		{
			SetVisibility(ViewStates.Visible);
			textContainer.Visibility = ViewStates.Visible;
            textView.Text = percent >= 50 ? string.Concat(Resources.GetString(Resource.String.danger_text), " ", percent, Resources.GetString(Resource.String.danger_level)) : string.Concat(Resources.GetString(Resource.String.safe_text), " ", percent, Resources.GetString(Resource.String.danger_level), Resources.GetString(Resource.String.stay_cautious));
            await Task.Delay(notifyDuration);
            textContainer.Visibility = ViewStates.Invisible;
            textView.Text = string.Empty;
        }

		public async void NotifyDangerURL()
		{
			SetVisibility(ViewStates.Visible);
			textContainer.Visibility = ViewStates.Visible;
			textView.Text = Resources.GetString(Resource.String.danger_url);
			await Task.Delay(notifyDuration);
			textContainer.Visibility = ViewStates.Invisible;
			textView.Text = string.Empty;
		}

        public async void NotifyPhoneNumberSafety(bool safe)
        {
			SetVisibility(ViewStates.Visible);
			textContainer.Visibility = ViewStates.Visible;
            textView.Text = safe ? Resources.GetString(Resource.String.safe_number) : Resources.GetString(Resource.String.danger_number);
			await Task.Delay(notifyDuration);
            textContainer.Visibility = ViewStates.Invisible;
            textView.Text = string.Empty;
		}

		public async void Notify(object text)
        {
			SetVisibility(ViewStates.Visible);
			textContainer.Visibility = ViewStates.Visible;
            textView.Text = text.ToString();
            await Task.Delay(3000);
            textContainer.Visibility = ViewStates.Invisible;
            textView.Text = string.Empty;
		}

        public void SetVisibility(ViewStates viewState) => floatingView.Visibility = viewState;

        private void SetTouchListener()
        {
            var rootContainer = floatingView.FindViewById<RelativeLayout>(Resource.Id.root);
            rootContainer.SetOnTouchListener(this);
        }
    }
}