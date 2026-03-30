// Import dependencies.
import { ComponentFixture, TestBed } from "@angular/core/testing"; // Import the angular testing utilities.
import { PatientDashboardComponent } from "./patient"; // Import the component to be tested.
import { MedicalRecordService } from "../../../core/services/medical-record/medical-record.service"; // Service for medical record operations.
import { AuthService } from "../../../core/services/auth/auth.service"; // Service to get the current logged-in user.
import { of } from "rxjs"; // RxJS utilities for observable streams.
import { NO_ERRORS_SCHEMA } from "@angular/core"; // Schema to ignore child component errors.
import { provideHttpClientTesting } from "@angular/common/http/testing"; // Provides a mock HttpClient.
import { provideHttpClient } from "@angular/common/http"; // Provides the standard HttpClient.
import { IPatientHistory } from "../../../shared/interfaces/patient-history.interface"; // Import the patient history interface.


// Define the test suite for the PatientDashboardComponent.
describe("PatientDashboardComponent", () => {
    let component: PatientDashboardComponent;
    let fixture: ComponentFixture<PatientDashboardComponent>;
    
    // Define mock services and fake data.
    let mockMedicalRecordService: any;
    let mockAuthService: any;

    const fakePatient = { id: 10, username: 'j_doe', role: 'Patient' };

    const fakeHistory: IPatientHistory = {
        patientFullName: "John Doe",
        specialities: [
            {
                speciality: "Cardiology",
                episodes: [
                    {
                        episodeID: 50,
                        chiefComplaint: "Chest Pain",
                        isActive: true,
                        records: [
                            {
                                medicalRecordID: 200,
                                visitDate: "2024-03-30",
                                currentVersion: 1,
                                accessLogs: [], // Added to fix TS2345 error.
                                versions: [
                                    { ipfS_CID: "Qm123", version: 1, displayName: "Record.pdf", timestamp: "2024-03-30T10:00:00Z" }
                                ]
                            }
                        ]
                    }
                ]
            }
        ],
        patientID: 10
    };


    // Set up the testing module and mock service implementations.
    beforeEach(async () => {
        mockAuthService = {
            CurrentUser: fakePatient
        };

        mockMedicalRecordService = {
            getPatientHistory: jasmine.createSpy().and.returnValue(of(fakeHistory)),
            getMedicalRecordFromCID: jasmine.createSpy().and.returnValue(of({ diagnosis: 'Stable' })),
            checkTamperByCID: jasmine.createSpy().and.returnValue(of({ isTampered: false, status: 'Intact' })),
            downloadMedicalRecord: jasmine.createSpy().and.returnValue(of(new Blob())),
            verifyPdfTampering: jasmine.createSpy().and.returnValue(of({ isTampered: false }))
        };

        await TestBed.configureTestingModule({
            imports: [PatientDashboardComponent],
            providers: [
                provideHttpClient(),
                provideHttpClientTesting(),
                { provide: AuthService, useValue: mockAuthService },
                { provide: MedicalRecordService, useValue: mockMedicalRecordService }
            ],
            schemas: [NO_ERRORS_SCHEMA]
        }).compileComponents();

        fixture = TestBed.createComponent(PatientDashboardComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });


    // Basic instantiation check.
    it("Should create the service.", () => {
        expect(component).toBeTruthy();
    });


    // Method to verify patient data and history is loaded on init.
    it("Should load patient history and set full name on init.", () => {
        expect(mockMedicalRecordService.getPatientHistory).toHaveBeenCalledWith(10);
        expect(component.patientFullName).toBe("John Doe");
        expect(component.history?.specialities.length).toBe(1);
    });


    // Method to check sidebar toggle logic.
    it("Should toggle the sidebar collapsed state.", () => {
        expect(component.isCollapsed).toBeFalse();
        component.toggleSidebar();
        expect(component.isCollapsed).toBeTrue();
    });


    // Method to verify navigation state changes.
    it("Should update the active link when navigating.", () => {
        component.onNavigate("pdf");
        expect(component.activeLink).toBe("pdf");
    });


    // Method to verify asynchronous record viewing logic.
    it("Should fetch record data from IPFS when viewing a specific version.", () => {
        const mockVersion = fakeHistory.specialities[0].episodes[0].records[0].versions[0];
        
        component.viewVersion(mockVersion);
        
        expect(component.showViewModal).toBeTrue();
        expect(component.viewLoading).toBeFalse();
        expect(mockMedicalRecordService.getMedicalRecordFromCID).toHaveBeenCalledWith("Qm123", 10);
        expect(component.viewedRecord?.diagnosis).toBe("Stable");
    });


    // Method to check access log modal state.
    it("Should open the access log modal for a specific record.", () => {
        const mockRecord = fakeHistory.specialities[0].episodes[0].records[0];
        
        component.viewAccessLog(mockRecord);
        
        expect(component.showAccessLogModal).toBeTrue();
        expect(component.accessLogRecord).toEqual(mockRecord);
    });


    // Method to verify tamper check initialisation and API call.
    it("Should trigger tamper check and display results in modal.", () => {
        const mockVersion = fakeHistory.specialities[0].episodes[0].records[0].versions[0];
        
        component.checkTamper(mockVersion);
        
        expect(component.showTamperModal).toBeTrue();
        expect(mockMedicalRecordService.checkTamperByCID).toHaveBeenCalledWith("Qm123", 10);
        expect(component.tamperResult?.isTampered).toBeFalse();
    });


    // Method to verify the download workflow and file naming.
    it("Should handle PDF downloads and prevent concurrent downloads.", () => {
        const mockVersion = fakeHistory.specialities[0].episodes[0].records[0].versions[0];
        
        spyOn(URL, 'createObjectURL').and.returnValue('blob:url');
        spyOn(URL, 'revokeObjectURL');
        
        component.downloadRecord(mockVersion);
        
        expect(component.downloadingCID).toBeNull(); 
        expect(mockMedicalRecordService.downloadMedicalRecord).toHaveBeenCalledWith("Qm123", 10);
    });


    // Method to verify manual PDF verification logic.
    it("Should call verifyPdfTampering and update loading state when a file is uploaded.", () => {
        const mockFile = new File([""], "test.pdf");
        
        component.onPdfVerifyRequested(mockFile);
        
        expect(component.pdfVerifyLoading).toBeFalse();
        expect(mockMedicalRecordService.verifyPdfTampering).toHaveBeenCalledWith(10, mockFile);
    });


    // Method to check the clear functionality for PDF verification.
    it("Should clear PDF verification results.", () => {
        component.pdfVerifyResult = { isTampered: false } as any;
        component.onPdfClear();
        expect(component.pdfVerifyResult).toBeNull();
    });


    // Method to verify error extraction from various response shapes.
    it("Should extract error messages correctly from API responses.", () => {
        const errorObj = { error: "Access Denied" }; 
        const message = (component as any)._extractErrorMessage(errorObj);
        expect(message).toBe("Access Denied");
    });
    
    
    // Method to verify fallback message for unknown error shapes.
    it("Should return a default message if the error shape is unrecognised.", () => {
        const weirdError = { something: "else" };
        const message = (component as any)._extractErrorMessage(weirdError);
        expect(message).toBe("An unexpected error occurred. Please try again.");
    });
});