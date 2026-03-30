// Import dependencies.
import { ComponentFixture, TestBed, fakeAsync, tick } from "@angular/core/testing"; // Import the angular testing utilities.
import { SuperadminDashboardComponent } from "./super-admin"; // Import the component to be tested.
import { HospitalService } from "../../../core/services/hospital/hospital.service"; // Service to interact with hospital API.
import { UserService } from "../../../core/services/user/user.service"; // Service to interact with user API.
import { By } from "@angular/platform-browser"; // Utility to query the DOM.
import { of } from "rxjs"; // RxJS utility to create observable streams.
import { NO_ERRORS_SCHEMA } from "@angular/core"; // Schema to ignore child component errors.
import { provideHttpClientTesting } from '@angular/common/http/testing'; // Provides a mock HttpClient for testing HTTP requests.
import { provideHttpClient } from '@angular/common/http'; // Provides the standard HttpClient for dependency injection in the test environment.



// Define the test suite for the SuperadminDashboardComponent.
describe("SuperadminDashboardComponent", () => {
  let component: SuperadminDashboardComponent;
  let fixture: ComponentFixture<SuperadminDashboardComponent>;
  
  // Define mock services and fake data.
  let mockHospitalService: any;
  let mockUserService: any;

  const fakeHospitals = [
    { id: 1, name: 'St. Marys', walletAddress: '0x123', ipAddresses: ['192.168.1.1'] }
  ];


  // Set up the testing module and mock service implementations.
  beforeEach(async () => {
    mockHospitalService = {
        getHospital: jasmine.createSpy().and.returnValue(of(fakeHospitals)),
        addHospital: jasmine.createSpy().and.returnValue(of({})),
        updateHospital: jasmine.createSpy().and.returnValue(of({})),
        deleteHospital: jasmine.createSpy().and.returnValue(of({}))
    };
    
    mockUserService = {
        getAllUsers: jasmine.createSpy().and.returnValue(of([])),
        createAdmin: jasmine.createSpy().and.returnValue(of({})),
        updateAdmin: jasmine.createSpy().and.returnValue(of({})),
        deleteUser: jasmine.createSpy().and.returnValue(of({}))
    };

    await TestBed.configureTestingModule({
      imports: [SuperadminDashboardComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: HospitalService, useValue: mockHospitalService },
        { provide: UserService, useValue: mockUserService }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(SuperadminDashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });


  // Method to check if the correct section renders on navigation.
  it("Should update activeLink and render the correct section.", () => {
    component.onNavigate("admins");
    fixture.detectChanges();
    
    const adminSection = fixture.debugElement.query(By.css('app-entity-section[title="Admins"]'));
    expect(adminSection).toBeTruthy();
    expect(component.activeLink).toBe("admins");
  });


  // Method to verify the admin creation button is disabled if no hospitals exist.
  it("Should disable the 'Add Admin' button if no hospitals exist.", () => {
    component.activeLink = "admins";
    component.hospitals = []; // Set hospitals to empty.
    fixture.detectChanges();

    const adminSection = fixture.debugElement.query(By.css('app-entity-section[title="Admins"]'));
    
    // Check the getter logic for noHospitals.
    expect(adminSection.componentInstance.addDisable).toBeTrue();
  });


  // Method to verify the hospital list is populated on init.
  it("Should fetch hospitals and admins on initialisation.", () => {
    expect(mockHospitalService.getHospital).toHaveBeenCalled();
    expect(mockUserService.getAllUsers).toHaveBeenCalled();
    expect(component.hospitals.length).toBe(1);
  });


  // Method to handle the async admin modal value mapping.
  it("Should open admin modal and map initial values correctly.", fakeAsync(() => {
    const adminData = { id: 1, username: 'admin_test', email: 'test@hospital.com', hospitalID: 1 } as any;
    
    component.openAdminModal(adminData);
    
    expect(component.adminInitialValue).toBeUndefined();
    
    tick();
    
    expect(component.adminInitialValue?.username).toBe('admin_test');
    expect(component.showAdminModal).toBeTrue();
  }));


  // Method to check if the hospital form reset is triggered when adding new.
  it("Should reset hospital form when opening modal for a new hospital.", () => {
    // Manually mock the ViewChild reference.
    component.hospitalFormRef = { resetForm: jasmine.createSpy('resetForm') } as any;
    
    component.openHospitalModal();
    
    expect(component.hospitalFormRef.resetForm).toHaveBeenCalled();
    expect(component.editingHospital).toBeNull();
  });


  // Method to ensure all modals are closed.
  it("Should close all modals using closeModals().", () => {
    component.showHospitalModal = true;
    component.showAdminModal = true;
    component.showDeleteModal = true;
    
    component.closeModals();
    
    expect(component.showHospitalModal).toBeFalse();
    expect(component.showAdminModal).toBeFalse();
    expect(component.showDeleteModal).toBeFalse();
  });


  // Method to verify hospital deletion flow.
  it("Should call deleteHospital and refresh data when confirmed.", () => {
    component.openDeleteModal(1, "General Hospital", "hospital");
    
    component.handleDeleteResult(true);
    
    expect(mockHospitalService.deleteHospital).toHaveBeenCalledWith(1);
    expect(mockHospitalService.getHospital).toHaveBeenCalled();
  });


  // Method to verify admin deletion flow.
  it("Should call deleteUser and refresh data when admin deletion is confirmed.", () => {
    component.openDeleteModal(10, "admin_user", "admin");
    
    component.handleDeleteResult(true);
    
    expect(mockUserService.deleteUser).toHaveBeenCalledWith(10);
    expect(mockUserService.getAllUsers).toHaveBeenCalled();
  });


  // Method to verify navigation when clicking the warning action in admin section.
  it("Should navigate to hospitals when warning action is triggered.", () => {
    spyOn(component, 'onNavigate');
    
    component.activeLink = "admins";
    fixture.detectChanges();
    
    const adminSection = fixture.debugElement.query(By.css('app-entity-section[title="Admins"]'));
    adminSection.triggerEventHandler('warningAction', 'hospitals');
    
    expect(component.onNavigate).toHaveBeenCalledWith('hospitals');
  });
});