// Import dependencies.
import { Component, Input, Output, EventEmitter } from "@angular/core"; // Import Angular core module.
import { CommonModule } from "@angular/common"; // Import CommonModule for common directives.


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
    // Inputs: data coming into the component.
    @Input() message : string = "Hi"; // Input for custom message.

    // Event emitter to toggle sidebar.
    @Output() toggleSidebar = new EventEmitter<void>();
}