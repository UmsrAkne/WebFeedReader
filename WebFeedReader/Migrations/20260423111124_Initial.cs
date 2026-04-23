using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebFeedReader.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.CreateTable(
            //     name: "FeedItems",
            //     columns: table => new
            //     {
            //         Id = table.Column<int>(type: "INTEGER", nullable: false)
            //             .Annotation("Sqlite:Autoincrement", true),
            //         Key = table.Column<string>(type: "TEXT", nullable: true),
            //         SourceId = table.Column<int>(type: "INTEGER", nullable: false),
            //         SourceName = table.Column<string>(type: "TEXT", nullable: true),
            //         Title = table.Column<string>(type: "TEXT", nullable: true),
            //         Link = table.Column<string>(type: "TEXT", nullable: true),
            //         Published = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
            //         Summary = table.Column<string>(type: "TEXT", nullable: true),
            //         Raw = table.Column<string>(type: "TEXT", nullable: true),
            //         IsRead = table.Column<bool>(type: "INTEGER", nullable: false),
            //         IsFavorite = table.Column<bool>(type: "INTEGER", nullable: false),
            //         NgWordCheckVersion = table.Column<int>(type: "INTEGER", nullable: false),
            //         IsNg = table.Column<bool>(type: "INTEGER", nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_FeedItems", x => x.Id);
            //     });
            //
            // migrationBuilder.CreateTable(
            //     name: "FeedSources",
            //     columns: table => new
            //     {
            //         Id = table.Column<int>(type: "INTEGER", nullable: false)
            //             .Annotation("Sqlite:Autoincrement", true),
            //         Name = table.Column<string>(type: "TEXT", nullable: true),
            //         Url = table.Column<string>(type: "TEXT", nullable: true),
            //         Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
            //         CheckIntervalMinutes = table.Column<int>(type: "INTEGER", nullable: false),
            //         UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
            //         CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
            //         Raw = table.Column<string>(type: "TEXT", nullable: true)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_FeedSources", x => x.Id);
            //     });
            //
            // migrationBuilder.CreateTable(
            //     name: "NgWords",
            //     columns: table => new
            //     {
            //         Id = table.Column<int>(type: "INTEGER", nullable: false)
            //             .Annotation("Sqlite:Autoincrement", true),
            //         Value = table.Column<string>(type: "TEXT", nullable: true),
            //         CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
            //         UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_NgWords", x => x.Id);
            //     });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeedItems");

            migrationBuilder.DropTable(
                name: "FeedSources");

            migrationBuilder.DropTable(
                name: "NgWords");
        }
    }
}