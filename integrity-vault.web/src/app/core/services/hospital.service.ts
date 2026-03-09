// Import necessary angular dependencies to create the service.
import { Injectable, inject } from '@angular/core'; // Injectable decorator and dependency injection functionality.
import { HttpClient } from '@angular/common/http'; // HttpClient for making HTTP requests.
import { Observable } from 'rxjs'; // Observable for managing asynchronous streams of data.
import { IHospital } from '../../shared/interfaces/hospital.interface'; // Hospital interface for type-checking hospital.


// Makes the service available at the root level of the application, so it's accessible globally.
@Injectable({
    providedIn: 'root'
})


// Service for handling hospital-related API requests.
export class HospitalService {
    // Inject HttpClient for making HTTP requests.
    private _http = inject(HttpClient);

    // Base API URL for the hospital api.
    private _apiUrl = 'https://localhost:7018/api/Hospital';

    // Fetch the list of hospitals.
    getHospital(): Observable<IHospital[]> {
        return this._http.get<IHospital[]>(`${this._apiUrl}`).pipe();
    }

    // Add a new hospital to the system.
    addHospital(hospital: IHospital): Observable<IHospital> {
        return this._http.post<IHospital>(`${this._apiUrl}`, hospital).pipe();
    }

    // Update an existing hospital's data.
    updateHospital(hospital: IHospital): Observable<IHospital> {
        return this._http.patch<IHospital>(`${this._apiUrl}/${hospital.id}`, hospital).pipe();
    }

    //Delete a hospital by its ID.
    deleteHospital(id: number): Observable<void> {
        return this._http.delete<void>(`${this._apiUrl}/${id}`).pipe();
    }
}