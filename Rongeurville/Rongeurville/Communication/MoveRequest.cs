using System;

namespace Rongeurville.Communication
{
    [Serializable]
    public class MoveRequest : Request
    {
        public Coordinates DesiredTile;
    }
}