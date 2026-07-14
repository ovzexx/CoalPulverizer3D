#if UNITY_EDITOR
using System.Linq;
using CoalPulverizer;
using CoalPulverizer.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace CoalPulverizerEditor
{
    [InitializeOnLoad]
    public static class CoalPulverizerSceneCreator
    {
        private const string ScenePath = "Assets/Scenes/CoalPulverizerDemo.unity";

        static CoalPulverizerSceneCreator()
        {
            EditorApplication.delayCall += CreateDemoSceneIfMissing;
        }

        [MenuItem("Tools/Coal Pulverizer/Create Demo Scene")]
        public static void CreateDemoScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject root = new GameObject("Coal Pulverizer");
            CoalPulverizerBuilder builder = root.AddComponent<CoalPulverizerBuilder>();

            // CAD 롤 타이어 메시 자동 로드 및 할당
            // OBJ에 o/g 선언이 없으면 LoadAllAssetsAtPath가 Mesh를 반환하지 않을 수 있음
            // → 모델 프리팹을 LoadAssetAtPath<GameObject>로 로드하고 MeshFilter에서 메시를 추출
            const string cadPath = "Assets/Coal_Pulverizer.obj";
            AssetDatabase.ImportAsset(cadPath, ImportAssetOptions.ForceSynchronousImport);
            Mesh cadMesh = null;
            // 1차: 프리팹 MeshFilter 방식 (가장 안정적)
            GameObject cadPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(cadPath);
            if (cadPrefab != null)
            {
                MeshFilter mf = cadPrefab.GetComponentInChildren<MeshFilter>(true);
                if (mf != null)
                    cadMesh = mf.sharedMesh;
            }
            // 2차: 서브에셋 순회 (fallback)
            if (cadMesh == null)
            {
                cadMesh = AssetDatabase.LoadAllAssetsAtPath(cadPath)
                    .OfType<Mesh>()
                    .FirstOrDefault();
            }
            if (cadMesh != null)
                builder.rollTireCADMesh = cadMesh;
            else
                Debug.LogWarning("[CoalPulverizer] CAD mesh not found at " + cadPath + " — using procedural roll tire.");

            builder.Rebuild();

            // ── 인터랙션 시스템 ──────────────────────────────────────
            root.AddComponent<PartHighlighter>();
            root.AddComponent<PartSelector>();

            // Canvas + PartInfoPanel (우측 슬라이드인 패널)
            GameObject canvasGo = new GameObject("UI Canvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight  = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            canvasGo.AddComponent<PartInfoPanel>();
            // PartInfoPanel.Awake() calls Build() automatically at runtime
            // ──────────────────────────────────────────────────────────

            GameObject cameraObject = new GameObject("Orbit Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.fieldOfView = 40f;
            OrbitCamera orbit = cameraObject.AddComponent<OrbitCamera>();
            orbit.SetTarget(root.transform);
            orbit.SetOrbit(12f, -35f, 38f);
            cameraObject.transform.position = new Vector3(7.5f, 8.7f, -9.5f);
            cameraObject.transform.LookAt(new Vector3(0f, 3.8f, 0f));

            GameObject key = new GameObject("Key Light");
            Light keyLight = key.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.intensity = 1.55f;
            keyLight.color = new Color(1f, 0.95f, 0.85f);
            key.transform.rotation = Quaternion.Euler(48f, -35f, 0f);

            GameObject fill = new GameObject("Fill Light");
            Light fillLight = fill.AddComponent<Light>();
            fillLight.type = LightType.Point;
            fillLight.intensity = 1.2f;
            fillLight.range = 10f;
            fillLight.color = new Color(0.72f, 0.82f, 1f);
            fill.transform.position = new Vector3(-3f, 5f, -4f);

            GameObject rim = new GameObject("Rim Light");
            Light rimLight = rim.AddComponent<Light>();
            rimLight.type = LightType.Directional;
            rimLight.intensity = 0.65f;
            rimLight.color = new Color(0.50f, 0.68f, 0.95f);
            rim.transform.rotation = Quaternion.Euler(22f, 148f, 0f);

            GameObject grind = new GameObject("Grinding Zone Light");
            Light grindLight = grind.AddComponent<Light>();
            grindLight.type = LightType.Point;
            grindLight.intensity = 1.1f;
            grindLight.range = 5f;
            grindLight.color = new Color(1f, 0.72f, 0.28f);
            grind.transform.position = new Vector3(0f, 3.8f, 0f);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.48f, 0.54f, 0.62f);
            RenderSettings.ambientEquatorColor = new Color(0.22f, 0.25f, 0.28f);
            RenderSettings.ambientGroundColor = new Color(0.08f, 0.09f, 0.10f);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            Selection.activeGameObject = root;
        }

        private static void CreateDemoSceneIfMissing()
        {
            if (Application.isPlaying)
                return;

            // 씬이 이미 있으면 재생성하지 않음
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
                return;

            CreateDemoScene();
        }

        [MenuItem("Tools/Coal Pulverizer/Rebuild Current Model")]
        public static void RebuildOpenPulverizerIfNeeded()
        {
            CreateDemoScene();
        }
    }
}
#endif
