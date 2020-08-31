using System;
using Xamarin.Forms;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Collections.ObjectModel;

using Thoth.Views;
using Thoth.Helpers;
using Thoth.Common;

namespace Thoth.ViewModels
{
    public class SeedViewModel : BaseViewModel
    {
        string _message = "";
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }
        public ObservableCollection<Models.FileInfo> Items { get; set; }
        public Command LoadItemsCommand { get; set; }

        public SeedViewModel()
        {
            Title = "Seed";
            Message = "";
            Items = new ObservableCollection<Models.FileInfo>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());

            MessagingCenter.Subscribe<SeedPage>(this, "SeedItems", async (obj) =>
            {
                try
                {
                    Message = "Started Seeding Data";
                    await DataStore.SeedData();
                    Message = "Done Seeding Data";
                }
                catch(Exception ex)
                {
                    //could not seed items
                    Message = "Problem Seeding Data " + ex.Message;
                }
            });

            MessagingCenter.Subscribe<SeedPage, Models.FileInfo>(this, "DeleteItem", async (obj, item) =>
            {   //This deletes the downloaded file WITHOUT updating the database, possibly producing and error
                var fileInfo = item as Models.FileInfo;
                try
                {
                    Message = "Deleting File..";
                    //delete downloaded file
                    var result = FileHelper.DeletePodcastFile(fileInfo.Path);
                    if (result)
                        Message = "Done Deleting File";
                    else
                        Message = "Could Not Delete File";
                }
                catch (Exception ex)
                {
                    //could not seed items
                    Message = "Problem Deleting Files " + ex.Message;
                }
            });
        }

        async Task ExecuteLoadItemsCommand()
        {
            IsBusy = true;

            try
            {
                Items.Clear();

                foreach (var file in System.IO.Directory.GetFiles(Constants.BaseFilePath))
                {
                    var fileItem = new Models.FileInfo() { Name = Path.GetFileName(file), Size = 0, Path = file };
                    Items.Add(fileItem);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
