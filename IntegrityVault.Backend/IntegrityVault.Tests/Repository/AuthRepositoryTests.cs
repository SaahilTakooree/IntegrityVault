using IntegrityVault.Common.Helpers;
using IntegrityVault.Repository.Contexts;


namespace IntegrityVault.Tests.Repository
{
    public class AuthRepositoryTests
    {
        private readonly IntegrityVaultDbContext _context;
        private readonly AuthRepository _repository;
        private const string TestPassword = "Qwerty!2";

        public AuthRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<IntegrityVaultDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new IntegrityVaultDbContext(options);
            _repository = new AuthRepository(_context);
        }

        #region Helpers


        private static string GetHashedPassword() => HashHelper.Hash(TestPassword)!;

        private static Doctor BuildTestDoctor() => new()
        {
            ID = 1,
            Username = "house.md",
            Email = "greg@house.com",
            Password = GetHashedPassword(),
            FirstName = "Greg",
            LastName = "House",
            Role = UserRole.Doctor,
            Specialty = DoctorSpecialty.GeneralMedicine,
            HospitalID = 101
        };

        private static Patient BuildTestPatient() => new()
        {
            ID = 2,
            Username = "wilson.j",
            Email = "james@wilson.com",
            Password = GetHashedPassword(),
            FirstName = "James",
            LastName = "Wilson",
            Role = UserRole.Patient,
            DOB = new DateOnly(1970, 1, 1),
            Gender = PatientGender.Male,
            HospitalID = 101
        };

        #endregion



        [Fact]
        public async Task GetUserByCredentialAsync_ShouldReturnDoctor_WhenEmailMatches()
        {
            // Arrange.
            var doctor = BuildTestDoctor();
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            // Act.
            var result = await _repository.GetUserByCredentialAsync("greg@house.com", TestPassword);

            // Assert.
            Assert.NotNull(result);
            Assert.IsType<Doctor>(result);
            Assert.Equal(doctor.Username, result.Username);
        }

        [Fact]
        public async Task GetUserByCredentialAsync_ShouldReturnPatient_WhenUsernameMatches()
        {
            // Arrange.
            var patient = BuildTestPatient();
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            // Act.
            var result = await _repository.GetUserByCredentialAsync("wilson.j", TestPassword);

            // Assert.
            Assert.NotNull(result);
            Assert.IsType<Patient>(result);
            Assert.Equal(patient.Email, result.Email);
        }


        [Fact]
        public async Task GetUserByCredentialAsync_ShouldReturnNull_WhenPasswordIsIncorrect()
        {
            // Arrange.
            var doctor = BuildTestDoctor();
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            // Act.
            var result = await _repository.GetUserByCredentialAsync("house.md", "WrongPassword");

            // Assert.
            Assert.Null(result);
        }


        [Fact]
        public async Task GetUserByCredentialAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Act.
            var result = await _repository.GetUserByCredentialAsync("ghost@nobody.com", TestPassword);

            // Assert.
            Assert.Null(result);
        }


        [Fact]
        public async Task GetUserByCredentialAsync_ShouldThrow_WhenPasswordInputIsNull()
        {
            // Arrange.
            var doctor = BuildTestDoctor();
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            // Act & Assert.
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _repository.GetUserByCredentialAsync("house.md", null!));

            Assert.Equal("Password cannot be null.", ex.Message);
        }


        [Fact]
        public async Task GetUserByCredentialAsync_ShouldSearchTablesInOrder()
        {
            // Arrange.
            var patient = BuildTestPatient();
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            // Act.
            var result = await _repository.GetUserByCredentialAsync(patient.Username, TestPassword);

            // Assert.
            Assert.NotNull(result);
            Assert.Equal(UserRole.Patient, result.Role);
        }
    }
}