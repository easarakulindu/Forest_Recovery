using UnityEngine;
using UnityEngine.InputSystem;

namespace ForestRecovery
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class RangerMotor : MonoBehaviour
    {
        public Transform cameraTransform;
        public Transform body;
        public float walkSpeed = 4.5f;
        public bool Grounded => controller != null && controller.isGrounded;
        private CharacterController controller;
        private float fallSpeed;

        private void Awake() => controller = GetComponent<CharacterController>();

        private void Update()
        {
            Vector2 input = Vector2.zero;
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                input.x = (keyboard.dKey.isPressed ? 1 : 0) - (keyboard.aKey.isPressed ? 1 : 0);
                input.y = (keyboard.wKey.isPressed ? 1 : 0) - (keyboard.sKey.isPressed ? 1 : 0);
            }
            // Clamp prevents diagonal movement from being faster than straight movement.
            Vector3 direction = Vector3.ClampMagnitude(new Vector3(input.x, 0, input.y), 1);
            if (cameraTransform != null)
                direction = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0) * direction;
            if (direction.sqrMagnitude > 0.001f && body != null)
                body.rotation = Quaternion.Slerp(body.rotation, Quaternion.LookRotation(direction), 14 * Time.deltaTime);

            if (controller.isGrounded && fallSpeed < 0) fallSpeed = -2;
            fallSpeed = Mathf.Max(fallSpeed - 22 * Time.deltaTime, -30);
            controller.Move((direction * walkSpeed + Vector3.up * fallSpeed) * Time.deltaTime);
        }
    }
}
