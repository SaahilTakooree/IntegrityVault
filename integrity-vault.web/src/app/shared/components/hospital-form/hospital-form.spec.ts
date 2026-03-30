// Import dependencies.
import { ComponentFixture, TestBed, fakeAsync, tick } from "@angular/core/testing"; // Import Angular testing utilities.
import { HospitalFormComponent } from "./hospital-form"; // Import the component being tested.
import { FormsModule } from "@angular/forms"; // Required for ngModel.
import { By } from "@angular/platform-browser"; // Utility to query the DOM.
import * as Validators from "../../utils/hospital/hospital-form.validator"; // Import validators to spy on them.


describe("HospitalFormComponent", () => {
    // Component instance and testing fixture.
    let component: HospitalFormComponent;
    let fixture: ComponentFixture<HospitalFormComponent>;


    beforeEach(async () => {
        // Set up testing module.
        await TestBed.configureTestingModule({
            imports: [HospitalFormComponent, FormsModule],
        }).compileComponents();

        // Create component instance.
        fixture = TestBed.createComponent(HospitalFormComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });


    // Basic instantiation check.
    it("Should create the component.", () => {
        expect(component).toBeTruthy();
    });


    // Verifies that 'full' mode renders name, wallet, and private key fields.
    it("Should render all fields when mode is 'full'.", fakeAsync(() => {
        // 1. Set the state
        component.mode = "full";
        component.resetForm();

        fixture.detectChanges(); 
        
        tick(); 
        fixture.detectChanges();

        const nameInput    = fixture.debugElement.query(By.css('input[name="name"]'));
        const walletInput  = fixture.debugElement.query(By.css('input[name="walletAddress"]'));
        const keyInput     = fixture.debugElement.query(By.css('input[name="privateKey"]'));

        expect(nameInput).withContext("Name input should exist").toBeTruthy();
        expect(walletInput).withContext("Wallet input should exist").toBeTruthy();
        expect(keyInput).withContext("Private key input should exist").toBeTruthy();
    }));


    // Confirms that only the IP address section is visible in 'ip-only' (non-full) mode.
    it("Should only render IP addresses when mode is not 'full'.", () => {
        component.mode = "ip-only" as any;
        fixture.detectChanges();

        const nameInput = fixture.debugElement.query(By.css('input[placeholder*="Hospital Name"]'));
        const ipTable = fixture.debugElement.query(By.css("table"));

        expect(nameInput).toBeNull();
        expect(ipTable).toBeTruthy();
    });


    // Validates that ngOnChanges correctly maps an initialValue to the internal form model.
    it("Should populate the form when initialValue is provided via ngOnChanges.", () => {
        const mockHospital = { 
            id: 1, name: "City Clinic", walletAddress: "0x123", 
            ipAddresses: ["192.168.1.1"], privateKey: "hidden" 
        };

        // Manually trigger ngOnChanges logic.
        component.initialValue = mockHospital;
        component.ngOnChanges({
            initialValue: {
                currentValue: mockHospital,
                previousValue: null,
                firstChange: true,
                isFirstChange: () => true
            }
        });

        expect(component.form.name).toBe("City Clinic");
        expect(component.form.walletAddress).toBe("0x123");
        expect(component.form.ipAddresses).toEqual(["192.168.1.1"]);
        // Private key should be reset to empty string for security/updates.
        expect(component.form.privateKey).toBe(""); 
    });


    // Tests the IP row management: adding a new row.
    it("Should add a new empty IP row when addIpRow is called.", () => {
        component.form.ipAddresses = ["1.1.1.1"];
        component.addIpRow();

        expect(component.form.ipAddresses.length).toBe(2);
        expect(component.form.ipAddresses[1]).toBe("");
    });


    // Tests the IP row management: removing a row.
    it("Should remove an IP row but keep at least one.", () => {
        component.form.ipAddresses = ["1.1.1.1", "2.2.2.2"];
        
        component.removeIpRow(0);
        expect(component.form.ipAddresses.length).toBe(1);
        expect(component.form.ipAddresses[0]).toBe("2.2.2.2");

        // Attempt to remove the last one.
        component.removeIpRow(0);
        expect(component.form.ipAddresses.length).toBe(1);
    });


    // Verifies that the UI correctly detects and flags duplicate IP addresses.
    it("Should identify duplicate IP addresses.", () => {
        component.form.ipAddresses = ["10.0.0.1", "10.0.0.1", "10.0.0.2"];
        
        expect(component.isDuplicateIp(0)).toBeTrue();
        expect(component.isDuplicateIp(1)).toBeTrue();
        expect(component.isDuplicateIp(2)).toBeFalse();
    });


    // Checks the password visibility toggle functionality.
    it("Should toggle private key visibility.", () => {
        component.mode = "full";
        component.showPrivateKey = false;
        fixture.detectChanges();

        const toggleBtn = fixture.debugElement.query(By.css(".input-group button"));
        toggleBtn.triggerEventHandler("click", null);
        fixture.detectChanges();

        expect(component.showPrivateKey).toBeTrue();
        const keyInput = fixture.debugElement.query(By.css("input.font-mono"));
        expect(keyInput.attributes["type"]).toBe("text");
    });


    // Ensures that formChange is emitted when fields are updated.
    it("Should emit formChange when onFieldChange is called.", () => {
        spyOn(component.formChange, "emit");
        component.form.name = "Updated Name";
        
        component.onFieldChange();

        expect(component.formChange.emit).toHaveBeenCalledWith(jasmine.objectContaining({
            value: jasmine.objectContaining({ name: "Updated Name" })
        }));
    });


    // Confirms that API errors are correctly displayed in the UI.
    it("Should display API error message if provided.", () => {
        const errorMessage = "Server Timeout";
        component.setApiError(errorMessage);
        fixture.detectChanges();

        const allErrors = fixture.debugElement.queryAll(By.css(".text-danger"));
        const apiErrorElement = allErrors.find(el => el.nativeElement.textContent.includes(errorMessage));

        expect(apiErrorElement).toBeTruthy();
        expect(apiErrorElement?.nativeElement.textContent).toContain(errorMessage);
    });


    // Validates that resetForm clears both the model and the error state.
    it("Should clear form and errors when resetForm is called.", () => {
        component.form.name = "Data";
        component.errors = { name: "Invalid" };
        
        component.resetForm();

        expect(component.form.name).toBe("");
        expect(component.errors).toEqual({});
    });
});