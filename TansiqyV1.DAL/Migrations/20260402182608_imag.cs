using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TansiqyV1.DAL.Migrations
{
    /// <inheritdoc />
    public partial class imag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Image",
                table: "Universities",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Image",
                table: "Universities");
        }
    }
}
