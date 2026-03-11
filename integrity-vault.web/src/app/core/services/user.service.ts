// Import necessary angular dependencies to create the service.
import { Injectable, inject } from "@angular/core"; // Injectable decorator and dependency injection functionality.
import { HttpClient, HttpParams } from "@angular/common/http"; // HttpClient for making HTTP requests and params parsing.
import { Observable } from "rxjs"; // Observable for managing asynchronous streams of data.
import { IAdmin } from "../../shared/interfaces/admin.interface"; // Admin interface for type checking admin.
import { IDoctor } from "../../shared/interfaces/doctor.interface"; // Doctor interface for type checking doctor.
import { IExternalProvider } from "../../shared/interfaces/external-provider.interface"; // ExternalProvider interface for type checking external provider.
import { IPatient } from "../../shared/interfaces/patient.interface"; // Patient interface for type checking patient.


// Makes the service available at the root level of the application, so it"s accessible globally.
@Injectable({
    providedIn: "root"
})


// Service for handling user related API requests.
export class UserService {
    // Inject HttpClient for making HTTP requests.
    private readonly _http = inject(HttpClient);

    // Base API URL for the user api.
    private readonly _apiUrl = "https://localhost:7018/api/User";


    // Fetch the list of all users.
    getAllUsers(hospitalId?: number) : Observable<(IAdmin | IDoctor | IExternalProvider | IPatient)[]> {
        let params = new HttpParams();

        if (hospitalId != null) {
            params = params.set("hospitalId", hospitalId.toString());
        }

        return this._http.get<(IAdmin | IDoctor | IExternalProvider | IPatient)[]>(this._apiUrl, {params}).pipe();
    }

    // Fetch a specific user by their ID.
    getUserById(id : number) : Observable<IAdmin | IDoctor | IExternalProvider | IPatient> {
        return this._http.get<IAdmin | IDoctor | IExternalProvider | IPatient>(`${this._apiUrl}/${id}`).pipe();
    }

    // Add a new doctor to the system.
    createDoctor(doctor : IDoctor) : Observable<boolean> {
        return this._http.post<boolean>(`${this._apiUrl}/doctor`, doctor).pipe();
    }

    // Add a new patient to the system.
    createPatient(patient : IPatient) : Observable<boolean> {
        return this._http.post<boolean>(`${this._apiUrl}/patient`, patient).pipe();
    }

    // Add a new admin to the system.
    createAdmin(admin : IAdmin) : Observable<boolean> {
        return this._http.post<boolean>(`${this._apiUrl}/admin`, admin).pipe();
    }

    // Add a new external provider to the system.
    createExternalProvider(externalProvider : IExternalProvider) : Observable<boolean> {
        return this._http.post<boolean>(`${this._apiUrl}/externalprovider`, externalProvider).pipe();
    }

    // Update an existing doctor"s data.
    updateDoctor(id : number, doctor : IDoctor) : Observable<boolean> {
        return this._http.patch<boolean>(`${this._apiUrl}/doctor/${id}`, doctor).pipe();
    }

    // Update an existing patient"s data.
    updatePatient(id : number, patient : IPatient) : Observable<boolean> {
        return this._http.patch<boolean>(`${this._apiUrl}/patient/${id}`, patient).pipe();
    }

    // Update an existing admin"s data.
    updateAdmin(id : number, admin : IAdmin) : Observable<boolean> {
        return this._http.patch<boolean>(`${this._apiUrl}/admin/${id}`, admin).pipe();
    }

    // Update an existing external provider"s data.
    updateExternalProvider(id : number, externalProvider : IExternalProvider) : Observable<boolean> {
        return this._http.patch<boolean>(`${this._apiUrl}/externalprovider/${id}`, externalProvider).pipe();
    }

    // Delete a user by their ID.
    deleteUser(id : number) : Observable<boolean> {
        return this._http.delete<boolean>(`${this._apiUrl}/${id}`).pipe();
    }
}