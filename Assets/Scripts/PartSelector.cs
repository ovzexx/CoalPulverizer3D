using UnityEngine;

namespace CoalPulverizer
{
    public sealed class PartSelector : MonoBehaviour
    {
        [SerializeField] private float dragThresholdPixels = 8f;
        [SerializeField] private float raycastDistance = 100f;

        private PartHighlighter _highlighter;
        private Vector2 _mouseDownPos;
        private bool _isDragging;

        private void Start()
        {
            _highlighter = FindFirstObjectByType<PartHighlighter>();
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
            if (Camera.main == null) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance))
            {
                PartTag tag = hit.collider.GetComponentInParent<PartTag>();
                if (tag != null)
                {
                    try { _highlighter?.Select(tag.PartId); }
                    catch (System.Exception e) { Debug.LogError($"[Highlighter] {e.Message}"); }

                    JSBridge.NotifyPartSelected(tag.PartId);
                    return;
                }
            }

            _highlighter?.Deselect();
            JSBridge.NotifyPartSelected("");
        }
    }
}
