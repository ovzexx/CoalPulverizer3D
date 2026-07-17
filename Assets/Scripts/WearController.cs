using System.Collections.Generic;
using UnityEngine;

namespace CoalPulverizer
{
    // JS에서 unityInstance.SendMessage('Coal Pulverizer', 'UpdateZoneWear', json)으로 호출
    public sealed class WearController : MonoBehaviour
    {
        // [rollerIdx 0~2][zoneIdx 0~9]  ZoneIndex 0=와이드끝, 9=내로우끝
        private readonly RollTireZone[][] _zones = new RollTireZone[3][];

        private void Start() => CollectZones();

        private void CollectZones()
        {
            for (int r = 0; r < 3; r++) _zones[r] = new RollTireZone[10];

            foreach (RollTireZone rtz in FindObjectsByType<RollTireZone>(FindObjectsSortMode.None))
            {
                // GO 이름: WearZone_R{rollIndex 1~3}_Z{zoneIndex 0~9}
                string[] parts = rtz.name.Split('_');
                if (parts.Length < 3) continue;
                if (!int.TryParse(parts[1].Substring(1), out int ri)) continue;
                if (!int.TryParse(parts[2].Substring(1), out int zi)) continue;
                int r = ri - 1;  // 0-based
                if (r < 0 || r > 2 || zi < 0 || zi > 9) continue;
                _zones[r][zi] = rtz;
            }
        }

        // JS 호출: UpdateZoneWear('{"r1":[w0..w9],"r2":[...],"r3":[...]}')
        // w[i]: ZoneIndex i (0=와이드끝/Zone10, 9=내로우끝/Zone1), 값 0.0~1.0
        public void UpdateZoneWear(string json)
        {
            WearPayload p;
            try { p = JsonUtility.FromJson<WearPayload>(json); }
            catch { Debug.LogWarning("[WearController] JSON parse error"); return; }

            ApplyToRoller(0, p.r1);
            ApplyToRoller(1, p.r2);
            ApplyToRoller(2, p.r3);
        }

        private void ApplyToRoller(int rollerIdx, float[] values)
        {
            if (values == null || _zones[rollerIdx] == null) return;
            for (int zi = 0; zi < 10 && zi < values.Length; zi++)
            {
                _zones[rollerIdx][zi]?.Apply(values[zi]);
            }
        }

        [System.Serializable]
        private class WearPayload
        {
            public float[] r1;
            public float[] r2;
            public float[] r3;
        }
    }
}
