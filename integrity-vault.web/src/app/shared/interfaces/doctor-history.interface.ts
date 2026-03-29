// Define the access log entry interface.
export interface IAccessLog {
  accessType: string;
  accessedByName: string;
  accessedByRole: string;
  timestamp: string;
}


// Define the record version metadata interface.
export interface IRecordVersion {
  displayName: string;
  ipfS_CID: string;
  version: number;
  timestamp: string;
}


// Define the medical record interface containing versions and logs.
export interface IMedicalRecord {
  medicalRecordID: number;
  visitDate: string;
  currentVersion: number;
  versions: IRecordVersion[];
  accessLogs: IAccessLog[];
}


// Define the episode interface representing a medical case.
export interface IEpisode {
  episodeID: number;
  chiefComplaint: string;
  isActive: boolean;
  records: IMedicalRecord[];
}


// Define the patient history interface with episodes.
export interface IPatientHistory {
  patientID: number;
  patientFullName: string;
  episodes: IEpisode[];
}


// Define the doctor history interface with patients.
export interface IDoctorHistory {
  doctorID: number;
  doctorFullName: string;
  patients: IPatientHistory[];
}


// Define the record view data interface for displaying record details.
export interface IRecordViewData {
  chiefComplaint: string;
  diagnosis: string;
  treatmentPlan: string;
  doctorNotes?: string;
  followUpInstructions?: string;
}