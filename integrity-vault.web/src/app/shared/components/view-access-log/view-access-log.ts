// Import dependecies.
import { Component, Input, Output, EventEmitter } from "@angular/core"; // Core Angular features for component, bindings, and DOM access.
import { CommonModule } from "@angular/common"; // Import common Angular module.
import { EntityModalComponent } from "../entity-modal/entity-modal"; // Child component to entity modal.
import { IMedicalRecord } from "../../interfaces/doctor-history.interface"; // Interface for medical record structure.


// Define view access log component.
@Component({
    selector: "app-view-access-log",
    standalone: true,
    imports: [CommonModule, EntityModalComponent],
    templateUrl: "./view-access-log.html",
    styleUrls: ["./view-access-log.scss"]
})


// View access log component class.
export class ViewAccessLogComponent {
    
    // Inputs: data coming into the component.
    @Input() show = false; // Input whether to show the modal.
    @Input() record: IMedicalRecord | null = null; // Input for the result of the verification.


    // Outputs: events the component emits to parent.
    @Output() closed = new EventEmitter<void>(); // Output event emitter to close the modal.


    // Method to format a timestamp string into a readable locale string.
    formatTimestamp(ts: string): string {
        if (!ts) {
            return "";
        }
        return new Date(ts).toLocaleString();
    }
}