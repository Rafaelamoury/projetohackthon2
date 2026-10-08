using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace InovaTrace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Group = table.Column<int>(type: "integer", nullable: false),
                    Team = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DurationWeeks = table.Column<int>(type: "integer", nullable: false),
                    SourceDirectory = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RawConfigurationJson = table.Column<string>(type: "text", nullable: false),
                    ImportedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Activity",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ProjectId = table.Column<string>(type: "character varying(16)", nullable: false),
                    Cycle = table.Column<string>(type: "text", nullable: false),
                    Phase = table.Column<string>(type: "text", nullable: false),
                    DeclaredNature = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Output = table.Column<string>(type: "text", nullable: false),
                    ResponsibleFunction = table.Column<string>(type: "text", nullable: false),
                    EvidenceIds = table.Column<List<string>>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Activity_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Analyses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<string>(type: "character varying(16)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Recommendation = table.Column<int>(type: "integer", nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: false),
                    Limit = table.Column<string>(type: "text", nullable: false),
                    Confidence = table.Column<double>(type: "double precision", nullable: false),
                    Lifecycle = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Analyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Analyses_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssayResult",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectId = table.Column<string>(type: "character varying(16)", nullable: false),
                    EssayId = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: false),
                    Metric = table.Column<string>(type: "text", nullable: false),
                    Operation = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    Base = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    BaseDescription = table.Column<string>(type: "text", nullable: false),
                    RatePercent = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    Unit = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false),
                    Nature = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssayResult", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssayResult_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChronologyEvent",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ProjectId = table.Column<string>(type: "character varying(16)", nullable: false),
                    Date = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: false),
                    Event = table.Column<string>(type: "text", nullable: false),
                    State = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChronologyEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChronologyEvent_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentRecord",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ProjectId = table.Column<string>(type: "character varying(16)", nullable: false),
                    EvidenceId = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    PackagePath = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    DocumentType = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentRecord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentRecord_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Evidence",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ProjectId = table.Column<string>(type: "character varying(16)", nullable: false),
                    EvidenceType = table.Column<string>(type: "text", nullable: false),
                    RelativeFile = table.Column<string>(type: "text", nullable: false),
                    ExpectedContent = table.Column<string>(type: "text", nullable: false),
                    InventoryStatus = table.Column<string>(type: "text", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Evidence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Evidence_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ImportWarning",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectId = table.Column<string>(type: "character varying(16)", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportWarning", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportWarning_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InputRecord",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ProjectId = table.Column<string>(type: "character varying(16)", nullable: false),
                    ContentJson = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InputRecord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InputRecord_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Measurement",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "character varying(16)", nullable: false),
                    EssayId = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: false),
                    Scenario = table.Column<string>(type: "text", nullable: false),
                    Kind = table.Column<string>(type: "text", nullable: false),
                    Metric = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    Numerator = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    Denominator = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    Weight = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    Unit = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Measurement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Measurement_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ObservationNote",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ProjectId = table.Column<string>(type: "character varying(16)", nullable: false),
                    Input = table.Column<string>(type: "text", nullable: false),
                    Reference = table.Column<string>(type: "text", nullable: false),
                    OutputOrSituation = table.Column<string>(type: "text", nullable: false),
                    Scope = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObservationNote", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ObservationNote_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnalysisReview",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnalysisRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    Choice = table.Column<int>(type: "integer", nullable: false),
                    FinalDecision = table.Column<int>(type: "integer", nullable: false),
                    Justification = table.Column<string>(type: "text", nullable: false),
                    AnalystName = table.Column<string>(type: "text", nullable: false),
                    DecidedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisReview", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnalysisReview_Analyses_AnalysisRunId",
                        column: x => x.AnalysisRunId,
                        principalTable: "Analyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditEntry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<string>(type: "character varying(16)", nullable: true),
                    AnalysisRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    At = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditEntry_Analyses_AnalysisRunId",
                        column: x => x.AnalysisRunId,
                        principalTable: "Analyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuditEntry_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Claim",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnalysisRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    ActivityId = table.Column<string>(type: "text", nullable: true),
                    Text = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false),
                    Confidence = table.Column<double>(type: "double precision", nullable: false),
                    EvidenceIds = table.Column<List<string>>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Claim", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Claim_Analyses_AnalysisRunId",
                        column: x => x.AnalysisRunId,
                        principalTable: "Analyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Contradiction",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnalysisRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceA = table.Column<string>(type: "text", nullable: false),
                    SourceB = table.Column<string>(type: "text", nullable: false),
                    ClaimA = table.Column<string>(type: "text", nullable: true),
                    ClaimB = table.Column<string>(type: "text", nullable: true),
                    AffectedCriterion = table.Column<int>(type: "integer", nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    Explanation = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contradiction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contradiction_Analyses_AnalysisRunId",
                        column: x => x.AnalysisRunId,
                        principalTable: "Analyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CriterionEvaluation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnalysisRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    Criterion = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Confidence = table.Column<double>(type: "double precision", nullable: false),
                    Reasoning = table.Column<string>(type: "text", nullable: false),
                    ClaimCodes = table.Column<List<string>>(type: "text[]", nullable: false),
                    SupportingEvidenceIds = table.Column<List<string>>(type: "text[]", nullable: false),
                    ContraryEvidenceIds = table.Column<List<string>>(type: "text[]", nullable: false),
                    MissingEvidenceIds = table.Column<List<string>>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CriterionEvaluation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CriterionEvaluation_Analyses_AnalysisRunId",
                        column: x => x.AnalysisRunId,
                        principalTable: "Analyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MathCheck",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnalysisRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    EssayId = table.Column<string>(type: "text", nullable: false),
                    Metric = table.Column<string>(type: "text", nullable: false),
                    Operation = table.Column<string>(type: "text", nullable: false),
                    Declared = table.Column<string>(type: "text", nullable: false),
                    Recalculated = table.Column<string>(type: "text", nullable: false),
                    Confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    Detail = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MathCheck", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MathCheck_Analyses_AnalysisRunId",
                        column: x => x.AnalysisRunId,
                        principalTable: "Analyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MissingEvidence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnalysisRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    AffectedCriterion = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    NeededEvidence = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissingEvidence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MissingEvidence_Analyses_AnalysisRunId",
                        column: x => x.AnalysisRunId,
                        principalTable: "Analyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Activity_ProjectId",
                table: "Activity",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Analyses_ProjectId",
                table: "Analyses",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisReview_AnalysisRunId",
                table: "AnalysisReview",
                column: "AnalysisRunId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssayResult_ProjectId_EssayId",
                table: "AssayResult",
                columns: new[] { "ProjectId", "EssayId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntry_AnalysisRunId",
                table: "AuditEntry",
                column: "AnalysisRunId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntry_ProjectId",
                table: "AuditEntry",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ChronologyEvent_ProjectId",
                table: "ChronologyEvent",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Claim_AnalysisRunId_Code",
                table: "Claim",
                columns: new[] { "AnalysisRunId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contradiction_AnalysisRunId",
                table: "Contradiction",
                column: "AnalysisRunId");

            migrationBuilder.CreateIndex(
                name: "IX_CriterionEvaluation_AnalysisRunId",
                table: "CriterionEvaluation",
                column: "AnalysisRunId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentRecord_ProjectId",
                table: "DocumentRecord",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Evidence_ProjectId",
                table: "Evidence",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportWarning_ProjectId",
                table: "ImportWarning",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_InputRecord_ProjectId",
                table: "InputRecord",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_MathCheck_AnalysisRunId",
                table: "MathCheck",
                column: "AnalysisRunId");

            migrationBuilder.CreateIndex(
                name: "IX_Measurement_ProjectId",
                table: "Measurement",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_MissingEvidence_AnalysisRunId",
                table: "MissingEvidence",
                column: "AnalysisRunId");

            migrationBuilder.CreateIndex(
                name: "IX_ObservationNote_ProjectId",
                table: "ObservationNote",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Activity");

            migrationBuilder.DropTable(
                name: "AnalysisReview");

            migrationBuilder.DropTable(
                name: "AssayResult");

            migrationBuilder.DropTable(
                name: "AuditEntry");

            migrationBuilder.DropTable(
                name: "ChronologyEvent");

            migrationBuilder.DropTable(
                name: "Claim");

            migrationBuilder.DropTable(
                name: "Contradiction");

            migrationBuilder.DropTable(
                name: "CriterionEvaluation");

            migrationBuilder.DropTable(
                name: "DocumentRecord");

            migrationBuilder.DropTable(
                name: "Evidence");

            migrationBuilder.DropTable(
                name: "ImportWarning");

            migrationBuilder.DropTable(
                name: "InputRecord");

            migrationBuilder.DropTable(
                name: "MathCheck");

            migrationBuilder.DropTable(
                name: "Measurement");

            migrationBuilder.DropTable(
                name: "MissingEvidence");

            migrationBuilder.DropTable(
                name: "ObservationNote");

            migrationBuilder.DropTable(
                name: "Analyses");

            migrationBuilder.DropTable(
                name: "Projects");
        }
    }
}
