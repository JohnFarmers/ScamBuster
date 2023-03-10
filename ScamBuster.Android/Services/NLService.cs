using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Service.Notification;
using AndroidX.Core.App;
using CsvHelper;
using System.IO;
using CsvHelper.Configuration;
using System.Globalization;
using System.Reflection;
using Xamarin.Essentials;
using Cloudmersive.APIClient.NETCore.Validate.Api;
using Cloudmersive.APIClient.NETCore.Validate.Client;
using Cloudmersive.APIClient.NETCore.Validate.Model;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;
using Xamarin.Forms;

namespace ScamBuster.Droid.Services
{
    [Service(Name = "ScamBuster.NLService", Label = "ServiceName", Permission = "android.permission.BIND_NOTIFICATION_LISTENER_SERVICE")]
    [IntentFilter(new[] { "android.service.notification.NotificationListenerService" })]
    public class NLService : NotificationListenerService
    {
        public static NLService instance;
        private const string channelID = "ScamBuster";
        private const string packageName = "com.potatolab.scambuster";
        private const string androidPackageName = "android";
        private ScamText[] scamTexts;
        private ScammerPhoneNumber[] scamNumbers;
        private readonly Regex linkParser = new Regex(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly List<UrlSafetyCheckResponseFull> responses = new List<UrlSafetyCheckResponseFull>();
        private double recentDangerLevel = 0;
        private bool checkingURL = false;

		public override void OnCreate()
        {
            base.OnCreate();
            instance = this;
            CsvConfiguration configuration = new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = false };
			scamTexts = new CsvReader(new StreamReader(Assets.Open("Scam.csv")), configuration).GetRecords<ScamText>().ToArray();
			scamNumbers = new CsvReader(new StreamReader(Assets.Open("ScamPhoneNumber.csv")), configuration).GetRecords<ScammerPhoneNumber>().ToArray();
			Configuration.Default.AddApiKey("Apikey", "06e4661e-e994-4083-8a3a-58c6c3f9fe31");
			Device.StartTimer(TimeSpan.FromSeconds(2), () => { 
                if(responses.Count > 0 && !checkingURL)
                {
                    checkingURL = true;
                    responses.ForEach(response => {
                        if (!(bool)response.CleanURL)
                            FloatingNotifier.instance.NotifiedURLSafety();
                        else
                            FloatingNotifier.instance.NotifiedDangerLevel(recentDangerLevel);
					});
                    responses.Clear();
                    recentDangerLevel = 0;
                    checkingURL = false;
				}
                return true; 
            });
			System.Diagnostics.Debug.WriteLine("Notification Listener Service Initialized!");
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
            if (sbn.Notification.Extras == null || sbn.PackageName == packageName || sbn.PackageName == androidPackageName)
                return;
            if (int.TryParse(sbn.Notification.Extras.GetCharSequence(Notification.ExtraTitle).ToString(), out int incomingNumber))
            {
                foreach (ScammerPhoneNumber number in scamNumbers)
                {
					if (incomingNumber.ToString() == number.Number)
					{
						FloatingNotifier.instance.NotifiedPhoneNumberSafety(false);
						return;
					}
				}
                FloatingNotifier.instance.NotifiedPhoneNumberSafety(true);
            }
            else
            {
                string text = sbn.Notification.Extras.GetCharSequence(Notification.ExtraText).ToString();
                FloatingNotifier.instance.ShowCheckingLink(true);
                bool checkURL = false;
                foreach (Match match in linkParser.Matches(text).Cast<Match>())
                {
                    new Thread(new ThreadStart(async delegate
                    {
                        var response = await CheckURLSafety(match.Value);
                        responses.Add(response);
                    })).Start();
                    checkURL = true;
                }
                double susLevel = 0;
                foreach (ScamText scam in scamTexts)
                {
                    double _susLevel = CalculateSimilarity(text.ToLower(), scam.Text.ToLower());
                    susLevel = _susLevel >= susLevel ? _susLevel : susLevel;
                }
                double dangerPrecent = Math.Round(susLevel *= 100);
                if (checkURL)
                    recentDangerLevel = dangerPrecent;
                else
                    FloatingNotifier.instance?.NotifiedDangerLevel(dangerPrecent);
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

        private async Task<UrlSafetyCheckResponseFull> CheckURLSafety(string link) => new DomainApi().DomainSafetyCheck(new UrlSafetyCheckRequestFull(link));

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