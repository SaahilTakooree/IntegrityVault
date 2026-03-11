// Import dependencies.
import { inject } from "@angular/core"; // Used for inject dependencies.
import { CanActivateFn, Router } from "@angular/router"; // Provides route guard and navigation.
import { AuthService } from "../services/auth.service"; // Handles authentication logic.


export const authGuard : CanActivateFn = () => {
    const authService = inject(AuthService); // Get the auth service.
    const router = inject (Router); // Get the router.

    // Allow access if logged in.
    if (authService.IsLoggedIn())
        return true

    // Redirect to login if not authenticated.
    router.navigate(["/login"]);

    // Block access.
    return false;
}