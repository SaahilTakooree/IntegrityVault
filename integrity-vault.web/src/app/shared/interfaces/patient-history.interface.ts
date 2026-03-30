// Re-use the IMedicalRecord from the sub-interfaces.
import { IMedicalRecord } from "./doctor-history.interface";

// Define the specialty group interface.
export interface ISpecialtyGroup {
    speciality: string;
    episodes: IPatientEpisode[];
}


// Define the patient episode interface.
export interface IPatientEpisode {
    episodeID: number;
    chiefComplaint: string;
    isActive: boolean;
    records: IMedicalRecord[];
}


// Define the full patient history interface returned by the API.
export interface IPatientHistory {
    patientID: number;
    patientFullName: string;
    specialities: ISpecialtyGroup[];
}