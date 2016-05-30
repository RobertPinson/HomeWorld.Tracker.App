using System;

namespace HomeWorld.Tracker.Dto
{
    public class MovementDto
    {
        public string Uid { get; set; }
        public int DeviceId { get; set; }
        public bool InLocation { get; set; }
        public DateTime SwipeTime { get; set; }
    }
}