using System;
using System.Collections.Generic;

namespace Rongeurville.Communication
{
    [Serializable]
    public class KillSignal : Signal
    {
        public List<int> RanksTargeted = new List<int>();
        public bool KillAll = false;
    }
}