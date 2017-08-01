using System;

namespace Rongeurville.Communication
{
    [Serializable]
    public class MoveSignal : Signal
    {
        public Coordinates InitialTile;
        public Coordinates FinalTile;
    }
}