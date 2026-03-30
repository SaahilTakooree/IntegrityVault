// Import dependencies.
import { TestBed } from "@angular/core/testing"; // Import the main Angular testing utility.
import { HttpRequest, HttpHandlerFn, HttpErrorResponse, HttpEvent } from "@angular/common/http"; // Import HTTP types for interceptor testing.
import { authInterceptor } from "./auth.interceptor"; // Import the functional interceptor being tested.
import { AuthService } from "../services/auth/auth.service"; // Import the auth service dependency.
import { of, throwError } from "rxjs"; // RxJS utilities for mocking observable streams.


describe("AuthInterceptor", () => {
    // Mocked auth service.
    let authServiceSpy: jasmine.SpyObj<AuthService>;

    // Mock for the next interceptor handler.
    let nextSpy: jasmine.Spy;


    beforeEach(() => {
        // Initialise auth service spy.
        authServiceSpy = jasmine.createSpyObj("AuthService", ["getToken", "logout"]);

        // Initialise next handler spy as a standalone Jasmine spy.
        nextSpy = jasmine.createSpy("HttpHandlerFn");

        // Set up testing module.
        TestBed.configureTestingModule({
            providers: [
                { provide: AuthService, useValue: authServiceSpy },
            ],
        });
    });


    // Ensure no outstanding expectations remain.
    afterEach(() => {
        // Spies don't need verification like HttpTestingController, but keep the block for style consistency.
    });


    // Verifies that the interceptor adds a Bearer token to the Authorisation header for protected API calls.
    it("Should add Authorisation header for protected endpoints.", (done) => {
        const token = "mock-jwt-token";
        authServiceSpy.getToken.and.returnValue(token);

        const req = new HttpRequest("GET", "https://localhost:7018/api/User");
        nextSpy.and.returnValue(of({} as HttpEvent<any>));

        TestBed.runInInjectionContext(() => {
            authInterceptor(req, nextSpy as unknown as HttpHandlerFn).subscribe(() => {
                const interceptedReq = nextSpy.calls.mostRecent().args[0] as HttpRequest<any>;
                expect(interceptedReq.headers.has("Authorization")).toBeTrue();
                expect(interceptedReq.headers.get("Authorization")).toBe(`Bearer ${token}`);
                done();
            });
        });
    });


    // Ensures that public endpoints like ipify do not receive the Authorisation header even if a token exists.
    it("Should not add Authorisation header for ipify requests.", (done) => {
        authServiceSpy.getToken.and.returnValue("some-token");

        const req = new HttpRequest("GET", "https://api.ipify.org?format=json");
        nextSpy.and.returnValue(of({} as HttpEvent<any>));

        TestBed.runInInjectionContext(() => {
            authInterceptor(req, nextSpy as unknown as HttpHandlerFn).subscribe(() => {
                const interceptedReq = nextSpy.calls.mostRecent().args[0] as HttpRequest<any>;
                expect(interceptedReq.headers.has("Authorisation")).toBeFalse();
                done();
            });
        });
    });


    // Confirms that the login/authentication endpoint remains public and without headers.
    it("Should not add Authorisation header for Auth API requests.", (done) => {
        // Pass empty body as third argument to satisfy HttpRequest POST overload.
        const req = new HttpRequest("POST", "https://localhost:7018/api/Auth", {});
        nextSpy.and.returnValue(of({} as HttpEvent<any>));

        TestBed.runInInjectionContext(() => {
            authInterceptor(req, nextSpy as unknown as HttpHandlerFn).subscribe(() => {
                const interceptedReq = nextSpy.calls.mostRecent().args[0] as HttpRequest<any>;
                expect(interceptedReq.headers.has("Authorisation")).toBeFalse();
                done();
            });
        });
    });


    // Validates that a 401 Unauthorised response triggers the logout method in the AuthService.
    it("Should trigger logout on 401 error response.", (done) => {
        const req = new HttpRequest("GET", "/api/protected");
        const errorResponse = new HttpErrorResponse({ status: 401, statusText: "Unauthorised" });
        
        nextSpy.and.returnValue(throwError(() => errorResponse));

        TestBed.runInInjectionContext(() => {
            authInterceptor(req, nextSpy as unknown as HttpHandlerFn).subscribe({
                next: () => fail("Should have thrown an error."),
                error: (error) => {
                    expect(authServiceSpy.logout).toHaveBeenCalled();
                    expect(error.status).toBe(401);
                    done();
                }
            });
        });
    });


    // Ensures that errors other than 401 do not trigger a logout and are passed downstream.
    it("Should not trigger logout on 500 error response.", (done) => {
        const req = new HttpRequest("GET", "/api/protected");
        const errorResponse = new HttpErrorResponse({ status: 500, statusText: "Server Error" });
        
        nextSpy.and.returnValue(throwError(() => errorResponse));

        TestBed.runInInjectionContext(() => {
            authInterceptor(req, nextSpy as unknown as HttpHandlerFn).subscribe({
                next: () => fail("Should have thrown an error."),
                error: (error) => {
                    expect(authServiceSpy.logout).not.toHaveBeenCalled();
                    expect(error.status).toBe(500);
                    done();
                }
            });
        });
    });
});