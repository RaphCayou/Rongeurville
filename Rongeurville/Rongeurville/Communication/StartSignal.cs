using System;

namespace Rongeurville.Communication
{
    [Serializable]
    public class StartSignal : Signal
    {
        public Map Map;
        public Coordinates Position;
    }
}