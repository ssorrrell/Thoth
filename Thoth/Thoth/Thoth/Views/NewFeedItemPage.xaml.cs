using System;
using System.ComponentModel;
using Xamarin.Forms;

using Thoth.ViewModels;
using Thoth.Services;

namespace Thoth.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class NewFeedItemPage : ContentPage
    {
        public string Link { get; set; }
        public string SaveIcon { get { return Constants.SaveIcon; } }
        public string CancelIcon { get { return Constants.CancelIcon; } }
        public NewFeedItemViewModel ViewModel = null;

        public NewFeedItemPage(NewFeedItemViewModel viewModel = null)
        {
            InitializeComponent();

            if (viewModel == null)
            {
                this.ViewModel = new NewFeedItemViewModel();
            }
            else
            {
                this.ViewModel = viewModel;
            }

            BindingContext = this.ViewModel;
        }

        async void Save_Clicked(object sender, EventArgs e)
        {
            //don't await, close immediately and let the parent screen show the new FeedItem when it finishes loading
            await ViewModel.ExecuteNewFeedItemCommand(); 
            await Navigation.PopModalAsync();
        }

        async void Cancel_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}