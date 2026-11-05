
[System.Serializable]
public class GameItem
{
    public string itemId;
    public ulong tokenId;
    public string itemName;
    public string rarity;
    public string description;
    public string owner;

    public GameItem(
        string id,
        ulong token,
        string name,
        string itemRarity,
        string itemDescription,
        string itemOwner)
    {
        itemId = id;
        tokenId = token;
        itemName = name;
        rarity = itemRarity;
        description = itemDescription;
        owner = itemOwner;
    }
}
