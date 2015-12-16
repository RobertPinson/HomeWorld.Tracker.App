using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Threading.Tasks;

namespace Tracker.Models
{
    public class Card
    {
        public int Id { get; set; }
        public string Uid { get; set; }

        public ICollection<PersonCard> PersonCards { get; set; }
    }
}
