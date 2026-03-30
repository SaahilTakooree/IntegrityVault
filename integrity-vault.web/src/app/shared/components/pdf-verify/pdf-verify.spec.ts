// Import dependencies.
import { ComponentFixture, TestBed } from "@angular/core/testing"; // Import Angular testing utilities.
import { PdfVerifyComponent } from "./pdf-verify"; // Import the component being tested.
import { By } from "@angular/platform-browser"; // Utility to query the DOM.
import { VerifyResultComponent } from "../verify-result/verify-result"; // Child component dependency.


describe("PdfVerifyComponent", () => {
    // Component instance and testing fixture.
    let component: PdfVerifyComponent;
    let fixture: ComponentFixture<PdfVerifyComponent>;


    beforeEach(async () => {
        // Set up testing module.
        await TestBed.configureTestingModule({
            // Note: VerifyResultComponent is imported here because it's used in the template.
            imports: [PdfVerifyComponent, VerifyResultComponent],
        }).compileComponents();

        // Create component instance.
        fixture = TestBed.createComponent(PdfVerifyComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });


    // Basic instantiation check.
    it("Should create the component.", () => {
        expect(component).toBeTruthy();
    });


    // Verifies that clicking the dropzone triggers the hidden file input.
    it("Should trigger file input click when dropzone is clicked.", () => {
        const spy = spyOn(component.fileInputRef.nativeElement, "click");
        
        const dropzone = fixture.debugElement.query(By.css(".iv-verify-dropzone"));
        dropzone.nativeElement.click();

        expect(spy).toHaveBeenCalled();
    });


    // Confirms that selecting a file via the input updates the state and emits clearRequested.
    it("Should handle file selection from input.", () => {
        spyOn(component.clearRequested, "emit");
        const mockFile = new File(["pdf content"], "test.pdf", { type: "application/pdf" });
        const event = { target: { files: [mockFile] } } as unknown as Event;

        component.onFileSelected(event);

        expect(component.selectedFile).toBe(mockFile);
        expect(component.clearRequested.emit).toHaveBeenCalled();
    });


    // Validates drag over state changes for visual feedback (CSS classes).
    it("Should set isDragging to true on dragover.", () => {
        const dropzone = fixture.debugElement.query(By.css(".iv-verify-dropzone"));
        const dragEvent = new DragEvent("dragover");
        
        dropzone.triggerEventHandler("dragover", dragEvent);
        fixture.detectChanges();

        expect(component.isDragging).toBeTrue();
        expect(dropzone.nativeElement.classList).toContain("dragover");
    });


    // Ensures that dropping a valid PDF file updates the component state.
    it("Should handle valid PDF file drop.", () => {
        const mockFile = new File(["pdf content"], "dropped.pdf", { type: "application/pdf" });
        const dropEvent = jasmine.createSpyObj("DragEvent", ["preventDefault"]);
        dropEvent.dataTransfer = { files: [mockFile] };

        component.onDrop(dropEvent);

        expect(component.selectedFile).toBe(mockFile);
        expect(component.isDragging).toBeFalse();
    });


    // Confirms that non-PDF files are ignored during a drop event.
    it("Should ignore non-PDF files on drop.", () => {
        const mockFile = new File(["text content"], "test.txt", { type: "text/plain" });
        const dropEvent = jasmine.createSpyObj("DragEvent", ["preventDefault"]);
        dropEvent.dataTransfer = { files: [mockFile] };

        component.onDrop(dropEvent);

        expect(component.selectedFile).toBeNull();
    });


    // Verifies that the verifyRequested event is emitted with the file when submitting.
    it("Should emit verifyRequested when submit is called.", () => {
        spyOn(component.verifyRequested, "emit");
        const mockFile = new File(["pdf"], "test.pdf", { type: "application/pdf" });
        component.selectedFile = mockFile;

        component.submit();

        expect(component.verifyRequested.emit).toHaveBeenCalledWith(mockFile);
    });


    // Validates that the UI displays the filename and size once a file is selected.
    it("Should display file info when a file is selected.", () => {
        component.selectedFile = new File(["content"], "record.pdf", { type: "application/pdf" });
        
        fixture.detectChanges();
        
        const fileNameElement = fixture.debugElement.query(By.css(".iv-verify-dropzone p.fw-semibold"));
        
        expect(fileNameElement).toBeTruthy();
        expect(fileNameElement.nativeElement.textContent).toContain("record.pdf");
    });


    // Checks that the loading spinner appears when the loading input is true.
    it("Should show loading spinner when loading is true.", () => {
        component.loading = true;
        fixture.detectChanges();

        const spinner = fixture.debugElement.query(By.css(".spinner-border"));
        expect(spinner).toBeTruthy();
        expect(fixture.nativeElement.textContent).toContain("Verifying PDF integrity");
    });


    // Verifies that clearAll resets the state and the file input value.
    it("Should clear selection and emit clearRequested.", () => {
        spyOn(component.clearRequested, "emit");
        
        component.selectedFile = new File([""], "test.pdf");
        
        component.clearAll();
        
        expect(component.selectedFile).toBeNull();
        expect(component.fileInputRef.nativeElement.value).toBe("");
        expect(component.clearRequested.emit).toHaveBeenCalled();
    });


    // Confirms the results component is rendered when verification finishes.
    it("Should render app-verify-result when result or error is present.", () => {
        component.loading = false;
        component.result = { isTampered: false, timestamp: "now" } as any;
        fixture.detectChanges();

        const resultComponent = fixture.debugElement.query(By.css("app-verify-result"));
        expect(resultComponent).toBeTruthy();
    });
});