using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Telephony;
using Android.Views;
using Android.Widget;
using CsvHelper.Configuration;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Android.Content.Res;
using ScamBuster.Droid.Resources;

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
            var callDetector = new PhoneCallDetector(Assets, Resources);
            var tm = (TelephonyManager)base.GetSystemService(TelephonyService);
            tm.Listen(callDetector, PhoneStateListenerFlags.CallState);
            return StartCommandResult.Sticky;
        }

        public class PhoneCallDetector : PhoneStateListener
        {
            public readonly ScammerPhoneNumber[] scamNumbers;
            private readonly Android.Content.Res.Resources resources;

			public PhoneCallDetector(AssetManager asset, Android.Content.Res.Resources resources)
            {
				CsvConfiguration configuration = new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = false };
				scamNumbers = new CsvReader(new StreamReader(asset.Open("ScamPhoneNumber.csv")), configuration).GetRecords<ScammerPhoneNumber>().ToArray();
                this.resources = resources;
            }

            public override void OnCallStateChanged(CallState state, string incomingNumber)
            {
                base.OnCallStateChanged(state, incomingNumber);
				NLService.instance.phoneRinging = state == CallState.Ringing;
				if (state == CallState.Ringing)
                {
                    foreach (ScammerPhoneNumber number in scamNumbers)
                    {
                        if (incomingNumber.ToString() == number.Number)
                        {
                            FloatingNotifier.instance?.NotifyPhoneNumberSafety(false);
                            PhoneFragment.PhoneListItems.Add(string.Concat(incomingNumber.ToString(), "(", resources.GetString(Resource.String.danger), ")"));
                            return;
                        }
                    }
                    FloatingNotifier.instance?.NotifyPhoneNumberSafety(true);
                    PhoneFragment.PhoneListItems.Add(string.Concat(incomingNumber.ToString(), "(", resources.GetString(Resource.String.safe), ")"));
                }
            }
        }

        public class ScammerPhoneNumber { public string Number { get; set; } }
    }
}