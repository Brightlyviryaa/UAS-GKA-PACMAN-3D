namespace BT_PacMan
{
    public class BehaviorTree
    {
        private Node root;

        public BehaviorTree(Node root)
        {
            this.root = root;
        }

        public void Tick()
        {
            if (root != null)
                root.Tick();
        }
    }
}
