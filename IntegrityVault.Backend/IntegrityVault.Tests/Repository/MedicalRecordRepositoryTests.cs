using IntegrityVault.Repository.Contexts;


namespace IntegrityVault.Tests.Repository
{
    public class MedicalRecordRepositoryTests
    {
        private readonly IntegrityVaultDbContext _context;
        private readonly MedicalRecordRepository _repository;

        public MedicalRecordRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<IntegrityVaultDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new IntegrityVaultDbContext(options);
            _repository = new MedicalRecordRepository(_context);
        }

        #region Helpers


        private static MedicalRecord BuildTestRecord(int id = 1, string cid = "QmPrimary", int episodeId = 10) => new()
        {
            ID = id,
            IPFS_CID = cid,
            CurrentVersion = 1,
            ContentHash = "Hash123",
            VersionHash = "VHash123",
            EpisodeID = episodeId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            VisitDate = new DateOnly(2023, 1, 1)
        };

        private static Episode BuildTestEpisode(int id, int patientId, int doctorId) => new()
        {
            ID = id,
            PatientID = patientId,
            DoctorID = doctorId,
            IsActive = true,
            Title = "Test Episode",
            Specialty = DoctorSpecialty.GeneralMedicine,
            CreatedAt = DateTime.UtcNow
        };

        #endregion



        [Fact]
        public async Task GetMedicalRecordById_ShouldReturnRecord_WhenExists()
        {
            // Arrange.
            var record = BuildTestRecord(id: 5);
            _context.MedicalRecords.Add(record);
            await _context.SaveChangesAsync();

            // Act.
            var result = await _repository.GetMedicalRecordById(5);

            // Assert.
            Assert.NotNull(result);
            Assert.Equal("QmPrimary", result!.IPFS_CID);
        }


        [Fact]
        public async Task CreateMedicalRecordAsync_ShouldAddAndReturnIdDTO()
        {
            // Arrange.
            var record = BuildTestRecord(id: 100);

            // Act.
            var result = await _repository.CreateMedicalRecordAsync(record);
            await _context.SaveChangesAsync();

            // Assert.
            Assert.Equal(100, result.ID);
            Assert.NotNull(await _context.MedicalRecords.FindAsync(100));
        }


        [Fact]
        public async Task PatchMedicalRecordAsync_ShouldOnlyUpdateProvidedFields()
        {
            // Arrange.
            var record = BuildTestRecord(id: 1, cid: "OldCID");
            _context.MedicalRecords.Add(record);
            await _context.SaveChangesAsync();

            var patchDto = new MedicalRecordPatchDTO
            {
                IPFS_CID = "NewCID",
                CurrentVersion = 2
            };

            // Act.
            var success = await _repository.PatchMedicalRecordAsync(1, patchDto);
            await _context.SaveChangesAsync();

            // Assert.
            var updated = await _context.MedicalRecords.FindAsync(1);
            Assert.True(success);
            Assert.Equal("NewCID", updated!.IPFS_CID);
            Assert.Equal(2, updated.CurrentVersion);
            Assert.Equal("Hash123", updated.ContentHash);
        }


        [Fact]
        public async Task GetMedicalRecordsByPatientIDAsync_ShouldIncludeRelatedData()
        {
            // Arrange.
            var patientId = 99;
            var episodeId = 10;
            var doctorId = 88;

            // Seed a Doctor so the ThenInclude(e => e!.Doctor) chain resolves correctly.
            var doctor = new Doctor
            {
                ID = doctorId,
                Username = "testdoctor",
                FirstName = "Test",
                LastName = "Doctor",
                Email = "test@hospital.com",
                Password = "hashed",
                Role = UserRole.Doctor,
                Specialty = DoctorSpecialty.GeneralMedicine
            };

            var episode = BuildTestEpisode(id: episodeId, patientId: patientId, doctorId: doctorId);
            var record = BuildTestRecord(id: 1, episodeId: episodeId);

            // Add all three together so EF wires all FK relationships in a single SaveChangesAsync.
            _context.Users.Add(doctor);
            _context.Episodes.Add(episode);
            _context.MedicalRecords.Add(record);
            await _context.SaveChangesAsync();

            // Act.
            var result = await _repository.GetMedicalRecordsByPatientIDAsync(patientId);

            // Assert.
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Equal(patientId, result[0].Episode!.PatientID);
        }


        [Fact]
        public async Task GetMedicalRecordsByDoctorIDAsync_ShouldReturnCorrectRecords()
        {
            // Arrange.
            var ep1 = BuildTestEpisode(1, 101, 77);
            var ep2 = BuildTestEpisode(2, 102, 77);

            var r1 = BuildTestRecord(id: 1, episodeId: 1); r1.Episode = ep1;
            var r2 = BuildTestRecord(id: 2, episodeId: 2); r2.Episode = ep2;

            _context.MedicalRecords.AddRange(r1, r2);
            await _context.SaveChangesAsync();

            // Act.
            var result = await _repository.GetMedicalRecordsByDoctorIDAsync(77);

            // Assert.
            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.Equal(77, r.Episode!.DoctorID));
        }


        [Fact]
        public async Task GetMedicalRecordByCIDAsync_ShouldReturnRecord_WhenCIDMatches()
        {
            // Arrange.
            var record = BuildTestRecord(id: 1, cid: "TargetCID");
            _context.MedicalRecords.Add(record);
            await _context.SaveChangesAsync();

            // Act.
            var result = await _repository.GetMedicalRecordByCIDAsync("TargetCID");

            // Assert.
            Assert.NotNull(result);
            Assert.Equal(1, result!.ID);
        }


        [Fact]
        public async Task PatchMedicalRecordAsync_ShouldThrow_WhenRecordNotFound()
        {
            // Act & Assert.
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _repository.PatchMedicalRecordAsync(999, new MedicalRecordPatchDTO()));
        }
    }
}