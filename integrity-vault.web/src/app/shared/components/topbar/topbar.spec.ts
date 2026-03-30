// Import dependencies.
import { ComponentFixture, TestBed } from "@angular/core/testing"; // Import Angular testing utilities.
import { TopbarComponent } from "./topbar"; // Import the component being tested.
import { By } from "@angular/platform-browser"; // Utility to query the DOM.
import { Component } from "@angular/core"; // Import Component decorator for host testing.


// Host component to test ng-content projection in the topbar.
@Component({
    standalone: true,
    imports: [TopbarComponent],
    template: `
        <app-topbar message="Welcome">
            <div id="test-projection">User Profile</div>
        </app-topbar>`
})
class TestHostComponent {}


describe("TopbarComponent", () => {
    // Component instance and testing fixture.
    let component: TopbarComponent;
    let fixture: ComponentFixture<TopbarComponent>;


    beforeEach(async () => {
        // Set up testing module.
        await TestBed.configureTestingModule({
            imports: [TopbarComponent, TestHostComponent],
        }).compileComponents();

        // Create component instance.
        fixture = TestBed.createComponent(TopbarComponent);
        component = fixture.componentInstance;
        fixture.detectChanges(); // Trigger initial data binding.
    });


    // Basic instantiation check.
    it("Should create the component.", () => {
        expect(component).toBeTruthy();
    });


    // Verifies that the default message is "Hi" when no input is provided.
    it("Should display the default 'Hi' message.", () => {
        const badge = fixture.debugElement.query(By.css(".badge"));
        expect(badge.nativeElement.textContent.trim()).toBe("Hi");
    });


    // Confirms that the custom message input is correctly rendered in the badge.
    it("Should display a custom message when input is provided.", () => {
        component.message = "System Update Ready";
        fixture.detectChanges();

        const badge = fixture.debugElement.query(By.css(".badge"));
        expect(badge.nativeElement.textContent.trim()).toBe("System Update Ready");
    });


    // Ensures that clicking the sidebar toggle button (mobile view) emits the toggleSidebar event.
    it("Should emit toggleSidebar when the toggle button is clicked.", () => {
        spyOn(component.toggleSidebar, "emit");
        
        const toggleBtn = fixture.debugElement.query(By.css(".btn-light.d-md-none"));
        toggleBtn.triggerEventHandler("click", null);

        expect(component.toggleSidebar.emit).toHaveBeenCalled();
    });


    // Validates that external content is correctly projected into the topbar via ng-content.
    it("Should project content provided in the component body.", () => {
        const hostFixture = TestBed.createComponent(TestHostComponent);
        hostFixture.detectChanges();

        const projected = hostFixture.debugElement.query(By.css("#test-projection"));
        expect(projected).toBeTruthy();
        expect(projected.nativeElement.textContent).toBe("User Profile");
    });
});