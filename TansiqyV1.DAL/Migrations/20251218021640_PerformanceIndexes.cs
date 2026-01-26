using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TansiqyV1.DAL.Migrations
{
    /// <inheritdoc />
    public partial class PerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UniversityBranches_IsDeleted",
                table: "UniversityBranches",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_UniversityBranches_UniversityId_IsDeleted",
                table: "UniversityBranches",
                columns: new[] { "UniversityId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Universities_Governorate_IsDeleted",
                table: "Universities",
                columns: new[] { "Governorate", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Universities_IsDeleted",
                table: "Universities",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Universities_Type_IsDeleted",
                table: "Universities",
                columns: new[] { "Type", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Departments_CollegeId_IsDeleted",
                table: "Departments",
                columns: new[] { "CollegeId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Departments_IsDeleted",
                table: "Departments",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_StudyType",
                table: "Departments",
                column: "StudyType");

            migrationBuilder.CreateIndex(
                name: "IX_Colleges_IsDeleted",
                table: "Colleges",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Colleges_UniversityId_IsDeleted",
                table: "Colleges",
                columns: new[] { "UniversityId", "IsDeleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UniversityBranches_IsDeleted",
                table: "UniversityBranches");

            migrationBuilder.DropIndex(
                name: "IX_UniversityBranches_UniversityId_IsDeleted",
                table: "UniversityBranches");

            migrationBuilder.DropIndex(
                name: "IX_Universities_Governorate_IsDeleted",
                table: "Universities");

            migrationBuilder.DropIndex(
                name: "IX_Universities_IsDeleted",
                table: "Universities");

            migrationBuilder.DropIndex(
                name: "IX_Universities_Type_IsDeleted",
                table: "Universities");

            migrationBuilder.DropIndex(
                name: "IX_Departments_CollegeId_IsDeleted",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Departments_IsDeleted",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Departments_StudyType",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Colleges_IsDeleted",
                table: "Colleges");

            migrationBuilder.DropIndex(
                name: "IX_Colleges_UniversityId_IsDeleted",
                table: "Colleges");
        }
    }
}
