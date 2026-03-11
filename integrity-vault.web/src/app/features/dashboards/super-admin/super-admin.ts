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
import { UserFormComponent } from "../../../shared/components/user-form/user-form"; // Import the user from..
import { ConfirmButtonStyle } from "../../../shared/enums/button-style.enum"; // Import confirm modal button enum.
import { UserRole } from "../../../shared/enums/user-role.enum"; // Import the user role enum.
import { HospitalFormComponent } from "../../../shared/components/hospital-form/hospital-form"; // Import hospital form..
import { IColumnDefinition } from "../../../shared/interfaces/column-definition.interface"; // Interface for column definition.
import { IHospital } from "../../../shared/interfaces/hospital.interface";  // Hospital interface for type-checking.
import { IAdmin } from "../../../shared/interfaces/admin.interface"; // Admin interface for type-checking.
import { IUserForm } from "../../../shared/interfaces/user-form.interface"; // User interface for type-checking.
import { HospitalService } from "../../../core/services/hospital.service"; // Service to interact with hospital API.
import { UserService } from "../../../core/services/user.service"; // Service to interact with user API.
import { parseHospitalApiError } from "../../../shared/utils/hospital-form.validator"; // Validation and error handling utils for hospital.
import { parseUserApiError } from "../../../shared/utils/user-form.validator"; // Validation and error handling utils for user.


// Define the component for the superadmin dashboard.
@Component({
  selector: "app-superadmin-dashboard",
  standalone: true,
  imports: [CommonModule, FormsModule, SidebarComponent, TopbarComponent, ConfirmModalComponent, EntitySectionComponent, EntityModalComponent, HospitalFormComponent, UserFormComponent ],
  templateUrl: "./super-admin.html",
  styleUrls: ["./super-admin.scss"]
})


// Export the SuperadminDashboardComponent class.
export class SuperadminDashboardComponent {
  // Reference to the HospitalFormComponent to access its methods and properties.
  @ViewChild(HospitalFormComponent) hospitalFormRef! : HospitalFormComponent;
  @ViewChild(UserFormComponent) userFormRef! : UserFormComponent;
  

  // Navigation state. 
  activeLink = "hospitals"; // To know which 
  isCollapsed: boolean = false; // When in small screen mode.. to know if the sidebar is close or not.
  ConfirmButtonStyle = ConfirmButtonStyle; // Enum for confirm button style.
  UserRole = UserRole; // Enum for user role.
  
  // Sidebar collapsed state and toggler function.
  toggleSidebar() {
    this.isCollapsed = !this.isCollapsed;
  }

  // Method to handle navigation between different tab. 
  onNavigate(link : string) { 
    this.activeLink = link;
  }


  // Data.
  hospitals : IHospital[] = []; // Define hospitals to store all the hospitals.
  admins : IAdmin[] = []; // Define admins to store all the admins

  // Computed property to check if there are any hospitals.
  get noHospitals() {
    return this.hospitals.length === 0;
  }

  // Column definitions for hospital table.
  hospitalColumns : IColumnDefinition<IHospital>[] = [
    { key : "id", label : "ID", mono : true },
    { key : "name", label : "Name", mono : false },
    { key : "walletAddress", label : "Wallet Address", mono : true, truncate : true },
    { key: "ipAddresses", label: "IP Addresses", mono: true }
  ]

  // Column definitions for admin table.
  adminColumns : IColumnDefinition<IAdmin>[] = [
    { key : "id", label : "ID", mono : true },
    { key : "username", label : "Username", mono : false },
    { key : "email", label : "Eamil", mono : false },
    { key: "hospitalID", label: "Hospital", mono: false },
    { key : "joinDate", label : "Joined", mono : false }
  ]


  // Hopsital form state.
  hospitalInitialValue : IHospital | null = null; // Value to pass to the hospital form.
  editingHospital : IHospital | null = null; // Tracks whether editing of an exisitng hospital is taking place.


  // Admin form state.
  adminInitialValue : IUserForm | null | undefined = undefined  // Value to pass to the user form.
  editingAdmin : IAdmin | null = null; // Tracks whether editing of an existing admin is taking place.


  // Modal state.
  showHospitalModal = false; // State to know whether to show the hospital modal or not.
  showAdminModal = false; // State to know whether to show the admin modal or not.
  showDeleteModal = false; // State to know whether to show the delete modal or not.
  deleteTargetType: "hospital" | "admin" = "hospital"; // Tracks which entity type is pending deletion.
  deleteTarget = ""; // Keep track of which row is being deleted.

  //The hospital or admin id is used when the delete modal is confirmed.
  private _pendingDeleteId : number = 0;


  // Service and teardown.
  private _destroy$ = new Subject<void>(); // Teardown signal for all subscriptions.
  private _hospitalService = inject(HospitalService) // Inject the hospital service to interact with the backend.
  private _userService = inject(UserService) // Inject the user service to interact with the backend.


  // LifeCycle.

  // Initialise data and subscribe to hospital data.
  ngOnInit() { 
    this.fetchHospitals();
    this.fetchAdmins();
  }

  // Clean up any ongoing subscriptions on component destruction
  ngOnDestroy(): void {
    this._destroy$.next();
    this._destroy$.complete();
  }


  // Hospital API.

  // Fetch the list of hospitals from the service.
  fetchHospitals() {
    // Subscribe to the hospital data and assign the result to the hospitals array.
    this._hospitalService.getHospital()
      .pipe(takeUntil(this._destroy$)) // Unsubscribe automatically when the component is destroyed.
      .subscribe({
        next: (hospitals: IHospital[]) => {
          this.hospitals = hospitals; // Assign the data to the hospitals array.
        },
        error: (err) => console.error("Error fetching hospitals:", err),
    });
  }


  // Admin API.

  // Fetch the list of admins from the service.
  fetchAdmins() {
    // Subscribe to the admin data and assign the result to the admins array.
    this._userService.getAllUsers()
      .pipe(takeUntil(this._destroy$)) // Unsubscribe automatically when the component is destroyed.
      .subscribe({
        next: (users) => {
          // Filter only Admin-role users and cast them to IAdmin.
          this.admins = users.filter(u => u.role === UserRole.Admin) as IAdmin[];
        },
        error: (err) => console.error("Error fetching admins:", err),
    });
  }


  // Modal openers.

  // Method to open the hospital modal.
  openHospitalModal(item? : IHospital) {
    this.editingHospital = item ?? null;
    this.hospitalInitialValue = item ?? null;

    // Reset the hospital form when creating a new hospital
    if (!item && this.hospitalFormRef) {
      this.hospitalFormRef.resetForm();
    }
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
      if (this.isCollapsed) this.toggleSidebar();
    });
  }


  // Method to open the delete modal.
  openDeleteModal(id : number, name : string, type: "hospital" | "admin") {
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
    this.showDeleteModal=false;
  }


  // Entity section row actions.

  // Method to edit a hospital row:
  onEditHospital(row : IHospital) {
    if (this.isCollapsed)
      this.toggleSidebar()
    this.openHospitalModal(row);
  }

  // Method to delete a hospital row
  onDeleteHospital(row : IHospital) {
    if (this.isCollapsed)
      this.toggleSidebar()
    this.openDeleteModal(row.id, row.name, "hospital");
  }

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
            this.fetchHospitals(); // Refresh the hospital list.
            this.closeModals(); // Close the modal after success.
          },
          error: (err: unknown) => {
            this.hospitalFormRef.setApiError(parseHospitalApiError(err));
          }
      });
    } else {
      // If adding a new hopsital, create the hospital.
      this._hospitalService.addHospital(formValue)
        .pipe(takeUntil(this._destroy$))
        .subscribe({
          next: () => {
            this.fetchHospitals(); // Refresh the hospital list.
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
  onAdminConfirmed(): void {
    // Stop if form is invalid.
    if (!this.userFormRef.validate())
      return;

    const formValue = this.userFormRef.getValue();

    // Build the IAdmin payload from the form value.
    const payload: IAdmin = {
        id: this.editingAdmin?.id ?? 0,
        username: formValue.username.trim(),
        email: formValue.email.trim(),
        password: formValue.password,
        role: UserRole.Admin,
        joinDate: this.editingAdmin?.joinDate ?? new Date(),
        hospitalID: formValue.hospitalID ?? 0,
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
      if (!this.userFormRef.isPasswordSkipped()) {
        updatePayload.password = payload.password;
      }

      this._userService.updateAdmin(payload.id, updatePayload as IAdmin)
        .pipe(takeUntil(this._destroy$))
        .subscribe({
          next: () => {
            this.fetchAdmins();
            this.closeModals();
          },
          error: (err: unknown) => {
            this.userFormRef.setApiError(parseUserApiError(err));
          }
        });
    } else {
      // Create new admin.
      this._userService.createAdmin(payload)
        .pipe(takeUntil(this._destroy$))
        .subscribe({
          next: () => { this.fetchAdmins(); this.closeModals(); },
          error: (err: unknown) => { this.userFormRef.setApiError(parseUserApiError(err)); }
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

    if (this.deleteTargetType === "hospital") {
      // Proceed with deleting the hospital if confirmed.
      this._hospitalService.deleteHospital(this._pendingDeleteId)
          .pipe(takeUntil(this._destroy$))
          .subscribe({
            next: () => {
              this.fetchHospitals(); // Refresh the hospital list.
              this.closeModals(); // Close the modal after success.
            },
            error: (err) => {
              console.error("Error adding hospital:", err)
              alert(err.error)
            }
        });
    } else {
      this._userService.deleteUser(this._pendingDeleteId)
        .pipe(takeUntil(this._destroy$))
        .subscribe({
          next: () => {
            this.fetchAdmins();
            this.closeModals();
          },
          error: (err) => {
            console.error("Error deleting admin:", err);
            alert(err.error);
          }
        });
    }
  }
}