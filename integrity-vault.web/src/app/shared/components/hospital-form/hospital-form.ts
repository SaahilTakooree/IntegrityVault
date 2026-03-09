// Import dependencies.
import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges } from "@angular/core"; // Import angular core components.
import { CommonModule } from "@angular/common"; // Import angular common module.
import { FormsModule } from "@angular/forms"; // Import angular form module.
import { IHospital } from "../../interfaces/hospital.interface"; // Import the hospital interface.
import { HospitalFormMode } from "../../types/hospital-form-type"; // Import the type avaliable for the hospital form.
import { HospitalFormOutput } from "../../interfaces/hospital-form-output.interface"; // Import the form output interface.
import { HospitalFormValidationErrors } from "../../types/hospital-form-validation-errors.type"; // Import all type of errors that hospital form can have.
import { validateHospitalForm, validateIpAddresses }from "../../utils/hospital-form.validator"; // Import the validation functions.


// Define the component for the hopsital form.
@Component({
    selector: "app-hospital-form",
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: "./hospital-form.html",
    styleUrls: ["./hospital-form.scss"]
})


// Export the hospital form class.
export class HospitalFormComponent implements OnChanges {
    @Input() mode : HospitalFormMode = "full"; // Show the model that the hospital is in. 
    @Input() initialValue : IHospital | null = null; // Initial value of hospital.
    @Input() apiError : string | null = null; // API errors that might happen when creating or updating the hospital.

    // Fires on every field change with the current form value and overall validity.
    @Output() formChange = new EventEmitter<HospitalFormOutput>();

    form : IHospital = this._blank(); // Create the hospital form. Clear the form initially.
    errors : HospitalFormValidationErrors = {}; // Store all the possible errors that might happen in the hospital form.


    // Function to allow the editing of IP address.
    get canEditIps(): boolean {
        return true;
    }


    // Angular function to detech changes in input properties.
    ngOnChanges(changes : SimpleChanges) : void {
        //
        if (changes["initialValue"]) {
            const v = this.initialValue;
            this.form = v
                ? { id: v.id, name: v.name, walletAddress: v.walletAddress, ipAddresses: [...v.ipAddresses] }
                : this._blank();
            this.errors = {}; // Reset errors when initial value changes.
        }

        // If API error changes, set API error message.
        if (changes["apiError"] && this.apiError) {
            this.errors = { ...this.errors, api: this.apiError };
        }
    }


    // Method call whenever there is any update to the form field.
     onFieldChange() : void {
        this._validate(); // Validate the fields of the hopsital form.
        this.formChange.emit({ value: { ...this.form, ipAddresses: [...this.form.ipAddresses] }, valid: this._isValid() }); // Emit the form changes.
    }


    // Method to handle IP change event.
    onIpChange(index : number, event : Event) : void {
        const value = (event.target as HTMLInputElement).value;
        const updated = [...this.form.ipAddresses]; // Update the IP address.
        updated[index] = value.trim(); // Remove extra spaces.
        this.form.ipAddresses = updated; // Assign the update IP addresss.
    }


    // Method to add a new IP row.
    addIpRow() : void {
        this.form.ipAddresses = [...this.form.ipAddresses, ""];
    }


    // Method to remove an IP row.
    removeIpRow(index : number) : void {
        // Prevent removal if there is only one row.
        if (this.form.ipAddresses.length === 1)
            return;
        this.form.ipAddresses = this.form.ipAddresses.filter((_, i) => i !== index); // Remove the IP row.
        this.onFieldChange(); // Validate the new field data again after removal.
    }


    // Method to check any IP rows are the same.
    isDuplicateIp(index : number) : boolean {
        const ip = this.form.ipAddresses[index];
        if (!ip) return false;
        return this.form.ipAddresses.filter(x => x === ip).length > 1;
    }


    // Method to call the validation function outside of the component.
    validate(): boolean {
        this._validate(); // Validae the form.
        return this._isValid(); // return the form is valid.
    }

    // Method to return all the value in the form outside of the component.
    getValue(): IHospital {
        return { ...this.form, ipAddresses: [...this.form.ipAddresses] };
    }

    // Method to set the api error span if there is error relating to the api.
    setApiError(message: string): void {
        this.errors = { ...this.errors, api: message };
    }

    // Method to clear all validation errors.
    clearHospitalErrors(): void {
        this.errors = {};
    }


    // Method to clear the hospital form field.
    private _blank(): IHospital {
        return { id: 0, name: "", walletAddress: "", ipAddresses: [""] };
    }


    // Function to validate the hospital form field.
    private _validate(): void {
        // Preserve the previous errors.
        const prev = this.errors;

        // Full form validation.
        if (this.mode === "full") {
            this.errors = validateHospitalForm(this.form.name, this.form.walletAddress, this.form.ipAddresses);
        } else {
            // Only validate the ip table..
            const ipErrors = validateIpAddresses(this.form.ipAddresses);
            this.errors = ipErrors ? { ipAddresses: ipErrors } : {};
        }

        // Preserve any api error unless a field-level error clears it implicitly.
        if (prev.api && !this.errors.api) {
            this.errors = { ...this.errors, api: prev.api };
        }
    }


    // Method to check if form is valid
    private _isValid(): boolean {
        return !this.errors.name && !this.errors.walletAddress && !this.errors.ipAddresses;
    }
}