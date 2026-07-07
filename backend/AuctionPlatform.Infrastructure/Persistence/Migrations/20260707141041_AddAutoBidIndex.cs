using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuctionPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAutoBidIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuctionWinners_Auctions_AuctionId",
                table: "AuctionWinners");

            migrationBuilder.CreateIndex(
                name: "IX_AutoBids_AuctionId_IsActive_MaxAmount",
                table: "AutoBids",
                columns: new[] { "AuctionId", "IsActive", "MaxAmount" },
                descending: new[] { false, false, true });

            migrationBuilder.AddForeignKey(
                name: "FK_AuctionWinners_Auctions_AuctionId",
                table: "AuctionWinners",
                column: "AuctionId",
                principalTable: "Auctions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuctionWinners_Auctions_AuctionId",
                table: "AuctionWinners");

            migrationBuilder.DropIndex(
                name: "IX_AutoBids_AuctionId_IsActive_MaxAmount",
                table: "AutoBids");

            migrationBuilder.AddForeignKey(
                name: "FK_AuctionWinners_Auctions_AuctionId",
                table: "AuctionWinners",
                column: "AuctionId",
                principalTable: "Auctions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
