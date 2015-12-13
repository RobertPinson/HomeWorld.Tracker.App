using System;
using System.Threading.Tasks;

namespace HomeWorld.Tracker.App.Service
{
    interface IPersonService
    {
        Task<PersonDto> GetPerson(string cardId);
    }

    public class PersonDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Byte[] Image { get; set; }
        public string PersonCards { get; set; }
    }
}
