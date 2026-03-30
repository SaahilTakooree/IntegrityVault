// Import the validation utility functions and dependencies.
import { validateEmail, validatePassword, validateDateOfBirth, validateUserForm,
    parseUserApiError, validateRequiredName } from "./user-form.validator";
import { UserRole } from "../../enums/user-role.enum";

describe("UserFormValidator", () => {

    describe("Individual Field Validators", () => {
        
        it("Should validate email format correctly.", () => {
            expect(validateEmail("test@domain.com")).toBeUndefined();
            expect(validateEmail("invalid-email")).toBe("Must be a valid email address.");
            expect(validateEmail("")).toBe("Email is required.");
        });


        it("Should enforce complex password requirements.", () => {
            const errorMsg = "Password must be at least 7 characters and include an uppercase letter, a lowercase letter, a number, and a special character.";
            expect(validatePassword("Short1!")).toBeUndefined();
            expect(validatePassword("password")).toBe(errorMsg);
            expect(validatePassword("1234567")).toBe(errorMsg);
        });


        it("Should enforce name capitalisation.", () => {
            expect(validateRequiredName("John", "First name")).toBeUndefined();
            expect(validateRequiredName("john", "First name")).toBe("First name must start with a capital letter.");
            expect(validateRequiredName("", "First name")).toBe("First name is required.");
        });


        it("Should prevent future dates for Date of Birth.", () => {
            const futureDate = new Date();
            futureDate.setDate(futureDate.getDate() + 1);
            expect(validateDateOfBirth(futureDate.toISOString())).toBe("Date of birth cannot be in the future.");
            expect(validateDateOfBirth("2000-01-01")).toBeUndefined();
        });
    });



    describe("validateUserForm", () => {
        const baseForm = {
            username: "jdoe",
            email: "j@doe.com",
            password: "Qwerty!2",
            hospitalID: 1
        };

        
        it("Should require Doctor-specific fields when role is Doctor.", () => {
            const doctorForm = { ...baseForm, firstName: "", specialty: null } as any;
            const errors = validateUserForm(doctorForm, UserRole.Doctor, true);
            
            expect(errors.firstName).toBe("First name is required.");
            expect(errors.specialty).toBe("Please select a specialty.");
        });


        it("Should require Patient-specific fields when role is Patient.", () => {
            const patientForm = { ...baseForm, firstName: "Jane", dob: "", gender: null } as any;
            const errors = validateUserForm(patientForm, UserRole.Patient, true);
            
            expect(errors.dob).toBe("Date of birth is required.");
            expect(errors.gender).toBe("Please select a gender.");
        });


        it("Should enforce cross-hospital constraints for External Providers.", () => {
            const providerForm = { 
                ...baseForm, 
                hospitalID: 5, 
                belongsToID: 5 // Same as hospitalID
            } as any;
            
            const errors = validateUserForm(providerForm, UserRole.ExternalProvider, true);
            expect(errors.belongsToID).toBe("The owning hospital cannot be the same as the login hospital.");
        });


        it("Should skip password validation when skipPassword is true.", () => {
            const form = { ...baseForm, password: "" } as any;
            const errors = validateUserForm(form, UserRole.Admin, true, true);
            expect(errors.password).toBeUndefined();
        });
    });



    describe("parseUserApiError", () => {
        it("Should identify duplicate username errors.", () => {
            const result = parseUserApiError("Error: Username already taken");
            expect(result).toBe("A user with this username already exists.");
        });


        it("Should identify duplicate email errors.", () => {
            const result = parseUserApiError("The Email field must be unique");
            expect(result).toBe("A user with this email address already exists.");
        });


        it("Should return generic error for unknown issues.", () => {
            expect(parseUserApiError("Database connection failed")).toBe("Error saving the user. Please try again.");
        });
    });
});