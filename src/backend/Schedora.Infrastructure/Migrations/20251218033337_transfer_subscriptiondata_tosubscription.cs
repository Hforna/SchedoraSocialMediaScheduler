using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Schedora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class transfer_subscriptiondata_tosubscription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubscriptionExpiresAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SubscriptionTier",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    GatewayProvider = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GatewaySubscriptionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GatewayCustomerId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GatewayPriceId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubscriptionTier = table.Column<int>(type: "int", nullable: false),
                    CurrentPeriodEndsAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CanceledAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_UserId",
                table: "Subscriptions",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.AddColumn<DateTime>(
                name: "SubscriptionExpiresAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubscriptionTier",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
