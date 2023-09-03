using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
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
        public const string pluginVersion = "1.0.0";
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
            int vertices = 200;
            LineRenderer line = minimapIcon.AddComponent<LineRenderer>();
            line.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));                        
            line.startWidth = line.endWidth = 0.5f;
            line.loop = true;
            float angle = 2 * Mathf.PI / vertices;
            line.positionCount = vertices;
            for (int i = 0; i < vertices; i++)
            {
                Matrix4x4 rotationMatrix = new Matrix4x4(new Vector4(Mathf.Cos(angle * i), 0, Mathf.Sin(angle * i), 0),
                                                         new Vector4(-1 * Mathf.Sin(angle * i), 0, Mathf.Cos(angle * i), 0),
                                           new Vector4(0, 1, 0, 0),
                                           new Vector4(0, 0, 0, 1));
                Vector3 initialRelativePosition = new Vector3(0, cfg_SatRange.Value, 0);
                line.SetPosition(i, minimapIcon.transform.position + rotationMatrix.MultiplyPoint(initialRelativePosition));

            }
            line.startColor = line.endColor = new Color(20, 115, 210, 1);
            line.colorGradient.Evaluate(0.2f);            
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
                SideInfo.AddMsg("<color=red>Satellites mod load failed.</color>");
            }
        }
    }
}
