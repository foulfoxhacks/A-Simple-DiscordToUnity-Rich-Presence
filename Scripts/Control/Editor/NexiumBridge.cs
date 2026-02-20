using UnityEditor;
using Discord;
using UnityEngine;
using System;

[InitializeOnLoad]
public class NexiumBridge
{
    private static Discord.Discord discord;
    private static long startTime;
    private static int cycleIndex = 0;
    private static double lastCycleTime;

    // !!! PASTE YOUR APPLICATION ID HERE !!!
    private const long ClientID = 1473390464748228783; 

    static NexiumBridge()
    {
        startTime = DateTimeOffset.Now.ToUnixTimeSeconds();
        EditorApplication.update += UpdateLoop;
        lastCycleTime = EditorApplication.timeSinceStartup;
        Init();
    }

    static void Init()
    {
        try {
            discord = new Discord.Discord(ClientID, (ulong)CreateFlags.NoRequireDiscord);
        } catch { }
    }

    static void UpdateLoop()
    {
        if (discord == null) return;

        try {
            discord.RunCallbacks();

            // Cycle the status text every 7 seconds
            if (EditorApplication.timeSinceStartup - lastCycleTime > 7.0f)
            {
                lastCycleTime = EditorApplication.timeSinceStartup;
                cycleIndex++;
                SyncVCCPresence();
            }

            // Immediate update if we start compiling or importing
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                if (Time.frameCount % 500 == 0) SyncVCCPresence();
            }
        } catch {
            discord = null;
        }
    }

    static void SyncVCCPresence()
    {
        var vcc = VCCManifestReader.GetStats();
        var am = discord.GetActivityManager();

        // 1. DETERMINE THE "DETAILS" (Top Line)
        string detailsText = $"Creating: {vcc.ProjectName}";

        if (EditorApplication.isCompiling) {
            detailsText = "🔨 Compiling Scripts...";
        } 
        else if (EditorApplication.isUpdating) {
            detailsText = "📦 Importing Assets...";
        }
        else if (Selection.activeObject != null) {
            detailsText = $"Editing: {Selection.activeObject.name}";
        }

        // 2. DETERMINE THE "STATE" (Bottom Line - Cycling Stats)
        string stateText = "";
        string[] cycleStats = new string[] {
            $"{vcc.SDKType}",
            $"{vcc.PackageCount} VPM Packages",
            $"Unity {Application.unityVersion}",
            "Status: Active"
        };

        // If we are compiling/updating, lock the state text
        if (EditorApplication.isCompiling || EditorApplication.isUpdating) {
            stateText = "Processing changes...";
        } else {
            stateText = cycleStats[cycleIndex % cycleStats.Length];
        }

        // 3. CONSTRUCT THE ACTIVITY
        var activity = new Activity
        {
            Details = detailsText,
            State = stateText,
            Assets = {
                LargeImage = "vcc_logo",
                LargeText = $"VRChat Project: {vcc.ProjectName}",
                SmallImage = "unity_icon",
                SmallText = $"VCC v1.0 (Nexium RPC)"
            },
            Timestamps = {
                Start = startTime
            }
        };

        am.UpdateActivity(activity, (res) => { });
    }
}
