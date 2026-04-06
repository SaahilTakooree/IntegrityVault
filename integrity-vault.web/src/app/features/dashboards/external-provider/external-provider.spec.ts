// Import dependencies.
import { ComponentFixture, TestBed } from "@angular/core/testing"; // Import the angular testing utilities.
import { ExternalProviderDashboardComponent } from "./external-provider"; // Import the component to be tested.
import { MedicalRecordService } from "../../../core/services/medical-record/medical-record.service"; // Service for medical record operations.
import { AuthService } from "../../../core/services/auth/auth.service"; // Service to get the current logged-in user.
import { of, throwError } from "rxjs"; // RxJS utilities for observable streams.
import { NO_ERRORS_SCHEMA } from "@angular/core"; // Schema to ignore child component errors.
import { provideHttpClientTesting } from "@angular/common/http/testing"; // Provides a mock HttpClient.
import { provideHttpClient } from "@angular/common/http"; // Provides the standard HttpClient.


describe("ExternalProviderDashboardComponent", () => {
    let component: ExternalProviderDashboardComponent;
    let fixture: ComponentFixture<ExternalProviderDashboardComponent>;
    
    // Define mock services and fake data.
    let mockMedicalRecordService: any;
    let mockAuthService: any;

    const fakeUser = { id: 99, username: 'ext_provider', role: 'ExternalProvider' };


    beforeEach(async () => {
        mockAuthService = {
            CurrentUser: fakeUser
        };

        mockMedicalRecordService = {
            verifyPdfTampering: jasmine.createSpy().and.returnValue(of({ isTampered: false }))
        };

        await TestBed.configureTestingModule({
            imports: [ExternalProviderDashboardComponent],
            providers: [
                provideHttpClient(),
                provideHttpClientTesting(),
                { provide: AuthService, useValue: mockAuthService },
                { provide: MedicalRecordService, useValue: mockMedicalRecordService }
            ],
            schemas: [NO_ERRORS_SCHEMA]
        }).compileComponents();

        fixture = TestBed.createComponent(ExternalProviderDashboardComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });


    // Basic instantiation check.
    it("Should create the component.", () => {
        expect(component).toBeTruthy();
    });


    // Method to verify that the user ID is correctly initialised from the auth service.
    it("Should initialise with the correct user ID from the auth service.", () => {
        expect(component.userID).toBe(99);
    });


    // Method to verify manual PDF verification logic and state transitions.
    it("Should call verifyPdfTampering and update state when a file is uploaded.", () => {
        const mockFile = new File([""], "test.pdf", { type: "application/pdf" });
        
        component.onPdfVerifyRequested(mockFile);
        
        expect(component.pdfVerifyLoading).toBeFalse();
        expect(mockMedicalRecordService.verifyPdfTampering).toHaveBeenCalledWith(99, mockFile);
        expect(component.pdfVerifyResult?.isTampered).toBeFalse();
    });


    // Method to check the clear functionality for PDF verification state.
    it("Should clear PDF verification results and error messages.", () => {
        component.pdfVerifyResult = { isTampered: false } as any;
        component.pdfVerifyErrorMessage = "Some error";
        
        component.onPdfClear();
        
        expect(component.pdfVerifyResult).toBeNull();
        expect(component.pdfVerifyErrorMessage).toBeNull();
    });


    // Method to verify that error messages are extracted from nested error objects.
    it("Should extract error messages from nested error objects in response.", () => {
        const errorResponse = { 
            error: { message: "Invalid PDF format." } 
        };
        
        const message = (component as any)._extractErrorMessage(errorResponse);
        expect(message).toBe("Invalid PDF format.");
    });


    // Method to verify that a fallback message is returned for unrecognised error shapes.
    it("Should return a default fallback error message for unknown shapes.", () => {
        const weirdError = { unknown: "data" };
        const message = (component as any)._extractErrorMessage(weirdError);
        expect(message).toBe("An unexpected error occurred during verification.");
    });


    // Method to verify that subscriptions are cleaned up on component destruction.
    it("Should complete the destroy subject on destruction.", () => {
        const nextSpy = spyOn((component as any)._destroy$, 'next');
        const completeSpy = spyOn((component as any)._destroy$, 'complete');
        
        component.ngOnDestroy();
        
        expect(nextSpy).toHaveBeenCalled();
        expect(completeSpy).toHaveBeenCalled();
    });


    // Method to ensure the loading indicator is reset even when the API returns an error.
    it("Should reset loading state when the tampering verification fails.", () => {
        mockMedicalRecordService.verifyPdfTampering.and.returnValue(throwError(() => new Error("Fail")));
        const mockFile = new File([""], "test.pdf");

        component.onPdfVerifyRequested(mockFile);

        expect(component.pdfVerifyLoading).toBeFalse();
        expect(component.pdfVerifyErrorMessage).toBeDefined();
    });
});