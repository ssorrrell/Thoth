using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Thoth.Models;
using Thoth.Views;
using Thoth.ViewModels;
using System.Net.Http;
using System.Collections.ObjectModel;

namespace Thoth.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class SeedPage : ContentPage
    {
        readonly SeedViewModel ViewModel;
        public ObservableCollection<Models.FileInfo> Items { get; set; }
        public string Message;

        public SeedPage(SeedViewModel viewModel = null)
        {
            InitializeComponent();
            Message = "";

            if (viewModel == null)
            {
                this.ViewModel = new SeedViewModel();
            }
            else
            {
                this.ViewModel = viewModel;
                Message = this.ViewModel.Message;
            }

            BindingContext = this.ViewModel;
        }

        async void Seed_Clicked(object sender, EventArgs e)
        {
            var answer = await DisplayAlert("Seed Data", "Are you sure you want to reseed data.", "Yes", "No");
            if (answer)
            {
                MessagingCenter.Send(this, "SeedItems");
                Message = this.ViewModel.Message;
                await Navigation.PopAsync();
            }
        }

        async void OnItemSelected(object sender, EventArgs args)
        {
            var layout = (BindableObject)sender;
            var item = (Models.FileInfo)layout.BindingContext;

            if (item.Name.Substring(item.Name.Length - 3, 3) != "db3") //don't let the database be deleted
            {
                var answer = await DisplayAlert("Delete File ", "Are you sure you want to delete this," + item.Name + ", file.", "Yes", "No");
                if (answer)
                {
                    MessagingCenter.Send(this, "DeleteItem", item);
                }
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (ViewModel.Items.Count == 0)
                ViewModel.IsBusy = true;
        }
    }
}