// Import dependencies.
import { DoctorSpecialty } from "../enums/doctor-specialty.enum"; // Import the DoctorSpecialty enum.
import { UserRole } from "../enums/user-role.enum"; // Import all user role.


// Define and export a TypeScript interface named Doctor.
export interface IDoctor {
    id : number; // Unique numeric identifier for the doctor.
    username : string; // Username of the doctor stored as a string.
    email : string; // Email of the doctor stored as a string.
    password : string; // Password of the doctor stored as a string.
    role : UserRole; // Role assigned to the doctor.
    joinDate : Date; // Join date of the doctor stored as date.
    hospitalID : number; // Hospital that the doctor is assigned to.
    firstName: string; // First name of the doctor.
    middleName?: string; // Optional middle name of the doctor.
    lastName: string; // Last name of the doctor.
    specialty: DoctorSpecialty; // Specialty of the doctor.
}