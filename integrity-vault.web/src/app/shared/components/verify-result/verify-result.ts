// Import dependencies.
import { Component, Input } from "@angular/core"; // Import the Angular core component.
import { CommonModule } from "@angular/common"; // Import commonModule.
import { ITamperResult } from "../../interfaces/tamper-result.interface"; // Import the tamper result interface.


// Define the component decorator.
@Component({
    selector: "app-verify-result",
    standalone: true,
    imports: [CommonModule],
    templateUrl: "./verify-result.html",
    styleUrls: ["./verify-result.scss"]
})


// Verify result component class.
export class VerifyResultComponent {

    // Inputs: data coming into the component.
    @Input() result: ITamperResult | null = null; // Input the result of the validation.
    @Input() errorMessage: string | null = null; // Input the error meesage that might have have happend when verifying a medical record.
    @Input() showDetails: boolean = true; // Input to show the details when of the result.


    // Derives which visual state to render.
    get state(): "intact" | "tampered" | "unauthorised" | "error" | null {
        if (this.errorMessage) {
            return "error";
        }
        
        if (!this.result) {
            return null;
        }
        
        if (this.result.status === "Unauthorised") {
            return "unauthorised";
        }
        
        return this.result.isTampered ? "tampered" : "intact";
    }


    // List of individual integrity checks to display in the UI.
    readonly checks = [
        { key: "contentHashMatch", label: "Content hash" },
        { key: "databaseHashMatch", label: "Database hash" },
        { key: "cidMatch", label: "CID match" },
        { key: "versionHashMatch", label: "Version chain" }
    ] as const;
}