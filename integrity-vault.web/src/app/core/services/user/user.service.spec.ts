// Import dependencies.
import { TestBed } from "@angular/core/testing"; // Import the main Angular testing utility.
import { UserService } from "./user.service"; // Import the service being tested.
import { provideHttpClient } from "@angular/common/http"; // Provides standard HTTP client.
import { provideHttpClientTesting, HttpTestingController } from "@angular/common/http/testing"; // Tools to mock HTTP requests.
import { IAdmin } from "../../../shared/interfaces/admin.interface"; // Import the admin interface.
import { IDoctor } from "../../../shared/interfaces/doctor.interface"; // Import the doctor interface.
import { IPatient } from "../../../shared/interfaces/patient.interface"; // Import the patient interface.
import { IExternalProvider } from "../../../shared/interfaces/external-provider.interface"; // Import the external provider interface.
import { UserRole } from "../../../shared/enums/user-role.enum" // Import the user role enum.


describe("UserService", () => {
    // Instance of the service.
    let service: UserService;

    // Controller to intercept HTTP calls.
    let httpMock: HttpTestingController;

    // Base API URL for verification.
    const apiUrl = "https://localhost:7018/api/User";


    beforeEach(() => {
        // Set up testing module.
        TestBed.configureTestingModule({
            providers: [
                provideHttpClient(),
                provideHttpClientTesting(),
                UserService,
            ],
        });

        // Get service instance.
        service = TestBed.inject(UserService);

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


    // Validates that the service retrieves all users and correctly appends the hospitalId query parameter when provided.
    it("Should fetch all users with a hospitalId parameter.", () => {
        const mockUsers: IAdmin[] = [{ id: 1, username: "admin1", hospitalID: 42 } as IAdmin];

        service.getAllUsers(42).subscribe(users => {
            expect(users.length).toBe(1);
            expect(users).toEqual(mockUsers);
        });

        const req = httpMock.expectOne(request => 
            request.url === apiUrl && request.params.get("hospitalId") === "42"
        );
        expect(req.request.method).toBe("GET");
        req.flush(mockUsers);
    });


    // Confirms that the service can retrieve any user type by their specific ID.
    it("Should fetch a user by ID.", () => {
        const mockDoctor = { id: 10, username: "dr_smith", role: UserRole.Doctor } as IDoctor;

        service.getUserById(10).subscribe(user => {
            expect(user.id).toBe(10);
            expect(user.username).toBe("dr_smith");
        });

        const req = httpMock.expectOne(`${apiUrl}/10`);
        expect(req.request.method).toBe("GET");
        req.flush(mockDoctor);
    });


    // Verifies that a new doctor record is correctly posted to the specialised doctor endpoint.
    it("Should create a new doctor.", () => {
        const newDoctor = { username: "new_doc", role: UserRole.Doctor } as IDoctor;

        service.createDoctor(newDoctor).subscribe(result => {
            expect(result).toBeTrue();
        });

        const req = httpMock.expectOne(`${apiUrl}/doctor`);
        expect(req.request.method).toBe("POST");
        expect(req.request.body).toEqual(newDoctor);
        req.flush(true);
    });


    // Ensures that patient data is sent to the correct patient creation route.
    it("Should create a new patient.", () => {
        const newPatient = { username: "patient_001", role: UserRole.Patient } as IPatient;

        service.createPatient(newPatient).subscribe(result => {
            expect(result).toBeTrue();
        });

        const req = httpMock.expectOne(`${apiUrl}/patient`);
        expect(req.request.method).toBe("POST");
        req.flush(true);
    });


    // Validates the update functionality for an existing doctor using the PATCH method.
    it("Should update an existing doctor.", () => {
        const updatedDoctor = { id: 5, username: "doc_updated" } as IDoctor;

        service.updateDoctor(5, updatedDoctor).subscribe(result => {
            expect(result).toBeTrue();
        });

        const req = httpMock.expectOne(`${apiUrl}/doctor/5`);
        expect(req.request.method).toBe("PATCH");
        expect(req.request.body).toEqual(updatedDoctor);
        req.flush(true);
    });


    // Checks that the external provider update correctly targets the externalprovider endpoint with the correct ID.
    it("Should update an existing external provider.", () => {
        const updatedProvider = { id: 8, username: "provider_ext" } as IExternalProvider;

        service.updateExternalProvider(8, updatedProvider).subscribe(result => {
            expect(result).toBeTrue();
        });

        const req = httpMock.expectOne(`${apiUrl}/externalprovider/8`);
        expect(req.request.method).toBe("PATCH");
        req.flush(true);
    });


    // Verifies that a user deletion request is sent to the base user endpoint with the appropriate ID.
    it("Should delete a user by ID.", () => {
        service.deleteUser(99).subscribe(result => {
            expect(result).toBeTrue();
        });

        const req = httpMock.expectOne(`${apiUrl}/99`);
        expect(req.request.method).toBe("DELETE");
        req.flush(true);
    });


    // Ensures that the service returns a 500 error status if the server fails during user creation.
    it("Should handle server error (500) gracefully during user creation.", () => {
        const admin = { username: "admin" } as IAdmin;

        service.createAdmin(admin).subscribe({
            next: () => fail("Should have failed with 500"),
            error: (error) => {
                expect(error.status).toBe(500);
            }
        });

        const req = httpMock.expectOne(`${apiUrl}/admin`);
        req.flush("Server Error", { status: 500, statusText: "Internal Server Error" });
    });
});