using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using Android.Service.Notification;
using AndroidX.Core.App;
using CsvHelper;
using System.IO;
using CsvHelper.Configuration;
using System.Globalization;
using Cloudmersive.APIClient.NETCore.Validate.Api;
using Cloudmersive.APIClient.NETCore.Validate.Client;
using Cloudmersive.APIClient.NETCore.Validate.Model;
using System.Text.RegularExpressions;
using System.Threading;
using Xamarin.Forms;

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
		private ScamText[] scamTexts;
		private ScammerPhoneNumber[] scamNumbers;
		private readonly Regex urlExtractRegex = new Regex("(http(s)?://)?([\\w-]+\\.)+[\\w-]+[.com]+(/[/?%&=]*)?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private readonly List<UrlSafetyCheckResponseFull> urlSafetyResponses = new List<UrlSafetyCheckResponseFull>();
		private readonly List<PhishingCheckResponse> phishingResponses = new List<PhishingCheckResponse>();
		private double recentDangerLevel = 0;
		private string recentText = string.Empty;
		private readonly DomainApi domainApi = new DomainApi();

		public override void OnCreate()
        {
            base.OnCreate();
            instance = this;
            CsvConfiguration configuration = new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = false };
			scamTexts = new CsvReader(new StreamReader(Assets.Open("Scam.csv")), configuration).GetRecords<ScamText>().ToArray();
			scamNumbers = new CsvReader(new StreamReader(Assets.Open("ScamPhoneNumber.csv")), configuration).GetRecords<ScammerPhoneNumber>().ToArray();
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
						ChatFragment.ChatListItems.Add(string.Concat(recentText, " (", recentDangerLevel, "% danger)"));
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
			FloatingNotifier.instance?.Notify("Ready!");
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

		public override void OnNotificationPosted(StatusBarNotification sbn)
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
			double susLevel = 0;
			foreach (ScamText scam in scamTexts)
			{
				double _susLevel = CalculateSimilarity(text.ToLower(), scam.Text.ToLower());
				susLevel = _susLevel >= susLevel ? _susLevel : susLevel;
			}
			double dangerPrecent = Math.Round(susLevel *= 100);
			if (checkURL)
			{
				recentDangerLevel = dangerPrecent;
				recentText = text;
			}
			else
			{
				FloatingNotifier.instance?.NotifyDangerLevel(dangerPrecent);
				ChatFragment.ChatListItems.Add(string.Concat(text, " (", dangerPrecent, "% danger)"));
			}
		}

		public override void OnNotificationRemoved(StatusBarNotification sbn)
        {
            base.OnNotificationRemoved(sbn);
        }

        private int ComputeLevenshteinDistance(string source, string target)
        {
            if ((source == null) || (target == null)) return 0;
            if ((source.Length == 0) || (target.Length == 0)) return 0;
            if (source == target) return source.Length;

            int sourceWordCount = source.Length;
            int targetWordCount = target.Length;

            if (sourceWordCount == 0)
                return targetWordCount;

            if (targetWordCount == 0)
                return sourceWordCount;

            int[,] distance = new int[sourceWordCount + 1, targetWordCount + 1];

            for (int i = 0; i <= sourceWordCount; distance[i, 0] = i++) ;
            for (int j = 0; j <= targetWordCount; distance[0, j] = j++) ;

            for (int i = 1; i <= sourceWordCount; i++)
            {
                for (int j = 1; j <= targetWordCount; j++)
                {
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;
                    distance[i, j] = Math.Min(Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1), distance[i - 1, j - 1] + cost);
                }
            }

            return distance[sourceWordCount, targetWordCount];
        }

        private double CalculateSimilarity(string source, string target)
        {
            if ((source == null) || (target == null)) return 0.0;
            if ((source.Length == 0) || (target.Length == 0)) return 0.0;
            if (source == target) return 1.0;
            int stepsToSame = ComputeLevenshteinDistance(source, target);
            return 1.0 - ((double)stepsToSame / (double)Math.Max(source.Length, target.Length));
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