﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoOtpad.Migrations
{
    /// <inheritdoc />
    public partial class AddImagePathToParts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Parts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Parts");
        }
    }
}
