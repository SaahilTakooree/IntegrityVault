// Represent all the type of error that login form can have.
export type LoginFormValidationErrors = {
  usernameOrEmail? : string;
  password? : string
  api? : string;
}