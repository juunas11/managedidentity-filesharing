using Microsoft.EntityFrameworkCore.Migrations;

namespace Joonasw.ManagedIdentityFileSharingDemo.Migrations
{
    public partial class AddFileSize : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "SizeInBytes",
                table: "StoredFiles",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SizeInBytes",
                table: "StoredFiles");
        }
    }
}
