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

                case "Metal Bed Frame Piece":
                    return new ItemData(
                        "Metal Bed Frame Piece",
                        "A sturdy piece of metal from a bed frame. Could be useful for crafting.",
                        false
                    );

                case "Toothbrush Handle":
                    return new ItemData(
                        "Toothbrush Handle",
                        "A handle from a toothbrush. The bristles have been removed.",
                        false
                    );

                case "Wire":
                    return new ItemData(
                        "Wire",
                        "Thin wire salvaged from a light fixture. Flexible and strong.",
                        false
                    );

                case "Makeshift Lockpick Set":
                    return new ItemData(
                        "Makeshift Lockpick Set",
                        "A crude but functional lockpick set. Can unlock the reinforced security gate!",
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