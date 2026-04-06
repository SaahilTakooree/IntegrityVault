// Import dependencies needed.
using IntegrityVault.Repository.Contexts; // Import the context class for interacting with the database.
using IntegrityVault.Repository.Interfaces; // Import the interface for the required to create and update the medical record.
using IntegrityVault.Service.Interfaces; // Import the interface for services.
using IntegrityVault.Common.Entities; // Import the entity class for required to create and update the medical record.
using IntegrityVault.Common.DTOs; // Import the data transfer objects (DTOs) used in the service layer.
using System.Text.Json; // Provides functionality for JSON serialisation and deserialisation.
using IntegrityVault.Service.Mappers; // Import the mapping utilities.
using Microsoft.EntityFrameworkCore.Storage; // Provides support for database transactions.
using IntegrityVault.Common.Enums; // Import enumerations used across the application.
using System.Security.Cryptography; // Provides functionality for JSON serialisation and deserialisation.


// Declaring the namespace where this service implementation resides.
namespace IntegrityVault.Service.Implementations
{
    // Define the MedicalRecordService class and injecting the IMedicalRecordRepository dependency.
    public class MedicalRecordService(IMedicalRecordRepository _medicalRecordRepository, IEpisodeRepository _episodeRepository,
        IRecordAccessLogRepository _recordAccessLogRepository, IMedicalRecordAuditLogRepository _medicalRecordAuditLogRepository,
        IUserRepository _userRepository, IHospitalRepository _hospitalRepository, IPDFService _pdfService,
        ICryptoService _cryptoService, IIPFSService _ipfsService, IBlockchainService _blockchainService, IntegrityVaultDbContext _context) : IMedicalRecordService
    {
        public async Task<bool> CreateMedicalRecordAndEpisodeAsync(CreateMedicalRecordDTO createMedicalRecordDTO)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var currentTime = DateTime.UtcNow;

                var episode = await CreateEpisodeAsync(createMedicalRecordDTO, currentTime);

                var (doctor, patient, hospital) = await ValidateAndGetDoctorPatientHospitalAsync(createMedicalRecordDTO.PatientID, createMedicalRecordDTO.DoctorID);

                return await ExecuteMedicalRecordCreationAsync(episode.ID, createMedicalRecordDTO, doctor, patient, hospital, transaction, currentTime);
            }
            catch (InvalidOperationException ex)
            {
                try { await transaction.RollbackAsync(); } catch { }
                throw new InvalidOperationException($"Medical record creation failed: {ex.Message}.");
            }
            catch (Exception ex)
            {
                try { await transaction.RollbackAsync(); } catch { }
                throw new InvalidOperationException($"Error during medical record creation: {ex.Message}.");
            }
        }


        public async Task<bool> AddMedicalRecordToEpisodeAsync(int episodeID, CreateMedicalRecordDTO createMedicalRecordDTO)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (!await _episodeRepository.IsEpisodeActiveAsync(episodeID))
                {
                    throw new InvalidOperationException("Cannot add a medical record to an inactive episode.");
                }

                var currentTime = DateTime.UtcNow;

                var episode = await _episodeRepository.GetEpisodeByIdAsync(new EpisodeIdDTO { ID = episodeID }) ?? throw new InvalidOperationException("Episode not found.");

                var (doctor, patient, hospital) = await ValidateAndGetDoctorPatientHospitalAsync(createMedicalRecordDTO.PatientID, createMedicalRecordDTO.DoctorID);

                if ((doctor.ID != episode.DoctorID) || (patient.ID != episode.PatientID) || (episode.Title != createMedicalRecordDTO.ChiefComplaint))
                {
                    throw new InvalidOperationException("Doctor, patient id or chief complaint does match on the episode.");
                }

                return await ExecuteMedicalRecordCreationAsync(episode.ID, createMedicalRecordDTO, doctor, patient, hospital, transaction, currentTime);
            }
            catch (InvalidOperationException ex)
            {
                try { await transaction.RollbackAsync(); } catch { }
                throw new InvalidOperationException($"Addition of medical record failed: {ex.Message}.");
            }
            catch (Exception ex)
            {
                try { await transaction.RollbackAsync(); } catch { }
                throw new InvalidOperationException($"Error during medical record addition: {ex.Message}.");
            }
        }


        // Method to update a medical record.
        public async Task<bool> PatchMedicalRecordAsync(int medicalRecordID, int episodeID, CreateMedicalRecordDTO createMedicalRecordDTO)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (!await _episodeRepository.IsEpisodeActiveAsync(episodeID))
                {
                    throw new InvalidOperationException("Cannot update a medical record for an inactive episode.");
                }

                var currentTime = DateTime.UtcNow;
                var version = 2;

                var medicalRecord = await _medicalRecordRepository.GetMedicalRecordById(medicalRecordID)
                        ?? throw new InvalidOperationException("Medical record not found.");

                if (medicalRecord.CurrentVersion != 1)
                {
                    var auditLogs = await _medicalRecordAuditLogRepository.GetAllVersionOfMedicalRecordByID(medicalRecordID);

                    if (medicalRecord.CurrentVersion != (auditLogs.Count + 1))
                    {
                        throw new InvalidOperationException($"Version mismatch: Expected version {auditLogs.Count + 1}, but found version {medicalRecord.CurrentVersion}. There may be an issue with the medical record versioning.");
                    }
                    else
                    {
                        version = auditLogs.Count + 2;
                    }
                }

                var episode = await _episodeRepository.GetEpisodeByIdAsync(new EpisodeIdDTO { ID = episodeID })
                    ?? throw new InvalidOperationException("Episode not found.");

                var (doctor, patient, hospital) = await ValidateAndGetDoctorPatientHospitalAsync(createMedicalRecordDTO.PatientID, createMedicalRecordDTO.DoctorID);

                if ((doctor.ID != episode.DoctorID) || (patient.ID != episode.PatientID) || (episode.Title != createMedicalRecordDTO.ChiefComplaint))
                {
                    throw new InvalidOperationException("Doctor, patient id or chief complaint does match on the episode.");
                }

                // Generate the updated PDF.
                var pdfBytes = GenerateMedicalRecordPDF(createMedicalRecordDTO, episodeID, patient, doctor, hospital, version);

                // Compute the new ContentHash.
                var newContentHashBytes = SHA256.HashData(pdfBytes);
                var newContentHashHex = Convert.ToHexString(newContentHashBytes).ToLowerInvariant();

                // Compute the new VersionHash.
                var previousVersionHashBytes = Convert.FromHexString(medicalRecord.VersionHash);
                var newVersionHashBytes = ComputeVersionHash(
                    newContentHashBytes,
                    previousVersionHashBytes,
                    version,
                    medicalRecordID);
                var newVersionHashHex = Convert.ToHexString(newVersionHashBytes).ToLowerInvariant();


                // Upload updated PDF to IPFS.
                var newIpfsCid = await _ipfsService.AddFileAsync(pdfBytes);
                var oldIpfsCid = medicalRecord.IPFS_CID;
                var oldContentHashHex = medicalRecord.ContentHash;
                var oldVersionHashHex = medicalRecord.VersionHash;


                if (oldContentHashHex == newContentHashHex)
                {
                    throw new InvalidOperationException("No changes detected. Content hash is identical.");
                }

                // Anchor the new version on the blockchain.
                var txHash = await _blockchainService.UpdateRecordOnChainAsync(
                    hospital.ID,
                    medicalRecordID,
                    medicalRecord.CurrentVersion,
                    newContentHashBytes,
                    newVersionHashBytes,
                    newIpfsCid);

                // Insert the audit log row.
                await _medicalRecordAuditLogRepository.InsertAuditLog(new CreateMedicalRecordAuditDTO
                {
                    RecordID = medicalRecordID,
                    UpdatedByDoctorID = doctor.ID,
                    PreviousIPFS_CID = oldIpfsCid,
                    NewIPFS_CID = newIpfsCid,
                    PreviousContentHash = oldContentHashHex,
                    NewContentHash = newContentHashHex,
                    PreviousVersionHash = oldVersionHashHex,
                    NewVersionHash = newVersionHashHex,
                    BlockchainTxHash = txHash,
                    Version = version,
                    UpdatedAt = currentTime
                });


                // Update the MedicalRecords row.
                await _medicalRecordRepository.PatchMedicalRecordAsync(medicalRecordID, new MedicalRecordPatchDTO
                {
                    IPFS_CID = newIpfsCid,
                    CurrentVersion = version,
                    UpdatedAt = currentTime,
                    ContentHash = newContentHashHex,
                    VersionHash = newVersionHashHex,
                    PreviousVersionHash = oldVersionHashHex,
                    BlockchainTxHash = txHash
                });

                // Log the access and commit.
                var accessLog = ToRecordAccessLogMapper.ToRecordAccessLogEntity(
                    medicalRecordID, createMedicalRecordDTO.DoctorID, (byte)AccessType.Update, currentTime);
                await _recordAccessLogRepository.CreateRecordAccessLogAsync(accessLog);
                

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (InvalidOperationException ex)
            {
                try { await transaction.RollbackAsync(); } catch { }
                throw new InvalidOperationException($"Updating of medical record failed: {ex.Message}.");
            }
            catch (Exception ex)
            {
                try { await transaction.RollbackAsync(); } catch { }
                throw new InvalidOperationException($"Error during medical record updating: {ex.Message}.");
            }
        }


        // Method to get the full medical record history for a patient.
        public async Task<PatientMedicalHistoryDTO> GetPatientMedicalHistoryAsync(int patientID)
        {
            try
            {
                // Get the patient information.
                var patient = await _userRepository.GetPatientByIdAsync(patientID) ?? throw new InvalidOperationException("Patient not found.");

                // Get the record infomation form the patient.
                var records = await _medicalRecordRepository.GetMedicalRecordsByPatientIDAsync(patient.ID);

                // Collect every unique UserID that appears in any access log.
                var accessLogUserIDs = records
                    .SelectMany(m => m.AccessLogs)
                    .Select(a => a.AccessedByUserID)
                    .Distinct()
                    .ToList();

                // Batch fetch both tables once.
                var doctors = await _userRepository.GetDoctorsByIDsAsync(accessLogUserIDs);
                var patients = await _userRepository.GetPatientsByIDsAsync(accessLogUserIDs);
                var doctorLookup = doctors.ToDictionary(d => d.ID);
                var patientLookup = patients.ToDictionary(p => p.ID);

                var providerLookup = new Dictionary<int, ExternalProvider>();
                var hospitalLookup = new Dictionary<int, Hospital>();

                foreach (var id in accessLogUserIDs)
                {
                    if (doctorLookup.ContainsKey(id) || patientLookup.ContainsKey(id)) continue;

                    var provider = await _userRepository.GetExternalProviderByIdAsync(id);
                    if (provider != null)
                    {
                        var hosp = await _hospitalRepository.GetHospitalByIdAsync(provider.BelongsToID);
                        if (hosp != null)
                        {
                            hospitalLookup[id] = hosp;
                        }
                    }
                }

                // Define the full name of the patient.
                var patientName = $"{patient.FirstName}{patient.MiddleName ?? ""}{patient.LastName}";

                // Group by Specialty, then descending by latest episode.
                var groupedBySpeciality = records.GroupBy(m => m.Episode!.Doctor!.Specialty)
                    .ToList();

                // The group the episode by speciality.
                var specialityGroups = new List<SpecialityGroupDTO>();

                // Loop for each speciality group.
                foreach (var specialityGroup in groupedBySpeciality)
                {
                    var specialityDTO = new SpecialityGroupDTO
                    {
                        Speciality = specialityGroup.Key.ToString(),
                        Episodes = []
                    };

                    // Group by EpisodeID.
                    var groupedByEpisode = specialityGroup
                        .GroupBy(m => m.EpisodeID)
                        .OrderByDescending(g => g.Max(m => m.CreatedAt))
                        .ToList();


                    // Loop thought each epsiode in each speciality.
                    foreach (var episodeGroup in groupedByEpisode)
                    {
                        var firstRecord = episodeGroup.First();

                        var episodeDTO = new EpisodeDetailDTO
                        {
                            EpisodeID = episodeGroup.Key,
                            ChiefComplaint = firstRecord.Episode!.Title,
                            IsActive = firstRecord.Episode.IsActive,
                            Records = []
                        };

                        var visitNumber = episodeGroup
                            .OrderBy(m => m.CreatedAt)
                            .ToList();

                        // Records descending — latest visit on top.
                        var orderedRecords = episodeGroup
                            .OrderByDescending(m => m.CreatedAt)
                            .ToList();

                        foreach (var record in orderedRecords)
                        {
                            // Visit number is the ascending position.
                            var vNum = visitNumber.IndexOf(record) + 1;
                            var chiefComplaint = firstRecord.Episode.Title.Replace(" ", "");

                            var recordDTO = new MedicalRecordDetailDTO
                            {
                                MedicalRecordID = record.ID,
                                VisitDate = record.VisitDate,
                                CurrentVersion = record.CurrentVersion,
                                Versions = [.. BuildVersionList(record, patientName, chiefComplaint, vNum).OrderByDescending(v => v.Version)], // Versions descending.
                                AccessLogs = BuildAccessLogList(record, doctorLookup, patientLookup, hospitalLookup) // Access logs ascending chronological.
                            };

                            episodeDTO.Records.Add(recordDTO);
                        }

                        specialityDTO.Episodes.Add(episodeDTO);
                    }

                    specialityGroups.Add(specialityDTO);
                }

                // Return the full list of the patient spciality.
                return new PatientMedicalHistoryDTO
                {
                    PatientID = patient.ID,
                    PatientFullName = $"{patient.FirstName} {patient.LastName}",
                    Specialities = specialityGroups
                };
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Error while trying to get medical records associated with a patient: {ex.Message}.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error while trying to get medical records associated with a patient: {ex.Message}.");
            }
        }


        // Method to get the full medical record for all the patients of a doctor.
        public async Task<DoctorMedicalHistoryDTO> GetDoctorMedicalHistoryAsync(int doctorID)
        {
            try
            { 
                // Get the doctor information.
                var doctor = await _userRepository.GetDoctorByIdAsync(doctorID)?? throw new InvalidOperationException("Doctor not found.");

                // Get all medical record that is associated with that doctor.
                var records = await _medicalRecordRepository.GetMedicalRecordsByDoctorIDAsync(doctor.ID);

                // Batch-fetch all unique patients and access log users.
                var patientIDs = records
                    .Select(m => m.Episode!.PatientID)
                    .Distinct()
                    .ToList();

                // Get the accessLogs which is reated by that user.
                var accessLogUserIDs = records
                    .SelectMany(m => m.AccessLogs)
                    .Select(a => a.AccessedByUserID)
                    .Distinct()
                    .ToList();

                // Get the patient information.
                var patients = await _userRepository.GetPatientsByIDsAsync(patientIDs);
                var accessDocs = await _userRepository.GetDoctorsByIDsAsync(accessLogUserIDs);
                var accessPats = await _userRepository.GetPatientsByIDsAsync(accessLogUserIDs);

                var patientLookup = patients.ToDictionary(p => p.ID);
                var doctorLookup = accessDocs.ToDictionary(d => d.ID);
                var accessPatLookup = accessPats.ToDictionary(p => p.ID);

                var providerLookup = new Dictionary<int, ExternalProvider>();
                var hospitalLookup = new Dictionary<int, Hospital>();

                foreach (var id in accessLogUserIDs)
                {
                    if (doctorLookup.ContainsKey(id) || patientLookup.ContainsKey(id)) continue;

                    var provider = await _userRepository.GetExternalProviderByIdAsync(id);
                    if (provider != null)
                    {
                        var hosp = await _hospitalRepository.GetHospitalByIdAsync(provider.BelongsToID);
                        if (hosp != null)
                        {
                            hospitalLookup[id] = hosp;
                        }
                    }
                }

                // Group by PatientID.
                var groupedByPatient = records
                    .GroupBy(m => m.Episode!.PatientID)
                    .ToList();

                var patientSummaries = new List<DoctorPatientSummaryDTO>();


                foreach (var patientGroup in groupedByPatient)
                {
                    var pat = patientLookup.GetValueOrDefault(patientGroup.Key)?? throw new InvalidOperationException($"Patient {patientGroup.Key} not found.");

                    var patientName = $"{pat.FirstName}{pat.MiddleName ?? ""}{pat.LastName}";

                    var patientSummary = new DoctorPatientSummaryDTO
                    {
                        PatientID = pat.ID,
                        PatientFullName = $"{pat.FirstName} {pat.LastName}",
                        Episodes = []
                    };

                    // Group by EpisodeID.
                    var groupedByEpisode = patientGroup
                        .GroupBy(m => m.EpisodeID)
                        .OrderByDescending(g => g.Max(m => m.CreatedAt))
                        .ToList();

                    foreach (var episodeGroup in groupedByEpisode)
                    {
                        var firstRecord = episodeGroup.First();

                        var episodeDTO = new EpisodeDetailDTO
                        {
                            EpisodeID = episodeGroup.Key,
                            ChiefComplaint = firstRecord.Episode!.Title,
                            IsActive = firstRecord.Episode.IsActive,
                            Records = []
                        };

                        var ascendingVisits = episodeGroup.OrderBy(m => m.CreatedAt).ToList();

                        // Records descending.
                        foreach (var record in ascendingVisits
                            .OrderByDescending(m => m.CreatedAt))
                        {
                            var vNum = ascendingVisits.IndexOf(record) + 1;
                            var chiefComplaint = firstRecord.Episode.Title.Replace(" ", "");

                            var recordDTO = new MedicalRecordDetailDTO
                            {
                                MedicalRecordID = record.ID,
                                VisitDate = record.VisitDate,
                                CurrentVersion = record.CurrentVersion,
                                Versions = [.. BuildVersionList(record, patientName, chiefComplaint, vNum).OrderByDescending(v => v.Version)],
                                AccessLogs = BuildAccessLogList(record, doctorLookup, accessPatLookup, hospitalLookup)
                            };

                            episodeDTO.Records.Add(recordDTO);
                        }

                        patientSummary.Episodes.Add(episodeDTO);
                    }

                    patientSummaries.Add(patientSummary);
                }

                return new DoctorMedicalHistoryDTO
                {
                    DoctorID = doctor.ID,
                    DoctorFullName = $"{doctor.FirstName} {doctor.LastName}",
                    Patients = patientSummaries
                };
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Error while trying to get medical record of all patient associated with a doctor: {ex.Message}.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error while trying to get medical record of all patient associated with a doctor: {ex.Message}.");
            }
        }


        // Method to verify a medical record by its cid.
        public async Task<VerifyMedicalRecordDTO> IsMedicalRecordTamperedAsync(string cid, int userID)
        {
            try
            {
                // Fetch PDF from IPFS.
                var pdfBytes = await _ipfsService.GetFileAsync(cid);

                // Recompute ContentHash.
                var recomputedHashBytes = SHA256.HashData(pdfBytes);
                var recomputedHashHex = Convert.ToHexString(recomputedHashBytes).ToLowerInvariant();

                // Get the store medical record, the version for the medical record, the stored content hash and store cid.
                var (medicalRecord, resolvedVersion, storedContentHashHex, storedCid) = await ResolveRecordFromCIDAsync(cid);

                // Extract embedded DTO for access control.
                var embeddedDto = _pdfService.ExtractJsonFromPdf(pdfBytes);
                var (allowed, reason) = await CheckAccessAsync(userID, embeddedDto);
                if (!allowed)
                {
                    return new VerifyMedicalRecordDTO
                    {
                        IsTampered = false,
                        Status = "Unauthorised",
                        ContentHashMatch = false,
                        DatabaseHashMatch = false,
                        CIDMatch = false,
                        Message = reason
                    };
                }

                //  Fetch the on-chain entry for this record and version.
                var chainEntry = await _blockchainService.GetRecordFromChainAsync(medicalRecord.ID, resolvedVersion);
                var chainContentHashHex = Convert.ToHexString(chainEntry.ContentHash).ToLowerInvariant();
                var chainCid = chainEntry.IpfsCID;

                // Get versionHash from blockchain.
                var chainVersionHashHex = Convert.ToHexString(chainEntry.VersionHash).ToLowerInvariant();

                // Get previous version hash.
                byte[] previousVersionHashBytes;

                if (resolvedVersion == 1)
                {
                    previousVersionHashBytes = new byte[32];
                }
                else
                {
                    // Fetch previous version from blockchain.
                    var prevChainEntry = await _blockchainService.GetRecordFromChainAsync(medicalRecord.ID, resolvedVersion - 1);
                    previousVersionHashBytes = prevChainEntry.VersionHash;
                }

                // Recompute expected versionHash.
                var expectedVersionHashBytes = ComputeVersionHash(
                    recomputedHashBytes,
                    previousVersionHashBytes,
                    resolvedVersion,
                    medicalRecord.ID
                );

                var expectedVersionHashHex = Convert.ToHexString(expectedVersionHashBytes).ToLowerInvariant();

                // Compare.
                bool versionHashMatch = expectedVersionHashHex == chainVersionHashHex;

                // Recomputed hash from fetched bytes vs blockchain anchor to detects  file content that was modified on IPFS after anchoring.
                bool contentHashMatch = recomputedHashHex == chainContentHashHex;

                // Stored hash in databaase vs blockchain ancho to detects database row or audit log tampered directly.
                bool databaseHashMatch = storedContentHashHex == chainContentHashHex;

                // Stored CID in database log vs blockchain anchor to detects CID column overwritten to point at a different file.
                bool cidMatch = storedCid == chainCid;

                bool isTampered = !contentHashMatch || !databaseHashMatch || !cidMatch || !versionHashMatch;

                // Log access.
                try
                {
                    var accessLog = ToRecordAccessLogMapper.ToRecordAccessLogEntity(
                        medicalRecord.ID, userID, (byte)AccessType.Verify, DateTime.UtcNow);
                    await _recordAccessLogRepository.CreateRecordAccessLogAsync(accessLog);
                    await _context.SaveChangesAsync();
                }
                catch {}


                // Return result.
                return BuildVerifyResult(isTampered, contentHashMatch, databaseHashMatch, cidMatch, versionHashMatch);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Verify by CID failed: {ex.Message}.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unexpected error during verify by CID: {ex.Message}.");
            }
        }


        // Method to check if pdf is has not been tampered with.
        public async Task<VerifyMedicalRecordDTO> VerifyPdfTamperingAsync(byte[] pdfBytes, int userID)
        {
            try
            {
                // Compute CID from uploaded bytes.
                var computedCid = await _ipfsService.GetCIDOnlyAsync(pdfBytes);

                // Recompute ContentHash.
                var recomputedHashBytes = SHA256.HashData(pdfBytes);
                var recomputedHashHex = Convert.ToHexString(recomputedHashBytes).ToLowerInvariant();

                // Get the correct version of the medical record.
                var (medicalRecord, resolvedVersion, storedContentHashHex, storedCid) = await ResolveRecordFromCIDAsync(computedCid);

                // Extract embedded DTO for access control.
                var embeddedDto = _pdfService.ExtractJsonFromPdf(pdfBytes);
                var (allowed, reason) = await CheckAccessAsync(userID, embeddedDto);
                if (!allowed)
                {
                    return new VerifyMedicalRecordDTO
                    {
                        IsTampered = false,
                        Status = "Unauthorised",
                        ContentHashMatch = false,
                        DatabaseHashMatch = false,
                        CIDMatch = false,
                        Message = reason
                    };
                }

                // Fetch on-chain entry.
                var chainEntry = await _blockchainService.GetRecordFromChainAsync(medicalRecord.ID, resolvedVersion);
                var chainContentHashHex = Convert.ToHexString(chainEntry.ContentHash).ToLowerInvariant();
                var chainCid = chainEntry.IpfsCID;

                // Get versionHash from blockchain
                var chainVersionHashHex = Convert.ToHexString(chainEntry.VersionHash).ToLowerInvariant();

                // Get previous version hash
                byte[] previousVersionHashBytes;

                if (resolvedVersion == 1)
                {
                    previousVersionHashBytes = new byte[32];
                }
                else
                {
                    // Fetch previous version from blockchain (STRONGEST source)
                    var prevChainEntry = await _blockchainService.GetRecordFromChainAsync(medicalRecord.ID, resolvedVersion - 1);
                    previousVersionHashBytes = prevChainEntry.VersionHash;
                }

                // Recompute expected versionHash
                var expectedVersionHashBytes = ComputeVersionHash(
                    recomputedHashBytes,
                    previousVersionHashBytes,
                    resolvedVersion,
                    medicalRecord.ID
                );

                var expectedVersionHashHex = Convert.ToHexString(expectedVersionHashBytes).ToLowerInvariant();

                // Compare
                bool versionHashMatch = expectedVersionHashHex == chainVersionHashHex;

                // Run three integrity checks.
                bool contentHashMatch = recomputedHashHex == chainContentHashHex;
                bool databaseHashMatch = storedContentHashHex == chainContentHashHex;
                bool cidMatch = storedCid == chainCid;
                bool isTampered = !contentHashMatch || !databaseHashMatch || !cidMatch || !versionHashMatch;

                // Log access.
                try
                {
                    var accessLog = ToRecordAccessLogMapper.ToRecordAccessLogEntity(
                        medicalRecord.ID, userID, (byte)AccessType.Verify, DateTime.UtcNow);
                    await _recordAccessLogRepository.CreateRecordAccessLogAsync(accessLog);
                    await _context.SaveChangesAsync();
                }
                catch {}

                // Return result.
                return BuildVerifyResult(isTampered, contentHashMatch, databaseHashMatch, cidMatch, versionHashMatch);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"PDF verify failed: {ex.Message}.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unexpected error during PDF verify: {ex.Message}.");
            }
        }

        
        // Method to get the infomration out of the medical record.
        public async Task<MedicalRecordPdfDataDTO> GetMedicalRecordInformationFromCIDAsync(string cid, int userID)
        {
            try
            {
                // Fetch PDF from IPFS.
                var pdfBytes = await _ipfsService.GetFileAsync(cid);

                // Extract embedded DTO.
                var embeddedDto = _pdfService.ExtractJsonFromPdf(pdfBytes);

                // Check if allow to get infomration.
                var user = await _userRepository.GetUserByIdAsync(userID)
                    ?? throw new InvalidOperationException($"User {userID} not found.");

                bool isAuthor = user.Role == UserRole.Doctor && embeddedDto.DoctorID == userID;
                bool isSubject = user.Role == UserRole.Patient && embeddedDto.PatientID == userID;

                if (!isAuthor && !isSubject)
                {
                    throw new InvalidOperationException("Unauthorised: You do not have permission to view this record.");
                }

                // Log access as View
                try
                {
                    var (medicalRecord, _, _, _) = await ResolveRecordFromCIDAsync(cid);
                    var accessLog = ToRecordAccessLogMapper.ToRecordAccessLogEntity(
                        medicalRecord.ID, userID, (byte)AccessType.View, DateTime.UtcNow);
                    await _recordAccessLogRepository.CreateRecordAccessLogAsync(accessLog);
                    await _context.SaveChangesAsync();
                }
                catch {}

                // Return the extracted DTO.
                return embeddedDto;
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Error retrieving medical record from CID: {ex.Message}.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unexpected error retrieving medical record from CID: {ex.Message}.");
            }
        }


        // Method to download a medical record.
        public async Task<(byte[] pdfBytes, string fileName)> DownloadMedicalRecordAsync(string cid, int userID)
        {
            try
            {
                // Fetch PDF from IPFS.
                var pdfBytes = await _ipfsService.GetFileAsync(cid);

                // Extract embedded DTO for security check.
                var embeddedDto = _pdfService.ExtractJsonFromPdf(pdfBytes);

                // Permission check.
                var user = await _userRepository.GetUserByIdAsync(userID)
                    ?? throw new InvalidOperationException($"User {userID} not found.");

                if (user.Role != UserRole.Patient)
                    throw new InvalidOperationException("Only patients may download their own medical records.");

                if (embeddedDto.PatientID != userID)
                    throw new InvalidOperationException("You may only download your own medical records.");

                // Resolve the record details.
                var (medicalRecord, resolvedVersion, _, _) = await ResolveRecordFromCIDAsync(cid);

                // Access the navigation properties from the medicalRecord object.
                var patientName = $"{embeddedDto.PatientFirstName}{embeddedDto.PatientMiddleName ?? ""}{embeddedDto.PatientLastName}";

                // Generate the File Name logic.
                var cleanComplaint = embeddedDto.ChiefComplaint.Replace(" ", "");

                // Get all records for this episode to calculate the Visit Number.
                var allRecords = await _medicalRecordRepository.GetMedicalRecordsByPatientIDAsync(embeddedDto.PatientID);
                var episodeRecords = allRecords
                    .Where(m => m.EpisodeID == embeddedDto.EpisodeID)
                    .OrderBy(m => m.CreatedAt)
                    .ToList();

                var vNum = episodeRecords.FindIndex(m => m.ID == medicalRecord.ID) + 1;

                // Construct the filename using the resolved version from IPFS.
                string fileName = $"{patientName}_{cleanComplaint}_Visit{vNum}_v{resolvedVersion}.pdf";

                // Log access as Download.
                try
                {
                    await StageRecordAccessLogAsync(medicalRecord.ID, userID, AccessType.Download, DateTime.UtcNow);
                    await _context.SaveChangesAsync();
                }
                catch {}

                return (pdfBytes, fileName);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Error downloading medical record: {ex.Message}.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unexpected error downloading medical record: {ex.Message}.");
            }
        }



        // Private function to add an episode.
        private async Task<Episode> CreateEpisodeAsync(CreateMedicalRecordDTO createMedicalRecordDTO, DateTime currentTime)
        {
            try
            {
                // Map the DTO to an Episode entity.
                var episode = ToEpisodeMapper.ToEpisodeEntity(createMedicalRecordDTO, currentTime);

                // Save the episode to the repository.
                await _episodeRepository.CreateEpisodeAsync(episode);

                // Ensure the changes are committed to the database.
                await _context.SaveChangesAsync();

                // Return the created episode.
                return episode;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error while creating episode: {ex.Message}");
            }
        }


        // Private function to add a medical record.
        private async Task<MedicalRecord> CreateMedicalRecordAsync(int episodeID, CreateMedicalRecordDTO createMedicalRecordDTO, string ipfsCid,
            string contentHashHex, string versionHashHex, string? blockchainTxHash, int version, DateTime currentTime)
        {
            try
            {
                // Map the data to a MedicalRecord entity.
                var medicalRecord = ToMedicalRecordMapper.ToMedicalRecordEntity(episodeID, createMedicalRecordDTO, ipfsCid,
                    contentHashHex, versionHashHex, blockchainTxHash, version, currentTime);

                // Save the medical record to the repository.
                await _medicalRecordRepository.CreateMedicalRecordAsync(medicalRecord);

                // Ensure the changes are committed to the database.
                await _context.SaveChangesAsync();

                // Return the created episode.
                return medicalRecord;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error while creating medical record: {ex.Message}");
            }
        }


        // Private function to create the PDF.
        private byte[] GenerateMedicalRecordPDF(CreateMedicalRecordDTO createMedicalRecordDTO, int episodeID, Patient patient, Doctor doctor, Hospital hospital, int version)
        {
            try
            {
                // Map the data to a MedicalRecordPdfDataDTO.
                var pdfData = MedicalRecordToPDFMapper.ToPDFDataDTO(episodeID, patient, doctor, hospital, createMedicalRecordDTO, version);

                // Encrypt the data before generating the PDF.
                var json = JsonSerializer.Serialize(pdfData);
                var encrypted = _cryptoService.Encrypt(json);

                // Generate the PDF from the DTO.
                var pdfBytes = _pdfService.GeneratePDF(pdfData, encrypted);

                return pdfBytes;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error while creating medical record: {ex.Message}");
            }
        }


        // Private function to create and insert RecordAccessLog.
        private async Task StageRecordAccessLogAsync(int recordID, int userID, AccessType accessType, DateTime currentTime)
        {
            try
            {
                // Map the data to a RecordAccessLog entity.
                var accessLog = ToRecordAccessLogMapper.ToRecordAccessLogEntity(recordID, userID, (byte)accessType, currentTime);

                // Save the record access log to the repository.
                var result = await _recordAccessLogRepository.CreateRecordAccessLogAsync(accessLog);

                if(!result) {
                    throw new InvalidOperationException("Failed to insert record access log.");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error while creating medical record: {ex.Message}");
            }
        }


        // Private function to help to validate patient, doctor and hospital.
        private async Task<(Doctor doctor, Patient patient, Hospital hospital)> ValidateAndGetDoctorPatientHospitalAsync(int patientID, int doctorID)
        {
            // Fetch the doctor and patient asynchronously.
            var patient = await _userRepository.GetPatientByIdAsync(patientID) ?? throw new InvalidOperationException("Patient not found.");
            var doctor = await _userRepository.GetDoctorByIdAsync(doctorID) ?? throw new InvalidOperationException("Doctor not found.");

            // Validate that the doctor and patient belong to the same hospital.
            if (doctor.HospitalID == null || patient.HospitalID == null)
            {
                throw new InvalidOperationException("Doctor or Patient does not have a valid HospitalID.");
            }

            if (doctor.HospitalID != patient.HospitalID)
            {
                throw new InvalidOperationException("Doctor cannot create a medical record for a patient from another hospital.");
            }

            // Fetch the hospital by ID.
            var hospital = await _hospitalRepository.GetHospitalByIdAsync(doctor.HospitalID.Value) ?? throw new InvalidOperationException("Hospital not found.");

            return (doctor, patient, hospital);
        }


        // Private method to generate the medical record and update the appropriate repository.
        private async Task<bool> ExecuteMedicalRecordCreationAsync(int episodeID, CreateMedicalRecordDTO createMedicalRecordDTO,
            Doctor doctor, Patient patient, Hospital hospital, IDbContextTransaction transaction, DateTime currentTime)
        {
            // Generate the PDF.
            var pdfBytes = GenerateMedicalRecordPDF(createMedicalRecordDTO, episodeID, patient, doctor, hospital, 1);

            // Compute ContentHash.
            var contentHashBytes = SHA256.HashData(pdfBytes);
            var contentHashHex = Convert.ToHexString(contentHashBytes).ToLowerInvariant();

            // Upload PDF to IPFS.
            var ipfsCid = await _ipfsService.AddFileAsync(pdfBytes);

            // Compute a temporary VersionHash using recordID.
            var previousVersionHashBytes = new byte[32]; // 32 zero bytes — no prior version.
            var tempVersionHashBytes = ComputeVersionHash(contentHashBytes, previousVersionHashBytes, version: 1, recordID: 0);
            var tempVersionHashHex = Convert.ToHexString(tempVersionHashBytes).ToLowerInvariant();

            // Insert the DB row to obtain the auto-generated RecordID
            var medicalRecord = await CreateMedicalRecordAsync(
                episodeID, createMedicalRecordDTO,
                ipfsCid,
                contentHashHex,
                versionHashHex: tempVersionHashHex,
                blockchainTxHash: null,
                version: 1,
                currentTime);

            // Compute the REAL VersionHash now that RecordID is known.
            var realVersionHashBytes = ComputeVersionHash(contentHashBytes, previousVersionHashBytes, version: 1, medicalRecord.ID);
            var realVersionHashHex = Convert.ToHexString(realVersionHashBytes).ToLowerInvariant();

            // Anchor on the blockchain with the real hashes
            var txHash = await _blockchainService.RegisterRecordOnChainAsync(
                hospital.ID,
                medicalRecord.ID,
                episodeID,
                contentHashBytes,
                realVersionHashBytes,
                ipfsCid);

            // Patch the database row with the real VersionHash and TxHash.
            await _medicalRecordRepository.PatchMedicalRecordAsync(medicalRecord.ID, new MedicalRecordPatchDTO
            {
                IPFS_CID = ipfsCid,
                CurrentVersion = 1,
                UpdatedAt = currentTime,
                ContentHash = contentHashHex,
                VersionHash = realVersionHashHex,
                PreviousVersionHash = null,
                BlockchainTxHash = txHash
            });

            // Log the access and commit.
            await StageRecordAccessLogAsync(medicalRecord.ID, createMedicalRecordDTO.DoctorID, AccessType.Create, currentTime);

            // Single flush + commit for the patch and access log together.
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }


        // Builds the full ordered version list for one medical record.
        private static List<MedicalRecordViewingItemDTO> BuildVersionList(MedicalRecord record, string patientName, string chiefComplaint, int visitNumber)
        {
            var allVersions = new List<MedicalRecordViewingItemDTO>();

            // If audit log is zero that means that it has never been update.
            if (record.AuditLogs.Count == 0)
            {
                allVersions.Add(new MedicalRecordViewingItemDTO
                {
                    DisplayName = $"{patientName}_{chiefComplaint}_Visit{visitNumber}_V1",
                    IPFS_CID = record.IPFS_CID,
                    Version = 1,
                    Timestamp = record.CreatedAt
                });
            }
            else
            {
                var orderedAuditLogs = record.AuditLogs.OrderBy(a => a.Version).ToList();
                allVersions.Add(new MedicalRecordViewingItemDTO
                {
                    DisplayName = $"{patientName}_{chiefComplaint}_Visit{visitNumber}_V1",
                    IPFS_CID = orderedAuditLogs.First().PreviousIPFS_CID,
                    Version = 1,
                    Timestamp = record.CreatedAt
                });
                foreach (var auditLog in orderedAuditLogs)
                {
                    allVersions.Add(new MedicalRecordViewingItemDTO
                    {
                        DisplayName = $"{patientName}_{chiefComplaint}_Visit{visitNumber}_V{auditLog.Version}",
                        IPFS_CID = auditLog.NewIPFS_CID,
                        Version = auditLog.Version,
                        Timestamp = auditLog.UpdatedAt
                    });
                }
            }

            return allVersions;
        }


        // Private method to resolves access log entries.
        private static List<RecordAccessLogItemDTO> BuildAccessLogList(MedicalRecord record, Dictionary<int, Doctor> doctorLookup, Dictionary<int, Patient> patientLookup, Dictionary<int, Hospital> hospitalLookup)
        {
            return record.AccessLogs
                .OrderBy(a => a.Timestamp)
                .Select(a =>
                {
                    string name;
                    string role;

                    if (doctorLookup.TryGetValue(a.AccessedByUserID, out var doc))
                    {
                        name = $"Dr. {doc.FirstName} {doc.MiddleName ?? ""} {doc.LastName}";
                        role = "Doctor";
                    }
                    else if (patientLookup.TryGetValue(a.AccessedByUserID, out var pat))
                    {
                        name = $"{pat.FirstName} {pat.MiddleName ?? ""} {pat.LastName}";
                        role = "Patient";
                    }
                    else if (hospitalLookup.TryGetValue(a.AccessedByUserID, out var prov))
                    {
                        name = prov.Name ?? "Associated Hospital";
                        role = "External Provider";
                    }
                    else
                    {
                        // Fallback if ID not found in either table.
                        name = $"Unknown (ID {a.AccessedByUserID})";
                        role = "Unknown";
                    }

                    return new RecordAccessLogItemDTO
                    {
                        AccessType = ((AccessType)a.AccessType).ToString(),
                        AccessedByName = name,
                        AccessedByRole = role,
                        Timestamp = a.Timestamp
                    };
                })
                .ToList();
        }


        // Computes the VersionHash that anchors a version into the tamper-evident chain.
        private static byte[] ComputeVersionHash(byte[] contentHash, byte[] previousVersionHash, int version, int recordID)
        {
            // Encode version and recordID as big-endian 4-byte arrays for deterministic input.
            var versionBytes = BitConverter.GetBytes(version);
            var recordIDBytes = BitConverter.GetBytes(recordID);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(versionBytes);
                Array.Reverse(recordIDBytes);
            }

            // Concatenate all inputs into a single buffer.
            var input = new byte[contentHash.Length + previousVersionHash.Length + versionBytes.Length + recordIDBytes.Length];
            Buffer.BlockCopy(contentHash, 0, input, 0, contentHash.Length);
            Buffer.BlockCopy(previousVersionHash, 0, input, contentHash.Length, previousVersionHash.Length);
            Buffer.BlockCopy(versionBytes, 0, input, contentHash.Length + previousVersionHash.Length, versionBytes.Length);
            Buffer.BlockCopy(recordIDBytes, 0, input, contentHash.Length + previousVersionHash.Length + versionBytes.Length, recordIDBytes.Length);

            return SHA256.HashData(input);
        }


        // Helper to check who is allowed to view your medical record.
        private async Task<(bool allowed, string reason)> CheckAccessAsync(int userID, MedicalRecordPdfDataDTO embeddedDto)
        {
            // Look up the requesting user to determine their role.
            var user = await _userRepository.GetUserByIdAsync(userID) ?? throw new InvalidOperationException($"User {userID} not found.");

            switch (user.Role)
            {
                case UserRole.Doctor:
                    if (embeddedDto.DoctorID != userID)
                        return (false, "Doctors may only verify records they authored.");
                    return (true, string.Empty);

                case UserRole.Patient:
                    if (embeddedDto.PatientID != userID)
                        return (false, "Patients may only verify their own records.");
                    return (true, string.Empty);

                case UserRole.ExternalProvider:
                    var provider = await _userRepository.GetExternalProviderByIdAsync(userID) ?? throw new InvalidOperationException($"External provider {userID} not found.");
                   
                    if (provider.BelongsToID != embeddedDto.HospitalID)
                        return (false, "External providers may only verify records belonging to their associated hospital.");
                    return (true, string.Empty);

                default:
                    return (false, "Your role does not have permission to verify medical records.");
            }
        }


        // Builds the VerifyMedicalRecordDTO from the three check results.
        private static VerifyMedicalRecordDTO BuildVerifyResult(bool isTampered,
            bool contentHashMatch, bool databaseHashMatch, bool cidMatch, bool versionHashMatch)
        {
            string message;

            if (!isTampered)
            {
                message = "All integrity checks passed. The record is intact.";
            }
            else
            {
                var failed = new List<string>();
                if (!contentHashMatch) failed.Add("ContentHash mismatch — the file content does not match the blockchain anchor.");
                if (!databaseHashMatch) failed.Add("Database hash mismatch — the stored hash does not match the blockchain anchor.");
                if (!cidMatch) failed.Add("CID mismatch — the stored IPFS address does not match the blockchain anchor.");
                if (!versionHashMatch) failed.Add("Version chain mismatch — hash linkage is broken.");
                message = string.Join(" | ", failed);
            }

            return new VerifyMedicalRecordDTO
            {
                IsTampered = isTampered,
                Status = isTampered ? "Tampered" : "Intact",
                ContentHashMatch = contentHashMatch,
                DatabaseHashMatch = databaseHashMatch,
                CIDMatch = cidMatch,
                VersionHashMatch = versionHashMatch,
                Message = message
            };
        }


        // Helper to get the correct detail of a medical record.
        private async Task<(MedicalRecord medicalRecord, int resolvedVersion, string storedContentHashHex, string storedCid)> ResolveRecordFromCIDAsync(string cid)
        {
            // CID belongs to the current version
            var record = await _medicalRecordRepository.GetMedicalRecordByCIDAsync(cid);
            if (record != null)
            {
                return (record, record.CurrentVersion, record.ContentHash, record.IPFS_CID);
            }

            // If CID belongs to a version set by an update (NewIPFS_CID) ──
            var auditByNew = await _medicalRecordAuditLogRepository.GetAuditLogByNewCIDAsync(cid);
            if (auditByNew != null)
            {
                var parentRecord = await _medicalRecordRepository.GetMedicalRecordById(auditByNew.RecordID) ?? throw new InvalidOperationException($"Parent record {auditByNew.RecordID} not found.");
                return (parentRecord, auditByNew.Version, auditByNew.NewContentHash, auditByNew.NewIPFS_CID);
            }

            // If CID belongs to version 1 of a record that was later updated.
            var auditByPrevious = await _medicalRecordAuditLogRepository.GetAuditLogByPreviousCIDAsync(cid);
            if (auditByPrevious != null)
            {
                var parentRecord = await _medicalRecordRepository.GetMedicalRecordById(auditByPrevious.RecordID) ?? throw new InvalidOperationException($"Parent record {auditByPrevious.RecordID} not found.");
                return (parentRecord, 1, auditByPrevious.PreviousContentHash, auditByPrevious.PreviousIPFS_CID);
            }

            throw new InvalidOperationException("No medical record found matching this CID.");
        }
    }
}