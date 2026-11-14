
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public string itemId = "ITEM-001";
    public ulong tokenId = 1;

    // Local fields retained for compatibility
    public string itemName = "Ancient Sword";
    public string rarity = "Rare";
    public string description =
        "An ancient sword found in the ruins.";
    public string owner = "Player";

    private bool isProcessing = false;

    private async void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || isProcessing)
            return;

        Inventory inventory = other.GetComponent<Inventory>();

        if (inventory == null)
            return;

        if (BlockchainManager.Instance == null)
        {
            Debug.LogError("BlockchainManager not found!");
            return;
        }

        if (inventory.HasToken(tokenId))
        {
            Debug.LogWarning(
                $"NFT #{tokenId} is already in inventory!"
            );
            return;
        }

        isProcessing = true;

        try
        {
            Debug.Log(
                $"Loading NFT #{tokenId} from Sepolia..."
            );

            GameItem item = await BlockchainManager.Instance
                .GetGameItemFromBlockchain(itemId, tokenId);

            if (item == null)
            {
                Debug.LogWarning(
                    $"Cannot pick up NFT #{tokenId}: " +
                    "ownership verification or data loading failed."
                );
                return;
            }

            if (inventory == null || inventory.HasToken(tokenId))
                return;

            inventory.AddItem(item);

            Debug.Log(
                $"NFT #{tokenId} loaded from blockchain " +
                "and added to inventory!"
            );

            Destroy(gameObject);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Item pickup error: " + ex);
        }
        finally
        {
            isProcessing = false;
        }
    }
}
