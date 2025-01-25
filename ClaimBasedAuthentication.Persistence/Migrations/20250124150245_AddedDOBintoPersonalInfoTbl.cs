using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClaimBasedAuthentication.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedDOBintoPersonalInfoTbl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DOB",
                table: "PersonalInfo",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DOB",
                table: "PersonalInfo");
        }
    }
}
