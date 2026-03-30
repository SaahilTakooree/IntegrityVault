// Import dependencies.
import { TestBed } from "@angular/core/testing"; // Import the main Angular testing utility.
import { MedicalRecordService } from "./medical-record.service"; // Import the service being tested.
import { provideHttpClient } from "@angular/common/http"; // Provides standard HTTP client.
import { provideHttpClientTesting, HttpTestingController } from "@angular/common/http/testing"; // Tools to mock HTTP requests.
import { CreateMedicalRecord } from "../../../shared/interfaces/create-medical-record.interface"; // Import record creation interface.
import { IDoctorHistory } from "../../../shared/interfaces/doctor-history.interface"; // Import doctor history interface.
import { ITamperResult } from "../../../shared/interfaces/tamper-result.interface"; // Import tamper result interface.


describe("MedicalRecordService", () => {
    // Instance of the service.
    let service: MedicalRecordService;

    // Controller to intercept HTTP calls.
    let httpMock: HttpTestingController;

    // Base API URLs for verification.
    const apiUrl = "https://localhost:7018/api/MedicalRecord";
    const userApiUrl = "https://localhost:7018/api/User";

    // Helper to generate a valid tamper result object.
    function createMockTamperResult(isTampered: boolean): ITamperResult {
        return {
            isTampered: isTampered,
            status: isTampered ? "Tampered" : "Intact",
            contentHashMatch: !isTampered,
            databaseHashMatch: !isTampered,
            cidMatch: !isTampered,
            versionHashMatch: !isTampered,
            message: isTampered ? "Tampering detected." : "Record is valid."
        };
    }


    beforeEach(() => {
        // Set up testing module.
        TestBed.configureTestingModule({
            providers: [
                provideHttpClient(),
                provideHttpClientTesting(),
                MedicalRecordService,
            ],
        });

        // Get service instance.
        service = TestBed.inject(MedicalRecordService);

        // Get HTTP controller.
        httpMock = TestBed.inject(HttpTestingController);
    });


    // Ensure no outstanding HTTP requests remain.
    afterEach(() => {
        httpMock.verify();
    });


    // Basic instantiation check.
    it("Should create the service.", () => {
        expect(service).toBeTruthy();
    });


    // Validates that the service correctly posts data to create both a medical record and a new episode.
    it("Should create a new medical record and episode.", () => {
        const mockData: CreateMedicalRecord = { patientID: 1, diagnosis: "Flu" } as any;

        service.createNewMedicalRecordAndEpisode(mockData).subscribe(res => {
            expect(res).toBeTruthy();
        });

        const req = httpMock.expectOne(apiUrl);
        expect(req.request.method).toBe("POST");
        expect(req.request.body).toEqual(mockData);
        req.flush({ success: true });
    });


    // Confirms that the service targets the specific episode endpoint when adding a record to an existing chain.
    it("Should add a medical record to an existing episode.", () => {
        const mockData: CreateMedicalRecord = { diagnosis: "Updated Flu" } as any;

        service.addMedicalRecordToEpisode(50, mockData).subscribe();

        const req = httpMock.expectOne(`${apiUrl}/episode/50`);
        expect(req.request.method).toBe("POST");
        req.flush({});
    });


    // Verifies that a PATCH request is issued with the correct URL parameters for updating a specific record.
    it("Should update an existing medical record.", () => {
        const mockData: CreateMedicalRecord = { diagnosis: "Revised" } as any;

        service.updateMedicalRecord(200, 50, mockData).subscribe();

        const req = httpMock.expectOne(`${apiUrl}/episode/50/200`);
        expect(req.request.method).toBe("PATCH");
        req.flush({});
    });


    // Checks that the doctor's medical history is retrieved from the correct specialised history endpoint.
    it("Should fetch full doctor history by doctor ID.", () => {
        const mockHistory: IDoctorHistory = { 
            doctorID: 7, 
            doctorFullName: "Dr. Smith", 
            patients: [] 
        };
        
        service.getDoctorHistory(7).subscribe(history => {
            expect(history.doctorFullName).toBe("Dr. Smith");
            expect(history.doctorID).toBe(7);
        });
        
        const req = httpMock.expectOne(`${apiUrl}/doctor/7/history`);
        expect(req.request.method).toBe("GET");
        req.flush(mockHistory);
    });


    // Validates the retrieval of medical record data from IPFS using a CID and user ID.
    it("Should fetch medical record content from IPFS via CID.", () => {
        service.getMedicalRecordFromCID("Qm123", 7).subscribe();

        const req = httpMock.expectOne(`${apiUrl}/ipfs/Qm123/user/7`);
        expect(req.request.method).toBe("GET");
        req.flush({});
    });


    // Verifies the tamper-check logic for a record stored on IPFS.
    it("Should check if an IPFS record is tampered.", () => {
        const mockTamper = createMockTamperResult(false);

        service.checkTamperByCID("Qm123", 7).subscribe(result => {
            expect(result.isTampered).toBeFalse();
            expect(result.status).toBe("Intact");
        });
        
        const req = httpMock.expectOne(`${apiUrl}/ipfs/Qm123/user/7/tamper-check`);
        expect(req.request.method).toBe("GET");
        req.flush(mockTamper);
    });


    // Confirms that PDF files are correctly uploaded via FormData for manual tamper verification.
    it("Should verify PDF tampering using FormData.", () => {
        const mockFile = new File([""], "test.pdf", { type: "application/pdf" });
        const mockTamper = createMockTamperResult(false);

        service.verifyPdfTampering(7, mockFile).subscribe(result => {
            expect(result.isTampered).toBeFalse();
        });

        const req = httpMock.expectOne(`${apiUrl}/pdf/tamper-check/user/7`);
        expect(req.request.method).toBe("POST");
        expect(req.request.body instanceof FormData).toBeTrue();
        req.flush(mockTamper);
    });


    // Checks that the download method requests a blob response type for PDF retrieval.
    it("Should download a medical record as a blob.", () => {
        service.downloadMedicalRecord("Qm123", 7).subscribe(blob => {
            expect(blob instanceof Blob).toBeTrue();
        });

        const req = httpMock.expectOne(`${apiUrl}/ipfs/Qm123/user/7/download`);
        expect(req.request.responseType).toBe("blob");
        req.flush(new Blob());
    });


    // Validates that the episode status can be toggled and that the doctor ID is sent in the body.
    it("Should update episode status.", () => {
        service.setEpisodeStatus(50, 7).subscribe();

        const req = httpMock.expectOne(`${apiUrl}/episode/50/status`);
        expect(req.request.method).toBe("PATCH");
        expect(req.request.body).toBe(7);
        req.flush({});
    });


    // Ensures that the service retrieves hospital-specific patient lists from the user API.
    it("Should fetch all patients for a specific hospital.", () => {
        service.getPatientsForHospital(101).subscribe();

        const req = httpMock.expectOne(`${userApiUrl}/patient/101`);
        expect(req.request.method).toBe("GET");
        req.flush([]);
    });


    // Verifies that a doctor's profile can be retrieved by their unique ID.
    it("Should fetch doctor details by ID.", () => {
        service.getDoctorById(7).subscribe();

        const req = httpMock.expectOne(`${userApiUrl}/7`);
        expect(req.request.method).toBe("GET");
        req.flush({});
    });
});