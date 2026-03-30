// Import the validation utility functions.
import { validateMedicalRecordForm, parseMedicalRecordApiError } from "./medical-record-form.validator";
import { IMedicalRecordForm } from "../../interfaces/medical-record-form.interface";

describe("MedicalRecordFormValidator", () => {

    // Helper to create a valid base form to avoid repetitive setup.
    const createValidForm = (): IMedicalRecordForm => ({
        patientID: 1,
        visitDate: "2026-03-29",
        chiefComplaint: "Sore throat",
        diagnosis: "Pharyngitis",
        treatmentPlan: "Rest and hydration",
        doctorNotes: "",
        followUpInstructions: ""
    });

    
    describe("validateMedicalRecordForm", () => {
        

        it("Should return an error if patientID is missing and requirePatient is true.", () => {
            const form = createValidForm();
            form.patientID = null;
            
            const errors = validateMedicalRecordForm(form, true, false);
            expect(errors.patientID).toBe("Patient is required.");
        });


        it("Should NOT return an error if patientID is missing but requirePatient is false.", () => {
            const form = createValidForm();
            form.patientID = null;
            
            const errors = validateMedicalRecordForm(form, false, false);
            expect(errors.patientID).toBeUndefined();
        });


        it("Should validate chiefComplaint only if requireChiefComplaint is true.", () => {
            const form = createValidForm();
            form.chiefComplaint = "   "; // Whitespace
            
            const errorsWithReq = validateMedicalRecordForm(form, false, true);
            expect(errorsWithReq.chiefComplaint).toBe("Chief complaint is required.");

            const errorsWithoutReq = validateMedicalRecordForm(form, false, false);
            expect(errorsWithoutReq.chiefComplaint).toBeUndefined();
        });


        it("Should always require visitDate, diagnosis, and treatmentPlan.", () => {
            const emptyForm: any = {
                visitDate: "",
                diagnosis: " ",
                treatmentPlan: null
            };

            const errors = validateMedicalRecordForm(emptyForm, false, false);
            
            expect(errors.visitDate).toBe("Visit date is required.");
            expect(errors.diagnosis).toBe("Diagnosis is required.");
            expect(errors.treatmentPlan).toBe("Treatment plan is required.");
        });


        it("Should return an empty errors object for a fully valid form.", () => {
            const form = createValidForm();
            const errors = validateMedicalRecordForm(form, true, true);
            
            expect(Object.keys(errors).length).toBe(0);
        });
    });



    describe("parseMedicalRecordApiError", () => {
        
        it("Should extract error string directly from the error property.", () => {
            const mockError = { error: "Diagnosis too short" };
            expect(parseMedicalRecordApiError(mockError)).toBe("Diagnosis too short");
        });


        it("Should extract message from a nested error object.", () => {
            const mockError = { 
                error: { message: "Invalid Patient ID provided." } 
            };
            expect(parseMedicalRecordApiError(mockError)).toBe("Invalid Patient ID provided.");
        });


        it("Should return a fallback message for null or malformed error objects.", () => {
            const fallback = "An unexpected error occurred. Please try again.";
            
            expect(parseMedicalRecordApiError(null)).toBe(fallback);
            expect(parseMedicalRecordApiError({})).toBe(fallback);
            expect(parseMedicalRecordApiError({ someOtherProp: "test" })).toBe(fallback);
        });
    });
});