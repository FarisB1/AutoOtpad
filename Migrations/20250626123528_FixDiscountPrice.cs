using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoOtpad.Migrations
{
    /// <inheritdoc />
    public partial class FixDiscountPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountedPrice",
                table: "Parts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DiscountedPrice",
                table: "Parts",
                type: "decimal(18,2)",
                nullable: true);
        }
    }
}
