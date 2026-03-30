// Import dependencies.
import { ComponentFixture, TestBed } from "@angular/core/testing"; // Import Angular testing utilities.
import { ConfirmModalComponent } from "./confirm-modal"; // Import the component being tested.
import { By } from "@angular/platform-browser"; // Utility to query the DOM.
import { ConfirmButtonStyle } from "../../enums/button-style.enum"; // Import the button style enum.
import { Component } from "@angular/core"; // Import Component decorator for host testing.


// Host component to test ng-content projection.
@Component({
    standalone: true,
    imports: [ConfirmModalComponent],
    template: `
        <app-confirm-modal [show]="true">
            <span icon id="test-icon">🔥</span>
        </app-confirm-modal>`
})
class TestHostComponent {}


describe("ConfirmModalComponent", () => {
    // Component instance and testing fixture.
    let component: ConfirmModalComponent;
    let fixture: ComponentFixture<ConfirmModalComponent>;


    beforeEach(async () => {
        // Set up testing module.
        await TestBed.configureTestingModule({
            imports: [ConfirmModalComponent, TestHostComponent],
        }).compileComponents();

        // Create component instance.
        fixture = TestBed.createComponent(ConfirmModalComponent);
        component = fixture.componentInstance;
        fixture.detectChanges(); // Trigger initial data binding.
    });


    // Basic instantiation check.
    it("Should create the component.", () => {
        expect(component).toBeTruthy();
    });


    // Verifies that the modal markup is completely absent from the DOM when the 'show' input is false.
    it("Should not render the modal when show is false.", () => {
        component.show = false;
        fixture.detectChanges();

        const modal = fixture.debugElement.query(By.css(".iv-modal-wrapper"));
        expect(modal).toBeNull();
    });


    // Confirms that the modal appears in the DOM and displays the correct title and message when 'show' is true.
    it("Should render the modal with title and message when show is true.", () => {
        component.show = true;
        component.title = "Delete Record";
        component.message = "Are you sure you want to delete this?";
        fixture.detectChanges();

        const titleText = fixture.debugElement.query(By.css(".iv-modal-title")).nativeElement.textContent;
        const messageText = fixture.debugElement.query(By.css(".iv-modal-message")).nativeElement.textContent;

        expect(titleText).toContain("Delete Record");
        expect(messageText).toContain("Are you sure you want to delete this?");
    });


    // Ensures that clicking the backdrop overlay triggers the close logic and emits the closed event.
    it("Should emit closed event when backdrop is clicked.", () => {
        spyOn(component.closed, "emit");
        component.show = true;
        fixture.detectChanges();

        const backdrop = fixture.debugElement.query(By.css(".iv-modal-backdrop"));
        backdrop.triggerEventHandler("click", null);

        expect(component.closed.emit).toHaveBeenCalled();
    });


    // Validates that the default 'Close' button is rendered when no custom buttons are provided in the inputs.
    it("Should render default Close button when buttons array is empty.", () => {
        component.show = true;
        component.buttons = [];
        fixture.detectChanges();

        const button = fixture.debugElement.query(By.css(".btn-secondary"));
        expect(button.nativeElement.textContent).toContain("Close");
    });


    // Checks that custom buttons are correctly generated based on the input array, including correct labels and styles.
    it("Should render custom buttons based on input.", () => {
        component.show = true;
        component.buttons = [
            { label: "Yes", result: true, style: ConfirmButtonStyle.Primary },
            { label: "No", result: false, style: ConfirmButtonStyle.Danger }
        ];
        fixture.detectChanges();

        const buttons = fixture.debugElement.queryAll(By.css("button"));
        expect(buttons.length).toBe(2);
        expect(buttons[0].nativeElement.textContent).toContain("Yes");
        expect(buttons[0].nativeElement.classList).toContain("btn-primary");
        expect(buttons[1].nativeElement.textContent).toContain("No");
        expect(buttons[1].nativeElement.classList).toContain("btn-danger");
    });


    // Verifies that clicking a custom button emits the correct boolean action and closes the modal.
    it("Should emit action event with true and close when primary button is clicked.", () => {
        spyOn(component.action, "emit");
        spyOn(component, "close");

        component.show = true;
        component.buttons = [{ label: "Confirm", result: true }];
        fixture.detectChanges();

        const button = fixture.debugElement.query(By.css("button"));
        button.triggerEventHandler("click", null);

        expect(component.action.emit).toHaveBeenCalledWith(true);
        expect(component.close).toHaveBeenCalled();
    });


    // Ensures that clicks on the modal content itself do not bubble up to the backdrop, preventing accidental closure.
    it("Should stop click propagation on the modal container.", () => {
        spyOn(component, "close");
        component.show = true;
        fixture.detectChanges();

        const modalContainer = fixture.debugElement.query(By.css(".iv-modal"));
        const event = new MouseEvent("click");
        spyOn(event, "stopPropagation");

        modalContainer.nativeElement.dispatchEvent(event);

        expect(event.stopPropagation).toHaveBeenCalled();
        expect(component.close).not.toHaveBeenCalled();
    });


    // Confirms that projected content (the icon) is correctly rendered via ng-content.
    it("Should project icon content.", () => {
        // Use the Host Component specifically for this test.
        const hostFixture = TestBed.createComponent(TestHostComponent);
        hostFixture.detectChanges();
        
        const icon = hostFixture.debugElement.query(By.css("#test-icon"));
        expect(icon).toBeTruthy();
        expect(icon.nativeElement.textContent).toBe("🔥");
    });
});