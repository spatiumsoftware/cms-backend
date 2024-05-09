using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddNewFeaturestoSubscriptionTB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsViewReport",
                table: "Subscription",
                newName: "SEO_Usage");

            migrationBuilder.AddColumn<int>(
                name: "NumberOfPosts",
                table: "Subscription",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StorageCapacity",
                table: "Subscription",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumberOfPosts",
                table: "Subscription");

            migrationBuilder.DropColumn(
                name: "StorageCapacity",
                table: "Subscription");

            migrationBuilder.RenameColumn(
                name: "SEO_Usage",
                table: "Subscription",
                newName: "IsViewReport");
        }
    }
}
