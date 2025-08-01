using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace MC_SVSatellites
{
    internal class SatelliteItem
    {
        internal const int id = 30000;
		internal const string refItemName = "Satellite";

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
			item.name = id + "." + refItemName;
			item.refName = refItemName;
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
			item.itemName = Language.itemName;
			item.description = Language.itemDescription;
			item.canUpgradeToTier = ItemRarity.Poor_0;
			item.craftable = true;
			item.craftingYield = 5;
			item.craftingLevelAffectsYield = true;
			item.craftingMaterials = new List<CraftMaterial>()
			{
				new CraftMaterial(42, 2),
				new CraftMaterial(9, 2),
				new CraftMaterial(51, 1)				
            };
			item.teachItemBlueprints = new int[0];
			return item;
		}
    }
}
