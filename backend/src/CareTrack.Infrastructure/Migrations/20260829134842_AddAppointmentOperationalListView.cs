using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareTrack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentOperationalListView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Narrow operational projection for appointment list/search only.
            // Transactional appointment workflows continue to use mapped tables.
            migrationBuilder.Sql(
                """
                CREATE OR ALTER VIEW [dbo].[vw_AppointmentOperationalList]
                AS
                SELECT
                    a.[Id],
                    a.[AppointmentReference],
                    a.[AppointmentType],
                    a.[Status],
                    a.[ScheduledStart],
                    a.[ScheduledEnd],
                    a.[Location],
                    a.[CreatedAt],
                    a.[PatientId],
                    p.[PatientReference],
                    CONCAT(p.[FirstName], CHAR(32), p.[LastName]) AS [PatientDisplayName],
                    a.[ReferralId],
                    r.[ReferralReference]
                FROM [dbo].[Appointments] AS a
                INNER JOIN [dbo].[Patients] AS p ON p.[Id] = a.[PatientId]
                INNER JOIN [dbo].[Referrals] AS r ON r.[Id] = a.[ReferralId];
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DROP VIEW IF EXISTS [dbo].[vw_AppointmentOperationalList];");
        }
    }
}
