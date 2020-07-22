using System;
using System.ComponentModel;
using Xamarin.Forms;

using Thoth.Models;
using Thoth.ViewModels;

namespace Thoth.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class FeedItemListPage : ContentPage
    {
        FeedItemListViewModel ViewModel;

        public FeedItemListPage()
        {
            InitializeComponent();

            BindingContext = ViewModel = new FeedItemListViewModel();
        }

        async void OnItemSelected(object sender, EventArgs args)
        {
            var layout = (BindableObject)sender;
            var item = (FeedItem)layout.BindingContext;
            await Navigation.PushAsync(new RssEpisodeListPage(new RssEpisodeListViewModel(item)));
        }

        async void AddItem_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new NavigationPage(new NewFeedItemPage()));
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (ViewModel.Items.Count == 0)
                ViewModel.IsBusy = true;
        }
    }
}