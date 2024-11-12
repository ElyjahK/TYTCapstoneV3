using Microsoft.Testing.Platform.Extensions.TestFramework;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace TYTCapstone
{
    public class SemicolonInserter
    {
        public string GroovySourceCode { get; set; } = string.Empty;
        private HashSet<string> INVALID_LINE_TERMINATORS = new HashSet<string> 
        { 
            "+", "-", "*", "/", "||", "?", ":", ".", "(", "[", ",", "}", "{", ";", "\t", "\n", "\r"
        };

        private Stack<int> ENUMERABLE_STACK = new Stack<int>();
        private Stack<int> CLOSURE_STACK = new Stack<int>();
        private Stack<int> METHOD_CALL_STACK = new Stack<int>();
        private Stack<int> SKIP_STACK = new Stack<int>();

        private bool InMultilineComment { get; set; } = false;
        private bool InClosure { get; set; } = false;

        private int Counter = 0;

        private List<string> CodeLines { get; set; } = new();

        private bool InChainedMethodCall { get; set; } = false;

        private bool InMultilineString { get; set; } = false;

        public bool NextLineBeginsWith(string beginsWith)
        {
            if (Counter >= CodeLines.Count - 1)
                return false;

            if (string.IsNullOrEmpty(CodeLines[Counter + 1]) || string.IsNullOrEmpty(CodeLines[Counter + 1].TrimStart()))
                return false;

            return CodeLines[Counter + 1].TrimStart()[0].ToString() == beginsWith;
        }

        public SemicolonInserter(string sourceCode)
        {
            GroovySourceCode = sourceCode;
        }

        private string DetermineEndingWhitespace(char trivia)
        {
            return trivia == '\n' || trivia == '\r' ? "\r\n" : "";
        }

        private string InsertSemicolonOntoLastClosureStatement(string line)
        {
            for (int i = line.Length - 1; i >= 0; i--)
            {
                if (line[i] != '}' && line[i] != '\t' && line[i] != ' ')
                {
                    line = line.Insert(i + 1, ";");
                    break;
                }
            }

            return line;
        }

        private string GetInlineComment(string line)
        {
            // Pattern to match comments at the end of the line, ignoring strings
            string pattern = @"(?<code>.*?)(?<comment>\s*(//.*|/\*.*))?$";

            // Remove string literals to avoid matching comment markers inside strings
            string lineWithoutStrings = Regex.Replace(line, @"""([^""\\]|\\.)*""", "\"\"");

            Match match = Regex.Match(lineWithoutStrings, pattern);
            if (match.Success && match.Groups["comment"].Success)
            {
                return match.Groups["comment"].Value;
            }
            return ""; // No comment found
        }

        public string Execute()
        {
            StringBuilder sb = new StringBuilder();

            CodeLines = GroovySourceCode.Split('\n').ToList();
            
            foreach (string line in CodeLines) 
            {
                Console.WriteLine("current line: " + line);
                if (string.IsNullOrEmpty(line))
                {
                    Counter++;
                    continue;
                }

                char whitespaceTrivia = line.Last();
                string trimmedLine = line.TrimEnd();
                string commentCache = string.Empty;

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

                if (trimmedLine.Contains("//") || trimmedLine.Contains("/*"))
                {
                    if (trimmedLine.TrimStart().StartsWith("//"))
                    {
                        Console.WriteLine("standalone comment" + trimmedLine);
                        sb.Append(trimmedLine + DetermineEndingWhitespace(whitespaceTrivia));

                        Counter++;
                        continue;
                    }

                    commentCache = GetInlineComment(trimmedLine);
                    Console.WriteLine(commentCache);

                    Console.WriteLine("in inline comment logic: " + trimmedLine);
                    trimmedLine = trimmedLine.Replace(commentCache, "");
                }

                if (trimmedLine.EndsWith("*/"))
                {
                    InMultilineComment = false;

                    Console.WriteLine("end of block comment" + trimmedLine);
                    sb.Append(trimmedLine + DetermineEndingWhitespace(whitespaceTrivia));
                    Counter++;
                    continue;
                }

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

                if (trimmedLine.TrimStart().Contains("'''"))
                {
                    InMultilineString = true;

                    Console.WriteLine("Multiline comment start: " + trimmedLine);
                    sb.Append(trimmedLine + DetermineEndingWhitespace(whitespaceTrivia));

                    Counter++;
                    continue;
                }

                if (InMultilineComment || InMultilineString)
                {
                    Console.WriteLine("Still in multiline string or comment: " + trimmedLine);
                    sb.Append(trimmedLine + DetermineEndingWhitespace(whitespaceTrivia));
                    Counter++;
                    continue;
                }

                if (NextLineBeginsWith(".") is false && InChainedMethodCall is true)
                {
                    Console.WriteLine("Out of chained call: " + trimmedLine);

                    trimmedLine = trimmedLine + ";" + commentCache + DetermineEndingWhitespace(whitespaceTrivia);
                    sb.Append(trimmedLine);

                    InChainedMethodCall = false;

                    Counter++;
                    continue;
                }

                if (InChainedMethodCall is true)
                {
                    Console.WriteLine("Still in chained method call" + trimmedLine);
                    sb.Append(trimmedLine + DetermineEndingWhitespace(whitespaceTrivia));

                    Counter++;
                    continue;
                }


                if (NextLineBeginsWith("."))
                {
                    Console.WriteLine("Chained method call: " + trimmedLine);

                    InChainedMethodCall = true;

                    sb.Append(trimmedLine + DetermineEndingWhitespace(whitespaceTrivia));

                    Counter++;
                    continue;
                }

                if (trimmedLine.EndsWith("->"))
                {
                    CLOSURE_STACK.Push(1);

                    Console.WriteLine("In closure: " + trimmedLine);
                    sb.Append(trimmedLine + DetermineEndingWhitespace(whitespaceTrivia));
                    Counter++;
                    continue;
                }

                if (trimmedLine.EndsWith("("))
                {
                    METHOD_CALL_STACK.Push(1);

                    Console.WriteLine("In unfinished method call: " + trimmedLine);
                    sb.Append(trimmedLine + DetermineEndingWhitespace(whitespaceTrivia));
                    Counter++;
                    continue;
                }

                if (trimmedLine.EndsWith("["))
                {
                    ENUMERABLE_STACK.Push(1);

                    Console.WriteLine("In unfinished enumerable declaration: " + trimmedLine);
                    sb.Append(trimmedLine + DetermineEndingWhitespace(whitespaceTrivia));
                    Counter++;
                    continue;
                }

                if (INVALID_LINE_TERMINATORS.Contains(trimmedLine[^1].ToString()) || trimmedLine.TrimStart().StartsWith('@'))
                {
                    Console.WriteLine("invalid terminator or anotation or in multiline comment" + trimmedLine);

                    if (trimmedLine.EndsWith("{") && CLOSURE_STACK.Count > 0)
                    {
                        SKIP_STACK.Push(1);
                    }

                    if (CLOSURE_STACK.Count > 0 && trimmedLine.EndsWith("}"))
                    {
                        Console.WriteLine("End closure" + trimmedLine);

                        if (SKIP_STACK.Count > 0)
                        {
                            Console.WriteLine("Skipping this line due to closure resolve: " + trimmedLine);
                            sb.Append(trimmedLine + DetermineEndingWhitespace(whitespaceTrivia));

                            SKIP_STACK.Pop();

                            Counter++;
                            continue;
                        }

                        trimmedLine = trimmedLine + ";" + commentCache + DetermineEndingWhitespace(whitespaceTrivia);
                        sb.Append(trimmedLine);

                        CLOSURE_STACK.Pop();

                        Counter++;
                        continue;
                    }

                    if (trimmedLine.EndsWith("}") && trimmedLine.Contains("->"))
                    {
                        Console.WriteLine("create normal closure " + trimmedLine);

                        trimmedLine = trimmedLine + ";" + commentCache + DetermineEndingWhitespace(whitespaceTrivia);
                        sb.Append(trimmedLine);

                        Counter++;
                        continue;
                    }

                    if (trimmedLine.EndsWith("}") && trimmedLine.Contains("->") is false && trimmedLine.Contains("{"))
                    {
                        Console.WriteLine("create silly closure " + trimmedLine);
                        trimmedLine = InsertSemicolonOntoLastClosureStatement(trimmedLine);

                        trimmedLine = trimmedLine + ";" + commentCache + DetermineEndingWhitespace(whitespaceTrivia);
                        sb.Append(trimmedLine);

                        Counter++;
                        continue;
                    }

                    if (trimmedLine.EndsWith("--"))
                    {
                        Console.WriteLine("overriding - ending, since its actually unary op --: " + trimmedLine);
                        trimmedLine = trimmedLine + ";" + commentCache + DetermineEndingWhitespace(whitespaceTrivia);
                        sb.Append(trimmedLine);

                        Counter++;
                        continue;
                    }

                    if (trimmedLine.EndsWith("++"))
                    {
                        Console.WriteLine("overriding + ending, since its actually unary op ++: " + trimmedLine);
                        trimmedLine = trimmedLine + ";" + commentCache + DetermineEndingWhitespace(whitespaceTrivia);
                        sb.Append(trimmedLine);

                        Counter++;
                        continue;
                    }

                    sb.Append(trimmedLine + DetermineEndingWhitespace(whitespaceTrivia));

                    Counter++;
                    continue;
                }

                if (METHOD_CALL_STACK.Count > 0)
                {
                    if (trimmedLine.EndsWith(")"))
                    {
                        Console.WriteLine("finished method call: " + trimmedLine);
                        trimmedLine = trimmedLine + ";" + commentCache + DetermineEndingWhitespace(whitespaceTrivia);
                        sb.Append(trimmedLine);

                        METHOD_CALL_STACK.Pop();

                        Counter++;
                        continue;
                    }

                    Console.WriteLine("Still in unfinished method call: " + trimmedLine);

                    sb.Append(trimmedLine + DetermineEndingWhitespace(whitespaceTrivia));

                    Counter++;
                    continue;
                }

                if (ENUMERABLE_STACK.Count > 0)
                {
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

                    sb.Append(trimmedLine + DetermineEndingWhitespace(whitespaceTrivia));

                    Counter++;
                    continue;
                }

                trimmedLine = trimmedLine + ";" + commentCache + DetermineEndingWhitespace(whitespaceTrivia);

                Console.WriteLine(trimmedLine);
                sb.Append(trimmedLine);
            }

            return sb.ToString();
        }
    }
}
