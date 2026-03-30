// Import dependencies.
import { ComponentFixture, TestBed } from "@angular/core/testing"; // Import Angular testing utilities.
import { EntityTableComponent } from "./entity-table"; // Import the component being tested.
import { By } from "@angular/platform-browser"; // Utility to query the DOM.
import { IColumnDefinition } from "../../interfaces/column-definition.interface"; // Import ColumnDef interface.


// Simple interface to satisfy the generic T in tests.
interface TestUser {
    id: number;
    name: string;
    joinedDate: Date;
}


describe("EntityTableComponent", () => {
    // Explicitly typed instance and fixture.
    let component: EntityTableComponent<TestUser>;
    let fixture: ComponentFixture<EntityTableComponent<TestUser>>;


    beforeEach(async () => {
        // Set up testing module.
        await TestBed.configureTestingModule({
            imports: [EntityTableComponent],
        }).compileComponents();

        // Create component instance with generic type.
        fixture = TestBed.createComponent<EntityTableComponent<TestUser>>(EntityTableComponent);
        component = fixture.componentInstance;

        // Setup default column definitions.
        component.columns = [
            { key: "name" as keyof TestUser, label: "User Name", mono: false },
            { key: "joinedDate" as keyof TestUser, label: "Joined", mono: true }
        ];

        fixture.detectChanges(); // Trigger initial data binding.
    });


    // Basic instantiation check.
    it("Should create the component.", () => {
        expect(component).toBeTruthy();
    });


    // Verifies that the empty state message and icon appear when no rows are provided.
    it("Should display empty message when rows array is empty.", () => {
        component.rows = [];
        component.emptyMessage = "No Users Found";
        component.emptyIcon = "bi-person-x";
        fixture.detectChanges();

        const emptyCell = fixture.debugElement.query(By.css("td.text-center"));
        const icon = fixture.debugElement.query(By.css("i.bi-person-x"));

        expect(emptyCell.nativeElement.textContent).toContain("No Users Found");
        expect(icon).toBeTruthy();
        // Check colspan covers all columns + action column.
        expect(emptyCell.attributes["colspan"]).toBe("3"); 
    });


    // Confirms that the sortedRows getter correctly places the current user at the top of the list.
    it("Should sort the current user to the top of the table.", () => {
        const currentUser = { id: 99, name: "Me", joinedDate: new Date() };
        component.rows = [
            { id: 1, name: "Alice", joinedDate: new Date() },
            currentUser,
            { id: 2, name: "Bob", joinedDate: new Date() }
        ];
        component.currentUserId = 99;

        const sorted = component.sortedRows;

        expect(sorted[0].id).toBe(99);
        expect(sorted[0].name).toBe("Me");
    });


    // Checks that the getValue method correctly formats Date objects using toLocaleDateString.
    it("Should format Date values correctly using getValue.", () => {
        const testDate = new Date(2026, 2, 28); // March 28, 2026.
        const row = { id: 1, name: "Alice", joinedDate: testDate };
        const column = component.columns[1]; // The joinedDate column.

        const result = component.getValue(row, column);

        expect(result).toBe(testDate.toLocaleDateString());
    });


    // Ensures that the column transform function is used if it is provided in the column definition.
    it("Should use transform function in getValue if provided.", () => {
        const row = { id: 1, name: "alice", joinedDate: new Date() };
        const column: IColumnDefinition<TestUser> = { 
            key: "name", 
            label: "Name", 
            mono: false,
            transform: (val: any) => String(val).toUpperCase()
        };

        const result = component.getValue(row, column);

        expect(result).toBe("ALICE");
    });


    // Validates that the delete button is hidden when the row represents the current user.
    it("Should hide the delete button for the current user.", () => {
        component.currentUserId = 10;
        component.rows = [{ id: 10, name: "Current User", joinedDate: new Date() }];
        fixture.detectChanges();

        const deleteBtn = fixture.debugElement.query(By.css(".btn-outline-danger"));
        
        expect(deleteBtn).toBeNull();
    });


    // Verifies that the editRow event is emitted with the correct row data when the edit button is clicked.
    it("Should emit editRow when the edit button is clicked.", () => {
        spyOn(component.editRow, "emit");
        const row = { id: 1, name: "Alice", joinedDate: new Date() };
        component.rows = [row];
        fixture.detectChanges();

        const editBtn = fixture.debugElement.query(By.css(".btn-outline-secondary"));
        editBtn.triggerEventHandler("click", null);

        expect(component.editRow.emit).toHaveBeenCalledWith(row);
    });


    // Checks that the deleteRow event is emitted when the delete button is clicked for a non-current user.
    it("Should emit deleteRow when the delete button is clicked.", () => {
        spyOn(component.deleteRow, "emit");
        const row = { id: 5, name: "Other User", joinedDate: new Date() };
        component.currentUserId = 10; // I am user 10.
        component.rows = [row];
        fixture.detectChanges();

        const deleteBtn = fixture.debugElement.query(By.css(".btn-outline-danger"));
        deleteBtn.triggerEventHandler("click", null);

        expect(component.deleteRow.emit).toHaveBeenCalledWith(row);
    });


    // Verifies that CSS classes like font-mono and fw-medium are applied correctly to cells.
    it("Should apply CSS classes based on column definition.", () => {
        component.rows = [{ id: 1, name: "Alice", joinedDate: new Date() }];
        fixture.detectChanges();

        const nameCell = fixture.debugElement.query(By.css("td.fw-medium"));
        const monoCell = fixture.debugElement.query(By.css("td.font-mono"));

        expect(nameCell).toBeTruthy();
        expect(nameCell.nativeElement.textContent).toContain("Alice");
        expect(monoCell).toBeTruthy();
    });
});