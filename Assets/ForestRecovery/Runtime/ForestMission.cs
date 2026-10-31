using UnityEngine;
using UnityEngine.InputSystem;

namespace ForestRecovery
{
    public sealed class ForestMission : MonoBehaviour
    {
        public RangerMotor ranger;
        public ForestInteractable[] interactables;
        public float interactionRange = 2.2f;
        public RestorationState State { get; private set; }
        public ForestInteractable Nearby { get; private set; }
        public string Feedback { get; private set; } = "Collect the three golden seeds.";

        private void Awake() => State = new RestorationState();

        private void Update()
        {
            Nearby = FindNearby();
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) TryInteract();
        }

        public ForestInteractable FindNearby()
        {
            ForestInteractable nearest = null;
            float bestDistance = interactionRange;
            Vector3 origin = ranger.transform.position + Vector3.up * 0.6f;
            foreach (var item in interactables)
            {
                if (item == null || item.Used) continue;
                float distance = Vector3.Distance(item.transform.position, ranger.transform.position);
                Vector3 destination = item.transform.position + Vector3.up * 0.4f;
                // Do not allow interactions through a tree trunk, rock or boundary wall.
                if (distance < bestDistance && !Physics.Linecast(origin, destination,
                    Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                {
                    bestDistance = distance;
                    nearest = item;
                }
            }
            return nearest;
        }

        public bool TryInteract()
        {
            var item = FindNearby();
            if (item == null) return false;
            bool success = item.Apply(State);
            Feedback = success ? "Seed collected!" : "This item cannot be used.";
            return success;
        }

        private GUIStyle title, text, small;

        private void OnGUI()
        {
            if (State == null) return;
            if (title == null)
            {
                title = Style(26, FontStyle.Bold);
                text = Style(19);
                small = Style(16);
            }
            // Draw in a 1280 x 720 coordinate system, scaled to the actual Game view.
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            Matrix4x4 previous = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(Vector3.one * scale);
            float width = Screen.width / scale;
            float height = Screen.height / scale;
            Panel(new Rect(24, 24, 440, 160));
            GUI.Label(new Rect(42, 36, 410, 34), "FOREST RECOVERY", title);
            GUI.Label(new Rect(42, 76, 410, 30), "Seeds held: " + State.Seeds, text);

            Panel(new Rect(width - 344, 24, 320, 150));
            GUI.Label(new Rect(width - 326, 38, 290, 26), "CONTROLS", text);
            GUI.Label(new Rect(width - 326, 73, 290, 90), "WASD  Move\nHold right mouse  Look around\nLeft / Right arrows  Turn camera\nE  Collect    Esc  Cursor", small);

            Panel(new Rect(24, height - 148, width - 48, 124));
            GUI.Label(new Rect(44, height - 138, width - 88, 30), "Collect the golden seeds.", text);
            string instruction = Nearby != null ? "[ E ]  " + Nearby.Label : "Walk up to a golden seed.";
            GUI.Label(new Rect(44, height - 100, width - 88, 28), instruction, text);
            GUI.Label(new Rect(44, height - 64, width - 88, 26), Feedback, small);
            GUI.matrix = previous;
        }

        private static GUIStyle Style(int size, FontStyle weight = FontStyle.Normal) =>
            new GUIStyle(GUI.skin.label) { fontSize = size, fontStyle = weight, normal = { textColor = Color.white } };

        private static void Panel(Rect rect)
        {
            Color previous = GUI.color;
            GUI.color = new Color(0.045f, 0.11f, 0.12f, 0.96f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }
    }
}
