// Import dependencies.
import { IMedicalRecordForm } from '../../interfaces/medical-record-form.interface'; // Import the form interface.
import { MedicalRecordFormValidationErrors } from '../../types/medical-record-form-validation-errors.type'; // Import the error type


// Validation function for medical record form.
export function validateMedicalRecordForm(form: IMedicalRecordForm, requirePatient: boolean, requireChiefComplaint: boolean): MedicalRecordFormValidationErrors {

    // Initialise empty errors object.
    const errors: MedicalRecordFormValidationErrors = {};


    // Check if patient is required and missing.
    if (requirePatient && !form.patientID) {
        errors.patientID = 'Patient is required.';
    }

    // Validate chief complaint field.
    if (requireChiefComplaint && !form.chiefComplaint?.trim()) {
        errors.chiefComplaint = 'Chief complaint is required.';
    }

    // Validate visit date field.
    if (!form.visitDate?.trim()) {
        errors.visitDate = 'Visit date is required.';
    }

    // Validate diagnosis field
    if (!form.diagnosis?.trim()) {
        errors.diagnosis = 'Diagnosis is required.';
    }

    // Validate treatment plan field.
    if (!form.treatmentPlan?.trim()) {
        errors.treatmentPlan = 'Treatment plan is required.';
    }

    // Return validation errors.
    return errors;
}


// Parse API error into readable message.
export function parseMedicalRecordApiError(err: unknown): string {

    // Check if error object contains an error property.
    if (err && typeof err === 'object' && 'error' in err) {
        const e = err as any;

        // Return error if it's a string.
        if (typeof e.error === 'string') return e.error;

        // Return nested error message if available.
        if (typeof e.error?.message === 'string') return e.error.message;
    }

    // Fallback generic error message.
    return 'An unexpected error occurred. Please try again.';
}