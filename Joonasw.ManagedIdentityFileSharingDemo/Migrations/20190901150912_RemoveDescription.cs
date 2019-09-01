using Microsoft.EntityFrameworkCore.Migrations;

namespace Joonasw.ManagedIdentityFileSharingDemo.Migrations
{
    public partial class RemoveDescription : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "StoredFiles");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "StoredFiles",
                maxLength: 512,
                nullable: true);
        }
    }
}
