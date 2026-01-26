using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TansiqyV1.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddCollegeFeesFieldsAndTechnologicalType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AdditionalFees",
                table: "Colleges",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FeesCategoryA",
                table: "Colleges",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FeesCategoryB",
                table: "Colleges",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FeesCategoryC",
                table: "Colleges",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FeesPerHour",
                table: "Colleges",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinimumHoursPerSemester",
                table: "Colleges",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalFees",
                table: "Colleges");

            migrationBuilder.DropColumn(
                name: "FeesCategoryA",
                table: "Colleges");

            migrationBuilder.DropColumn(
                name: "FeesCategoryB",
                table: "Colleges");

            migrationBuilder.DropColumn(
                name: "FeesCategoryC",
                table: "Colleges");

            migrationBuilder.DropColumn(
                name: "FeesPerHour",
                table: "Colleges");

            migrationBuilder.DropColumn(
                name: "MinimumHoursPerSemester",
                table: "Colleges");
        }
    }
}
