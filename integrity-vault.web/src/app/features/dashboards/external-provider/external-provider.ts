// Import dependencies.
import { Component, inject, OnDestroy } from "@angular/core"; // Import angular core component functionality.
import { CommonModule } from "@angular/common"; // Import common Angular module for common features.
import { Subject, takeUntil } from "rxjs"; // Import RxJS for managing subscriptions.
import { TopbarComponent } from "../../../shared/components/topbar/topbar"; // Import the topbar component.
import { PdfVerifyComponent } from "../../../shared/components/pdf-verify/pdf-verify"; // Import the PDF verification component.
import { MedicalRecordService } from "../../../core/services/medical-record/medical-record.service"; // Import the medical record service.
import { AuthService } from "../../../core/services/auth/auth.service"; // Service to get the current logged-in user.
import { ITamperResult } from "../../../shared/interfaces/tamper-result.interface"; // Import the check tamper result interface.


// Define the component for the patient dashboard.
@Component({
    selector: "app-external-provider",
    standalone: true,
    imports: [ CommonModule, TopbarComponent, PdfVerifyComponent] ,
    templateUrl: "./external-provider.html",
    styleUrls: ["./external-provider.scss"]
})


// Export the ExternalProviderDashboardComponent class.
export class ExternalProviderDashboardComponent implements OnDestroy{
    // Inject required services.
    private readonly _medicalRecordService = inject(MedicalRecordService);
    private readonly _authService = inject(AuthService);
    private readonly _destroy$ = new Subject<void>();

    // Store provider/user identifier.
    readonly userID = this._authService.CurrentUser!.id;

    // PDF verify state.
    pdfVerifyLoading = false;
    pdfVerifyResult: ITamperResult | null = null;
    pdfVerifyErrorMessage: string | null = null;


    ngOnDestroy(): void {
        this._destroy$.next();
        this._destroy$.complete();
    }


    onPdfVerifyRequested(file: File): void {
        this.pdfVerifyResult = null;
        this.pdfVerifyErrorMessage = null;
        this.pdfVerifyLoading = true;
        
        this._medicalRecordService.verifyPdfTampering(this.userID, file)
            .pipe(takeUntil(this._destroy$))
            .subscribe({
                next: (result) => {
                    this.pdfVerifyResult = result;
                    this.pdfVerifyLoading = false;
                },
                error: (err) => {
                    this.pdfVerifyErrorMessage = this._extractErrorMessage(err);
                    this.pdfVerifyLoading = false;
                }
            });
    }


    // Method to clear the PDF verify section.
    onPdfClear(): void {
        this.pdfVerifyResult = null;
        this.pdfVerifyErrorMessage = null;
    }


    // Helper to extract the error message from an HTTP error response.
    private _extractErrorMessage(err: any): string {
        if (err && typeof err === "object") {
            if (typeof err.error === "string" && err.error.trim()) return err.error.trim();
            if (err.error && typeof err.error.message === "string") return err.error.message.trim();
            if (typeof err.message === "string") return err.message.trim();
        }
        return "An unexpected error occurred during verification.";
    }
}