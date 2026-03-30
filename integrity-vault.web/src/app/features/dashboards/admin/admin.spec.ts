// Import dependencies
import { ComponentFixture, TestBed, fakeAsync, tick } from "@angular/core/testing"; // Import the angular testing utilities.
import { AdminDashboardComponent } from "./admin"; // Import the component to be tested.
import { HospitalService } from "../../../core/services/hospital/hospital.service"; // Service to interact with hospital API.
import { UserService } from "../../../core/services/user/user.service"; // Service to interact with user API.
import { AuthService } from "../../../core/services/auth/auth.service"; // Service to interact with auth API.
import { UserRole } from "../../../shared/enums/user-role.enum"; // Import user role enum.
import { By } from "@angular/platform-browser"; // Utility to query the DOM.
import { of } from "rxjs"; // RxJS utility to create observable streams.
import { NO_ERRORS_SCHEMA } from "@angular/core"; // Schema to ignore child component errors.


// Define the test suite for the AdminDashboardComponent.
describe("AdminDashboardComponent", () => {
  let component: AdminDashboardComponent;
  let fixture: ComponentFixture<AdminDashboardComponent>;
  
  // Define mock services and fake data.
  let mockHospitalService: any;
  let mockUserService: any;
  let mockAuthService: any;

  const fakeAdmin = { id: 10, username: 'admin_one', hospitalId: 1 };


  // Set up the testing module and mock service implementations.
  beforeEach(async () => {
    mockHospitalService = {
        getHospital: jasmine.createSpy().and.returnValue(of([])),
        getHospitalById: jasmine.createSpy().and.returnValue(
            of({ name: 'General Hospital', ipAddresses: ['127.0.0.1'] })
        ),
        updateHospital: jasmine.createSpy().and.returnValue(of({}))
    };
    
    mockUserService = {
        getAllUsers: jasmine.createSpy().and.returnValue(of([])),
        deleteUser: jasmine.createSpy().and.returnValue(of({})),
        createDoctor: jasmine.createSpy().and.returnValue(of({}))
    };

    mockAuthService = {
      CurrentUser: fakeAdmin
    };

    await TestBed.configureTestingModule({
      imports: [AdminDashboardComponent],
      providers: [
        { provide: HospitalService, useValue: mockHospitalService },
        { provide: UserService, useValue: mockUserService },
        { provide: AuthService, useValue: mockAuthService }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(AdminDashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });



  // Method to check if the correct section renders on navigation.
  it("Should update activeLink and render the correct section.", () => {
    component.onNavigate("doctors");
    fixture.detectChanges();
    
    const doctorSection = fixture.debugElement.query(By.css('app-entity-section[title="Doctor"]'));
    expect(doctorSection).toBeTruthy();
    expect(component.activeLink).toBe("doctors");
  });


  // Method to verify the add patient constraint logic.
  it("Should disable the 'Add Patient' button if no doctors exist.", () => {
    component.activeLink = "patients";
    component.doctors = []; 
    fixture.detectChanges();

    const patientSection = fixture.debugElement.query(By.css('app-entity-section[title="Patient"]'));
    
    expect(patientSection.componentInstance.addDisable).toBeTrue();
  });


  // Method to check if the IP table renders correctly with data.
  it("Should render the IP address table when hospital data is present.", () => {
    component.activeLink = "ip-management";
    component.hospital = {
        name: "St. Marys",
        walletAddress: "0x123",
        ipAddresses: ["192.168.1.1", "10.0.0.1"]
    } as any;
    fixture.detectChanges();

    const rows = fixture.debugElement.queryAll(By.css("tbody tr"));
    expect(rows.length).toBe(2);
    expect(rows[0].nativeElement.textContent).toContain("192.168.1.1");
  });



  // Method to verify users are sorted by role correctly.
  it("Should sort raw users into specific role arrays on fetchUser.", () => {
    const mixedUsers = [
      { id: 1, role: UserRole.Admin },
      { id: 2, role: UserRole.Doctor },
      { id: 3, role: UserRole.Patient }
    ];
    
    mockUserService.getAllUsers.and.returnValue(of(mixedUsers));
    
    component.fetchUser();
    
    expect(component.admins.length).toBe(1);
    expect(component.doctors.length).toBe(1);
    expect(component.patients.length).toBe(1);
  });


  // Method to handle the async modal reset logic.
  it("Should open doctor modal and handle the setTimeout reset.", fakeAsync(() => {
    const doctorData = { id: 5, username: 'dr_smith', firstName: 'John' } as any;
    
    component.openDoctorModal(doctorData);
    
    expect(component.doctorInitialValue).toBeUndefined();
    
    tick();
    
    expect(component.doctorInitialValue?.username).toBe('dr_smith');
    expect(component.showDoctorModal).toBeTrue();
  }));


  // Method to verify user deletion and data refresh.
  it("Should call deleteUser and refresh data when deletion is confirmed.", () => {
    component.openDeleteModal(50, "Test User", "patient");
    
    component.handleDeleteResult(true);
    
    expect(mockUserService.deleteUser).toHaveBeenCalledWith(50);
    expect(mockUserService.getAllUsers).toHaveBeenCalled(); 
  });


  // Method to ensure all modals are closed.
  it("Should close all modals using closeModals().", () => {
    component.showAdminModal = true;
    component.showHospitalModal = true;
    
    component.closeModals();
    
    expect(component.showAdminModal).toBeFalse();
    expect(component.showHospitalModal).toBeFalse();
  });
});