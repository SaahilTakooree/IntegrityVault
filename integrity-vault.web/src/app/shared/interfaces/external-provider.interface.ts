// Import dependencies.
import { UserRole } from "../enums/user-role.enum"; // Import all user role.


// Define and export a TypeScript interface named ExternalProvider.
export interface IExternalProvider {
    id : number; // Unique numeric identifier for the external provider.
    username : string; // Username of the external provider stored as a string.
    email : string; // Email of the external provider stored as a string.
    password : string; // Password of the external provider stored as a string.
    role : UserRole; // // Role assigned to the external providers.
    joinDate : Date; // Join date of the external provider stored as date.
    hospitalID : number; // Hospital that the external provider is assigned to.
}