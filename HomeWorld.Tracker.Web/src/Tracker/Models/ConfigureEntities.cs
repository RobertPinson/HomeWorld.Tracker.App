using Microsoft.Data.Entity;

namespace Tracker.Models
{
    internal class ConfigureEntities
    {
        public static void ConfigurePersonCard(ModelBuilder builder)
        {
            builder.Entity<PersonCard>()
                .HasKey(pc => new {pc.PersonId, pc.CardId});
        }

        public static void ConfigureCard(ModelBuilder builder)
        {
            builder.Entity<Card>()
                .HasKey(c => new {c.Id});

          

            builder.Entity<Card>()
                .Property(c => c.Uid)
                .IsRequired();
        }

        public static void ConfigurePerson(ModelBuilder builder)
        {
            builder.Entity<Person>()
                .HasKey(p => p.Id);

            builder.Entity<Person>()
                .Property(p => p.FirstName)
                .IsRequired();

            builder.Entity<Person>()
                .Property(p => p.LastName)
                .IsRequired();
        }
    }
}