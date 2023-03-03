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

namespace ScamBuster.Droid.Services
{
    [Service(Name = "ScamBuster.NLService", Label = "ServiceName", Permission = "android.permission.BIND_NOTIFICATION_LISTENER_SERVICE")]
    [IntentFilter(new[] { "android.service.notification.NotificationListenerService" })]
    public class NLService : NotificationListenerService
    {
        public static NLService instance;
        private const string channelID = "ScamBuster";
        private ScamText[] scamTexts;
        
        public override void OnCreate()
        {
            base.OnCreate();
            instance = this;
            scamTexts = new CsvReader(new StreamReader(Assets.Open("Scam.csv")), new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = false }).GetRecords<ScamText>().ToArray();
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
            if (sbn.Notification.Extras == null)
                return;
			double susLevel = 0;
            foreach (ScamText scam in scamTexts)
            {
                double _susLevel = CalculateSimilarity(sbn.Notification.Extras.GetCharSequence(Notification.ExtraText).ToString().ToLower(), scam.text.ToLower());
                susLevel = _susLevel >= susLevel ? _susLevel : susLevel;
            }
            if(FloatingNotifier.instance != null)
                FloatingNotifier.instance.NotifiedDangerLevel(Math.Round(susLevel *= 100));
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
            return (1.0 - ((double)stepsToSame / (double)Math.Max(source.Length, target.Length)));
        }

        public void SendNotification(string sender, string message)
		{
            var channel = new NotificationChannel(channelID, channelID, NotificationImportance.Default);
            ((NotificationManager)GetSystemService(NotificationService)).CreateNotificationChannel(channel);
            Intent notifIntent = new Intent(this, typeof(MainActivity));
            const int pendingIntentId = 0;
            PendingIntent pendingIntent = PendingIntent.GetActivity(this, pendingIntentId, notifIntent, PendingIntentFlags.OneShot);
            NotificationCompat.Builder builder = new NotificationCompat.Builder(this, channelID)
                .SetContentIntent(pendingIntent)
                .SetContentTitle(sender)
                .SetContentText(message)
                .SetTicker(message)
                .SetSmallIcon(Resource.Drawable.xamarin_logo);
            (GetSystemService(Context.NotificationService) as NotificationManager).Notify(0, builder.Build());
        }

        private class ScamText { public string text { get; set; } }
    }
}