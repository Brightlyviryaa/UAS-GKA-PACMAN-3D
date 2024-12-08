using System;

namespace BT_PacMan
{
    public class ActionNode : Node
    {
        private Func<NodeState> action;

        public ActionNode(Func<NodeState> action)
        {
            this.action = action;
        }

        public override NodeState Tick()
        {
            return action();
        }
    }
}
