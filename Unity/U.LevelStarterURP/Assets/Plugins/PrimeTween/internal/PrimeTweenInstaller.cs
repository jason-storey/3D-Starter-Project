using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.GUILayout;

namespace PrimeTween {
    internal class PrimeTweenInstaller : ScriptableObject {
        [SerializeField] internal SceneAsset demoScene;
        [SerializeField] internal SceneAsset demoSceneUrp;
        [SerializeField] internal Color uninstallButtonColor;
    }
    
    [CustomEditor(typeof(PrimeTweenInstaller), false)]
    internal class InstallerInspector : Editor {
        const string pluginName = "PrimeTween";
        const string pluginPackageId = "com.kyrylokuzyk.primetween";
        const string tgzPath = "Assets/Plugins/" + pluginName + "/internal/" + pluginPackageId + ".tgz";
        const string documentationUrl = "https://github.com/KyryloKuzyk/PrimeTween";
        bool isInstalled;
        GUIStyle boldButtonStyle;
        GUIStyle uninstallButtonStyle;
        
        void OnEnable() {
            isInstalled = CheckPluginInstalled();
        }

        static bool CheckPluginInstalled() {
            var listRequest = Client.List(true);
            while (!listRequest.IsCompleted) {
            }
            return listRequest.Result.Any(_ => _.name == pluginPackageId);
        }

        public override void OnInspectorGUI() {
            if (boldButtonStyle == null) {
                boldButtonStyle = new GUIStyle(GUI.skin.button) {fontStyle = FontStyle.Bold};
            }
            var installer = (PrimeTweenInstaller) target;
            if (uninstallButtonStyle == null) {
                uninstallButtonStyle = new GUIStyle(GUI.skin.button) {normal = {textColor = installer.uninstallButtonColor}};
            }
            Space(8);
            Label(pluginName, EditorStyles.boldLabel);
            if (!isInstalled) {
                Space(8);
                if (Button("Install " + pluginName)) {
                    installPlugin();
                }
                return;
            }

            Space(8);
            if (Button("Documentation", boldButtonStyle)) {
                Application.OpenURL(documentationUrl);
            }
            
            Space(8);
            if (Button("Open Demo", boldButtonStyle)) {
                var rpAsset = GraphicsSettings.renderPipelineAsset;
                bool isUrp = rpAsset != null && rpAsset.GetType().Name.Contains("Universal");
                var demoScene = isUrp ? installer.demoSceneUrp : installer.demoScene;
                if (demoScene == null) {
                    Debug.LogError("Please re-import the plugin from Asset Store and import the 'Demo' folder.\n");
                    return;
                }
                var path = AssetDatabase.GetAssetPath(demoScene);
                EditorSceneManager.OpenScene(path);
            }
            #if UNITY_2019_4_OR_NEWER
            if (Button("Import Basic Examples")) {
                EditorUtility.DisplayDialog(pluginName, $"Please select the '{pluginName}' package in 'Package Manager', then press the 'Samples/Import' button at the bottom of the plugin's description.", "Ok");
                UnityEditor.PackageManager.UI.Window.Open(pluginPackageId);
            }
            #endif
            if (Button("Support")) {
                Application.OpenURL("https://github.com/KyryloKuzyk/PrimeTween#support");
            }

            Space(8);
            if (Button("Uninstall", uninstallButtonStyle)) {
                Client.Remove(pluginPackageId);
                isInstalled = false;
                var msg = $"Please remove the folder manually to uninstall {pluginName} completely: 'Assets/Plugins/{pluginName}'";
                EditorUtility.DisplayDialog(pluginName,msg,"Ok");
                Debug.Log(msg);
            }
        }

        static void installPlugin() {
            var path = $"file:../{tgzPath}";
            var addRequest = Client.Add(path);
            while (!addRequest.IsCompleted) {
            }
            if (addRequest.Status == StatusCode.Success) {
                Debug.Log($"{pluginName} installed successfully.\n" +
                          $"Offline documentation is located at Packages/{pluginName}/Documentation.md.\n" +
                          $"Online documentation: {documentationUrl}\n");
            } else {
                Debug.LogError($"Please re-import the plugin from the Asset Store and check that the file exists: [{path}].\n\n{addRequest.Error?.message}\n");
            }
        }
        
        #if !PRIME_TWEEN_INSTALLED && UNITY_2019_1_OR_NEWER
        internal class AssetPostprocessor : UnityEditor.AssetPostprocessor {
            static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
                foreach (var path in importedAssets) {
                    if (path == tgzPath) {
                        Selection.activeObject = AssetDatabase.LoadAssetAtPath<PrimeTweenInstaller>("Assets/Plugins/PrimeTween/PrimeTweenInstaller.asset");
                        installPlugin();
                        return;
                    }
                }
            }
        }
        #endif
    }
}