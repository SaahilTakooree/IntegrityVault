// Import dependencies.
import { ComponentFixture, TestBed } from "@angular/core/testing"; // Import Angular testing utilities.
import { VerifyResultComponent } from "./verify-result"; // Import the component being tested.
import { By } from "@angular/platform-browser"; // Utility to query the DOM.


describe("VerifyResultComponent", () => {
    // Component instance and testing fixture.
    let component: VerifyResultComponent;
    let fixture: ComponentFixture<VerifyResultComponent>;


    beforeEach(async () => {
        // Set up testing module.
        await TestBed.configureTestingModule({
            imports: [VerifyResultComponent],
        }).compileComponents();

        // Create component instance.
        fixture = TestBed.createComponent(VerifyResultComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });


    // Basic instantiation check.
    it("Should create the component.", () => {
        expect(component).toBeTruthy();
    });


    // Verifies the "intact" state when the result shows no tampering.
    it("Should render 'intact' state when isTampered is false.", () => {
        component.result = { isTampered: false, status: "Success", message: "Record is valid" } as any;
        fixture.detectChanges();

        const successDiv = fixture.debugElement.query(By.css(".iv-verify-result-success"));
        expect(successDiv).toBeTruthy();
        expect(successDiv.nativeElement.textContent).toContain("Intact.");
        expect(successDiv.nativeElement.textContent).toContain("Record is valid");
    });


    // Verifies the "tampered" state when isTampered is true.
    it("Should render 'tampered' state when isTampered is true.", () => {
        component.result = { isTampered: true, status: "Failed", message: "Hash mismatch" } as any;
        fixture.detectChanges();

        const failDiv = fixture.debugElement.query(By.css(".iv-verify-result-fail"));
        expect(failDiv).toBeTruthy();
        expect(failDiv.nativeElement.textContent).toContain("Tampered.");
        expect(failDiv.nativeElement.textContent).toContain("Hash mismatch");
    });


    // Verifies the "unauthorised" state based on the status string.
    it("Should render 'unauthorised' state when status is Unauthorised.", () => {
        component.result = { status: "Unauthorised", message: "Access denied" } as any;
        fixture.detectChanges();

        const icon = fixture.debugElement.query(By.css(".bi-shield-exclamation"));
        
        expect(icon).withContext("Unauthorised icon should be visible").toBeTruthy();
        
        const container = icon.parent;
        expect(container?.nativeElement.textContent).toContain("Unauthorised.");
    });


    // Confirms that providing an errorMessage triggers the error UI.
    it("Should render 'error' state when errorMessage is provided.", () => {
        component.errorMessage = "Network Timeout";
        fixture.detectChanges();

        const errorDiv = fixture.debugElement.query(By.css(".bi-exclamation-circle"))?.parent;
        expect(errorDiv?.nativeElement.textContent).toContain("Network Timeout");
    });


    // Validates that the individual check details are rendered when showDetails is true.
    it("Should render check details when showDetails is true and state is valid.", () => {
        component.showDetails = true;
        component.result = { 
            isTampered: false, 
            contentHashMatch: true, 
            databaseHashMatch: false 
        } as any;
        fixture.detectChanges();

        const checks = fixture.debugElement.queryAll(By.css(".bi-check-circle-fill, .bi-x-circle-fill"));
        // Check for specific icons based on the boolean values above.
        expect(checks[0].nativeElement.classList).toContain("bi-check-circle-fill"); // contentHashMatch
        expect(checks[1].nativeElement.classList).toContain("bi-x-circle-fill"); // databaseHashMatch
    });


    // Ensures details are hidden when explicitly disabled via Input.
    it("Should hide check details when showDetails is false.", () => {
        component.showDetails = false;
        component.result = { isTampered: false } as any;
        fixture.detectChanges();

        const detailsHeader = fixture.debugElement.query(By.css(".text-uppercase"));
        expect(detailsHeader).toBeNull();
    });


    // Checks that the state getter returns null when no inputs are provided.
    it("Should return null state when result and errorMessage are missing.", () => {
        component.result = null;
        component.errorMessage = null;
        expect(component.state).toBeNull();
    });


    // Confirms details are not shown in error or unauthorised states to prevent UI clutter.
    it("Should not show details header if state is 'unauthorised' or 'error'.", () => {
        component.errorMessage = "Critical Error";
        component.showDetails = true;
        fixture.detectChanges();

        const detailsHeader = fixture.debugElement.query(By.css(".text-uppercase"));
        expect(detailsHeader).toBeNull();
    });
});