using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesynqServer.Migrations
{
    /// <inheritdoc />
    public partial class AddBpmToWip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Bpm",
                table: "Wips",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bpm",
                table: "Wips");
        }
    }
}
