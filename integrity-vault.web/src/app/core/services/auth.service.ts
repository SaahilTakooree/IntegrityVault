// Import dependencies to create the auth service.
import { Injectable, inject } from "@angular/core"; // Injectable decorator and dependency injection functionality.
import { HttpClient } from "@angular/common/http"; // HttpClient for making HTTP requests
import { Router } from "@angular/router"; // Provides Router for navigating between routes.
import { BehaviorSubject, map, Observable, switchMap, tap } from "rxjs"; // Use for reactive programming.
import { LoginResponce } from "../../shared/interfaces/login-responce.interface"; // Defines the shape of data for for login requests.
import { LoginRequest } from "../../shared/interfaces/login-request.interface"; // Defines the shape of data for for login responce.
import { UserSession } from "../../shared/interfaces/user-session.interface"; // Defines the shape of data for for user session
import { DecodedToken } from "../../shared/interfaces/decoded-token.interface"; // Defines the shape of data for the decoded token
import { jwtDecode } from "jwt-decode"; // Use to decode JWT tokens into a readable object.


// Makes the service available at the root level of the application, so it"s accessible globally.
@Injectable({
  providedIn: "root"
})


// Service for handling user login.
export class AuthService {
  private readonly _http = inject(HttpClient); // Inject HttpClient for making API requests.
  private readonly _router = inject(Router); // Inject Router to handle navigation.
  private readonly _apiUrl = "https://localhost:7018/api/Auth"; // Base API URL for authentication endpoints.
  private readonly _tokenKey = "token"; // Key used to store the JWT token in sessionStorage.

  // In-memory user state.
  private readonly _user$ = new BehaviorSubject<UserSession | null>(this._loadUserFromSession()); // In-memory BehaviorSubject to track the currently logged-in user session.
  readonly user$ = this._user$.asObservable(); // Public Observable of the current user session.


  // Login with username/email and password.
  login(usernameOrEmail: string, password: string): Observable<LoginResponce> {
    return this._getPublicIp$().pipe(
      switchMap(ipAddress =>
        this._http.post<LoginResponce>(this._apiUrl, <LoginRequest>{
          usernameOrEmail,
          password,
          ipAddress
        })
      ),
      tap(({ token }) => this._handleLoginSuccess(token))
    )
  }


  // Method to log the user out.
  logout() : void {
    sessionStorage.removeItem(this._tokenKey);
    this._user$.next(null);
    this._router.navigate(["/login"]);
  }


  // Method to get the current JWT token stored in sessionStorage, if any.
  getToken() : string | null {
    return sessionStorage.getItem(this._tokenKey);
  }


  // Checks whether the user is currently logged in.
  IsLoggedIn() : boolean {
    const token = this.getToken()
    return !!token && !this._isTokenExpired(token)
  }


  // Returns the current user session from the BehaviorSubject.
  get CurrentUser() : UserSession | null {
    return this._user$.value
  }


  // Method to handle post login success logic
  private _handleLoginSuccess(token : string) : void {
    sessionStorage.setItem(this._tokenKey, token);
    const user = this._decodeToken(token);
    this._user$.next(user);
    this._redirectByRole(user.role);
  }


  // Method to decodes the JWT token into a UserSession object.
  private _decodeToken(token : string ) : UserSession {
    const decoded = jwtDecode<DecodedToken>(token);
    return {
      id: Number(decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"]),
      role: decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"],
      hospitalId: decoded.HospitalID ? Number(decoded.HospitalID) : null,
      lastName: decoded.LastName ?? ""
    };
  }


  // Method to check if a token is expired
  private _isTokenExpired(token : string) : boolean {
    try {
      const { exp } = jwtDecode<DecodedToken>(token);
      return Date.now() >= exp * 1000;
    } catch {
      return true;
    }
  }


  // Method to loads the user session from sessionStorage if a valid token exists.
  private _loadUserFromSession() : UserSession | null {
    const token = sessionStorage.getItem(this._tokenKey);

    if (!token || this._isTokenExpired(token)) {
      sessionStorage.removeItem(this._tokenKey);
      return null;
    }

    return this._decodeToken(token)
  }


  // Method to redirects the user to the appropriate route based on their role.
  private _redirectByRole(role: string): void {
    const roleRoutes: Record<string, string> = {
      SuperAdmin: "/superadmin",
      Admin: "/admin",
      Doctor: "/doctor",
      Patient: "/patient",
      ExternalProvider: "/external-provider"
    };
    this._router.navigate([roleRoutes[role] ?? "/login"]);
  }


  // Method to fetches the public IP address of the client.
  private _getPublicIp$() : Observable<string> {
    return this._http.get<{ ip: string }>("https://api.ipify.org?format=json")
      .pipe(map(res => res.ip));
  }
}