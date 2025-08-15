using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesynqServer.Migrations
{
    /// <inheritdoc />
    public partial class ChangeUserName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdatedUserNameUTC",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "NormalizedSavedUserName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SavedUserName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastUpdatedUserNameUTC",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NormalizedSavedUserName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SavedUserName",
                table: "AspNetUsers");
        }
    }
}
