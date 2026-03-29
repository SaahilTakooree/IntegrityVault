// Import dependecies.
import { Component, Input, Output, EventEmitter } from "@angular/core"; // Core Angular features for component, bindings, and DOM access.
import { CommonModule } from "@angular/common"; // Import common Angular module.
import { EntityModalComponent } from "../entity-modal/entity-modal"; // Child component to display entity-modal.
import { IRecordVersion, IRecordViewData } from "../../interfaces/doctor-history.interface"; // Import the interface 


// Define view medical record component.
@Component({
    selector: "app-view-medical-record",
    standalone: true,
    imports: [CommonModule, EntityModalComponent],
    templateUrl: "./view-medical-record.html",
    styleUrls: ["./view-medical-record.scss"]
})


// View medical record component class.
export class ViewMedicalRecordComponent {
    
    // Inputs: data coming into the component.
    @Input() show = false; // Input whether to show the modal.
    @Input() loading = false; // Input whether the loading is happening.
    @Input() record: IRecordViewData | null = null; // Input the records to be displayed.
    @Input() version: IRecordVersion | null = null; // Input the version to be displayed.


    // Outputs: events the component emits to parent.
    @Output() closed = new EventEmitter<void>(); // Output event emitter to close the modal.
}