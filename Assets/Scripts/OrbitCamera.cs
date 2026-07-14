using UnityEngine;

namespace CoalPulverizer
{
    public sealed class OrbitCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float distance = 12f;
        [SerializeField] private float yaw = -35f;
        [SerializeField] private float pitch = 38f;
        [SerializeField] private float rotateSpeed = 140f;
        [SerializeField] private float zoomSpeed = 4f;

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            if (Input.GetMouseButton(0))
            {
                yaw += Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime;
                pitch -= Input.GetAxis("Mouse Y") * rotateSpeed * Time.deltaTime;
                pitch = Mathf.Clamp(pitch, -15f, 75f);
            }

            distance -= Input.mouseScrollDelta.y * zoomSpeed * Time.deltaTime * 10f;
            distance = Mathf.Clamp(distance, 4f, 28f);

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 focus = target.position + Vector3.up * 3.8f;
            transform.position = focus + rotation * new Vector3(0f, 0f, -distance);
            transform.LookAt(focus);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public void SetOrbit(float newDistance, float newYaw, float newPitch)
        {
            distance = newDistance;
            yaw = newYaw;
            pitch = newPitch;
        }
    }
}
