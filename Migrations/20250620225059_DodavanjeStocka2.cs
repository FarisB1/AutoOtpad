using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoOtpad.Migrations
{
    /// <inheritdoc />
    public partial class DodavanjeStocka2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NeedsRestock",
                table: "Parts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ReorderLevel",
                table: "Parts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NeedsRestock",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "ReorderLevel",
                table: "Parts");
        }
    }
}
