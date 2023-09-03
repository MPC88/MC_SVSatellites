using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MC_SVSatellites.PersistentData;

namespace MC_SVSatellites
{
    internal class SatelliteDeployerEquipment
    {
        internal const int id = 30000;
        internal const string equipmentName = "Articulated Arm";
        internal const string description = "A remotely operated articulated robotic arm.";
        
        [HarmonyPatch(typeof(EquipmentDB), "LoadDatabaseForce")]
        [HarmonyPostfix]
        private static void EquipmentDBLoadDBForce_Post()
        {
            AccessTools.StaticFieldRefAccess<List<Equipment>>(typeof(EquipmentDB), "equipments").Add(SatelliteDeployerEquipment.CreateEquipment());
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.EquipItem))]
        [HarmonyPrefix]
        private static bool InventoryEquipItem_Pre(Inventory __instance, int ___selectedItem, CargoSystem ___cs, SpaceShip ___ss)
        {
            if (___selectedItem < 0)
                return true;

            bool isArticulatingArm = ___cs.cargo[___selectedItem].itemType == 2 && ___cs.cargo[___selectedItem].itemID == id;
            if (!isArticulatingArm || (!__instance.inStation && GameData.data.difficulty == 1))
                return true;

            foreach (ActiveEquipment ae in ___ss.activeEquips)
            {
                if (ae.equipment.id == id)
                {
                    InfoPanelControl.inst.ShowWarning("Only one " + equipmentName + " may be installed.", 1, false);
                    return false;
                }
            }

            return true;
        }

        private static Equipment CreateEquipment()
        {
            Equipment equipment = ScriptableObject.CreateInstance<Equipment>();
            equipment.name = id + "." + equipmentName;
            equipment.id = id;
            equipment.refName = equipmentName;
            equipment.minShipClass = ShipClassLevel.Corvette;
            equipment.activated = true;
            equipment.enableChangeKey = true;
            equipment.space = 2;
            equipment.energyCost = 10f;
            equipment.energyCostPerShipClass = false;
            equipment.rarityCostMod = 0.2f;
            equipment.techLevel = 10;
            equipment.sortPower = 2;
            equipment.massChange = 0;
            equipment.type = EquipmentType.Utility;
            equipment.effects = new List<Effect>() { new Effect() { type = 32, description = "", mod = 1f, value = 10f, uniqueLevel = 0 } };
            equipment.uniqueReplacement = true;
            equipment.rarityMod = 5f;
            equipment.sellChance = 80;
            equipment.repReq = new ReputationRequisite() { factionIndex = 0, repNeeded = 0 };
            equipment.dropLevel = DropLevel.Normal;
            equipment.lootChance = 20;
            equipment.spawnInArena = false;
            equipment.sprite = Assets.equipmentIcon;
            equipment.activeEquipmentIndex = id;
            equipment.defaultKey = KeyCode.Alpha1;
            equipment.requiredItemID = -1;
            equipment.requiredQnt = 0;
            equipment.equipName = equipmentName;
            equipment.description = description;
            equipment.craftingMaterials = null;
            equipment.buff = null;

            return equipment;
        }

        private static AE_ArticulatedArm MakeActiveEquip(Equipment equipment, SpaceShip ss, KeyCode key, int rarity, int qnt)
        {
            AE_ArticulatedArm articulatedArm = new AE_ArticulatedArm
            {
                id = equipment.id,
                rarity = rarity,
                key = key,
                ss = ss,
                isPlayer = (ss != null && ss.CompareTag("Player")),
                equipment = equipment,
                qnt = qnt
            };
            articulatedArm.active = false;
            return articulatedArm;
        }

        [HarmonyPatch(typeof(ActiveEquipment), nameof(ActiveEquipment.AddActivatedEquipment))]
        [HarmonyPrefix]
        internal static bool ActiveEquipmentAdd_Pre(Equipment equipment, SpaceShip ss, KeyCode key, int rarity, int qnt, ref ActiveEquipment __result)
        {
            if (equipment.id != id)
                return true;

            AE_ArticulatedArm ae = MakeActiveEquip(equipment, ss, key, rarity, qnt);
            ss.activeEquips.Add(ae);
            ae.AfterConstructor();
            __result = ae;
            return false;
        }
    }

    public class AE_ArticulatedArm : ActiveEquipment
    {
        public override void ActivateDeactivate(bool shiftPressed, Transform target)
        {
            if (!this.isPlayer)
                return;

            if (!shiftPressed)
            {
                // Spawn satellite
                int consume = this.ss.GetComponent<CargoSystem>().ConsumeItem(3, SatelliteItem.id, 1, -1);
                if (consume < 0)
                {
                    InfoPanelControl.inst.ShowWarning("No satellites in cargo.", 1, false);
                    return;
                }

                GameObject sat = Main.SpawnSatellite(null);
                SatData newData = new SatData()
                {
                    x = sat.transform.position.x,
                    z = sat.transform.position.z,
                    yRot = sat.transform.eulerAngles.y,
                    go = sat
                };

                if (Main.data == null)
                    Main.data = new PersistentData();

                if (!Main.data.deployedSats.ContainsKey(GameData.data.currentSectorIndex))
                    Main.data.deployedSats.Add(GameData.data.currentSectorIndex, new List<SatData>());

                Main.data.deployedSats[GameData.data.currentSectorIndex].Add(newData);
            }
            else
            {
                // Destroy nearest satellite
                if (Main.data == null ||
                    !Main.data.deployedSats.ContainsKey(GameData.data.currentSectorIndex))
                    return;

                SatData[] satellites = Main.data.deployedSats[GameData.data.currentSectorIndex].ToArray();
                if (satellites.Length == 0)
                    return;

                Vector3 pos = this.ss.transform.position;
                pos.y = SatelliteBehaviour.y;
                satellites = satellites.OrderBy(sd => (pos - new Vector3(sd.x, SatelliteBehaviour.y, sd.z)).sqrMagnitude).ToArray();
                
                if (satellites[0] != null)
                {
                    Main.data.deployedSats[GameData.data.currentSectorIndex].Remove(satellites[0]);
                    if(Main.data.deployedSats[GameData.data.currentSectorIndex].Count == 0)
                        Main.data.deployedSats.Remove(GameData.data.currentSectorIndex);

                    GameObject.Destroy(satellites[0].go);                    
                }
            }

            Main.UpdateDebrisFields();
        }
    }
}
