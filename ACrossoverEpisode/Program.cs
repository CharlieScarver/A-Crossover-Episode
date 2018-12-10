namespace EmotionPlayground
{
    using Emotion.Engine;
    using Emotion.Engine.Hosting.Desktop;
    using Emotion.Game.Animation;
    using Emotion.Game.Layering;
    using Emotion.Graphics;
    using Emotion.Graphics.Text;
    using System.Numerics;
    using EmotionPlayground.GameObjects;
    using System.Collections.Generic;
    using Emotion.Primitives;

    public class MainLayer : Layer
    {
        public static List<GameObject> GameObjects = new List<GameObject>();
        public static List<Unit> Units = new List<Unit>();

        public readonly float CameraOffsetX = -275;
        public readonly float Velocity = 5;

        public Horseman player;

        AnimatedTexture starAnimation;

        static void Main()
        {            
            Context.Setup(config =>
            {
                config.WindowTitle = "A Crossover Episode";
                config.WindowMode = WindowMode.Windowed;
                config.Volume = 75;
            });

            Renderer.CircleDetail = 90;

            Context.LayerManager.Add(new MainLayer(), "Main Layer", 1);

            Context.Run();
        }

        public override void Load()
        {
            Context.AssetLoader.Get<Texture>("background.png");

            starAnimation = new AnimatedTexture(
                Context.AssetLoader.Get<Texture>("star-spritesheet.png"),
                new Vector2(48, 48),
                AnimationLoopType.Normal,
                500,
                0,
                1
            );

            Context.AssetLoader.Get<Font>("debugFont.otf");

            // Context.SoundManager.Play(Context.AssetLoader.Get<SoundFile>("tuguduk.wav"), "Main Layer").Looping = true;

            this.player = new Horseman(new Vector3(275, 400, 0), new Vector2(96, 96));
            Bouncer bouncer1 = new Bouncer(new Vector3(850, 340, 0), new Vector2(96, 96));
            Bouncer bouncer2 = new Bouncer(new Vector3(850, 455, 0), new Vector2(96, 96));

            Bouncer bouncer3 = new Bouncer(new Vector3(1450, 360, 0), new Vector2(96, 96));
            Bouncer bouncer4 = new Bouncer(new Vector3(2150, 410, 0), new Vector2(96, 96));

            GameObjects.Add(bouncer1);
            GameObjects.Add(bouncer3);
            GameObjects.Add(player);
            GameObjects.Add(bouncer2);
            GameObjects.Add(bouncer4);

            Units.Add(bouncer1);
            Units.Add(bouncer3);
            Units.Add(player);
            Units.Add(bouncer2);
            Units.Add(bouncer4);
        }

        public override void Unload()
        {

        }

        public override void Update(float frameTime)
        {
            starAnimation.Update(frameTime);
            foreach (Unit u in Units)
            {
                u.Update(frameTime);
            }

            // The camera will follow the player
            Context.Renderer.Camera.X = this.player.X - this.CameraOffsetX;

            if (player.Position.Y < -700)
            {
                Context.Quit();
            }
        }

        public override void Draw(Renderer renderer)
        {
            // Render background
            renderer.Render(new Vector3(-500, 0, 0), new Vector2(5000, 1000), Color.CornflowerBlue);
            renderer.Render(Context.Renderer.Camera.Position, Context.Host.Size, Color.White, Context.AssetLoader.Get<Texture>("background.png"));

            renderer.RenderLine(new Vector3(-500, 430, 0), new Vector3(5000, 430, 0), Color.Lerp(Color.Red, Color.Black, 0.5f));
            renderer.RenderLine(new Vector3(-500, 440 + 96, 0), new Vector3(5000, 440 + 96, 0), Color.Lerp(Color.Red, Color.Black, 0.5f));

            renderer.RenderString(Context.AssetLoader.Get<Font>("debugFont.otf"), 15, "This game is like life: you can only go forward.", new Vector3(200, 100, 0), Color.Black);
            renderer.RenderString(Context.AssetLoader.Get<Font>("debugFont.otf"), 15, "Your mind sees what your eyes cannot.", new Vector3(4700, 150, 0), Color.White);

            // LINQ Select can be JIT-ed better
            foreach (Unit u in Units)
            {
                u.Draw(renderer);
            }

            // Render background
            // new Vector3(275, 400, 0)

            renderer.Render(
                new Vector3(4950, 450, 0),
                new Vector2(48, 48),
                Color.White,
                starAnimation.Texture,
                starAnimation.CurrentFrame
            );
        }
    }
}
