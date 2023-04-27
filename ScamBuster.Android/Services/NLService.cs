using Android.App;
using Android.Content;
using Android.OS;
using System;
using System.Collections.Generic;
using System.Linq;
using Android.Service.Notification;
using AndroidX.Core.App;
using System.IO;
using Cloudmersive.APIClient.NETCore.Validate.Api;
using Cloudmersive.APIClient.NETCore.Validate.Client;
using Cloudmersive.APIClient.NETCore.Validate.Model;
using System.Text.RegularExpressions;
using System.Threading;
using Xamarin.Forms;
using System.Net.Http;
using System.Threading.Tasks;

namespace ScamBuster.Droid.Services
{
    [Service(Name = "ScamBuster.NLService", Label = "ServiceName", Permission = "android.permission.BIND_NOTIFICATION_LISTENER_SERVICE")]
    [IntentFilter(new[] { "android.service.notification.NotificationListenerService" })]
    public class NLService : NotificationListenerService
    {
		public static NLService instance;
		public bool phoneRinging = false;
		private const string channelID = "ScamBuster";
		private const string packageName = "com.potatolab.scambuster";
		private const string androidPackageName = "android";
		private readonly Regex urlExtractRegex = new Regex("(http(s)?://)?([\\w-]+\\.)+[\\w-]+[.com]+(/[/?%&=]*)?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private readonly List<UrlSafetyCheckResponseFull> urlSafetyResponses = new List<UrlSafetyCheckResponseFull>();
		private readonly List<PhishingCheckResponse> phishingResponses = new List<PhishingCheckResponse>();
		private double recentDangerLevel = 0;
		private string recentText = string.Empty;
		private readonly DomainApi domainApi = new DomainApi();
		private readonly string wepApiUrl = "https://scambuster-scamtextclassificationapi.azurewebsites.net/";

		public override void OnCreate()
        {
            base.OnCreate();
            instance = this;
			Configuration.Default.AddApiKey("Apikey", "c09bee5d-213f-4965-978f-3a5eeb7bc927");
			Forms.Init(this, null);
			Device.StartTimer(TimeSpan.FromSeconds(3), () => {
				bool DangerUrl(bool[] results)
				{
					foreach (var cleanUrl in results)
						if (!cleanUrl)
							return true;
					return false;
				}
				if (phishingResponses.Count > 0 && urlSafetyResponses.Count > 0)
                {
					List<bool> results = new List<bool>();
                    urlSafetyResponses.ForEach(response => results.Add((bool)response.CleanURL));
					phishingResponses.ForEach(response => results.Add((bool)response.CleanURL));
					if (DangerUrl(results.ToArray()))
						FloatingNotifier.instance?.NotifyDangerURL();
					else
					{
						FloatingNotifier.instance?.NotifyDangerLevel(recentDangerLevel);
						ChatFragment.ChatListItems.Add(string.Concat(recentText, " (", recentDangerLevel, "% ", Resources.GetString(Resource.String.danger), ")"));
					}
					urlSafetyResponses.Clear();
					phishingResponses.Clear();
					recentDangerLevel = 0;
					recentText = string.Empty;
				}
                return true;
            });
			MainActivity.isNLservice = true;
			System.Diagnostics.Debug.WriteLine("Notification Listener Service Initialized!");
		}

		public override void OnListenerConnected()
		{
			MainActivity.isNLservice = true;
			FloatingNotifier.instance?.Notify(Resources.GetString(Resource.String.ready));
			base.OnListenerConnected();
		}

		public override void OnListenerDisconnected()
		{
			MainActivity.isNLservice = false;
			base.OnListenerDisconnected();
		}

		public override void OnDestroy()
        {
            base.OnDestroy();
            if(instance == this)
                instance = null;
        }

        public override IBinder OnBind(Intent intent)
        {
            return base.OnBind(intent);
        }

        public override bool OnUnbind(Intent intent)
        {
            return base.OnUnbind(intent);
        }

		public override async void OnNotificationPosted(StatusBarNotification sbn)
		{
			base.OnNotificationPosted(sbn);
			if (sbn.Notification.Extras == null || sbn.PackageName == packageName || sbn.PackageName == androidPackageName || phoneRinging)
				return;
			string text = sbn.Notification.Extras.GetCharSequence(Notification.ExtraText).ToString();
			FloatingNotifier.instance?.ShowCheckingLink(true);
			bool checkURL = false;
			foreach (string match in urlExtractRegex.Matches(text).Cast<Match>().Select(m => m.Value).ToArray())
			{
				LinkFragment.LinkListItems.Add(match);
				checkURL = true;
				new Thread(new ThreadStart(async delegate
				{
					UrlSafetyCheckResponseFull urlSafetyResponse = await domainApi.DomainSafetyCheckAsync(new UrlSafetyCheckRequestFull(match));
					PhishingCheckResponse phishingResponse = await domainApi.DomainPhishingCheckAsync(new PhishingCheckRequest(match));
					urlSafetyResponses.Add(urlSafetyResponse);
					phishingResponses.Add(phishingResponse);
				})).Start();
			}
			Result result = await CheckTextSafety(text);
			double dangerLevel = result.DangerLevel;
			if (checkURL)
			{
				recentDangerLevel = dangerLevel;
				recentText = text;
			}
			else
			{
				FloatingNotifier.instance?.NotifyDangerLevel(dangerLevel);
				ChatFragment.ChatListItems.Add(string.Concat(text, " (", dangerLevel, "% ", Resources.GetString(Resource.String.danger), ")"));
			}
		}

		private async Task<Result> CheckTextSafety(string text)
		{
			string parameters = "scamtextclassification?text=" + text;
			HttpClient client = new HttpClient { BaseAddress = new Uri(wepApiUrl) };
			HttpResponseMessage response = client.GetAsync(parameters).Result;
			client.Dispose();
			return response.IsSuccessStatusCode ? Newtonsoft.Json.JsonConvert.DeserializeObject<Result>(response.Content.ReadAsStringAsync().Result) : null;
		}

		public override void OnNotificationRemoved(StatusBarNotification sbn)
        {
            base.OnNotificationRemoved(sbn);
        }

        public void SendNotification(string sender, string message)
		{
            ((NotificationManager)GetSystemService(NotificationService)).CreateNotificationChannel(new NotificationChannel(channelID, channelID, NotificationImportance.Default));
            (GetSystemService(NotificationService) as NotificationManager)
                .Notify(0, new NotificationCompat.Builder(this, channelID)
                .SetContentIntent(PendingIntent.GetActivity(this, 0, new Intent(this, typeof(MainActivity)), PendingIntentFlags.OneShot))
                .SetContentTitle(sender)
                .SetContentText(message)
                .SetTicker(message)
                .SetSmallIcon(Resource.Drawable.icon_scambuster).Build());
        }

        private class ScamText { public string Text { get; set; } }
        
        private class ScammerPhoneNumber { public string Number { get; set; } }
    }
}