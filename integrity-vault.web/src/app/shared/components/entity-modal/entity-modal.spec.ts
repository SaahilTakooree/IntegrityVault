// Import dependencies.
import { ComponentFixture, TestBed } from "@angular/core/testing"; // Import Angular testing utilities.
import { EntityModalComponent } from "./entity-modal"; // Import the component being tested.
import { By } from "@angular/platform-browser"; // Utility to query the DOM.
import { Component } from "@angular/core"; // Import Component decorator for host testing.


// Host component to test the main ng-content projection in the modal body.
@Component({
    standalone: true,
    imports: [EntityModalComponent],
    template: `
        <app-entity-modal [show]="true">
            <div id="test-content">Entity Form Content</div>
        </app-entity-modal>`
})
class TestHostComponent {}


describe("EntityModalComponent", () => {
    // Component instance and testing fixture.
    let component: EntityModalComponent;
    let fixture: ComponentFixture<EntityModalComponent>;


    beforeEach(async () => {
        // Set up testing module.
        await TestBed.configureTestingModule({
            imports: [EntityModalComponent, TestHostComponent],
        }).compileComponents();

        // Create component instance.
        fixture = TestBed.createComponent(EntityModalComponent);
        component = fixture.componentInstance;
        fixture.detectChanges(); // Trigger initial data binding.
    });


    // Basic instantiation check.
    it("Should create the component.", () => {
        expect(component).toBeTruthy();
    });


    // Verifies that the modal and backdrop are not present in the DOM when 'show' is false.
    it("Should not render modal or backdrop when show is false.", () => {
        component.show = false;
        fixture.detectChanges();

        const modal = fixture.debugElement.query(By.css(".modal"));
        const backdrop = fixture.debugElement.query(By.css(".modal-backdrop"));

        expect(modal).toBeNull();
        expect(backdrop).toBeNull();
    });


    // Confirms that the modal displays the correct title and confirmation button label.
    it("Should display the correct title and confirm label.", () => {
        component.show = true;
        component.title = "Create New Hospital";
        component.confirmLabel = "Create";
        fixture.detectChanges();

        const title = fixture.debugElement.query(By.css(".modal-title")).nativeElement.textContent;
        const confirmBtn = fixture.debugElement.query(By.css(".btn-primary")).nativeElement.textContent;

        expect(title).toContain("Create New Hospital");
        expect(confirmBtn).toContain("Create");
    });


    // Ensures that clicking the close button in the header emits the closed event.
    it("Should emit closed event when the header close button is clicked.", () => {
        spyOn(component.closed, "emit");
        component.show = true;
        fixture.detectChanges();

        const closeBtn = fixture.debugElement.query(By.css(".btn-close"));
        closeBtn.triggerEventHandler("click", null);

        expect(component.closed.emit).toHaveBeenCalled();
    });


    // Validates that clicking the 'Cancel' button triggers the closed event.
    it("Should emit closed event when the cancel button is clicked.", () => {
        spyOn(component.closed, "emit");
        component.show = true;
        fixture.detectChanges();

        const cancelBtn = fixture.debugElement.query(By.css(".btn-outline-secondary"));
        cancelBtn.triggerEventHandler("click", null);

        expect(component.closed.emit).toHaveBeenCalled();
    });


    // Verifies that clicking the primary action button triggers the confirmed event.
    it("Should emit confirmed event when the primary button is clicked.", () => {
        spyOn(component.confirmed, "emit");
        component.show = true;
        fixture.detectChanges();

        const confirmBtn = fixture.debugElement.query(By.css(".btn-primary"));
        confirmBtn.triggerEventHandler("click", null);

        expect(component.confirmed.emit).toHaveBeenCalled();
    });


    // Checks that clicking the backdrop properly emits the closed event to dismiss the modal.
    it("Should emit closed event when the backdrop is clicked.", () => {
        spyOn(component.closed, "emit");
        component.show = true;
        fixture.detectChanges();

        const backdrop = fixture.debugElement.query(By.css(".modal-backdrop"));
        backdrop.triggerEventHandler("click", null);

        expect(component.closed.emit).toHaveBeenCalled();
    });


    // Confirms that clicking the outer modal container (fade show) also triggers a close event.
    it("Should emit closed event when clicking outside the modal dialog.", () => {
        spyOn(component.closed, "emit");
        component.show = true;
        fixture.detectChanges();

        const modalContainer = fixture.debugElement.query(By.css(".modal.fade.show"));
        modalContainer.triggerEventHandler("click", null);

        expect(component.closed.emit).toHaveBeenCalled();
    });


    // Ensures that clicks inside the modal content do not propagate and cause the modal to close.
    it("Should prevent click propagation from the modal dialog to the container.", () => {
        spyOn(component.closed, "emit");
        component.show = true;
        fixture.detectChanges();

        const modalDialog = fixture.debugElement.query(By.css(".modal-dialog"));
        const event = new MouseEvent("click");
        spyOn(event, "stopPropagation");

        modalDialog.nativeElement.dispatchEvent(event);

        expect(event.stopPropagation).toHaveBeenCalled();
        expect(component.closed.emit).not.toHaveBeenCalled();
    });


    // Tests that any content placed inside the component tags is correctly projected into the modal body.
    it("Should project the provided content into the modal body.", () => {
        const hostFixture = TestBed.createComponent(TestHostComponent);
        hostFixture.detectChanges();

        const projectedContent = hostFixture.debugElement.query(By.css("#test-content"));
        
        expect(projectedContent).toBeTruthy();
        expect(projectedContent.nativeElement.textContent).toContain("Entity Form Content");
    });
});