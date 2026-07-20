using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuctionPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAutoBid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AutoBids_AspNetUsers_BidderId",
                table: "AutoBids");

            migrationBuilder.DropIndex(
                name: "IX_AutoBids_AuctionId",
                table: "AutoBids");

            migrationBuilder.CreateIndex(
                name: "IX_AutoBids_AuctionId_BidderId",
                table: "AutoBids",
                columns: new[] { "AuctionId", "BidderId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AutoBids_AspNetUsers_BidderId",
                table: "AutoBids",
                column: "BidderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AutoBids_AspNetUsers_BidderId",
                table: "AutoBids");

            migrationBuilder.DropIndex(
                name: "IX_AutoBids_AuctionId_BidderId",
                table: "AutoBids");

            migrationBuilder.CreateIndex(
                name: "IX_AutoBids_AuctionId",
                table: "AutoBids",
                column: "AuctionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AutoBids_AspNetUsers_BidderId",
                table: "AutoBids",
                column: "BidderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
