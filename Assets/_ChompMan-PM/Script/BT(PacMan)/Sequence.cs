using System.Collections.Generic;

namespace BT_PacMan
{
    public class Sequence : Node
    {
        private List<Node> children = new List<Node>();

        public void AddChild(Node child)
        {
            children.Add(child);
        }

        public override NodeState Tick()
        {
            foreach (var child in children)
            {
                var state = child.Tick();
                if (state != NodeState.Success)
                    return state;
            }
            return NodeState.Success;
        }
    }
}
