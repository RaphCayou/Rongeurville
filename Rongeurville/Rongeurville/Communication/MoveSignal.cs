using System;

namespace Rongeurville.Communication
{
    [Serializable]
    public class MoveSignal : Signal
    {
        public Tile InitialTile;
        public Tile FinalTile;
    }
}