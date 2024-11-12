using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TYTCapstone.Tests.Categories.SemiColonInserter
{

    [TestClass]
    public class SemicolonInserterTests
    {
        private SemicolonInserter? SemicolonInserter { get; set; }

        [TestMethod]
        public void SimpleStatementTest()
        {
            string code = "def greeting = 'Hello'";

            SemicolonInserter = new SemicolonInserter(code);
            string result = SemicolonInserter.Execute();

            Assert.AreEqual("def greeting = 'Hello';", result);
        }

        [TestMethod]
        public void MultiLineExpressionTest()
        {
            string code = "def sum = 1 +\r\n          2 +\r\n          3";

            SemicolonInserter = new SemicolonInserter(code);
            string result = SemicolonInserter.Execute();

            Console.WriteLine("\n\nRESULT: \n\n" + result);

            Assert.AreEqual("def sum = 1 +\r\n          2 +\r\n          3;", result);
        }

        [TestMethod]
        public void MethodClosureTest()
        {
            string code = "def greet() {\r\n    println 'Hello, World!'\r\n}";

            SemicolonInserter = new SemicolonInserter(code);
            string result = SemicolonInserter.Execute();

            Console.WriteLine("\n\nRESULT: \n\n" + result);

            Assert.AreEqual("def greet() {\r\n    println 'Hello, World!';\r\n}", result);
        }

        [TestMethod]
        public void ListDeclarationTest()
        {
            string code = "list = [1,\r\n        2,\r\n        3]";

            SemicolonInserter = new SemicolonInserter(code);
            string result = SemicolonInserter.Execute();

            Console.WriteLine("\n\nRESULT: \n\n" + result);

            Assert.AreEqual("list = [1,\r\n        2,\r\n        3];", result);
        }

        [TestMethod]
        public void ControlStatementTest()
        {
            string code = "if (x > 0) {\r\n    println 'Positive'\r\n}";

            SemicolonInserter = new SemicolonInserter(code);
            string result = SemicolonInserter.Execute();

            Console.WriteLine("\n\nRESULT: \n\n" + result);

            Assert.AreEqual("if (x > 0) {\r\n    println 'Positive';\r\n}", result);
        }

        [TestMethod]
        public void AnnotationTest()
        {
            string code = "@Override\r\ndef toString() {\r\n    println 'Hello, World!'\r\n}";

            SemicolonInserter = new SemicolonInserter(code);
            string result = SemicolonInserter.Execute();

            Console.WriteLine("\n\nRESULT: \n\n" + result);

            Assert.AreEqual("@Override\r\ndef toString() {\r\n    println 'Hello, World!';\r\n}", result);
        }

        [TestMethod]
        public void InlineCommentTest()
        {
            string code = "def greeting = 'Hello'  // Insert semicolon here";

            SemicolonInserter = new SemicolonInserter(code);
            string result = SemicolonInserter.Execute();

            Console.WriteLine("\n\nRESULT: \n\n" + result);

            Assert.AreEqual("def greeting = 'Hello';  // Insert semicolon here", result);
        }

        [TestMethod]
        public void StandaloneCommentTest()
        {
            string code = "// This is a comment";

            SemicolonInserter = new SemicolonInserter(code);
            string result = SemicolonInserter.Execute();

            Console.WriteLine("\n\nRESULT: \n\n" + result);

            Assert.AreEqual("// This is a comment", result);
        }

        [TestMethod]
        public void StandaloneMultiLineCommentTest()
        {
            string code = "/* a standalone multiline comment\r\n   spanning two lines */";

            SemicolonInserter = new SemicolonInserter(code);
            string result = SemicolonInserter.Execute();

            Console.WriteLine("\n\nRESULT: \n\n" + result);

            Assert.AreEqual("/* a standalone multiline comment\r\n   spanning two lines */", result);
        }

        [TestMethod]
        public void InlineMultiLineCommentTest()
        {
            string code = "def greeting = 'Hello'  /* a standalone multiline comment*/\r\n";

            SemicolonInserter = new SemicolonInserter(code);
            string result = SemicolonInserter.Execute();

            Console.WriteLine("\n\nRESULT: \n\n" + result);

            Assert.AreEqual("def greeting = 'Hello';  /* a standalone multiline comment*/\r\n", result);
        }

        [TestMethod]
        public void GroovyDocTest()
        {
            string code = "/**\r\n * A Class description\r\n */\r\nclass Person {\r\n    /** the name of the person */\r\n    String name\r\n\r\n    /**\r\n     * Creates a greeting method for a certain person.\r\n     *\r\n     * @param otherPerson the person to greet\r\n     * @return a greeting message\r\n     */\r\n    String greet(String otherPerson) {\r\n}\r\n}";

            SemicolonInserter = new SemicolonInserter(code);
            string result = SemicolonInserter.Execute();

            Console.WriteLine("\n\nRESULT: \n\n" + result);

            Assert.AreEqual("/**\r\n * A Class description\r\n */\r\nclass Person {\r\n    /** the name of the person */\r\n    String name;\r\n\r\n    /**\r\n     * Creates a greeting method for a certain person.\r\n     *\r\n     * @param otherPerson the person to greet\r\n     * @return a greeting message\r\n     */\r\n    String greet(String otherPerson) {\r\n}\r\n}", result);
        }

        [TestMethod]
        public void InlineBlockCommentTest()
        {
            string code = "/* a standalone multiline comment\r\n   spanning two lines */\r\nprintln \"hello\" /* a multiline comment starting\r\n                   at the end of a statement */";

            SemicolonInserter = new SemicolonInserter(code);
            string result = SemicolonInserter.Execute();

            Console.WriteLine("\n\nRESULT: \n\n" + result);

            Assert.AreEqual("/* a standalone multiline comment\r\n   spanning two lines */\r\nprintln \"hello\"; /* a multiline comment starting\r\n                   at the end of a statement */", result);
        }

        [TestMethod]
        public void ClosureTest()
        {
            string code = "def list = [1, 2, 3];\r\nlist.each { item ->\r\n    println item\r\n}";

            SemicolonInserter = new SemicolonInserter(code);
            string result = SemicolonInserter.Execute();

            Console.WriteLine("\n\nRESULT: \n\n" + result);

            Assert.AreEqual("def list = [1, 2, 3];\r\nlist.each { item ->\r\n    println item;\r\n};", result);
        }

        [TestMethod]
        public void MultiLineMethodCallTest()
        {
            string code = "println(\r\n    \"Hello, World!\"\r\n)";

            SemicolonInserter = new SemicolonInserter(code);
            string result = SemicolonInserter.Execute();

            Console.WriteLine("\n\nRESULT: \n\n" + result);

            Assert.AreEqual("println(\r\n    \"Hello, World!\"\r\n);", result);
        }

        [TestMethod]
        public void MultilineEnumerableDeclarationTest()
        {
            string code = "def numbers = [\r\n    1,\r\n    2,\r\n    3\r\n]\r\ndef person = [\r\n    name: 'Alice',\r\n    age: 30\r\n]";

            SemicolonInserter = new SemicolonInserter(code);
            string result = SemicolonInserter.Execute();

            Console.WriteLine("\n\nRESULT: \n\n" + result);

            Assert.AreEqual("def numbers = [\r\n    1,\r\n    2,\r\n    3\r\n];\r\ndef person = [\r\n    name: 'Alice',\r\n    age: 30\r\n];", result);
        }

        [TestMethod]
        public void ForLoopTest()
        {
            string code = "for (int i = 0; i < 5; i++) {\r\n    println i\r\n}";

            SemicolonInserter = new SemicolonInserter(code);
            string result = SemicolonInserter.Execute();

            Console.WriteLine("\n\nRESULT: \n\n" + result);

            Assert.AreEqual("for (int i = 0; i < 5; i++) {\r\n    println i;\r\n}", result);
        }

        [TestMethod]
        public void WhileLoopTest()
        {
            string code = "int count = 5\r\nwhile (count > 0) {\r\n    println count\r\n    count--\r\n    count++\r\n}";

            SemicolonInserter = new SemicolonInserter(code);
            string result = SemicolonInserter.Execute();

            Console.WriteLine("\n\nRESULT: \n\n" + result);

            Assert.AreEqual("int count = 5;\r\nwhile (count > 0) {\r\n    println count;\r\n    count--;\r\n    count++;\r\n}", result);
        }

        [TestMethod]
        public void InlineClosureTest()
        {
            string code = "def square = { num -> num * num }\r\nprintln square(5)";

            SemicolonInserter = new SemicolonInserter(code);
            string result = SemicolonInserter.Execute();

            Console.WriteLine("\n\nRESULT: \n\n" + result);

            Assert.AreEqual("def square = { num -> num * num };\r\nprintln square(5);", result);
        }

        [TestMethod]
        public void StringLineContinuationTest()
        {
            string code = "def message = \"This is a long message \" +\r\n              \"that spans multiple lines.\"\r\nprintln message";

            SemicolonInserter = new SemicolonInserter(code);
            string result = SemicolonInserter.Execute();

            Console.WriteLine("\n\nRESULT: \n\n" + result);

            Assert.AreEqual("def message = \"This is a long message \" +\r\n              \"that spans multiple lines.\";\r\nprintln message;", result);
        }

        [TestMethod]
        public void MethodCallTest()
        {
            string code = "def result = \"Hello\".toUpperCase().reverse()\r\nprintln result";

            SemicolonInserter = new SemicolonInserter(code);
            string result = SemicolonInserter.Execute();

            Console.WriteLine("\n\nRESULT: \n\n" + result);

            Assert.AreEqual("def result = \"Hello\".toUpperCase().reverse();\r\nprintln result;", result);
        }

        [TestMethod]
        public void ChainedMethodCallsTest()
        {
            string code = "def result = \"Hello\"\r\n                .toUpperCase()\r\n                .reverse()\r\nprintln result";

            SemicolonInserter = new SemicolonInserter(code);
            string result = SemicolonInserter.Execute();

            Console.WriteLine("\n\nRESULT: \n\n" + result);

            Assert.AreEqual("def result = \"Hello\"\r\n                .toUpperCase()\r\n                .reverse();\r\nprintln result;", result);
        }

        [TestMethod]
        public void TernaryOperatorTest()
        {
            string code = "def max = (a > b) ? a : b";

            SemicolonInserter = new SemicolonInserter(code);
            string result = SemicolonInserter.Execute();

            Console.WriteLine("\n\nRESULT: \n\n" + result);

            Assert.AreEqual("def max = (a > b) ? a : b;", result);
        }

        [TestMethod]
        public void SwitchTest()
        {
            string code = "def value = 2\r\nswitch (value) {\r\n    case 1:\r\n        println 'One'\r\n        break\r\n    case 2:\r\n        println 'Two'\r\n        break\r\n    default:\r\n        println 'Other'\r\n}";

            SemicolonInserter = new SemicolonInserter(code);
            string result = SemicolonInserter.Execute();

            Console.WriteLine("\n\nRESULT: \n\n" + result);

            Assert.AreEqual("def value = 2;\r\nswitch (value) {\r\n    case 1:\r\n        println 'One';\r\n        break;\r\n    case 2:\r\n        println 'Two';\r\n        break;\r\n    default:\r\n        println 'Other';\r\n}", result);
        }

        [TestMethod]
        public void TryCatchFinallyTest()
        {
            string code = "try {\r\n    // Some code that might throw an exception\r\n} catch (Exception e) {\r\n    println 'An error occurred'\r\n} finally {\r\n    println 'This always executes'\r\n}";

            SemicolonInserter = new SemicolonInserter(code);
            string result = SemicolonInserter.Execute();

            Console.WriteLine("\n\nRESULT: \n\n" + result);

            Assert.AreEqual("try {\r\n    // Some code that might throw an exception\r\n} catch (Exception e) {\r\n    println 'An error occurred';\r\n} finally {\r\n    println 'This always executes';\r\n}", result);
        }

        [TestMethod]
        public void MethodCallNamedParametersTest()
        {
            string code = "def configure(Map options) {\r\n    println options\r\n}\r\nconfigure(\r\n    timeout: 30,\r\n    verbose: true\r\n)";

            SemicolonInserter = new SemicolonInserter(code);
            string result = SemicolonInserter.Execute();

            Console.WriteLine("\n\nRESULT: \n\n" + result);

            Assert.AreEqual("def configure(Map options) {\r\n    println options;\r\n}\r\nconfigure(\r\n    timeout: 30,\r\n    verbose: true\r\n);", result);
        }

        [TestMethod]
        public void ClassDefinitionTest()
        {
            string code = "class Person {\r\n    String name\r\n    int age\r\n\r\n    def introduce() {\r\n        println \"Hi, I'm $name and I'm $age years old.\"\r\n    }\r\n}\r\n\r\ndef person = new Person(name: 'Alice', age: 30)\r\nperson.introduce()";

            SemicolonInserter = new SemicolonInserter(code);
            string result = SemicolonInserter.Execute();

            Console.WriteLine("\n\nRESULT: \n\n" + result);

            Assert.AreEqual("class Person {\r\n    String name;\r\n    int age;\r\n\r\n    def introduce() {\r\n        println \"Hi, I'm $name and I'm $age years old.\";\r\n    }\r\n}\r\n\r\ndef person = new Person(name: 'Alice', age: 30);\r\nperson.introduce();", result);
        }

        [TestMethod]
        public void NestedClosuresTest()
        {
            string code = "def times = { n, closure ->\r\n    for (int i = 0; i < n; i++) {\r\n        closure(i)\r\n    }\r\n}\r\ntimes(3) { println it }";

            SemicolonInserter = new SemicolonInserter(code);
            string result = SemicolonInserter.Execute();

            Console.WriteLine("\n\nRESULT: \n\n" + result);

            Assert.AreEqual("def times = { n, closure ->\r\n    for (int i = 0; i < n; i++) {\r\n        closure(i);\r\n    }\r\n};\r\ntimes(3) { println it; };", result);
        }

        [TestMethod]
        public void MultiLineStringTest()
        {
            string code = "def multiLineString = '''\r\nThis is a\r\nmulti-line\r\nstring\r\n'''\r\nprintln multiLineString";

            SemicolonInserter = new SemicolonInserter(code);
            string result = SemicolonInserter.Execute();

            Console.WriteLine("\n\nRESULT: \n\n" + result);

            Assert.AreEqual("def multiLineString = '''\r\nThis is a\r\nmulti-line\r\nstring\r\n''';\r\nprintln multiLineString;", result);
        }

        [TestMethod]
        public void ComplexNestedClosureTest()
        {
            string code = "if (swTimeStampsAx1!='' && swTimeStampsAx2!='' && swTimeStampsAx3!='' && swTimeStampsAx4!=''){\r\n                int idx = 1\r\n                swTimeStampsAx1.split(',').each{s->\r\n                    if (s.size()>2){\r\n                        String sub1 = s.substring(0,2)\r\n                        newLog.setProperty(\"s${String.format(\"%02d\", idx)}Name\", sub1)\r\n                        String sub2 = s.substring(2, s.size())\r\n                        newLog.setProperty(\"s${String.format(\"%02d\", idx)}1\", Integer.valueOf(sub2))\r\n                    }\r\n                    idx++\r\n                }\r\n            }";

            SemicolonInserter = new SemicolonInserter(code);
            string result = SemicolonInserter.Execute();

            Console.WriteLine("\n\nRESULT: \n\n" + result);

            Assert.AreEqual("if (swTimeStampsAx1!='' && swTimeStampsAx2!='' && swTimeStampsAx3!='' && swTimeStampsAx4!=''){\r\n                int idx = 1;\r\n                swTimeStampsAx1.split(',').each{s->\r\n                    if (s.size()>2){\r\n                        String sub1 = s.substring(0,2);\r\n                        newLog.setProperty(\"s${String.format(\"%02d\", idx)}Name\", sub1);\r\n                        String sub2 = s.substring(2, s.size());\r\n                        newLog.setProperty(\"s${String.format(\"%02d\", idx)}1\", Integer.valueOf(sub2));\r\n                    }\r\n                    idx++;\r\n                };\r\n            }", result);
        }

        [TestMethod]
        public void ComplexMethodMultilineArguments()
        {
            string code = "static private String createWhereProjections(CarLogFilterObjectService carLogFilter, //Filter for the car records\r\n                                             List <String> distincts, //Returns results using a single or collection of distinct property names - Usage: \"distinct(\"lastName\") or distinct(['firstName', 'lastName'])\"\r\n                                             List <String> avgs, //Returns the average value of the given property\r\n                                             List <String> counts, //Returns the count of the given property name\r\n                                             List <String> countDistincts,//Returns the distinct count of the given property name\r\n                                             String groupPropertyStr,//Groups the results by the given property\r\n                                             List <String> maxs, //Returns the maximum value of the given property\r\n                                             List <String> mins, //Returns the minimum value of the given property\r\n                                             List <String> sums, //Returns the sum of the given property\r\n                                             String rowCount //Returns count of the number of rows returned\r\n    ){}";

            SemicolonInserter = new SemicolonInserter(code);
            string result = SemicolonInserter.Execute();

            Console.WriteLine("\n\nRESULT: \n\n" + result);

            Assert.AreEqual("static private String createWhereProjections(CarLogFilterObjectService carLogFilter, //Filter for the car records\r\n                                             List <String> distincts, //Returns results using a single or collection of distinct property names - Usage: \"distinct(\"lastName\") or distinct(['firstName', 'lastName'])\"\r\n                                             List <String> avgs, //Returns the average value of the given property\r\n                                             List <String> counts, //Returns the count of the given property name\r\n                                             List <String> countDistincts,//Returns the distinct count of the given property name\r\n                                             String groupPropertyStr,//Groups the results by the given property\r\n                                             List <String> maxs, //Returns the maximum value of the given property\r\n                                             List <String> mins, //Returns the minimum value of the given property\r\n                                             List <String> sums, //Returns the sum of the given property\r\n                                             String rowCount //Returns count of the number of rows returned\r\n    ){}", result);
        }
    }
}
