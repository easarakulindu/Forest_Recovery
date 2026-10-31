using UnityEngine;
using UnityEngine.InputSystem;

namespace ForestRecovery
{
    public sealed class RangerCamera : MonoBehaviour
    {
        public Transform target;
        public float distance = 7;
        public float yaw;
        public float pitch = 32;
        private Vector3 smoothedFocus;
        private bool initialized;
        private bool lookSuppressed;

        private void LateUpdate()
        {
            if (target == null) return;
            var mouse = Mouse.current;
            var keyboard = Keyboard.current;
            bool released = keyboard != null && keyboard.escapeKey.wasPressedThisFrame;
            if (released) lookSuppressed = true;
            if (mouse == null || !mouse.rightButton.isPressed) lookSuppressed = false;
            bool looking = !lookSuppressed && Application.isFocused && mouse != null && mouse.rightButton.isPressed;
            Cursor.lockState = looking ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !looking;
            if (looking)
            {
                Vector2 delta = mouse.delta.ReadValue();
                yaw += delta.x * 0.15f;
                pitch = Mathf.Clamp(pitch - delta.y * 0.15f, 12, 65);
            }
            if (keyboard != null)
            {
                yaw += ((keyboard.rightArrowKey.isPressed ? 1 : 0) -
                    (keyboard.leftArrowKey.isPressed ? 1 : 0)) * 100 * Time.deltaTime;
            }
            Vector3 focus = target.position + Vector3.up * 1.25f;
            smoothedFocus = initialized ? Vector3.Lerp(smoothedFocus, focus, 1 - Mathf.Exp(-14 * Time.deltaTime)) : focus;
            initialized = true;
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
            Vector3 backward = rotation * Vector3.back;
            // Ranger uses Ignore Raycast; collectibles and foliage have no colliders.
            float length = distance;
            if (Physics.SphereCast(smoothedFocus, 0.2f, backward, out RaycastHit hit,
                distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                length = Mathf.Max(0.35f, hit.distance - 0.12f);
            transform.SetPositionAndRotation(smoothedFocus + backward * length, rotation);
        }

        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
