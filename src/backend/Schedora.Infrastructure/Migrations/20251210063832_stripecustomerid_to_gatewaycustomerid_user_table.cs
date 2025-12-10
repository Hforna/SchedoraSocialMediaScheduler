using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Schedora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class stripecustomerid_to_gatewaycustomerid_user_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GatewayCustomerId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GatewayCustomerId",
                table: "AspNetUsers");
        }
    }
}
