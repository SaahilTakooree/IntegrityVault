// Represents the full tamper verification result returned by the API.
export interface ITamperResult {
    isTampered: boolean;
    status: "Intact" | "Tampered" | "Unauthorised";
    contentHashMatch: boolean;
    databaseHashMatch: boolean;
    cidMatch: boolean;
    versionHashMatch: boolean;
    message: string;
}