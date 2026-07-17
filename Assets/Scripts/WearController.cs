using UnityEngine;

namespace CoalPulverizer
{
    // 롤타이어 Zone 색상 오버레이를 런타임에 생성·관리.
    // JS: unityInstance.SendMessage('Coal Pulverizer', 'UpdateZoneWear', json)
    public sealed class WearController : MonoBehaviour
    {
        // [rollerIdx 0~2][zoneIdx 0~9]  ZoneIndex 0=와이드끝(Zone10), 9=내로우끝(Zone1)
        private readonly RollTireZone[][] _zones = new RollTireZone[3][];
        private string _pendingJson;
        private bool _ready;

        // 롤타이어 단면 파라미터 (CoalPulverizerBuilder.RollTireProfile 과 동일)
        private const int   N      = 10;
        private const float Z_WIDE = -0.42f;
        private const float Z_NARR =  0.42f;
        private const float R_WIDE =  0.80f;
        private const float R_NARR =  0.34f;
        private const float EPS    =  0.018f; // z-fighting 방지 반경 여유

        private void Start()
        {
            BuildZoneOverlays();
            _ready = true;
            if (_pendingJson != null)
            {
                ApplyJson(_pendingJson);
                _pendingJson = null;
            }
        }

        private void BuildZoneOverlays()
        {
            float zRange = Z_NARR - Z_WIDE;  // 0.84
            float rRange = R_WIDE - R_NARR;  // 0.46
            float zoneH  = zRange / N;       // 0.084

            for (int r = 0; r < 3; r++)
            {
                _zones[r] = new RollTireZone[N];
                int rollIndex = r + 1;

                // 타이어 GameObject 탐색 (빌더가 붙인 이름과 일치)
                GameObject tireGo = GameObject.Find($"Replaceable Asymmetric Roll Tire {rollIndex}");
                if (tireGo == null)
                {
                    Debug.LogWarning($"[WearController] 타이어 없음: Roll Tire {rollIndex}");
                    continue;
                }
                Transform tireTf = tireGo.transform;

                for (int i = 0; i < N; i++)
                {
                    // i=0: 와이드끝(Zone10, ZoneIndex 0), i=9: 내로우끝(Zone1, ZoneIndex 9)
                    float zCenter = Z_WIDE + (i + 0.5f) * zoneH;
                    float rBot    = R_WIDE -  i      * (rRange / N) + EPS;
                    float rTop    = R_WIDE - (i + 1) * (rRange / N) + EPS;

                    GameObject go = new GameObject($"WearZone_R{rollIndex}_Z{i}");
                    go.transform.SetParent(tireTf, false);
                    go.transform.localPosition = new Vector3(0f, zCenter, 0f);

                    // 런타임에 메시 생성 → 씬 직렬화 의존성 없음
                    go.AddComponent<MeshFilter>().sharedMesh =
                        MeshFactory.Frustum(rBot, rTop, zoneH, 48);
                    go.AddComponent<MeshRenderer>();

                    RollTireZone rtz = go.AddComponent<RollTireZone>();
                    rtz.ZoneIndex = i;
                    _zones[r][i] = rtz;
                }
            }
        }

        // JS SendMessage 수신
        // json: {"r1":[w0..w9],"r2":[...],"r3":[...]}
        // w[i] 0.0~1.0, ZoneIndex i (0=와이드끝/Zone10, 9=내로우끝/Zone1)
        public void UpdateZoneWear(string json)
        {
            if (!_ready) { _pendingJson = json; return; }
            ApplyJson(json);
        }

        private void ApplyJson(string json)
        {
            WearPayload p;
            try   { p = JsonUtility.FromJson<WearPayload>(json); }
            catch { Debug.LogWarning("[WearController] JSON 파싱 오류"); return; }

            ApplyToRoller(0, p.r1);
            ApplyToRoller(1, p.r2);
            ApplyToRoller(2, p.r3);
        }

        private void ApplyToRoller(int ri, float[] values)
        {
            if (values == null || _zones[ri] == null) return;
            for (int zi = 0; zi < N && zi < values.Length; zi++)
                _zones[ri][zi]?.Apply(values[zi]);
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
