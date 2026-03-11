// Represents the structure of a decoded authentication token.
export interface DecodedToken {
    exp : number; // Expiration time of the token.
    iss : string; // Issuer of the token.
    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name" : string; // Unique identifier of the authenticated user.
    "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" : string; // The role assigned to the authenticated user.
    HospitalID : string; // Unique identifier of the hospital associated with the user.
    LastName : string; // The last name of the authenticated user.
}