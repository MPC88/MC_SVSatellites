using UnityEngine;

namespace MC_SVSatellites
{
    public class Assets
    {
        private const string languageFilename = "\\MC_SVSatellitesLang.txt";

        internal static GameObject satellite;
        internal static Sprite mapIcon;
        internal static Sprite itemIcon;
        internal static Sprite equipmentIcon;

        internal static void Load(string pluginPath)
        {
            string pluginfolder = System.IO.Path.GetDirectoryName(pluginPath);
            string bundleName = "mc_svsatellites";
            AssetBundle assets = AssetBundle.LoadFromFile($"{pluginfolder}\\{bundleName}");

            satellite = assets.LoadAsset<GameObject>("Assets/Satellite/satmodel.prefab");
            mapIcon = assets.LoadAsset<Sprite>("Assets/Satellite/mapicon.png");
            itemIcon = assets.LoadAsset<Sprite>("Assets/Satellite/itemicon.png");
            equipmentIcon = assets.LoadAsset<Sprite>("Assets/Satellite/equipmenticon.png");
                        
            Language.Load(pluginfolder + languageFilename);
        }
    }
}
