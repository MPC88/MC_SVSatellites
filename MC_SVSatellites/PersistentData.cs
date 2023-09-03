using System;
using System.Collections.Generic;
using UnityEngine;

namespace MC_SVSatellites
{
    [Serializable]
    public class PersistentData
    {
        internal Dictionary<int, List<SatData>> deployedSats = new Dictionary<int, List<SatData>>();

        [Serializable]
        internal class SatData
        {
            internal float x;
            internal float z;
            internal float yRot;
            [NonSerialized]
            internal GameObject go;
        }
    }
}
