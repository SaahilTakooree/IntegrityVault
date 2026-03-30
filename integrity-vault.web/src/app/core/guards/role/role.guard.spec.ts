// Import dependencies.
import { TestBed } from "@angular/core/testing"; // Import the main Angular testing utility.
import { ActivatedRouteSnapshot, Router, RouterStateSnapshot } from "@angular/router"; // Provides route information and navigation.
import { roleGuard } from "./role.guard"; // Import the role-based guard being tested.
import { AuthService } from "../../services/auth/auth.service"; // Handles authentication logic.
import { UserRole } from "../../../shared/enums/user-role.enum"; // Import the user role enum for type safety.
import { UserSession } from "../../../shared/interfaces/user-session.interface"; // Import the UserSession interface for type casting.


describe("RoleGuard", () => {
    let authServiceSpy: Partial<AuthService>;
    let routerSpy: jasmine.SpyObj<Router>;
    let currentUser: UserSession | null;


    beforeEach(() => {
        currentUser = null;

        authServiceSpy = {
            get CurrentUser() {
                return currentUser;
            }
        } as AuthService;

        routerSpy = jasmine.createSpyObj("Router", ["navigate"]);

        TestBed.configureTestingModule({
            providers: [
                { provide: AuthService, useValue: authServiceSpy },
                { provide: Router, useValue: routerSpy },
            ],
        });
    });


    // Verifies that the guard returns true when the user's role exists within the route's allowed roles array.
    it("Should allow access (return true) when user role is authorised.", () => {
        currentUser = { role: String(UserRole.Admin), id: 1, hospitalId: 42, lastName: "Doe" };

        const mockRoute = { data: { roles: [UserRole.Admin, UserRole.SuperAdmin] } } as unknown as ActivatedRouteSnapshot;

        const result = TestBed.runInInjectionContext(() => roleGuard(mockRoute, {} as RouterStateSnapshot));

        expect(result).toBeTrue(); 
        expect(routerSpy.navigate).not.toHaveBeenCalled();
    });


    // Ensures that the guard blocks access and returns false when the user's role is not included in the allowed list.
    it("Should block access (return false) when user role is not authorised.", () => {
        currentUser = { 
            role: UserRole.Patient as unknown as string, 
            id: 0, 
            hospitalId: 0, 
            lastName: "Test" 
        };

        const mockRoute = { data: { roles: [UserRole.Admin] } } as unknown as ActivatedRouteSnapshot;

        const result = TestBed.runInInjectionContext(() => roleGuard(mockRoute, {} as any));

        expect(result).toBeFalse();
    });


    // Validates that there is a redirect to /login if role is not authorised.
    it("Should redirect to /login when user role is not authorised.", () => {
        currentUser = { 
            role: UserRole.Doctor as unknown as string, 
            id: 1, 
            hospitalId: 42, 
            lastName: "Smith" 
        };

        const mockRoute = { data: { roles: [UserRole.Admin] } } as unknown as ActivatedRouteSnapshot;

        TestBed.runInInjectionContext(() => roleGuard(mockRoute, {} as any));

        expect(routerSpy.navigate).toHaveBeenCalledWith(["/login"]);
    });


    // Validates that the guard correctly handles scenarios where no user is logged in (CurrentUser is null).
    it("Should block access and redirect if CurrentUser is null.", () => {
        currentUser = null;

        const mockRoute = { data: { roles: [UserRole.Admin] } } as unknown as ActivatedRouteSnapshot;

        const result = TestBed.runInInjectionContext(() => roleGuard(mockRoute, {} as any));

        expect(result).toBeFalse();
        expect(routerSpy.navigate).toHaveBeenCalledWith(["/login"]);
    });


    // Validates that the guard correctly handles scenarios where there is no role.
    it("Should block access if the route has no roles defined in data.", () => {
        currentUser = { 
            role: UserRole.Admin as unknown as string, 
            id: 3, 
            hospitalId: 67, 
            lastName: "Jo" 
        };

        const mockRoute = { data: {} } as unknown as ActivatedRouteSnapshot;

        const result = TestBed.runInInjectionContext(() => roleGuard(mockRoute, {} as any));

        expect(result).toBeFalse();
        expect(routerSpy.navigate).toHaveBeenCalledWith(["/login"]);
    });
});