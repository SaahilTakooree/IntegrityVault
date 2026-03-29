// Import dependencies.
import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges } from "@angular/core"; // Import the Angular core features for component and lifecycle.
import { CommonModule } from "@angular/common"; // Import the common Angular module.
import { FormsModule } from "@angular/forms"; // Import the forms module.
import { IMedicalRecordForm } from "../../interfaces/medical-record-form.interface"; // Import the medical record form interface.
import { MedicalRecordFormValidationErrors } from "../../types/medical-record-form-validation-errors.type"; // Import the validation errors type.
import { validateMedicalRecordForm } from "../../utils/medical-record/medical-record-form.validator"; // Import the validation utility function.
import { MedicalRecordFormOutput } from "../../interfaces/medical-record-form-output.interface" // Import the form output interface.


// Define medical record form component.
@Component({
  selector: "app-medical-record-form",
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: "./medical-record-form.html"
})


// Medical record form component class.
export class MedicalRecordFormComponent implements OnChanges {

  // Toggle patient selection visibility.
  @Input() showPatientSelect = false;

  // Toggle chief complaint visibility.
  @Input() showChiefComplaint = false;

  // List of patients.
  @Input() patients: { id: number; fullName: string }[] = [];

  // Initial visit date value.
  @Input() initialVisitDate: string = "";

  // Initial form value.
  @Input() initialValue: IMedicalRecordForm | null = null;

  // Context label for form usage.
  @Input() contextLabel: string = "";

  // Emit form changes.
  @Output() formChange = new EventEmitter<MedicalRecordFormOutput>();


  // Form model state.
  form: IMedicalRecordForm = this._blank();

  // Validation errors state.
  errors: MedicalRecordFormValidationErrors = {};

  // Today's date in ISO format.
  today = new Date().toISOString().split("T")[0];


  // Handle input changes.
  ngOnChanges(changes: SimpleChanges): void {
    if (changes["initialValue"] || changes["initialVisitDate"]) {
      if (this.initialValue) {
        this.form = { ...this.initialValue };
      } else {
        this.form = this._blank();
        if (this.initialVisitDate) {
          this.form.visitDate = this.initialVisitDate;
        } else {
          this.form.visitDate = this.today;
        }
      }
      this.errors = {};
    }
  }


  // Handle field changes.
  onFieldChange(): void {
    this._validate();
    this.formChange.emit({ value: { ...this.form }, valid: this._isValid() });
  }


  // Validate form manually.
  validate(): boolean {
    this._validate();
    return this._isValid();
  }


  // Get current form value.
  getValue(): IMedicalRecordForm {
    return { ...this.form };
  }


  // Set API error message.
  setApiError(msg: string): void {
    this.errors = { ...this.errors, api: msg };
  }


  // Reset form to default state.
  resetForm(): void {
    this.form = this._blank();
    if (this.initialVisitDate) this.form.visitDate = this.initialVisitDate;
    else this.form.visitDate = this.today;
    this.errors = {};
  }


  // Create blank form object.
  private _blank(): IMedicalRecordForm {
    return {
      patientID: null,
      visitDate: this.today,
      chiefComplaint: "",
      diagnosis: "",
      treatmentPlan: "",
      doctorNotes: "",
      followUpInstructions: ""
    };
  }


  // Run validation logic.
  private _validate(): void {
    this.errors = validateMedicalRecordForm(this.form, this.showPatientSelect, this.showChiefComplaint);
  }


  // Check if form is valid.
  private _isValid(): boolean {
    return !this.errors.patientID && !this.errors.chiefComplaint &&
      !this.errors.visitDate && !this.errors.diagnosis && !this.errors.treatmentPlan;
  }
}