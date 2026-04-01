using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace University_Notebank.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherQuestionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // RelationQuestion: add IsPublished and Deadline
            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "RelationQuestions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "Deadline",
                table: "RelationQuestions",
                type: "datetime2",
                nullable: true);

            // QuestionAnswer: add Marks and TeacherFeedback
            migrationBuilder.AddColumn<int>(
                name: "Marks",
                table: "QuestionAnswers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeacherFeedback",
                table: "QuestionAnswers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "IsPublished", table: "RelationQuestions");
            migrationBuilder.DropColumn(name: "Deadline", table: "RelationQuestions");
            migrationBuilder.DropColumn(name: "Marks", table: "QuestionAnswers");
            migrationBuilder.DropColumn(name: "TeacherFeedback", table: "QuestionAnswers");
        }
    }
}
