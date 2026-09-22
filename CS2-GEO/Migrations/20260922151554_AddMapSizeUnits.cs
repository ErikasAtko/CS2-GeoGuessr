using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CS2_GEO.Migrations
{
    /// <inheritdoc />
    public partial class AddMapSizeUnits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "size_units",
                table: "maps",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "size_units",
                table: "maps");
        }
    }
}
