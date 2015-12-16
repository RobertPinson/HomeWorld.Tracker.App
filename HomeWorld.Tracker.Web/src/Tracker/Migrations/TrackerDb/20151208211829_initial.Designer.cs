using System;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Tracker.Models;

namespace Tracker.Migrations.TrackerDb
{
    [DbContext(typeof(TrackerDbContext))]
    [Migration("20151208211829_initial")]
    partial class initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0-rc1-16348")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Tracker.Models.Card", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Uid")
                        .IsRequired();

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Tracker.Models.Person", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("FirstName")
                        .IsRequired();

                    b.Property<byte[]>("Image");

                    b.Property<string>("LastName")
                        .IsRequired();

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Tracker.Models.PersonCard", b =>
                {
                    b.Property<int>("PersonId");

                    b.Property<int>("CardId");

                    b.HasKey("PersonId", "CardId");
                });

            modelBuilder.Entity("Tracker.Models.PersonCard", b =>
                {
                    b.HasOne("Tracker.Models.Card")
                        .WithMany()
                        .HasForeignKey("CardId");

                    b.HasOne("Tracker.Models.Person")
                        .WithMany()
                        .HasForeignKey("PersonId");
                });
        }
    }
}
