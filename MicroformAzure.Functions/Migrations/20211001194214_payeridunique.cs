using Microsoft.EntityFrameworkCore.Migrations;

namespace MicroformAzure.Functions.Migrations
{
    public partial class payeridunique : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PayerId",
                table: "ApplicationPayerInfo",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationPayerInfo_PayerId",
                table: "ApplicationPayerInfo",
                column: "PayerId",
                unique: true,
                filter: "[PayerId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ApplicationPayerInfo_PayerId",
                table: "ApplicationPayerInfo");

            migrationBuilder.AlterColumn<string>(
                name: "PayerId",
                table: "ApplicationPayerInfo",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
