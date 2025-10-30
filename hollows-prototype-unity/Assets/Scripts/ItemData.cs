using UnityEngine;

namespace Assets.Scripts
{
    [System.Serializable]
    public class ItemData
    {
        public string itemName;
        public string description;
        public Sprite icon;
        public bool isUsable;

        public ItemData(string name, string desc, bool usable = true)
        {
            itemName = name;
            description = desc;
            isUsable = usable;
        }
    }

    public static class ItemDatabase
    {
        public static ItemData GetItemData(string itemName)
        {
            switch (itemName)
            {
                case "Battery":
                    return new ItemData(
                        "Battery",
                        "Recharges your flashlight. Click to equip, then use with [Right Click] or [Use Button].",
                        true
                    );

                case "Key":
                    return new ItemData(
                        "Key",
                        "Opens the exit door. Equip this and go to the exit to escape!",
                        true
                    );

                default:
                    return new ItemData(
                        itemName,
                        "A mysterious item.",
                        false
                    );
            }
        }
    }
}