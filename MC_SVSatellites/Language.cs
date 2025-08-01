using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MC_SVSatellites
{
    class Language
    {
        internal static string loadFailed = "Satellites mod load failed.";
        internal static string equipmentName = "Articulated Arm";
        internal static string equipmentDescription = "A remotely operated articulated robotic arm.";
        internal static string onlyOneEquipmentMsg = "Only one Articulated Arm may be installed.";
        internal static string noSatellites = "No satellites in cargo.";
        internal static string itemName = "Satellite";
        internal static string itemDescription = "A deployable remote satellite equipped with an on-board scanner, revealing hidden debris fields and relaying realtime data to your ship's computer.\n\nNote deployed satellites cannot be retrieved, only destroyed.  Requires articulated arm to deploy.";

        internal static void Load(string file)
        {
            try
            {
                if (File.Exists(file))
                {
                    StreamReader sr = new StreamReader(file);
                    loadFailed = sr.ReadLine();
                    equipmentName = sr.ReadLine();
                    equipmentDescription = sr.ReadLine();
                    onlyOneEquipmentMsg = sr.ReadLine();
                    noSatellites = sr.ReadLine();
                    itemName = sr.ReadLine();
                    itemDescription = sr.ReadLine();
                }
            }
            catch
            {
                Debug.Log("Language load failed");
            }
        }
    }
}
