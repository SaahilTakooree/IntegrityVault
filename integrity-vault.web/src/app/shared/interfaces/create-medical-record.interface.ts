// Represent what is need to crate a medical record.
export interface CreateMedicalRecord {
  doctorID: number;
  patientID: number;
  specialty: number;
  visitDate: string;
  chiefComplaint: string;
  diagnosis: string;
  treatmentPlan: string;
  doctorNotes?: string;
  followUpInstructions?: string;
}