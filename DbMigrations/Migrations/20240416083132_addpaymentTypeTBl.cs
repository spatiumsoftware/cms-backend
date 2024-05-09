using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class addpaymentTypeTBl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PaymentTypeId",
                table: "BillingHistory",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PaymentTypes",
                schema: "Lookup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BillingHistory_PaymentTypeId",
                table: "BillingHistory",
                column: "PaymentTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_BillingHistory_PaymentTypes_PaymentTypeId",
                table: "BillingHistory",
                column: "PaymentTypeId",
                principalSchema: "Lookup",
                principalTable: "PaymentTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BillingHistory_PaymentTypes_PaymentTypeId",
                table: "BillingHistory");

            migrationBuilder.DropTable(
                name: "PaymentTypes",
                schema: "Lookup");

            migrationBuilder.DropIndex(
                name: "IX_BillingHistory_PaymentTypeId",
                table: "BillingHistory");

            migrationBuilder.DropColumn(
                name: "PaymentTypeId",
                table: "BillingHistory");
        }
    }
}
