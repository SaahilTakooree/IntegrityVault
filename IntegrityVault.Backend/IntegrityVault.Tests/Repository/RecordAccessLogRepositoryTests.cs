using IntegrityVault.Repository.Contexts;

namespace IntegrityVault.Tests.Repository
{
    public class RecordAccessLogRepositoryTests
    {
        private readonly IntegrityVaultDbContext _context;
        private readonly RecordAccessLogRepository _repository;

        public RecordAccessLogRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<IntegrityVaultDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new IntegrityVaultDbContext(options);
            _repository = new RecordAccessLogRepository(_context);
        }

        #region Helpers


        private static RecordAccessLog BuildTestAccessLog(int id = 1) => new()
        {
            ID = id,
            RecordID = 500,
            AccessedByUserID = 11,
            AccessType = AccessType.Create,
            Timestamp = DateTime.UtcNow
        };

        #endregion



        [Fact]
        public async Task CreateRecordAccessLogAsync_ShouldAddLogToContext()
        {
            // Arrange.
            var log = BuildTestAccessLog(id: 1);

            // Act.
            var result = await _repository.CreateRecordAccessLogAsync(log);

            // We save here because the repository doesn't call SaveChangesAsync internally.
            await _context.SaveChangesAsync();

            // Assert.
            Assert.True(result);
            var dbLog = await _context.RecordAccessLogs.FindAsync(1);
            Assert.NotNull(dbLog);
            Assert.Equal(11, dbLog.AccessedByUserID);
            Assert.Equal(500, dbLog.RecordID);
        }


        [Fact]
        public async Task CreateRecordAccessLogAsync_ShouldHandleMultipleLogs()
        {
            // Arrange.
            var log1 = BuildTestAccessLog(id: 10);
            var log2 = BuildTestAccessLog(id: 11);

            // Act.
            await _repository.CreateRecordAccessLogAsync(log1);
            await _repository.CreateRecordAccessLogAsync(log2);
            await _context.SaveChangesAsync();

            // Assert.
            var count = await _context.RecordAccessLogs.CountAsync();
            Assert.Equal(2, count);
        }
    }
}