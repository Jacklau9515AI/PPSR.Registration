﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using PPSR.Registration.Infrastructure.Persistence;

#nullable disable

namespace PPSR.Registration.Infrastructure.Migrations
{
    [DbContext(typeof(PpsrDbContext))]
    [Migration("20250329061906_InitPostgresSchema")]
    partial class InitPostgresSchema
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.14")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("PPSR.Registration.Domain.Entities.PpsrRegistration", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("Duration")
                        .HasColumnType("integer");

                    b.Property<string>("GrantorFirstName")
                        .IsRequired()
                        .HasMaxLength(35)
                        .HasColumnType("character varying(35)");

                    b.Property<string>("GrantorLastName")
                        .IsRequired()
                        .HasMaxLength(35)
                        .HasColumnType("character varying(35)");

                    b.Property<string>("GrantorMiddleNames")
                        .HasMaxLength(75)
                        .HasColumnType("character varying(75)");

                    b.Property<DateTime>("RegistrationStartDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("SpgAcn")
                        .IsRequired()
                        .HasMaxLength(9)
                        .HasColumnType("character varying(9)");

                    b.Property<string>("SpgOrganizationName")
                        .IsRequired()
                        .HasMaxLength(75)
                        .HasColumnType("character varying(75)");

                    b.Property<string>("VIN")
                        .IsRequired()
                        .HasMaxLength(17)
                        .HasColumnType("character varying(17)");

                    b.HasKey("Id");

                    b.HasIndex("GrantorFirstName", "GrantorLastName", "VIN", "SpgAcn")
                        .IsUnique();

                    b.ToTable("Registrations");
                });
#pragma warning restore 612, 618
        }
    }
}
