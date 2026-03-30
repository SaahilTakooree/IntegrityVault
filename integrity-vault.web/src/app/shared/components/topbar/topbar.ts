// Import dependencies.
import { Component, Input, Output, EventEmitter, inject } from "@angular/core"; // Import Angular core module.
import { CommonModule } from "@angular/common"; // Import CommonModule for common directives.
import { AuthService } from "../../../core/services/auth/auth.service"; // Import the authtication services.


// Define the component decorator.
@Component({
    selector: "app-topbar",
    standalone: true,
    imports: [CommonModule],
    templateUrl: "./topbar.html",
    styleUrls: ["./topbar.scss"]
})


// Define the TopbarComponent class.
export class TopbarComponent {
    // Get the authentication services.
    private readonly _authService = inject(AuthService);


    // Inputs: data coming into the component.
    @Input() message : string = "Hi"; // Input for custom message.
    @Input() showLogout : boolean = false; // Input to show the logout or not.


    // Event emitter to toggle sidebar.
    @Output() toggleSidebar = new EventEmitter<void>();


    // Log the user out when click.
    onLogout(): void {
        this._authService.logout();
    }   
}