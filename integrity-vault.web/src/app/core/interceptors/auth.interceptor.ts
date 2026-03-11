// Import dependencies.
import { HttpInterceptorFn, HttpErrorResponse } from "@angular/common/http"; // Import HTTP interceptor types.
import { inject } from "@angular/core"; // Used to inject dependencies.
import { catchError, throwError } from "rxjs"; // RxJS operators for error handling.
import { AuthService } from "../services/auth.service"; // Handles authentication.


// Auth HTTP interceptor.
export const authInterceptor: HttpInterceptorFn = (req, next) => {
    const auth = inject(AuthService); // Get the auth service
    const token = auth.getToken();// Get JWT token.

    // Define the public endpoints.
    const isPublic = req.url.includes("ipify.org") || req.url.includes("api/Auth");

    // Check if the token need to be send with request.
    const cloned = (!isPublic && token)
        ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) // Add token if needed.
        : req; // Keep original request if public

    return next(cloned).pipe(
        catchError((error: HttpErrorResponse) => {
            // If token is expired or rejected by server, force a logout.
            if (error.status === 401)
                auth.logout();
            
            return throwError(() => error); // Pass the error downstream.
        })
    );
};