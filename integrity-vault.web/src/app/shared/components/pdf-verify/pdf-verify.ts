// Import dependecies.
import { Component, Input, Output, EventEmitter, ViewChild, ElementRef } from "@angular/core"; // Core Angular features for component, bindings, and DOM access.
import { CommonModule } from "@angular/common"; // Import common Angular module.
import { ITamperResult } from "../../interfaces/tamper-result.interface"; // Interface for verification result structure.
import { VerifyResultComponent } from "../verify-result/verify-result"; // Child component to display verification results.


// Define PDF verify component.
@Component({
    selector: "app-pdf-verify",
    standalone: true,
    imports: [CommonModule, VerifyResultComponent],
    templateUrl: "./pdf-verify.html",
    styleUrls: ["./pdf-verify.scss"]
})


// PDF verification component class.
export class PdfVerifyComponent {
    
    // Inputs: data coming into the component.
    @Input() loading = false; // Input for wheather the loading is happening.
    @Input() result: ITamperResult | null = null; // Input for the result of the verification.
    @Input() errorMessage: string | null = null; // Input for the error message that might happend during the verification.


    // Outputs: events the component emits to parent.
    @Output() verifyRequested = new EventEmitter<File>(); // Output event emitter for verification request.
    @Output() clearRequested = new EventEmitter<void>(); // Output event emitter for clearing the result, pdf uploader and the error message.
    
    // Reference to file input element.
    @ViewChild("fileInput") fileInputRef!: ElementRef<HTMLInputElement>;
    

    // Selected file state.
    selectedFile: File | null = null;

    // Dragging state flag.
    isDragging = false;
    

    // Handle file selection from input.
    onFileSelected(event: Event): void {
        const input = event.target as HTMLInputElement;
        if (input.files?.length) {
            this._setFile(input.files[0]);
        }
    }
    
    
    // Handle file drop event.
    onDrop(event: DragEvent): void {
        event.preventDefault();
        this.isDragging = false;
        const file = event.dataTransfer?.files?.[0];
        
        if (file && file.type === "application/pdf") {
            this._setFile(file);
        }
    }
    

    // Handle drag over event.
    onDragOver(event: DragEvent): void {
        event.preventDefault();
        this.isDragging = true;
    }
    

    // Handle drag leave event.
    onDragLeave(): void {
        this.isDragging = false;
    }
    

    // Trigger file input click.
    triggerFileInput(): void {
        this.fileInputRef.nativeElement.click();
    }
    

    // Submit selected file for verification.
    submit(): void {
        if (this.selectedFile) {
            this.verifyRequested.emit(this.selectedFile);
        }
    }
    

    // Clear selected file.
    clearAll(): void {
        this.selectedFile = null;
        
        if (this.fileInputRef) {
            this.fileInputRef.nativeElement.value = "";
        }

        this.clearRequested.emit();
    }
    

    // Set selected file.
    private _setFile(file: File): void {
        this.selectedFile = file;
        this.clearRequested.emit();
    }
}