// Represent all the type of error that hospital form can have.
export type HospitalFormValidationErrors = {
  name?: string;
  walletAddress?: string;
  ipAddresses?: string;
  api?: string;
}