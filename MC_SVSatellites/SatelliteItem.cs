using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace MC_SVSatellites
{
    internal class SatelliteItem
    {
        internal const int id = 30000;
        internal const string itemName = "Satellite";
        internal const string description = "A deployable remote satellite equipped with an on-board scanner, revealing hidden debris fields and relaying realtime data to your ship's computer.\n\nNote deployed satellites cannot be retrieved, only destroyed.  Requires articulated arm to deploy.";

        [HarmonyPatch(typeof(ItemDB), "LoadDatabaseForce")]
        [HarmonyPostfix]
		private static void ItemDBLoadDBForce_Post()
        {
			AccessTools.StaticFieldRefAccess<List<Item>>(typeof(ItemDB), "items").Add(CreateItem());
		}

		private static Item CreateItem()
		{
			Item item = ScriptableObject.CreateInstance<Item>();
			item.id = id;
			item.name = id + "." + itemName;
			item.refName = itemName;
			item.rarity = 1;
			item.levelPlus = 1;
			item.weight = 1f;
			item.basePrice = 200f;
			item.priceVariation = 0.4f;
			item.tradeChance = new int[7] { 100, 100, 100, 100, 100, 100, 50 };
			item.tradeQuantity = 10;
			item.type = ItemType.Electronic;
			item.gameObj = ItemDB.GetItem(22).gameObj;
			item.sprite = Assets.itemIcon;
			item.askedInQuests = true;
			item.canBeStashed = false;
			item.itemName = itemName;
			item.description = description;
			item.canUpgradeToTier = ItemRarity.Poor_0;
			item.craftable = true;
			item.craftingYield = 5;
			item.craftingLevelAffectsYield = true;
			item.craftingMaterials = new List<CraftMaterial>()
			{
				new CraftMaterial(84, 2),
				new CraftMaterial(9, 2),
				new CraftMaterial(51, 1)				
            };
			item.teachItemBlueprints = new int[0];
			return item;
		}

        /**
         * The following methods are required to due vanilla game assumption that item ID == index in ItemDB
         * items List<Item>.  While this is fair for vanilla entries, a mod entry which attempts to avoid
         * ID collisions causes failures:
         * MarketSystem.GetItemForQuest => null reference exception.
         * MarketSystem.GenerateMarket => mod item will never appear (again, null ref, but original method checks so no exception).
		 */
        [HarmonyPatch(typeof(MarketSystem), nameof(MarketSystem.GetItemForQuest))]
        [HarmonyPrefix]
        private static bool MarketSystemGetItemForQuest_Pre(int rarity, float maxPrice, List<MarketItem> forbiddenList, System.Random rand, ref Item __result)
        {
            List<Item> items = AccessTools.StaticFieldRefAccess<List<Item>>(typeof(ItemDB), "items");
            List<Item> list = new List<Item>();
            for (int i = 0; i < ItemDB.count; i++)
            {
                Item item = items[i];
                if (item.askedInQuests && item.rarity == rarity && item.basePrice <= maxPrice)
                {
                    if (forbiddenList == null)
                    {
                        list.Add(item);
                    }
                    else if (MarketSystem.GetMarketItem(3, item.id, forbiddenList, null) == null)
                    {
                        list.Add(item);
                    }
                    else if (MarketSystem.GetMarketItem(3, item.id, forbiddenList, null).stock == 0)
                    {
                        list.Add(item);
                    }
                }
            }
            if (list.Count > 0)
            {
                __result = list[rand.Next(0, list.Count)];
            }
            __result = null;

            return false;
        }

        [HarmonyPatch(typeof(MarketSystem), nameof(MarketSystem.GenerateMarket))]
        [HarmonyPostfix]
        private static void MarketSystemGenerateMarket_Post(int factionIndex, System.Random rand, ref List<MarketItem> __result)
        {
            Item item = ItemDB.GetItem(id);
            if (rand.Next(1, 101) <= item.tradeChance[factionIndex])
            {
                __result.Add(new MarketItem(3, id, 1, rand.Next((int)((float)item.tradeQuantity * 0.1f), item.tradeQuantity), null));
                MarketSystem.SortMarket(__result);
            }
        }
    }
}
