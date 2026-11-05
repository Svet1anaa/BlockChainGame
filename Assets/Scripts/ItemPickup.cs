
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public string itemId = "ITEM-001";
    public ulong tokenId = 1;
    public string itemName = "Ancient Sword";
    public string rarity = "Rare";
    public string description = "An ancient sword found in the ruins.";
    public string owner = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Inventory inventory = other.GetComponent<Inventory>();

            if (inventory != null)
            {
                GameItem item = new GameItem(
                    itemId,
                    tokenId,
                    itemName,
                    rarity,
                    description,
                    owner
                );

                inventory.AddItem(item);
                Destroy(gameObject);
            }
        }
    }
}
