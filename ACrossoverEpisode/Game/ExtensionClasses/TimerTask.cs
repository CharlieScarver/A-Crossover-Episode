using Emotion.Engine;
using EmotionPlayground.Game.ExtensionClasses;

namespace ACrossoverEpisode.Game.ExtensionClasses
{
    public class TimerTask : Timer
    {
        public EmTask Task;

        public TimerTask(float goal) : base(goal)
        {
            Task = new EmTask();
        }

        public override void Stop()
        {
            base.Stop();
            Task.Run();
        }
    }
}