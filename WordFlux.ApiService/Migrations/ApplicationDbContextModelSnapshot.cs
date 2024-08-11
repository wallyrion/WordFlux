﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using WordFlux.ApiService;

#nullable disable

namespace WordFlux.ApiService.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("WordFlux.ApiService.Card", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("CreatedBy")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("NextReviewDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<TimeSpan>("ReviewInterval")
                        .HasColumnType("interval");

                    b.Property<string>("Term")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Cards");
                });

            modelBuilder.Entity("WordFlux.ApiService.Card", b =>
                {
                    b.OwnsMany("WordFlux.ApiService.CardTranslationItem", "Translations", b1 =>
                        {
                            b1.Property<Guid>("CardId")
                                .HasColumnType("uuid");

                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("integer");

                            b1.Property<string>("ExampleOriginal")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.Property<string>("ExampleTranslated")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.Property<string>("Term")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.HasKey("CardId", "Id");

                            b1.ToTable("Cards");

                            b1.ToJson("Translations");

                            b1.WithOwner()
                                .HasForeignKey("CardId");
                        });

                    b.Navigation("Translations");
                });
#pragma warning restore 612, 618
        }
    }
}
