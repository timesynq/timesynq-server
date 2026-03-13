using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesynqServer.Migrations
{
    /// <inheritdoc />
    public partial class AddAcceptanceToShares : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAccepted",
                table: "Shares",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAccepted",
                table: "Shares");
        }
    }
}
