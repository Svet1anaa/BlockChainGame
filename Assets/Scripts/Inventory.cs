
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Inventory : MonoBehaviour
{
    private List<GameItem> items = new List<GameItem>();

    public TextMeshProUGUI inventoryText;

    public void AddItem(GameItem item)
    {
        items.Add(item);

        UpdateUI();

        Debug.Log("Added to inventory: " + item.itemId);
        Debug.Log("Token ID: " + item.tokenId);
        Debug.Log("Name: " + item.itemName);
        Debug.Log("Rarity: " + item.rarity);
        Debug.Log("Description: " + item.description);
        Debug.Log("Owner: " + item.owner);
        Debug.Log("Inventory items: " + items.Count);
    }

    private void UpdateUI()
    {
        if (inventoryText == null)
        {
            Debug.LogWarning("Inventory Text is not assigned!");
            return;
        }

        inventoryText.text = "Inventory\n";

        foreach (GameItem item in items)
        {
            inventoryText.text +=
                "- " + item.itemId + ": " +
                item.itemName + " [" + item.rarity + "]\n";
        }
    }
}
