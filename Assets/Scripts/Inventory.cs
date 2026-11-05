using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Inventory : MonoBehaviour
{
    private List<string> items = new List<string>();

    public TextMeshProUGUI inventoryText;

    public void AddItem(string itemId, string itemName, string rarity, string description, string owner)
    {
        string item = itemId + ": " + itemName + " [" + rarity + "]";

        items.Add(item);

        UpdateUI();

        Debug.Log("Added to inventory: " + itemId);
        Debug.Log("Name: " + itemName);
        Debug.Log("Rarity: " + rarity);
        Debug.Log("Description: " + description);
        Debug.Log("Owner: " + owner);
        Debug.Log("Inventory items: " + items.Count);
    }

    private void UpdateUI()
    {
        inventoryText.text = "Inventory\n";

        foreach (string item in items)
        {
            inventoryText.text += "- " + item + "\n";
        }
    }
}
