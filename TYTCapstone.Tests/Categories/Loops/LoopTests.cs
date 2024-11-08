using Microsoft.VisualStudio.TestTools.UnitTesting;
using Antlr4.Runtime;
using TYTCapstone.Transpiler;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Castle.DynamicProxy;

namespace TYTCapstone.Tests.Categories.Loops
{
    [TestClass]
    public class LoopTests
    {
        private GroovyToCSharpTranspiler _transpiler;
        public TestContext TestContext { get; set; }

        private ProxyGenerator proxyGenerator = new ProxyGenerator();

        [TestInitialize]
        public void Setup()
        {
            _transpiler = proxyGenerator.CreateClassProxy<GroovyToCSharpTranspiler>(new LoggingInterceptor());
        }

        private void Log(string message)
        {
            TestContext.WriteLine(message);
        }

        private GroovyParser ParseGroovy(string input)
        {
            var inputStream = new AntlrInputStream(input);
            var lexer = new GroovyLexer(inputStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new GroovyParser(tokenStream);
            
            parser.RemoveErrorListeners();
            var errorListener = new ParserErrorListener();
            parser.AddErrorListener(errorListener);
            
            return parser;
        }

        private void PrintTokens(string input)
        {
            var inputStream = new AntlrInputStream(input);
            var lexer = new GroovyLexer(inputStream);
            var tokens = lexer.GetAllTokens();
            
            Log("\nTokens:");
            foreach (var token in tokens)
            {
                Log($"{token.Type}({lexer.Vocabulary.GetSymbolicName(token.Type)}) '{token.Text}' at line {token.Line}:{token.Column}");
            }
            Log("");
        }

        [TestMethod]
        public void TestClassicForLoop()
        {
            // Arrange
            Log("\n=== Testing Classic For Loop Transpilation ===");
            Log("\nInput Groovy Code:");
            var groovyCode = @"for (def i = 0; i < 5; i++) {
    println i;
}";
            Log(groovyCode);

            try
            {
                // Act
                PrintTokens(groovyCode);
                var parser = ParseGroovy(groovyCode);
                var tree = parser.compilationUnit();
                Log("\nParse Tree Structure:");
                Log(tree.ToStringTree(parser));
                
                var result = _transpiler.Transpile(tree);

                // Log the generated code BEFORE assertions
                Assert.IsNotNull(result, "Transpilation result should not be null");
                var csharpCode = result.NormalizeWhitespace().ToFullString();
                Log("\nGenerated C# code (before assertions):");
                Log(csharpCode);

                // Add debug information about specific parts we're looking for
                Log("\nDebug Information:");
                Log($"Contains 'for': {csharpCode.Contains("for")}");
                Log($"Contains 'var i = 0': {csharpCode.Contains("var i = 0")}");
                Log($"Contains 'i < 5': {csharpCode.Contains("i < 5")}");
                Log($"Contains 'i++': {csharpCode.Contains("i++")}");
                Log($"Contains 'Console.WriteLine': {csharpCode.Contains("Console.WriteLine")}");

                // Verify the structure
                Assert.IsTrue(csharpCode.Contains("for"), "Should contain for keyword");
                Assert.IsTrue(csharpCode.Contains("var i = 0"), "Should contain initialization");
                Assert.IsTrue(csharpCode.Contains("i < 5"), "Should contain condition");
                Assert.IsTrue(csharpCode.Contains("i++"), "Should contain increment");
                Assert.IsTrue(csharpCode.Contains("Console.WriteLine"), "Should contain WriteLine");

                Log("\nTest completed successfully!");
            }
            catch (Exception ex)
            {
                Log($"\nDetailed Exception Information:");
                Log($"Message: {ex.Message}");
                Log($"Stack Trace: {ex.StackTrace}");
                throw; // Re-throw to preserve the original stack trace
            }
        }

        [TestMethod]
        public void TestForInLoop()
        {
            // Arrange
            Log("\n=== Testing For-In Loop Transpilation ===");
            Log("\nInput Groovy Code:");
            var groovyCode = @"def numbers = [1, 2, 3, 4, 5];
for (num in numbers) {
    println num;
}";
            Log(groovyCode);

            try
            {
                // Act
                PrintTokens(groovyCode);
                var parser = ParseGroovy(groovyCode);
                var tree = parser.compilationUnit();
                Log("\nParse Tree Structure:");
                Log(tree.ToStringTree(parser));
                
                var result = _transpiler.Transpile(tree);

                // Assert
                Assert.IsNotNull(result, "Transpilation result should not be null");
                var csharpCode = result.NormalizeWhitespace().ToFullString();
                
                Log("\nGenerated C# code:");
                Log(csharpCode);

                // Verify the structure
                Assert.IsTrue(csharpCode.Contains("foreach"), "Should contain foreach keyword");
                Assert.IsTrue(csharpCode.Contains("var num"), "Should contain iteration variable");
                Assert.IsTrue(csharpCode.Contains("numbers"), "Should contain collection name");
                Assert.IsTrue(csharpCode.Contains("Console.WriteLine"), "Should contain WriteLine");

                Log("\nTest completed successfully!");
            }
            catch (Exception ex)
            {
                Log($"\nDetailed Exception Information:");
                Log($"Message: {ex.Message}");
                Log($"Stack Trace: {ex.StackTrace}");
                Assert.Fail($"Transpilation failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        [TestMethod]
        public void TestForColonLoop()
        {
            // Arrange
            Log("\n=== Testing For-Colon Loop Transpilation ===");
            Log("\nInput Groovy Code:");
            var groovyCode = @"def numbers = [1, 2, 3, 4, 5];
for (int num : numbers) {
    println num;
}";
            Log(groovyCode);

            try
            {
                // Act
                PrintTokens(groovyCode);
                var parser = ParseGroovy(groovyCode);
                var tree = parser.compilationUnit();
                Log("\nParse Tree Structure:");
                Log(tree.ToStringTree(parser));
                
                var result = _transpiler.Transpile(tree);

                // Assert
                Assert.IsNotNull(result, "Transpilation result should not be null");
                var csharpCode = result.NormalizeWhitespace().ToFullString();
                
                Log("\nGenerated C# code:");
                Log(csharpCode);

                // Verify the structure
                Assert.IsTrue(csharpCode.Contains("foreach"), "Should contain foreach keyword");
                Assert.IsTrue(csharpCode.Contains("int num"), "Should contain typed iteration variable");
                Assert.IsTrue(csharpCode.Contains("numbers"), "Should contain collection name");
                Assert.IsTrue(csharpCode.Contains("Console.WriteLine"), "Should contain WriteLine");

                Log("\nTest completed successfully!");
            }
            catch (Exception ex)
            {
                Log($"\nDetailed Exception Information:");
                Log($"Message: {ex.Message}");
                Log($"Stack Trace: {ex.StackTrace}");
                Assert.Fail($"Transpilation failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        [TestMethod]
        public void TestWhileLoop()
        {
            // Arrange
            Log("\n=== Testing While Loop Transpilation ===");
            Log("\nInput Groovy Code:");
            var groovyCode = @"def count = 0;
while (count < 5) {
    println count;
    count++;
}";
            Log(groovyCode);

            try
            {
                // Act
                PrintTokens(groovyCode);
                var parser = ParseGroovy(groovyCode);
                var tree = parser.compilationUnit();
                Log("\nParse Tree Structure:");
                Log(tree.ToStringTree(parser));
                
                var result = _transpiler.Transpile(tree);

                // Assert
                Assert.IsNotNull(result, "Transpilation result should not be null");
                var csharpCode = result.NormalizeWhitespace().ToFullString();
                
                Log("\nGenerated C# code:");
                Log(csharpCode);

                // Verify the structure
                Assert.IsTrue(csharpCode.Contains("while"), "Should contain while keyword");
                Assert.IsTrue(csharpCode.Contains("count < 5"), "Should contain condition");
                Assert.IsTrue(csharpCode.Contains("count++"), "Should contain increment");
                Assert.IsTrue(csharpCode.Contains("Console.WriteLine"), "Should contain WriteLine");

                Log("\nTest completed successfully!");
            }
            catch (Exception ex)
            {
                Log($"\nDetailed Exception Information:");
                Log($"Message: {ex.Message}");
                Log($"Stack Trace: {ex.StackTrace}");
                Assert.Fail($"Transpilation failed: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
