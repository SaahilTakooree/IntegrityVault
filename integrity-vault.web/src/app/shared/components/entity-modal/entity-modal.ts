// Import dependencies.
import { Component, Input, Output, EventEmitter } from "@angular/core"; // Import Angular core module.
import { CommonModule } from "@angular/common"; // Import CommonModule for common directives.


// Define the component decorator.
@Component({
    selector: "app-entity-modal",
    standalone: true,
    imports: [CommonModule],
    templateUrl: "./entity-modal.html",
    styleUrls: ["./entity-modal.scss"]
})


// Reusable modal componenet used for creating or edditn entities.
export class EntityModalComponent {
    // Inputs: data coming into the component.
    @Input() show : boolean = false; // Determines whether the modal is visible or hidden.
    @Input() title : string = ""; // Title text displayed at the top of the modal dialog.
    @Input() confirmLabel = "Save"; // Label text for the confirmation button.

    // Outputs: events the component emits to parent.
    @Output() closed = new EventEmitter<void>(); // Event triggered when the modal is closed or cancelled.
    @Output() confirmed = new EventEmitter<void>(); // Event triggered when the user clicks the confirm button.
}