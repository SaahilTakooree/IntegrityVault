// Import dependecies.
import { Component, Input, Output, EventEmitter } from "@angular/core"; // Core Angular features for component, bindings, and DOM access.
import { CommonModule } from "@angular/common"; // Import common Angular module.
import { ITamperResult } from "../../interfaces/tamper-result.interface"; // Interface for verification result structure.
import { VerifyResultComponent } from "../verify-result/verify-result"; // Child component to display verification results.
import { EntityModalComponent } from "../entity-modal/entity-modal"; // Child component to display entity-modal.


// Define PDF verify component.
@Component({
    selector: "app-tamper-check",
    standalone: true,
    imports: [CommonModule, EntityModalComponent, VerifyResultComponent],
    templateUrl: "./tamper-check.html",
    styleUrls: ["./tamper-check.scss"]
})


// PDF verification component class.
export class TamperCheckComponent {
    
    // Inputs: data coming into the component.
    @Input() show = false; // Input whether to show the modal.
    @Input() recordName = ""; // Input for the name of the medical record.
    @Input() cid = ""; // Input for the cid of the medical record.
    @Input() loading = false; // Input whether the loading is happening.
    @Input() result: ITamperResult | null = null; // Input for the result of the verification.
    @Input() errorMessage: string | null = null; // Input for the error message that might happend during the verification.


    // Outputs: events the component emits to parent.
    @Output() closed = new EventEmitter<void>(); // Output event emitter to close the modal.


    // Method to close the modal.
    onClose(): void {
        this.closed.emit();
    }
}