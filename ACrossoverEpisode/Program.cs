#region Using

using System;
using ACrossoverEpisode.Game;
using Emotion.Engine;
using Emotion.IO;

#endregion

namespace EmotionPlayground
{
    public class MainLayer
    {
        public const string DebugFont = "debugFont.otf";
        public const string PixelatedFont = "Fonts/pixelated_princess/pixelated_princess.ttf";

        private static void Main()
        {
            // Configuration.
            Context.Setup(config =>
            {
                config.HostSettings.Title = "A Crossover Episode";
                config.SoundSettings.Volume = 75;
                config.ScriptingSettings.Timeout = TimeSpan.FromMinutes(1);
            });
            Context.Flags.RenderFlags.CircleDetail = 90;

            // Load test map.
            Context.SceneManager.SetScene(new GameScene(Context.AssetLoader.Get<TextFile>("Maps/testmap.json")));
            Context.Run();
        }
    }
}