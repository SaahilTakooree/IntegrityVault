// Import dependencies.
import { ComponentFixture, TestBed } from "@angular/core/testing"; // Import the angular testing utilities.
import { DoctorDashboardComponent } from "./doctor"; // Import the component to be tested.
import { MedicalRecordService } from "../../../core/services/medical-record/medical-record.service"; // Service for medical record/IPFS operations.
import { AuthService } from "../../../core/services/auth/auth.service"; // Service to get the current logged-in user.
import { of } from "rxjs"; // RxJS utilities for observable streams.
import { NO_ERRORS_SCHEMA } from "@angular/core"; // Schema to ignore child component errors.
import { provideHttpClientTesting } from "@angular/common/http/testing"; // Provides a mock HttpClient.
import { provideHttpClient } from "@angular/common/http"; // Provides the standard HttpClient.


// Define the test suite for the DoctorDashboardComponent.
describe("DoctorDashboardComponent", () => {
  let component: DoctorDashboardComponent;
  let fixture: ComponentFixture<DoctorDashboardComponent>;
  
  // Define mock services and fake data.
  let mockMedicalRecordService: any;
  let mockAuthService: any;

  const fakeDoctor = { id: 7, username: 'dr_house', hospitalId: 101 };
  const fakePatientList = [{ id: 1, firstName: 'John', lastName: 'Doe' }];
  const fakeHistory = {
    doctorFullName: "Gregory House",
    patients: [
      {
        patientID: 1,
        patientFullName: "John Doe",
        episodes: [
          {
            episodeID: 50,
            chiefComplaint: "Back Pain",
            isActive: true,
            records: []
          }
        ]
      }
    ]
  };


  // Set up the testing module and mock service implementations.
  beforeEach(async () => {
    mockAuthService = {
      CurrentUser: fakeDoctor
    };

    mockMedicalRecordService = {
      getDoctorById: jasmine.createSpy().and.returnValue(of({ firstName: 'Gregory', lastName: 'House', specialty: 1 })),
      getPatientsForHospital: jasmine.createSpy().and.returnValue(of(fakePatientList)),
      getDoctorHistory: jasmine.createSpy().and.returnValue(of(fakeHistory)),
      getMedicalRecordFromCID: jasmine.createSpy().and.returnValue(of({ diagnosis: 'Lupus', treatmentPlan: 'Rest' })),
      createNewMedicalRecordAndEpisode: jasmine.createSpy().and.returnValue(of({})),
      checkTamperByCID: jasmine.createSpy().and.returnValue(of({ isTampered: false })),
      setEpisodeStatus: jasmine.createSpy().and.returnValue(of({}))
    };

    await TestBed.configureTestingModule({
      imports: [DoctorDashboardComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: mockAuthService },
        { provide: MedicalRecordService, useValue: mockMedicalRecordService }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(DoctorDashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });


  // Method to verify doctor and patient data is loaded on init.
  it("Should load doctor details and hospital patients on init", () => {
    expect(mockMedicalRecordService.getDoctorById).toHaveBeenCalledWith(7);
    expect(mockMedicalRecordService.getPatientsForHospital).toHaveBeenCalledWith(101);
    expect(component.doctorFullName).toBe("Gregory House");
    expect(component.patients.length).toBe(1);
    expect(component.patients[0].fullName).toContain("John Doe");
  });


  // Method to check the history loading state.
  it("Should fetch doctor history and update loading state.", () => {
    component.loadHistory();
    expect(component.historyLoading).toBeFalse(); // Becomes false after 'of' emits.
    expect(component.history?.patients.length).toBe(1);
  });


  // Method to verify the state when opening a new episode.
  it("Should set correct modal state for new episodes.", () => {
    component.openNewEpisodeModal();
    
    expect(component.modalMode).toBe("newEpisode");
    expect(component.showRecordModal).toBeTrue();
    expect(component.activeVisitDate).toBe(new Date().toISOString().split("T")[0]);
  });


  // Method to check asynchronous data fetching when updating a record.
  it("Should fetch existing record data from IPFS when opening update modal.", () => {
    const mockEpisode = fakeHistory.patients[0].episodes[0] as any;
    const mockRecord = { medicalRecordID: 200, visitDate: '2023-01-01', versions: [{ ipfS_CID: 'Qm123' }] } as any;
    
    component.openUpdateRecordModal(mockEpisode, mockRecord, 1);
    
    expect(mockMedicalRecordService.getMedicalRecordFromCID).toHaveBeenCalledWith('Qm123', 7);
    expect(component.initialFormValue?.diagnosis).toBe('Lupus');
    expect(component.showRecordModal).toBeTrue();
  });


  // Method to verify tamper check initialisation.
  it("Should trigger tamper check API and show loading.", () => {
    const mockVersion = { ipfS_CID: 'Qm456', displayName: 'Record_V1.pdf' } as any;
    
    component.checkTamper(mockVersion);
    
    expect(component.tamperCID).toBe('Qm456');
    expect(mockMedicalRecordService.checkTamperByCID).toHaveBeenCalledWith('Qm456', 7);
    expect(component.showTamperModal).toBeTrue();
  });


  // Method to verify new episode creation submission.
  it("Should call createNewMedicalRecordAndEpisode when modalMode is 'newEpisode'.", () => {
    component.modalMode = "newEpisode";
    component.recordFormRef = {
      validate: () => true,
      getValue: () => ({
        patientID: 1,
        visitDate: '2024-01-01',
        diagnosis: 'Flu',
        treatmentPlan: 'Water',
        chiefComplaint: 'Cough'
      })
    } as any;

    component.onRecordConfirmed();

    expect(mockMedicalRecordService.createNewMedicalRecordAndEpisode).toHaveBeenCalled();
    expect(mockMedicalRecordService.getDoctorHistory).toHaveBeenCalled(); // Verifies refresh.
  });


  // Method to verify episode status toggling.
  it("Should call setEpisodeStatus and refresh history when confirmed.", () => {
    component.pendingStatusEpisodeID = 50;
    component.handleStatusResult(true);
    
    expect(mockMedicalRecordService.setEpisodeStatus).toHaveBeenCalledWith(50, 7);
    expect(mockMedicalRecordService.getDoctorHistory).toHaveBeenCalled();
  });


  // Method to verify date formatting logic.
  it("Should format timestamps correctly.", () => {
    const ts = "2024-03-30T10:00:00Z";
    const formatted = component.formatTimestamp(ts);
    expect(formatted).toContain("2024");
  });


  // Method to verify error extraction from various error object shapes.
  it("Should extract error messages correctly from API responses.", () => {
    const errorObj = { error: " Blockchain Timeout " };
    const message = (component as any)._extractErrorMessage(errorObj);
    expect(message).toBe("Blockchain Timeout");
  });


  // Method to check conditional rendering logic for episode editing.
  it("Should return true for episodeCanEdit if episode is active.", () => {
    const activeEpisode = { isActive: true } as any;
    const inactiveEpisode = { isActive: false } as any;
    
    expect(component.episodeCanEdit(activeEpisode)).toBeTrue();
    expect(component.episodeCanEdit(inactiveEpisode)).toBeFalse();
  });
});