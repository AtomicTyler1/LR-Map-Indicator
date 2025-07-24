using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace LeftAndRightPlayerTerminal.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        private static Transform? mapDot;
        private static Transform? mDI;
        private static Transform? mDIR;
        private static Transform? mDIL;
        private static GameObject? cloneR;
        private static GameObject? cloneL;

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void OverrideMapRadarDirectionIndicator(PlayerControllerB __instance)
        {
            if (__instance.gameObject.transform.Find("Misc").Find("MapDot").Find("MapDirectionIndicatorRight") == null)
            {
                mapDot = __instance.gameObject.transform.Find("Misc").Find("MapDot");
                mDI = mapDot.Find("MapDirectionIndicator");

                cloneR = GameObject.Instantiate(mDI.gameObject);
                cloneL = GameObject.Instantiate(mDI.gameObject);
                cloneR.name = "MapDirectionIndicatorRight";
                cloneL.name = "MapDirectionIndicatorLeft";
                cloneR.transform.SetParent(mapDot);
                cloneL.transform.SetParent(mapDot);

                if (Plugin.Meshes == null || Plugin.Materials == null)
                {
                    Plugin.Logger.LogError("Meshes or Materials are not initialized!");
                    return;
                }

                cloneR.GetComponent<MeshFilter>().sharedMesh = Plugin.Meshes[1];
                cloneL.GetComponent<MeshFilter>().sharedMesh = Plugin.Meshes[0];

                Material rightMaterial = new Material(Plugin.Materials[1]);
                rightMaterial.color = new Color(
                    Plugin.RightIndicatorColorR.Value / 255f,
                    Plugin.RightIndicatorColorG.Value / 255f,
                    Plugin.RightIndicatorColorB.Value / 255f
                );
                cloneR.GetComponent<MeshRenderer>().material = rightMaterial;

                Material leftMaterial = new Material(Plugin.Materials[0]);
                leftMaterial.color = new Color(
                    Plugin.LeftIndicatorColorR.Value / 255f,
                    Plugin.LeftIndicatorColorG.Value / 255f,
                    Plugin.LeftIndicatorColorB.Value / 255f
                );
                cloneL.GetComponent<MeshRenderer>().material = leftMaterial;

                cloneR.transform.localRotation = Quaternion.Euler(0, -180, 0);
                cloneL.transform.localRotation = Quaternion.Euler(0, 0, 0);
                cloneR.transform.localScale = new Vector3(0.4f, 0.4191016f, 0.7f);
                cloneL.transform.localScale = new Vector3(0.4f, 0.4191016f, 0.7f);
                cloneR.transform.localPosition = new Vector3(2.05f, 1.42f, 0);
                cloneL.transform.localPosition = new Vector3(-2.14f, 1.42f, 0.37f);
            }

            mDIR = __instance.gameObject.transform.Find("Misc").Find("MapDot").Find("MapDirectionIndicatorRight");
            mDIL = __instance.gameObject.transform.Find("Misc").Find("MapDot").Find("MapDirectionIndicatorLeft");

            if (Plugin.FixedRotationIndicators.Value)
            {
                if (mDIR.rotation.eulerAngles.y != 45)
                {
                    mDIR.Rotate(Vector3.up, -mDIR.rotation.eulerAngles.y + 45, Space.Self);
                }
                if (mDIL.rotation.eulerAngles.y != 45)
                {
                    mDIL.Rotate(Vector3.up, -mDIL.rotation.eulerAngles.y + 45, Space.Self);
                }
            }

            mDIR.GetComponent<MeshRenderer>().material.color = new Color(
                Plugin.RightIndicatorColorR.Value / 255f,
                Plugin.RightIndicatorColorG.Value / 255f,
                Plugin.RightIndicatorColorB.Value / 255f
            );
            mDIL.GetComponent<MeshRenderer>().material.color = new Color(
                Plugin.LeftIndicatorColorR.Value / 255f,
                Plugin.LeftIndicatorColorG.Value / 255f,
                Plugin.LeftIndicatorColorB.Value / 255f
            );
        }
    }
}