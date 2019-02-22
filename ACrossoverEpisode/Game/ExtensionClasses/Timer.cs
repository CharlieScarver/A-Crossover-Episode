namespace EmotionPlayground.Game.ExtensionClasses
{
    public class Timer
    {
        public float Goal; // in miliseconds

        public Timer(float goal)
        {
            PassedTime = 0;
            Ready = true;
            Goal = goal;
        }

        public float PassedTime // in miliseconds
        {
            get;
            private set;
        }

        public bool Ready { get; private set; }

        // Starts or restarts the timer
        public virtual void Start()
        {
            PassedTime = 0;
            Ready = false;
        }

        public virtual void Update(float deltaTime)
        {
            if (!Ready)
            {
                PassedTime += deltaTime;

                if (PassedTime >= Goal) Stop();
            }
        }

        public virtual void Stop()
        {
            Ready = true;
        }
    }
}