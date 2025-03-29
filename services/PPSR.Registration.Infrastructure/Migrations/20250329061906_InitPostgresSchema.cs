using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PPSR.Registration.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitPostgresSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Registrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GrantorFirstName = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: false),
                    GrantorMiddleNames = table.Column<string>(type: "character varying(75)", maxLength: 75, nullable: true),
                    GrantorLastName = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: false),
                    VIN = table.Column<string>(type: "character varying(17)", maxLength: 17, nullable: false),
                    RegistrationStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Duration = table.Column<int>(type: "integer", nullable: false),
                    SpgAcn = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    SpgOrganizationName = table.Column<string>(type: "character varying(75)", maxLength: 75, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Registrations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_GrantorFirstName_GrantorLastName_VIN_SpgAcn",
                table: "Registrations",
                columns: new[] { "GrantorFirstName", "GrantorLastName", "VIN", "SpgAcn" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Registrations");
        }
    }
}
