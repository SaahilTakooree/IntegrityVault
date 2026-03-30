// Import dependencies.
import { ComponentFixture, TestBed } from "@angular/core/testing"; // Import Angular testing utilities.
import { MedicalRecordFormComponent } from "./medical-record-form"; // Import the component being tested.
import { FormsModule } from "@angular/forms"; // Required for ngModel binding.
import { By } from "@angular/platform-browser"; // Utility to query the DOM.
import { SimpleChange } from "@angular/core"; // Used to simulate lifecycle hook changes.


describe("MedicalRecordFormComponent", () => {
    // Component instance and testing fixture.
    let component: MedicalRecordFormComponent;
    let fixture: ComponentFixture<MedicalRecordFormComponent>;


    beforeEach(async () => {
        // Set up testing module.
        await TestBed.configureTestingModule({
            imports: [MedicalRecordFormComponent, FormsModule],
        }).compileComponents();

        // Create component instance.
        fixture = TestBed.createComponent(MedicalRecordFormComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });


    // Basic instantiation check.
    it("Should create the component.", () => {
        expect(component).toBeTruthy();
    });


    // Verifies that the context label is rendered only when provided.
    it("Should display the context label when provided.", () => {
        component.contextLabel = "New Consultation";
        fixture.detectChanges();

        const label = fixture.debugElement.query(By.css(".text-muted.small"));
        expect(label.nativeElement.textContent).toContain("New Consultation");
    });


    // Confirms that the patient selector renders and populates options correctly.
    it("Should render patient selector and list patients when showPatientSelect is true.", () => {
        component.showPatientSelect = true;
        component.patients = [
            { id: 1, fullName: "John Doe" },
            { id: 2, fullName: "Jane Smith" }
        ];
        fixture.detectChanges();

        const select = fixture.debugElement.query(By.css('select[name="patientID"]'));
        const options = select.nativeElement.querySelectorAll("option");

        expect(select).toBeTruthy();
        // Total options = 2 patients + 1 default disabled option.
        expect(options.length).toBe(3);
        expect(options[1].textContent).toContain("John Doe");
    });


    // Verifies that the chief complaint field visibility is toggled by input.
    it("Should show chief complaint field when showChiefComplaint is true.", () => {
        component.showChiefComplaint = true;
        fixture.detectChanges();

        const input = fixture.debugElement.query(By.css('input[name="chiefComplaint"]'));
        expect(input).toBeTruthy();
    });


    // Ensures the visit date field is readonly as per the template requirement.
    it("Should have a readonly visit date field.", () => {
        const dateInput = fixture.debugElement.query(By.css('input[type="date"]'));
        expect(dateInput.nativeElement.readOnly).toBeTrue();
    });


    // Validates that ngOnChanges correctly updates the form when an initial value is provided.
    it("Should populate the form when initialValue changes.", () => {
        const mockValue = {
            patientID: 5,
            visitDate: "2023-10-10",
            chiefComplaint: "Flu",
            diagnosis: "Influenza",
            treatmentPlan: "Rest",
            doctorNotes: "",
            followUpInstructions: ""
        };

        component.initialValue = mockValue;

        component.ngOnChanges({
            initialValue: new SimpleChange(null, mockValue, false) 
        });

        fixture.detectChanges();

        expect(component.form.diagnosis).toBe("Influenza");
        expect(component.form.patientID).toBe(5);
    });


    // Confirms that the formChange event is emitted with the correct validity status.
    it("Should emit formChange when a field is updated.", () => {
        spyOn(component.formChange, "emit");

        component.form.diagnosis = "Common Cold";
        component.onFieldChange();

        expect(component.formChange.emit).toHaveBeenCalledWith(jasmine.objectContaining({
            value: jasmine.objectContaining({ diagnosis: "Common Cold" }),
            valid: jasmine.any(Boolean)
        }));
    });


    // Checks that validation errors are displayed in the UI.
    it("Should display validation errors when fields are invalid.", () => {
        // Set an error for diagnosis.
        component.errors = { diagnosis: "Diagnosis is required" };
        fixture.detectChanges();

        // Target the specific invalid-feedback for diagnosis.
        const feedback = fixture.debugElement.query(By.css("textarea[name='diagnosis'] + .invalid-feedback"));
        expect(feedback.nativeElement.textContent).toContain("Diagnosis is required");
    });


    // Verifies the reset logic returns the form to its blank state or initial date.
    it("Should reset the form to default values.", () => {
        component.form.diagnosis = "Temporary";
        component.initialVisitDate = "2024-01-01";
        
        component.resetForm();

        expect(component.form.diagnosis).toBe("");
        expect(component.form.visitDate).toBe("2024-01-01");
        expect(component.errors).toEqual({});
    });


    // Validates that API errors are correctly rendered at the bottom of the form.
    it("Should display API error messages when set.", () => {
        const errorMsg = "Server connection failed";
        component.setApiError(errorMsg);
        fixture.detectChanges();

        const errorDiv = fixture.debugElement.query(By.css(".text-danger.small"));
        expect(errorDiv.nativeElement.textContent).toContain(errorMsg);
    });


    // Confirms the manual validate() method updates the error state.
    it("Should update errors state when validate() is called.", () => {
        component.form.diagnosis = "";
        const isValid = component.validate();

        expect(isValid).toBeFalse();
        expect(component.errors.diagnosis).toBeTruthy();
    });
});