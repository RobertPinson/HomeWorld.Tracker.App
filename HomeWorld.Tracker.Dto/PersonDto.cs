using System;

namespace HomeWorld.Tracker.Dto
{
    public class PersonDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Byte[] Image { get; set; }
        public string CardUId { get; set; }
        public bool InLocation { get; set; } 
    }
}
