using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace HomeWorld.Tracker.App.Model
{
    public class PobItem : IComparable<PobItem>
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public BitmapImage Image { get; set; }

        public int CompareTo(PobItem that)
        {
            return string.Compare(Name, that.Name, StringComparison.Ordinal);
        }
    }
}
