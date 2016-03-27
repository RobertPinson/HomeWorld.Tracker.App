using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace HomeWorld.Tracker.App.Model
{
    public class PobViewSource
    {
        public static ObservableCollection<PobItem> GetPobList()
        {
            var result = new ObservableCollection<PobItem>();

            return result;
        }
    }

    public class PobItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public BitmapImage Image { get; set; }
    }
}
