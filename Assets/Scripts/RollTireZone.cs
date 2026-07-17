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
            // Unlit 셰이더 사용: 조명 계산 없이 순수 색상만 표시
            // Lit 셰이더는 씬 포인트 라이트(Grinding Zone Light)를 받아 색이 실제보다
            // 과장되게 밝아져 타이어 원본 모양이 안 보이는 문제가 있음
            Shader sh = Shader.Find("Universal Render Pipeline/Unlit")
                     ?? Shader.Find("Unlit/Transparent")
                     ?? Shader.Find("Unlit/Color");

            if (sh == null)
            {
                Debug.LogWarning("[RollTireZone] No Unlit shader found.");
                return null;
            }

            var mat = new Material(sh) { name = "ZoneOverlay_" + ZoneIndex };

            bool isURPUnlit = sh.name.Contains("Universal Render Pipeline");
            if (isURPUnlit)
            {
                mat.SetFloat("_Surface",   1f); // Transparent
                mat.SetFloat("_Blend",     0f); // Alpha blend
                mat.SetFloat("_AlphaClip", 0f);
                mat.SetFloat("_ZWrite",    0f);
                mat.SetFloat("_Cull",      2f); // Back culling
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.renderQueue = 3100;
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            }
            else
            {
                // Unlit/Transparent 등 기본 유니티 셰이더 폴백
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
