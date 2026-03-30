// Import dependencies.
import { TestBed } from "@angular/core/testing"; // Import the main Angular testing utility.
import { HospitalService } from "./hospital.service"; // Import the service  being tested.
import { IHospital } from "../../../shared/interfaces/hospital.interface"; // Import the hospital interface to be use in the test.
import { provideHttpClient } from "@angular/common/http"; // Provides standard HTTP client.
import { provideHttpClientTesting, HttpTestingController } from "@angular/common/http/testing"; // Tools to mock HTTP requests.


describe("HospitalService", () => {
    // Instance of the service.
    let service: HospitalService;

    // Controller to intercept HTTP calls.
    let httpMock: HttpTestingController;

    // Base API URL for verification.
    const apiUrl = "https://localhost:7018/api/Hospital";

    // Helper to create a mock hospital object.
    function createMockHospital(id: number, name: string): IHospital {
        return {
            id: id,
            name: name,
            walletAddress: "0x123abc",
            ipAddresses: ["192.168.1.1"],
        };
    }


    beforeEach(() => {
        // Set up testing module.
        TestBed.configureTestingModule({
            providers: [
                provideHttpClient(),
                provideHttpClientTesting(),
                HospitalService,
            ],
        });

        // Get service instance.
        service = TestBed.inject(HospitalService);

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


    // Validates that the service retrieves a full list of hospitals via a GET request.
    it("Should fetch all hospitals.", () => {
        const mockHospitals: IHospital[] = [
            createMockHospital(1, "General Hospital"),
            createMockHospital(2, "City Clinic")
        ];

        service.getHospital().subscribe(hospitals => {
            expect(hospitals.length).toBe(2);
            expect(hospitals).toEqual(mockHospitals);
        });

        const req = httpMock.expectOne(apiUrl);
        expect(req.request.method).toBe("GET");
        req.flush(mockHospitals);
    });


    // Confirms that the service can retrieve a single hospital's data by its unique identifier.
    it("Should fetch a hospital by ID.", () => {
        const mockHospital = createMockHospital(42, "St. Jude");

        service.getHospitalById(42).subscribe(hospital => {
            expect(hospital.name).toBe("St. Jude");
            expect(hospital.id).toBe(42);
        });

        const req = httpMock.expectOne(`${apiUrl}/42`);
        expect(req.request.method).toBe("GET");
        req.flush(mockHospital);
    });


    // Verifies that sending a new hospital object via POST returns the created resource.
    it("Should add a new hospital.", () => {
        const newHospital = createMockHospital(0, "New Hospital");
        const savedHospital = createMockHospital(101, "New Hospital");

        service.addHospital(newHospital).subscribe(hospital => {
            expect(hospital.id).toBe(101);
            expect(hospital.name).toBe("New Hospital");
        });

        const req = httpMock.expectOne(apiUrl);
        expect(req.request.method).toBe("POST");
        expect(req.request.body).toEqual(newHospital);
        req.flush(savedHospital);
    });


    // Ensures that the service correctly issues a PATCH request to update existing hospital records.
    it("Should update an existing hospital.", () => {
        const updatedData = createMockHospital(1, "Updated Name");

        service.updateHospital(updatedData).subscribe(hospital => {
            expect(hospital.name).toBe("Updated Name");
        });

        const req = httpMock.expectOne(`${apiUrl}/1`);
        expect(req.request.method).toBe("PATCH");
        expect(req.request.body).toEqual(updatedData);
        req.flush(updatedData);
    });


    // Checks that the service correctly handles the deletion of a hospital record by ID.
    it("Should delete a hospital by ID.", () => {
        service.deleteHospital(1).subscribe(response => {
            expect(response).toBeNull();
        });

        const req = httpMock.expectOne(`${apiUrl}/1`);
        expect(req.request.method).toBe("DELETE");
        req.flush(null);
    });


    // Validates the error handling if the API returns a 404 for a non-existent hospital.
    it("Should handle 404 error when fetching non-existent hospital.", () => {
        service.getHospitalById(999).subscribe({
            next: () => fail("Should have failed with 404"),
            error: (error) => {
                expect(error.status).toBe(404);
            }
        });

        const req = httpMock.expectOne(`${apiUrl}/999`);
        req.flush("Not Found", { status: 404, statusText: "Not Found" });
    });
});