using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Schedora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class change_media_columns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AspectRatio",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "BlobUrl",
                table: "Media");

            migrationBuilder.RenameColumn(
                name: "ThumbnailUrl",
                table: "Media",
                newName: "ThumbnailName");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UploadedAt",
                table: "Media",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ThumbnailName",
                table: "Media",
                newName: "ThumbnailUrl");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UploadedAt",
                table: "Media",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AspectRatio",
                table: "Media",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BlobUrl",
                table: "Media",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }
    }
}
