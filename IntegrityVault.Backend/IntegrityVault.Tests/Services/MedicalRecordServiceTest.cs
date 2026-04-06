using System.Security.Cryptography;
using System.Text;
using IntegrityVault.Repository.Contexts;
using Microsoft.EntityFrameworkCore.Diagnostics;


namespace IntegrityVault.Tests.Services
{

    public class MedicalRecordServiceTests
    {
        private readonly Mock<IMedicalRecordRepository> _medicalRecordRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IEpisodeRepository> _episodeRepoMock;
        private readonly Mock<IHospitalRepository> _hospitalRepoMock;
        private readonly Mock<IMedicalRecordAuditLogRepository> _auditLogRepoMock;
        private readonly Mock<IRecordAccessLogRepository> _accessLogRepoMock;
        private readonly Mock<IIPFSService> _ipfsServiceMock;
        private readonly Mock<IBlockchainService> _blockchainServiceMock;
        private readonly Mock<IPDFService> _pdfServiceMock;
        private readonly Mock<ICryptoService> _cryptoServiceMock;
        private readonly IntegrityVaultDbContext _context;
        private readonly MedicalRecordService _service;


        public MedicalRecordServiceTests()
        {
            _medicalRecordRepoMock = new Mock<IMedicalRecordRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _episodeRepoMock = new Mock<IEpisodeRepository>();
            _hospitalRepoMock = new Mock<IHospitalRepository>();
            _auditLogRepoMock = new Mock<IMedicalRecordAuditLogRepository>();
            _accessLogRepoMock = new Mock<IRecordAccessLogRepository>();
            _ipfsServiceMock = new Mock<IIPFSService>();
            _blockchainServiceMock = new Mock<IBlockchainService>();
            _pdfServiceMock = new Mock<IPDFService>();
            _cryptoServiceMock = new Mock<ICryptoService>();


            var options = new DbContextOptionsBuilder<IntegrityVaultDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            _context = new IntegrityVaultDbContext(options);


            _service = new MedicalRecordService(
                _medicalRecordRepoMock.Object,
                _episodeRepoMock.Object,
                _accessLogRepoMock.Object,
                _auditLogRepoMock.Object,
                _userRepoMock.Object,
                _hospitalRepoMock.Object,
                _pdfServiceMock.Object,
                _cryptoServiceMock.Object,
                _ipfsServiceMock.Object,
                _blockchainServiceMock.Object,
                _context
            );
        }



        #region Helpers (The Style)

        private static MedicalRecordPdfDataDTO BuildPdfDto(int doctorId = 1, int patientId = 10, int hospitalId = 100) => new()
        {
            DoctorID = doctorId,
            PatientID = patientId,
            HospitalID = hospitalId,
            DoctorFirstName = "Greg",
            DoctorLastName = "House",
            DoctorSpecialy = DoctorSpecialty.GeneralMedicine,
            PatientFirstName = "John",
            PatientLastName = "Doe",
            PatientGender = PatientGender.Male,
            HospitalName = "Princeton-Plainsboro",
            ChiefComplaint = "Headache",
            Diagnosis = "Migraine",
            TreatmentPlan = "Rest",
            DoctorNotes = "None",
            FollowUpInstructions = "Drink water"
        };

        private static Doctor BuildDoctor(int id = 1, int hospitalId = 101) => new()
        {
            ID = id,
            HospitalID = hospitalId,
            Username = "dr.house",
            FirstName = "Greg",
            LastName = "House",
            Role = UserRole.Doctor,
            Email = "house@integrityvault.com",
            Password = "hashed_password",
            Specialty = DoctorSpecialty.GeneralMedicine
        };

        private static Patient BuildPatient(int id = 2, int hospitalId = 101) => new()
        {
            ID = id,
            HospitalID = hospitalId,
            Username = "wilson.james",
            FirstName = "Wilson",
            LastName = "James",
            Gender = PatientGender.Male,
            Email = "wilson.james@integrityvault.com",
            Password = "hashed_password",
            Role = UserRole.Patient,
            DOB = new DateOnly(1990, 1, 1)
        };

        private static Hospital BuildHospital(int id = 101) => new()
        {
            ID = id,
            Name = "Princeton-Plainsboro",
            WalletAddress = "0xHospitalWallet",
            EncryptedPrivateKey = Encoding.UTF8.GetBytes("key")
        };

        private static MedicalRecord BuildRecord(int id = 500, string cid = "QmTest123") => new()
        {
            ID = id,
            IPFS_CID = cid,
            EpisodeID = 1,
            ContentHash = Convert.ToHexString(new byte[32]).ToLower(),
            VersionHash = Convert.ToHexString(new byte[32]).ToLower(),
            VisitDate = new DateOnly(1999, 1, 1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        #endregion



        [Fact]
        public async Task GetMedicalRecordInformationFromCIDAsync_ShouldReturnData_WhenUserIsAuthor()
        {
            // Arrange
            string cid = "QmTest123";
            int doctorId = 1;
            var pdfBytes = Encoding.UTF8.GetBytes("Fake PDF Content");
            var embeddedDto = BuildPdfDto(doctorId: doctorId);

            _ipfsServiceMock.Setup(s => s.GetFileAsync(cid)).ReturnsAsync(pdfBytes);
            _pdfServiceMock.Setup(s => s.ExtractJsonFromPdf(pdfBytes)).Returns(embeddedDto);
            _userRepoMock.Setup(s => s.GetUserByIdAsync(doctorId)).ReturnsAsync(BuildDoctor(doctorId));
            _medicalRecordRepoMock.Setup(r => r.GetMedicalRecordByCIDAsync(cid)).ReturnsAsync(BuildRecord(cid: cid));

            // Act
            var result = await _service.GetMedicalRecordInformationFromCIDAsync(cid, doctorId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(doctorId, result.DoctorID);
            Assert.Equal("Migraine", result.Diagnosis);
        }


        [Fact]
        public async Task DownloadMedicalRecordAsync_ShouldThrow_WhenUserIsNotThePatient()
        {
            // Arrange
            string cid = "QmTest123";
            int wrongUserId = 99;
            var pdfBytes = Encoding.UTF8.GetBytes("Fake PDF Content");
            var embeddedDto = BuildPdfDto(patientId: 10);

            _ipfsServiceMock.Setup(s => s.GetFileAsync(cid)).ReturnsAsync(pdfBytes);
            _pdfServiceMock.Setup(s => s.ExtractJsonFromPdf(pdfBytes)).Returns(embeddedDto);
            _userRepoMock.Setup(s => s.GetUserByIdAsync(wrongUserId)).ReturnsAsync(BuildPatient(wrongUserId));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DownloadMedicalRecordAsync(cid, wrongUserId));
            Assert.Contains("You may only download your own medical records", ex.Message);
        }


        [Fact]
        public async Task ExecuteMedicalRecordCreationAsync_ShouldOrchestrateFullChain()
        {
            var doctor = BuildDoctor();
            var patient = BuildPatient();
            var hospital = BuildHospital();
            var createDto = new CreateMedicalRecordDTO
            {
                DoctorID = doctor.ID,
                PatientID = patient.ID,
                ChiefComplaint = "Lupus",
                Diagnosis = "Undetermined",
                TreatmentPlan = "Further tests"
            };

            _userRepoMock.Setup(r => r.GetDoctorByIdAsync(doctor.ID)).ReturnsAsync(doctor);
            _userRepoMock.Setup(r => r.GetPatientByIdAsync(patient.ID)).ReturnsAsync(patient);
            _hospitalRepoMock.Setup(r => r.GetHospitalByIdAsync(hospital.ID)).ReturnsAsync(hospital);

            _pdfServiceMock.Setup(s => s.GeneratePDF(It.IsAny<MedicalRecordPdfDataDTO>(), It.IsAny<byte[]>()))
              .Returns(Encoding.UTF8.GetBytes("GeneratedPDF"));

            _cryptoServiceMock.Setup(s => s.Encrypt(It.IsAny<string>())).Returns(Encoding.UTF8.GetBytes("encrypted"));
            _ipfsServiceMock.Setup(s => s.AddFileAsync(It.IsAny<byte[]>())).ReturnsAsync("QmNewCid");

            _medicalRecordRepoMock.Setup(r => r.CreateMedicalRecordAsync(It.IsAny<MedicalRecord>()))
              .Callback<MedicalRecord>(r => r.ID = 777)
              .ReturnsAsync(new MedicalRecordIdDTO { ID = 777 });

            _medicalRecordRepoMock.Setup(r => r.PatchMedicalRecordAsync(It.IsAny<int>(), It.IsAny<MedicalRecordPatchDTO>()))
              .ReturnsAsync(true);
            _accessLogRepoMock.Setup(r => r.CreateRecordAccessLogAsync(It.IsAny<RecordAccessLog>()))
              .ReturnsAsync(true);
            _blockchainServiceMock.Setup(b => b.RegisterRecordOnChainAsync(
              It.IsAny<int>(),
              It.IsAny<int>(),
              It.IsAny<int>(),
              It.IsAny<byte[]>(),
              It.IsAny<byte[]>(),
              It.IsAny<string>()))
              .ReturnsAsync("0xTxHash");

            _episodeRepoMock.Setup(r => r.CreateEpisodeAsync(It.IsAny<Episode>())).ReturnsAsync(new EpisodeIdDTO { ID = 1 });

            var result = await _service.CreateMedicalRecordAndEpisodeAsync(createDto);

            Assert.True(result);
            _blockchainServiceMock.Verify(b => b.RegisterRecordOnChainAsync(
              It.IsAny<int>(), 777, It.IsAny<int>(), It.IsAny<byte[]>(), It.IsAny<byte[]>(), "QmNewCid"), Times.Once);
        }


        [Fact]
        public void ComputeVersionHash_ShouldProduceConsistentOutput()
        {
            // Arrange
            var contentHash = SHA256.HashData(Encoding.UTF8.GetBytes("SampleContent"));
            var prevHash = new byte[32];
            int version = 1;
            int recordId = 55;

            var method = typeof(MedicalRecordService).GetMethod("ComputeVersionHash", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            // Act
            var hash1 = (byte[]?)method?.Invoke(null, new object[] { contentHash, prevHash, version, recordId });
            var hash2 = (byte[]?)method?.Invoke(null, new object[] { contentHash, prevHash, version, recordId });

            // Assert
            Assert.NotNull(hash1);
            Assert.Equal(Convert.ToHexString(hash1), Convert.ToHexString(hash2!));
        }


        [Fact]
        public async Task ResolveRecordFromCIDAsync_ShouldThrow_WhenCidIsGhost()
        {
            // Arrange
            _medicalRecordRepoMock.Setup(r => r.GetMedicalRecordByCIDAsync(It.IsAny<string>())).ReturnsAsync((MedicalRecord?)null);
            _auditLogRepoMock.Setup(a => a.GetAuditLogByNewCIDAsync(It.IsAny<string>())).ReturnsAsync((MedicalRecordAuditLog?)null);
            _auditLogRepoMock.Setup(a => a.GetAuditLogByPreviousCIDAsync(It.IsAny<string>())).ReturnsAsync((MedicalRecordAuditLog?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DownloadMedicalRecordAsync("ghost_cid", 1));
        }


        [Theory]
        [InlineData(UserRole.Doctor, 1, 10, true)]   // Dr House (ID 1) created it
        [InlineData(UserRole.Doctor, 2, 10, false)]  // Dr Chase (ID 2) trying to peek
        [InlineData(UserRole.Patient, 10, 10, true)] // Patient John Doe looking at his own
        [InlineData(UserRole.Patient, 11, 10, false)] // Patient Wilson looking at Doe's
        public async Task CheckAccessAsync_LogicVerification(UserRole role, int requestUserId, int recordOwnerId, bool expectedResult)
        {
            // Arrange
            var embeddedDto = BuildPdfDto(doctorId: 1, patientId: recordOwnerId);
            var user = role == UserRole.Doctor ? (User)BuildDoctor(requestUserId) : (User)BuildPatient(requestUserId);

            _userRepoMock.Setup(u => u.GetUserByIdAsync(requestUserId)).ReturnsAsync(user);

            var method = typeof(MedicalRecordService).GetMethod("CheckAccessAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var resultTask = (Task<(bool allowed, string reason)>?)method?.Invoke(_service, new object[] { requestUserId, embeddedDto });
            var (allowed, _) = await resultTask!;

            // Assert
            Assert.Equal(expectedResult, allowed);
        }


        [Fact]
        public void BuildVerifyResult_ShouldReportTampering_WhenHashesDontAlign()
        {
            // Arrange
            var method = typeof(MedicalRecordService).GetMethod("BuildVerifyResult", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            // Act
            // Arguments: isTampered, contentHashMatch, databaseHashMatch, cidMatch, versionHashMatch
            var result = (VerifyMedicalRecordDTO?)method?.Invoke(null, new object[] { true, false, true, true, true });

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsTampered);
            Assert.Equal("Tampered", result.Status);
            Assert.Contains("ContentHash mismatch", result.Message);
        }
    }
}