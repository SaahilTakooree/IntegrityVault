// Import dependencies.
import { IMedicalRecordForm } from "./medical-record-form.interface"; // Medical record form interface.


// To show the shape emitted on every change so parent can react.
export interface MedicalRecordFormOutput {
    value: IMedicalRecordForm;
    valid: boolean;
}