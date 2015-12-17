using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;

namespace HomeWorld.Tracker.Web.Models
{
    internal class ConfigureEntities
    {
        public static void ConfigurePersonCard(ModelBuilder builder)
        {
            builder.Entity<PersonCard>()
                .HasKey(pc => new {pc.PersonId, pc.CardId});

            builder.Entity<PersonCard>()
                .HasOne(pc => pc.Card)
                .WithMany(c => c.PersonCards)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PersonCard>()
                .HasOne(pc => pc.Person)
                .WithMany(c => c.PersonCards)
                .OnDelete(DeleteBehavior.Restrict);
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

        public static void ConfigureLocation(ModelBuilder builder)
        {
            builder.Entity<Location>()
                .HasKey(l => l.Id);

            builder.Entity<Location>()
                .Property(l => l.Name)
                .IsRequired();
        }

        public static void ConfigureDevice(ModelBuilder builder)
        {
            builder.Entity<Device>()
                .HasKey(d => d.Id);

            builder.Entity<Device>()
                .Property(d => d.Name)
                .IsRequired();

            builder.Entity<Device>()
                .HasOne(d => d.Location)
                .WithMany(l => l.Devices)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public static void ConfigureMovement(ModelBuilder builder)
        {
            builder.Entity<Movement>()
                .HasKey(m => m.Id);

            builder.Entity<Movement>()
                .HasOne(m => m.Location)
                .WithMany(l => l.Movements)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Movement>()
                .HasOne(m => m.Device)
                .WithMany(l => l.Movements)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}