using Android.OS;
using Android.Views;

namespace ScamBuster.Droid.Resources
{
    public class DetectionFragment : AndroidX.Fragment.App.Fragment
    {
        View view;
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            view = inflater.Inflate(Resource.Layout.Detection_Layout, container, false);
            return view;
        }
    }
}