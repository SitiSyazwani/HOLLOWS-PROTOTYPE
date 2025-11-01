using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts
{
    [System.Serializable]
    public class OrderedCraftingRecipe
    {
        public string recipeName;
        public List<string> orderedItems = new List<string>(); // ORDER MATTERS!
        public string resultItem;
        public string description;

        public OrderedCraftingRecipe(string name, List<string> ordered, string result, string desc)
        {
            recipeName = name;
            orderedItems = ordered;
            resultItem = result;
            description = desc;
        }

        public bool CheckOrder(List<string> placedItems)
        {
            if (placedItems.Count != orderedItems.Count)
            {
                return false;
            }

            for (int i = 0; i < orderedItems.Count; i++)
            {
                if (placedItems[i] != orderedItems[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}