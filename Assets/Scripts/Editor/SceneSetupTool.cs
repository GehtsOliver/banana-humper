using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using BananaHumper.Bootstrap;

namespace BananaHumper.EditorTools
{
    /// <summary>
    /// Baut die minimale Bootstrap-Szene (siehe README "Projekt öffnen und
    /// spielen") und speichert sie unter Assets/Scenes/Main.unity, damit man
    /// nicht bei jedem Test von Hand eine leere Szene anlegen und
    /// GameBootstrap draufziehen muss. Nur ein Editor-Werkzeug, nicht Teil
    /// des Laufzeit-Codes.
    /// </summary>
    public static class SceneSetupTool
    {
        const string ScenePath = "Assets/Scenes/Main.unity";

        [MenuItem("BananaHumper/Bootstrap-Szene erzeugen")]
        public static void CreateMainScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var go = new GameObject("GameBootstrap");
            go.AddComponent<GameBootstrap>();

            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            Debug.Log($"SceneSetupTool: {ScenePath} erzeugt.");
        }
    }
}
