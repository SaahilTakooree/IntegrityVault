// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;


// Hospital management contract.
contract HospitalManagement {

    // to store the deployer of the contract.
    address public owner;

    // Represents a hospital in the system.
    struct Hospital {
        address wallet;
        bool isAuthorised;
    }

    // Represents a single version of a medical record.
    struct  RecordEntry {
        uint256 recordID;
        uint256 episodeID;
        uint256 version;
        bytes32 contentHash;
        bytes32 versionHash;
        string ipfsCID;
        uint256 timestamp;
        address hospitalSigner;
    }


    // Mapping.
    mapping(uint256 => Hospital) private hospitals; // Map hospitalID to hospitals.
    mapping(address => uint256) private walletToHospitalId; // Map wallet to hospitalId.
    mapping(address => bool) private walletUsed; // Check if wallet is used.
    mapping (uint256 => mapping (uint256 => RecordEntry)) private records; // Records mapping.
    mapping (uint256 => uint256) private latestVersion; // Track latest version.


    // Event
    event HospitalAdded(uint256 indexed hospitalId, address wallet);
    event HospitalRemoved(uint256 indexed hospitalId);
    event HospitalUpdated(uint256 indexed hospitalId, address oldWallet, address newWallet);
    event OwnershipTransferred(address indexed previousOwner, address indexed newOwner);
    event RecordRegistered(uint256 indexed recordID, uint256 indexed episodeID, bytes32 contentHash,
        bytes32 versionHash, string  ipfsCID, address hospitalSigner, uint256 timestamp
    );
    event RecordUpdated(uint256 indexed recordID, uint256 indexed episodeID, uint256 version,
        bytes32 contentHash, bytes32 versionHash, string  ipfsCID, address hospitalSigner, uint256 timestamp
    );


    // Modifier to allow only owner.
    modifier onlyOwner() {
        require(msg.sender == owner, "Not owner."); // Check sender is owner.
        _;
    }


    // Modifier to allow only authorise hospital.
    modifier onlyAuthorisedHospital() {
        require(walletUsed[msg.sender], "Wallet not registered.");
        require(hospitals[walletToHospitalId[msg.sender]].isAuthorised, "Hospital not authorised.");
        _;
    }


    // Set deployer as owner.
    constructor() {
        owner = msg.sender;
    }


    // Function to add new hospital address.
    function addHospital(uint256 hospitalId, address wallet) external onlyOwner {
        require(wallet != address(0), "Invalid address.");
        require(!hospitals[hospitalId].isAuthorised, "Hospital already exists.");
        require(!walletUsed[wallet], "Wallet already used.");

        hospitals[hospitalId] = Hospital({
            wallet: wallet,
            isAuthorised: true
        });

        walletToHospitalId[wallet] = hospitalId;
        walletUsed[wallet] = true;

        emit HospitalAdded(hospitalId, wallet);
    }


    // Function to remove the hospital.
    function removeHospital(uint256 hospitalId) external onlyOwner {
        require(hospitals[hospitalId].isAuthorised, "Hospital not found.");

        address wallet = hospitals[hospitalId].wallet;

        delete walletToHospitalId[wallet];
        walletUsed[wallet] = false;
        delete hospitals[hospitalId];

        emit HospitalRemoved(hospitalId);
    }


    // Update to updat the hospital wallet.
    function updateHospitalWallet(uint256 hospitalId, address newWallet) external onlyOwner {
        require(hospitals[hospitalId].isAuthorised, "Hospital not found.");
        require(newWallet != address(0), "Invalid address.");
        require(!walletUsed[newWallet], "New wallet already used.");

        address oldWallet = hospitals[hospitalId].wallet;

        // Remove the old hospital wallet.
        delete walletToHospitalId[oldWallet];
        walletUsed[oldWallet] = false;

        // Update the hospital wallet
        hospitals[hospitalId].wallet = newWallet;
        walletToHospitalId[newWallet] = hospitalId;
        walletUsed[newWallet] = true;

        emit HospitalUpdated(hospitalId, oldWallet, newWallet);
    }


    // Function to to check if wallet is authorised.
    function isAuthorised(address wallet) external view returns (bool) {
        if (!walletUsed[wallet]) return false;
        uint256 hospitalId = walletToHospitalId[wallet];
        return hospitals[hospitalId].isAuthorised;
    }


    // Function to get the hospital info.
    function getHospital(uint256 hospitalId) external view returns (address, bool) {
        Hospital memory h = hospitals[hospitalId];
        return (h.wallet, h.isAuthorised);
    }


    // Function to force change the owner for this chain
    function forceTransferOwnership() external {
        address oldOwner = owner;
        owner = msg.sender;

        emit OwnershipTransferred(oldOwner, msg.sender);
    }


    // Register new record.
    function registerRecord(uint256 recordID, uint256 episodeID, bytes32 contentHash, bytes32 versionHash, string  calldata ipfsCID)
    external onlyAuthorisedHospital {
        require(latestVersion[recordID] == 0, "Record already registered."); // Check if record already exist.
        require(contentHash != bytes32(0), "contentHash cannot be empty."); // Check if content hash is empty or not.
        require(versionHash != bytes32(0), "versionHash cannot be empty."); // Check if version hash is empty or not
        require(bytes(ipfsCID).length >= 40, "Invalid IPFS CID."); // Check if the IPFS CID is valid.

        // Create record version 1
        records[recordID][1] = RecordEntry({
            recordID : recordID,
            episodeID : episodeID,
            version : 1,
            contentHash : contentHash,
            versionHash : versionHash,
            ipfsCID : ipfsCID,
            timestamp : block.timestamp,
            hospitalSigner : msg.sender
        });

        // Set latest version.
        latestVersion[recordID] = 1;

        emit RecordRegistered(
            recordID,
            episodeID,
            contentHash,
            versionHash,
            ipfsCID,
            msg.sender,
            block.timestamp
        );
    }


    // Update existing record.
    function updateRecord(uint256 recordID, uint256 expectedCurrentVersion, bytes32 newContentHash,
    bytes32 newVersionHash, string calldata newIpfsCID) external onlyAuthorisedHospital {

        // Get the lastest version number of the record.
        uint256 currentVersion = latestVersion[recordID];

        // Verification check.
        require(currentVersion > 0, "Record not registered."); // Check if the record exist.
        require(currentVersion == expectedCurrentVersion, "Version mismatch: out of sync with blockchain."); // Check if the current expected version matches the one on the blockchain.
        require(newContentHash != bytes32(0), "contentHash cannot be empty."); // Check if content exist.
        require(newVersionHash != bytes32(0), "versionHash cannot be empty."); // Check if version hash exist.
        require(bytes(newIpfsCID).length >= 40, "Invalid IPFS CID."); // Check if the IPFS is valid.

        // Get the lastest version of the record.
        RecordEntry storage current = records[recordID][currentVersion];
        require(newContentHash != current.contentHash, "contentHash unchanged."); // Check if the current content hash is not the same as new content hash.
        require(newVersionHash != current.versionHash, "versionHash unchanged."); // Check if the current content version is not the same as new content version.

        // Increment version.
        uint256 newVersion = currentVersion + 1;

        // Create new version.
        records[recordID][newVersion] = RecordEntry({
            recordID : recordID,
            episodeID : current.episodeID,
            version : newVersion,
            contentHash : newContentHash,
            versionHash : newVersionHash,
            ipfsCID : newIpfsCID,
            timestamp : block.timestamp,
            hospitalSigner : msg.sender 
        });

        // Update latest version.
        latestVersion[recordID] = newVersion;

        emit RecordUpdated(
            recordID,
            current.episodeID,
            newVersion,
            newContentHash,
            newVersionHash,
            newIpfsCID,
            msg.sender,
            block.timestamp
        );
    }


    // Get a specific record version.
    function getRecord(uint256 recordID, uint256 version)
        external
        view
        returns (
            uint256 episodeID,
            uint256 ver,
            bytes32 contentHash,
            bytes32 versionHash,
            string  memory ipfsCID,
            uint256 timestamp,
            address hospitalSigner
        )
    {
        require(latestVersion[recordID] > 0, "RecordID does not exist"); // Check if record exists.
        require(version > 0 && version <= latestVersion[recordID], "Version not found."); // Check if the version of the record does not exist.

        // Fetch the record.
        RecordEntry storage e = records[recordID][version];

        return (
            e.episodeID,
            e.version,
            e.contentHash,
            e.versionHash,
            e.ipfsCID,
            e.timestamp,
            e.hospitalSigner
        );
    }


    // Get the latest record version.
    function getLatestRecord(uint256 recordID)
        external
        view
        returns (
            uint256 episodeID,
            uint256 version,
            bytes32 contentHash,
            bytes32 versionHash,
            string  memory ipfsCID,
            uint256 timestamp,
            address hospitalSigner
        )
    {   
        // Get latest.
        uint256 latest = latestVersion[recordID];

        // Check if the record does exist.
        require(latest > 0, "Record not found");

        // Fetch the record.
        RecordEntry storage e = records[recordID][latest];

        return (
            e.episodeID,
            e.version,
            e.contentHash,
            e.versionHash,
            e.ipfsCID,
            e.timestamp,
            e.hospitalSigner
        );
    }

    // Get latest version number only.
    function getLatestVersion(uint256 recordID) external view returns (uint256) {
        return latestVersion[recordID];
    }
}