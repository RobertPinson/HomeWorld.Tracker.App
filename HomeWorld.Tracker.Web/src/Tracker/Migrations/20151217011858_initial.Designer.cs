using System;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using HomeWorld.Tracker.Web.Models;

namespace Tracker.Migrations
{
    [DbContext(typeof(TrackerDbContext))]
    [Migration("20151217011858_initial")]
    partial class initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0-rc1-16348")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("HomeWorld.Tracker.Web.Models.Card", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("IsDeleted");

                    b.Property<string>("Uid")
                        .IsRequired();

                    b.HasKey("Id");
                });

            modelBuilder.Entity("HomeWorld.Tracker.Web.Models.Device", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("IsActive");

                    b.Property<int>("LocationId");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("Id");
                });

            modelBuilder.Entity("HomeWorld.Tracker.Web.Models.Location", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("Id");
                });

            modelBuilder.Entity("HomeWorld.Tracker.Web.Models.Movement", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CardId");

                    b.Property<int?>("CardId1");

                    b.Property<int>("DeviceId");

                    b.Property<int>("LocationId");

                    b.Property<DateTime>("SwipeTime");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("HomeWorld.Tracker.Web.Models.Person", b =>
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

            modelBuilder.Entity("HomeWorld.Tracker.Web.Models.PersonCard", b =>
                {
                    b.Property<int>("PersonId");

                    b.Property<int>("CardId");

                    b.HasKey("PersonId", "CardId");
                });

            modelBuilder.Entity("HomeWorld.Tracker.Web.Models.Device", b =>
                {
                    b.HasOne("HomeWorld.Tracker.Web.Models.Location")
                        .WithMany()
                        .HasForeignKey("LocationId");
                });

            modelBuilder.Entity("HomeWorld.Tracker.Web.Models.Movement", b =>
                {
                    b.HasOne("HomeWorld.Tracker.Web.Models.Card")
                        .WithMany()
                        .HasForeignKey("CardId1");

                    b.HasOne("HomeWorld.Tracker.Web.Models.Device")
                        .WithMany()
                        .HasForeignKey("DeviceId");

                    b.HasOne("HomeWorld.Tracker.Web.Models.Location")
                        .WithMany()
                        .HasForeignKey("LocationId");
                });

            modelBuilder.Entity("HomeWorld.Tracker.Web.Models.PersonCard", b =>
                {
                    b.HasOne("HomeWorld.Tracker.Web.Models.Card")
                        .WithMany()
                        .HasForeignKey("CardId");

                    b.HasOne("HomeWorld.Tracker.Web.Models.Person")
                        .WithMany()
                        .HasForeignKey("PersonId");
                });
        }
    }
}
