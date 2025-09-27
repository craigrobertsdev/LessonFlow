using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LessonFlow.Api.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountSetupComplete = table.Column<bool>(type: "boolean", nullable: false),
                    LastSelectedYear = table.Column<int>(type: "integer", nullable: false),
                    LastSelectedWeekStart = table.Column<DateOnly>(type: "date", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Calendar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TermNumber = table.Column<int>(type: "integer", nullable: false),
                    TermStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TermEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Calendar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CurriculumSubjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurriculumSubjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "locations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StreetNumber = table.Column<string>(type: "text", nullable: false),
                    StreetName = table.Column<string>(type: "text", nullable: false),
                    Suburb = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_locations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TermDates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TermNumber = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TermDates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YearData",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolName = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CalendarYear = table.Column<int>(type: "integer", nullable: false),
                    WorkingDays = table.Column<string>(type: "text", nullable: false),
                    YearLevels = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YearData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YearData_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Resources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Url = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    AssociatedStrands = table.Column<string>(type: "text", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    YearLevels = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Resources_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Resources_CurriculumSubjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "CurriculumSubjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YearLevels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    YearLevelValue = table.Column<int>(type: "integer", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YearLevels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YearLevels_CurriculumSubjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "CurriculumSubjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SchoolEvent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FullDay = table.Column<bool>(type: "boolean", nullable: false),
                    EventStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EventEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchoolEvent_locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    YearDataId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Students_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Students_YearData_YearDataId",
                        column: x => x.YearDataId,
                        principalTable: "YearData",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SubjectYearData",
                columns: table => new
                {
                    SubjectsTaughtId = table.Column<Guid>(type: "uuid", nullable: false),
                    YearDataId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectYearData", x => new { x.SubjectsTaughtId, x.YearDataId });
                    table.ForeignKey(
                        name: "FK_SubjectYearData_CurriculumSubjects_SubjectsTaughtId",
                        column: x => x.SubjectsTaughtId,
                        principalTable: "CurriculumSubjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubjectYearData_YearData_YearDataId",
                        column: x => x.YearDataId,
                        principalTable: "YearData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TermPlanners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    YearDataId = table.Column<Guid>(type: "uuid", nullable: false),
                    CalendarYear = table.Column<int>(type: "integer", nullable: false),
                    YearLevels = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TermPlanners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TermPlanners_YearData_YearDataId",
                        column: x => x.YearDataId,
                        principalTable: "YearData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WeekPlanners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    YearDataId = table.Column<Guid>(type: "uuid", nullable: false),
                    WeekStart = table.Column<DateOnly>(type: "date", nullable: false),
                    WeekNumber = table.Column<int>(type: "integer", nullable: false),
                    TermNumber = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeekPlanners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeekPlanners_YearData_YearDataId",
                        column: x => x.YearDataId,
                        principalTable: "YearData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WeekPlannerTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    YearDataId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeekPlannerTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeekPlannerTemplates_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WeekPlannerTemplates_YearData_YearDataId",
                        column: x => x.YearDataId,
                        principalTable: "YearData",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Capabilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Descriptors = table.Column<List<string>>(type: "text[]", nullable: false),
                    YearLevelId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Capabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Capabilities_YearLevels_YearLevelId",
                        column: x => x.YearLevelId,
                        principalTable: "YearLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConceptualOrganisers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    WhatItIs = table.Column<string>(type: "text", nullable: false),
                    WhyItMatters = table.Column<string>(type: "text", nullable: false),
                    ConceptualUnderstandings = table.Column<List<string>>(type: "text[]", nullable: false),
                    YearLevelId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConceptualOrganisers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConceptualOrganisers_YearLevels_YearLevelId",
                        column: x => x.YearLevelId,
                        principalTable: "YearLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dispositions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    DevelopedWhen = table.Column<List<string>>(type: "text[]", nullable: false),
                    YearLevelId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dispositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dispositions_YearLevels_YearLevelId",
                        column: x => x.YearLevelId,
                        principalTable: "YearLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CalendarSchoolEvent",
                columns: table => new
                {
                    CalendarId = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolEventsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarSchoolEvent", x => new { x.CalendarId, x.SchoolEventsId });
                    table.ForeignKey(
                        name: "FK_CalendarSchoolEvent_Calendar_CalendarId",
                        column: x => x.CalendarId,
                        principalTable: "Calendar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CalendarSchoolEvent_SchoolEvent_SchoolEventsId",
                        column: x => x.SchoolEventsId,
                        principalTable: "SchoolEvent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Assessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    YearLevel = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    AssessmentType = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    PlanningNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DateConducted = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assessments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Assessments_CurriculumSubjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "CurriculumSubjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Assessments_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    YearLevel = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reports_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Reports_CurriculumSubjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "CurriculumSubjects",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Reports_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TermPlan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TermPlannerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TermNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TermPlan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TermPlan_TermPlanners_TermPlannerId",
                        column: x => x.TermPlannerId,
                        principalTable: "TermPlanners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DayPlan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WeekPlannerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    BreakDutyOverrides = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DayPlan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DayPlan_WeekPlanners_WeekPlannerId",
                        column: x => x.WeekPlannerId,
                        principalTable: "WeekPlanners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DayTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Periods = table.Column<string>(type: "text", nullable: false),
                    WeekPlannerTemplateId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DayTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DayTemplates_WeekPlannerTemplates_WeekPlannerTemplateId",
                        column: x => x.WeekPlannerTemplateId,
                        principalTable: "WeekPlannerTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TemplatePeriods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    WeekPlannerTemplateId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplatePeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemplatePeriods_WeekPlannerTemplates_WeekPlannerTemplateId",
                        column: x => x.WeekPlannerTemplateId,
                        principalTable: "WeekPlannerTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContentDescriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConceptualOrganiserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    CurriculumCodes = table.Column<string[]>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentDescriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentDescriptions_ConceptualOrganisers_ConceptualOrganise~",
                        column: x => x.ConceptualOrganiserId,
                        principalTable: "ConceptualOrganisers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Comments = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Grade_Grade = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Percentage = table.Column<double>(type: "double precision", nullable: false),
                    DateMarked = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentResults", x => new { x.Id, x.AssessmentId });
                    table.ForeignKey(
                        name: "FK_AssessmentResults_Assessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalTable: "Assessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Grade = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Comments = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CharacterLimit = table.Column<int>(type: "integer", nullable: false),
                    ReportId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportComments_Reports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "Reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubjectTermPlan",
                columns: table => new
                {
                    SubjectsId = table.Column<Guid>(type: "uuid", nullable: false),
                    TermPlanId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectTermPlan", x => new { x.SubjectsId, x.TermPlanId });
                    table.ForeignKey(
                        name: "FK_SubjectTermPlan_CurriculumSubjects_SubjectsId",
                        column: x => x.SubjectsId,
                        principalTable: "CurriculumSubjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubjectTermPlan_TermPlan_TermPlanId",
                        column: x => x.TermPlanId,
                        principalTable: "TermPlan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DayPlanSchoolEvent",
                columns: table => new
                {
                    DayPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolEventsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DayPlanSchoolEvent", x => new { x.DayPlanId, x.SchoolEventsId });
                    table.ForeignKey(
                        name: "FK_DayPlanSchoolEvent_DayPlan_DayPlanId",
                        column: x => x.DayPlanId,
                        principalTable: "DayPlan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DayPlanSchoolEvent_SchoolEvent_SchoolEventsId",
                        column: x => x.SchoolEventsId,
                        principalTable: "SchoolEvent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LessonPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    YearDataId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanningNotes = table.Column<string>(type: "text", nullable: false),
                    PlanningNotesHtml = table.Column<string>(type: "text", nullable: false),
                    LessonDate = table.Column<DateOnly>(type: "date", nullable: false),
                    NumberOfLessons = table.Column<int>(type: "integer", nullable: false),
                    StartPeriod = table.Column<int>(type: "integer", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DayPlanId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonPlans_CurriculumSubjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "CurriculumSubjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LessonPlans_DayPlan_DayPlanId",
                        column: x => x.DayPlanId,
                        principalTable: "DayPlan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LessonPlans_YearData_YearDataId",
                        column: x => x.YearDataId,
                        principalTable: "YearData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LessonComment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LessonPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Completed = table.Column<bool>(type: "boolean", nullable: false),
                    StruckOut = table.Column<bool>(type: "boolean", nullable: false),
                    CompletedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonComment", x => new { x.Id, x.LessonPlanId });
                    table.ForeignKey(
                        name: "FK_LessonComment_LessonPlans_LessonPlanId",
                        column: x => x.LessonPlanId,
                        principalTable: "LessonPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LessonPlanResource",
                columns: table => new
                {
                    LessonPlansId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourcesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonPlanResource", x => new { x.LessonPlansId, x.ResourcesId });
                    table.ForeignKey(
                        name: "FK_LessonPlanResource_LessonPlans_LessonPlansId",
                        column: x => x.LessonPlansId,
                        principalTable: "LessonPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LessonPlanResource_Resources_ResourcesId",
                        column: x => x.ResourcesId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentResults_AssessmentId",
                table: "AssessmentResults",
                column: "AssessmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_StudentId",
                table: "Assessments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_SubjectId",
                table: "Assessments",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_UserId",
                table: "Assessments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarSchoolEvent_SchoolEventsId",
                table: "CalendarSchoolEvent",
                column: "SchoolEventsId");

            migrationBuilder.CreateIndex(
                name: "IX_Capabilities_YearLevelId",
                table: "Capabilities",
                column: "YearLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_ConceptualOrganisers_YearLevelId",
                table: "ConceptualOrganisers",
                column: "YearLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentDescriptions_ConceptualOrganiserId",
                table: "ContentDescriptions",
                column: "ConceptualOrganiserId");

            migrationBuilder.CreateIndex(
                name: "IX_DayPlan_WeekPlannerId",
                table: "DayPlan",
                column: "WeekPlannerId");

            migrationBuilder.CreateIndex(
                name: "IX_DayPlanSchoolEvent_SchoolEventsId",
                table: "DayPlanSchoolEvent",
                column: "SchoolEventsId");

            migrationBuilder.CreateIndex(
                name: "IX_DayTemplates_WeekPlannerTemplateId",
                table: "DayTemplates",
                column: "WeekPlannerTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Dispositions_YearLevelId",
                table: "Dispositions",
                column: "YearLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonComment_LessonPlanId",
                table: "LessonComment",
                column: "LessonPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonPlanResource_ResourcesId",
                table: "LessonPlanResource",
                column: "ResourcesId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonPlans_DayPlanId",
                table: "LessonPlans",
                column: "DayPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonPlans_SubjectId",
                table: "LessonPlans",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonPlans_YearDataId",
                table: "LessonPlans",
                column: "YearDataId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportComments_ReportId",
                table: "ReportComments",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_StudentId",
                table: "Reports",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_SubjectId",
                table: "Reports",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_UserId",
                table: "Reports",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_SubjectId",
                table: "Resources",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_UserId",
                table: "Resources",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolEvent_LocationId",
                table: "SchoolEvent",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_UserId",
                table: "Students",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_YearDataId",
                table: "Students",
                column: "YearDataId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectTermPlan_TermPlanId",
                table: "SubjectTermPlan",
                column: "TermPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectYearData_YearDataId",
                table: "SubjectYearData",
                column: "YearDataId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplatePeriods_WeekPlannerTemplateId",
                table: "TemplatePeriods",
                column: "WeekPlannerTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_TermPlan_TermPlannerId",
                table: "TermPlan",
                column: "TermPlannerId");

            migrationBuilder.CreateIndex(
                name: "IX_TermPlanners_YearDataId",
                table: "TermPlanners",
                column: "YearDataId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WeekPlanners_YearDataId",
                table: "WeekPlanners",
                column: "YearDataId");

            migrationBuilder.CreateIndex(
                name: "IX_WeekPlannerTemplates_UserId",
                table: "WeekPlannerTemplates",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WeekPlannerTemplates_YearDataId",
                table: "WeekPlannerTemplates",
                column: "YearDataId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_YearData_UserId",
                table: "YearData",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_YearLevels_SubjectId",
                table: "YearLevels",
                column: "SubjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AssessmentResults");

            migrationBuilder.DropTable(
                name: "CalendarSchoolEvent");

            migrationBuilder.DropTable(
                name: "Capabilities");

            migrationBuilder.DropTable(
                name: "ContentDescriptions");

            migrationBuilder.DropTable(
                name: "DayPlanSchoolEvent");

            migrationBuilder.DropTable(
                name: "DayTemplates");

            migrationBuilder.DropTable(
                name: "Dispositions");

            migrationBuilder.DropTable(
                name: "LessonComment");

            migrationBuilder.DropTable(
                name: "LessonPlanResource");

            migrationBuilder.DropTable(
                name: "ReportComments");

            migrationBuilder.DropTable(
                name: "SubjectTermPlan");

            migrationBuilder.DropTable(
                name: "SubjectYearData");

            migrationBuilder.DropTable(
                name: "TemplatePeriods");

            migrationBuilder.DropTable(
                name: "TermDates");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Assessments");

            migrationBuilder.DropTable(
                name: "Calendar");

            migrationBuilder.DropTable(
                name: "ConceptualOrganisers");

            migrationBuilder.DropTable(
                name: "SchoolEvent");

            migrationBuilder.DropTable(
                name: "LessonPlans");

            migrationBuilder.DropTable(
                name: "Resources");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "TermPlan");

            migrationBuilder.DropTable(
                name: "WeekPlannerTemplates");

            migrationBuilder.DropTable(
                name: "YearLevels");

            migrationBuilder.DropTable(
                name: "locations");

            migrationBuilder.DropTable(
                name: "DayPlan");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "TermPlanners");

            migrationBuilder.DropTable(
                name: "CurriculumSubjects");

            migrationBuilder.DropTable(
                name: "WeekPlanners");

            migrationBuilder.DropTable(
                name: "YearData");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
