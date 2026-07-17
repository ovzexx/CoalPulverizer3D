using UnityEngine;

namespace CoalPulverizer
{
    // 롤 타이어 위에 씌우는 10구간 마모 오버레이. 반투명 색상으로 마모 강도 시각화.
    public sealed class RollTireZone : MonoBehaviour
    {
        public int   ZoneIndex; // 0 = 와이드끝(하우징), 9 = 내로우끝(중심)
        [HideInInspector] public float Wear; // 0~1

        private static readonly Color ColLow  = new Color(1.00f, 0.88f, 0.00f); // 노랑
        private static readonly Color ColMid  = new Color(1.00f, 0.45f, 0.00f); // 주황
        private static readonly Color ColHigh = new Color(0.96f, 0.14f, 0.14f); // 빨강
        private const float Alpha = 0.42f;

        private Material _mat;

        private void Awake() => Apply(Wear);
        private void Start()  => Apply(Wear);

        public void Apply(float wear)
        {
            Wear = Mathf.Clamp01(wear);
            var rend = GetComponent<Renderer>();
            if (rend == null) return;

            if (_mat == null)
                _mat = BuildTransparentMat();

            if (_mat == null) return;

            Color c = WearColor(Wear);
            // 마모율 5% 미만이면 완전 투명 — 초기 "기본" 상태를 깔끔하게 유지
            float a = Wear < 0.05f ? 0f : Alpha;
            c.a = a;
            if (_mat.HasProperty("_BaseColor")) _mat.SetColor("_BaseColor", c);
            if (_mat.HasProperty("_Color"))     _mat.SetColor("_Color",     c);
            _mat.color = c;

            // sharedMaterial: 복사본 없이 직접 참조 → 이후 색상 갱신도 즉시 반영
            rend.sharedMaterial = _mat;
        }

        private Material BuildTransparentMat()
        {
            // 빌드에 포함된 셰이더 우선순위로 탐색
            // WebGL 빌드에 URP/Unlit이 없는 경우 Standard(항상 포함)로 폴백
            Shader sh = Shader.Find("Universal Render Pipeline/Unlit")
                     ?? Shader.Find("Unlit/Transparent")
                     ?? Shader.Find("Unlit/Color")
                     ?? Shader.Find("Standard");

            if (sh == null)
            {
                Debug.LogWarning("[RollTireZone] 사용 가능한 셰이더 없음.");
                return null;
            }

            var mat = new Material(sh) { name = "ZoneOverlay_" + ZoneIndex };

            if (sh.name.Contains("Universal Render Pipeline"))
            {
                // URP Unlit 투명 설정
                mat.SetFloat("_Surface",   1f);
                mat.SetFloat("_Blend",     0f);
                mat.SetFloat("_AlphaClip", 0f);
                mat.SetFloat("_ZWrite",    0f);
                mat.SetFloat("_Cull",      2f);
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.renderQueue = 3100;
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            }
            else if (sh.name == "Standard")
            {
                // Standard 셰이더 Transparent 모드 설정 (Built-in 파이프라인 호환)
                mat.SetFloat("_Mode", 3f);  // Transparent
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.renderQueue = 3000;
            }
            else
            {
                // Unlit/Transparent 등 레거시 폴백
                mat.renderQueue = 3100;
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.SetOverrideTag("RenderType", "Transparent");
            }

            return mat;
        }

        private static Color WearColor(float w)
        {
            return w < 0.5f
                ? Color.Lerp(ColLow, ColMid,  w * 2f)
                : Color.Lerp(ColMid, ColHigh, (w - 0.5f) * 2f);
        }
    }
}
