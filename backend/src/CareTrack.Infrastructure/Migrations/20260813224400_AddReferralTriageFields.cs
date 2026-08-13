using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareTrack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReferralTriageFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TriageNote",
                table: "Referrals",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TriagedAt",
                table: "Referrals",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TriageNote",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "TriagedAt",
                table: "Referrals");
        }
    }
}
