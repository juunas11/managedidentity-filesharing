using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Joonasw.ManagedIdentityFileSharingDemo.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StoredFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    FileName = table.Column<string>(maxLength: 256, nullable: true),
                    Description = table.Column<string>(maxLength: 512, nullable: true),
                    StoredBlobId = table.Column<Guid>(nullable: false),
                    CreatorTenantId = table.Column<string>(maxLength: 64, nullable: true),
                    CreatorObjectId = table.Column<string>(maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoredFiles", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StoredFiles");
        }
    }
}
