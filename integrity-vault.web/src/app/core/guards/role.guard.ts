// Import dependencies.
import { inject } from "@angular/core"; // Used for inject dependencies.
import { CanActivateFn, ActivatedRouteSnapshot, Router } from "@angular/router"; // Provides route guard and navigation.
import { AuthService } from "../services/auth.service"; // Handles authentication logic.


export const roleGuard : CanActivateFn = (route : ActivatedRouteSnapshot) => {
    const authService = inject(AuthService); // Get the auth service.
    const router = inject (Router); // Get the router.

    // Roles allowed for this route
    const allowedRoles : string[] = route.data["roles"] ?? [];

    // Get current user's role.
    const userRole = authService.CurrentUser?.role;

    // Allow if role matches.
    if (userRole && allowedRoles.includes(userRole))
        return true;

    // Redirect to login if not authenticated.
    router.navigate(["/login"]);

    // Block access.
    return false;
}