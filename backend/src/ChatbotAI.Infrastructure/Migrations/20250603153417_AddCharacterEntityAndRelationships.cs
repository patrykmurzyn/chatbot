using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ChatbotAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCharacterEntityAndRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CharacterId",
                table: "Messages",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "Characters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Characters", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Characters",
                columns: new[] { "Id", "Key" },
                values: new object[,]
                {
                    { 1, "rick" },
                    { 2, "yoda" },
                    { 3, "sherlock" },
                    { 4, "socrates" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_CharacterId",
                table: "Messages",
                column: "CharacterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Characters_CharacterId",
                table: "Messages",
                column: "CharacterId",
                principalTable: "Characters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Characters_CharacterId",
                table: "Messages");

            migrationBuilder.DropTable(
                name: "Characters");

            migrationBuilder.DropIndex(
                name: "IX_Messages_CharacterId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "CharacterId",
                table: "Messages");
        }
    }
}
