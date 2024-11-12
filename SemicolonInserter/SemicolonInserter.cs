using Microsoft.Extensions.Primitives;
using Microsoft.Testing.Platform.Extensions.TestFramework;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace TYTCapstone
{
    /// <summary>
    /// Utility that automatically inserts semicolons in the syntactically correct places in Groovy source code.
    /// </summary>
    public class SemicolonInserter
    {
        public string GroovySourceCode { get; set; } = string.Empty;

        /// <summary>
        /// General set of symbols that act as markers where a semicolon cannot follow.  However, 
        /// this might be broken, such as multiline expressions, ++ and -- operators, and } acting
        /// as a closure ending.
        /// </summary>
        private HashSet<string> INVALID_LINE_TERMINATORS = new HashSet<string> 
        { 
            "+", "-", "*", "/", "||", "?", ":", ".", "(", "[", ",", "}", "{", ";", "\t", "\n", "\r"
        };

        private HashSet<string> RELATIONAL_OPERATORS = new HashSet<string>
        {
            "==", "!=", ">", "<", "<=", ">="
        };

        private HashSet<string> CONTROL_STRUCTURES = new HashSet<string>
        {
            "if", "else", "for", "while", "switch", "try", "catch", "finally"
        };

        /// <summary>
        /// Stack tracking the square braces [] when a multiline enumerator declaration occurs.
        /// </summary>
        private Stack<int> ENUMERABLE_STACK = new Stack<int>();

        /// <summary>
        /// Stack tracking nested closures.
        /// </summary>
        private Stack<int> CLOSURE_STACK = new Stack<int>();

        /// <summary>
        /// Stack tracking unbalanced parenthesis that occur in multiline method declarations or control structures.
        /// </summary>
        private Stack<int> PAREN_STACK = new Stack<int>();

        /// <summary>
        /// Since this system works with very little semantic analysis, each time we see an opening {
        /// we assume it is an invalid line ending, and skip the semicolon.  However, on the corresponding
        /// }, previous assumptions lead to the algorithm to assume that is where a closure ends.  This stack
        /// manages this state, an skips over blocks within a closure to prevent that.
        /// </summary>
        private Stack<int> SKIP_STACK = new Stack<int>();

        /// <summary>
        /// Flag determining if the current line is in a multiline comment.
        /// </summary>
        private bool InMultilineComment { get; set; } = false;

        /// <summary>
        /// Used to track which line we are looking at.
        /// </summary>
        private int Counter = 0;

        /// <summary>
        /// List of lines in the Groovy source code.
        /// </summary>
        private List<string> CodeLines { get; set; } = new();

        /// <summary>
        /// Flag determining if we are in a chained method call.
        /// </summary>
        private bool InChainedMethodCall { get; set; } = false;

        /// <summary>
        /// Flag determining if the current line is within a multiline string.
        /// </summary>
        private bool InMultilineString { get; set; } = false;

        /// <summary>
        /// Flag determining if the current line is within an unfinished control structure call.
        /// </summary>
        private bool InUnfinishedControlStructure { get; set; } = false;

        /// <summary>
        /// Checks if the next line in the source code begins with a certain string
        /// </summary>
        /// <param name="beginsWith"></param>
        /// <returns>True if the next line begins with the input string, false otherwise</returns>
        public bool NextLineBeginsWith(string beginsWith)
        {
            if (Counter >= CodeLines.Count - 1)
                return false;

            if (string.IsNullOrEmpty(CodeLines[Counter + 1]) || string.IsNullOrEmpty(CodeLines[Counter + 1].TrimStart()))
                return false;

            return CodeLines[Counter + 1].TrimStart()[0].ToString() == beginsWith;
        }

        /// <summary>
        /// Utility that automatically inserts semicolons in the syntactically correct places in Groovy source code.
        /// </summary>
        public SemicolonInserter(string sourceCode)
        {
            GroovySourceCode = sourceCode;
        }

        /// <summary>
        /// Determines if the end of the line is a \n or a \r.  
        /// </summary>
        /// <param name="trivia"></param>
        /// <returns>A string "\r\n" if the ending whitespace trivia is a carriage return or a new line.  "" otherwise.</returns>
        private static string DetermineEndingWhitespace(char trivia)
        {
            return trivia == '\n' || trivia == '\r' ? "\r\n" : "";
        }

        private bool LastStatementInClosureIsExpression(int closingBraceIdx, string line)
        {
            StringBuilder block = new();
            int openingBraceIdx = 0;

            for (int i = closingBraceIdx; i >= 0; i--)
            {
                if (line[i] == '{')
                {
                    openingBraceIdx = i;
                }
            }

            for (int i = openingBraceIdx; i < closingBraceIdx; i++)
            {
                block.Append(line[i]);
            }

            Console.WriteLine("Block: " + block.ToString());

            if (RELATIONAL_OPERATORS.Any(op => block.ToString().Contains(op)))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Some statements in closures can happen inline. This inserts a semicolon on the last statement within that inline closure.
        /// </summary>
        /// <param name="line"></param>
        /// <returns>The line with the inserted semicolon.</returns>
        private string InsertSemicolonOntoLastClosureStatement(string line)
        {
            for (int i = line.Length - 1; i >= 0; i--)
            {
                if (line[i] == '}')
                {
                    Console.WriteLine("Checking block for expression");
                    if (LastStatementInClosureIsExpression(i, line))
                    {
                        Console.WriteLine("Block is expression, returning...");
                        return line;
                    }
                }

                if (line[i] != '}' && line[i] != '\t' && line[i] != ' ')
                {
                    line = line.Insert(i + 1, ";");
                    break;
                }
            }

            return line;
        }

        /// <summary>
        /// Returns the inline comment and all whitespace before it.
        /// </summary>
        /// <param name="line"></param>
        /// <returns>The inline comment with all preceding whitespace.</returns>
        private static string GetInlineComment(string line)
        {
            // Pattern to match comments at the end of the line, ignoring strings
            string pattern = @"(?<code>.*?)(?<comment>\s*(//.*|/\*.*))?$";

            Match match = Regex.Match(line, pattern);
            if (match.Success && match.Groups["comment"].Success)
            {
                return match.Groups["comment"].Value;
            }
            return ""; // No comment found
        }

        /// <summary>
        /// Checks if a line contains a block (anything between a { and a }) that has nothing in it.
        /// </summary>
        /// <param name="line"></param>
        /// <returns>True if the inline block is empty, false otherwise.</returns>
        private static bool BlockContainsNothing(string line)
        {
            int idx1 = line.IndexOf('{');
            int idx2 = line.IndexOf('}');

            return idx2 - idx1 == 1;
        }

        /// <summary>
        /// Checks if the parenthesis in a line are balanced.
        /// </summary>
        /// <param name="line"></param>
        /// <returns>True if the parenthesis are balanced, false otherwise.</returns>
        private static bool ParensAreBalanced(string line)
        {
            Stack<char> chars = new Stack<char>();

            foreach (char c in line){
                if (c == '(')
                    chars.Push(c);
                if (c == ')' && chars.Count > 0)
                    chars.Pop();
            }

            return !(chars.Count > 0);
        }

        /// <summary>
        /// Updates the stack of parenthesis based on the current line.
        /// </summary>
        /// <param name="line"></param>
        private void UpdateParenStack(string line)
        {
            foreach (char c in line)
            {
                if (c == '(')
                {
                    PAREN_STACK.Push(c);
                }
                else if (c == ')' && PAREN_STACK.Count > 0)
                {
                    PAREN_STACK.Pop();
                }
            }
        }

        /// <summary>
        /// Inserts semicolons into the semantically correct positions of Groovy source code.
        /// </summary>
        /// <returns>A string containing Groovy code with inserted semicolons.</returns>
        public string Execute()
        {
            StringBuilder sb = new StringBuilder();

            // split the code into lines, preserving the separator (\n)
            CodeLines = Regex.Split(GroovySourceCode, @"(?<=[\n])").ToList();
            
            foreach (string line in CodeLines) 
            {
                Console.WriteLine("\n\nCURRENT LINE: " + line);

                // Skip line if it is empty
                if (string.IsNullOrEmpty(line))
                {
                    Counter++;
                    continue;
                }

                // Save whitespace at the end of the line
                char whitespaceTrivia = line.Last();

                // Then trim the end
                string trimmedLine = line.TrimEnd();

                // Setup a small cache for inline comments, since we are going to need to break 
                // apart the code and comment, then recombine them again later.
                string commentCache = string.Empty;

                // Update the parenthesis stack
                UpdateParenStack(trimmedLine);

                // If the trimmed line is still empty, write the line and continue.
                if (string.IsNullOrEmpty(trimmedLine))
                {
                    Console.WriteLine("empty line after trim" + trimmedLine);
                    sb.Append(trimmedLine + DetermineEndingWhitespace(whitespaceTrivia));

                    Counter++;
                    continue;
                }

                if (trimmedLine.TrimStart().StartsWith("/*") || trimmedLine.TrimStart().StartsWith("/**"))
                {
                    if (trimmedLine.EndsWith("*/") is false)
                    {
                        InMultilineComment = true;
                    }

                    Console.WriteLine("in block comment logic" + trimmedLine);
                    sb.Append(trimmedLine + DetermineEndingWhitespace(whitespaceTrivia));

                    Counter++;
                    continue;
                }

                // If the line contains an inline comment, cache it for recombination later.
                if (trimmedLine.Contains("//") || trimmedLine.Contains("/*"))
                {
                    // However, if this line is just a comment, pass it through and move on.
                    if (trimmedLine.TrimStart().StartsWith("//"))
                    {
                        Console.WriteLine("standalone comment" + trimmedLine);
                        sb.Append(trimmedLine + DetermineEndingWhitespace(whitespaceTrivia));

                        Counter++;
                        continue;
                    }

                    commentCache = GetInlineComment(trimmedLine);
                    Console.WriteLine("FOUND COMMENT: " + commentCache);

                    Console.WriteLine("in inline comment logic: " + trimmedLine);

                    // Need to remove the comment from the line so we know where to place the semicolon.
                    trimmedLine = trimmedLine.Replace(commentCache, "");
                }

                // If our parenthesis are still unbalanced, do not place a semicolon and continue
                if (PAREN_STACK.Count > 0)
                {
                    Console.WriteLine("Parens unbalanced still, must be in an unfinished method call or control structure: " + trimmedLine);   
                    sb.Append(trimmedLine + commentCache + DetermineEndingWhitespace(whitespaceTrivia));
                    Counter++;
                    continue;
                }

                // If the line is a balanced control structure that does not end with a {, then skip.
                if (CONTROL_STRUCTURES.Any(keyword => trimmedLine.TrimStart().StartsWith(keyword)) && ParensAreBalanced(trimmedLine) && trimmedLine.EndsWith("{") is false)
                {
                    Console.WriteLine("Non block control statement" + trimmedLine);
                    sb.Append(trimmedLine + commentCache + DetermineEndingWhitespace(whitespaceTrivia));
                    Counter++;
                    continue;
                }

                // If this is the end of a multiline comment, do not place a semicolon and continue
                if (trimmedLine.EndsWith("*/"))
                {
                    InMultilineComment = false;

                    Console.WriteLine("end of block comment" + trimmedLine);
                    sb.Append(trimmedLine + commentCache + DetermineEndingWhitespace(whitespaceTrivia));
                    Counter++;
                    continue;
                }

                // If this is the end of a multiline string, place a semicolon.
                if (trimmedLine.Trim() == "'''")
                {
                    InMultilineString = false;

                    Console.WriteLine("End of multiline string: " + trimmedLine);

                    trimmedLine = trimmedLine + ";" + commentCache + DetermineEndingWhitespace(whitespaceTrivia);
                    sb.Append(trimmedLine);

                    InChainedMethodCall = false;

                    Counter++;
                    continue;
                }

                // If this is the start of a multiline string, set the flag to true, do not place a semicolon, and move on.
                if (trimmedLine.TrimStart().Contains("'''"))
                {
                    InMultilineString = true;

                    Console.WriteLine("Multiline comment start: " + trimmedLine);
                    sb.Append(trimmedLine + commentCache + DetermineEndingWhitespace(whitespaceTrivia));

                    Counter++;
                    continue;
                }

                // If this line is in a multiline string or comment, do not place a semicolon and move on.
                if (InMultilineComment || InMultilineString)
                {
                    Console.WriteLine("Still in multiline string or comment: " + trimmedLine);
                    sb.Append(trimmedLine + commentCache + DetermineEndingWhitespace(whitespaceTrivia));
                    Counter++;
                    continue;
                }

                // If the next line does not begin with a ., we can assume the multiline chained method call is over, and place a semicolon at the end.
                if (NextLineBeginsWith(".") is false && InChainedMethodCall is true)
                {
                    Console.WriteLine("Out of chained call: " + trimmedLine);

                    trimmedLine = trimmedLine + ";" + commentCache + DetermineEndingWhitespace(whitespaceTrivia);
                    sb.Append(trimmedLine);

                    InChainedMethodCall = false;

                    Counter++;
                    continue;
                }

                // If we are still in a multiline chained method call, do not place a semicolon and move on.
                if (InChainedMethodCall is true)
                {
                    Console.WriteLine("Still in chained method call" + trimmedLine);
                    sb.Append(trimmedLine + commentCache + DetermineEndingWhitespace(whitespaceTrivia));

                    Counter++;
                    continue;
                }

                // If the next line begins with a ".", we can assume we are entering a multiline chained method call, do not place a semicolon and move on.
                if (NextLineBeginsWith("."))
                {
                    Console.WriteLine("Chained method call: " + trimmedLine);

                    InChainedMethodCall = true;

                    sb.Append(trimmedLine + commentCache + DetermineEndingWhitespace(whitespaceTrivia));

                    Counter++;
                    continue;
                }

                // If the line end with a ->, we are in a closure, do not place a semicolon and move on.
                if (trimmedLine.EndsWith("->"))
                {
                    CLOSURE_STACK.Push(1);

                    Console.WriteLine("In closure: " + trimmedLine);
                    sb.Append(trimmedLine + commentCache + DetermineEndingWhitespace(whitespaceTrivia));
                    Counter++;
                    continue;
                }

                // If the parenthesis are unbalanced at this point, we are still in an unfinished control structure or method call, do not place a semicolon and move on.
                if (ParensAreBalanced(trimmedLine) is false)
                {
                    UpdateParenStack(trimmedLine);
                    
                    Console.WriteLine("In unfinished method call or control structure: " + trimmedLine);
                    sb.Append(trimmedLine + commentCache + DetermineEndingWhitespace(whitespaceTrivia));
                    Counter++;
                    continue;
                }

                // Indicates the start of a multiline enumerable declaration, do not place a semicolon and move on.
                if (trimmedLine.EndsWith("["))
                {
                    ENUMERABLE_STACK.Push(1);

                    Console.WriteLine("In unfinished enumerable declaration: " + trimmedLine);
                    sb.Append(trimmedLine + commentCache + DetermineEndingWhitespace(whitespaceTrivia));
                    Counter++;
                    continue;
                }

                // If the line ends with an invalid terminator or is an anotation, it requires further analysis.
                if (INVALID_LINE_TERMINATORS.Contains(trimmedLine[^1].ToString()) || trimmedLine.TrimStart().StartsWith('@'))
                {
                    Console.WriteLine("invalid terminator or anotation or in multiline comment" + trimmedLine);

                    // If we are nested in a closure already, arm our skip stack with a block to skip over.
                    if (trimmedLine.EndsWith("{") && CLOSURE_STACK.Count > 0)
                    {
                        SKIP_STACK.Push(1);
                    }

                    // If we are still in a closure, and the line ends with a }, we are out of the closure and can place a semicolon
                    if (CLOSURE_STACK.Count > 0 && trimmedLine.EndsWith("}"))
                    {
                        Console.WriteLine("End closure" + trimmedLine);

                        // However, if the skip stack is armed with something, skip over this brace and not place a semicolon.
                        if (SKIP_STACK.Count > 0)
                        {
                            Console.WriteLine("Skipping this line due to closure resolve: " + trimmedLine);
                            sb.Append(trimmedLine + commentCache + DetermineEndingWhitespace(whitespaceTrivia));

                            SKIP_STACK.Pop();

                            Counter++;
                            continue;
                        }

                        trimmedLine = trimmedLine + ";" + commentCache + DetermineEndingWhitespace(whitespaceTrivia);
                        sb.Append(trimmedLine);

                        // Pop from the stack
                        CLOSURE_STACK.Pop();

                        Counter++;
                        continue;
                    }

                    // If are in an inline closure, then we can place a semicolon.
                    if (trimmedLine.EndsWith("}") && trimmedLine.Contains("->"))
                    {
                        Console.WriteLine("create normal closure " + trimmedLine);

                        trimmedLine = trimmedLine + ";" + commentCache + DetermineEndingWhitespace(whitespaceTrivia);
                        sb.Append(trimmedLine);

                        Counter++;
                        continue;
                    }

                    // If we are in different form of an inline closure, then we can place a semicolon.
                    if (trimmedLine.Contains("{") && trimmedLine.EndsWith("}") && trimmedLine.Contains("->") is false)
                    {
                        // However, if the text between the { and } is empty, then skip.
                        if (BlockContainsNothing(trimmedLine)) 
                        {
                            Console.WriteLine("Empty block: " + trimmedLine);
                            sb.Append(trimmedLine + commentCache + DetermineEndingWhitespace(whitespaceTrivia));
                            Counter++;
                            continue;
                        }

                        Console.WriteLine("create silly closure " + trimmedLine);

                        // Also need to place a semicolon at the last statement in this closure.
                        trimmedLine = InsertSemicolonOntoLastClosureStatement(trimmedLine);

                        trimmedLine = trimmedLine + ";" + commentCache + DetermineEndingWhitespace(whitespaceTrivia);
                        sb.Append(trimmedLine);

                        Counter++;
                        continue;
                    }

                    // If the line ends with an --, then override the invalid line terminator logic, and place a semicolon.
                    if (trimmedLine.EndsWith("--"))
                    {
                        Console.WriteLine("overriding '-' ending, since its actually unary operator --: " + trimmedLine);
                        trimmedLine = trimmedLine + ";" + commentCache + DetermineEndingWhitespace(whitespaceTrivia);
                        sb.Append(trimmedLine);

                        Counter++;
                        continue;
                    }

                    // If the line ends with an ++, then override the invalid line terminator logic, and place a semicolon.
                    if (trimmedLine.EndsWith("++"))
                    {
                        Console.WriteLine("overriding '+' ending, since its actually unary operator ++: " + trimmedLine);
                        trimmedLine = trimmedLine + ";" + commentCache + DetermineEndingWhitespace(whitespaceTrivia);
                        sb.Append(trimmedLine);

                        Counter++;
                        continue;
                    }

                    // Else, just skip.
                    sb.Append(trimmedLine + commentCache + DetermineEndingWhitespace(whitespaceTrivia));

                    Counter++;
                    continue;
                }

                // If the [ hasnt been closed, dont place a semicolon and move on.
                if (ENUMERABLE_STACK.Count > 0)
                {
                    // If it actually ends with a brace however, then place a semicolon.
                    if (trimmedLine.EndsWith("]"))
                    {
                        Console.WriteLine("finished enuermable declaration: " + trimmedLine);

                        trimmedLine = trimmedLine + ";" + commentCache + DetermineEndingWhitespace(whitespaceTrivia);
                        sb.Append(trimmedLine);

                        ENUMERABLE_STACK.Pop();

                        Counter++;
                        continue;
                    }

                    Console.WriteLine("Still in unfinished enumerable declaration: " + trimmedLine);

                    sb.Append(trimmedLine + commentCache + DetermineEndingWhitespace(whitespaceTrivia));

                    Counter++;
                    continue;
                }

                // If all the above falls through, we insert a semicolon.
                trimmedLine = trimmedLine + ";" + commentCache + DetermineEndingWhitespace(whitespaceTrivia);

                Console.WriteLine("INSERTED SEMICOLON: " + trimmedLine);
                sb.Append(trimmedLine);
            }

            return sb.ToString();
        }
    }
}
