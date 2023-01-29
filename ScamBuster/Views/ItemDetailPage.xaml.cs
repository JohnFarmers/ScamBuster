using ScamBuster.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace ScamBuster.Views
{
	public partial class ItemDetailPage : ContentPage
	{
		public ItemDetailPage()
		{
			InitializeComponent();
			BindingContext = new ItemDetailViewModel();
		}
	}
}