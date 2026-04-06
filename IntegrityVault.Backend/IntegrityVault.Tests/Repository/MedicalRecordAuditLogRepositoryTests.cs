using IntegrityVault.Repository.Contexts;

namespace IntegrityVault.Tests.Repository
{
    public class MedicalRecordAuditLogRepositoryTests
    {
        private readonly IntegrityVaultDbContext _context;
        private readonly MedicalRecordAuditLogRepository _repository;


        public MedicalRecordAuditLogRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<IntegrityVaultDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new IntegrityVaultDbContext(options);
            _repository = new MedicalRecordAuditLogRepository(_context);
        }

        #region Helpers


        private static MedicalRecordAuditLog BuildTestAuditLog(int id = 1, int recordId = 100, string prevCid = "QmOld", string newCid = "QmNew") => new()
        {
            ID = id,
            RecordID = recordId,
            UpdatedByDoctorID = 11,
            PreviousIPFS_CID = prevCid,
            NewIPFS_CID = newCid,
            Version = 2,
            UpdatedAt = DateTime.UtcNow,
            NewContentHash = "HashNew",
            NewVersionHash = "VHashNew",
            PreviousContentHash = "HashOld",
            PreviousVersionHash = "VHashOld"
        };

        #endregion



        [Fact]
        public async Task GetAllVersionOfMedicalRecordByID_ShouldReturnAllLogsForRecord()
        {
            // Arrange.
            _context.MedicalRecordsAuditLogs.AddRange(
                BuildTestAuditLog(1, 100),
                BuildTestAuditLog(2, 100),
                BuildTestAuditLog(3, 200)
            );
            await _context.SaveChangesAsync();

            // Act.
            var result = await _repository.GetAllVersionOfMedicalRecordByID(100);

            // Assert.
            Assert.Equal(2, result.Count);
            Assert.All(result, log => Assert.Equal(100, log.RecordID));
        }


        [Fact]
        public async Task InsertAuditLog_ShouldAddRecordToDatabase()
        {
            var excepted_CID = "QmYo2LcmU5o5ifUuRm3QQzdknuSmmD1TsgwbgKtmsVPjze";

            // Arrange.
            var dto = new CreateMedicalRecordAuditDTO
            {
                RecordID = 500,
                UpdatedByDoctorID = 11,
                PreviousIPFS_CID = "QmdSPMyJF8LZRkmT1F2MYYZ4m9Bbx6FLdtJK1hBreUwn5U",
                NewIPFS_CID = excepted_CID,
                PreviousContentHash = "1a0205de12779f82d57ff22aa243c6c77188a1cb7fa5ca4adcaf74ed221d2458",
                NewContentHash = "669f3c4f9347bd39f3bda980abb528efbb0a1b9ff1da893e19a62e9e6bf311fe",
                PreviousVersionHash = "b3cb7581b9abcc4d25237fb2820d4268e98131a617b425b7c0a088c21ca456b1",
                NewVersionHash = "386a25b3aeb02e6e83652a5050d4d2b1951b8f35b4317e8918eddf581d3847d3",
                BlockchainTxHash = "0x0fbc19894eb7609216369b7d1fb7b19091f6767d98e656429d8d5ff9ae45fd4c",
                Version = 1,
                UpdatedAt = DateTime.UtcNow
            };

            // Act.
            var result = await _repository.InsertAuditLog(dto);
            await _context.SaveChangesAsync(); // Repository adds, we save.

            // Assert.
            Assert.True(result);
            var dbLog = await _context.MedicalRecordsAuditLogs.FirstOrDefaultAsync(l => l.RecordID == 500);
            Assert.NotNull(dbLog);
            Assert.Equal(excepted_CID, dbLog.NewIPFS_CID);
        }


        [Fact]
        public async Task GetAuditLogByNewCIDAsync_ShouldReturnCorrectLog()
        {
            // Arrange.
            var log = BuildTestAuditLog(1, 100, "QmPrev", "QmTargetNew");
            _context.MedicalRecordsAuditLogs.Add(log);
            await _context.SaveChangesAsync();

            // Act.
            var result = await _repository.GetAuditLogByNewCIDAsync("QmTargetNew");

            // Assert.
            Assert.NotNull(result);
            Assert.Equal(100, result.RecordID);
            Assert.Equal("QmPrev", result.PreviousIPFS_CID);
        }


        [Fact]
        public async Task GetAuditLogByPreviousCIDAsync_ShouldReturnCorrectLog()
        {
            // Arrange.
            var log = BuildTestAuditLog(1, 100, "QmTargetOld", "QmNext");
            _context.MedicalRecordsAuditLogs.Add(log);
            await _context.SaveChangesAsync();

            // Act.
            var result = await _repository.GetAuditLogByPreviousCIDAsync("QmTargetOld");

            // Assert.
            Assert.NotNull(result);
            Assert.Equal("QmNext", result.NewIPFS_CID);
        }


        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task CIDLookupMethods_ShouldReturnNull_WhenCIDIsInvalid(string? invalidCid)
        {
            // Act.
            var resultNew = await _repository.GetAuditLogByNewCIDAsync(invalidCid!);
            var resultPrev = await _repository.GetAuditLogByPreviousCIDAsync(invalidCid!);

            // Assert.
            Assert.Null(resultNew);
            Assert.Null(resultPrev);
        }


        [Fact]
        public async Task GetAllVersionOfMedicalRecordByID_ShouldReturnEmptyList_WhenNoLogsExist()
        {
            // Act.
            var result = await _repository.GetAllVersionOfMedicalRecordByID(999);

            // Assert.
            Assert.Empty(result);
        }
    }
}