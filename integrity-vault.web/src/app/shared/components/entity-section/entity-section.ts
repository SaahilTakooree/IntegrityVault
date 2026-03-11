// Import dependencies.
import { Component, Input, Output, EventEmitter } from "@angular/core"; // Import Angular core module.
import { CommonModule } from "@angular/common"; // Import CommonModule for common directives.
import { EntityTableComponent } from "../entity-table/entity-table"; // Import 
import { IColumnDefinition } from "../../interfaces/column-definition.interface"; // Import ColumnDef interface.


// Define the component decorator.
@Component({
    selector: "app-entity-section",
    standalone: true,
    imports: [CommonModule, EntityTableComponent],
    templateUrl: "./entity-section.html",
    styleUrls: ["./entity-section.scss"]
})


// Generic section component used to wrap an entity table with a title, action, and optional warinigs.
export class EntitySectionComponent<T extends object> {
    // Inputs: data coming into the component.
    @Input() title : string = ""; // The title displayed at the top of the entity section.
    @Input() addLabel : string = "Add"; // Lable text for the "Add" button used to create a new entity.
    @Input() addDisable : boolean = false; // Determines wheather the "Add" is disable.
    @Input() columns : IColumnDefinition<T>[] =[]; // Column configuration describing how table data showed be displayed.
    @Input() rows : T[] = []; // Array of entity objects that will populate the table rows.
    @Input() emptyIcon : string = "bi bi-table"; // Icon displayed when the table contains no records.
    @Input() emptyMessage : string = "No Records Yet." // Message displayd when there are now rows to show.
    @Input() warningMessage : string = "" // Option warning message displayed above the table.
    @Input() warningActionLabel : string = "" // Label for optional action button associated with the warning message.
    @Input() currentUserId: number | null = null; // User id of the one who is currently viewing the table.


    // Outputs: events the component emits to parent.
    @Output() addClicked = new EventEmitter<void>() // Event triggered when the user clicks the "Add" button.
    @Output() warningAction = new EventEmitter<void>() // Event triggered when the user clicks the warning action button.
    @Output() editRow = new EventEmitter<T>() // Event emitted when a row edit action is triggered.
    @Output() deleteRow = new EventEmitter<T>() // Event emitted when a row delete action is triggered.
}