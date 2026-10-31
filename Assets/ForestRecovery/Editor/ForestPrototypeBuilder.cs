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

    }
}
