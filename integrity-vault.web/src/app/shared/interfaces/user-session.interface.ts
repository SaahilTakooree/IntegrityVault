// Represents the authenticated user's session information stored or used within the application after login.
export interface UserSession {
    id : number; // Unique identifier of the user.
    role : string; // Role assigned to the user.
    hospitalId : number | null; // Identifier of the hospital associated with the user.
    lastName : string; // Last name of the user.
}