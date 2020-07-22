using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Thoth.Models;
using Thoth.ViewModels;

namespace Thoth.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class FeedItemDetailPage : ContentPage
    {
        readonly FeedItemListViewModel viewModel;
        public FeedItem FeedItem { get; set; }

        public FeedItemDetailPage(FeedItemListViewModel viewModel)
        {
            InitializeComponent();

            FeedItem = viewModel.FeedItem;
            BindingContext = this.viewModel = viewModel;
        }

        async void DeleteItem_Clicked(object sender, EventArgs e)
        {
            var answer = await DisplayAlert("Delete Feed ", "Are you sure you want to delete this," + FeedItem.Text + ", Podcast subscription and downloaded files.", "Yes", "No");
            if (answer)
            {
                MessagingCenter.Send(this, "DeleteItem", FeedItem);
                await Navigation.PopAsync();
            }
        }
    }
}