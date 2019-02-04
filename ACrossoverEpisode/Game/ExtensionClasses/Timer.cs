namespace EmotionPlayground.Game.ExtensionClasses
{
    public class Timer
    {
        public float Goal;              // in miliseconds

        public Timer(float goal)
        {
            this.PassedTime = 0;
            this.Ready = true;
            this.Goal = goal;
        }

        public float PassedTime         // in miliseconds
        {
            get;
            private set;
        }

        public bool Ready
        {
            get;
            private set;
        }

        // Starts or restarts the timer
        public virtual void Start()
        {
            this.PassedTime = 0;
            this.Ready = false;
        }

        public virtual void Update(float deltaTime)
        {
            if (!this.Ready)
            {
                this.PassedTime += deltaTime;

                if (this.PassedTime >= this.Goal)
                {
                    this.Stop();
                }
            }
        }

        public virtual void Stop()
        {
            this.Ready = true;
        }
    }
}
