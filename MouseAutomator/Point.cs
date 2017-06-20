namespace MouseAutomator
{
    internal struct Point
    {
        private int x;
        public int X
        {
            get
            {
                return x;
            }
            set
            {
                x = value;
            }
        }

        private int y;
        public int Y
        {
            get
            {
                return y;
            }
            set
            {
                y = value;
            }
        }
        private int delay;
        /// <summary>
        /// Gets or sets the delay after this click and before the next one. 
        /// </summary>
        public int Delay
        {
            get
            {
                return delay;
            }
            set
            {
                delay = value;
            }
        }

        public Point(int x, int y, int delay)
        {
            this.x = x;
            this.y = y;
            this.delay = delay;
        }
    }
}
