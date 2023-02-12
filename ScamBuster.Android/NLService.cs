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

namespace ScamBuster.Droid
{
    [Service(Name = "ScamBuster.NLService", Label = "ServiceName", Permission = "android.permission.BIND_NOTIFICATION_LISTENER_SERVICE")]
    [IntentFilter(new[] { "android.service.notification.NotificationListenerService" })]
    public class NLService : NotificationListenerService
    {
        private const string channelID = "ScamBuster";
        private SpamText[] records;
        
        public override void OnCreate()
        {
            base.OnCreate();
            System.Diagnostics.Debug.WriteLine("Notification Listener Service Initialized!");
            records = new CsvReader(new StreamReader(Assets.Open("Scam.csv")), new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = false }).GetRecords<SpamText>().ToArray();
            SendNotification("Scammer", "Free entry in 2 a wkly comp to win FA Cup final tkts 21st May 2005. Text FA to 87121 to receive entry question(std txt rate)T&C's apply 08452810075over18's");
            SendNotification("Friend", "Hello");
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
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
            double susLevel = 0;
            foreach (SpamText scam in records)
            {
                double _susLevel = CalculateSimilarity(sbn.Notification.TickerText.ToString().ToLower(), scam.text.ToLower());
                susLevel = _susLevel >= susLevel ? _susLevel : susLevel;
            }
            susLevel *= 100;
            System.Diagnostics.Debug.WriteLine(susLevel >= 50 ? $"BE CAREFUL! The recent message has {susLevel}% danger level!" : $"SAFE! The recent message has {susLevel}% danger level, but ALWAY STAY CAUTIOUS!");
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

        private void SendNotification(string sender, string message)
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

        private class SpamText { public string text { get; set; } }
    }
}