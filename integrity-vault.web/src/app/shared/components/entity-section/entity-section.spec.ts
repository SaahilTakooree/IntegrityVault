// Import dependencies.
import { ComponentFixture, TestBed } from "@angular/core/testing"; // Import Angular testing utilities.
import { EntitySectionComponent } from "./entity-section"; // Import the component being tested.
import { By } from "@angular/platform-browser"; // Utility to query the DOM.
import { Component, Input, Output, EventEmitter } from "@angular/core"; // Import Angular core for mocking.
import { IColumnDefinition } from "../../interfaces/column-definition.interface"; // Import ColumnDef interface.
import { EntityTableComponent } from "../entity-table/entity-table"; // Import original child.


// Simple interface to satisfy the generic T in tests.
interface TestEntity {
    id: number;
    name: string;
}


// Mock child component to isolate EntitySectionComponent testing.
@Component({
    selector: "app-entity-table",
    standalone: true,
    template: "<div>Mock Table</div>"
})
class MockEntityTableComponent {
    @Input() columns: any[] = [];
    @Input() rows: any[] = [];
    @Input() emptyIcon: string = "";
    @Input() emptyMessage: string = "";
    @Input() currentUserId: number | null = null;
    @Output() editRow = new EventEmitter<any>();
    @Output() deleteRow = new EventEmitter<any>();
}


describe("EntitySectionComponent", () => {
    // Explicitly typed instance and fixture.
    let component: EntitySectionComponent<TestEntity>;
    let fixture: ComponentFixture<EntitySectionComponent<TestEntity>>;


    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [EntitySectionComponent],
        })
        .overrideComponent(EntitySectionComponent, {
            remove: { imports: [EntityTableComponent] },
            add: { imports: [MockEntityTableComponent] }
        })
        .compileComponents();

        // FIX: Explicitly pass the generic type to createComponent.
        fixture = TestBed.createComponent<EntitySectionComponent<TestEntity>>(EntitySectionComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });


    // Basic instantiation check.
    it("Should create the component.", () => {
        expect(component).toBeTruthy();
    });


    // Verifies rendering of title and add button.
    it("Should render the title and add button label.", () => {
        component.title = "Hospitals";
        component.addLabel = "New Hospital";
        fixture.detectChanges();

        const titleText = fixture.debugElement.query(By.css("h5")).nativeElement.textContent;
        const buttonText = fixture.debugElement.query(By.css(".btn-primary")).nativeElement.textContent;

        expect(titleText).toContain("Hospitals");
        expect(buttonText).toContain("New Hospital");
    });


    // Validates that the warning section appears correctly under specific conditions.
    it("Should show warning message only when warningMessage is present and addDisable is true.", () => {
        component.warningMessage = "Limit reached";
        component.addDisable = true;
        fixture.detectChanges();

        const alert = fixture.debugElement.query(By.css(".alert"));
        expect(alert).toBeTruthy();
        expect(alert.nativeElement.textContent).toContain("Limit reached");
    });


    // Confirms that data is passed to the mock table component correctly.
    it("Should pass inputs correctly to the app-entity-table.", () => {
        const mockRows: TestEntity[] = [{ id: 1, name: "General Hospital" }];
        
        // FIX: Added 'mono' property to satisfy the IColumnDefinition interface.
        const mockCols: IColumnDefinition<TestEntity>[] = [
            { key: "name", label: "Name", mono: false }
        ];

        component.rows = mockRows;
        component.columns = mockCols;
        component.currentUserId = 123;
        fixture.detectChanges();

        const tableComponent = fixture.debugElement.query(By.directive(MockEntityTableComponent)).componentInstance;
        
        expect(tableComponent.rows).toEqual(mockRows);
        expect(tableComponent.columns).toEqual(mockCols);
        expect(tableComponent.currentUserId).toBe(123);
    });


    // Verifies the re-emission of the editRow event.
    it("Should re-emit editRow event from the table.", () => {
        spyOn(component.editRow, "emit");
        const mockRow: TestEntity = { id: 1, name: "General Hospital" };
        
        const tableComponent = fixture.debugElement.query(By.directive(MockEntityTableComponent)).componentInstance;
        tableComponent.editRow.emit(mockRow);

        expect(component.editRow.emit).toHaveBeenCalledWith(mockRow);
    });


    // Verifies the re-emission of the deleteRow event.
    it("Should re-emit deleteRow event from the table.", () => {
        spyOn(component.deleteRow, "emit");
        const mockRow: TestEntity = { id: 1, name: "General Hospital" };
        
        const tableComponent = fixture.debugElement.query(By.directive(MockEntityTableComponent)).componentInstance;
        tableComponent.deleteRow.emit(mockRow);

        expect(component.deleteRow.emit).toHaveBeenCalledWith(mockRow);
    });
});