using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace ForestRecovery.Editor
{
    // Editor-only authoring helper. The saved scene works without running this builder.
    public static class ForestPrototypeBuilder
    {
        public const string ScenePath = "Assets/ForestRecovery/Scenes/Stage01_Greybox.unity";

        [MenuItem("Forest Recovery/Open Stage 1 Prototype")]
        public static void OpenPrototype()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!File.Exists(ScenePath)) CreateScene();
            else EditorSceneManager.OpenScene(ScenePath);
        }

        public static void CreateScene()
        {
            if (File.Exists(ScenePath))
                throw new System.InvalidOperationException("Scene already exists. Open it instead of overwriting it.");
            CreateStructure(out var camera);
            var environment = AddEnvironment();
            var ranger = AddRanger(camera);
            SaveSceneAndLaunch();
        }

        internal static void CreateStructure(out Camera camera)
        {
            Directory.CreateDirectory("Assets/ForestRecovery/Scenes");
            AssetDatabase.Refresh();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            camera = cameraObject.AddComponent<Camera>();
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 120;
            camera.fieldOfView = 60;
            camera.backgroundColor = new Color(0.66f, 0.78f, 0.77f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            cameraObject.AddComponent<AudioListener>();
            cameraObject.transform.SetPositionAndRotation(new Vector3(0, 5.11f, -14.94f), Quaternion.Euler(32, 0, 0));
            var light = new GameObject("Sun").AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.4f;
            light.color = new Color(1, 0.94f, 0.81f);
            light.shadows = LightShadows.Soft;
            light.transform.rotation = Quaternion.Euler(48, -30, 0);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.62f, 0.7f, 0.68f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = camera.backgroundColor;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 28;
            RenderSettings.fogEndDistance = 75;
            RenderSettings.sun = light;
        }

        internal static void SaveSceneAndLaunch()
        {
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);
            var scenes = EditorBuildSettings.scenes.Where(s => s.path != ScenePath).ToList();
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
        }

        internal static Transform AddEnvironment()
        {
            Directory.CreateDirectory("Assets/ForestRecovery/Materials");
            AssetDatabase.Refresh();
            var healthy = Material("Healthy moss", new Color(0.25f, 0.43f, 0.28f));
            var damaged = Material("Damaged earth", new Color(0.48f, 0.36f, 0.24f));
            var bark = Material("Bark", new Color(0.28f, 0.20f, 0.13f));
            var leaves = Material("Canopy", new Color(0.15f, 0.36f, 0.23f));
            var stone = Material("Stone", new Color(0.43f, 0.49f, 0.46f));
            Transform environment = new GameObject("Forest / editable greybox").transform;
            Shape("Healthy forest ground", PrimitiveType.Cube, new Vector3(-8, -0.3f, 2), new Vector3(16, 0.6f, 30), healthy, environment);
            Shape("Damaged clearing ground", PrimitiveType.Cube, new Vector3(8, -0.3f, 2), new Vector3(16, 0.6f, 30), damaged, environment);
            // A visible perimeter prevents the ranger leaving the small demo area.
            Shape("North ridge", PrimitiveType.Cube, new Vector3(0, 0.65f, 17), new Vector3(33, 1.3f, 1), stone, environment);
            Shape("South ridge", PrimitiveType.Cube, new Vector3(0, 0.65f, -13), new Vector3(33, 1.3f, 1), stone, environment);
            Shape("West ridge", PrimitiveType.Cube, new Vector3(-16, 0.65f, 2), new Vector3(1, 1.3f, 30), stone, environment);
            Shape("East ridge", PrimitiveType.Cube, new Vector3(16, 0.65f, 2), new Vector3(1, 1.3f, 30), stone, environment);

            Vector3[] treePositions = { new Vector3(-12, 0, -8), new Vector3(-7, 0, -7), new Vector3(-12, 0, -1),
                new Vector3(-10, 0, 5), new Vector3(-13, 0, 11), new Vector3(-7, 0, 12), new Vector3(-3, 0, 14),
                new Vector3(12, 0, 13), new Vector3(13, 0, -8) };
            for (int i = 0; i < treePositions.Length; i++)
                Tree("Mature tree " + (i + 1), treePositions[i], 1 + (i % 3) * 0.12f, bark, leaves, environment);
            Vector3[] stumps = { new Vector3(6, 0, -6), new Vector3(10, 0, -2), new Vector3(11, 0, 5), new Vector3(7, 0, 11) };
            foreach (var point in stumps)
                Shape("Logging stump", PrimitiveType.Cylinder, point + Vector3.up * 0.3f, new Vector3(1, 0.3f, 1), bark, environment);
            Vector3[] rocks = { new Vector3(-9, 0, -3), new Vector3(10, 0, 8), new Vector3(5, 0, 14), new Vector3(-11, 0, 8) };
            foreach (var point in rocks)
                Shape("Rock", PrimitiveType.Sphere, point + Vector3.up * 0.45f, new Vector3(1.8f, 1.1f, 1.5f), stone, environment);

            return environment;
        }

        internal static RangerMotor AddRanger(Camera camera)
        {
            var bark = Material("Bark", new Color(0.28f, 0.20f, 0.13f));
            var uniform = Material("Ranger jacket", new Color(0.71f, 0.31f, 0.14f));
            var skin = Material("Ranger face", new Color(0.77f, 0.57f, 0.36f));
            var dark = Material("Ranger boots", new Color(0.13f, 0.20f, 0.19f));
            var player = new GameObject("Ranger / WASD");
            player.layer = 2; // Ignore Raycast: camera and interaction checks ignore the ranger.
            player.transform.position = new Vector3(0, 0.15f, -9);
            var controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.33f;
            controller.center = new Vector3(0, 0.9f, 0);
            controller.stepOffset = 0.3f;
            controller.slopeLimit = 45;
            var body = new GameObject("Ranger greybox model").transform;
            body.SetParent(player.transform, false);
            Shape("Jacket", PrimitiveType.Capsule, player.transform.position + Vector3.up * 0.85f,
                new Vector3(0.65f, 0.55f, 0.55f), uniform, body, false);
            Shape("Head", PrimitiveType.Sphere, player.transform.position + Vector3.up * 1.55f,
                Vector3.one * 0.42f, skin, body, false);
            Shape("Hat brim", PrimitiveType.Cylinder, player.transform.position + Vector3.up * 1.76f,
                new Vector3(0.7f, 0.035f, 0.7f), bark, body, false);
            Shape("Hat crown", PrimitiveType.Cylinder, player.transform.position + Vector3.up * 1.86f,
                new Vector3(0.4f, 0.1f, 0.4f), bark, body, false);
            for (int side = -1; side <= 1; side += 2)
                Shape("Boot", PrimitiveType.Cube, player.transform.position + new Vector3(side * 0.17f, 0.2f, 0.08f),
                    new Vector3(0.23f, 0.4f, 0.32f), dark, body, false);
            var motor = player.AddComponent<RangerMotor>();
            motor.body = body;

            motor.cameraTransform = camera.transform;
            return motor;
        }

        private static Material Material(string name, Color color)
        {
            string path = "Assets/ForestRecovery/Materials/" + name.Replace(" ", "_") + ".mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name };
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", 0.05f);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static GameObject Shape(string name, PrimitiveType type, Vector3 position, Vector3 scale,
            Material material, Transform parent, bool collision = true)
        {
            var shape = GameObject.CreatePrimitive(type);
            shape.name = name;
            shape.transform.SetParent(parent, false);
            shape.transform.position = position;
            shape.transform.localScale = scale;
            shape.GetComponent<Renderer>().sharedMaterial = material;
            if (!collision) Object.DestroyImmediate(shape.GetComponent<Collider>());
            return shape;
        }

        private static void Tree(string name, Vector3 point, float size, Material bark, Material leaves, Transform parent)
        {
            Shape(name + " / trunk", PrimitiveType.Cylinder, point + Vector3.up * 1.25f * size,
                new Vector3(0.6f, 1.25f, 0.6f) * size, bark, parent);
            Shape(name + " / crown", PrimitiveType.Sphere, point + Vector3.up * 3 * size,
                new Vector3(2.8f, 2.9f, 2.8f) * size, leaves, parent, false);
        }
    }
}
