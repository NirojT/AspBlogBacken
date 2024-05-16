using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Blog.Migrations
{
    /// <inheritdoc />
    public partial class secondsa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Blogs_Reacts_reactid",
                table: "Blogs");

            migrationBuilder.DropIndex(
                name: "IX_Blogs_reactid",
                table: "Blogs");

            migrationBuilder.DropColumn(
                name: "reactid",
                table: "Blogs");

            migrationBuilder.AddColumn<Guid>(
                name: "Blogsid",
                table: "Reacts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reacts_Blogsid",
                table: "Reacts",
                column: "Blogsid");

            migrationBuilder.AddForeignKey(
                name: "FK_Reacts_Blogs_Blogsid",
                table: "Reacts",
                column: "Blogsid",
                principalTable: "Blogs",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reacts_Blogs_Blogsid",
                table: "Reacts");

            migrationBuilder.DropIndex(
                name: "IX_Reacts_Blogsid",
                table: "Reacts");

            migrationBuilder.DropColumn(
                name: "Blogsid",
                table: "Reacts");

            migrationBuilder.AddColumn<Guid>(
                name: "reactid",
                table: "Blogs",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Blogs_reactid",
                table: "Blogs",
                column: "reactid");

            migrationBuilder.AddForeignKey(
                name: "FK_Blogs_Reacts_reactid",
                table: "Blogs",
                column: "reactid",
                principalTable: "Reacts",
                principalColumn: "id");
        }
    }
}
