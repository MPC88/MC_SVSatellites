using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static MC_SVSatellites.PersistentData;

namespace MC_SVSatellites
{
    [Serializable]
    internal class SatelliteBehaviour : MonoBehaviour
    {        
        internal const float y = 20f;
        internal const float scannerUpdateTime = 0.5f;
        private const float timeOn = 0.1f;
        private const float timeBetweenLights = 0.3f;
        private const float timeBetweenFlash = 1f;

        internal static float scannerUpdateCnt = 0f;
        private static MethodInfo aiControl_ShowHideMinimapIcon;
        private static MethodInfo hideShowObject_ShowMinimapIcon;
        private static MethodInfo collectible_ShowMiniMapIcon;

        private GameObject redLight;
        private GameObject greenLight;
        private float animationTimeCnt;

        private void Awake()
        {
            this.redLight = base.transform.Find("RedLight").gameObject;
            this.greenLight = base.transform.Find("GreenLight").gameObject;
            this.greenLight.SetActive(false);
            this.animationTimeCnt = 0;
            if(aiControl_ShowHideMinimapIcon == null)
                aiControl_ShowHideMinimapIcon = typeof(AIControl).GetMethod("ShowHideMinimapIcon", AccessTools.all);
            if (hideShowObject_ShowMinimapIcon == null)
                hideShowObject_ShowMinimapIcon = typeof(HideShowObject).GetMethod("ShowMinimapIcon", AccessTools.all);
            if (collectible_ShowMiniMapIcon == null)
                collectible_ShowMiniMapIcon = typeof(Collectible).GetMethod("ShowMinimapIcon", AccessTools.all);
        }

        private void Update()
        {
            // Lights
            this.animationTimeCnt += Time.deltaTime;
            if (this.animationTimeCnt >= timeBetweenFlash + timeBetweenLights + (timeOn * 2))
            {
                this.redLight.SetActive(true);
                this.animationTimeCnt = 0;
            }
            else if (this.animationTimeCnt >= timeBetweenLights + (timeOn * 2))
                this.greenLight.SetActive(false);
            else if (this.animationTimeCnt >= timeBetweenLights + timeOn)
                this.greenLight.SetActive(true);
            else if (this.animationTimeCnt >= timeOn)
                this.redLight.SetActive(false);
        }

        [HarmonyPatch(typeof(DebrisFieldControl), "ShowLabelIfAble")]
        [HarmonyPrefix]
        private static void DebrisFieldShowLabelIfAble_Pre(DebrisFieldControl __instance, ref bool inScavengeRange)
        {
            if (Main.data == null)
                return;

            if (Main.data.deployedSats.TryGetValue(GameData.data.currentSectorIndex, out List<SatData> satDatas))
                foreach (SatData sd in satDatas)
                    if (Vector2.Distance(new Vector2(__instance.debrisField.centerX, __instance.debrisField.centerY), new Vector2(sd.x, sd.z)) <= Main.cfg_SatRange.Value &&
                        __instance.debrisField.special && !__instance.debrisField.scavenged)
                    {
                        inScavengeRange = true;
                        break;
                    }
        }

        [HarmonyPatch(typeof(DebrisFieldControl), "OnTriggerExit")]
        [HarmonyPostfix]
        private static void DebrisFieldControlOnTriggerExit_Post(DebrisFieldControl __instance, GameObject ___labelBar)
        {
            if (Main.data == null || !__instance.debrisField.special)
                return;

            if (___labelBar == null || ___labelBar.activeSelf)
                return;

            if (Main.data.deployedSats.TryGetValue(GameData.data.currentSectorIndex, out List<SatData> satDatas))
                foreach (SatData sd in satDatas)
                    if (Vector2.Distance(new Vector2(__instance.debrisField.centerX, __instance.debrisField.centerY), new Vector2(sd.x, sd.z)) <= Main.cfg_SatRange.Value &&
                        __instance.debrisField.special && !__instance.debrisField.scavenged)
                    {
                        ___labelBar.SetActive(true);
                        break;
                    }
        }

        [HarmonyPatch(typeof(AIControl), "Update")]
        [HarmonyPostfix]
        private static void AIControlUpdate_Post(AIControl __instance, Transform ___tf)
        {
            if (Main.data == null || scannerUpdateCnt <= scannerUpdateTime)
                return;

            if (Main.data.deployedSats.TryGetValue(GameData.data.currentSectorIndex, out List<SatData> satDatas))
                foreach (SatData sd in satDatas)
                    if (Main.cfg_SatRange.Value > Vector3.Distance(new Vector3(sd.x, 0, sd.z), ___tf.position))
                    {
                        aiControl_ShowHideMinimapIcon.Invoke(__instance, new object[] { true });
                        break;
                    }
        }

        [HarmonyPatch(typeof(AIControl), "ShowHideMinimapIcon")]
        [HarmonyPrefix]
        private static void AIControlShowHideMinimapIcon_Pre(ref bool condition, Transform ___tf)
        {
            if (condition || Main.data == null)
                return;

            if (Main.data.deployedSats.TryGetValue(GameData.data.currentSectorIndex, out List<SatData> satDatas))
                foreach (SatData sd in satDatas)
                    if (Main.cfg_SatRange.Value > Vector3.Distance(new Vector3(sd.x, 0, sd.z), ___tf.position))
                    {
                        condition = true;
                        break;
                    }
        }

        [HarmonyPatch(typeof(Asteroid), "Update")]
        [HarmonyPostfix]
        private static void AsteroidUpdate_Post(Asteroid __instance, Transform ___tf)
        {
            if (Main.data == null || scannerUpdateCnt <= scannerUpdateTime)
                return;

            if (Main.data.deployedSats.TryGetValue(GameData.data.currentSectorIndex, out List<SatData> satDatas))
                foreach (SatData sd in satDatas)
                    if (Main.cfg_SatRange.Value > Vector3.Distance(new Vector3(sd.x, 0, sd.z), ___tf.position))
                    {
                        __instance.ShowMapIcon(true);
                        break;
                    }
        }

        [HarmonyPatch(typeof(Asteroid), "ShowMapIcon")]
        [HarmonyPrefix]
        private static void AsteroidShowMapIcon_Pre(ref bool condition, Transform ___tf)
        {
            if (condition || Main.data == null)
                return;

            if (Main.data.deployedSats.TryGetValue(GameData.data.currentSectorIndex, out List<SatData> satDatas))
                foreach (SatData sd in satDatas)
                    if (Main.cfg_SatRange.Value > Vector3.Distance(new Vector3(sd.x, 0, sd.z), ___tf.position))
                    {
                        condition = true;
                        break;
                    }
        }

        [HarmonyPatch(typeof(HideShowObject), "Update")]
        [HarmonyPostfix]
        private static void HideShowObjectUpdate_Post(HideShowObject __instance)
        {
            if (Main.data == null || scannerUpdateCnt <= scannerUpdateTime)
                return;

            if (Main.data.deployedSats.TryGetValue(GameData.data.currentSectorIndex, out List<SatData> satDatas))
                foreach (SatData sd in satDatas)
                    if (Main.cfg_SatRange.Value > Vector3.Distance(new Vector3(sd.x, 0, sd.z), __instance.gameObject.transform.position))
                    {
                        hideShowObject_ShowMinimapIcon.Invoke(__instance, null);
                        break;
                    }
        }

        [HarmonyPatch(typeof(HideShowObject), "HideMinimapIcon")]
        [HarmonyPrefix]
        private static bool HideShowObjectHideMinimapIcon_Pre(HideShowObject __instance)
        {
            if (Main.data == null)
                return true;

            if (Main.data.deployedSats.TryGetValue(GameData.data.currentSectorIndex, out List<SatData> satDatas))
                foreach (SatData sd in satDatas)
                    if (Main.cfg_SatRange.Value > Vector3.Distance(new Vector3(sd.x, 0, sd.z), __instance.gameObject.transform.position))
                        return false;

            return true;
        }

        [HarmonyPatch(typeof(POIControl), "Update")]
        [HarmonyPostfix]
        private static void POIControlUpdate_Post(POIControl __instance)
        {
            if (Main.data == null || scannerUpdateCnt <= scannerUpdateTime)
                return;

            if (Main.data.deployedSats.TryGetValue(GameData.data.currentSectorIndex, out List<SatData> satDatas))
                foreach (SatData sd in satDatas) 
                    if (Main.cfg_SatRange.Value > Vector3.Distance(new Vector3(sd.x, 0, sd.z), __instance.gameObject.transform.position))
                    {
                        __instance.Discover(true);
                        break;
                    }
        }

        [HarmonyPatch(typeof(Collectible), "Update")]
        [HarmonyPostfix]
        private static void CollectibleUpdate_Post(Collectible __instance)
        {
            if (Main.data == null || scannerUpdateCnt <= scannerUpdateTime)
                return;

            if (Main.data.deployedSats.TryGetValue(GameData.data.currentSectorIndex, out List<SatData> satDatas))
                foreach (SatData sd in satDatas)
                    if (Main.cfg_SatRange.Value > Vector3.Distance(new Vector3(sd.x, 0, sd.z), __instance.gameObject.transform.position))
                        collectible_ShowMiniMapIcon.Invoke(__instance, null);
        }

        [HarmonyPatch(typeof(Collectible), "HideMinimapIcon")]
        [HarmonyPrefix]
        private static bool CollectibleHideMinimapIcon_Pre(Collectible __instance)
        {
            if (Main.data == null)
                return true;

            if (Main.data.deployedSats.TryGetValue(GameData.data.currentSectorIndex, out List<SatData> satDatas))
                foreach (SatData sd in satDatas)
                    if (Main.cfg_SatRange.Value > Vector3.Distance(new Vector3(sd.x, 0, sd.z), __instance.gameObject.transform.position))
                        return false;

            return true;
        }

        [HarmonyPatch(typeof(FOWControl), "Update")]
        [HarmonyPostfix]
        private static void FOWControlUpdate_Post(FOWControl __instance)
        {
            if (Main.data == null || scannerUpdateCnt <= scannerUpdateTime)
                return;

            if (Main.data.deployedSats.TryGetValue(GameData.data.currentSectorIndex, out List<SatData> satDatas))
                foreach (SatData sd in satDatas)
                    if (Main.cfg_SatRange.Value > Vector3.Distance(new Vector3(sd.x, 0, sd.z), __instance.gameObject.transform.position))
                    {
                        GameManager.instance.DestroyFOWTile(__instance.fowTile, true);
                        GameObject.Destroy(__instance.gameObject);
                    }
        }
    }
}
