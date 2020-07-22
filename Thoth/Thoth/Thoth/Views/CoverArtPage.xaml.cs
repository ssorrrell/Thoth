using System.ComponentModel;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Thoth.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CoverArtPage : ContentPage, INotifyPropertyChanged
    {
        public string FileName { get; set; } = "";

        public CoverArtPage(string fileName)
        {
            InitializeComponent();

            FileName = fileName;
            BindingContext = this;
            
        }
    }
}