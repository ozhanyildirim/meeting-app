using Microsoft.EntityFrameworkCore.Migrations;

namespace MeetingApp.Migrations
{
    public partial class AddMeetingDeleteTrigger : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE TABLE MeetingDeleteLogs (
                    Id INT IDENTITY PRIMARY KEY,
                    MeetingId INT,
                    Title NVARCHAR(200),
                    DeletedAt DATETIME DEFAULT GETDATE(),
                    DeletedBy NVARCHAR(100)
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TRIGGER trg_Meeting_Delete
                ON Meetings
                AFTER DELETE
                AS
                BEGIN
                    INSERT INTO MeetingDeleteLogs (MeetingId, Title, DeletedAt)
                    SELECT Id, Title, GETDATE()
                    FROM deleted;
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_Meeting_Delete");
            migrationBuilder.Sql("DROP TABLE IF EXISTS MeetingDeleteLogs");
        }
    }
}