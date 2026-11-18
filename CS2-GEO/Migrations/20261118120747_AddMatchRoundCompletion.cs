using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CS2_GEO.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchRoundCompletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "completed_at",
                table: "match_rounds",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "completed_at",
                table: "match_rounds");
        }
    }
}
