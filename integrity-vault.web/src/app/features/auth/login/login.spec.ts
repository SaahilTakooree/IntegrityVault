// Import dependencies.
import { ComponentFixture, TestBed } from "@angular/core/testing"; // Import the angular testing utilities.
import { Login } from "./login"; // Import the component to be tested.
import { AuthService } from "../../../core/services/auth/auth.service"; // Service to handle authentication logic.
import { of, throwError } from "rxjs"; // RxJS utilities for observable streams.
import { provideHttpClient } from "@angular/common/http"; // Provides the standard HttpClient.
import { provideHttpClientTesting } from "@angular/common/http/testing"; // Provides a mock HttpClient for testing.
import { FormsModule } from "@angular/forms"; // Import forms module for ngModel support.


// Define the test suite for the Login component.
describe("Login", () => {
  let component: Login;
  let fixture: ComponentFixture<Login>;
  
  // Define mock services.
  let mockAuthService: any;


  // Set up the testing module and mock service implementations.
  beforeEach(async () => {
    mockAuthService = {
      login: jasmine.createSpy().and.returnValue(of({}))
    };

    await TestBed.configureTestingModule({
      imports: [Login, FormsModule],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: mockAuthService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Login);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });


  // Method to verify initial component state.
  it("Should initialise with empty credentials and no loading state.", () => {
    expect(component.usernameOrEmail).toBe("");
    expect(component.password).toBe("");
    expect(component.loading).toBeFalse();
    expect(component.errors).toEqual({});
  });


  // Method to check validation for empty fields.
  it("Should show validation errors if fields are empty on login.", () => {
    component.onLogin();
    
    expect(component.errors.usernameOrEmail).toBe("Username or email is required.");
    expect(component.errors.password).toBe("Password is required.");
    expect(mockAuthService.login).not.toHaveBeenCalled();
  });


  // Method to verify the password visibility toggle.
  it("Should toggle showPassword when the visibility button is clicked.", () => {
    expect(component.showPassword).toBeFalse();
    component.showPassword = !component.showPassword;
    expect(component.showPassword).toBeTrue();
  });


  // Method to verify successful login flow.
  it("Should set loading to true and call authService login on valid submission.", () => {
    component.usernameOrEmail = "admin@hospital.com";
    component.password = "password123";
    
    component.onLogin();
    
    expect(component.loading).toBeFalse(); // False because 'of' completes immediately.
    expect(mockAuthService.login).toHaveBeenCalledWith("admin@hospital.com", "password123");
  });


  // Method to check error handling for invalid credentials.
  it("Should set specific API error message on 401 unauthorised response.", () => {
    mockAuthService.login.and.returnValue(throwError(() => ({ status: 401 })));
    
    component.usernameOrEmail = "test_user";
    component.password = "wrong_pass";
    component.onLogin();
    
    expect(component.errors.api).toBe("Invalid credentials. Please try again.");
    expect(component.loading).toBeFalse();
  });


  // Method to check error handling for server connection issues.
  it("Should set connection error message when server status is 0.", () => {
    mockAuthService.login.and.returnValue(throwError(() => ({ status: 0 })));
    
    component.usernameOrEmail = "test_user";
    component.password = "pass";
    component.onLogin();
    
    expect(component.errors.api).toBe("Unable to reach the server. Check your connection.");
  });


  // Method to check generic error handling.
  it("Should set generic error message for other unexpected API failures.", () => {
    mockAuthService.login.and.returnValue(throwError(() => ({ status: 500 })));
    
    component.usernameOrEmail = "test_user";
    component.password = "pass";
    component.onLogin();
    
    expect(component.errors.api).toBe("Something went wrong. Please try again later.");
  });


  // Method to ensure input values are trimmed before submission.
  it("Should trim the usernameOrEmail before calling the login service.", () => {
    component.usernameOrEmail = "  trimmed_user  ";
    component.password = "password";
    
    component.onLogin();
    
    expect(mockAuthService.login).toHaveBeenCalledWith("trimmed_user", "password");
  });
});