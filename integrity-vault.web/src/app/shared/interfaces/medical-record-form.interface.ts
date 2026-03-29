// Represents the what the medical record is going to look like.
export interface IMedicalRecordForm {
  patientID: number | null;
  visitDate: string;
  chiefComplaint: string;
  diagnosis: string;
  treatmentPlan: string;
  doctorNotes: string;
  followUpInstructions: string;
}