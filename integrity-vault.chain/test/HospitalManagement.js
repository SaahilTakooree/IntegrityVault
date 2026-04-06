const { expect } = require("chai");
const { ethers } = require("hardhat");


// Test suite for the hospital management system.
describe("HospitalManagement", function () {
    // To store the instance of the contract.
    let registry;

    // To store the signer accounts.
    let owner, hospitalA, hospitalB, attacker;



    // Runs before each test.
    beforeEach(async function () {
        // Get a list of test accounts.
        [owner, hospitalA, hospitalB, attacker] = await ethers.getSigners();

        // Deploy a fresh instance of the contract.
        const Registry = await ethers.getContractFactory("HospitalManagement");
        registry = await Registry.deploy();
        await registry.waitForDeployment();
    });



    // Deployment tests.
    describe("Deployment", function () {
        // Check if the deployer is correctly set as the owner.
        it("Should set the deployer as owner", async function () {
            expect(await registry.owner()).to.equal(owner.address);
        });
    });



    // Tests for hospital management functionality.
    describe("Hospital Management", function () {
        describe("Add Hospital", function () {

            // Test if the owner can add a hospital.
            it("Owner can add a hospital", async function () {
                await expect(registry.addHospital(1, hospitalA.address))
                    .to.emit(registry, "HospitalAdded") // Check for emitted event.
                    .withArgs(1, hospitalA.address);

                const result = await registry.getHospital(1);
                expect(result[0]).to.equal(hospitalA.address); // Hospital wallet address.
                expect(result[1]).to.equal(true); // Hospital active status.
            });


            // Non-owner cannot add.
            it("Non-owner cannot add hospital", async function () {
                await expect(
                registry.connect(attacker).addHospital(1, hospitalA.address)
                ).to.be.revertedWith("Not owner.");
            });
        });


        // Remove hospital.
        describe("Remove Hospital", function () {
            beforeEach(async function () {
                await registry.addHospital(1, hospitalA.address);
            });

            it("Owner can remove hospital", async function () {
                await expect(registry.removeHospital(1))
                    .to.emit(registry, "HospitalRemoved") // Event check.
                    .withArgs(1);

                const result = await registry.getHospital(1);
                expect(result[1]).to.equal(false); // Hospital is no longer active.
            });

            it("Non-owner cannot remove hospital", async function () {
                await expect(
                    registry.connect(attacker).removeHospital(1)
                ).to.be.revertedWith("Not owner.");
            });
        });


        // Update the whole 
        describe("Update Hospital Wallet", function () {
            beforeEach(async function () {
                await registry.addHospital(1, hospitalA.address);
            });

            it("Owner can update hospital wallet", async function () {
                await expect(registry.updateHospitalWallet(1, hospitalB.address))
                    .to.emit(registry, "HospitalUpdated") // Event check.
                    .withArgs(1, hospitalA.address, hospitalB.address);

                const result = await registry.getHospital(1);
                expect(result[0]).to.equal(hospitalB.address); // Wallet updated.
            });

            it("Non-owner cannot update hospital wallet", async function () {
                await expect(
                    registry.connect(attacker).updateHospitalWallet(1, hospitalB.address)
                ).to.be.revertedWith("Not owner.");
            });
        });


        // Wallet authorisation check.
        describe("Wallet Authorisation", function () {
            it("Should return correct authorisation status", async function () {
                await registry.addHospital(1, hospitalA.address);
                expect(await registry.isAuthorised(hospitalA.address)).to.equal(true);  // Authorised.
                expect(await registry.isAuthorised(hospitalB.address)).to.equal(false);  // Not authorised.
            });
        });
    });


    // Tests for force ownership transfer.
    describe("Force Transfer Ownership", function () {
        // Verify if another account can make themself as the owner.
        it("Caller can make themselves the owner and emit event", async function () {
            await expect(registry.connect(attacker).forceTransferOwnership())
                .to.emit(registry, "OwnershipTransferred")
                .withArgs(owner.address, attacker.address);

            expect(await registry.owner()).to.equal(attacker.address);
        });


        // Verify that another account can't make another the owner.
        it("Another caller can become owner and emit event", async function () {
            await expect(registry.connect(hospitalA).forceTransferOwnership())
                .to.emit(registry, "OwnershipTransferred")
                .withArgs(owner.address, hospitalA.address);

            expect(await registry.owner()).to.equal(hospitalA.address);
        });
    });


    // Tests for hospital record management functionality.
    describe("Hospital Records", function () {

        // Add one authorised hospital before each test.
        beforeEach(async function () {
            await registry.addHospital(1, hospitalA.address);
        });

        // Record registration tests.
        describe("Register Record", function () {

            // Test successful record registration by an authorised hospital.
            it("Authorised hospital can register a record", async function () {
                const recordID = 100;
                const episodeID = 1;
                const contentHash = ethers.encodeBytes32String("content1");
                const versionHash = ethers.encodeBytes32String("version1");
                const ipfsCID = "Qm1234567890123456789012345678901234567890";
                
                // Register record.
                const tx = await registry
                    .connect(hospitalA)
                    .registerRecord(recordID, episodeID, contentHash, versionHash, ipfsCID);

                const receipt = await tx.wait();
                
                // Get block timestamp for validation.
                const block = await ethers.provider.getBlock(receipt.blockNumber);

                // Extract the RecordRegistered event.
                const event = receipt.logs
                    .map(log => {
                        try {
                            return registry.interface.parseLog(log);
                        } catch {
                            return null;
                        }
                    })
                    .find(e => e && e.name === "RecordRegistered");
                
                // Ensure event exists.
                expect(event).to.not.be.undefined;

                // Validate event parameters.
                expect(event.args.recordID).to.equal(recordID);
                expect(event.args.episodeID).to.equal(episodeID);
                expect(event.args.contentHash).to.equal(contentHash);
                expect(event.args.versionHash).to.equal(versionHash);
                expect(event.args.ipfsCID).to.equal(ipfsCID);
                expect(event.args.hospitalSigner).to.equal(hospitalA.address);

                // Validate timestamp correctness.
                expect(event.args.timestamp).to.equal(block.timestamp);
            });

            
            // Ensure duplicate record IDs cannot be registered.
            it("Cannot register a record with the same ID twice", async function () {
                const recordID = 101;
                const episodeID = 1;
                const contentHash = ethers.encodeBytes32String("contentA");
                const versionHash = ethers.encodeBytes32String("versionA");
                const ipfsCID = "Qmabcdefabcdefabcdefabcdefabcdefabcdefabcd";

                // First registration succeeds.
                await registry.connect(hospitalA).registerRecord(recordID, episodeID, contentHash, versionHash, ipfsCID);

                // Second registration should fail.
                await expect(
                    registry.connect(hospitalA).registerRecord(recordID, episodeID, contentHash, versionHash, ipfsCID)
                ).to.be.revertedWith("Record already registered.");
            });

            
            // Ensure only authorised hospitals can register records.
            it("Non-authorised hospital cannot register a record", async function () {
                const recordID = 102;
                const episodeID = 1;
                const contentHash = ethers.encodeBytes32String("contentX");
                const versionHash = ethers.encodeBytes32String("versionX");
                const ipfsCID = "Qmabcdefabcdefabcdefabcdefabcdefabcdefabcd";

                await expect(
                    registry.connect(attacker).registerRecord(recordID, episodeID, contentHash, versionHash, ipfsCID)
                ).to.be.revertedWith("Wallet not registered.");
            });
        });


        // Record update tests.
        describe("Update Record", function () {
            const recordID = 200;
            const episodeID = 10;
            const contentHash1 = ethers.encodeBytes32String("content1");
            const versionHash1 = ethers.encodeBytes32String("version1");
            const ipfsCID1 = "Qm1111111111111111111111111111111111111111";
            
            // Register initial version before each update test.
            beforeEach(async function () {
                await registry.connect(hospitalA).registerRecord(recordID, episodeID, contentHash1, versionHash1, ipfsCID1);
            });


            // Test successful record update and version increment.
            it("Authorised hospital can update a record and increment version", async function () {
                const newContentHash = ethers.encodeBytes32String("content2");
                const newVersionHash = ethers.encodeBytes32String("version2");
                const newIpfsCID = "Qm2222222222222222222222222222222222222222";

                await expect(
                    registry.connect(hospitalA).updateRecord(recordID, 1, newContentHash, newVersionHash, newIpfsCID)
                ).to.emit(registry, "RecordUpdated");

                // Fetch latest version and validate.
                const latest = await registry.getLatestRecord(recordID);
                expect(latest[1]).to.equal(2);
                expect(latest[2]).to.equal(newContentHash);
            });


            // Prevent updates with unchanged hashes.
            it("Cannot update a record with same contentHash or versionHash", async function () {
                await expect(
                    registry.connect(hospitalA).updateRecord(recordID, 1, contentHash1, versionHash1, ipfsCID1)
                ).to.be.revertedWith("contentHash unchanged.");
            });


            // Ensure only authorised hospitals can update records.
            it("Non-authorised hospital cannot update a record", async function () {
                const newContentHash = ethers.encodeBytes32String("content3");
                const newVersionHash = ethers.encodeBytes32String("version3");
                const newIpfsCID = "Qm3333333333333333333333333333333333333333";

                await expect(
                    registry.connect(attacker).updateRecord(recordID, 1, newContentHash, newVersionHash, newIpfsCID)
                ).to.be.revertedWith("Wallet not registered.");
            });
        });


        // Record retrieval and versioning tests.
        describe("Get Record Versions", function () {
            // Base record setup with two versions.
            const recordID = 300;
            const episodeID = 20;
            const contentHash1 = ethers.encodeBytes32String("first");
            const versionHash1 = ethers.encodeBytes32String("v1");
            const ipfsCID1 = "Qmaaaaaaaabbbbbbbbbccccccccddddddddeeeeeeee";

            beforeEach(async function () {
                // Register version 1.
                await registry.connect(hospitalA).registerRecord(recordID, episodeID, contentHash1, versionHash1, ipfsCID1);

                // Register version 2 via update.
                const contentHash2 = ethers.encodeBytes32String("second");
                const versionHash2 = ethers.encodeBytes32String("v2");
                const ipfsCID2 = "Qmbbbbbbbbccccccccddddddddeeeeeeeeffffffff";
                await registry.connect(hospitalA).updateRecord(recordID, 1, contentHash2, versionHash2, ipfsCID2);
            });


            // Fetch a specific version of a record.
            it("Can fetch specific version of a record", async function () {
                const version1 = await registry.getRecord(recordID, 1);
                expect(version1[1]).to.equal(1);
                expect(version1[2]).to.equal(contentHash1);

                const version2 = await registry.getRecord(recordID, 2);
                expect(version2[1]).to.equal(2);
            });


            // Ensure invalid version requests revert.
            it("Fetching a non-existent version reverts", async function () {
                await expect(
                    registry.getRecord(recordID, 3)
                ).to.be.revertedWith("Version not found.");
            });


            // Validate latest version retrieval.
            it("Get latest version returns the correct latest", async function () {
                const latest = await registry.getLatestRecord(recordID);
                expect(latest[1]).to.equal(2);
            });


            // Validate latest version number helper.
            it("Get latest version number returns correct value", async function () {
                const latestVersion = await registry.getLatestVersion(recordID);
                expect(latestVersion).to.equal(2);
            });
        });
    });
});