using System.Collections.Generic;
using Microsoft.Data.Entity;

namespace HomeWorld.Tracker.Web.Models
{
    public class TrackerDbContext : DbContext
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
           base.OnModelCreating(builder);

            ConfigureEntities.ConfigurePerson(builder);
            ConfigureEntities.ConfigureCard(builder);
            ConfigureEntities.ConfigurePersonCard(builder);
            ConfigureEntities.ConfigureLocation(builder);
            ConfigureEntities.ConfigureDevice(builder);
            ConfigureEntities.ConfigureMovement(builder);
        }

        public DbSet<Person> Person { get; set; }
        public DbSet<Card> Card { get; set; }
        public DbSet<PersonCard> PersonCard { get; set; }
        public DbSet<Location> Location { get; set; }
        public DbSet<Device> Device { get; set; }
        public DbSet<Movement> Movement { get; set; }
    }

    public class Device
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int LocationId { get; set; }
        public bool IsActive { get; set; }

        public Location Location { get; set; }
        public ICollection<Movement> Movements { get; set; }
    }
}
