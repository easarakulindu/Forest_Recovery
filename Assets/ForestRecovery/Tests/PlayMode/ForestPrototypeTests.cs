using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ForestRecovery.Tests
{
    public sealed class ForestPrototypeTests
    {
        private const string ScenePath = "Assets/ForestRecovery/Scenes/Stage01_Greybox.unity";
        private ForestMission mission;
        private Keyboard keyboard;
        private InputSettings.BackgroundBehavior previousBackgroundBehavior;
        private bool previousRunInBackground;
        private float previousCaptureDeltaTime;
#if UNITY_EDITOR
        private InputSettings.EditorInputBehaviorInPlayMode previousEditorBehavior;
        private UnityEditor.EditorWindow evidenceGameView;
        private bool previousAsyncCompilation;
#endif

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            previousBackgroundBehavior = InputSystem.settings.backgroundBehavior;
            previousRunInBackground = Application.runInBackground;
            previousCaptureDeltaTime = Time.captureDeltaTime;
            Time.captureDeltaTime = 1f / 60;
            Application.runInBackground = true;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
            previousEditorBehavior = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.editorInputBehaviorInPlayMode =
                InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            previousAsyncCompilation = UnityEditor.ShaderUtil.allowAsyncCompilation;
            if (!Application.isBatchMode)
            {
                // A separate floating view avoids altering the user's docked editor layout.
                evidenceGameView = (UnityEditor.EditorWindow)ScriptableObject.CreateInstance(
                    System.Type.GetType("UnityEditor.GameView,UnityEditor"));
                evidenceGameView.position = new Rect(30, 30, 1280, 760);
                evidenceGameView.Show();
                evidenceGameView.Focus();
                UnityEditor.ShaderUtil.allowAsyncCompilation = false;
            }
#endif
            yield return SceneManager.LoadSceneAsync(ScenePath);
            mission = Object.FindFirstObjectByType<ForestMission>();
            Assert.That(mission, Is.Not.Null);
            keyboard = InputSystem.AddDevice<Keyboard>();
            yield return null;
            yield return Frames(10);
        }

        [TearDown]
        public void TearDown()
        {
            if (keyboard != null && keyboard.added) InputSystem.RemoveDevice(keyboard);
            InputSystem.settings.backgroundBehavior = previousBackgroundBehavior;
            Application.runInBackground = previousRunInBackground;
            Time.captureDeltaTime = previousCaptureDeltaTime;
#if UNITY_EDITOR
            InputSystem.settings.editorInputBehaviorInPlayMode = previousEditorBehavior;
            UnityEditor.ShaderUtil.allowAsyncCompilation = previousAsyncCompilation;
            if (evidenceGameView != null) evidenceGameView.Close();
#endif
        }

        private void Keys(params Key[] keys) => InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));

        // Yield each frame explicitly: a hidden batch editor may not keep ticking the player
        // during a wall-clock WaitForSeconds. Fixed capture time makes distances repeatable.
        private static IEnumerator Frames(int count)
        {
            for (int frame = 0; frame < count; frame++) yield return null;
        }

        private void StandAt(Vector3 position)
        {
            var controller = mission.ranger.GetComponent<CharacterController>();
            controller.enabled = false;
            mission.ranger.transform.position = position;
            controller.enabled = true;
            Physics.SyncTransforms();
        }

        private IEnumerator PressInteract()
        {
            Keys(Key.E);
            yield return null;
            yield return null;
            Keys();
            yield return null;
        }

        [UnityTest]
        public IEnumerator KeyboardMovementGroundCollisionAndCameraFollow()
        {
            Vector3 start = mission.ranger.transform.position;
            var follow = Object.FindFirstObjectByType<RangerCamera>();
            Vector3 cameraStart = follow.transform.position;
            Keys(Key.W);
            yield return Frames(30);
            Keys();
            yield return null;
            float walkingDistance = mission.ranger.transform.position.z - start.z;
            Assert.That(walkingDistance, Is.GreaterThan(1.2f));
            Assert.That(follow.transform.position.z - cameraStart.z, Is.GreaterThan(0.7f));
            Assert.That(Mathf.Abs(mission.ranger.transform.position.y), Is.LessThan(0.15f));
            Assert.That(mission.ranger.Grounded, Is.True);

            // Verify the other three keys, using the same camera orientation.
            foreach (var key in new[] { Key.A, Key.S, Key.D })
            {
                start = mission.ranger.transform.position;
                Keys(key);
                yield return Frames(30);
                Keys();
                yield return null;
                Vector3 delta = mission.ranger.transform.position - start;
                if (key == Key.A) Assert.That(delta.x, Is.LessThan(-1.2f));
                if (key == Key.S) Assert.That(delta.z, Is.LessThan(-1.2f));
                if (key == Key.D) Assert.That(delta.x, Is.GreaterThan(1.2f));
            }

            var camera = follow;
            float yaw = camera.yaw;
            Keys(Key.RightArrow);
            yield return Frames(12);
            Keys();
            yield return null;
            Assert.That(camera.yaw, Is.GreaterThan(yaw + 5));
            camera.yaw = 0;
            StandAt(new Vector3(15, 0.05f, 0));
            Keys(Key.D);
            yield return Frames(30);
            Keys();
            yield return null;
            Assert.That(mission.ranger.transform.position.x, Is.LessThan(15.3f), "Perimeter must block the ranger.");
        }

        [UnityTest]
        public IEnumerator FullLoopThroughKeyboardUpdatesResourcesVisualsAndHealth()
        {
            Assert.That(mission.State.Health, Is.EqualTo(40));
            Assert.That(mission.interactables.Length, Is.EqualTo(6));
            yield return Capture("stage01-start.png");
            foreach (ForestAction action in new[] { ForestAction.Seed, ForestAction.Plant })
            {
                foreach (var item in mission.interactables.Where(i => i.action == action))
                {
                    StandAt(item.transform.position + new Vector3(0, 0.08f, -1.4f));
                    yield return null;
                    Assert.That(mission.FindNearby(), Is.SameAs(item));
                    yield return PressInteract();
                    Assert.That(item.Used, Is.True);
                    Assert.That(item.availableVisual.activeSelf, Is.False);
                    if (item.restoredVisual != null) Assert.That(item.restoredVisual.activeSelf, Is.True);
                    Assert.That(mission.State.Health, Is.EqualTo(40 + mission.State.TreesPlanted * 20));
                    int health = mission.State.Health;
                    Assert.That(mission.TryInteract(), Is.False, "Used objects cannot award points twice.");
                    Assert.That(mission.State.Health, Is.EqualTo(health));
                }
            }
            Assert.That(mission.State.SeedsCollected, Is.EqualTo(3));
            Assert.That(mission.State.Seeds, Is.Zero);
            Assert.That(mission.State.TreesPlanted, Is.EqualTo(3));
            Assert.That(mission.State.Health, Is.EqualTo(100));
            Assert.That(mission.State.Complete, Is.True);
            StandAt(new Vector3(0, 0.05f, -3));
            yield return Frames(18);
            yield return Capture("stage01-restored.png");
        }

        [UnityTest]
        public IEnumerator PlantingRejectsMissingResourcesAndRemoteInteractions()
        {
            Assert.That(mission.TryInteract(), Is.False, "Spawn must be outside interaction range.");
            var plot = mission.interactables.First(i => i.action == ForestAction.Plant);
            StandAt(plot.transform.position + new Vector3(0, 0.05f, -1.4f));
            yield return PressInteract();
            Assert.That(plot.Used, Is.False);
            Assert.That(plot.restoredVisual.activeSelf, Is.False);
            Assert.That(mission.State.Health, Is.EqualTo(40));
            Assert.That(mission.Feedback, Does.Contain("needs 1 seed"));

            var seed = mission.interactables.First(i => i.action == ForestAction.Seed);
            StandAt(seed.transform.position + new Vector3(0, 0.05f, -1.4f));
            var obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstacle.transform.position = seed.transform.position + new Vector3(0, 0.7f, -0.7f);
            obstacle.transform.localScale = new Vector3(2, 1.4f, 0.2f);
            Physics.SyncTransforms();
            Assert.That(mission.TryInteract(), Is.False, "Cannot collect through a solid obstacle.");
            Object.Destroy(obstacle);
            yield return null;
        }

        [UnityTest]
        public IEnumerator CameraMovesInFrontOfAnObstruction()
        {
            var follow = Object.FindFirstObjectByType<RangerCamera>();
            var obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstacle.transform.position = mission.ranger.transform.position + new Vector3(0, 2.6f, -3);
            obstacle.transform.localScale = new Vector3(3, 4, 0.5f);
            Physics.SyncTransforms();
            yield return Frames(3);
            Vector3 focus = mission.ranger.transform.position + Vector3.up * 1.25f;
            Assert.That(Vector3.Distance(follow.transform.position, focus), Is.LessThan(4));
            Assert.That(Physics.Linecast(focus, follow.transform.position,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore), Is.False);
            Object.Destroy(obstacle);
            yield return null;
        }

        private static IEnumerator Capture(string name)
        {
            if (Application.isBatchMode || SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) yield break;
            string directory = Path.Combine(Application.dataPath, "../Documentation/Evidence");
            Directory.CreateDirectory(directory);
            yield return new WaitForSeconds(0.5f);
            ScreenCapture.CaptureScreenshot(Path.Combine(directory, name));
            for (int frame = 0; frame < 12; frame++) yield return null;
        }
    }
}
