// Represent all the type of errors that medical record form can have.
export type MedicalRecordFormValidationErrors = {
    chiefComplaint?: string;
    patientID?: string;
    visitDate?: string;
    diagnosis?: string;
    treatmentPlan?: string;
    api?: string;
}