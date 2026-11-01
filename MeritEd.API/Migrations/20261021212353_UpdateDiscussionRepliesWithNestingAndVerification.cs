using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeritEd.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDiscussionRepliesWithNestingAndVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsInstructorVerified",
                table: "DiscussionReplies",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentReplyId",
                table: "DiscussionReplies",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiscussionReplies_ParentReplyId",
                table: "DiscussionReplies",
                column: "ParentReplyId");

            migrationBuilder.AddForeignKey(
                name: "FK_DiscussionReplies_DiscussionReplies_ParentReplyId",
                table: "DiscussionReplies",
                column: "ParentReplyId",
                principalTable: "DiscussionReplies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiscussionReplies_DiscussionReplies_ParentReplyId",
                table: "DiscussionReplies");

            migrationBuilder.DropIndex(
                name: "IX_DiscussionReplies_ParentReplyId",
                table: "DiscussionReplies");

            migrationBuilder.DropColumn(
                name: "IsInstructorVerified",
                table: "DiscussionReplies");

            migrationBuilder.DropColumn(
                name: "ParentReplyId",
                table: "DiscussionReplies");
        }
    }
}
