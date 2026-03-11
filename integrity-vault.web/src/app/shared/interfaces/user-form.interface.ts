// Import dependencies.
import { DoctorSpecialty } from "../enums/doctor-specialty.enum"; // Doctor specialty enum.
import { PatientGender } from "../enums/patient-gender.enum"; // Patient gender enum.
import { UserRole } from "../enums/user-role.enum"; // Import all the type of users.


// Represents the full shape of the user creation / edit form.
export interface IUserForm {
  username: string;
  email: string;
  password: string;
  hospitalID: number | null;
  belongsToID: number | null;
  role: UserRole;

  // Doctor only fields.
  firstName: string;
  middleName: string;
  lastName: string;
  specialty: DoctorSpecialty | null;

  // Patient only fields.
  dob: string;
  gender: PatientGender | null;
}