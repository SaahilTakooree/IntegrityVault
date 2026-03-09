// Import dependencies.
import { UserRole } from "../enums/user-role.enum"; // Import all user role.


// Define and export a TypeScript interface named Admin.
export interface IAdmin {
    id : number; // Unique numeric identifier for the admin.
    username : string; // Username of the admin stored as a string.
    email : string; // Email of the admin stored as a string.
    password : string; // Password of the admin stored as a string.
    role: UserRole; // Role assigned to the admin.
    joinDate : Date; // Join date of the admin stored as date.
    hospitalID : number; // Hospital that the admin is assigned to.
}