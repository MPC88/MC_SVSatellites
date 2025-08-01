﻿using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using static MC_SVSatellites.PersistentData;

namespace MC_SVSatellites
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string pluginGuid = "mc.starvalor.satellites";
        public const string pluginName = "SV Satellites";
        public const string pluginVersion = "1.3.2";
        private const string modSaveFolder = "/MCSVSaveData/";  // /SaveData/ sub folder
        private const string modSaveFilePrefix = "Sats_"; // modSaveFlePrefixNN.dat

        internal static PersistentData data;
        internal static ConfigEntry<float> cfg_SatRange;
        internal static MethodInfo debrisFieldControl_ShowLabelIfAble;

        private void Awake()
        {
            Assets.Load(GetType().Assembly.Location);            
            Harmony.CreateAndPatchAll(typeof(Main));
            Harmony.CreateAndPatchAll(typeof(SatelliteDeployerEquipment));
            Harmony.CreateAndPatchAll(typeof(SatelliteItem));
            Harmony.CreateAndPatchAll(typeof(SatelliteBehaviour));

            debrisFieldControl_ShowLabelIfAble = typeof(DebrisFieldControl).GetMethod("ShowLabelIfAble", AccessTools.all);

            cfg_SatRange = Config.Bind<float>("Configuration",
                "Satellite scanner power",
                230f,
                "Range of satellite scanners.");
        }

        private void Update()
        {
            // Global update timer for all map icon updates
            if(GameManager.instance != null && GameManager.instance.inGame &&
                data != null && data.deployedSats.Count > 0 && 
                data.deployedSats.ContainsKey(GameData.data.currentSectorIndex))
            {
                if (SatelliteBehaviour.scannerUpdateCnt > SatelliteBehaviour.scannerUpdateTime)
                    SatelliteBehaviour.scannerUpdateCnt = 0;
                else
                    SatelliteBehaviour.scannerUpdateCnt += Time.deltaTime;
            }
        }

        internal static GameObject SpawnSatellite(SatData satData)
        {
            GameObject newGo = GameObject.Instantiate(Assets.satellite);            
            newGo.AddComponent<SatelliteBehaviour>();
            
            newGo.transform.eulerAngles = new Vector3(newGo.transform.eulerAngles.x, 
                satData == null ? UnityEngine.Random.Range(1,360) : satData.yRot, 
                newGo.transform.eulerAngles.z);
            newGo.transform.position = new Vector3(satData == null ? GameManager.instance.Player.transform.position.x : satData.x, 
                SatelliteBehaviour.y,
                satData == null ? GameManager.instance.Player.transform.position.z : satData.z);
            if (satData != null)
                satData.go = newGo;
            Animator anim = newGo.GetComponent<Animator>();
            anim.SetBool("play", true);

            // Minimap
            GameObject minimapIcon = GameObject.Instantiate<GameObject>(ObjManager.GetObj("Icons/MinimapIconShip"), newGo.transform.position, newGo.transform.rotation);
            minimapIcon.transform.SetParent(newGo.transform);
            minimapIcon.transform.GetChild(0).GetComponent<Renderer>().material.mainTexture = Assets.mapIcon.texture;
            minimapIcon.transform.GetChild(0).localScale = new Vector3(30f, 30f, 1f);
            minimapIcon.name = "Satellite Icon";

            int segments = 360;
            LineRenderer line = minimapIcon.AddComponent<LineRenderer>();
            line.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
            line.useWorldSpace = false;
            line.startWidth = 1f;
            line.endWidth = 1f;
            line.positionCount = segments + 1;
            int pointCount = segments + 1;
            Vector3[] points = new Vector3[pointCount];

            for (int i = 0; i < pointCount; i++)
            {
                float rad = Mathf.Deg2Rad * (i * 360f / segments);
                points[i] = new Vector3(Mathf.Sin(rad) * cfg_SatRange.Value, 0, Mathf.Cos(rad) * cfg_SatRange.Value);
            }
            line.startColor = line.endColor = new Color(0f, 0.63f, 0.9f, 1);
            line.SetPositions(points);
            
            minimapIcon.SetActive(true);

            return newGo;
        }

        internal static void UpdateDebrisFields()
        {
            foreach (DebrisField df in GameData.data.GetCurrentSector().debrisFields)
                if (df != null && df.debrisFieldControl != null)
                    debrisFieldControl_ShowLabelIfAble.Invoke(df.debrisFieldControl, new object[] { false });
        }

        [HarmonyPatch(typeof(GameManager), nameof(GameManager.SetupGame))]
        [HarmonyPostfix]
        private static void GameManagerSetupGame_Post()
        {
            if(data != null && data.deployedSats.TryGetValue(GameData.data.currentSectorIndex, out List<SatData> sectorSats))
                foreach(SatData sd in sectorSats)
                    SpawnSatellite(sd);

            UpdateDebrisFields();
        }

        [HarmonyPatch(typeof(GameData), "SetGameData")]
        [HarmonyPostfix]
        private static void GameDataSetGameData_Post(GameDataInfo ___data)
        {
            //Auto add satellite blueprint if not known
            if(___data.character.GetBlueprint(3, SatelliteItem.id, false) == null)
            {
                PChar.Char.AddBlueprint(3, SatelliteItem.id, 1f);
                PChar.Char.SortBlueprints();
            }
        }

        [HarmonyPatch(typeof(GameData), nameof(GameData.SaveGame))]
        [HarmonyPrefix]
        private static void GameDataSaveGame_Pre()
        {
            if (data == null || data.deployedSats.Count == 0)
                return;

            string tempPath = Application.dataPath + GameData.saveFolderName + modSaveFolder + "SATTemp.dat";

            if (!Directory.Exists(Path.GetDirectoryName(tempPath)))
                Directory.CreateDirectory(Path.GetDirectoryName(tempPath));

            if (File.Exists(tempPath))
                File.Delete(tempPath);

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream fileStream = File.Create(tempPath);
            binaryFormatter.Serialize(fileStream, data);
            fileStream.Close();

            File.Copy(tempPath, Application.dataPath + GameData.saveFolderName + modSaveFolder + modSaveFilePrefix + GameData.gameFileIndex.ToString("00") + ".dat", true);
            File.Delete(tempPath);
        }

        [HarmonyPatch(typeof(MenuControl), nameof(MenuControl.LoadGame))]
        [HarmonyPostfix]
        private static void MenuControlLoadGame_Post()
        {
            string modData = Application.dataPath + GameData.saveFolderName + modSaveFolder + modSaveFilePrefix + GameData.gameFileIndex.ToString("00") + ".dat";
            try
            {
                if (!GameData.gameFileIndex.ToString("00").IsNullOrWhiteSpace() && File.Exists(modData))
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    FileStream fileStream = File.Open(modData, FileMode.Open);
                    PersistentData loadData = (PersistentData)binaryFormatter.Deserialize(fileStream);
                    fileStream.Close();

                    if (loadData == null)
                        data = new PersistentData();
                    else
                        data = loadData;
                }
                else
                    data = new PersistentData();
            }
            catch
            {
                SideInfo.AddMsg("<color=red>" + Language.loadFailed + "</color>");
            }
        }

        [HarmonyPatch(typeof(MenuControl), nameof(MenuControl.DeleteSaveGame))]
        [HarmonyPrefix]
        private static void DeleteSave_Pre()
        {
            if (GameData.ExistsAnySaveFile(GameData.gameFileIndex) &&
                File.Exists(Application.dataPath + GameData.saveFolderName + modSaveFolder + modSaveFilePrefix + GameData.gameFileIndex.ToString("00") + ".dat"))
            {
                File.Delete(Application.dataPath + GameData.saveFolderName + modSaveFolder + modSaveFilePrefix + GameData.gameFileIndex.ToString("00") + ".dat");
            }
        }

        [HarmonyPatch(typeof(GameData), nameof(GameData.CreateDefaultChar))]
        [HarmonyPostfix]
        private static void GameDataCreateDefaultChar_Post()
        {
            if (PChar.Char != null && PChar.Char.HasPerk(324))
            {
                PChar.Char.AddBlueprint(2, SatelliteDeployerEquipment.id, 1f);
                PChar.Char.AddBlueprint(3, SatelliteItem.id, 1f);
                PChar.Char.SortBlueprints();
            }
        }
    }
}
