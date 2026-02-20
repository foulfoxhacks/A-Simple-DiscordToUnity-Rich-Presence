using System.IO;
using UnityEngine;

public static class VCCManifestReader
{
    public struct VCCStats
    {
        public string ProjectName;
        public string SDKType;
        public int PackageCount;
    }

    public static VCCStats GetStats()
    {
        VCCStats stats = new VCCStats();
        stats.ProjectName = Application.productName;
        
        // VCC stores project info in vpm-manifest.json in the project root
        string path = Path.Combine(Application.dataPath, "..", "Packages", "vpm-manifest.json");
        
        if (File.Exists(path))
        {
            string content = File.ReadAllText(path);
            if (content.Contains("com.vrchat.avatars")) stats.SDKType = "Avatar Project";
            else if (content.Contains("com.vrchat.worlds")) stats.SDKType = "World Project";
            else stats.SDKType = "VRChat Project";

            // Count package entries
            int count = 0;
            string[] lines = File.ReadAllLines(path);
            foreach (var line in lines) if (line.Contains("\"version\":")) count++;
            stats.PackageCount = count;
        }
        else
        {
            stats.SDKType = "Unity Editor";
        }
        return stats;
    }
}
