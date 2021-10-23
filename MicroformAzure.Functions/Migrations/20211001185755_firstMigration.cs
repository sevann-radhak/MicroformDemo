using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MicroformAzure.Functions.Migrations
{
    public partial class firstMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationLogs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RelatedEventId = table.Column<int>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    CreatedTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationPayerInfo",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccessCode = table.Column<string>(nullable: true),
                    IsAccessCodeAvailable = table.Column<bool>(nullable: false),
                    CreatedTime = table.Column<DateTime>(nullable: false),
                    CustomerId = table.Column<string>(nullable: true),
                    PayerId = table.Column<string>(nullable: true),
                    ShippingAddressId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationPayerInfo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationSetup",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationKey = table.Column<string>(nullable: false),
                    ApplicationName = table.Column<string>(nullable: false),
                    CreatedTime = table.Column<DateTime>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    UpdatedTime = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationSetup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CybersourceConfiguration",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuthenticationType = table.Column<string>(nullable: false),
                    EnableLog = table.Column<string>(nullable: false),
                    KeyAlias = table.Column<string>(nullable: false),
                    KeysDirectory = table.Column<string>(nullable: false),
                    KeyFilename = table.Column<string>(nullable: false),
                    KeyPass = table.Column<string>(nullable: false),
                    MerchantID = table.Column<string>(nullable: false),
                    MerchantKeyId = table.Column<string>(nullable: false),
                    MerchantsecretKey = table.Column<string>(nullable: false),
                    RunEnvironment = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CybersourceConfiguration", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledPayments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Param1 = table.Column<string>(nullable: true),
                    Param2 = table.Column<string>(nullable: true),
                    Param3 = table.Column<string>(nullable: true),
                    Frequency = table.Column<string>(nullable: true),
                    LastExecution = table.Column<DateTime>(nullable: false),
                    ExecutionExact = table.Column<DateTime>(nullable: false),
                    Status = table.Column<string>(nullable: true),
                    CreatedTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledPayments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationPayerToken",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CardInfo = table.Column<string>(nullable: true),
                    CreatedTime = table.Column<DateTime>(nullable: false),
                    PaymentMethod = table.Column<string>(nullable: true),
                    Token = table.Column<string>(nullable: true),
                    UpdatedTime = table.Column<DateTime>(nullable: false),
                    ApplicationPayerInfoId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationPayerToken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationPayerToken_ApplicationPayerInfo_ApplicationPayerInfoId",
                        column: x => x.ApplicationPayerInfoId,
                        principalTable: "ApplicationPayerInfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationRequest",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<decimal>(nullable: false),
                    ApplicationName = table.Column<string>(nullable: true),
                    CreatedTime = table.Column<DateTime>(nullable: false),
                    Currency = table.Column<string>(nullable: true),
                    Language = table.Column<string>(nullable: true),
                    Message = table.Column<string>(nullable: true),
                    OfficeName = table.Column<string>(nullable: true),
                    PaymentMethod = table.Column<string>(nullable: true),
                    ReferenceId = table.Column<string>(nullable: true),
                    TransactionCode = table.Column<string>(nullable: true),
                    UrlRedirectAfterPayment = table.Column<string>(nullable: true),
                    ApplicationPayerInfoId = table.Column<int>(nullable: true),
                    ExecutionExact = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationRequest_ApplicationPayerInfo_ApplicationPayerInfoId",
                        column: x => x.ApplicationPayerInfoId,
                        principalTable: "ApplicationPayerInfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentRequest",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BillingInfo = table.Column<string>(nullable: true),
                    CardInfo = table.Column<string>(nullable: true),
                    CreatedTime = table.Column<DateTime>(nullable: false),
                    PaymentMethod = table.Column<string>(nullable: true),
                    ApplicationRequestId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentRequest_ApplicationRequest_ApplicationRequestId",
                        column: x => x.ApplicationRequestId,
                        principalTable: "ApplicationRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentResult",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedTime = table.Column<DateTime>(nullable: false),
                    ReturnDesicion = table.Column<string>(nullable: true),
                    ReturnResult = table.Column<string>(nullable: true),
                    PaymentRequestId = table.Column<int>(nullable: true),
                    Status = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentResult", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentResult_PaymentRequest_PaymentRequestId",
                        column: x => x.PaymentRequestId,
                        principalTable: "PaymentRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationPayerToken_ApplicationPayerInfoId",
                table: "ApplicationPayerToken",
                column: "ApplicationPayerInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationRequest_ApplicationPayerInfoId",
                table: "ApplicationRequest",
                column: "ApplicationPayerInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationSetup_ApplicationName",
                table: "ApplicationSetup",
                column: "ApplicationName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CybersourceConfiguration_MerchantID",
                table: "CybersourceConfiguration",
                column: "MerchantID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequest_ApplicationRequestId",
                table: "PaymentRequest",
                column: "ApplicationRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentResult_PaymentRequestId",
                table: "PaymentResult",
                column: "PaymentRequestId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationLogs");

            migrationBuilder.DropTable(
                name: "ApplicationPayerToken");

            migrationBuilder.DropTable(
                name: "ApplicationSetup");

            migrationBuilder.DropTable(
                name: "CybersourceConfiguration");

            migrationBuilder.DropTable(
                name: "PaymentResult");

            migrationBuilder.DropTable(
                name: "ScheduledPayments");

            migrationBuilder.DropTable(
                name: "PaymentRequest");

            migrationBuilder.DropTable(
                name: "ApplicationRequest");

            migrationBuilder.DropTable(
                name: "ApplicationPayerInfo");
        }
    }
}
