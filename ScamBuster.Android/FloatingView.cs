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

namespace ScamBuster.Droid
{
	public class FloatingView : Service, View.IOnTouchListener
	{
		public override IBinder OnBind(Intent intent)
		{
			return null;
		}

		public bool OnTouch(View v, MotionEvent e)
		{
			throw new NotImplementedException();
		}
	}
}