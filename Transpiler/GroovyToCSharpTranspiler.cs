using System.Text;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TYTCapstone.Transpiler
{
public class GroovyToCSharpTranspiler : GroovyParserBaseVisitor<CSharpSyntaxNode>
{
    private readonly List<UsingDirectiveSyntax> Usings = new();
    private readonly List<MemberDeclarationSyntax> Members = new();

    /// <summary>
    /// Change how much logging information is seen when the autologger 
    /// intercepts a visitor and displays its information.
    /// </summary>
    private const LoggerVerbosity LoggingVerbosity = LoggerVerbosity.Minimal;

    public CompilationUnitSyntax Transpile(GroovyParser.CompilationUnitContext context)
    {
        return (CompilationUnitSyntax)Visit(context);
    }

    [AutoLog(LoggingVerbosity)]
    public override CSharpSyntaxNode VisitCompilationUnit([NotNull] GroovyParser.CompilationUnitContext context)
    {
        var usingDirectives = new List<UsingDirectiveSyntax>();
        var members = new List<MemberDeclarationSyntax>();

        foreach (IParseTree child in context.children)
        {
            // Visit each child and determine the type of node returned
            var visitedNode = Visit(child);

            if (visitedNode is UsingDirectiveSyntax usingDirective)
            {
                // Collect using directives separately
                usingDirectives.Add(usingDirective);
            }
            else if (visitedNode is MemberDeclarationSyntax member)
            {
                // Collect top-level members like classes, interfaces, enums
                members.Add(member);
            }
            else if (visitedNode is StatementSyntax statement)
            {
                // Wrap top-level statements in GlobalStatementSyntax
                var globalStatement = SyntaxFactory.GlobalStatement(statement);
                members.Add(globalStatement);
            }
        }

        // Create the compilation unit, adding both usings and members
        return SyntaxFactory.CompilationUnit()
            .AddUsings(usingDirectives.ToArray())
            .AddMembers(members.ToArray())
            .NormalizeWhitespace();
    }


    [AutoLog(LoggingVerbosity)]
    public override CSharpSyntaxNode VisitScriptPart([NotNull] GroovyParser.ScriptPartContext context)
    {
        if (context.statement() != null)
        {
            return Visit(context.statement());
        }
        else if (context.methodDeclaration() != null)
        {
            return Visit(context.methodDeclaration());
        }
        return null;
    }

    [AutoLog(LoggingVerbosity)]
    public override CSharpSyntaxNode VisitFieldDeclaration([NotNull] GroovyParser.FieldDeclarationContext context)
    {
        var declarations = context.singleDeclaration();
        if (declarations == null || declarations.Length == 0)
        {
            throw new InvalidOperationException("No declarations found in field declaration");
        }

        var firstDeclaration = declarations[0];
        var identifier = firstDeclaration.IDENTIFIER()?.GetText();
        var expression = firstDeclaration.expression();

        if (identifier == null)
        {
            throw new InvalidOperationException("No identifier found in declaration");
        }

        var expressionSyntax = (ExpressionSyntax)Visit(expression);
        if (expressionSyntax == null)
        {
            throw new InvalidOperationException($"Failed to parse expression: {expression?.GetText() ?? "null"}");
        }

        var variableDeclaration = VariableDeclaration(
            IdentifierName("var"))
            .WithVariables(
                SingletonSeparatedList(
                    VariableDeclarator(
                        Identifier(identifier))
                    .WithInitializer(
                        EqualsValueClause(expressionSyntax))));

        return LocalDeclarationStatement(variableDeclaration);
    }

    [AutoLog(LoggingVerbosity)]
    public override CSharpSyntaxNode VisitExpressionStatement([NotNull] GroovyParser.ExpressionStatementContext context)
    {
        var expressionNode = Visit(context.expression());
        
        if (expressionNode == null)
        {
            throw new InvalidOperationException($"Failed to parse expression: {context.expression().GetText()}");
        }

        if (expressionNode is not ExpressionSyntax expr)
        {
            throw new InvalidOperationException($"Expected ExpressionSyntax but got {expressionNode.GetType().Name}");
        }

        return ExpressionStatement(expr);
    }

    [AutoLog(LoggingVerbosity)]
    public override CSharpSyntaxNode VisitCmdExpression([NotNull] GroovyParser.CmdExpressionContext context)
    {
        var nonKwCalls = context.nonKwCallExpressionRule();
        if (nonKwCalls != null && nonKwCalls.Length > 0)
        {
            return Visit(nonKwCalls[0]);
        }
        
        return base.VisitCmdExpression(context);
    }

    [AutoLog(LoggingVerbosity)]
    public override CSharpSyntaxNode VisitNonKwCallExpressionRule([NotNull] GroovyParser.NonKwCallExpressionRuleContext context)
    {
        if (context.IDENTIFIER() != null && context.argumentList() != null)
        {
            var methodName = context.IDENTIFIER().GetText();
            var argumentList = context.argumentList();
            
            var argumentListNode = Visit(argumentList);
            
            if (argumentListNode is ArgumentListSyntax args)
            {
                if (methodName == "println")
                {
                    return InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("Console"),
                            IdentifierName("WriteLine")),
                        args);
                }
                
                return InvocationExpression(
                    IdentifierName(methodName),
                    args);
            }
            else
            {
                throw new InvalidOperationException($"Expected ArgumentListSyntax but got {argumentListNode?.GetType().Name ?? "null"}");
            }
        }
        
        return base.VisitNonKwCallExpressionRule(context);
    }

    [AutoLog(LoggingVerbosity)]
    public override CSharpSyntaxNode VisitArgumentList([NotNull] GroovyParser.ArgumentListContext context)
    {
        var arguments = new List<ArgumentSyntax>();
        
        foreach (var arg in context.argument())
        {
            var argNode = Visit(arg);
            if (argNode is ExpressionSyntax expr)
            {
                arguments.Add(Argument(expr));
            }
            else
            {
                throw new InvalidOperationException($"Expected ExpressionSyntax but got {argNode?.GetType().Name ?? "null"}");
            }
        }
        
        return ArgumentList(SeparatedList<ArgumentSyntax>(arguments));
    }

    [AutoLog(LoggingVerbosity)]
    public override CSharpSyntaxNode VisitArgument([NotNull] GroovyParser.ArgumentContext context)
    {
        return Visit(context.expression());
    }

    [AutoLog(LoggingVerbosity)]
    public override CSharpSyntaxNode VisitConstantExpression([NotNull] GroovyParser.ConstantExpressionContext context)
    {
        var text = context.STRING().GetText();
        // Remove the surrounding quotes (either single or double)
        text = text.Substring(1, text.Length - 2);
        return LiteralExpression(
            SyntaxKind.StringLiteralExpression,
            Literal(text));
    }

    public override CSharpSyntaxNode VisitConstantIntegerExpression([NotNull] GroovyParser.ConstantIntegerExpressionContext context)
    {
        var text = context.INTEGER().GetText();
        if (int.TryParse(text, out int value))
        {
            return LiteralExpression(
                SyntaxKind.NumericLiteralExpression,
                Literal(value));
        }
        throw new InvalidOperationException($"Failed to parse integer literal: {text}");
    }

    [AutoLog(LoggingVerbosity)]
    public override CSharpSyntaxNode VisitGstringExpression([NotNull] GroovyParser.GstringExpressionContext context)
    {
        var parts = new List<InterpolatedStringContentSyntax>();
        var gstring = context.gstring();
        
        // Handle the start text before the first interpolation
        var startText = gstring.GSTRING_START().GetText();
        startText = startText.Substring(2); // Remove the "$ part
        if (!string.IsNullOrEmpty(startText))
        {
            parts.Add(InterpolatedStringText(
                Token(TriviaList(), SyntaxKind.InterpolatedStringTextToken, startText, startText, TriviaList())
            ));
        }

        // Handle each expression body and the text parts that follow
        for (int i = 0; i < gstring.gstringExpressionBody().Length; i++)
        {
            var body = gstring.gstringExpressionBody()[i];
            var expr = Visit(body);
            if (expr is ExpressionSyntax exprSyntax)
            {
                parts.Add(Interpolation(exprSyntax));
            }

            // Add the text part that follows (if any)
            if (i < gstring.GSTRING_PART().Length)
            {
                var text = gstring.GSTRING_PART()[i].GetText();
                text = text.Substring(0, text.Length - 1); // Remove the $ part
                if (!string.IsNullOrEmpty(text))
                {
                    parts.Add(InterpolatedStringText(
                        Token(TriviaList(), SyntaxKind.InterpolatedStringTextToken, text, text, TriviaList())
                    ));
                }
            }
        }

        // Handle the end text
        var endText = gstring.GSTRING_END().GetText();
        endText = endText.Substring(0, endText.Length - 1); // Remove the closing quote
        if (!string.IsNullOrEmpty(endText))
        {
            parts.Add(InterpolatedStringText(
                Token(TriviaList(), SyntaxKind.InterpolatedStringTextToken, endText, endText, TriviaList())
            ));
        }

        return InterpolatedStringExpression(
            Token(SyntaxKind.InterpolatedStringStartToken),
            List(parts),
            Token(SyntaxKind.InterpolatedStringEndToken));
    }

    [AutoLog(LoggingVerbosity)]
    public override CSharpSyntaxNode VisitGstringExpressionBody([NotNull] GroovyParser.GstringExpressionBodyContext context)
    {
        if (context.expression() != null)
        {
            return Visit(context.expression());
        }
        
        return base.VisitGstringExpressionBody(context);
    }

    [AutoLog(LoggingVerbosity)]
    public override CSharpSyntaxNode VisitVariableExpression([NotNull] GroovyParser.VariableExpressionContext context)
    {
        return IdentifierName(context.IDENTIFIER().GetText());
    }

    public override CSharpSyntaxNode VisitBlockStatementWithCurve([NotNull] GroovyParser.BlockStatementWithCurveContext context)
    {
        var blockStatement = context.blockStatement();
        if (blockStatement == null)
        {
            return Block();
        }

        var statements = new List<StatementSyntax>();
        var visitedNode = Visit(blockStatement);
        
        if (visitedNode is StatementSyntax stmt)
        {
            // If it's already a block, return it directly
            if (stmt is BlockSyntax blockSyntax)
            {
                return blockSyntax;
            }
            statements.Add(stmt);
        }

        return Block(statements);
    }

    public override CSharpSyntaxNode VisitBlockStatement([NotNull] GroovyParser.BlockStatementContext context)
    {
        var statements = new List<StatementSyntax>();
        
        foreach (var stmt in context.statement())
        {
            var visitedStmt = Visit(stmt);
            if (visitedStmt is StatementSyntax statement)
            {
                statements.Add(statement);
            }
        }

        return Block(statements);
    }

    public override CSharpSyntaxNode VisitIfStatement([NotNull] GroovyParser.IfStatementContext context)
    {
        var condition = (ExpressionSyntax)Visit(context.expression());
        if (condition == null)
        {
            throw new InvalidOperationException("Failed to parse if condition");
        }

        // Get the main statement block
        var thenStatement = (StatementSyntax)Visit(context.statementBlock(0));
        if (thenStatement == null)
        {
            throw new InvalidOperationException("Failed to parse if statement block");
        }

        // Check if there's an else clause
        if (context.KW_ELSE() != null && context.statementBlock().Length > 1)
        {
            var elseStatement = (StatementSyntax)Visit(context.statementBlock(1));
            if (elseStatement == null)
            {
                throw new InvalidOperationException("Failed to parse else statement block");
            }

            return IfStatement(condition, 
                thenStatement is BlockSyntax ? thenStatement : Block(thenStatement))
                .WithElse(ElseClause(
                    elseStatement is BlockSyntax ? elseStatement : Block(elseStatement)));
        }

        return IfStatement(condition, 
            thenStatement is BlockSyntax ? thenStatement : Block(thenStatement));
    }

    public override CSharpSyntaxNode VisitStatementBlock([NotNull] GroovyParser.StatementBlockContext context)
    {
        if (context.blockStatementWithCurve() != null)
        {
            return Visit(context.blockStatementWithCurve());
        }
        else if (context.statement() != null)
        {
            var stmt = (StatementSyntax)Visit(context.statement());
            return stmt is BlockSyntax ? stmt : Block(stmt);
        }
        return Block();
    }

    public override CSharpSyntaxNode VisitBinaryExpression([NotNull] GroovyParser.BinaryExpressionContext context)
    {
        var left = (ExpressionSyntax)Visit(context.expression(0));
        var right = (ExpressionSyntax)Visit(context.expression(1));

        // Check for each possible operator
        if (context.GT() != null)
        {
            return BinaryExpression(SyntaxKind.GreaterThanExpression, left, right);
        }

        throw new InvalidOperationException($"Unsupported binary operator in expression: {context.GetText()}");
    }

    public override CSharpSyntaxNode VisitSwitchStatement([NotNull] GroovyParser.SwitchStatementContext context)
    {
        Console.WriteLine($"Visiting SwitchStatement: {context.GetText()}");

        // Get the switch expression
        var switchExpression = (ExpressionSyntax)Visit(context.expression());
        if (switchExpression == null)
        {
            throw new InvalidOperationException("Failed to parse switch expression");
        }

        var sections = new List<SwitchSectionSyntax>();

        // Handle case statements
        foreach (var caseStatement in context.caseStatement())
        {
            var section = (SwitchSectionSyntax)Visit(caseStatement);
            if (section != null)
            {
                sections.Add(section);
            }
        }

        // Handle default case if present
        if (context.KW_DEFAULT() != null)
        {
            var defaultStatements = new List<StatementSyntax>();
            var defaultStart = false;

            foreach (var child in context.children)
            {
                if (child.GetText() == "default")
                {
                    defaultStart = true;
                    continue;
                }

                if (defaultStart && child is GroovyParser.StatementContext stmtContext)
                {
                    var stmt = Visit(stmtContext);
                    if (stmt is StatementSyntax statementSyntax)
                    {
                        defaultStatements.Add(statementSyntax);
                    }
                }
            }

            if (!defaultStatements.Any(s => s is BreakStatementSyntax))
            {
                defaultStatements.Add(BreakStatement());
            }

            sections.Add(
                SwitchSection()
                    .WithLabels(SingletonList<SwitchLabelSyntax>(
                        DefaultSwitchLabel()))
                    .WithStatements(List(defaultStatements)));
        }

        return SwitchStatement(switchExpression)
            .WithSections(List(sections));
    }

    public override CSharpSyntaxNode VisitCaseStatement([NotNull] GroovyParser.CaseStatementContext context)
    {
        Console.WriteLine($"Visiting CaseStatement: {context.GetText()}");

        // Get the case expression
        var caseExpression = (ExpressionSyntax)Visit(context.expression());
        if (caseExpression == null)
        {
            throw new InvalidOperationException("Failed to parse case expression");
        }

        // Collect all statements for this case
        var statements = new List<StatementSyntax>();
        foreach (var stmt in context.statement())
        {
            var visitedStmt = Visit(stmt);
            if (visitedStmt is StatementSyntax statementSyntax)
            {
                statements.Add(statementSyntax);
            }
        }

        // Add break statement if not present
        if (!statements.Any(s => s is BreakStatementSyntax))
        {
            statements.Add(BreakStatement());
        }

        return SwitchSection()
            .WithLabels(SingletonList<SwitchLabelSyntax>(
                CaseSwitchLabel(caseExpression)))
            .WithStatements(List(statements));
    }

} // Close GroovyToCSharpTranspiler class
} // Close TYTCapstone.Transpiler namespace
