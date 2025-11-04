using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Seisundid.Migrations
{
    /// <inheritdoc />
    public partial class addedPaevaGraafik : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Poed",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nimi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TananePaev = table.Column<int>(type: "int", nullable: false),
                    PraeguneAeg = table.Column<TimeSpan>(type: "time", nullable: false),
                    OnAvatud = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Poed", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaevaGraafiks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Paev = table.Column<int>(type: "int", nullable: false),
                    AvatudAlates = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AvatudKuni = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PoodId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaevaGraafiks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaevaGraafiks_Poed_PoodId",
                        column: x => x.PoodId,
                        principalTable: "Poed",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaevaGraafiks_PoodId",
                table: "PaevaGraafiks",
                column: "PoodId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaevaGraafiks");

            migrationBuilder.DropTable(
                name: "Poed");
        }
    }
}
