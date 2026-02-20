using System;

[Serializable]
public class DiscordPresence
{
    public string details;
    public string state;
    public DiscordAssets assets;
    public DiscordTimestamps timestamps;
}