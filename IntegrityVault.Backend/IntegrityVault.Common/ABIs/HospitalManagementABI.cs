// Declaring the namespace for ABI definitions.
namespace IntegrityVault.Common.ABIs
{
    // Static class holding the ABI for the HospitalManagement smart contract.
    public static class HospitalManagementABI
    {
        // JSON string representing the smart contract ABI.
        public static readonly string Value = """
        [
          {
            "inputs": [],
            "stateMutability": "nonpayable",
            "type": "constructor"
          },
          {
            "anonymous": false,
            "inputs": [
              {
                "indexed": true,
                "internalType": "uint256",
                "name": "hospitalId",
                "type": "uint256"
              },
              {
                "indexed": false,
                "internalType": "address",
                "name": "wallet",
                "type": "address"
              }
            ],
            "name": "HospitalAdded",
            "type": "event"
          },
          {
            "anonymous": false,
            "inputs": [
              {
                "indexed": true,
                "internalType": "uint256",
                "name": "hospitalId",
                "type": "uint256"
              }
            ],
            "name": "HospitalRemoved",
            "type": "event"
          },
          {
            "anonymous": false,
            "inputs": [
              {
                "indexed": true,
                "internalType": "uint256",
                "name": "hospitalId",
                "type": "uint256"
              },
              {
                "indexed": false,
                "internalType": "address",
                "name": "oldWallet",
                "type": "address"
              },
              {
                "indexed": false,
                "internalType": "address",
                "name": "newWallet",
                "type": "address"
              }
            ],
            "name": "HospitalUpdated",
            "type": "event"
          },
          {
            "anonymous": false,
            "inputs": [
              {
                "indexed": true,
                "internalType": "address",
                "name": "previousOwner",
                "type": "address"
              },
              {
                "indexed": true,
                "internalType": "address",
                "name": "newOwner",
                "type": "address"
              }
            ],
            "name": "OwnershipTransferred",
            "type": "event"
          },
          {
            "anonymous": false,
            "inputs": [
              {
                "indexed": true,
                "internalType": "uint256",
                "name": "recordID",
                "type": "uint256"
              },
              {
                "indexed": true,
                "internalType": "uint256",
                "name": "episodeID",
                "type": "uint256"
              },
              {
                "indexed": false,
                "internalType": "bytes32",
                "name": "contentHash",
                "type": "bytes32"
              },
              {
                "indexed": false,
                "internalType": "bytes32",
                "name": "versionHash",
                "type": "bytes32"
              },
              {
                "indexed": false,
                "internalType": "string",
                "name": "ipfsCID",
                "type": "string"
              },
              {
                "indexed": false,
                "internalType": "address",
                "name": "hospitalSigner",
                "type": "address"
              },
              {
                "indexed": false,
                "internalType": "uint256",
                "name": "timestamp",
                "type": "uint256"
              }
            ],
            "name": "RecordRegistered",
            "type": "event"
          },
          {
            "anonymous": false,
            "inputs": [
              {
                "indexed": true,
                "internalType": "uint256",
                "name": "recordID",
                "type": "uint256"
              },
              {
                "indexed": true,
                "internalType": "uint256",
                "name": "episodeID",
                "type": "uint256"
              },
              {
                "indexed": false,
                "internalType": "uint256",
                "name": "version",
                "type": "uint256"
              },
              {
                "indexed": false,
                "internalType": "bytes32",
                "name": "contentHash",
                "type": "bytes32"
              },
              {
                "indexed": false,
                "internalType": "bytes32",
                "name": "versionHash",
                "type": "bytes32"
              },
              {
                "indexed": false,
                "internalType": "string",
                "name": "ipfsCID",
                "type": "string"
              },
              {
                "indexed": false,
                "internalType": "address",
                "name": "hospitalSigner",
                "type": "address"
              },
              {
                "indexed": false,
                "internalType": "uint256",
                "name": "timestamp",
                "type": "uint256"
              }
            ],
            "name": "RecordUpdated",
            "type": "event"
          },
          {
            "inputs": [
              {
                "internalType": "uint256",
                "name": "hospitalId",
                "type": "uint256"
              },
              {
                "internalType": "address",
                "name": "wallet",
                "type": "address"
              }
            ],
            "name": "addHospital",
            "outputs": [],
            "stateMutability": "nonpayable",
            "type": "function"
          },
          {
            "inputs": [],
            "name": "forceTransferOwnership",
            "outputs": [],
            "stateMutability": "nonpayable",
            "type": "function"
          },
          {
            "inputs": [
              {
                "internalType": "uint256",
                "name": "hospitalId",
                "type": "uint256"
              }
            ],
            "name": "getHospital",
            "outputs": [
              {
                "internalType": "address",
                "name": "",
                "type": "address"
              },
              {
                "internalType": "bool",
                "name": "",
                "type": "bool"
              }
            ],
            "stateMutability": "view",
            "type": "function"
          },
          {
            "inputs": [
              {
                "internalType": "uint256",
                "name": "recordID",
                "type": "uint256"
              }
            ],
            "name": "getLatestRecord",
            "outputs": [
              {
                "internalType": "uint256",
                "name": "episodeID",
                "type": "uint256"
              },
              {
                "internalType": "uint256",
                "name": "version",
                "type": "uint256"
              },
              {
                "internalType": "bytes32",
                "name": "contentHash",
                "type": "bytes32"
              },
              {
                "internalType": "bytes32",
                "name": "versionHash",
                "type": "bytes32"
              },
              {
                "internalType": "string",
                "name": "ipfsCID",
                "type": "string"
              },
              {
                "internalType": "uint256",
                "name": "timestamp",
                "type": "uint256"
              },
              {
                "internalType": "address",
                "name": "hospitalSigner",
                "type": "address"
              }
            ],
            "stateMutability": "view",
            "type": "function"
          },
          {
            "inputs": [
              {
                "internalType": "uint256",
                "name": "recordID",
                "type": "uint256"
              }
            ],
            "name": "getLatestVersion",
            "outputs": [
              {
                "internalType": "uint256",
                "name": "",
                "type": "uint256"
              }
            ],
            "stateMutability": "view",
            "type": "function"
          },
          {
            "inputs": [
              {
                "internalType": "uint256",
                "name": "recordID",
                "type": "uint256"
              },
              {
                "internalType": "uint256",
                "name": "version",
                "type": "uint256"
              }
            ],
            "name": "getRecord",
            "outputs": [
              {
                "internalType": "uint256",
                "name": "episodeID",
                "type": "uint256"
              },
              {
                "internalType": "uint256",
                "name": "ver",
                "type": "uint256"
              },
              {
                "internalType": "bytes32",
                "name": "contentHash",
                "type": "bytes32"
              },
              {
                "internalType": "bytes32",
                "name": "versionHash",
                "type": "bytes32"
              },
              {
                "internalType": "string",
                "name": "ipfsCID",
                "type": "string"
              },
              {
                "internalType": "uint256",
                "name": "timestamp",
                "type": "uint256"
              },
              {
                "internalType": "address",
                "name": "hospitalSigner",
                "type": "address"
              }
            ],
            "stateMutability": "view",
            "type": "function"
          },
          {
            "inputs": [
              {
                "internalType": "address",
                "name": "wallet",
                "type": "address"
              }
            ],
            "name": "isAuthorised",
            "outputs": [
              {
                "internalType": "bool",
                "name": "",
                "type": "bool"
              }
            ],
            "stateMutability": "view",
            "type": "function"
          },
          {
            "inputs": [],
            "name": "owner",
            "outputs": [
              {
                "internalType": "address",
                "name": "",
                "type": "address"
              }
            ],
            "stateMutability": "view",
            "type": "function"
          },
          {
            "inputs": [
              {
                "internalType": "uint256",
                "name": "recordID",
                "type": "uint256"
              },
              {
                "internalType": "uint256",
                "name": "episodeID",
                "type": "uint256"
              },
              {
                "internalType": "bytes32",
                "name": "contentHash",
                "type": "bytes32"
              },
              {
                "internalType": "bytes32",
                "name": "versionHash",
                "type": "bytes32"
              },
              {
                "internalType": "string",
                "name": "ipfsCID",
                "type": "string"
              }
            ],
            "name": "registerRecord",
            "outputs": [],
            "stateMutability": "nonpayable",
            "type": "function"
          },
          {
            "inputs": [
              {
                "internalType": "uint256",
                "name": "hospitalId",
                "type": "uint256"
              }
            ],
            "name": "removeHospital",
            "outputs": [],
            "stateMutability": "nonpayable",
            "type": "function"
          },
          {
            "inputs": [
              {
                "internalType": "uint256",
                "name": "hospitalId",
                "type": "uint256"
              },
              {
                "internalType": "address",
                "name": "newWallet",
                "type": "address"
              }
            ],
            "name": "updateHospitalWallet",
            "outputs": [],
            "stateMutability": "nonpayable",
            "type": "function"
          },
          {
            "inputs": [
              {
                "internalType": "uint256",
                "name": "recordID",
                "type": "uint256"
              },
              {
                "internalType": "uint256",
                "name": "expectedCurrentVersion",
                "type": "uint256"
              },
              {
                "internalType": "bytes32",
                "name": "newContentHash",
                "type": "bytes32"
              },
              {
                "internalType": "bytes32",
                "name": "newVersionHash",
                "type": "bytes32"
              },
              {
                "internalType": "string",
                "name": "newIpfsCID",
                "type": "string"
              }
            ],
            "name": "updateRecord",
            "outputs": [],
            "stateMutability": "nonpayable",
            "type": "function"
          }
        ]
        """;
    }
}