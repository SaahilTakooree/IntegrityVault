// Import dependencies.
import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges } from "@angular/core"; // Import Angular core module.
import { CommonModule } from "@angular/common"; // Import CommonModule for common directives.
import { FormsModule } from "@angular/forms"; // Import FormModule to enable template driven froms and ngModel binding.
import { UserRole } from "../../enums/user-role.enum"; // Import the UserRole type which defines the allowed user roles.
import { DoctorSpecialty } from "../../enums/doctor-specialty.enum"; // Import the specialty type a doctor can be.
import { PatientGender } from "../../enums/patient-gender.enum"; // Import the gender which a patient can be.
import { IHospital } from "../../interfaces/hospital.interface"; // Import the Hospital interface describing the structure of the hospital object.
import { IUserForm } from "../../interfaces/user-form.interface"; // Import the user interface.
import { UserFormOutput } from "../../interfaces/user-form-output.interface"; // Import the form output interface.
import { UserFormValidationErrors } from "../../types/user-form-validation-errors.type"; // Import all possible error types.
import { validateUserForm } from "../../utils/user-form.validator"; // Import the validation function.


// Define the component decorator.
@Component({
  selector: "app-user-form",
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: "./user-form.html",
  styleUrls: ["./user-form.scss"]
})


// Componenet resposible for the rendering and managing the user creation/edit form.
export class UserFormComponent implements OnChanges{
  // Inputs: data coming into the component.
  @Input() role : UserRole = UserRole.Admin; // Speicifes the role of the user being created or edited
  @Input() hospitals : IHospital[] | null = null; // List of hopistla avaliable for selection in the form.
  @Input() defaultHospitalID : number | null = null; // The default ID that the user belongs to.
  @Input() showHospitalSelect : boolean = false; // Determines whether the hospital section dropdown should be visible.
  @Input() initialValue : IUserForm | null | undefined = undefined; // Initial value to populate the form when editing.
  @Input() apiError : string | null = null; // API errors that might happen when creating or updating the user.

  // Fires on every field change with the current form value.
  @Output() formChange = new EventEmitter<UserFormOutput>();


  form: IUserForm = this._blank(); // Initialise the form with blank values.
  errors: UserFormValidationErrors = {}; // Store all possible validation errors.

  // Make the enums avaliable in the template.
  userRole = UserRole; // Static list of type of users.
  patientGenders = PatientGender; // Static list of genders for patients.
  doctorSpecialty = DoctorSpecialty; // Static list of specialty for doctor.

  // Toggle whether the password field is visible or hidden in the form.
  showPassword : boolean = false; 

  // Derived from initialValue: true when editing an existing user, false when creating.
  isEditMode: boolean = false;

  // Starts as false so existing users don't need to re-enter their password.
  changePassword: boolean = false;

  // To know what is today date.
  today: string = new Date().toISOString().split('T')[0];


  // Angular function to detech changes in input properties.
  ngOnChanges(changes : SimpleChanges): void {
    // Repopulate the form whenever a new initial value is supplied.
    if (changes["initialValue"]) {
      const v = this.initialValue;
      this.isEditMode = v != null;
      this.changePassword = false;
      this.form = v ? { ...v } : this._blank();
      this.errors = {}; // Reset errors when initial value changes.
    }

    // Add the API errors into the errors object.
    if (changes["apiError"] && this.apiError) {
      this.errors = { ...this.errors, api: this.apiError };
    }

    
    if (changes["defaultHospitalID"] && !this.initialValue) {
      this.form.hospitalID = this.defaultHospitalID;
    }
  }


  // Method when the user flips the "Change password" toggle.
  onChangePasswordToggle(): void {
    // Clear the password field and its error whenever the toggle changes state.
    this.form.password = "";
    this.errors = { ...this.errors, password: undefined };
    this.onFieldChange();
  }


  // Method call whenever there is any update to the form field.
  onFieldChange() : void {
    this._validate(); // Validate the fields of the user form.
    this.formChange.emit({ value: { ...this.form }, valid: this._isValid() }); // Emit the form changes.
  }


  // Utility method to get enum keys.
  getEnumKeys(enumObj : Record<any, any | any>): any[] {
    return Object.keys(enumObj).filter(key => isNaN(Number(key)));
  }


  // Returns true when in edit mode and the change-password toggle is off.
  isPasswordSkipped(): boolean {
    return this.isEditMode && !this.changePassword;
  }

  // Method to call the validation function outside of the component.
  validate() : boolean {
    this._validate(); // Validate the form.
    return this._isValid(); // return the form is valid.
  }


  // Method to return all the value in the form outside of the component.
  getValue() : IUserForm {
    return {
      ...this.form,
      username: this.form.username.trim(),
      email: this.form.email.trim(),
      password: this.form.password.trim(),
      firstName: this.form.firstName.trim(),
      middleName: this.form.middleName.trim(),
      lastName: this.form.lastName.trim(),
      dob: this.form.dob.trim(),
    };
  }


  // Method to set the api error span if there is error relating to the api.
  setApiError(message : string): void {
    this.errors = { ...this.errors, api: message };
  }

  // Method to clear all validation errors.
  clearUserErrors() : void {
    this.errors = {};
  }


  // Method to clear the user form field.
  private _blank() : IUserForm {
    return {
      username: "",
      email: "",
      password: "",
      hospitalID: this.defaultHospitalID,
      role: UserRole.Admin,
      firstName: "",
      middleName: "",
      lastName: "",
      specialty: null,
      dob: "",
      gender: null
    };
  }


  // Function to validate the user form field.
  private _validate() : void {
    // Skip only in edit mode when the "Change password" toggle is off.
    const skipPassword = this.isEditMode && !this.changePassword;

    // Preserve the previous errors.
    const prev = this.errors;

    this.errors = validateUserForm(this.form, this.role, this.showHospitalSelect, skipPassword);

    // Preserve any api error unless a field-level error clears it implicitly.
    if (prev.api && !this.errors.api) {
      this.errors = { ...this.errors, api: prev.api };
    }
  }

  // Method to check if form is valid
  private _isValid() : boolean {
    return (
      !this.errors.username &&
      !this.errors.email &&
      !this.errors.password &&
      !this.errors.hospitalId &&
      !this.errors.firstName &&
      !this.errors.middleName &&
      !this.errors.lastName &&
      !this.errors.specialty &&
      !this.errors.dob &&
      !this.errors.gender
    );
  }
}