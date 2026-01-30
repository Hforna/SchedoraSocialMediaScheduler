using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Schedora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnsOnMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlatformSpecificSettings",
                table: "PostMedias");

            migrationBuilder.AlterColumn<int>(
                name: "Width",
                table: "Media",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Height",
                table: "Media",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Format",
                table: "Media",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Media",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PostMedias_MediaId",
                table: "PostMedias",
                column: "MediaId");

            migrationBuilder.AddForeignKey(
                name: "FK_PostMedias_Media_MediaId",
                table: "PostMedias",
                column: "MediaId",
                principalTable: "Media",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PostPlatforms_Posts_PostId",
                table: "PostPlatforms",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostMedias_Media_MediaId",
                table: "PostMedias");

            migrationBuilder.DropForeignKey(
                name: "FK_PostPlatforms_Posts_PostId",
                table: "PostPlatforms");

            migrationBuilder.DropIndex(
                name: "IX_PostMedias_MediaId",
                table: "PostMedias");

            migrationBuilder.DropColumn(
                name: "Format",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Media");

            migrationBuilder.AddColumn<string>(
                name: "PlatformSpecificSettings",
                table: "PostMedias",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Width",
                table: "Media",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "Height",
                table: "Media",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
