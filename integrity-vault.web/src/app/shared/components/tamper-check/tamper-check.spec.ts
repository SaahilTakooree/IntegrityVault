// Import dependencies.
import { ComponentFixture, TestBed } from "@angular/core/testing"; // Import Angular testing utilities.
import { TamperCheckComponent } from "./tamper.check"; // Import the component being tested.
import { By } from "@angular/platform-browser"; // Utility to query the DOM.
import { EntityModalComponent } from "../entity-modal/entity-modal"; // Child component dependency.
import { VerifyResultComponent } from "../verify-result/verify-result"; // Child component dependency.


describe("TamperCheckComponent", () => {
    // Component instance and testing fixture.
    let component: TamperCheckComponent;
    let fixture: ComponentFixture<TamperCheckComponent>;

    beforeEach(async () => {
        // Set up testing module.
        await TestBed.configureTestingModule({
            imports: [TamperCheckComponent, EntityModalComponent, VerifyResultComponent],
        }).compileComponents();

        // Create component instance.
        fixture = TestBed.createComponent(TamperCheckComponent);
        component = fixture.componentInstance;
        
        // Initial state for most tests to ensure content is projected.
        component.show = true; 
        fixture.detectChanges();
    });


    // Basic instantiation check.
    it("Should create the component.", () => {
        expect(component).toBeTruthy();
    });


    // Verifies that the modal visibility is controlled by the 'show' input.
    it("Should pass the 'show' input to the entity modal.", () => {
        // Set to false to verify the toggle works.
        component.show = false;
        fixture.detectChanges();

        const modal = fixture.debugElement.query(By.directive(EntityModalComponent));
        expect(modal.componentInstance.show).toBeFalse();
    });


    // Confirms that record details (Name and CID) are displayed correctly in the template.
    it("Should display the record name and CID.", () => {
        component.recordName = "Blood_Test_001.pdf";
        component.cid = "QmXoyp...789";
        fixture.detectChanges();

        const paragraphs = fixture.debugElement.queryAll(By.css(".font-mono"));
        const textContent = paragraphs.map(p => p.nativeElement.textContent.trim());

        expect(textContent).toContain("Blood_Test_001.pdf");
        expect(textContent).toContain("QmXoyp...789");
    });


    // Checks that the loading spinner is shown during the on-chain integrity check.
    it("Should show the loading spinner when loading is true.", () => {
        component.loading = true;
        fixture.detectChanges();

        const spinner = fixture.debugElement.query(By.css(".spinner-border"));
        const loadingText = fixture.debugElement.nativeElement.textContent;

        expect(spinner).toBeTruthy();
        expect(loadingText).toContain("Checking integrity on-chain");
    });


    // Verifies that the results component is only rendered when loading is finished.
    it("Should render app-verify-result when not loading and result is available.", () => {
        component.loading = false;
        component.result = { isTampered: false, timestamp: "2026-03-29" } as any;
        fixture.detectChanges();

        const resultComponent = fixture.debugElement.query(By.directive(VerifyResultComponent));
        expect(resultComponent).toBeTruthy();
        expect(resultComponent.componentInstance.result.isTampered).toBeFalse();
    });


    // Ensures that error messages are passed to the results component.
    it("Should display error message via app-verify-result when provided.", () => {
        component.loading = false;
        component.errorMessage = "Blockchain node unreachable";
        fixture.detectChanges();

        const resultComponent = fixture.debugElement.query(By.directive(VerifyResultComponent));
        
        expect(resultComponent).withContext("VerifyResultComponent should be visible").toBeTruthy();
        expect(resultComponent.componentInstance.errorMessage).toBe("Blockchain node unreachable");
    });


    // Validates that the 'closed' event is emitted when the modal confirms/closes.
    it("Should emit the closed event when onClose is called.", () => {
        spyOn(component.closed, "emit");
        
        component.onClose();

        expect(component.closed.emit).toHaveBeenCalled();
    });

    
    // Confirms that the internal onClose is triggered by the modal's (confirmed) output.
    it("Should trigger onClose when the entity modal emits 'confirmed'.", () => {
        spyOn(component, "onClose");
        const modal = fixture.debugElement.query(By.directive(EntityModalComponent));
        
        modal.triggerEventHandler("confirmed", null);

        expect(component.onClose).toHaveBeenCalled();
    });
});