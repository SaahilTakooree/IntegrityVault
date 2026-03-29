// Import dependencies.
import { Component, inject, ViewChild, OnInit, OnDestroy } from "@angular/core"; // Import angular core component functionality.
import { CommonModule } from "@angular/common"; // Import common Angular module for common features.
import { FormsModule } from "@angular/forms"; // Import angular forms module for template-driven forms.
import { Subject, takeUntil } from "rxjs"; // Import RxJS for managing subscriptions.
import { SidebarComponent } from "../../../shared/components/sidebar/sidebar"; // Import the sidebar component.
import { TopbarComponent } from "../../../shared/components/topbar/topbar"; // Import the topbar component.
import { EntityModalComponent } from "../../../shared/components/entity-modal/entity-modal"; // Modal for creating and editing entities.
import { ConfirmModalComponent } from "../../../shared/components/confirm-modal/confirm-modal"; // Import the confirm modal.
import { ConfirmButtonStyle } from "../../../shared/enums/button-style.enum"; // Import confirm modal button enum.
import { MedicalRecordFormComponent } from "../../../shared/components/medical-record-form/medical-record-form"; // Import the medical record form component.
import { PdfVerifyComponent } from "../../../shared/components/pdf-verify/pdf-verify"; // Import the PDF verification component.
import { TamperCheckComponent } from "../../../shared/components/tamper-check/tamper.check"; // Import the tamper check component.
import { ViewMedicalRecordComponent } from "../../../shared/components/view-medical-record/view-medical-record"; // Import the view medical record component.
import { ViewAccessLogComponent } from "../../../shared/components/view-access-log/view-access-log"; // Import the view medical record access log component.
import { MedicalRecordService } from "../../../core/services/medical-record/medical-record.service"; // Import the medical record service.
import { CreateMedicalRecord } from "../../../shared/interfaces/create-medical-record.interface"; // Import the create medical record interface.
import { AuthService } from "../../../core/services/auth/auth.service"; // Service to get the current logged-in user.
import { IDoctorHistory, IEpisode, IMedicalRecord, IRecordVersion, IRecordViewData } from "../../../shared/interfaces/doctor-history.interface"; // Import the doctor history related interfaces.
import { IMedicalRecordForm } from "../../../shared/interfaces/medical-record-form.interface"; // Import the medical record form interface.
import { ITamperResult } from "../../../shared/interfaces/tamper-result.interface"; // Import the rich tamper result interface.
import { parseMedicalRecordApiError } from "../../../shared/utils/medical-record/medical-record-form.validator"; // Import the API error parsing utility.
import { MedicalRecordFormMode } from "../../../shared/types/medical-record-form.type"; // Import the medical record form mode type.
import { IDoctor } from "../../../shared/interfaces/doctor.interface"; // Import the doctor interfaces.
import { IPatient } from "../../../shared/interfaces/patient.interface"; // Import the patient interfaces.


// Define the component for the doctor dashboard.
@Component({
  selector: "app-doctor",
  standalone: true,
  imports: [CommonModule, FormsModule, SidebarComponent, TopbarComponent,
    EntityModalComponent, ConfirmModalComponent, MedicalRecordFormComponent,
    PdfVerifyComponent, TamperCheckComponent, ViewMedicalRecordComponent,
    ViewAccessLogComponent],
  templateUrl: "./doctor.html",
  styleUrls: ["./doctor.scss"]
})


// Export the DoctorDashboardComponent class.
export class DoctorDashboardComponent implements OnInit, OnDestroy {

  // Inject required services.
  private readonly _auth = inject(AuthService);
  private readonly _medicalRecordService = inject(MedicalRecordService);
  private readonly _destroy$ = new Subject<void>();

  // Store doctor and hospital identifiers.
  readonly doctorID = this._auth.CurrentUser!.id;
  readonly hospitalID = this._auth.CurrentUser!.hospitalId ?? 0;
  doctorSpecialty: number = 0;
  doctorFullName = "";

  // Navigation state.
  activeLink = "history";
  isCollapsed = false;
  ConfirmButtonStyle = ConfirmButtonStyle;

  // Toggle sidebar visibility.
  toggleSidebar() { this.isCollapsed = !this.isCollapsed; }
  // Handle navigation link change.
  onNavigate(link: string) { this.activeLink = link; }

  // History data.
  history: IDoctorHistory | null = null;
  historyLoading = false;

  // Patient list for the new episode selector.
  patients: { id: number; fullName: string }[] = [];

  // Modal state.
  showRecordModal = false;
  modalMode: MedicalRecordFormMode = "newEpisode";
  modalLoading = false;

  // Context passed into the form heading.
  modalContextLabel = "";
  modalTitle = "";
  modalConfirmLabel = "";

  // Track which episode or record we are operating on.
  activeEpisodeID: number | null = null;
  activeMedicalRecordID: number | null = null;
  activePatientID: number | null = null;
  activeVisitDate = "";
  activeChiefComplaint = "";

  // Initial form value state.
  initialFormValue: IMedicalRecordForm | null = null;

  // Reference to medical record form component.
  @ViewChild(MedicalRecordFormComponent) recordFormRef!: MedicalRecordFormComponent;

  // View record modal.
  showViewModal = false;
  viewLoading = false;
  viewedRecord: IRecordViewData | null = null;
  viewedVersion: IRecordVersion | null = null;

  // Access log modal.
  showAccessLogModal = false;
  accessLogRecord: IMedicalRecord | null = null;

  // Tamper check.
  showTamperModal = false;
  tamperLoading = false;
  tamperResult: ITamperResult | null = null;
  tamperErrorMessage: string | null = null;
  tamperCID = "";
  tamperRecordName = "";

  // Episode toggle confirm.
  showStatusModal = false;
  pendingStatusEpisodeID: number | null = null;
  pendingStatusIsActive: boolean = true;

  // PDF verify.
  pdfVerifyLoading = false;
  pdfVerifyResult: ITamperResult | null = null;
  pdfVerifyErrorMessage: string | null = null;

  // LifeCycle.
  ngOnInit(): void {
    this._loadDoctor();
    this._loadPatients();
    this.loadHistory();
  }

  ngOnDestroy(): void {
    this._destroy$.next();
    this._destroy$.complete();
  }

  // Data loaders.

  // Method to get the doctor information.
  private _loadDoctor(): void {
    this._medicalRecordService.getDoctorById(this.doctorID)
      .pipe(takeUntil(this._destroy$))
      .subscribe({
        next: (doc: IDoctor) => {
          this.doctorSpecialty = doc.specialty ?? 0;
          this.doctorFullName = `${doc.firstName ?? ""} ${doc.lastName ?? ""}`.trim();
        }
      });
  }

  // Method to load all the patients for the hospital.
  private _loadPatients(): void {
    if (!this.hospitalID) return;
    this._medicalRecordService.getPatientsForHospital(this.hospitalID)
      .pipe(takeUntil(this._destroy$))
      .subscribe({
        next: (patients: IPatient[]) => {
          this.patients = patients.map(p => ({
            id: p.id,
            fullName: `${p.firstName} ${p.lastName} (ID: ${p.id})`.trim()
          }));
        }
      });
  }

  // Method to load all the records that the doctor had access to.
  loadHistory(): void {
    this.historyLoading = true;
    this._medicalRecordService.getDoctorHistory(this.doctorID)
      .pipe(takeUntil(this._destroy$))
      .subscribe({
        next: (h) => { this.history = h; this.historyLoading = false; },
        error: () => { this.historyLoading = false; }
      });
  }

  // Modal openers.

  // Method to open the modal to create a new episode.
  openNewEpisodeModal(): void {
    this.modalMode = "newEpisode";
    this.modalTitle = "New Episode & Medical Record";
    this.modalConfirmLabel = "Create";
    this.modalContextLabel = "";
    this.activeEpisodeID = null;
    this.activeMedicalRecordID = null;
    this.activePatientID = null;
    this.activeVisitDate = new Date().toISOString().split("T")[0];
    this.initialFormValue = null;
    this.showRecordModal = true;
  }

  // Method to open the modal to add a medical record to an episode.
  openAddRecordModal(episode: IEpisode, patientID: number): void {
    this.modalMode = "addRecord";
    this.modalTitle = "Add Medical Record";
    this.modalConfirmLabel = "Add Record";
    this.modalContextLabel = `${episode.chiefComplaint} — Addition`;
    this.activeEpisodeID = episode.episodeID;
    this.activeMedicalRecordID = null;
    this.activePatientID = patientID;
    this.activeChiefComplaint = episode.chiefComplaint;
    this.activeVisitDate = new Date().toISOString().split("T")[0];
    this.initialFormValue = null;
    this.showRecordModal = true;
  }

  // Method to open the modal to update a medical record.
  openUpdateRecordModal(episode: IEpisode, record: IMedicalRecord, patientID: number): void {
    const latestCID = record.versions[0]?.ipfS_CID;
    if (!latestCID) return;

    this.modalMode = "updateRecord";
    this.modalTitle = "Update Medical Record";
    this.modalConfirmLabel = "Save Changes";
    this.modalContextLabel = `${episode.chiefComplaint} — Update`;
    this.activeEpisodeID = episode.episodeID;
    this.activeMedicalRecordID = record.medicalRecordID;
    this.activePatientID = patientID;
    this.activeChiefComplaint = episode.chiefComplaint;
    this.activeVisitDate = record.visitDate;
    this.modalLoading = true;
    this.showRecordModal = true;
    this.initialFormValue = null;

    this._medicalRecordService.getMedicalRecordFromCID(latestCID, this.doctorID)
      .pipe(takeUntil(this._destroy$))
      .subscribe({
        next: (data) => {
          this.initialFormValue = {
            patientID: null,
            visitDate: record.visitDate,
            chiefComplaint: episode.chiefComplaint,
            diagnosis: data.diagnosis,
            treatmentPlan: data.treatmentPlan,
            doctorNotes: data.doctorNotes ?? "",
            followUpInstructions: data.followUpInstructions ?? ""
          };
          this.modalLoading = false;
        },
        error: () => { this.modalLoading = false; }
      });
  }

  // Method to close the medical record modal.
  closeRecordModal(): void {
    this.showRecordModal = false;
    this.initialFormValue = null;
    this.modalLoading = false;
    if (this.modalMode === "updateRecord" || this.modalMode === "addRecord") {
      this.loadHistory();
    }
  }

  // Record modal confirm.
  onRecordConfirmed(): void {
    if (!this.recordFormRef.validate()) return;

    const form = this.recordFormRef.getValue();
    const dto: CreateMedicalRecord = {
      doctorID: this.doctorID,
      patientID: this.modalMode === "newEpisode"
        ? (form.patientID ?? 0)
        : (this.activePatientID ?? 0),
      specialty: this.doctorSpecialty,
      visitDate: form.visitDate,
      chiefComplaint: (form.chiefComplaint || this.activeChiefComplaint).trim(),
      diagnosis: form.diagnosis.trim(),
      treatmentPlan: form.treatmentPlan.trim(),
      doctorNotes: form.doctorNotes?.trim() || "",
      followUpInstructions: form.followUpInstructions?.trim() || ""
    };

    this.modalLoading = true;

    if (this.modalMode === "newEpisode") {
      this._medicalRecordService.createNewMedicalRecordAndEpisode(dto)
        .pipe(takeUntil(this._destroy$))
        .subscribe({
          next: () => { this._onRecordSuccess(); },
          error: (err) => { this._onRecordError(err); }
        });
    } else if (this.modalMode === "addRecord" && this.activeEpisodeID) {
      this._medicalRecordService.addMedicalRecordToEpisode(this.activeEpisodeID, dto)
        .pipe(takeUntil(this._destroy$))
        .subscribe({
          next: () => { this._onRecordSuccess(); },
          error: (err) => { this._onRecordError(err); }
        });
    } else if (this.modalMode === "updateRecord" && this.activeMedicalRecordID && this.activeEpisodeID) {
      this._medicalRecordService.updateMedicalRecord(this.activeMedicalRecordID, this.activeEpisodeID, dto)
        .pipe(takeUntil(this._destroy$))
        .subscribe({
          next: () => { this._onRecordSuccess(); },
          error: (err) => { this._onRecordError(err); }
        });
    }
  }

  // Method to update the UI if the record operation was successful.
  private _onRecordSuccess(): void {
    this.modalLoading = false;
    this.showRecordModal = false;
    this.initialFormValue = null;
    this.loadHistory();
  }

  // Method to add an error to the form if the operation failed.
  private _onRecordError(err: unknown): void {
    this.modalLoading = false;
    this.recordFormRef?.setApiError(parseMedicalRecordApiError(err));
  }

  // View version.

  // Method to view a specific version of a medical record.
  viewVersion(version: IRecordVersion): void {
    this.viewedVersion = version;
    this.viewedRecord = null;
    this.viewLoading = true;
    this.showViewModal = true;

    this._medicalRecordService.getMedicalRecordFromCID(version.ipfS_CID, this.doctorID)
      .pipe(takeUntil(this._destroy$))
      .subscribe({
        next: (data) => { this.viewedRecord = data; this.viewLoading = false; },
        error: () => { this.viewLoading = false; }
      });
  }

  // Method to close the view version modal.
  closeViewModal(): void {
    this.showViewModal = false;
    this.viewedRecord = null;
    this.viewedVersion = null;
    this.loadHistory();
  }

  // Access log.
  viewAccessLog(record: IMedicalRecord): void {
    this.accessLogRecord = record;
    this.showAccessLogModal = true;
  }

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

    this._medicalRecordService.checkTamperByCID(version.ipfS_CID, this.doctorID)
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

  // Method to close the tamper modal and refresh history (Verify log was written).
  closeTamperModal(): void {
    this.showTamperModal = false;
    this.tamperResult = null;
    this.tamperErrorMessage = null;
    this.loadHistory();
  }

  // Episode status toggle.
  openStatusConfirm(episode: IEpisode): void {
    this.pendingStatusEpisodeID = episode.episodeID;
    this.pendingStatusIsActive = episode.isActive;
    this.showStatusModal = true;
  }

  // Method to toggle an episode active/inactive.
  handleStatusResult(confirmed: boolean): void {
    this.showStatusModal = false;
    if (!confirmed || !this.pendingStatusEpisodeID) return;

    this._medicalRecordService.setEpisodeStatus(this.pendingStatusEpisodeID, this.doctorID)
      .pipe(takeUntil(this._destroy$))
      .subscribe({ next: () => this.loadHistory() });
  }


  // PDF verify.
  onPdfVerifyRequested(file: File): void {
    this.pdfVerifyResult = null;
    this.pdfVerifyErrorMessage = null;
    this.pdfVerifyLoading = true;

    this._medicalRecordService.verifyPdfTampering(this.doctorID, file)
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


  // Method to check if an episode can be edited.
  episodeCanEdit(episode: IEpisode): boolean {
    return episode.isActive;
  }


  // Method to format a timestamp string into a readable locale string.
  formatTimestamp(ts: string): string {
    if (!ts) {
      return "";
    }
    return new Date(ts).toLocaleString();
  }


  // Method to clear the pdf section.
  onPdfClear(): void {
    this.pdfVerifyResult = null;
    this.pdfVerifyErrorMessage = null;
  }


  // Method to extract the error message.
  private _extractErrorMessage(err: unknown): string {
    if (err && typeof err === "object") {
      const e = err as any;
      if (typeof e.error === "string" && e.error.trim()) {
        return e.error.trim();
      }
      if (typeof e.message === "string" && e.message.trim()) {
        return e.message.trim();
      }
    }
    return "An unexpected error occurred. Please try again.";
  }
}