using UnityEngine;

namespace CoalPulverizer
{
    public sealed class RollTireZone : MonoBehaviour
    {
        public int   ZoneIndex;
        [HideInInspector] public float Wear;

        private static readonly Color ColLow  = new Color(1.00f, 0.88f, 0.00f);
        private static readonly Color ColMid  = new Color(1.00f, 0.45f, 0.00f);
        private static readonly Color ColHigh = new Color(0.96f, 0.14f, 0.14f);
        private const float Alpha = 0.50f;

        private Material _mat;

        private void Awake() => Apply(Wear);

        public void Apply(float wear)
        {
            Wear = Mathf.Clamp01(wear);
            var rend = GetComponent<Renderer>();
            if (rend == null) return;

            if (_mat == null)
            {
                Shader sh = Shader.Find("NEXTRO/ZoneOverlay");
                if (sh == null)
                {
                    Debug.LogWarning("[RollTireZone] NEXTRO/ZoneOverlay shader not found.");
                    return;
                }
                _mat = new Material(sh) { name = "ZoneOverlay_" + ZoneIndex };
            }

            Color c = WearColor(Wear);
            c.a = Wear < 0.05f ? 0f : Alpha;
            _mat.SetColor("_Color", c);
            rend.sharedMaterial = _mat;
        }

        private static Color WearColor(float w)
        {
            return w < 0.5f
                ? Color.Lerp(ColLow, ColMid,  w * 2f)
                : Color.Lerp(ColMid, ColHigh, (w - 0.5f) * 2f);
        }
    }
}
