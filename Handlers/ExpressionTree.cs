namespace PowerBI_MCP.Handlers
{
    public partial class ExpressionTree
    {
        public string data;
        public List<ExpressionTree> children;
        public ExpressionTree? parent;
        public int count;
        public List<string> dependencyList;
        bool isSeparateExpression = false;
        public int nodePosition;
        public int nodeCount;

        public ExpressionTree(string _data, int _nodePosition, Dictionary<string, List<string>> measureDetails)
        {
            this.data = _data;
            this.nodePosition = _nodePosition;
            this.nodeCount = -1000;
            children = new List<ExpressionTree>();
            parent = null;
            dependencyList = ComplianceHandler.GetUsageCount(_data, measureDetails);
            count = dependencyList.Count;
            isSeparateExpression = false;
        }

        public ExpressionTree(string data, int _nodePosition, ExpressionTree parent, Dictionary<string, List<string>> measureDetails)
        {
            this.data = data;
            this.nodePosition = _nodePosition;
            this.nodeCount = -1000;
            children = new List<ExpressionTree>();
            this.parent = parent;
            dependencyList = ComplianceHandler.GetUsageCount(data, measureDetails);
            count = dependencyList.Count; //call the measureUsage function
            isSeparateExpression = false;
        }
        public static void GetSeparateExpressions(ExpressionTree root, Dictionary<string, List<string>> separateExpressions)
        {
            HashSet<string> operators = new() { "=", "==", "<", ">", ">=", "<=", "<>", "&&", "||" };
            bool expressionExists = false;
            if (root == null)
            {
                Console.WriteLine("Empty Tree");
                return; // Handle empty tree
            }
            Queue<ExpressionTree> queue = new();
            queue.Enqueue(root);
            //  Console.WriteLine("--------------------testTraversal---------------------");
            while (queue.Count > 0)
            {
                //Console.WriteLine("-------------------------");
                int levelSize = queue.Count;
                for (int i = 0; i < levelSize; i++)
                {
                    try
                    {
                        ExpressionTree currentNode = queue.Dequeue();
                        // Console.WriteLine(currentNode.data + "   " + currentNode.count);
                        //   Console.WriteLine(JsonConvert.SerializeObject(currentNode.data, Formatting.Indented));

                        if (currentNode.count > 1)
                        {
                            int parentCount = getParentCount(currentNode);
                            if ((currentNode.count - parentCount) > 1)
                            {
                                if (
                                    (currentNode.children.Count != 1 || (currentNode.data.Contains(",") && !(currentNode.data.Trim().StartsWith("(") && currentNode.data.Trim().EndsWith(")"))))
                                    && !(operators.Any(currentNode.data.Contains))
                                )
                                {
                                    foreach (string exp in separateExpressions.Keys)
                                    {
                                        try
                                        {
                                            if (ComplianceHandler.ToLowerSpaceRemoved(currentNode.data) == ComplianceHandler.ToLowerSpaceRemoved(exp))
                                            {
                                                expressionExists = true;
                                                break;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            GlobalHandler.WriteCrashLog(ex.ToString());
                                        }
                                    }
                                    if (!expressionExists)
                                    {
                                        //Console.WriteLine($"{currentNode.data}");
                                        currentNode.isSeparateExpression = true;
                                        separateExpressions.Add(currentNode.data, currentNode.dependencyList);
                                    }
                                }
                            }
                        }
                        foreach (ExpressionTree child in currentNode.children)
                        {
                            try
                            {
                                queue.Enqueue(child);
                            }
                            catch (Exception ex)
                            {
                                GlobalHandler.WriteCrashLog(ex.ToString());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        GlobalHandler.WriteCrashLog(ex.ToString());
                    }
                }
            }
        }

        public static int getParentCount(ExpressionTree root)
        {
            if (root.parent == null)
            {
                return 0;
            }
            else if (root.parent.isSeparateExpression)
            {
                return root.parent.count;
            }
            else
            {
                return getParentCount(root.parent);
            }
        }
        public static ExpressionTree Insert(ExpressionTree root, ExpressionTree parent, ExpressionTree node)
        {
            parent.children.Add(node);
            return node;
        }
    }
}