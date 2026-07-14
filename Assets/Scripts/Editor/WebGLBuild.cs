#if UNITY_EDITOR
using System.IO;
using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CoalPulverizerEditor
{
    public static class WebGLBuild
    {
        private const string ScenePath = "Assets/Scenes/CoalPulverizerDemo.unity";
        private const string OutputPath = "Builds/unity_webgl";

        [MenuItem("Tools/Coal Pulverizer/Build WebGL Prototype")]
        public static void BuildPrototype()
        {
            CoalPulverizerSceneCreator.CreateDemoScene();
            EditorSceneManager.OpenScene(ScenePath);

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };

            Directory.CreateDirectory(OutputPath);

            PlayerSettings.companyName = "NEXTRO";
            PlayerSettings.productName = "BowlMillPrototype";
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback = false;
            PlayerSettings.WebGL.template = "APPLICATION:Default";

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = OutputPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException("WebGL build failed: " + report.summary.result);
            }

            Debug.Log("[CoalPulverizer] WebGL build complete: " + Path.GetFullPath(OutputPath));
        }
    }
}
#endif
