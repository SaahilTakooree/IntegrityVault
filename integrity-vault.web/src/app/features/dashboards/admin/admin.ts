// Import dependencies.
import { Component, inject, ViewChild } from "@angular/core"; // Import angular core component functionality
import { CommonModule } from "@angular/common"; // Import common Angular module for common features.
import { FormsModule } from "@angular/forms"; // Import angular forms module for template-driven forms.
import { Subject, takeUntil } from "rxjs"; // Import RxJS for managing subscriptions and reactive programming.
import { SidebarComponent } from "../../../shared/components/sidebar/sidebar"; // Import the side bar component.
import { TopbarComponent } from "../../../shared/components/topbar/topbar"; // Import the topbar component.
import { ConfirmModalComponent } from "../../../shared/components/confirm-modal/confirm-modal"; // Import the confirm modal.
import { EntitySectionComponent } from "../../../shared/components/entity-section/entity-section"; // Section for entity data.
import { EntityModalComponent } from "../../../shared/components/entity-modal/entity-modal"; // Modal for creating and editing entities.
import { UserFormComponent } from "../../../shared/components/user-form/user-form"; // Import the user form.
import { HospitalFormComponent } from "../../../shared/components/hospital-form/hospital-form"; // Import the hospital form.
import { UserRole } from "../../../shared/enums/user-role.enum"; // Import user role enum.
import { ConfirmButtonStyle } from "../../../shared/enums/button-style.enum"; // Import confirm modal button enum.
import { DoctorSpecialty } from "../../../shared/enums/doctor-specialty.enum";
import { PatientGender } from "../../../shared/enums/patient-gender.enum"; // Import doctor enum.
import { IColumnDefinition } from "../../../shared/interfaces/column-definition.interface"; // Import patient enum.
import { IUserForm } from "../../../shared/interfaces/user-form.interface"; // User interface for type-checking.
import { IHospital } from "../../../shared/interfaces/hospital.interface"; // Hospital interface for type-checking.
import { IAdmin } from "../../../shared/interfaces/admin.interface"; // Admin interface for type-checking.
import { IDoctor } from "../../../shared/interfaces/doctor.interface"; // Doctor interface for type-checking.
import { IPatient } from "../../../shared/interfaces/patient.interface"; // Patient interface for type-checking.
import { IExternalProvider } from "../../../shared/interfaces/external-provider.interface"; // External provider interface for type-checking.
import { HospitalService } from "../../../core/services/hospital.service"; // Service to interact with hospital API.
import { UserService } from "../../../core/services/user.service"; // Service to interact with user API.
import { AuthService } from "../../../core/services/auth.service"; // Service to interact with auth API.
import { parseHospitalApiError } from "../../../shared/utils/hospital-form.validator"; // Validation and error handling utils for hospital.
import { parseUserApiError } from "../../../shared/utils/user-form.validator"; // Validation and error handling utils for user.


// Define the component for the superadmin dashboard.
@Component({
  selector: "app-admin-dashboard",
  standalone: true,
  imports: [CommonModule, FormsModule, SidebarComponent, TopbarComponent, ConfirmModalComponent, EntitySectionComponent, EntityModalComponent, UserFormComponent, HospitalFormComponent ],
  templateUrl: "./admin.html",
  styleUrls: ["./admin.scss"]
})


// Export the SuperadminDashboardComponent class.
export class AdminDashboardComponent {
    // Reference to the HospitalFormComponent to access its methods and properties.
    @ViewChild(HospitalFormComponent) hospitalFormRef! : HospitalFormComponent;
    @ViewChild("adminForm") adminFormRef! : UserFormComponent;
    @ViewChild("patientForm") patientFormRef! : UserFormComponent;
    @ViewChild("doctorForm") doctorFormRef! : UserFormComponent;
    @ViewChild("externalProviderForm") externalProviderFormRef! : UserFormComponent;
    

    // Set the hopsital id that the admin belongs to.
    adminHospital: IHospital | null = null; // Store the hospital detail of the admin.


    // Navigation state. 
    activeLink = "patients"; // To know which 
    isCollapsed: boolean = false; // When in small screen mode.. to know if the sidebar is close or not.
    UserRole = UserRole; // Enum for user role.
    ConfirmButtonStyle = ConfirmButtonStyle; // Enum for confirm button style.
    
    // Sidebar collapsed state and toggler function.
    toggleSidebar() {
        this.isCollapsed = !this.isCollapsed;
    }

    // Method to handle navigation between different tab. 
    onNavigate(link : string) { 
        this.activeLink = link;
    }


    // Data.
    hospital : IHospital | null = null; // Define hospital to store the hopsital information that the admin belongs too.
    hospitals : IHospital[] | null = null; // Define all the hospitals.
    admins : IAdmin[] = []; // Define doctors to store all the doctors
    doctors : IDoctor[] = []; // Define doctors to store all the doctors
    patients : IPatient[] = []; // Define patients to store all the patients
    externalProviders : IExternalProvider[] = []; // Define external providers to store all the external providers

    // Computed property to check if there are any doctors.
    get noDoctors() {
      return this.doctors.length === 0;
    }

    // Get the hospital id of the user currently.
    get adminHospitalID(): number {
      return this._authService.CurrentUser?.hospitalId ?? 0;
    }

    // Get the current user id of the person login.
    get currentUserId(): number | null {
      console.log(this._authService.CurrentUser?.id)
      return this._authService.CurrentUser?.id ?? null;
    }


    // Column definitions for admin table.
    adminColumns : IColumnDefinition<IAdmin>[] = [
      { key : "id", label : "ID", mono : true },
      { key : "username", label : "Username", mono : false },
      { key : "email", label : "Eamil", mono : false },
      { key : "joinDate", label : "Joined", mono : false }
    ]


    // Column definitions for doctor table.
    doctorColumns : IColumnDefinition<IDoctor>[] = [
        { key : "id", label : "ID", mono : true },
        { key : "username", label : "Username", mono : false },
        { key : "email", label : "Eamil", mono : false },
        { key : "joinDate", label : "Joined", mono : false },
        { key : "firstName", label : "First Name", mono : false },
        { key : "middleName", label : "Middle Name", mono : false, transform: (val: string) => val?.trim() ? val : "—" },
        { key : "lastName", label : "Last Name", mono : false },
        { key : "specialty", label : "Speicialty", mono : false, transform: (val: DoctorSpecialty) => DoctorSpecialty[val] ?? String(val) }
    ]


    // Column definitions for patient table.
    patientColumns : IColumnDefinition<IPatient>[] = [
        { key : "id", label : "ID", mono : true },
        { key : "username", label : "Username", mono : false },
        { key : "email", label : "Eamil", mono : false },
        { key : "joinDate", label : "Joined", mono : false },
        { key : "firstName", label : "Fist Name", mono : false },
        { key : "middleName", label : "Middle Name", mono : false, transform: (val: string) => val?.trim() ? val : "—" },
        { key : "lastName", label : "Last Name", mono : false },
        { key : "dob", label : "Date of Birth", mono : false },
        { key : "gender", label : "Gender", mono : false, transform: (val: PatientGender) => PatientGender[val] ?? String(val) }
    ]


    // Column definitions for external provider table.
    externalProviderColumns : IColumnDefinition<IExternalProvider>[] = [
        { key : "id", label : "ID", mono : true },
        { key : "username", label : "Username", mono : false },
        { key : "email", label : "Eamil", mono : false },
        { key : "joinDate", label : "Joined", mono : false },
        { key: "belongsToID",  label: "Belongs To",     mono: true },
    ]


    // Column definitions for hospital table.
    hospitalColumns : IColumnDefinition<IHospital>[] = [
        { key : "id", label : "ID", mono : true },
        { key : "name", label : "Name", mono : false },
        { key : "walletAddress", label : "Wallet Address", mono : true, truncate : true },
        { key : "ipAddresses", label : "IP Addresses", mono: true }
    ]


    // Admin form state.
    adminInitialValue : IUserForm | null | undefined = undefined  // Value to pass to the user form.
    editingAdmin : IAdmin | null = null; // Tracks whether editing of an existing admin is taking place.


    // Doctor form state.
    doctorInitialValue : IUserForm | null | undefined = undefined  // Value to pass to the user form.
    editingDoctor : IDoctor | null = null; // Tracks whether editing of an existing doctor is taking place.
    

    // Patient form state.
    patientInitialValue : IUserForm | null | undefined = undefined  // Value to pass to the user form.
    editingPatient : IPatient | null = null; // Tracks whether editing of an existing patient is taking place.
    

    // external provider form state.
    externalProviderInitialValue : IUserForm | null | undefined = undefined  // Value to pass to the user form.
    editingExternalProvider : IExternalProvider | null = null; // Tracks whether editing of an existing external provider is taking place.


    // Hopsital form state.
    hospitalInitialValue : IHospital | null = null; // Value to pass to the hospital form.
    editingHospital : IHospital | null = null; // Tracks whether editing of an exisitng hospital is taking place.


    // Modal state.
    showHospitalModal = false; // State to know whether to show the hospital modal or not.
    showAdminModal = false; // State to know whether to show the admin modal or not.
    showPatientModal = false; // State to know whether to show the patient modal or not.
    showDoctorModal = false; // State to know whether to show the doctor modal or not.
    showExternalProviderModal = false; // State to know whether to show the ExternalProvider modal or not.
    showDeleteModal = false; // State to know whether to show the delete modal or not.
    deleteTargetType: "externalProvider" | "patient" | "doctor" | "admin" = "patient"; // Tracks which entity type is pending deletion.
    deleteTarget = ""; // Keep track of which row is being deleted.

    //The id is used when the delete modal is confirmed.
    private _pendingDeleteId : number = 0;


    // Service and teardown.
    private _destroy$ = new Subject<void>(); // Teardown signal for all subscriptions.
    private _hospitalService = inject(HospitalService) // Inject the user service to interact with the backend.
    private _userService = inject(UserService) // Inject the user service to interact with the backend.
    private _authService = inject(AuthService) // Inject the auth to get the detail of the current login user.
    

    // LifeCycle.

    // Initialise data and subscribe to hospital data.
    ngOnInit() { 
        this.fetchHospital();
        this.fetchHospitalByID();
        this.fetchUser();
    }


    // Clean up any ongoing subscriptions on component destruction
    ngOnDestroy(): void {
        this._destroy$.next();
        this._destroy$.complete();
    }


    // Hospital API.

    // Fetch the list of hospitals from the service.
    fetchHospital() {
        // Subscribe to the hospital data and assign the result to the hospital variable.
        this._hospitalService.getHospital()
        .pipe(takeUntil(this._destroy$)) // Unsubscribe automatically when the component is destroyed.
        .subscribe({
            next: (hospitals: IHospital[]) => {
            this.hospitals = hospitals; // Assign the data to the hospital variable.
            },
            error: (err) => console.error("Error fetching hospitals:", err),
        });
    }

    // Fetch the list of hospitals from the service.
    fetchHospitalByID() {
        // Subscribe to the hospital data and assign the result to the hospital variable.
        this._hospitalService.getHospitalById(this.adminHospitalID)
        .pipe(takeUntil(this._destroy$)) // Unsubscribe automatically when the component is destroyed.
        .subscribe({
            next: (hospital: IHospital) => {
            this.hospital = hospital; // Assign the data to the hospital variable.
            },
            error: (err) => console.error("Error fetching hospitals:", err),
        });
    }


    // User API.

    // Fetch the list of user from the service.
    fetchUser() {
        // Subscribe to the admin data and assign the result to the user array.
        this._userService.getAllUsers(this.adminHospitalID)
        .pipe(takeUntil(this._destroy$)) // Unsubscribe automatically when the component is destroyed.
        .subscribe({
            next: (users) => {
            this.admins = users.filter(u => u.role === UserRole.Admin) as IAdmin[]; // Filter only Admin role users and cast them to IAdmin.
            this.doctors = users.filter(u => u.role === UserRole.Doctor) as IDoctor[]; // Filter only Doctor role users and cast them to IDoctor.
            this.patients = users.filter(u => u.role === UserRole.Patient) as IPatient[]; // Filter only Patient role users and cast them to IPatient.
            this.externalProviders = users.filter(u => u.role === UserRole.ExternalProvider) as IExternalProvider[]; // Filter only ExternalProvider role users and cast them to IExternalProvider.
            },
            error: (err) => console.error("Error fetching user:", err),
        });
    }


    // Modal openers.

    // Method to open the hospital modal.
    openHospitalModal(item? : IHospital) {
        this.editingHospital = item ?? null;
        this.hospitalInitialValue = item ?? null;
        this.showHospitalModal = true;
        if (this.isCollapsed)
        this.toggleSidebar()
    }


    // Method to open the admin modal.
    openAdminModal(item? : IAdmin) {
        this.editingAdmin = item ?? null;
        this.adminInitialValue = undefined;
        setTimeout(() => {
        this.adminInitialValue = item
            ? {
            username: item.username,
            email: item.email,
            password: "",
            hospitalID: item.hospitalID ?? null,
            belongsToID: null,
            role: UserRole.Admin,
            firstName: "",
            middleName: "",
            lastName: "",
            specialty: null,
            dob: "",
            gender: null
            }
            : null;

        this.showAdminModal = true;
        if (this.isCollapsed)
            this.toggleSidebar();
        });
    }


    // Method to open the doctor modal.
    openDoctorModal(item? : IDoctor) {
        this.editingDoctor = item ?? null;
        this.doctorInitialValue = undefined;
        setTimeout(() => {
        this.doctorInitialValue = item
            ? {
            username: item.username,
            email: item.email,
            password: "",
            hospitalID: item.hospitalID ?? null,
            belongsToID: null,
            role: UserRole.Doctor,
            firstName: item.firstName,
            middleName: item.middleName ?? "",
            lastName: item.lastName,
            specialty: item.specialty,
            dob: "",
            gender: null
            }
            : null;

        this.showDoctorModal = true;
        if (this.isCollapsed)
            this.toggleSidebar();
        });
    }


    // Method to open the patient modal.
    openPatientModal(item? : IPatient) {
        this.editingPatient = item ?? null;
        this.patientInitialValue = undefined;
        setTimeout(() => {
        this.patientInitialValue = item
            ? {
            username: item.username,
            email: item.email,
            password: "",
            hospitalID: item.hospitalID ?? null,
            belongsToID: null,
            role: UserRole.Patient,
            firstName: item.firstName,
            middleName: item.middleName ?? "",
            lastName: item.lastName,
            specialty: null,
            dob: item.dob,
            gender: item.gender
            }
            : null;

        this.showPatientModal = true;
        if (this.isCollapsed)
            this.toggleSidebar();
        });
    }


    // Method to open the external provider modal.
    openExternalProviderModal(item? : IExternalProvider) {
        this.editingExternalProvider = item ?? null;
        this.externalProviderInitialValue = undefined;
        setTimeout(() => {
        this.externalProviderInitialValue = item
            ? {
            username: item.username,
            email: item.email,
            password: "",
            hospitalID: item.hospitalID ?? null,
            belongsToID: item.belongsToID ?? null,
            role: UserRole.ExternalProvider,
            firstName: "",
            middleName: "",
            lastName: "",
            specialty: null,
            dob: "",
            gender: null
            }
            : null;

        this.showExternalProviderModal = true;
        if (this.isCollapsed)
            this.toggleSidebar();
        });
    }


    // Method to open the delete modal.
    openDeleteModal(id : number, name : string, type : "externalProvider" | "patient" | "doctor" | "admin") {
        this._pendingDeleteId = id
        this.deleteTarget = name;
        this.deleteTargetType = type;
        this.showDeleteModal = true;
        if (this.isCollapsed)
        this.toggleSidebar()
    }


    // Method to close the all modal.
    closeModals() {
        this.showHospitalModal=false;
        this.showAdminModal=false;
        this.showDoctorModal=false;
        this.showPatientModal=false;
        this.showExternalProviderModal=false;
        this.showDeleteModal=false;
    }


    // Entity section row actions.

    // Method to edit an admin row.
    onEditAdmin(row : IAdmin) {
      if (this.isCollapsed)
        this.toggleSidebar()
      this.openAdminModal(row);
    }

    // Method to delete an admin row.
    onDeleteAdmin( row : IAdmin) {
      if (this.isCollapsed)
        this.toggleSidebar()
      this.openDeleteModal(row.id, row.username, "admin");
    }

    // Method to edit an doctor row.
    onEditDoctor(row : IDoctor) {
      if (this.isCollapsed)
        this.toggleSidebar()
      this.openDoctorModal(row);
    }

    // Method to delete an doctor row.
    onDeleteDoctor( row : IDoctor) {
      if (this.isCollapsed)
        this.toggleSidebar()
      this.openDeleteModal(row.id, row.username, "doctor");
    }

    // Method to edit an patient row.
    onEditPatient(row : IPatient) {
      if (this.isCollapsed)
        this.toggleSidebar()
      this.openPatientModal(row);
    }

    // Method to delete an patient row.
    onDeletePatient( row : IPatient) {
      if (this.isCollapsed)
        this.toggleSidebar()
      this.openDeleteModal(row.id, row.username, "patient");
    }

    // Method to edit an external provider row.
    onEditExternalProvider(row : IExternalProvider) {
      if (this.isCollapsed)
        this.toggleSidebar()
      this.openExternalProviderModal(row);
    }

    // Method to delete an external provider row.
    onDeleteExternalProvider( row : IExternalProvider) {
      if (this.isCollapsed)
        this.toggleSidebar()
      this.openDeleteModal(row.id, row.username, "externalProvider");
    }
  
  
    // Hospital form confirmation.
  
    // Method to hospital handle the edit or add a new hospital.
    onHospitalConfirmed() {
      // Check if the form has any errors. if it does stop the confirmation.
      if (!this.hospitalFormRef.validate())
        return
  
      // Get the value from the hospital form.
      const formValue = this.hospitalFormRef.getValue();
  
      // Trim all the values (name, walletAddress, and ipAddresses) to remove leading/trailing spaces.
      formValue.name = formValue.name.trim();
      formValue.walletAddress = formValue.walletAddress.trim();
      formValue.ipAddresses = formValue.ipAddresses.map(ip => ip.trim()); // Trim each IP address.
  
      // If editing an existing hospital, update the hospital details.
      if (this.editingHospital) {
        // If the walletAddress or ip address hasn"t changed, exclude it from the update payload.
        const walletChanged = formValue.walletAddress !== this.editingHospital.walletAddress;
        const { walletAddress, ipAddresses, ...base } = formValue;
        const hospitalData = walletChanged ? formValue : { ...base, ipAddresses } as IHospital;
  
        this._hospitalService.updateHospital(hospitalData)
          .pipe(takeUntil(this._destroy$))
          .subscribe({
            next: () => {
              this.fetchHospitalByID(); // Refresh the hospital list.
              this.closeModals(); // Close the modal after success.
            },
            error: (err: unknown) => {
              this.hospitalFormRef.setApiError(parseHospitalApiError(err));
            }
        });
      }
    }


    // Admin form confirmation.

    // Admin form confirmation – handles both create and update.
    onAdminConfirmed() : void {
        // Stop if form is invalid.
        if (!this.adminFormRef.validate())
          return;
    
        const formValue = this.adminFormRef.getValue();
    
        // Build the admin payload from the form value.
        const payload : IAdmin = {
            id: this.editingAdmin?.id ?? 0,
            username: formValue.username.trim(),
            email: formValue.email.trim(),
            password: formValue.password,
            role: UserRole.Admin,
            joinDate: this.editingAdmin?.joinDate ?? new Date(),
            hospitalID: formValue.hospitalID ?? this.adminHospitalID
        };
    
        if (this.editingAdmin) {
          const updatePayload: Partial<IAdmin> & { id: number } = {
            id: payload.id,
            hospitalID: payload.hospitalID,
          };
    
          // Only include username if it changed.
          if (payload.username !== this.editingAdmin.username) {
            updatePayload.username = payload.username;
          }
    
          // Only include email if it changed.
          if (payload.email !== this.editingAdmin.email) {
            updatePayload.email = payload.email;
          }
    
          // Only include password if user chose to change it
          if (!this.adminFormRef.isPasswordSkipped()) {
            updatePayload.password = payload.password;
          }
    
          this._userService.updateAdmin(payload.id, updatePayload as IAdmin)
            .pipe(takeUntil(this._destroy$))
            .subscribe({
              next: () => {
                this.fetchUser();
                this.closeModals();
              },
              error: (err: unknown) => {
                this.adminFormRef.setApiError(parseUserApiError(err));
              }
            });
        } else {
          // Create new admin.
          this._userService.createAdmin(payload)
            .pipe(takeUntil(this._destroy$))
            .subscribe({
              next: () => {
                this.fetchUser();
                this.closeModals();
            },
              error: (err: unknown) => { this.adminFormRef.setApiError(parseUserApiError(err)); }
            });
        }
    }


    // Doctor form confirmation.
    
    // Doctor form confirmation – handles both create and update.
    onDoctorConfirmed() : void {
      // Stop if form is invalid.
      if (!this.doctorFormRef.validate())
        return;
        

      const formValue = this.doctorFormRef.getValue();
  
      // Build the Idoctor payload from the form value.
      const payload : IDoctor = {
          id: this.editingDoctor?.id ?? 0,
          username: formValue.username.trim(),
          email: formValue.email.trim(),
          password: formValue.password,
          role: UserRole.Doctor,
          joinDate: this.editingDoctor?.joinDate ?? new Date(),
          hospitalID: formValue.hospitalID ?? this.adminHospitalID,
          firstName: formValue.firstName,
          middleName: formValue.middleName,
          lastName: formValue.lastName,
          specialty: formValue.specialty as DoctorSpecialty
      };
  
      if (this.editingDoctor) {
        const updatePayload: Partial<IDoctor> & { id: number } = {
          id: payload.id,
          hospitalID: payload.hospitalID,
          firstName: payload.firstName,
          middleName: payload.middleName,
          lastName: payload.lastName,
          specialty: payload.specialty 
        };
  
        // Only include username if it changed.
        if (payload.username !== this.editingDoctor.username) {
          updatePayload.username = payload.username;
        }
  
        // Only include email if it changed.
        if (payload.email !== this.editingDoctor.email) {
          updatePayload.email = payload.email;
        }
  
        // Only include password if user chose to change it
        if (!this.doctorFormRef.isPasswordSkipped()) {
          updatePayload.password = payload.password;
        }
  
        this._userService.updateDoctor(payload.id, updatePayload as IDoctor)
          .pipe(takeUntil(this._destroy$))
          .subscribe({
            next: () => {
              this.fetchUser();
              this.closeModals();
            },
            error: (err: unknown) => {
              this.doctorFormRef.setApiError(parseUserApiError(err));
            }
          });
      } else {
        // Create new doctor.
        this._userService.createDoctor(payload)
          .pipe(takeUntil(this._destroy$))
          .subscribe({
            next: () => {
              this.fetchUser();
              this.closeModals();
          },
            error: (err) => {
              this.doctorFormRef.setApiError(parseUserApiError(err));
              alert(err.error);
              
            }
          });
      }
    }


    // Patient form confirmation.
    
    // Patient form confirmation – handles both create and update.
    onPatientConfirmed() : void {
        // Stop if form is invalid.
        if (!this.patientFormRef.validate())
          return;
    
        const formValue = this.patientFormRef.getValue();
    
        // Build the Ipatient payload from the form value.
        const payload : IPatient = {
            id: this.editingPatient?.id ?? 0,
            username: formValue.username.trim(),
            email: formValue.email.trim(),
            password: formValue.password,
            role: UserRole.Patient,
            joinDate: this.editingPatient?.joinDate ?? new Date(),
            hospitalID: formValue.hospitalID ?? this.adminHospitalID,
            firstName: formValue.firstName,
            middleName: formValue.middleName,
            lastName: formValue.lastName,
            dob: formValue.dob,
            gender: formValue.gender as PatientGender
        };
    
        if (this.editingPatient) {
          const updatePayload: Partial<IPatient> & { id: number } = {
            id: payload.id,
            hospitalID: payload.hospitalID,
            firstName: payload.firstName,
            middleName: payload.middleName,
            lastName: payload.lastName,
            dob: payload.dob,
            gender: payload.gender
          };
    
          // Only include username if it changed.
          if (payload.username !== this.editingPatient.username) {
            updatePayload.username = payload.username;
          }
    
          // Only include email if it changed.
          if (payload.email !== this.editingPatient.email) {
            updatePayload.email = payload.email;
          }
    
          // Only include password if user chose to change it
          if (!this.patientFormRef.isPasswordSkipped()) {
            updatePayload.password = payload.password;
          }
    
          this._userService.updatePatient(payload.id, updatePayload as IPatient)
            .pipe(takeUntil(this._destroy$))
            .subscribe({
              next: () => {
                this.fetchUser();
                this.closeModals();
              },
              error: (err: unknown) => {
                this.patientFormRef.setApiError(parseUserApiError(err));
              }
            });
        } else {
          // Create new patient.
          this._userService.createPatient(payload)
            .pipe(takeUntil(this._destroy$))
            .subscribe({
              next: () => {
                this.fetchUser();
                this.closeModals();
            },
              error: (err: unknown) => { this.patientFormRef.setApiError(parseUserApiError(err)); }
            });
        }
    }


    // External provider form confirmation.
    
    // External provider form confirmation – handles both create and update.
    onExternalProviderConfirmed() : void {
        // Stop if form is invalid.
        if (!this.externalProviderFormRef.validate())
          return;
    
        const formValue = this.externalProviderFormRef.getValue();
    
        // Build the Iexternal provider payload from the form value.
        const payload : IExternalProvider = {
            id: this.editingExternalProvider?.id ?? 0,
            username: formValue.username.trim(),
            email: formValue.email.trim(),
            password: formValue.password,
            role: UserRole.ExternalProvider,
            joinDate: this.editingExternalProvider?.joinDate ?? new Date(),
            hospitalID: formValue.hospitalID ?? this.adminHospitalID,
            belongsToID: formValue.belongsToID!
        };
    
        if (this.editingExternalProvider) {
          const updatePayload: Partial<IExternalProvider> & { id: number } = {
            id: payload.id,
            hospitalID: payload.hospitalID,
            belongsToID: payload.belongsToID
          };
    
          // Only include username if it changed.
          if (payload.username !== this.editingExternalProvider.username) {
            updatePayload.username = payload.username;
          }
    
          // Only include email if it changed.
          if (payload.email !== this.editingExternalProvider.email) {
            updatePayload.email = payload.email;
          }
    
          // Only include password if user chose to change it
          if (!this.externalProviderFormRef.isPasswordSkipped()) {
            updatePayload.password = payload.password;
          }
    
          this._userService.updateExternalProvider(payload.id, updatePayload as IExternalProvider)
            .pipe(takeUntil(this._destroy$))
            .subscribe({
              next: () => {
                this.fetchUser();
                this.closeModals();
              },
              error: (err: unknown) => {
                this.externalProviderFormRef.setApiError(parseUserApiError(err));
              }
            });
        } else {
          // Create new external provider.
          this._userService.createExternalProvider(payload)
            .pipe(takeUntil(this._destroy$))
            .subscribe({
              next: () => {
                this.fetchUser();
                this.closeModals();
            },
              error: (err: unknown) => { this.externalProviderFormRef.setApiError(parseUserApiError(err)); }
            });
        }
    }


    // Handle the result of a delete confirmation action.
    handleDeleteResult(confirmed: boolean) {
      // Close the modal if the action was cancelled.
      if (!confirmed) {
      this.closeModals();
      return;
      }

      
      this._userService.deleteUser(this._pendingDeleteId)
        .pipe(takeUntil(this._destroy$))
        .subscribe({
        next: () => {
            this.fetchUser();
            this.closeModals();
        },
        error: (err) => {
            console.error("Error deleting user:", err);
            alert(err.error);
        }
      });
    }
}