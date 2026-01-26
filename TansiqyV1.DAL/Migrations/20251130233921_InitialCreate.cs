using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TansiqyV1.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Universities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    OfficialWebsite = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Governorate = table.Column<int>(type: "int", nullable: false),
                    LastYearCoordination = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Fees = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    InformationSources = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Universities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Colleges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UniversityId = table.Column<int>(type: "int", nullable: false),
                    OfficialWebsite = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fees = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LastYearCoordination = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Colleges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Colleges_Universities_UniversityId",
                        column: x => x.UniversityId,
                        principalTable: "Universities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UniversityBranches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UniversityId = table.Column<int>(type: "int", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Governorate = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UniversityBranches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UniversityBranches_Universities_UniversityId",
                        column: x => x.UniversityId,
                        principalTable: "Universities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CollegeId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StudyType = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Departments_Colleges_CollegeId",
                        column: x => x.CollegeId,
                        principalTable: "Colleges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Colleges_NameAr",
                table: "Colleges",
                column: "NameAr");

            migrationBuilder.CreateIndex(
                name: "IX_Colleges_UniversityId",
                table: "Colleges",
                column: "UniversityId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_CollegeId",
                table: "Departments",
                column: "CollegeId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_NameAr",
                table: "Departments",
                column: "NameAr");

            migrationBuilder.CreateIndex(
                name: "IX_Universities_Governorate",
                table: "Universities",
                column: "Governorate");

            migrationBuilder.CreateIndex(
                name: "IX_Universities_NameAr",
                table: "Universities",
                column: "NameAr");

            migrationBuilder.CreateIndex(
                name: "IX_Universities_Type",
                table: "Universities",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_UniversityBranches_UniversityId",
                table: "UniversityBranches",
                column: "UniversityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "UniversityBranches");

            migrationBuilder.DropTable(
                name: "Colleges");

            migrationBuilder.DropTable(
                name: "Universities");
        }
    }
}
