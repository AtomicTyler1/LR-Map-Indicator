using System;
using System.IO;
using System.Reflection;
using BepInEx;
using UnityEngine;
using HarmonyLib;
using BepInEx.Logging;
using GameNetcodeStuff;

namespace LeftAndRightPlayerTerminal
{
    [BepInPlugin("com.atomic.lrmap", "LR Map Indicator", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private const string FbxAssetName = "assets/lethalcompany/NewDirectionIndicator_2.fbx";
        private const string TargetMeshNameInFbx = "NewDirectionIndicator_2(Clone)";
        private const string AssetBundleFileName = "newdirectionindicator";
        public static AssetBundle? _customAssetBundle;
        public static Mesh? _newDirectionMesh;
        public static ManualLogSource PluginLogger;

        // For any modders reading this, I had trouble with making a good assetbundle so I had to use some scuffed code. It shouldn't cost much performance but if you want you can fix it lol.

        private void Awake()
        {
            PluginLogger = Logger;
            PluginLogger.LogInfo("LR Terminal Indicator Plugin Loaded!");

            string pluginLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string assetBundlePath = Path.Combine(pluginLocation, AssetBundleFileName);

            if (!File.Exists(assetBundlePath))
            {
                PluginLogger.LogError($"Asset bundle not found at: {assetBundlePath}. Please ensure '{AssetBundleFileName}' is in the same folder as your plugin DLL.");
                return;
            }

            try
            {
                _customAssetBundle = AssetBundle.LoadFromFile(assetBundlePath);
                if (_customAssetBundle == null)
                {
                    PluginLogger.LogError($"FATAL: Failed to load asset bundle from {assetBundlePath}. Contact @atomictyler!");
                    return;
                }
                PluginLogger.LogInfo($"Successfully loaded asset bundle: {AssetBundleFileName}");

                string[] assetNames = _customAssetBundle.GetAllAssetNames();
                PluginLogger.LogInfo($"Assets found in bundle '{AssetBundleFileName}':");
                foreach (string assetName in assetNames)
                {
                    PluginLogger.LogInfo($"- {assetName}");
                }

                GameObject fbxPrefab = _customAssetBundle.LoadAsset<GameObject>(FbxAssetName);
                if (fbxPrefab == null)
                {
                    PluginLogger.LogError($"FATAL: FBX asset '{FbxAssetName}' not found in asset bundle '{AssetBundleFileName}'. " +
                                            "Ensure the FBX asset name is correct and it was included in the bundle.");
                    _customAssetBundle.Unload(true);
                    return;
                }
                PluginLogger.LogInfo($"Successfully loaded FBX prefab: {FbxAssetName} from bundle.");

                GameObject tempFbxInstance = UnityEngine.Object.Instantiate(fbxPrefab);
                tempFbxInstance.SetActive(false);
                UnityEngine.Object.DontDestroyOnLoad(tempFbxInstance);

                MeshFilter[] meshFilters = tempFbxInstance.GetComponentsInChildren<MeshFilter>(true);
                foreach (MeshFilter mf in meshFilters)
                {
                    if (mf.mesh != null && mf.mesh.name == TargetMeshNameInFbx)
                    {
                        _newDirectionMesh = mf.mesh;
                        PluginLogger.LogInfo($"Found target mesh '{TargetMeshNameInFbx}' within FBX prefab '{FbxAssetName}'.");
                        break;
                    }
                    else if (mf.mesh != null)
                    {
                        PluginLogger.LogInfo($"Found mesh: {mf.mesh.name} on GameObject: {mf.gameObject.name} (in FBX prefab).");
                    }
                }

                UnityEngine.Object.Destroy(tempFbxInstance);
                PluginLogger.LogInfo("Temporary FBX instance destroyed.");

                if (_newDirectionMesh == null)
                {
                    PluginLogger.LogError($"FATAL: Mesh '{TargetMeshNameInFbx}' not found within the hierarchy of FBX asset '{FbxAssetName}'. " +
                                            "Please verify the mesh name and its location inside the FBX model in Unity.");
                    _customAssetBundle.Unload(true);
                    return;
                }
                PluginLogger.LogInfo($"Successfully identified target mesh: {_newDirectionMesh.name} for replacement.");
            }
            catch (Exception ex)
            {
                PluginLogger.LogError($"FATAL: Error loading asset bundle or extracting mesh: {ex.Message}. Contact @atomictyler!");
                return;
            }

            var harmony = new Harmony("com.atomic.lrmap");
            try
            {
                harmony.PatchAll(typeof(PlayerControllerBPatch));
                PluginLogger.LogInfo("Harmony patch applied to PlayerControllerB.");
            }
            catch (Exception ex)
            {
                PluginLogger.LogError($"Error applying Harmony patch: {ex.Message}");
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB))]
        internal class PlayerControllerBPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Update")]
            static void Postfix(PlayerControllerB __instance)
            {
                if (Plugin._newDirectionMesh == null)
                {
                    return;
                }

                Transform? mapDirectionIndicatorTransform = __instance.gameObject.transform
                    .Find("Misc")?
                    .Find("MapDot")?
                    .Find("MapDirectionIndicator");

                if (mapDirectionIndicatorTransform != null)
                {
                    GameObject mapDirectionIndicator = mapDirectionIndicatorTransform.gameObject;
                    MeshFilter targetMeshFilter = mapDirectionIndicator.GetComponent<MeshFilter>();

                    if (targetMeshFilter != null)
                    {
                        if (targetMeshFilter.mesh != Plugin._newDirectionMesh)
                        {
                            targetMeshFilter.mesh = Plugin._newDirectionMesh;
                            PluginLogger.LogInfo($"Successfully replaced mesh of '{mapDirectionIndicator.name}' with '{Plugin._newDirectionMesh.name}'.");
                        }
                    }
                    else
                    {
                        PluginLogger.LogWarning($"MapDirectionIndicator GameObject does not have a MeshFilter component. Cannot replace mesh.");
                    }
                }
            }
        }
    }
}
