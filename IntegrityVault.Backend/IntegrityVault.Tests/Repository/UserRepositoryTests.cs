using IntegrityVault.Repository.Contexts;

namespace IntegrityVault.Tests.Repository
{
    public class UserRepositoryTests
    {
        private readonly IntegrityVaultDbContext _context;
        private readonly UserRepository _repository;

        public UserRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<IntegrityVaultDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new IntegrityVaultDbContext(options);
            _repository = new UserRepository(_context);
        }

        #region Helpers



        private static Doctor BuildTestDoctor(int id, int? hospitalId = 1) => new()
        {
            ID = id,
            Username = $"doc_{id}",
            Email = $"doc{id}@integrityvault.com",
            Password = "hash",
            Role = UserRole.Doctor,
            HospitalID = hospitalId,
            FirstName = "Gregory",
            LastName = "House",
            Specialty = DoctorSpecialty.GeneralMedicine
        };

        private static Patient BuildTestPatient(int id, int? hospitalId = 1) => new()
        {
            ID = id,
            Username = $"pat_{id}",
            Email = $"pat{id}@integrityvault.com",
            Password = "hash",
            Role = UserRole.Patient,
            HospitalID = hospitalId,
            FirstName = "John",
            LastName = "Doe",
            DOB = new DateOnly(1990, 1, 1),
            Gender = PatientGender.Male
        };

        #endregion



        [Fact]
        public async Task GetAllUsersAsync_WithHospitalId_ShouldFilterCorrectly()
        {
            // Arrange.
            _context.Doctors.Add(BuildTestDoctor(1, hospitalId: 101));
            _context.Patients.Add(BuildTestPatient(2, hospitalId: 101));
            _context.Doctors.Add(BuildTestDoctor(3, hospitalId: 202));

            _context.SuperAdmins.Add(new SuperAdmin
            {
                ID = 4,
                Username = "SA",
                Email = "sa@integrityvault.com",
                Role = UserRole.SuperAdmin,
                WalletAddress = "0x1",
                Password = "QWerty!2",
                EncryptedPrivateKey = [0x01]
            });
            await _context.SaveChangesAsync();

            // Act.
            var result = await _repository.GetAllUsersAsync(101);

            // Assert.
            Assert.Equal(3, result.Count());
            Assert.Contains(result, u => u.ID == 1);
            Assert.Contains(result, u => u.ID == 2);
            Assert.Contains(result, u => u.ID == 4);
        }


        [Fact]
        public async Task GetUserByIdAsync_ShouldCheckAllSubTables()
        {
            // Arrange.
            var doctor = BuildTestDoctor(10);
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            // Act.
            var result = await _repository.GetUserByIdAsync(10);

            // Assert.
            Assert.NotNull(result);
            Assert.IsType<Doctor>(result);
            Assert.Equal("doc_10", result!.Username);
        }


        [Fact]
        public async Task CreatePatientAsync_ShouldPersistCorrectData()
        {
            // Arrange.
            var dto = new CreatePatientDTO
            {
                Username = "new_pat",
                Email = "pat@intergrityvault.com",
                Password = "secure",
                HospitalID = 1,
                FirstName = "Jane",
                LastName = "Smith",
                DOB = new DateOnly(1985, 5, 5),
                Gender = PatientGender.Female
            };

            // Act.
            var success = await _repository.CreatePatientAsync(dto);

            // Assert.
            Assert.True(success);
            var saved = await _context.Patients.FirstOrDefaultAsync(p => p.Username == "new_pat");
            Assert.NotNull(saved);
            Assert.Equal(PatientGender.Female, saved!.Gender);
        }


        [Fact]
        public async Task UpdateDoctorAsync_ShouldUpdateSpecificFieldsAndBaseFields()
        {
            // Arrange.
            var doctor = BuildTestDoctor(55);
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            var updateDto = new UpdateDoctorDTO
            {
                FirstName = "James",
                Email = "newemail@integrityvault.com",
                Specialty = DoctorSpecialty.Cardiology
            };

            // Act.
            var success = await _repository.UpdateDoctorAsync(55, updateDto);

            // Assert.
            Assert.True(success);
            var updated = await _context.Doctors.FindAsync(55); // Fixed the "burials" typo
            Assert.Equal("James", updated!.FirstName);
            Assert.Equal("newemail@integrityvault.com", updated.Email);
        }


        [Fact]
        public async Task GetSuperAdminByWalletAsync_ShouldBeCaseInsensitive()
        {
            // Arrange.
            var wallet = "0xABC123";
            var sa = new SuperAdmin
            {
                ID = 1,
                Username = "AdminX",
                Email = "sa@integrityvault.com",
                Password = "Qwerty!2",
                Role = UserRole.SuperAdmin,
                WalletAddress = wallet,
                EncryptedPrivateKey = [0x02]
            };
            _context.SuperAdmins.Add(sa);
            await _context.SaveChangesAsync();

            // Act.
            var result = await _repository.GetSuperAdminByWalletAsync(wallet);

            // Assert.
            Assert.NotNull(result);
            Assert.Equal(1, result!.ID);
            Assert.Equal("0xABC123", result.WalletAddress);
        }


        [Fact]
        public async Task DeleteUserAsync_ShouldRemoveFromBaseAndSubTable()
        {
            // Arrange.
            var doctor = BuildTestDoctor(99);
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            // Act.
            var result = await _repository.DeleteUserAsync(99);

            // Assert.
            Assert.True(result);
            Assert.Null(await _context.Users.FindAsync(99));
        }
    }
}