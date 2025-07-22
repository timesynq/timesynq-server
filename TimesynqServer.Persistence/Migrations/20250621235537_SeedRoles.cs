using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TimesynqServer.Migrations
{
    /// <inheritdoc />
    public partial class SeedRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("9f743e92-c55a-4bc2-a238-014b015a01a7"), null, "Admin", "ADMIN" },
                    { new Guid("a10f83cc-2836-487a-96d2-22579c443511"), null, "ConfirmedUser", "CONFIRMEDUSER" },
                    { new Guid("fb24feb3-a13a-4170-bbc0-c5469566d184"), null, "UnconfirmedUser", "UNCONFIRMEDUSER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("9f743e92-c55a-4bc2-a238-014b015a01a7"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("a10f83cc-2836-487a-96d2-22579c443511"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("fb24feb3-a13a-4170-bbc0-c5469566d184"));
        }
    }
}
