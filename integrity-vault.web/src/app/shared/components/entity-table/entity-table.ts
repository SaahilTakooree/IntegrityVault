// Import dependencies.
import { Component, Input, Output, EventEmitter } from "@angular/core"; // Import Angular core module.
import { CommonModule } from "@angular/common"; // Import CommonModule for common directives.
import { IColumnDefinition } from "../../interfaces/column-definition.interface"; // Import ColumnDef interface.


// Define the component decorator.
@Component({
    selector: "app-entity-table",
    standalone: true,
    imports: [CommonModule],
    templateUrl: "./entity-table.html",
    styleUrls: ["./entity-table.scss"]
})


// Generic reusable table componenet used to display entity data.
export class EntityTableComponent<T extends object> {
    // Inputs: data coming into the component.
    @Input() columns : IColumnDefinition<T>[] = [];  // Input property that receives the column configuration describing how each column should be displayed.
    @Input() rows : T[] = []; // Input property that receives the array of rows to render the table.
    @Input() emptyIcon : string = "bi bi-table"; // Input property defining the icon displayed when the table has no data.
    @Input() emptyMessage : string = "No Record Yet."; // Input property defining the message displayed when there are no record in the table.
    @Input() currentUserId : number | null = null; // Input the user id of the one who is currently viewing that table.


    // Outputs: events the component emits to parent.
    @Output() editRow = new EventEmitter<T>(); // Output event emitter triggered when a row edit action is requested.
    @Output() deleteRow = new EventEmitter<T>(); // Output event emitter triggered when a row delete action is requested.

    
    // Method to sort the row withe the current user to be on top.
    get  sortedRows() :  T[] {
        // No need to sort if the current user id not provided.
        if (this.currentUserId == null) 
            return this.rows

        return [...this.rows].sort((a, b) => {
            const aIsMe = (a as any).id === this.currentUserId ? -1 : 0;
            const bIsMe = (b as any).id === this.currentUserId ? 1 : 0;
            return aIsMe + bIsMe;
        });
    }


    // Method to check if row belong to the current user.
    isCurrentUser(row : T) : boolean {
        return this.currentUserId != null && (row as any).id === this.currentUserId;
    }


    // Helper function used to extract and format a value from a row based.
    getValue(row : T, column: IColumnDefinition<T>) : string {
        // Retrieve the value from the row using the provided key.
        const value = row[column.key];

        // If a transform function is defined on the column, use it.
        if (column.transform)
            return column.transform(value);
        
        // If the value is a Date object, convert it to a readable locale date string.
        if (value instanceof Date) 
            return value.toLocaleDateString();

        // Return the convert value to a string if it exists, or return an empty string if null or undefined.
        return value != null ? String(value) : "";
    }
}