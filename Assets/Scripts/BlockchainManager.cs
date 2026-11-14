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

    private Web3 web3;
    private Contract contract;
    private bool isReady = false;

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
        try
        {
            Debug.Log("Connecting to ChainQuestNFT...");

            string configPath = Path.Combine(
                Application.streamingAssetsPath,
                "blockchain-config.json"
            );

            if (!File.Exists(configPath))
            {
                Debug.LogError("Blockchain config file not found!");
                return;
            }

            string json = File.ReadAllText(configPath);
            BlockchainConfig config =
                JsonUtility.FromJson<BlockchainConfig>(json);

            if (config == null ||
                string.IsNullOrWhiteSpace(config.rpcUrl))
            {
                Debug.LogError("RPC URL is missing!");
                return;
            }

            if (string.IsNullOrWhiteSpace(contractAddress) ||
                string.IsNullOrWhiteSpace(playerWalletAddress))
            {
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
                Debug.LogError("Invalid Ethereum address!");
                return;
            }

            web3 = new Web3(config.rpcUrl);

            var chainId = await web3.Eth.ChainId.SendRequestAsync();

            Debug.Log("Connected Chain ID: " + chainId.Value);

            if (chainId.Value != 11155111)
            {
                Debug.LogError("Wrong network! Expected Sepolia.");
                return;
            }

            string contractCode = await web3.Eth.GetCode
                .SendRequestAsync(contractAddress);

            if (string.IsNullOrEmpty(contractCode) ||
                contractCode == "0x")
            {
                Debug.LogError("Contract not found on Sepolia!");
                return;
            }

            TextAsset abiFile =
                Resources.Load<TextAsset>("ChainQuestNFT_ABI");

            if (abiFile == null)
            {
                Debug.LogError("ABI file not found!");
                return;
            }

            contract = web3.Eth.GetContract(
                abiFile.text,
                contractAddress
            );

            isReady = true;

            Debug.Log("Blockchain initialized successfully!");

            await ReadNFT(1);
            await ReadNFT(2);
        }
        catch (Exception ex)
        {
            Debug.LogError(
                "Blockchain initialization error: " + ex
            );
        }
    }

    public async Task<bool> IsNFTOwnedByPlayer(ulong tokenId)
    {
        if (!isReady || contract == null)
        {
            Debug.LogWarning("Blockchain is not ready!");
            return false;
        }

        try
        {
            var ownerFunction = contract.GetFunction("ownerOf");

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

    public async Task ReadNFT(int tokenId)
    {
        if (!isReady || contract == null)
            return;

        try
        {
            var ownerFunction = contract.GetFunction("ownerOf");

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
            var ownerFunction = contract.GetFunction("ownerOf");

            string owner = await ownerFunction
                .CallAsync<string>(new BigInteger(tokenId));

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





}
