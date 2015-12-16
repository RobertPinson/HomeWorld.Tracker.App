using System.Collections.Generic;

namespace HomeWorld.Tracker.Web.Models
{
    public class Card
    {
        public int Id { get; set; }
        public string Uid { get; set; }

        public ICollection<PersonCard> PersonCards { get; set; }
    }
}
