using System;

namespace BT_PacMan
{
    public class ConditionNode : Node
    {
        private Func<bool> condition;

        public ConditionNode(Func<bool> condition)
        {
            this.condition = condition;
        }

        public override NodeState Tick()
        {
            return condition() ? NodeState.Success : NodeState.Failure;
        }
    }
}
