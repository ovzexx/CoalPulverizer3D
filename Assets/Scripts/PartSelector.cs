using UnityEngine;

namespace CoalPulverizer
{
    // 마우스 클릭을 감지하여 3D 부품을 선택. 드래그와 클릭을 구분함.
    public sealed class PartSelector : MonoBehaviour
    {
        [SerializeField] private float dragThresholdPixels = 8f;
        [SerializeField] private float raycastDistance = 100f;

        private PartHighlighter _highlighter;
        private UI.PartInfoPanel _infoPanel;
        private Vector2 _mouseDownPos;
        private bool _isDragging;

        private void Start()
        {
            _highlighter = FindFirstObjectByType<PartHighlighter>();
            _infoPanel   = FindFirstObjectByType<UI.PartInfoPanel>();
            Debug.Log($"[PartSelector] Start — highlighter={_highlighter != null}, infoPanel={_infoPanel != null}");
        }

        private void Update()
        {
            if (!Application.isPlaying) return;

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
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance))
            {
                Debug.Log($"[PartSelector] Hit: {hit.collider.gameObject.name}");
                PartTag tag = hit.collider.GetComponentInParent<PartTag>();
                if (tag != null)
                {
                    Debug.Log($"[PartSelector] PartTag id={tag.PartId}");
                    PartData data = PartDatabase.GetById(tag.PartId);
                    Debug.Log($"[PartSelector] PartData={data?.KoreanName ?? "NULL"}");
                    _highlighter?.Select(tag.PartId);
                    _infoPanel?.Show(data);
                    return;
                }
                Debug.Log("[PartSelector] PartTag not found on hit object");
            }
            else
            {
                Debug.Log("[PartSelector] Raycast hit nothing");
            }

            // 빈 공간 클릭 → 선택 해제
            _highlighter?.Deselect();
            _infoPanel?.Hide();
        }
    }
}
