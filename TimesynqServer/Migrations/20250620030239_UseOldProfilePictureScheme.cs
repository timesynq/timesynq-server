using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesynqServer.Migrations
{
    /// <inheritdoc />
    public partial class UseOldProfilePictureScheme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProfilePictureFilename",
                table: "AspNetUsers",
                newName: "ProfilePicture");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProfilePicture",
                table: "AspNetUsers",
                newName: "ProfilePictureFilename");
        }
    }
}
