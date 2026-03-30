// Import dependencies.
import { Component, inject, OnInit, OnDestroy } from "@angular/core"; // Import angular core component functionality.
import { CommonModule } from "@angular/common"; // Import common Angular module for common features.
import { Subject, takeUntil } from "rxjs"; // Import RxJS for managing subscriptions.
import { SidebarComponent } from "../../../shared/components/sidebar/sidebar"; // Import the sidebar component.
import { TopbarComponent } from "../../../shared/components/topbar/topbar"; // Import the topbar component.
import { PdfVerifyComponent } from "../../../shared/components/pdf-verify/pdf-verify"; // Import the PDF verification component.
import { TamperCheckComponent } from "../../../shared/components/tamper-check/tamper.check"; // Import the tamper check component.
import { ViewMedicalRecordComponent } from "../../../shared/components/view-medical-record/view-medical-record"; // Import the view medical record component.
import { ViewAccessLogComponent } from "../../../shared/components/view-access-log/view-access-log"; // Import the view medical record access log component.
import { MedicalRecordService } from "../../../core/services/medical-record/medical-record.service"; // Import the medical record service.
import { AuthService } from "../../../core/services/auth/auth.service"; // Service to get the current logged-in user.
import { IMedicalRecord, IRecordVersion, IRecordViewData } from "../../../shared/interfaces/doctor-history.interface"; // Import the doctor history related interfaces.
import { ITamperResult } from "../../../shared/interfaces/tamper-result.interface"; // Import the check tamper result interface.
import { IPatientHistory } from "../../../shared/interfaces/patient-history.interface"; // Import the patient interface.


// Define the component for the patient dashboard.
@Component({
    selector: "app-patient",
    standalone: true,
    imports: [ CommonModule, SidebarComponent, TopbarComponent,
      PdfVerifyComponent, TamperCheckComponent, ViewMedicalRecordComponent,
      ViewAccessLogComponent ],
    templateUrl: "./patient.html",
    styleUrls: ["./patient.scss"]
})


// Export the PatientDashboardComponent class.
export class PatientDashboardComponent implements OnInit, OnDestroy {

    // Inject required services.
    private readonly _auth = inject(AuthService);
    private readonly _medicalRecordService = inject(MedicalRecordService);
    private readonly _destroy$ = new Subject<void>();
    
    // Store patient identifier.
    readonly patientID = this._auth.CurrentUser!.id;
    patientFullName = "";

    // Navigation state.
    activeLink = "history";
    isCollapsed = false;
    
    // Toggle sidebar visibility.
    toggleSidebar() {
        this.isCollapsed = !this.isCollapsed;
    }
    
    // Handle navigation link change.
    onNavigate(link: string) {
        this.activeLink = link;
    }
    
    // History data.
    history: IPatientHistory | null = null;
    historyLoading = false;
    
    
    // View record modal.
    showViewModal = false;
    viewLoading = false;
    viewedRecord: IRecordViewData | null = null;
    viewedVersion: IRecordVersion | null = null;
    
    
    // Access log modal.
    showAccessLogModal = false;
    accessLogRecord: IMedicalRecord | null = null;
    
    
    // Tamper check modal.
    showTamperModal = false;
    tamperLoading = false;
    tamperResult: ITamperResult | null = null;
    tamperErrorMessage: string | null = null;
    tamperCID = "";
    tamperRecordName = "";
    

    // Download state — tracks which CID is currently downloading.
    downloadingCID: string | null = null;
    
    
    // PDF verify.
    pdfVerifyLoading = false;
    pdfVerifyResult: ITamperResult | null = null;
    pdfVerifyErrorMessage: string | null = null;
    
    
    // LifeCycle.
    ngOnInit(): void {
        this.loadHistory();
    }
    
    
    ngOnDestroy(): void {
        this._destroy$.next();
        this._destroy$.complete();
    }
    
    
    // Data loaders.
    
    // Method to load the patient's full medical history.
    loadHistory(): void {
        this.historyLoading = true;
        this._medicalRecordService.getPatientHistory(this.patientID)
            .pipe(takeUntil(this._destroy$))
            .subscribe({
                next: (h) => {
                    this.history = h;
                    this.patientFullName = h.patientFullName;
                    this.historyLoading = false;
                },
                error: () => {
                    this.historyLoading = false;
                }
            });
    }
    
    
    
    // View version.
    
    // Method to view a specific version of a medical record.
    viewVersion(version: IRecordVersion): void {
        this.viewedVersion = version;
        this.viewedRecord = null;
        this.viewLoading = true;
        this.showViewModal = true; 
        
        this._medicalRecordService.getMedicalRecordFromCID(version.ipfS_CID, this.patientID)
            .pipe(takeUntil(this._destroy$))
            .subscribe({
                next: (data) => {
                    this.viewedRecord = data;
                    this.viewLoading = false;
                },
                error: () => {
                    this.viewLoading = false;
                    this.showViewModal = false;
                }
            });
    }
    
    
    // Method to close the view version modal and reload history.
    closeViewModal(): void {
        this.showViewModal = false;
        this.viewedRecord = null;
        this.viewedVersion = null;
        this.loadHistory();
    }
    
    
    // Access log.
     
    // Method to open the access log modal for a specific record.
    viewAccessLog(record: IMedicalRecord): void {
        this.accessLogRecord = record;
        this.showAccessLogModal = true;
    }
    

    // Method to close the access log modal.
    closeAccessLog(): void {
        this.showAccessLogModal = false;
        this.accessLogRecord = null;
    }
    
    
    // Tamper check.
    
    // Method to open the tamper check modal and call the verify endpoint.
    checkTamper(version: IRecordVersion): void {
        this.tamperCID = version.ipfS_CID;
        this.tamperRecordName = version.displayName;
        this.tamperResult = null;
        this.tamperErrorMessage = null;
        this.tamperLoading = true;
        this.showTamperModal = true;
        
        this._medicalRecordService.checkTamperByCID(version.ipfS_CID, this.patientID)
            .pipe(takeUntil(this._destroy$))
            .subscribe({
                next: (result) => {
                    this.tamperResult = result;
                    this.tamperLoading = false;
                },
                error: (err) => {
                    this.tamperErrorMessage = this._extractErrorMessage(err);
                    this.tamperLoading = false;
                }
            });
    }
    
    
    // Method to close the tamper modal and reload history.
    closeTamperModal(): void {
        this.showTamperModal = false;
        this.tamperResult = null;
        this.tamperErrorMessage = null;
        this.loadHistory();
    }
    
    
    // Download.
     
    // Method to download a medical record PDF.
    downloadRecord(version: IRecordVersion): void {
        if (this.downloadingCID) {
            return;
        }
        
        this.downloadingCID = version.ipfS_CID;
        
        this._medicalRecordService.downloadMedicalRecord(version.ipfS_CID, this.patientID)
            .pipe(takeUntil(this._destroy$))
            .subscribe({
                next: (blob) => {
                    const url = URL.createObjectURL(blob);
                    const anchor = document.createElement("a");
                    anchor.href = url;
                    anchor.download = `${version.displayName}.pdf`;
                    anchor.click();
                    URL.revokeObjectURL(url);
                    this.downloadingCID = null;
                    this.loadHistory();
                },
                error: () => {
                    this.downloadingCID = null;
                }
            });
    }
    
    
    // PDF verify.
    
    // Method to verify PDF tampering.
    onPdfVerifyRequested(file: File): void {
        this.pdfVerifyResult = null;
        this.pdfVerifyErrorMessage = null;
        this.pdfVerifyLoading = true;
 
        this._medicalRecordService.verifyPdfTampering(this.patientID, file)
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
    
    
    // Method to format a timestamp string into a readable locale string.
    formatTimestamp(ts: string): string {
        if (!ts) {
            return "";
        }
        return new Date(ts).toLocaleString();
    }
    

    // Method to extract the error message from an HTTP error response.
    private _extractErrorMessage(err: unknown): string {
    if (err && typeof err === "object") {
        const e = err as any;
        if (typeof e.error === "string" && e.error.trim()) return e.error.trim();
        if (e.error && typeof e.error.message === "string" && e.error.message.trim()) return e.error.message.trim();
        if (typeof e.message === "string" && e.message.trim()) return e.message.trim();
    }
    return "An unexpected error occurred. Please try again.";
}
}