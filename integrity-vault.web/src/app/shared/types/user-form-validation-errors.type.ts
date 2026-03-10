// Represent all the type of errors that user form can have.
export type UserFormValidationErrors = {
    username?: string;
    email?: string;
    password?: string;
    hospitalId?: string;
    firstName?: string;
    middleName?: string;
    lastName?: string;
    specialty?: string;
    dob?: string;
    gender?: string;
    api?: string;
}