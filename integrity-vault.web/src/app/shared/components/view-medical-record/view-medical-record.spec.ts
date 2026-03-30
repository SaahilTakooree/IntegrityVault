// Import dependencies.
import { ComponentFixture, TestBed } from "@angular/core/testing"; // Import Angular testing utilities.
import { ViewMedicalRecordComponent } from "./view-medical-record"; // Import the component being tested.
import { By } from "@angular/platform-browser"; // Utility to query the DOM.
import { EntityModalComponent } from "../entity-modal/entity-modal"; // Child component dependency.


describe("ViewMedicalRecordComponent", () => {
    // Component instance and testing fixture.
    let component: ViewMedicalRecordComponent;
    let fixture: ComponentFixture<ViewMedicalRecordComponent>;

    beforeEach(async () => {
        // Set up testing module.
        await TestBed.configureTestingModule({
            imports: [ViewMedicalRecordComponent, EntityModalComponent],
        }).compileComponents();

        fixture = TestBed.createComponent(ViewMedicalRecordComponent);
        component = fixture.componentInstance;
        
        // Default to showing the modal for template testing.
        component.show = true;
        fixture.detectChanges();
    });


    // Basic instantiation check.
    it("Should create the component.", () => {
        expect(component).toBeTruthy();
    });


    // Checks that the loading spinner appears and record content is hidden during fetch.
    it("Should show loading spinner when loading is true.", () => {
        component.loading = true;
        fixture.detectChanges();

        const spinner = fixture.debugElement.query(By.css(".spinner-border"));
        expect(spinner).withContext("Spinner should be visible during loading").toBeTruthy();
        expect(fixture.nativeElement.textContent).toContain("Loading record from IPFS");
    });


    // Verifies that the diagnosis and treatment plan (required fields) are correctly rendered.
    it("Should display diagnosis and treatment plan when record is available.", () => {
        component.loading = false;
        component.record = {
            chiefComplaint: "Bronchitis Problem",
            diagnosis: "Acute Bronchitis",
            treatmentPlan: "Antibiotics and rest",
            doctorNotes: "",
            followUpInstructions: ""
        };
        fixture.detectChanges();

        const diagnosisArea = fixture.debugElement.query(By.css("textarea[name='record.diagnosis']"));
        const treatmentArea = fixture.debugElement.query(By.css("textarea[name='record.treatmentPlan']"));

        expect(diagnosisArea.nativeElement.value).toBe("Acute Bronchitis");
        expect(treatmentArea.nativeElement.value).toBe("Antibiotics and rest");
    });


    // Confirms that optional fields (Doctor Notes) only appear when data exists.
    it("Should show optional doctor notes only if they exist.", () => {
        component.record = { doctorNotes: "Patient was stable" } as any;
        fixture.detectChanges();
        expect(fixture.debugElement.query(By.css("textarea[name='record.doctorNotes']"))).toBeTruthy();

        component.record = { doctorNotes: "" } as any;
        fixture.detectChanges();
        expect(fixture.debugElement.query(By.css("textarea[name='record.doctorNotes']"))).toBeNull();
    });


    // Verifies that the IPFS CID from the version input is correctly displayed.
    it("Should display the IPFS CID in the read-only input.", () => {
        component.version = { ipfS_CID: "QmTest123", displayName: "Test Record" } as any;
        component.record = {} as any; // Trigger the record block.
        fixture.detectChanges();

        const cidInput = fixture.debugElement.query(By.css("input[name='version.ipfS_CID']"));
        expect(cidInput.nativeElement.value).toBe("QmTest123");
    });


    // Ensures all form fields in this view are read-only to prevent accidental edits.
    it("Should have all textareas and inputs as read-only.", () => {
        component.record = { diagnosis: "D", treatmentPlan: "T", doctorNotes: "N" } as any;
        fixture.detectChanges();

        const controls = fixture.debugElement.queryAll(By.css("textarea, input"));
        controls.forEach(control => {
            expect(control.nativeElement.readOnly).withContext(`Field ${control.nativeElement.name} should be read-only`).toBeTrue();
        });
    });


    // Verifies the modal title logic based on the version display name.
    it("Should pass the correct title to the entity modal.", () => {
        component.version = { displayName: "Consultation_01" } as any;
        fixture.detectChanges();

        const modal = fixture.debugElement.query(By.directive(EntityModalComponent));
        expect(modal.componentInstance.title).toBe("Record: Consultation_01");
    });


    // Confirms the 'closed' event emission when the modal interactions occur.
    it("Should emit closed event when modal emits closed or confirmed.", () => {
        spyOn(component.closed, "emit");
        const modal = fixture.debugElement.query(By.directive(EntityModalComponent));

        modal.triggerEventHandler("closed", null);
        modal.triggerEventHandler("confirmed", null);

        expect(component.closed.emit).toHaveBeenCalledTimes(2);
    });
});