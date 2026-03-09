// Import dependencies.
import { IUserForm } from "./user.interface"; // User form value interface.


// To show the shape emitted on every change so parent can react.
export interface UserFormOutput {
  value: IUserForm;
  valid: boolean;
}