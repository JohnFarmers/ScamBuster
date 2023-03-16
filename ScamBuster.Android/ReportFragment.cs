using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System.Net.Mail;
using System.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScamBuster.Droid
{
    public class ReportFragment : AndroidX.Fragment.App.Fragment
    {
        View view;
        Button button;
        EditText BodyText;
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            view = inflater.Inflate(Resource.Layout.Report_Layout, container, false);
            BodyText = view.FindViewById<EditText>(Resource.Id.textInputEditText2);
            button = view.FindViewById<Button>(Resource.Id.submit_button);
            button.Click += delegate
            {
                SmtpClient Client = new SmtpClient()
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential()
                    {
                        UserName = "34355@student.sg.ac.th",
                        Password = "sgcom0902829644"
                    }
                };
                MailAddress FromEmail = new MailAddress("34355@student.sg.ac.th");
                MailAddress ToEmail = new MailAddress("johnfarmer.potato@gmail.com");
                MailMessage Message = new MailMessage()
                {
                    From = FromEmail,
                    Subject = "ScamBuster",
                    Body = BodyText.Text,

                };
                Message.To.Add(ToEmail);
                try
                {
                    Client.Send(Message);
                    BodyText.Text = string.Empty;
                    Console.WriteLine("Done!");
                }
                catch
                {
                    Console.WriteLine("Error!");
                }
            };
            return view;
        }
        
    }
}