using Antlr4.Runtime.Tree;
using Antlr4.Runtime;

public class TreeNode
{
    public string name { get; set; }
    public List<TreeNode> children { get; set; }
}

public class ParseTreeJsonConverter
{
    public TreeNode ConvertToJson(IParseTree tree, Parser parser)
    {
        if (tree == null)
            return null;

        TreeNode node = new TreeNode();

        if (tree is TerminalNodeImpl terminalNode)
        {
            // It's a leaf node (token)
            string tokenText = terminalNode.GetText();
            node.name = tokenText.Replace("\"", "\\\""); // Escape quotes
        }
        else
        {
            // It's a rule node
            var ruleNode = tree as ParserRuleContext;
            string ruleName = parser.RuleNames[ruleNode.RuleIndex];
            node.name = ruleName;

            int childCount = tree.ChildCount;
            if (childCount > 0)
            {
                node.children = new List<TreeNode>();
                for (int i = 0; i < childCount; i++)
                {
                    var child = tree.GetChild(i);
                    var childNode = ConvertToJson(child, parser);
                    node.children.Add(childNode);
                }
            }
        }

        return node;
    }
}