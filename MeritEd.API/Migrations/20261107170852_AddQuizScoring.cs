using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeritEd.API.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizScoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Activities_EnrollmentId",
                table: "Activities");

            migrationBuilder.AddColumn<int>(
                name: "CorrectAnswers",
                table: "Activities",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int[]>(
                name: "SelectedAnswers",
                table: "Activities",
                type: "integer[]",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalQuestions",
                table: "Activities",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Activities_EnrollmentId_ContentItemId",
                table: "Activities",
                columns: new[] { "EnrollmentId", "ContentItemId" },
                unique: true,
                filter: "\"Status\" <> 2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Activities_EnrollmentId_ContentItemId",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "CorrectAnswers",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "SelectedAnswers",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "TotalQuestions",
                table: "Activities");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_EnrollmentId",
                table: "Activities",
                column: "EnrollmentId");
        }
    }
}
