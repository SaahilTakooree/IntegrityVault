// Import dependencies.
import { PatientGender } from "../enums/patient-gender.enum"; // Import the PatientGender enum.
import { UserRole } from "../enums/user-role.enum"; // Import all user role.


// Define and export a TypeScript interface named Patient.
export interface IPatient {
    id : number; // Unique numeric identifier for the patient.
    username : string; // Username of the patient stored as a string.
    email : string; // Email of the patient stored as a string.
    password : string; // Password of the patient stored as a string.
    joinDate : Date; // Join date of the patient stored as date.
    role: UserRole; // Role assigned to the patient.
    hospitalID: number; // Hospital that the patient is assigned to.
    firstName: string; // First name of the patient.
    middleName?: string; // Optional middle name of the patient.
    lastName: string; // Last name of the patient.
    dob: string;  // Patient's date of birth in ISO date string format.
    gender: PatientGender; // Patient's gender.
}