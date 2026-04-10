// Import dependencies.
using IntegrityVault.Service.Interfaces; // Get the interfaces for blockchain-related services.
using Nethereum.Web3; // Web3 provider for Ethereum blockchain interactions.
using Nethereum.Web3.Accounts; // For account creation and signing transactions
using IntegrityVault.Common.DTOs; // Get access to the blockchain specific DTOs.
using IntegrityVault.Repository.Interfaces; // Interfaces for user repository operations.
using System.Numerics; // BigInteger type for blockchain numeric values.
using Microsoft.Extensions.Options; // Get the IOptions.
using IntegrityVault.Common.Configurations; // Contains strongly-typed configuration classes.
using IntegrityVault.Common.ABIs; // Contains smart contract ABIs and function message definitions used by Nethereum.



// Declaring the namespace where this service implementation resides.
namespace IntegrityVault.Service.Implementations
{
    // Define the BlockchainService class and injecting the IUserRepository, ICryptoService
    public  class BlockchainService (IOptions<BlockchainSettings> blockchainOptions, IUserRepository _userRepository, IHospitalRepository _hospitalRepository, ICryptoService _cryptoService) : IBlockchainService
    {
        private readonly string _rpcUrl = blockchainOptions.Value.RPC_URL; // Ethereum RPC endpoint.
        private readonly string _contractAddress = blockchainOptions.Value.ContractAddress; // Deployed smart contract address.
        private readonly string _superAdminWalletAddress = blockchainOptions.Value.SuperAdminWalletAddress; // Add of the owner of the superadmin.


        // Add a hospital record to the blockchain.
        public async Task AddHospitalToChainAsync(int hospitalId, string walletAddress)
        {
            try
            {
                var web3 = await BuildSuperAdminWeb3Async(); // Build Web3 instance with superadmin account.

                // Create a typed function message.
                var addHospitalFunction = new AddHospitalFunction
                {
                    HospitalId = (BigInteger)hospitalId,
                    Wallet = walletAddress
                };

                // Create a handler for sending typed function messages.
                var handler = web3.Eth.GetContractTransactionHandler<AddHospitalFunction>();

                // Estimate gas.
                addHospitalFunction.Gas = await EstimateGasWithBufferAsync(handler, _contractAddress, addHospitalFunction);

                // Ensure sufficient balance.
                await EnsureSufficientBalanceAsync(web3, new Nethereum.Hex.HexTypes.HexBigInteger(addHospitalFunction.Gas.Value));

                // Send transaction and wait for receipt.
                var receipt = await handler.SendRequestAndWaitForReceiptAsync(_contractAddress, addHospitalFunction);

                if (receipt.Status.Value != 1)
                    throw new InvalidOperationException($"addHospital transaction failed. TxHash: {receipt.TransactionHash}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Blockchain error adding hospital {hospitalId}: {ex.Message}");
            }
        }


        // Update a hospital's wallet address on the blockchain.
        public async Task UpdateHospitalWalletOnChainAsync(int hospitalId, string NewWallet)
        {
            try
            {
                var web3 = await BuildSuperAdminWeb3Async(); // Build Web3 instance with superadmin account.

                // Create a typed function message.
                var updateHospitalFunction = new UpdateHospitalWalletFunction
                {
                    HospitalId = (BigInteger)hospitalId,
                    NewWallet = NewWallet
                };

                // Create a handler for sending typed function messages.
                var handler = web3.Eth.GetContractTransactionHandler<UpdateHospitalWalletFunction>();

                // Estimate gas.
                updateHospitalFunction.Gas = await EstimateGasWithBufferAsync(handler, _contractAddress, updateHospitalFunction);

                // Ensure sufficient balance.
                await EnsureSufficientBalanceAsync(web3, new Nethereum.Hex.HexTypes.HexBigInteger(updateHospitalFunction.Gas.Value));

                // Send transaction and wait for receipt.
                var receipt = await handler.SendRequestAndWaitForReceiptAsync(_contractAddress, updateHospitalFunction);

                if (receipt.Status.Value != 1)
                    throw new InvalidOperationException($"updateHospitalWallet transaction failed. TxHash: {receipt.TransactionHash}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Blockchain error updating hospital {hospitalId}: {ex.Message}");
            }
        }


        // Remove a hospital wallet form the chain.
        public async Task DeleteHospitalWalletFromChainAsync(int hospitalId)
        {
            try
            {
                var web3 = await BuildSuperAdminWeb3Async(); // Build Web3 instance with superadmin account.

                // Create a typed function message.
                var deleteHospitalFunction = new RemoveHospitalFunction
                {
                    HospitalId = (BigInteger)hospitalId,
                };

                // Create a handler for sending typed function messages.
                var handler = web3.Eth.GetContractTransactionHandler<RemoveHospitalFunction>();

                // Estimate gas.
                deleteHospitalFunction.Gas = await EstimateGasWithBufferAsync(handler, _contractAddress, deleteHospitalFunction);

                // Ensure sufficient balance.
                await EnsureSufficientBalanceAsync(web3, new Nethereum.Hex.HexTypes.HexBigInteger(deleteHospitalFunction.Gas.Value));

                // Send transaction and wait for receipt.
                var receipt = await handler.SendRequestAndWaitForReceiptAsync(_contractAddress, deleteHospitalFunction);

                if (receipt.Status.Value != 1)
                    throw new InvalidOperationException($"updateHospitalWallet transaction failed. TxHash: {receipt.TransactionHash}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Blockchain error deleting hospital {hospitalId}: {ex.Message}");
            }
        }


        // Add a record to the chain.
        public async Task<string> RegisterRecordOnChainAsync(int hospitalId, int recordId,
            int episodeId, byte[] contentHash, byte[] versionHash, string ipfsCid)
        {
            try
            {
                // Build Web3 instance for the hospital doing the action.
                var web3 = await BuildHospitalWeb3Async(hospitalId);

                // Create a typed function message.
                var registerRecordFunction = new RegisterRecordFunction
                {
                    RecordID = (BigInteger)recordId,
                    EpisodeID = (BigInteger)episodeId,
                    ContentHash = contentHash,
                    VersionHash = versionHash,
                    IpfsCID = ipfsCid
                };

                // Create a handler for sending typed function messages.
                var handler = web3.Eth.GetContractTransactionHandler<RegisterRecordFunction>();

                // Estimate gas.
                registerRecordFunction.Gas = await EstimateGasWithBufferAsync(handler, _contractAddress, registerRecordFunction);

                // Ensure sufficient balance.
                await EnsureSufficientBalanceAsync(web3, new Nethereum.Hex.HexTypes.HexBigInteger(registerRecordFunction.Gas.Value));

                // Send transaction and wait for receipt.
                var receipt = await handler.SendRequestAndWaitForReceiptAsync(_contractAddress, registerRecordFunction);

                if (receipt.Status.Value != 1)
                    throw new InvalidOperationException($"registerRecord transaction failed. TxHash: {receipt.TransactionHash}");

                return receipt.TransactionHash;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Blockchain error registering record {recordId}: {ex.Message}");
            }
        }


        // Add an updated version of an existing medical record on-chain.
        public async Task<string> UpdateRecordOnChainAsync(int hospitalId, int recordId,
            int currentVersion, byte[] newContentHash, byte[] newVersionHash, string newIpfsCid)
        {
            try
            {
                // Build Web3 instance for the hospital doing the action.
                var web3 = await BuildHospitalWeb3Async(hospitalId);

                // Create a typed function message.
                var updateRecordFunction = new UpdateRecordFunction
                {
                    RecordID = (BigInteger)recordId,
                    ExpectedCurrentVersion = (BigInteger)currentVersion,
                    NewContentHash = newContentHash,
                    NewVersionHash = newVersionHash,
                    NewIpfsCID = newIpfsCid
                };

                // Create a handler for sending typed function messages.
                var handler = web3.Eth.GetContractTransactionHandler<UpdateRecordFunction>();

                // Estimate gas.
                updateRecordFunction.Gas = await EstimateGasWithBufferAsync(handler, _contractAddress, updateRecordFunction);

                // Ensure sufficient balance.
                await EnsureSufficientBalanceAsync(web3, new Nethereum.Hex.HexTypes.HexBigInteger(updateRecordFunction.Gas.Value));

                // Send transaction and wait for receipt.
                var receipt = await handler.SendRequestAndWaitForReceiptAsync(_contractAddress, updateRecordFunction);

                if (receipt.Status.Value != 1)
                    throw new InvalidOperationException($"updateRecord transaction failed. TxHash: {receipt.TransactionHash}");

                return receipt.TransactionHash;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Blockchain error updating record {recordId}: {ex.Message}");
            }
        }


        // Retrieve a specific version of a record from the blockchain.
        public async Task<RecordEntryOutput> GetRecordFromChainAsync(int recordId, int version)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_rpcUrl))
                    throw new InvalidOperationException("RPC URL is not configured.");

                if (string.IsNullOrWhiteSpace(_contractAddress))
                    throw new InvalidOperationException("Contract address is not configured.");

                var web3 = new Web3(_rpcUrl);

                await web3.Net.Version.SendRequestAsync();


                var function = web3.Eth.GetContract(HospitalManagementABI.Value, _contractAddress).GetFunction("getRecord");
                var result = await function.CallDeserializingToObjectAsync<RecordEntryOutput>(
                    (BigInteger)recordId,
                    (BigInteger)version
                );
                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Blockchain error reading record {recordId} v{version}: {ex.Message}");
            }
        }


        // Retrieve the latest anchored version of a record from the blockchain.
        public async Task<RecordEntryOutput> GetLatestRecordFromChainAsync(int recordId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_rpcUrl))
                    throw new InvalidOperationException("RPC URL is not configured.");

                if (string.IsNullOrWhiteSpace(_contractAddress))
                    throw new InvalidOperationException("Contract address is not configured.");

                var web3 = new Web3(_rpcUrl);

                await web3.Net.Version.SendRequestAsync();

                var function = web3.Eth.GetContract(HospitalManagementABI.Value, _contractAddress).GetFunction("getLatestRecord");
                var result = await function.CallDeserializingToObjectAsync<RecordEntryOutput>(
                    (BigInteger)recordId
                );
                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Blockchain error reading latest record {recordId}: {ex.Message}");
            }
        }



        // Helper method to build a Web3 instance using superadmin account.
        private async Task<Web3> BuildSuperAdminWeb3Async()
        {
            // Get the superadmin owner record.
            var superAdmin = await _userRepository.GetSuperAdminByWalletAsync(_superAdminWalletAddress) ?? throw new InvalidOperationException("Superadmin owner record not found in database.");

            // Decrypt the stored private key.
            string privateKey = _cryptoService.Decrypt(superAdmin.EncryptedPrivateKey); // Decrypt superadmin private key.

            // Build an Nethereum Account and authenticated Web3 instance.
            var account = new Account(privateKey); // Create Nethereum account from private key.
            return new Web3(account, _rpcUrl); // Return authenticated Web3 instance.
        }


        // Helper method to build a Web3 instance using a hospital account.
        private async Task<Web3> BuildHospitalWeb3Async(int hospitalId)
        {
            // Get the hospital record.
            var hospital = await _hospitalRepository.GetHospitalByIdAsync(hospitalId) 
                ?? throw new InvalidOperationException($"Hospital {hospitalId} not found in database.");

            // Decrypt the stored private key.
            string privateKey = _cryptoService.Decrypt(hospital.EncryptedPrivateKey);

            // Build an Nethereum Account and authenticated Web3 instance.
            var account = new Account(privateKey); // Create Nethereum account from private key.
            return new Web3(account, _rpcUrl); // Return authenticated Web3 instance.
        }


        // Helper method to estimate gas with a 20% buffer.
        private static async Task<Nethereum.Hex.HexTypes.HexBigInteger> EstimateGasWithBufferAsync<TFunction>(
            Nethereum.Contracts.ContractHandlers.IContractTransactionHandler<TFunction> handler,
            string contractAddress,
            TFunction functionMessage)
            where TFunction : Nethereum.Contracts.FunctionMessage, new()
        {
            var gas = await handler.EstimateGasAsync(contractAddress, functionMessage);

            // Add 20% buffer
            var bufferedGas = gas.Value * 120 / 100;

            return new Nethereum.Hex.HexTypes.HexBigInteger(bufferedGas);
        }


        // Helper method to check if the superadmin has sufficient balance to cover gas.
        private static async Task EnsureSufficientBalanceAsync(Web3 web3, Nethereum.Hex.HexTypes.HexBigInteger estimatedGas)
        {
            // Get current balance of the superadmin wallet.
            var balance = await web3.Eth.GetBalance.SendRequestAsync(
                web3.TransactionManager.Account.Address);

            // Get current gas price from the network.
            var gasPrice = await web3.Eth.GasPrice.SendRequestAsync();

            // Calculate total cost.
            var totalCost = estimatedGas.Value * gasPrice.Value;

            if (balance.Value < totalCost)
            {
                throw new InvalidOperationException(
                    $"Insufficient wallet balance. " +
                    $"Required: {Nethereum.Web3.Web3.Convert.FromWei(totalCost)} ETH, " +
                    $"Available: {Nethereum.Web3.Web3.Convert.FromWei(balance.Value)} ETH.");
            }
        }
    }
}
