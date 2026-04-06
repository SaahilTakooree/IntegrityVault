// Import dependencies.
import { ComponentFixture, TestBed } from "@angular/core/testing"; // Import the angular testing utilities.
import { TopbarComponent } from "./topbar"; // Import the component to be tested.
import { AuthService } from "../../../core/services/auth/auth.service"; // Service to check authentication state.
import { provideHttpClient } from "@angular/common/http"; // Provides the standard HttpClient.
import { provideHttpClientTesting } from "@angular/common/http/testing"; // Provides a mock HttpClient.
import { By } from "@angular/platform-browser"; // Utility to query the DOM.
import { Component } from "@angular/core"; // Required for content projection testing.


// Host component to test content projection.
@Component({
    standalone: true,
    imports: [TopbarComponent],
    template: `<app-topbar message="Test"><span id="test-content">Projected Content</span></app-topbar>`
})
class TestHostComponent {}


// Define the test suite for the TopbarComponent.
describe("TopbarComponent", () => {
    let component: TopbarComponent;
    let fixture: ComponentFixture<TopbarComponent>;
    
    // Define mock services.
    let mockAuthService: any;

    beforeEach(async () => {
        mockAuthService = {
            logout: jasmine.createSpy("logout"),
            CurrentUser: { username: "test_user" }
        };

        await TestBed.configureTestingModule({
            imports: [TopbarComponent, TestHostComponent],
            providers: [
                provideHttpClient(),
                provideHttpClientTesting(),
                { provide: AuthService, useValue: mockAuthService }
            ]
        })
        .overrideComponent(TopbarComponent, {
            set: {
                providers: [{ provide: AuthService, useValue: mockAuthService }]
            }
        })
        .compileComponents();

        fixture = TestBed.createComponent(TopbarComponent);
        component = fixture.componentInstance;
        
        component.message = "Hi";
        component.showLogout = false;
        
        fixture.detectChanges();
    });


    // Basic instantiation check.
    it("Should create the component.", () => {
        expect(component).toBeTruthy();
    });


    // Method to verify the default message rendering.
    it("Should display the default 'Hi' message.", () => {
        const compiled = fixture.nativeElement as HTMLElement;
        expect(compiled.textContent).toContain("Hi");
    });


    // Method to verify custom message input rendering.
    it("Should display a custom message when input is provided.", () => {
        component.message = "Welcome back, Admin";
        fixture.detectChanges();
        
        const compiled = fixture.nativeElement as HTMLElement;
        expect(compiled.textContent).toContain("Welcome back, Admin");
    });


    // Method to check the sidebar toggle event emission.
    it("Should emit toggleSidebar when the toggle button is clicked.", () => {
        spyOn(component.toggleSidebar, "emit");
        
        // The toggle button only exists when showLogout is false.
        const toggleBtn = fixture.debugElement.query(By.css("button.btn-light"));
        expect(toggleBtn).withContext("Sidebar toggle button should exist").toBeTruthy();
        
        toggleBtn.triggerEventHandler("click", null);
        expect(component.toggleSidebar.emit).toHaveBeenCalled();
    });


    // Method to verify the logout functionality triggers the service.
    it("Should call authService logout when the logout button is clicked.", () => {
        component.showLogout = true;
        fixture.detectChanges();
        
        const logoutLink = fixture.debugElement.query(By.css(".topbar-logout-link"));
        expect(logoutLink).withContext("Logout link should be visible when showLogout is true").toBeTruthy();
        
        logoutLink.triggerEventHandler("click", null);
        expect(mockAuthService.logout).toHaveBeenCalled();
    });


    // Method to verify content projection via ng-content.
    it("Should project content provided in the component body.", () => {
        const hostFixture = TestBed.createComponent(TestHostComponent);
        hostFixture.detectChanges();
        
        const projectedElement = hostFixture.debugElement.query(By.css("#test-content"));
        expect(projectedElement).withContext("Projected content should be found inside the topbar").toBeTruthy();
        expect(projectedElement.nativeElement.textContent).toBe("Projected Content");
    });
});