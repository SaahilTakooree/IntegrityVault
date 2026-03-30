// Import dependencies.
import { Injectable, inject } from "@angular/core"; // Import the Angular injectable decorator and inject function.
import { HttpClient } from "@angular/common/http"; // Import HTTP client for API calls/
import { Observable } from "rxjs"; // Import Observable for async handling.
import { IDoctorHistory, IRecordViewData } from "../../../shared/interfaces/doctor-history.interface"; // Import interfaces for doctor history and record view.
import { IPatientHistory } from "../../../shared/interfaces/patient-history.interface"; // Import interfaces for patient history and record view.
import { CreateMedicalRecord } from "../../../shared/interfaces/create-medical-record.interface" // Import interface for creating medical record.
import { ITamperResult } from "../../../shared/interfaces/tamper-result.interface" // Import interface for tamper result..
import { IPatient } from "../../../shared/interfaces/patient.interface"; // Import interface for patient.
import { IDoctor } from "../../../shared/interfaces/doctor.interface"; // Import interface for doctor.


// Define injectable service.
@Injectable({
    providedIn: "root"
})


// Medical record service class.
export class MedicalRecordService {

    // Inject the HttpClient instance.
    private readonly _http = inject(HttpClient);

    // Base API URL for medical records.
    private readonly _apiUrl = "https://localhost:7018/api/MedicalRecord";

    // Base API URL for user-related endpoints.
    private readonly _userApiUrl = "https://localhost:7018/api/User";


    // Create new medical record and new episode.
    createNewMedicalRecordAndEpisode(createMedicalRecord: CreateMedicalRecord): Observable<any> {
        return this._http.post<any>(this._apiUrl, createMedicalRecord);
    }

    // Add a medical record to an existing episode.
    addMedicalRecordToEpisode(episodeID: number, createMedicalRecord: CreateMedicalRecord): Observable<any> {
        return this._http.post<any>(`${this._apiUrl}/episode/${episodeID}`, createMedicalRecord);
    }

    // Update a medical record.
    updateMedicalRecord(medicalRecordID: number, episodeID: number, createMedicalRecord: CreateMedicalRecord): Observable<any> {
        return this._http.patch<any>(`${this._apiUrl}/episode/${episodeID}/${medicalRecordID}`, createMedicalRecord);
    }

    // Get doctor"s full medical history.
    getDoctorHistory(doctorID: number): Observable<IDoctorHistory> {
        return this._http.get<IDoctorHistory>(`${this._apiUrl}/doctor/${doctorID}/history`);
    }

    // Get patient's full medical history, grouped by specialty.
    getPatientHistory(patientID: number): Observable<IPatientHistory> {
        return this._http.get<IPatientHistory>(`${this._apiUrl}/patient/${patientID}/history`);
    }

    // Get medical record content from IPFS CID.
    getMedicalRecordFromCID(cid: string, userID: number): Observable<IRecordViewData> {
        return this._http.get<IRecordViewData>(`${this._apiUrl}/ipfs/${cid}/user/${userID}`);
    }

    // Check if IPFS record is tampered.
    checkTamperByCID(cid: string, userID: number): Observable<ITamperResult> {
        return this._http.get<ITamperResult>(`${this._apiUrl}/ipfs/${cid}/user/${userID}/tamper-check`);
    }

    // Verify PDF tampering.
    verifyPdfTampering(userID: number, file: File): Observable<ITamperResult> {
        const formData = new FormData();
        formData.append("file", file);
        return this._http.post<ITamperResult>(`${this._apiUrl}/pdf/tamper-check/user/${userID}`, formData);
    }

    // Download a medical record PDF from IPFS.
    downloadMedicalRecord(cid: string, userID: number): Observable<Blob> {
        return this._http.get(`${this._apiUrl}/ipfs/${cid}/user/${userID}/download`, {
            responseType: "blob"
        });
    }

    // Set episode active/inactive.
    setEpisodeStatus(episodeID: number, doctorID: number): Observable<any> {
        return this._http.patch<any>(`${this._apiUrl}/episode/${episodeID}/status`, doctorID);
    }

    // Get all patients for a hospital.
    getPatientsForHospital(hospitalID: number): Observable<IPatient[]> {
        return this._http.get<IPatient[]>(`${this._userApiUrl}/patient/${hospitalID}`);
    }

    // Get doctor by ID.
    getDoctorById(doctorID: number): Observable<IDoctor> {
        return this._http.get<any>(`${this._userApiUrl}/${doctorID}`);
    }
}