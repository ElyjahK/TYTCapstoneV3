using Antlr4.Runtime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TYTCapstone.Transpiler;

namespace TYTCapstone
{
    class Program
    {
        static void Main(string[] args)
        {
            // Example usage
            var groovyCode = @"
                class Person {
                    String name
                    Integer age
                    
                    def greet() {
                        return ""Hello, ${name}!""
                    }
                }";

            try
            {
                // Set up the lexer and parser
                var inputStream = new AntlrInputStream(groovyCode);
                var lexer = new GroovyLexer(inputStream);
                var tokenStream = new CommonTokenStream(lexer);
                var parser = new GroovyParser(tokenStream);

                // Parse the input
                var tree = parser.compilationUnit();

                // Create and run transpiler
                var transpiler = new GroovyToCSharpTranspiler();
                var result = transpiler.Transpile(tree);

                // Print the result
                Console.WriteLine("Transpiled C# code:");
                Console.WriteLine(result.ToFullString());

                // Verify the output compiles
                var syntaxTree = CSharpSyntaxTree.ParseText(result.ToFullString());
                var compilation = CSharpCompilation.Create("TestAssembly")
                    .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                    .AddSyntaxTrees(syntaxTree);

                var diagnostics = compilation.GetDiagnostics();
                foreach (var diagnostic in diagnostics)
                {
                    Console.WriteLine($"{diagnostic.Severity}: {diagnostic.GetMessage()}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}