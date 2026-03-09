// Import dependencies.
import { HospitalFormValidationErrors } from "../types/hospital-form-validation-errors.type"; // Import all the possible error that hospital form might contain.


// Regex for validating Ethereum wallet address format.
const ETH_WALLET_REGEX = /^0x[0-9a-fA-F]{40}$/;

// Regex for validating IPv4 and IPv6 addresses.
const IPV4_REGEX = /^(\d{1,3}\.){3}\d{1,3}$/;
const IPV6_REGEX = /^[0-9a-fA-F:]{2,45}$/;

// Substring used to detect duplicate-wallet errors returned from the API.
const DUPLICATE_WALLET_MSG = "A Hospital with the wallet address";


// Function what check if the hospital name is valid or not.
export function validateHospitalName(name : string): string | undefined {
  if (!name.trim())
    return "Hospital name is required.";

  return undefined;
}


// Function to validate an ethereum address.
export function validateWalletAddress(walletAddress: string): string | undefined {
  if (!walletAddress.trim())
    return "Wallet address is required.";

  if (!ETH_WALLET_REGEX.test(walletAddress))
    return "Must be a valid Ethereum address (0x followed by 40 hex characters).";

  return undefined;
}

// Function to check if an ip address valid or not.
export function isValidIpAddress(ip : string): boolean {
  return IPV4_REGEX.test(ip) || IPV6_REGEX.test(ip);
}


// Validates the ip addresses array.
export function validateIpAddresses(ipAddresses : string[]): string | undefined {
  if (ipAddresses.length === 0 || ipAddresses.every(ip => !ip.trim()))
    return "At least one IP address is required.";

  const invalid = ipAddresses.find(ip => ip.trim() && !isValidIpAddress(ip.trim()));
  if (invalid) return `"${invalid}" is not a valid IPv4 or IPv6 address.`;

  const filled = ipAddresses.filter(ip => ip.trim());
  if (new Set(filled).size !== filled.length)
    return "Duplicate IP addresses are not allowed.";

  return undefined;
}


// Function to validates the complete hospital form.
export function validateHospitalForm( name : string, walletAddress : string, ipAddresses : string[] ): HospitalFormValidationErrors {
  const errors: HospitalFormValidationErrors = {};

  const nameErr = validateHospitalName(name);
  if (nameErr)
    errors.name = nameErr;

  const walletErr = validateWalletAddress(walletAddress);
  if (walletErr)
    errors.walletAddress = walletErr;

  const ipErr = validateIpAddresses(ipAddresses);
  if (ipErr)
    errors.ipAddresses = ipErr;
  
  return errors;
}


// Function to converts api  error into a human-readable string suitable for display in the form.
export function parseHospitalApiError(err: unknown): string {
  const message =
    err instanceof Error
      ? err.message
      : typeof err === "string"
      ? err
      : JSON.stringify(err);

  if (message.includes(DUPLICATE_WALLET_MSG)) {
    const match = message.match(/0x[0-9a-fA-F]{40}/);
    const wallet = match ? match[0] : "";
    return `A hospital with the wallet address ${wallet} already exists.`;
  }

  if (message.includes("IP address"))
    return "One or more of these IP addresses is already registered to this hospital.";

  return "Error saving the hospital. Please try again.";
}