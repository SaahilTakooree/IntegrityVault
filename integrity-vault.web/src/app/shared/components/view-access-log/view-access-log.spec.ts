// Import dependencies.
import { ComponentFixture, TestBed } from "@angular/core/testing"; // Import Angular testing utilities.
import { ViewAccessLogComponent } from "./view-access-log"; // Import the component being tested.
import { By } from "@angular/platform-browser"; // Utility to query the DOM.
import { EntityModalComponent } from "../entity-modal/entity-modal"; // Child component dependency.


describe("ViewAccessLogComponent", () => {
    // Component instance and testing fixture.
    let component: ViewAccessLogComponent;
    let fixture: ComponentFixture<ViewAccessLogComponent>;

    beforeEach(async () => {
        // Set up testing module.
        await TestBed.configureTestingModule({
            imports: [ViewAccessLogComponent, EntityModalComponent],
        }).compileComponents();

        fixture = TestBed.createComponent(ViewAccessLogComponent);
        component = fixture.componentInstance;

        component.show = true; 
        fixture.detectChanges();
    });


    // Basic instantiation check.
    it("Should create the component.", () => {
        expect(component).toBeTruthy();
    });


    // Verifies that the table renders one row per access log entry.
    it("Should render a row for each access log.", () => {
        component.record = {
            accessLogs: [
                { accessType: "Viewed", accessedByName: "Dr. House", accessedByRole: "Doctor", timestamp: "2026-01-01T10:00:00" },
                { accessType: "Downloaded", accessedByName: "John Doe", accessedByRole: "Patient", timestamp: "2026-01-02T12:00:00" }
            ]
        } as any;
        
        fixture.detectChanges();

        const rows = fixture.debugElement.queryAll(By.css("tbody tr"));
        expect(rows.length).toBe(2);
        expect(rows[0].nativeElement.textContent).toContain("Dr. House");
    });


    // Confirms that the role badge gets the correct bootstrap class.
    it("Should apply correct badge classes based on user role.", () => {
        component.record = {
            accessLogs: [
                { accessedByRole: "Doctor", accessType: "V", accessedByName: "N", timestamp: "T" },
                { accessedByRole: "Patient", accessType: "V", accessedByName: "N", timestamp: "T" },
                { accessedByRole: "Admin", accessType: "V", accessedByName: "N", timestamp: "T" }
            ]
        } as any;
        
        fixture.detectChanges();

        const badges = fixture.debugElement.queryAll(By.css(".badge"));
        expect(badges.length).withContext("Should find 3 badges").toBe(3);
        expect(badges[0].nativeElement.classList).toContain("bg-primary");
        expect(badges[1].nativeElement.classList).toContain("bg-info");
        expect(badges[2].nativeElement.classList).toContain("bg-secondary");
    });


    // Ensures the empty state message appears when no logs are present.
    it("Should show empty state message when accessLogs array is empty.", () => {
        component.record = { accessLogs: [] } as any;
        fixture.detectChanges();

        // Querying for the specific td.
        const emptyCell = fixture.debugElement.query(By.css("td.text-center"));
        
        expect(emptyCell).withContext("Empty state cell should exist").toBeTruthy();
        expect(emptyCell.nativeElement.textContent).toContain("No access logs recorded");
    });


    // Validates the timestamp formatting method.
    it("Should format timestamp correctly via formatTimestamp.", () => {
        const isoDate = "2026-03-29T15:30:00";
        const formatted = component.formatTimestamp(isoDate);
        
        expect(formatted).toContain("2026");
        expect(component.formatTimestamp("")).toBe("");
    });


    // Verifies that the 'closed' event is bubbled up from the modal.
    it("Should emit closed event when modal emits closed or confirmed.", () => {
        spyOn(component.closed, "emit");
        const modal = fixture.debugElement.query(By.directive(EntityModalComponent));

        modal.triggerEventHandler("closed", null);
        modal.triggerEventHandler("confirmed", null);

        expect(component.closed.emit).toHaveBeenCalledTimes(2);
    });


    // Checks that the table is completely hidden if the record input is null.
    it("Should not render the table if record is null.", () => {
        component.record = null;
        fixture.detectChanges();

        const table = fixture.debugElement.query(By.css("table"));
        expect(table).toBeNull();
    });
});