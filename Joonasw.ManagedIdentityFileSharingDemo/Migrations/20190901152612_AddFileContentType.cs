using Microsoft.EntityFrameworkCore.Migrations;

namespace Joonasw.ManagedIdentityFileSharingDemo.Migrations
{
    public partial class AddFileContentType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileContentType",
                table: "StoredFiles",
                maxLength: 128,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileContentType",
                table: "StoredFiles");
        }
    }
}
