// Import the validation utility functions.
import { validateHospitalName, validateWalletAddress, 
    isValidIpAddress, validateIpAddresses, validateHospitalForm, 
    parseHospitalApiError } from "./hospital-form.validator";


describe("HospitalFormValidator", () => {

    describe("validateHospitalName", () => {
        it("Should return an error message if the name is empty or just whitespace.", () => {
            expect(validateHospitalName("")).toBe("Hospital name is required.");
            expect(validateHospitalName("   ")).toBe("Hospital name is required.");
        });


        it("Should return undefined for a valid name.", () => {
            expect(validateHospitalName("General Hospital")).toBeUndefined();
        });
    });



    describe("validateWalletAddress", () => {
        it("Should return an error if the wallet address is empty.", () => {
            expect(validateWalletAddress("")).toBe("Wallet address is required.");
        });


        it("Should return an error for invalid Ethereum address formats.", () => {
            const invalidMsg = "Must be a valid Ethereum address (0x followed by 40 hex characters).";
            expect(validateWalletAddress("0x123")).toBe(invalidMsg);
            expect(validateWalletAddress("1234567890123456789012345678901234567890")).toBe(invalidMsg); // Missing 0x
            expect(validateWalletAddress("0xGHIJKLMNOPQRSTUVWXYZGHIJKLMNOPQRSTUVWXYZ")).toBe(invalidMsg); // Non-hex
        });


        it("Should return undefined for a valid 40-character hex Ethereum address.", () => {
            const validAddress = "0x1234567890abcdef1234567890abcdef12345678";
            expect(validateWalletAddress(validAddress)).toBeUndefined();
        });
    });



    describe("isValidIpAddress", () => {
        it("Should validate IPv4 addresses correctly.", () => {
            expect(isValidIpAddress("192.168.1.1")).toBeTrue();
            expect(isValidIpAddress("999.999.999.999")).toBeTrue(); // Regex is simple, checks format 3.3.3.3
        });

        it("Should validate IPv6 addresses correctly.", () => {
            expect(isValidIpAddress("2001:0db8:85a3:0000:0000:8a2e:0370:7334")).toBeTrue();
            expect(isValidIpAddress("::1")).toBeTrue();
        });

        it("Should return false for invalid IP formats.", () => {
            expect(isValidIpAddress("not-an-ip")).toBeFalse();
            expect(isValidIpAddress("123.456")).toBeFalse();
        });
    });



    describe("validateIpAddresses", () => {
        it("Should return error if the list is empty or contains only empty strings.", () => {
            expect(validateIpAddresses([])).toBe("At least one IP address is required.");
            expect(validateIpAddresses([" ", ""])).toBe("At least one IP address is required.");
        });


        it("Should detect invalid IP formats in the array.", () => {
            const result = validateIpAddresses(["127.0.0.1", "invalid-ip"]);
            expect(result).toBe('"invalid-ip" is not a valid IPv4 or IPv6 address.');
        });


        it("Should return an error if there are duplicate IP addresses.", () => {
            expect(validateIpAddresses(["1.1.1.1", "1.1.1.1"])).toBe("Duplicate IP addresses are not allowed.");
        });


        it("Should return undefined for a valid list of IPs.", () => {
            expect(validateIpAddresses(["192.168.0.1", "10.0.0.1"])).toBeUndefined();
        });
    });
    
    

    describe("validateHospitalForm", () => {
        it("Should require a private key when isEdit is false.", () => {
            const errors = validateHospitalForm("Hosp", "0x1234567890123456789012345678901234567890", ["1.1.1.1"], "", false);
            expect(errors.privateKey).toBe("Private key is required.");
        });


        it("Should NOT require a private key when isEdit is true.", () => {
            const errors = validateHospitalForm("Hosp", "0x1234567890123456789012345678901234567890", ["1.1.1.1"], "", true);
            expect(errors.privateKey).toBeUndefined();
        });


        it("Should aggregate multiple validation errors.", () => {
            const errors = validateHospitalForm("", "invalid", [], "", false);
            expect(errors.name).toBeTruthy();
            expect(errors.walletAddress).toBeTruthy();
            expect(errors.ipAddresses).toBeTruthy();
            expect(errors.privateKey).toBeTruthy();
        });
    });

    

    describe("parseHospitalApiError", () => {
        it("Should handle duplicate wallet errors and extract the address.", () => {
            const apiError = "A Hospital with the wallet address 0x1234567890123456789012345678901234567890 already exists.";
            const parsed = parseHospitalApiError(apiError);
            expect(parsed).toContain("already exists");
            expect(parsed).toContain("0x1234567890123456789012345678901234567890");
        });


        it("Should handle IP address registration errors.", () => {
            const parsed = parseHospitalApiError("This IP address is taken.");
            expect(parsed).toBe("One or more of these IP addresses is already registered to this hospital.");
        });


        it("Should return a generic error message for unknown errors.", () => {
            expect(parseHospitalApiError({})).toBe("Error saving the hospital. Please try again.");
        });
    });
});