using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareTrack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentSchedulingIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PatientId_ScheduledStart_ScheduledEnd",
                table: "Appointments",
                columns: new[] { "PatientId", "ScheduledStart", "ScheduledEnd" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointments_PatientId_ScheduledStart_ScheduledEnd",
                table: "Appointments");
        }
    }
}
