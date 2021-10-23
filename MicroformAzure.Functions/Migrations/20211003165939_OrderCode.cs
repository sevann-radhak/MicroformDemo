using Microsoft.EntityFrameworkCore.Migrations;

namespace MicroformAzure.Functions.Migrations
{
    public partial class OrderCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrderCode",
                table: "ApplicationRequest",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderCode",
                table: "ApplicationRequest");
        }
    }
}
