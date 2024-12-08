using UnityEngine;

namespace BT_PacMan
{
    public abstract class Node
    {
        public enum NodeState { Running, Success, Failure }

        public abstract NodeState Tick();
    }
}
