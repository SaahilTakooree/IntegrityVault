// Import dependencies.
import { TestBed } from "@angular/core/testing"; // Import the main Angular testing utility.
import { AuthService } from "./auth.service"; // Import the service being tested.
import { provideHttpClient } from "@angular/common/http"; // Provides standard HTTP client.
import { provideHttpClientTesting, HttpTestingController } from "@angular/common/http/testing"; // Tools to mock HTTP requests.
import { Router } from "@angular/router"; // Angular router for navigation testing.
import { skip, take, toArray } from "rxjs"; // RxJS operators for stream manipulation.


describe("AuthService", () => {
    // Instance of the service.
    let service: AuthService;

    // Controller to intercept HTTP calls.
    let httpMock: HttpTestingController;

    // Mocked router to track navigation.
    let routerSpy: jasmine.SpyObj<Router>;

    // Helper to generate a mock JWT.
    function generateTokenWithRole(role: string, isExpired: boolean = false, isMalformed: boolean = false): string {
        // Return string for error testing.
        if (isMalformed) return "This is not a jwt.";

        // Set timestamp in past or future.
        const expiration = isExpired 
            ? Math.floor(Date.now() / 1000) - 1000 
            : Math.floor(Date.now() / 1000) + 1000;

        // Standard JWT payload structure.
        const payload = {
            exp: expiration,
            "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name": "1",
            "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": role,
            HospitalID: 42,
            LastName: "Doe",
        };

        // Encode payload to Base64.
        const base64Payload = btoa(JSON.stringify(payload))
        .replace(/\+/g, "-").replace(/\//g, "_").replace(/=+$/, "");

        // Return full mock token.
        return `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.${base64Payload}.signature`;
    }

    
    beforeEach(() => {
        // Initialise router spy.
        routerSpy = jasmine.createSpyObj("Router", ["navigate"]);

        // Ensure clean state for every test.
        sessionStorage.clear();

        // Set up testing module.
        TestBed.configureTestingModule({
            providers: [
                provideHttpClient(),
                provideHttpClientTesting(),
                AuthService,
                { provide: Router, useValue: routerSpy },
            ],
        });

        // Get service instance.
        service = TestBed.inject(AuthService);

        // Get HTTP controller.
        httpMock = TestBed.inject(HttpTestingController);
    });


    // Ensure no outstanding HTTP requests remain.
    afterEach(() => {
        httpMock.verify();
    });


    // Basic instantiation check.
    it("should create the service.", () => {
        expect(service).toBeTruthy();
    });


    // Validates that logging in triggers a new emission from the user observable containing the correct profile data.
    it("Should emit the user session via user$ observable on login.", (done) => {
        const token = generateTokenWithRole("Admin");
        
        service.user$.pipe(skip(1), take(1)).subscribe(user => {
        expect(user?.role).toBe("Admin");
        expect(user?.lastName).toBe("Doe");
        done();
        });

        // Cast to any to access the private success handler for direct state testing.
        (service as any)._handleLoginSuccess(token);
    });


    // Confirms that the user observable correctly transitions from a logged-in state to null upon logout.
    it("Should emit null via user$ observable on logout.", (done) => {
        const token = generateTokenWithRole("Admin");
        (service as any)._handleLoginSuccess(token); 

        service.user$.pipe(
            take(2), 
            toArray()
        ).subscribe({
            next: (emissions) => {
            expect(emissions[0]?.role).toBe("Admin");
            expect(emissions[1]).toBeNull();
            done();
            },
            error: done.fail
        });

        service.logout();
    });


    // Tests that the login status remains false if the session storage contains a non-JWT string.
    it("Should return false for isLoggedIn() if token is malformed.", () => {
        sessionStorage.setItem("token", "Invalid string.");
        expect(service.IsLoggedIn()).toBeFalse();
    });


    // Ensures the login process halts and throws an error if the preliminary IP address check fails.
    it("Should handle IP address fetch failure gracefully.", () => {
        service.login("user", "pass").subscribe({
            next: () => fail("Login should have failed because IP fetch failed."),
            error: (err) => {
                expect(err.status).toBe(500);
            }
        });

        const ipReq = httpMock.expectOne("https://api.ipify.org?format=json");
        ipReq.flush("Network Error", { status: 500, statusText: "Internal Server Error." });
        
        httpMock.expectNone("https://localhost:7018/api/Auth");
    });


    // Verifies that the token decoder handles missing optional data fields without crashing the service.
    it("Should handle missing optional claims (HospitalID) during decoding.", () => {
        const payload = {
        exp: Math.floor(Date.now() / 1000) + 1000,
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name": "1",
        "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": "Doctor",
        };
        const token = `header.${btoa(JSON.stringify(payload))}.sig`;
        
        const user = (service as any)._decodeToken(token);
        expect(user.hospitalId).toBeNull();
    });


    // Definition of roles and their corresponding dashboard route expectations.
    const roles = [
        { role: "SuperAdmin", expected: "/superadmin" },
        { role: "Admin", expected: "/admin" },
        { role: "Doctor", expected: "/doctor" },
        { role: "Patient", expected: "/patient" },
        { role: "ExternalProvider", expected: "/external-provider" },
        { role: "Unknown", expected: "/login" }
    ];

    // Iterates through each role to ensure the application navigates to the specific dashboard authorised for that user.
    roles.forEach(({ role, expected }) => {
        it(`Should navigate to ${expected} when role is ${role}.`, () => {
            const token = generateTokenWithRole(role);
            (service as any)._handleLoginSuccess(token);
            expect(routerSpy.navigate).toHaveBeenCalledWith([expected]);
        });
    });


    // Validates that a successful login correctly updates both local storage and the service's internal state.
    it("Should store token and user on successful login.", () => {
        service.login("user", "pass").subscribe();

        httpMock.expectOne("https://api.ipify.org?format=json").flush({ ip: "1.1.1.1" });
        
        const token = generateTokenWithRole("Admin");
        httpMock.expectOne("https://localhost:7018/api/Auth").flush({ token });

        expect(sessionStorage.getItem("token")).toBe(token);
        expect(service.CurrentUser?.role).toBe("Admin");
    });


    // Checks that logging out removes all sensitive session data and returns the user to the login screen.
    it("Should logout correctly.", () => {
        sessionStorage.setItem("token", "some-token");
        service.logout();
        expect(sessionStorage.getItem("token")).toBeNull();
        expect(service.CurrentUser).toBeNull();
        expect(routerSpy.navigate).toHaveBeenCalledWith(["/login"]);
    });


    // Verifies that the service identifies a token as invalid once its expiration timestamp has passed.
    it("Should return false for isLoggedIn() when token is expired.", () => {
        const token = generateTokenWithRole("Admin", true);
        sessionStorage.setItem("token", token);
        expect(service.IsLoggedIn()).toBeFalse();
    });


    // Ensures that the service automatically wipes any pre-existing expired tokens from the storage during startup.
    it("Should clear session if token is expired during service initialisation.", () => {
        const expiredToken = generateTokenWithRole("Admin", true);
        sessionStorage.setItem("token", expiredToken);

        const freshService = TestBed.runInInjectionContext(() => new AuthService());
        
        expect(sessionStorage.getItem("token")).toBeNull();
        expect(freshService.CurrentUser).toBeNull();
    });


    // Validates that an unauthorised login attempt does not persist any data or navigate away from the login page.
    it("Should not store token and should return an error on failed login (401).", (done) => {
        service.login("wrong-user", "wrong-pass").subscribe({
        next: () => {
            fail("Should have failed with 401");
        },
        error: (error) => {
            expect(error.status).toBe(401);
            expect(sessionStorage.getItem("token")).toBeNull();
            expect(service.CurrentUser).toBeNull();
            expect(routerSpy.navigate).not.toHaveBeenCalled();
            done();
        }
        });

        httpMock.expectOne("https://api.ipify.org?format=json").flush({ ip: "1.1.1.1" });

        const authReq = httpMock.expectOne("https://localhost:7018/api/Auth");
        authReq.flush("Invalid credentials", { status: 401, statusText: "Unauthorised" });
    });
});