using System;

namespace HomeWorld.Tracker.Web.Models
{
    public class Movement
    {
        public int Id { get; set; }
        public string CardId { get; set; }
        public int LocationId { get; set; }
        public int DeviceId { get; set; }
        public DateTime SwipeTime { get; set; }

        public Location Location { get; set; }
        public Device Device { get; set; }
    }
}