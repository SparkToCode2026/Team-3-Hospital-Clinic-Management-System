using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Team_3_HMS.Migrations
{
    /// <inheritdoc />
    public partial class intial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MedicalRecords",
                columns: table => new
                {
                    MedicalRecordID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Diagnosis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TreatmentPlan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Symptom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecordDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AppointmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalRecords", x => x.MedicalRecordID);
                });

            migrationBuilder.CreateTable(
                name: "Medications",
                columns: table => new
                {
                    MedicationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GenericName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DosageForm = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UnitPrice = table.Column<double>(type: "float", nullable: false),
                    PrescriptionID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medications", x => x.MedicationID);
                });

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    RoomId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoomNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.RoomId);
                });

            migrationBuilder.CreateTable(
                name: "Specializations",
                columns: table => new
                {
                    SpecializationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DoctorProfileId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specializations", x => x.SpecializationId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    userID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fullname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.userID);
                });

            migrationBuilder.CreateTable(
                name: "LabTests",
                columns: table => new
                {
                    LabTestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TestName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TestDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cost = table.Column<double>(type: "float", nullable: false),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MedicalRecordId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabTests", x => x.LabTestId);
                    table.ForeignKey(
                        name: "FK_LabTests_MedicalRecords_MedicalRecordId",
                        column: x => x.MedicalRecordId,
                        principalTable: "MedicalRecords",
                        principalColumn: "MedicalRecordID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Prescriptions",
                columns: table => new
                {
                    PrescriptionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IssuedDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Instructions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MedicalRecordID = table.Column<int>(type: "int", nullable: false),
                    MedicalRecordId = table.Column<int>(type: "int", nullable: false),
                    MedicationID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prescriptions", x => x.PrescriptionID);
                    table.ForeignKey(
                        name: "FK_Prescriptions_MedicalRecords_MedicalRecordId",
                        column: x => x.MedicalRecordId,
                        principalTable: "MedicalRecords",
                        principalColumn: "MedicalRecordID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DoctorProfiles",
                columns: table => new
                {
                    DoctorProfileId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LicenseNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    YearsOfExperience = table.Column<int>(type: "int", nullable: false),
                    ConsultationFee = table.Column<double>(type: "float", nullable: false),
                    userID = table.Column<int>(type: "int", nullable: false),
                    SpecializationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorProfiles", x => x.DoctorProfileId);
                    table.ForeignKey(
                        name: "FK_DoctorProfiles_Users_userID",
                        column: x => x.userID,
                        principalTable: "Users",
                        principalColumn: "userID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientProfiles",
                columns: table => new
                {
                    PatientProfileID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BloodGroup = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    emergencyContact = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    userID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientProfiles", x => x.PatientProfileID);
                    table.ForeignKey(
                        name: "FK_PatientProfiles_Users_userID",
                        column: x => x.userID,
                        principalTable: "Users",
                        principalColumn: "userID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicationPrescription",
                columns: table => new
                {
                    MedicationsMedicationID = table.Column<int>(type: "int", nullable: false),
                    PrescriptionsPrescriptionID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicationPrescription", x => new { x.MedicationsMedicationID, x.PrescriptionsPrescriptionID });
                    table.ForeignKey(
                        name: "FK_MedicationPrescription_Medications_MedicationsMedicationID",
                        column: x => x.MedicationsMedicationID,
                        principalTable: "Medications",
                        principalColumn: "MedicationID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicationPrescription_Prescriptions_PrescriptionsPrescriptionID",
                        column: x => x.PrescriptionsPrescriptionID,
                        principalTable: "Prescriptions",
                        principalColumn: "PrescriptionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    DepartmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BuildingLocation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DoctorProfileId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.DepartmentId);
                    table.ForeignKey(
                        name: "FK_Departments_DoctorProfiles_DoctorProfileId",
                        column: x => x.DoctorProfileId,
                        principalTable: "DoctorProfiles",
                        principalColumn: "DoctorProfileId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DoctorProfileSpecialization",
                columns: table => new
                {
                    doctorsDoctorProfileId = table.Column<int>(type: "int", nullable: false),
                    specializationsSpecializationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorProfileSpecialization", x => new { x.doctorsDoctorProfileId, x.specializationsSpecializationId });
                    table.ForeignKey(
                        name: "FK_DoctorProfileSpecialization_DoctorProfiles_doctorsDoctorProfileId",
                        column: x => x.doctorsDoctorProfileId,
                        principalTable: "DoctorProfiles",
                        principalColumn: "DoctorProfileId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DoctorProfileSpecialization_Specializations_specializationsSpecializationId",
                        column: x => x.specializationsSpecializationId,
                        principalTable: "Specializations",
                        principalColumn: "SpecializationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    AppointmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AppointmentDateTime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReasonForVisit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PatientProfileID = table.Column<int>(type: "int", nullable: false),
                    DoctorProfileId = table.Column<int>(type: "int", nullable: false),
                    RoomId = table.Column<int>(type: "int", nullable: false),
                    MedicalRecordID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.AppointmentId);
                    table.ForeignKey(
                        name: "FK_Appointments_DoctorProfiles_DoctorProfileId",
                        column: x => x.DoctorProfileId,
                        principalTable: "DoctorProfiles",
                        principalColumn: "DoctorProfileId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Appointments_MedicalRecords_MedicalRecordID",
                        column: x => x.MedicalRecordID,
                        principalTable: "MedicalRecords",
                        principalColumn: "MedicalRecordID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Appointments_PatientProfiles_PatientProfileID",
                        column: x => x.PatientProfileID,
                        principalTable: "PatientProfiles",
                        principalColumn: "PatientProfileID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Appointments_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "RoomId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    InvoiceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TotalAmount = table.Column<double>(type: "float", nullable: false),
                    Paymentmethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IssuedDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AppointmentID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.InvoiceID);
                    table.ForeignKey(
                        name: "FK_Invoices_Appointments_AppointmentID",
                        column: x => x.AppointmentID,
                        principalTable: "Appointments",
                        principalColumn: "AppointmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DoctorProfileId",
                table: "Appointments",
                column: "DoctorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_MedicalRecordID",
                table: "Appointments",
                column: "MedicalRecordID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PatientProfileID",
                table: "Appointments",
                column: "PatientProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_RoomId",
                table: "Appointments",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_DoctorProfileId",
                table: "Departments",
                column: "DoctorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorProfiles_userID",
                table: "DoctorProfiles",
                column: "userID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DoctorProfileSpecialization_specializationsSpecializationId",
                table: "DoctorProfileSpecialization",
                column: "specializationsSpecializationId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_AppointmentID",
                table: "Invoices",
                column: "AppointmentID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabTests_MedicalRecordId",
                table: "LabTests",
                column: "MedicalRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationPrescription_PrescriptionsPrescriptionID",
                table: "MedicationPrescription",
                column: "PrescriptionsPrescriptionID");

            migrationBuilder.CreateIndex(
                name: "IX_PatientProfiles_userID",
                table: "PatientProfiles",
                column: "userID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_MedicalRecordId",
                table: "Prescriptions",
                column: "MedicalRecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "DoctorProfileSpecialization");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "LabTests");

            migrationBuilder.DropTable(
                name: "MedicationPrescription");

            migrationBuilder.DropTable(
                name: "Specializations");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "Medications");

            migrationBuilder.DropTable(
                name: "Prescriptions");

            migrationBuilder.DropTable(
                name: "DoctorProfiles");

            migrationBuilder.DropTable(
                name: "PatientProfiles");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropTable(
                name: "MedicalRecords");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
