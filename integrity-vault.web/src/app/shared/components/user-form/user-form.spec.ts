// Import dependencies.
import { ComponentFixture, TestBed, fakeAsync, tick } from "@angular/core/testing"; // Import Angular testing utilities.
import { UserFormComponent } from "./user-form"; // Import the component being tested.
import { FormsModule } from "@angular/forms"; // Required for ngModel.
import { By } from "@angular/platform-browser"; // Utility to query the DOM.
import { UserRole } from "../../enums/user-role.enum"; // Import user roles.


describe("UserFormComponent", () => {
    // Component instance and testing fixture.
    let component: UserFormComponent;
    let fixture: ComponentFixture<UserFormComponent>;


    beforeEach(async () => {
        // Set up testing module.
        await TestBed.configureTestingModule({
            imports: [UserFormComponent, FormsModule],
        }).compileComponents();

        // Create component instance.
        fixture = TestBed.createComponent(UserFormComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });


    // Basic instantiation check.
    it("Should create the component.", () => {
        expect(component).toBeTruthy();
    });


    // Verifies that Doctor specific name fields and specialty render correctly.
    it("Should render Doctor specific name fields and specialty.", () => {
        component.role = UserRole.Doctor;
        fixture.detectChanges();

        const firstName = fixture.debugElement.query(By.css('input[name="drFirstName"]'));
        const middleName = fixture.debugElement.query(By.css('input[name="drMiddleName"]'));
        const lastName = fixture.debugElement.query(By.css('input[name="drLastName"]'));
        const specialty = fixture.debugElement.query(By.css('select[name="specialty"]'));

        expect(firstName).withContext("Doctor First Name missing").toBeTruthy();
        expect(middleName).withContext("Doctor Middle Name missing").toBeTruthy();
        expect(lastName).withContext("Doctor Last Name missing").toBeTruthy();
        expect(specialty).toBeTruthy();
    });


    // Verifies that Patient specific name fields, DOB, and Gender render correctly.
    it("Should render Patient specific name fields, DOB, and Gender.", () => {
        component.role = UserRole.Patient;
        fixture.detectChanges();

        const firstName = fixture.debugElement.query(By.css('input[name="ptFirstName"]'));
        const middleName = fixture.debugElement.query(By.css('input[name="ptMiddleName"]'));
        const lastName = fixture.debugElement.query(By.css('input[name="ptLastName"]'));
        const dobInput = fixture.debugElement.query(By.css('input[name="dob"]'));
        const genderSelect = fixture.debugElement.query(By.css('select[name="gender"]'));

        expect(firstName).withContext("Patient First Name missing").toBeTruthy();
        expect(middleName).withContext("Patient Middle Name missing").toBeTruthy();
        expect(lastName).withContext("Patient Last Name missing").toBeTruthy();
        expect(dobInput).toBeTruthy();
        expect(genderSelect).toBeTruthy();
    });


    // Confirms that name fields are validated for capitalisation and string-only content.
    it("Should validate that names are strings and start with a capital letter.", () => {
        component.role = UserRole.Doctor;
        // Invalid data: lowercase and contains numbers.
        component.form.firstName = "john123"; 
        component.onFieldChange();
        fixture.detectChanges();

        expect(component.errors.firstName).toBeTruthy();
        
        // Valid data: Starts with capital and is string only.
        component.form.firstName = "John";
        component.onFieldChange();
        fixture.detectChanges();

        expect(component.errors.firstName).toBeFalsy();
    });


    // Ensures middle name remains optional and doesn't trigger errors when empty.
    it("Should treat middle name as optional.", () => {
        component.role = UserRole.Doctor;
        component.form.firstName = "John";
        component.form.lastName = "Doe";
        component.form.middleName = ""; // Left empty.

        component.onFieldChange();
        fixture.detectChanges();

        expect(component.errors.middleName).toBeFalsy();
    });


    // Checks the password toggle logic in edit mode.
    it("Should hide password by default in edit mode and show on toggle.", fakeAsync(() => {
        const mockUser = { username: "jdoe", email: "j@d.com" } as any;
        component.initialValue = mockUser;
        component.ngOnChanges({
            initialValue: { currentValue: mockUser, previousValue: null, firstChange: true, isFirstChange: () => true }
        });
        fixture.detectChanges();
        tick();

        let passwordInput = fixture.debugElement.query(By.css('input[name="password"]'));
        expect(passwordInput).toBeNull();

        component.changePassword = true;
        component.onChangePasswordToggle();
        fixture.detectChanges();
        tick();

        passwordInput = fixture.debugElement.query(By.css('input[name="password"]'));
        expect(passwordInput).toBeTruthy();
    }));


    // Verifies the filtering logic for the belongsToHospitals list.
    it("Should filter the current hospital from belongsToHospitals.", () => {
        component.defaultHospitalID = 10;
        component.hospitals = [
            { id: 10, name: "Hospital A" } as any,
            { id: 20, name: "Hospital B" } as any
        ];

        const result = component.belongsToHospitals;

        expect(result.length).toBe(1);
        expect(result[0].id).toBe(20);
    });


    // Confirms that formChange is emitted when onFieldChange is called.
    it("Should emit formChange when a field is updated.", () => {
        spyOn(component.formChange, "emit");
        component.onFieldChange();
        expect(component.formChange.emit).toHaveBeenCalled();
    });


    // Validates the getValue method trims whitespace.
    it("Should return trimmed values from getValue.", () => {
        component.form.username = "  admin  ";
        const val = component.getValue();
        expect(val.username).toBe("admin");
    });


    // Checks that API errors are displayed in the template.
    it("Should display API errors.", () => {
        const msg = "Invalid Username";
        component.setApiError(msg);
        fixture.detectChanges();

        const errorDiv = fixture.debugElement.query(By.css(".text-danger.small"));
        expect(errorDiv.nativeElement.textContent).toContain(msg);
    });
});