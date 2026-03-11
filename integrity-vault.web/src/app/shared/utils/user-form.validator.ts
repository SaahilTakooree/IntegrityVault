// Import dependencies.
import { UserRole }      from "../enums/user-role.enum"; // User role enum.
import { DoctorSpecialty } from "../enums/doctor-specialty.enum"; // Doctor specialty enum.
import { PatientGender }   from "../enums/patient-gender.enum"; // Patient gender enum.
import { UserFormValidationErrors } from "../types/user-form-validation-errors.type"; // All possible form errors.
import { IUserForm } from "../interfaces/user-form.interface"; // User form value interface.


// Regex to have a password with at least 1 uppercase, 1 lowercase, 1 digit, 1 special char, and min 7 chars.
const PASSWORD_REGEX = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?`~]).{7,}$/;

// Regex to valid email format.
const EMAIL_REGEX = /^.+@.{2,}\..{2,}$/;

// Regex to must start with a capital letter.
const STARTS_CAPITAL_REGEX = /^[A-Z]/;


// Validates the username field.
export function validateUsername(username : string) : string | undefined {
  if (!username.trim())
    return "Username is required.";
  return undefined;
}


// Validates the email field.
export function validateEmail(email : string) : string | undefined {
  if (!email.trim())
    return "Email is required.";
  if (!EMAIL_REGEX.test(email.trim()))
    return "Must be a valid email address.";
  return undefined;
}


// Validates the password field.
export function validatePassword(password : string) : string | undefined {
  if (!password)
    return "Password is required.";
  if (!PASSWORD_REGEX.test(password))
    return "Password must be at least 7 characters and include an uppercase letter, a lowercase letter, a number, and a special character.";
  return undefined;
}


// Validates the hospital selection when it is required.
export function validateHospitalId(hospitalId : number | null) : string | undefined {
  if (hospitalId === null || hospitalId === 0)
    return "Please select a hospital.";
  return undefined;
}


// Validates a name field that must start with a capital letter.
export function validateRequiredName(value : string, fieldLabel : string): string | undefined {
  if (!value.trim())
    return `${fieldLabel} is required.`;
  if (!STARTS_CAPITAL_REGEX.test(value.trim()))
    return `${fieldLabel} must start with a capital letter.`;
  return undefined;
}


// Validates the optional middle name field.
export function validateMiddleName(value : string) : string | undefined {
  if (!value.trim())
    return undefined; // Optional – empty is fine.
  if (!STARTS_CAPITAL_REGEX.test(value.trim()))
    return "Middle name must start with a capital letter.";
  return undefined;
}


// Validates the doctor specialty selection.
export function validateSpecialty(specialty : DoctorSpecialty | null) : string | undefined {
  if (specialty === null)
    return "Please select a specialty.";
  return undefined;
}


// Validates the date of birth field.
export function validateDateOfBirth(dateOfBirth : string) : string | undefined {
  if (!dateOfBirth ||!dateOfBirth.trim())
    return "Date of birth is required.";

  const dob = new Date(dateOfBirth);
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  if (dob > today)
    return "Date of birth cannot be in the future.";
  
  return undefined;

}


// Validates the gender selection.
export function validateGender(gender : PatientGender | null) : string | undefined {
  if (gender === null)
    return "Please select a gender.";
  return undefined;
}


// Validates the complete user form based on role and whether the hospital field is shown.
export function validateUserForm( form : IUserForm, role : UserRole, showHospitalSelect : boolean, skipPassword: boolean = false) : UserFormValidationErrors {
  const errors: UserFormValidationErrors = {};

  // Base fields that is present for every role.
  const usernameErr = validateUsername(form.username);
  if (usernameErr) errors.username = usernameErr;

  const emailErr = validateEmail(form.email);
  if (emailErr) errors.email = emailErr;

  // Only validate password when it is required.
  if (!skipPassword) {
    const passwordErr = validatePassword(form.password);
    if (passwordErr) errors.password = passwordErr;
  }

  // Hospital selection only when the parent chooses to show it.
  if (showHospitalSelect) {
    const hospitalErr = validateHospitalId(form.hospitalID);
    if (hospitalErr) errors.hospitalId = hospitalErr;
  }

  // External providers specific fields
  if (role === UserRole.ExternalProvider) {
    // belongsToID is required for external providers.
    if (form.belongsToID == null) {
      errors.belongsToID = "Belongs To is required.";
    }
    // belongsToID cannot match hospitalID — enforces the cross-hospital constraint.
    else if (form.hospitalID != null && form.belongsToID === form.hospitalID) {
      errors.belongsToID = "The owning hospital cannot be the same as the login hospital.";
    }
  }

  // Doctor specific fields.
  if (role === UserRole.Doctor) {
    const firstErr = validateRequiredName(form.firstName, "First name");
    if (firstErr) errors.firstName = firstErr;

    const middleErr = validateMiddleName(form.middleName);
    if (middleErr) errors.middleName = middleErr;

    const lastErr = validateRequiredName(form.lastName, "Last name");
    if (lastErr) errors.lastName = lastErr;

    const specialtyErr = validateSpecialty(form.specialty);
    if (specialtyErr) errors.specialty = specialtyErr;
  }

  // Patient specific fields.
  if (role === UserRole.Patient) {
    const firstErr = validateRequiredName(form.firstName, "First name");
    if (firstErr) errors.firstName = firstErr;

    const middleErr = validateMiddleName(form.middleName);
    if (middleErr) errors.middleName = middleErr;

    const lastErr = validateRequiredName(form.lastName, "Last name");
    if (lastErr) errors.lastName = lastErr;

    const dobErr = validateDateOfBirth(form.dob);
    if (dobErr) errors.dob = dobErr;

    const genderErr = validateGender(form.gender);
    if (genderErr) errors.gender = genderErr;
  }

  return errors;
}


// Converts an API error into a human-readable string suitable for display in the form.
export function parseUserApiError(err: unknown): string {
  const message =
    err instanceof Error
      ? err.message
      : typeof err === "string"
      ? err
      : JSON.stringify(err);

  if (message.toLowerCase().includes("username"))
    return "A user with this username already exists.";
  if (message.toLowerCase().includes("email"))
    return "A user with this email address already exists.";
  return "Error saving the user. Please try again.";
}