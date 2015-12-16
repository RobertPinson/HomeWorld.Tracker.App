using System.Linq;
using Microsoft.AspNet.Builder;

namespace HomeWorld.Tracker.Web.Models
{
    public static class TrackerStoreExtensions
    {
        public static void EnsureSampleData(this IApplicationBuilder app)
        {
            var context = app.ApplicationServices.GetService(typeof(TrackerDbContext));
            var trackerContext = ((TrackerDbContext)context);

            if (!trackerContext.Card.Any())
            {
                trackerContext.Card.AddRange(new Card { Uid = "FF-00-FF-FF" }, new Card { Uid = "6B-FF-00-FF-FF-AD" });
                trackerContext.Person.AddRange(new Person { FirstName = "Bill", LastName = "Gates" }, new Person { FirstName = "Joe", LastName = "Blogs" });
                trackerContext.PersonCard.AddRange(new PersonCard { CardId = 1, PersonId = 1 }, new PersonCard { CardId = 2, PersonId = 2 });
                trackerContext.SaveChanges();
            }
        }
    }
}
