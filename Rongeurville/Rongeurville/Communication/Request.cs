using System;

namespace Rongeurville.Communication
{
    [Serializable]
    public abstract class Request : Message
    {
        public int Rank;
    }
}