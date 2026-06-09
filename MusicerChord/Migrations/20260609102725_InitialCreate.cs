using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicerChord.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ListenHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SoundFileId = table.Column<int>(type: "INTEGER", nullable: false),
                    ListenedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListenHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SoundFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RelativePath = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    DurationMs = table.Column<int>(type: "INTEGER", nullable: false),
                    IsSkip = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoundFiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ListenHistories_SoundFileId",
                table: "ListenHistories",
                column: "SoundFileId");

            migrationBuilder.CreateIndex(
                name: "IX_SoundFiles_RelativePath",
                table: "SoundFiles",
                column: "RelativePath",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ListenHistories");

            migrationBuilder.DropTable(
                name: "SoundFiles");
        }
    }
}
