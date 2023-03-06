using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Telephony;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScamBuster.Droid.Services
{
    [Service]
	public class PhoneCallListener : Service
	{
        public override void OnCreate()
        {
            base.OnCreate();
            System.Diagnostics.Debug.WriteLine("Phone call listener service initialized!");
        }

        public override IBinder OnBind(Intent intent)
        {
            throw new NotImplementedException();
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            base.OnStartCommand(intent, flags, startId);
            var callDetector = new PhoneCallDetector();
            var tm = (TelephonyManager)base.GetSystemService(TelephonyService);
            tm.Listen(callDetector, PhoneStateListenerFlags.CallState);
            return StartCommandResult.Sticky;
        }

        public class PhoneCallDetector : PhoneStateListener
        {
            public override void OnCallStateChanged(CallState state, string incomingNumber)
            {
                base.OnCallStateChanged(state, incomingNumber);
                if (state == CallState.Ringing)
                {
                    FloatingNotifier.instance.Notify(incomingNumber);
                }
            }
        }

        private class ScammerPhoneNumber { public string Number { get; set; } }
    }
}