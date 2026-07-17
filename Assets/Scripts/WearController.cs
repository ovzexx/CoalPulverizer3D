using System.Globalization;
using UnityEngine;

namespace CoalPulverizer
{
    // JS: unityInstance.SendMessage('Coal Pulverizer', 'UpdateR{1|2|3}Wear', "0.12,0.25,...")
    // CSV 값: ZoneIndex 0~9 (0=와이드끝/Zone10, 9=내로우끝/Zone1), 0.0~1.0
    public sealed class WearController : MonoBehaviour
    {
        private readonly RollTireZone[][] _zones = new RollTireZone[3][];
        private readonly string[] _pending = new string[3];
        private bool _ready;

        private const int   N      = 10;
        private const float Z_WIDE = -0.42f;
        private const float Z_NARR =  0.42f;
        private const float R_WIDE =  0.80f;
        private const float R_NARR =  0.34f;
        private const float EPS    =  0.018f;

        private void Start()
        {
            BuildZoneOverlays();
            _ready = true;
            for (int r = 0; r < 3; r++)
            {
                if (_pending[r] != null) { ParseAndApply(r, _pending[r]); _pending[r] = null; }
            }
        }

        private void BuildZoneOverlays()
        {
            float zRange = Z_NARR - Z_WIDE;
            float rRange = R_WIDE - R_NARR;
            float zoneH  = zRange / N;

            for (int r = 0; r < 3; r++)
            {
                _zones[r] = new RollTireZone[N];
                int ri = r + 1;
                GameObject tireGo = GameObject.Find("Replaceable Asymmetric Roll Tire " + ri);
                if (tireGo == null)
                {
                    Debug.LogWarning("[WearController] 타이어 없음: " + ri);
                    continue;
                }
                Transform tireTf = tireGo.transform;

                for (int i = 0; i < N; i++)
                {
                    float zCenter = Z_WIDE + (i + 0.5f) * zoneH;
                    float rBot    = R_WIDE -  i      * (rRange / N) + EPS;
                    float rTop    = R_WIDE - (i + 1) * (rRange / N) + EPS;

                    GameObject go = new GameObject("WearZone_R" + ri + "_Z" + i);
                    go.transform.SetParent(tireTf, false);
                    go.transform.localPosition = new Vector3(0f, zCenter, 0f);

                    go.AddComponent<MeshFilter>().sharedMesh =
                        MeshFactory.Frustum(rBot, rTop, zoneH, 48);
                    go.AddComponent<MeshRenderer>();

                    RollTireZone rtz = go.AddComponent<RollTireZone>();
                    rtz.ZoneIndex = i;
                    _zones[r][i] = rtz;
                }
            }
        }

        public void UpdateR1Wear(string csv) { if (!_ready) { _pending[0] = csv; return; } ParseAndApply(0, csv); }
        public void UpdateR2Wear(string csv) { if (!_ready) { _pending[1] = csv; return; } ParseAndApply(1, csv); }
        public void UpdateR3Wear(string csv) { if (!_ready) { _pending[2] = csv; return; } ParseAndApply(2, csv); }

        private void ParseAndApply(int ri, string csv)
        {
            string[] parts = csv.Split(',');
            for (int zi = 0; zi < N && zi < parts.Length; zi++)
            {
                if (float.TryParse(parts[zi], NumberStyles.Float,
                                   CultureInfo.InvariantCulture, out float v))
                    _zones[ri][zi]?.Apply(v);
            }
        }
    }
}
