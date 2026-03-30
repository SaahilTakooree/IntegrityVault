// Import dependencies.
import { TestBed } from "@angular/core/testing"; // Import the main Angular testing utility.
import { Router } from "@angular/router"; // Provides navigation functionality.
import { authGuard } from "./auth.guard"; // Import the guard being tested.
import { AuthService } from "../../services/auth/auth.service"; // Handles authentication logic.


describe("AuthGuard", () => {
    // Mocked auth service.
    let authServiceSpy: jasmine.SpyObj<AuthService>;

    // Mocked router to track navigation.
    let routerSpy: jasmine.SpyObj<Router>;


    beforeEach(() => {
        // Initialise auth service spy.
        authServiceSpy = jasmine.createSpyObj("AuthService", ["IsLoggedIn"]);

        // Initialise router spy.
        routerSpy = jasmine.createSpyObj("Router", ["navigate"]);

        // Set up testing module.
        TestBed.configureTestingModule({
            providers: [
                { provide: AuthService, useValue: authServiceSpy },
                { provide: Router, useValue: routerSpy },
            ],
        });
    });


    // Verifies that the guard returns true and allows the navigation to proceed when the user is authenticated.
    it("Should allow access (return true) when user is logged in.", () => {
        authServiceSpy.IsLoggedIn.and.returnValue(true);

        // Execute the guard within the injection context.
        const result = TestBed.runInInjectionContext(() => authGuard({} as any, {} as any));

        expect(result).toBeTrue();
        expect(routerSpy.navigate).not.toHaveBeenCalled();
    });


    // Ensures that the guard returns false and prevents navigation when no valid session is detected.
    it("Should block access (return false) when user is not logged in.", () => {
        authServiceSpy.IsLoggedIn.and.returnValue(false);

        const result = TestBed.runInInjectionContext(() => authGuard({} as any, {} as any));

        expect(result).toBeFalse();
    });


    // Confirms that an unauthenticated attempt triggers a redirection to the login page to protect the route.
    it("Should redirect to /login when user is not logged in.", () => {
        authServiceSpy.IsLoggedIn.and.returnValue(false);

        TestBed.runInInjectionContext(() => authGuard({} as any, {} as any));

        expect(routerSpy.navigate).toHaveBeenCalledWith(["/login"]);
    });


    // Validates that the guard's logic is correctly isolated from other router states.
    it("Should call IsLoggedIn() exactly once during check.", () => {
        authServiceSpy.IsLoggedIn.and.returnValue(true);

        TestBed.runInInjectionContext(() => authGuard({} as any, {} as any));

        expect(authServiceSpy.IsLoggedIn).toHaveBeenCalledTimes(1);
    });
});