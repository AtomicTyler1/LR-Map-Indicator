using System;
using System.IO;
using System.Reflection;
using BepInEx;
using UnityEngine;
using HarmonyLib;
using BepInEx.Logging;
using GameNetcodeStuff;
using System.Collections.Generic;
using LeftAndRightPlayerTerminal.Patches;
using System.Linq;
using BepInEx.Configuration;

namespace LeftAndRightPlayerTerminal
{
    [BepInPlugin("com.atomic.lrmap", "LR Map Indicator", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("com.atomic.lrmap");
        private static Plugin? instance;

        internal static ManualLogSource Logger;

        internal static List<Mesh>? Meshes;
        internal static List<Material>? Materials;
        internal static AssetBundle? bundle;

        internal static ConfigEntry<bool>? FixedRotationIndicators;

        internal static ConfigEntry<float>? LeftIndicatorColorR;
        internal static ConfigEntry<float>? LeftIndicatorColorG;
        internal static ConfigEntry<float>? LeftIndicatorColorB;
        internal static ConfigEntry<float>? RightIndicatorColorR;
        internal static ConfigEntry<float>? RightIndicatorColorG;
        internal static ConfigEntry<float>? RightIndicatorColorB;

        void Awake()
        {
            if (instance == null) { instance = this; }
            Logger = BepInEx.Logging.Logger.CreateLogSource("LRMapIndicator");
            Logger.LogInfo("I am alive!");

            FixedRotationIndicators = Config.Bind(
                "General",
                "Fixed Rotation Indicators",
                true,
                "The L and R symbols do not rotate with the player's view." 
            );

            LeftIndicatorColorR = Config.Bind(
                "Left Indicator Color",
                "Red",
                0f,
                "Red component (0-255) for the L indicator."
            );
            LeftIndicatorColorG = Config.Bind(
                "Left Indicator Color",
                "Green",
                0f,
                "Green component (0-255) for the L indicator."
            );
            LeftIndicatorColorB = Config.Bind(
                "Left Indicator Color",
                "Blue",
                255f,
                "Blue component (0-255) for the L indicator."
            );

            RightIndicatorColorR = Config.Bind(
                "Right Indicator Color",
                "Red",
                255f,
                "Red component (0-255) for the R indicator."
            );
            RightIndicatorColorG = Config.Bind(
                "Right Indicator Color",
                "Green",
                0f,
                "Green component (0-255) for the R indicator."
            );
            RightIndicatorColorB = Config.Bind(
                "Right Indicator Color",
                "Blue",
                0f,
                "Blue component (0-255) for the R indicator."
            );


            harmony.PatchAll(typeof(PlayerControllerBPatch));

            Meshes = new List<Mesh>();

            string location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            bundle = AssetBundle.LoadFromFile(Path.Combine(location, "radardotleftrightindicator"));

            if (bundle == null) { Logger.LogError("Failed to load asset bundle! Abort!!!"); return; }

            Meshes = bundle.LoadAllAssets<Mesh>().ToList();
            Materials = bundle.LoadAllAssets<Material>().ToList();
        }
    }
}