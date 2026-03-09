// Define and export a TypeScript interface named Hospital.
export interface IHospital extends Record<string, unknown> {
    id : number; // Unique numeric identifier for the hospital.
    name : string; // Name of the hospital stored as a string.
    walletAddress : string; // Blockchain wallet address associated with the hospital.
    ipAddresses: string[]; // List of public IP addresses associated with the hospital.
}