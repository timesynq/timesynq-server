using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesynqServer.Migrations
{
    /// <inheritdoc />
    public partial class AddChannelCountToWip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Channels",
                table: "Wips",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Channels",
                table: "Wips");
        }
    }
}
