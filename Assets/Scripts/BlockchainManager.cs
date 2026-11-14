
using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;
using Nethereum.Web3;
using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;

[Serializable]
public class BlockchainConfig
{
    public string rpcUrl;
}

[FunctionOutput]
public class ItemDetailsDTO : IFunctionOutputDTO
{
    [Parameter("string", "", 1)]
    public string ItemName { get; set; }

    [Parameter("string", "", 2)]
    public string ItemRarity { get; set; }

    [Parameter("string", "", 3)]
    public string ItemDescription { get; set; }
}

public class BlockchainManager : MonoBehaviour
{
    public static BlockchainManager Instance { get; private set; }

    [SerializeField] private string contractAddress = "";
    [SerializeField] private string playerWalletAddress = "";

    // Blockchain connection states
    public enum ConnectionStatus
    {
        Disconnected,
        Connecting,
        Ready,
        Error
    }

    public ConnectionStatus CurrentStatus { get; private set; }
        = ConnectionStatus.Disconnected;

    public string StatusMessage { get; private set; }
        = "Not connected";

    private Web3 web3;
    private Contract contract;
    private bool isReady = false;

    private void SetStatus(
        ConnectionStatus status,
        string message)
    {
        CurrentStatus = status;
        StatusMessage = message;

        Debug.Log("Blockchain status: " + message);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private async void Start()
    {
        await InitializeBlockchain();
    }

    private async Task InitializeBlockchain()
    {
        isReady = false;

        SetStatus(
            ConnectionStatus.Connecting,
            "Connecting to Sepolia..."
        );

        try
        {
            Debug.Log("Connecting to ChainQuestNFT...");

            // Load local RPC configuration
            string configPath = Path.Combine(
                Application.streamingAssetsPath,
                "blockchain-config.json"
            );

            if (!File.Exists(configPath))
            {
                SetStatus(
                    ConnectionStatus.Error,
                    "Blockchain config file not found"
                );

                Debug.LogError(
                    "Blockchain config file not found!"
                );
                return;
            }

            string json = File.ReadAllText(configPath);

            BlockchainConfig config =
                JsonUtility.FromJson<BlockchainConfig>(json);

            if (config == null ||
                string.IsNullOrWhiteSpace(config.rpcUrl))
            {
                SetStatus(
                    ConnectionStatus.Error,
                    "RPC URL is missing"
                );

                Debug.LogError("RPC URL is missing!");
                return;
            }

            // Validate contract and wallet addresses
            if (string.IsNullOrWhiteSpace(contractAddress) ||
                string.IsNullOrWhiteSpace(playerWalletAddress))
            {
                SetStatus(
                    ConnectionStatus.Error,
                    "Contract or wallet address is missing"
                );

                Debug.LogError(
                    "Contract or player wallet address is missing!"
                );
                return;
            }

            if (!Nethereum.Util.AddressUtil.Current
                .IsValidEthereumAddressHexFormat(contractAddress) ||
                !Nethereum.Util.AddressUtil.Current
                .IsValidEthereumAddressHexFormat(playerWalletAddress))
            {
                SetStatus(
                    ConnectionStatus.Error,
                    "Invalid Ethereum address"
                );

                Debug.LogError("Invalid Ethereum address!");
                return;
            }

            // Connect to Ethereum Sepolia
            web3 = new Web3(config.rpcUrl);
            Debug.Log("RPC client created. Requesting Chain ID...");

            var chainId = await web3.Eth.ChainId.SendRequestAsync();
            Debug.Log("Chain ID request completed.");

            Debug.Log("Connected Chain ID: " + chainId.Value);

            if (chainId.Value != 11155111)
            {
                SetStatus(
                    ConnectionStatus.Error,
                    "Wrong network: expected Sepolia"
                );

                Debug.LogError(
                    "Wrong network! Expected Sepolia."
                );
                return;
            }

            // Check that the smart contract exists
            string contractCode = await web3.Eth.GetCode
                .SendRequestAsync(contractAddress);

            if (string.IsNullOrEmpty(contractCode) ||
                contractCode == "0x")
            {
                SetStatus(
                    ConnectionStatus.Error,
                    "Contract not found on Sepolia"
                );

                Debug.LogError(
                    "Contract not found on Sepolia!"
                );
                return;
            }

            // Load smart contract ABI
            TextAsset abiFile =
                Resources.Load<TextAsset>("ChainQuestNFT_ABI");

            if (abiFile == null)
            {
                SetStatus(
                    ConnectionStatus.Error,
                    "Contract ABI file not found"
                );

                Debug.LogError("ABI file not found!");
                return;
            }

            // Initialize smart contract
            contract = web3.Eth.GetContract(
                abiFile.text,
                contractAddress
            );

            isReady = true;

            SetStatus(
                ConnectionStatus.Ready,
                "Connected to Sepolia"
            );

            Debug.Log(
                "Blockchain initialized successfully!"
            );

            // Read existing NFT data
            await ReadNFT(1);
            await ReadNFT(2);
        }
        catch (Exception ex)
        {
            isReady = false;

            SetStatus(
                ConnectionStatus.Error,
                "Failed to connect to Sepolia"
            );

            Debug.LogError(
                "Blockchain initialization error: " + ex
            );
        }
    }

    // Check whether the player owns a specific NFT
    public async Task<bool> IsNFTOwnedByPlayer(ulong tokenId)
    {
        if (!isReady || contract == null)
        {
            Debug.LogWarning("Blockchain is not ready!");
            return false;
        }

        try
        {
            var ownerFunction =
                contract.GetFunction("ownerOf");

            string owner = await ownerFunction
                .CallAsync<string>(new BigInteger(tokenId));

            bool isOwner = string.Equals(
                owner,
                playerWalletAddress,
                StringComparison.OrdinalIgnoreCase
            );

            Debug.Log(
                $"NFT #{tokenId}: Owner={owner}, " +
                $"Player owns NFT={isOwner}"
            );

            return isOwner;
        }
        catch (Exception ex)
        {
            Debug.LogError(
                $"NFT ownership check failed for #{tokenId}: {ex}"
            );

            return false;
        }
    }

    // Read NFT ownership and metadata
    public async Task ReadNFT(int tokenId)
    {
        if (!isReady || contract == null)
            return;

        try
        {
            var ownerFunction =
                contract.GetFunction("ownerOf");

            string owner = await ownerFunction
                .CallAsync<string>(new BigInteger(tokenId));

            var detailsFunction =
                contract.GetFunction("getItemDetails");

            var details = await detailsFunction
                .CallDeserializingToObjectAsync<ItemDetailsDTO>(
                    new BigInteger(tokenId)
                );

            Debug.Log($"NFT #{tokenId} Owner: {owner}");

            Debug.Log(
                $"NFT #{tokenId}: {details.ItemName}, " +
                $"Rarity: {details.ItemRarity}, " +
                $"Description: {details.ItemDescription}"
            );
        }
        catch (Exception ex)
        {
            Debug.LogError(
                $"Error reading NFT #{tokenId}: {ex}"
            );
        }
    }

    // Load a GameItem directly from the blockchain
    public async Task<GameItem> GetGameItemFromBlockchain(
        string itemId,
        ulong tokenId)
    {
        if (!isReady || contract == null)
        {
            Debug.LogWarning("Blockchain is not ready!");
            return null;
        }

        try
        {
            var ownerFunction =
                contract.GetFunction("ownerOf");

            string owner = await ownerFunction
                .CallAsync<string>(new BigInteger(tokenId));

            // Verify that the NFT belongs to the player
            if (!string.Equals(
                owner,
                playerWalletAddress,
                StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogWarning(
                    $"Player does not own NFT #{tokenId}"
                );

                return null;
            }

            // Retrieve NFT metadata from Sepolia
            var detailsFunction =
                contract.GetFunction("getItemDetails");

            var details = await detailsFunction
                .CallDeserializingToObjectAsync<ItemDetailsDTO>(
                    new BigInteger(tokenId)
                );

            return new GameItem(
                itemId,
                tokenId,
                details.ItemName,
                details.ItemRarity,
                details.ItemDescription,
                owner
            );
        }
        catch (Exception ex)
        {
            Debug.LogError(
                $"Failed to load NFT #{tokenId}: {ex}"
            );

            return null;
        }
    }


    public async void RetryConnection()
    {
        if (CurrentStatus == ConnectionStatus.Connecting)
        {
            Debug.LogWarning(
                "Connection attempt is already in progress."
            );
            return;
        }

        Debug.Log("Retry requested by player.");

        isReady = false;
        contract = null;
        web3 = null;

        Debug.Log("Starting blockchain reinitialization...");

        await InitializeBlockchain();

        Debug.Log(
            "Retry finished. Status: " + CurrentStatus
        );
    }





}
