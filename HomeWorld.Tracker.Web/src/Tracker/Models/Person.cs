using System.Collections.Generic;

namespace HomeWorld.Tracker.Web.Models
{
    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public byte[] Image { get; set; }

        public ICollection<PersonCard> PersonCards { get; set; }
    }
}
