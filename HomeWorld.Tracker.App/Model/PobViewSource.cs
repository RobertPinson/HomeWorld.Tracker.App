﻿using System.Collections.ObjectModel;

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
    }
}
