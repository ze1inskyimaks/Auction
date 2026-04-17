using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auction.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoriesAndRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "AuctionLots",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AuctionCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuctionCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CategoryRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    RequestedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AdminComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryRequests_AspNetUsers_RequestedById",
                        column: x => x.RequestedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CategoryRequests_AspNetUsers_ReviewedById",
                        column: x => x.ReviewedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CategoryRequests_AuctionCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "AuctionCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuctionLots_CategoryId",
                table: "AuctionLots",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AuctionCategories_Name",
                table: "AuctionCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CategoryRequests_CategoryId",
                table: "CategoryRequests",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryRequests_CreatedAt",
                table: "CategoryRequests",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryRequests_RequestedById",
                table: "CategoryRequests",
                column: "RequestedById");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryRequests_ReviewedById",
                table: "CategoryRequests",
                column: "ReviewedById");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryRequests_Status",
                table: "CategoryRequests",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_AuctionLots_AuctionCategories_CategoryId",
                table: "AuctionLots",
                column: "CategoryId",
                principalTable: "AuctionCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuctionLots_AuctionCategories_CategoryId",
                table: "AuctionLots");

            migrationBuilder.DropTable(
                name: "CategoryRequests");

            migrationBuilder.DropTable(
                name: "AuctionCategories");

            migrationBuilder.DropIndex(
                name: "IX_AuctionLots_CategoryId",
                table: "AuctionLots");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "AuctionLots");
        }
    }
}
