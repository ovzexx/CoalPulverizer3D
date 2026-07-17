using UnityEngine;

namespace CoalPulverizer
{
    public sealed class PartSelector : MonoBehaviour
    {
        [SerializeField] private float dragThresholdPixels = 8f;
        [SerializeField] private float raycastDistance = 100f;

        private PartHighlighter _highlighter;
        private UI.PartInfoPanel _infoPanel;
        private Vector2 _mouseDownPos;
        private bool _isDragging;

        // Debug
        private string _lastHit   = "none";
        private string _lastPartId = "none";
        private bool   _showDebug  = true;

        private void Start()
        {
            _highlighter = FindFirstObjectByType<PartHighlighter>();
            _infoPanel   = FindFirstObjectByType<UI.PartInfoPanel>();
            Debug.Log($"[PartSelector] Start — highlighter={_highlighter != null}, infoPanel={_infoPanel != null}");
        }

        private void Update()
        {
            if (!Application.isPlaying) return;

            // F1: 디버그 오버레이 토글
            if (Input.GetKeyDown(KeyCode.F1))
                _showDebug = !_showDebug;

            // Space: 롤타이어 패널 강제 표시 (레이캐스트 없이)
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("[PartSelector] SPACE pressed — force-show roll_assembly");
                var data = PartDatabase.GetById("roll_assembly");
                if (_infoPanel != null)
                    _infoPanel.Show(data);
                else
                    Debug.LogError("[PartSelector] _infoPanel is NULL!");
            }

            if (Input.GetMouseButtonDown(0))
            {
                _mouseDownPos = Input.mousePosition;
                _isDragging   = false;
            }

            if (Input.GetMouseButton(0))
            {
                if (Vector2.Distance(Input.mousePosition, _mouseDownPos) > dragThresholdPixels)
                    _isDragging = true;
            }

            if (Input.GetMouseButtonUp(0) && !_isDragging)
                TrySelect();
        }

        private void TrySelect()
        {
            if (Camera.main == null)
            {
                Debug.LogError("[PartSelector] Camera.main is NULL!");
                _lastHit = "NO CAMERA";
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance))
            {
                _lastHit = hit.collider.gameObject.name;
                Debug.Log($"[PartSelector] Hit: {hit.collider.gameObject.name}");

                PartTag tag = hit.collider.GetComponentInParent<PartTag>();
                if (tag != null)
                {
                    _lastPartId = tag.PartId;
                    Debug.Log($"[PartSelector] PartTag id={tag.PartId}");
                    PartData data = PartDatabase.GetById(tag.PartId);
                    Debug.Log($"[PartSelector] PartData={data?.KoreanName ?? "NULL"}");

                    try { _highlighter?.Select(tag.PartId); }
                    catch (System.Exception e) { Debug.LogError($"[Highlighter] {e.Message}"); }

                    try { _infoPanel?.Show(data); }
                    catch (System.Exception e) { Debug.LogError($"[InfoPanel.Show] {e.Message}\n{e.StackTrace}"); }
                    return;
                }
                _lastPartId = "NO TAG";
                Debug.Log("[PartSelector] PartTag not found on hit object");
            }
            else
            {
                _lastHit = "nothing";
                Debug.Log("[PartSelector] Raycast hit nothing");
            }

            // 빈 공간 클릭
            _lastPartId = "-";
            _highlighter?.Deselect();
            _infoPanel?.Hide();
        }

        private void OnGUI()
        {
            if (!Application.isPlaying || !_showDebug) return;

            var style = new GUIStyle(GUI.skin.box)
            {
                fontSize  = 14,
                alignment = TextAnchor.UpperLeft,
                richText  = true,
            };
            style.normal.textColor = Color.white;

            string panelBuilt = (_infoPanel != null) ? "<color=lime>OK</color>" : "<color=red>NULL</color>";
            string hlBuilt    = (_highlighter != null) ? "<color=lime>OK</color>" : "<color=red>NULL</color>";
            string msg =
                $"[F1: 숨기기  /  Space: 패널 강제표시]\n" +
                $"InfoPanel : {panelBuilt}\n" +
                $"Highlighter: {hlBuilt}\n" +
                $"Last Hit   : {_lastHit}\n" +
                $"Last PartId: {_lastPartId}";

            GUI.Box(new Rect(10, 10, 340, 110), msg, style);
        }
    }
}
