// Import dependencies.
import { IHospital } from "./hospital.interface"; // Hospital interface.


// To show the shape emitted on every change so parent can react.
export interface HospitalFormOutput {
  value: IHospital;
  valid: boolean;
}