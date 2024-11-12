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

        [TestMethod]
        public void ControlStructureTest2()
        {
            string code = "static boolean IsFieldGroupedDividedBy100(String field){\r\n        boolean rrIsDividedBy100 = AppConfigService.getBoolean('IsRRAndGradeShowingToHundredths', true)\r\n        if (rrIsDividedBy100 &&\r\n                (field.equals(\"throatRrError\") || field.equals(\"masterRrError\") || field.equals(\"interRrError\") || field.equals(\"groupRrError\") || field.equals(\"lastSwRrError\") || field.equals(\"curveRrError\") || field.equals(\"bowlRrError\")||\r\n                        field.equals('arm') || field.equals('ari') || field.equals('arg') || field.equals('arls') || field.equals('arc') || field.equals('arb') || field.equals('ard')||\r\n                        field.equals('art') || field.equals('arm') || field.equals('ari') || field.equals('arg') || field.equals('arls') || field.equals('arc') || field.equals('arb') || field.equals('ard')||\r\n                        field.equals('prt') || field.equals('prm') || field.equals('pri') || field.equals('prg') || field.equals('prls') || field.equals('prc') || field.equals('prb') ||\r\n                        field.equals('gt')  || field.equals('gm') || field.equals('gi') || field.equals('gg') || field.equals('gls') || field.equals('gc') || field.equals('gb') || field.equals('gbm') || field.equals('gbd'))\r\n        ){\r\n            return true\r\n        }else{\r\n            return false\r\n        }\r\n    }";

            SemicolonInserter = new SemicolonInserter(code);
            string result = SemicolonInserter.Execute();

            Console.WriteLine("\n\nRESULT: \n\n" + result);

            Assert.AreEqual("static boolean IsFieldGroupedDividedBy100(String field){\r\n        boolean rrIsDividedBy100 = AppConfigService.getBoolean('IsRRAndGradeShowingToHundredths', true);\r\n        if (rrIsDividedBy100 &&\r\n                (field.equals(\"throatRrError\") || field.equals(\"masterRrError\") || field.equals(\"interRrError\") || field.equals(\"groupRrError\") || field.equals(\"lastSwRrError\") || field.equals(\"curveRrError\") || field.equals(\"bowlRrError\")||\r\n                        field.equals('arm') || field.equals('ari') || field.equals('arg') || field.equals('arls') || field.equals('arc') || field.equals('arb') || field.equals('ard')||\r\n                        field.equals('art') || field.equals('arm') || field.equals('ari') || field.equals('arg') || field.equals('arls') || field.equals('arc') || field.equals('arb') || field.equals('ard')||\r\n                        field.equals('prt') || field.equals('prm') || field.equals('pri') || field.equals('prg') || field.equals('prls') || field.equals('prc') || field.equals('prb') ||\r\n                        field.equals('gt')  || field.equals('gm') || field.equals('gi') || field.equals('gg') || field.equals('gls') || field.equals('gc') || field.equals('gb') || field.equals('gbm') || field.equals('gbd'))\r\n        ){\r\n            return true;\r\n        }else{\r\n            return false;\r\n        }\r\n    }", result);
        }

        [TestMethod]
        public void ControlStructureTest3()
        {
            string code = "static boolean IsFieldDisplayedDividedBy100(String field){\r\n        boolean varDividedBy100\r\n        //boolean rrIsDividedBy100 = AppConfigService.getBoolean('IsRRAndGradeShowingToHundredths', true)\r\n        if ( items_DisplayedDividedBy100().contains(field) ||\r\n                (IsFieldGroupedDividedBy100(field))\r\n        ){\r\n            varDividedBy100 = true;\r\n        }else{\r\n            varDividedBy100 = false\r\n        }\r\n        return varDividedBy100\r\n    }";

            SemicolonInserter = new SemicolonInserter(code);
            string result = SemicolonInserter.Execute();

            Console.WriteLine("\n\nRESULT: \n\n" + result);

            Assert.AreEqual("static boolean IsFieldDisplayedDividedBy100(String field){\r\n        boolean varDividedBy100;\r\n        //boolean rrIsDividedBy100 = AppConfigService.getBoolean('IsRRAndGradeShowingToHundredths', true)\r\n        if ( items_DisplayedDividedBy100().contains(field) ||\r\n                (IsFieldGroupedDividedBy100(field))\r\n        ){\r\n            varDividedBy100 = true;\r\n        }else{\r\n            varDividedBy100 = false;\r\n        }\r\n        return varDividedBy100;\r\n    }", result);
        }

        [TestMethod]
        public void NonBlockIfStatementTest()
        {
            string code = "if (lookForSeqPrev)\r\n                filterNext.seq = [lastCar.seq+1]\r\n            else\r\n                filterNext.seq = [lastCar.seq-1]\r\n            filterNext.orderByActual = [\"logDate\":\"desc\"]\r\n\r\n            List <CarLog> nextCars = returnCarLogList(filterNext)\r\n\r\n            if (nextCars.size()>0)\r\n                return nextCars[0] as CarLog";

            SemicolonInserter = new SemicolonInserter(code);
            string result = SemicolonInserter.Execute();

            Console.WriteLine("\n\nRESULT: \n\n" + result);

            Assert.AreEqual("if (lookForSeqPrev)\r\n                filterNext.seq = [lastCar.seq+1];\r\n            else\r\n                filterNext.seq = [lastCar.seq-1];\r\n            filterNext.orderByActual = [\"logDate\":\"desc\"];\r\n\r\n            List <CarLog> nextCars = returnCarLogList(filterNext);\r\n\r\n            if (nextCars.size()>0)\r\n                return nextCars[0] as CarLog;", result);
        }

        [TestMethod]
        public void NestedClosuresTest2()
        {
            string code = "def multiplier = { x ->\r\n    return { y ->\r\n        x * y\r\n    }\r\n}\r\n\r\ndef double = multiplier(2)\r\ndef triple = multiplier(3)\r\n\r\nprintln double(5)\r\nprintln triple(5)\r\n\r\ntry {\r\n    def result = 10 / 0\r\n} catch (ArithmeticException e) {\r\n    println \"Caught exception: ${e.message}\"\r\n} finally {\r\n    println \"Execution completed\"\r\n}";

            SemicolonInserter = new SemicolonInserter(code);
            string result = SemicolonInserter.Execute();

            Console.WriteLine("\n\nRESULT: \n\n" + result);

            Assert.AreEqual("def multiplier = { x ->\r\n    return { y ->\r\n        x * y;\r\n    };\r\n};\r\n\r\ndef double = multiplier(2);\r\ndef triple = multiplier(3);\r\n\r\nprintln double(5);\r\nprintln triple(5);\r\n\r\ntry {\r\n    def result = 10 / 0;\r\n} catch (ArithmeticException e) {\r\n    println \"Caught exception: ${e.message}\";\r\n} finally {\r\n    println \"Execution completed\";\r\n}", result);
        }

        [TestMethod]
        public void FunctionalProgrammingTest()
        {
            string code = "def numbers = [1, 2, 3, 4, 5, 6, 7, 8, 9]\r\n\r\ndef evenNumbers = numbers.findAll { it % 2 == 0 }\r\n\r\nevenNumbers.each { num ->\r\n    println \"Even number: $num\"\r\n}\r\n\r\ndef sum = numbers.inject(0) { total, element ->\r\n    total + element\r\n}\r\n\r\nprintln \"Sum of numbers: $sum\"\r\n\r\ndef maxNumber = numbers.max()\r\nprintln \"Maximum number is $maxNumber\"\r\n\r\ndef map = [name: 'John Doe', age: 30, email: 'john.doe@example.com']\r\n\r\nmap.each { key, value ->\r\n    println \"$key: $value\"\r\n}\r\n";

            SemicolonInserter = new SemicolonInserter(code);
            string result = SemicolonInserter.Execute();

            Console.WriteLine("\n\nRESULT: \n\n" + result);

            Assert.AreEqual("def numbers = [1, 2, 3, 4, 5, 6, 7, 8, 9];\r\n\r\ndef evenNumbers = numbers.findAll { it % 2 == 0 };\r\n\r\nevenNumbers.each { num ->\r\n    println \"Even number: $num\";\r\n};\r\n\r\ndef sum = numbers.inject(0) { total, element ->\r\n    total + element;\r\n};\r\n\r\nprintln \"Sum of numbers: $sum\";\r\n\r\ndef maxNumber = numbers.max();\r\nprintln \"Maximum number is $maxNumber\";\r\n\r\ndef map = [name: 'John Doe', age: 30, email: 'john.doe@example.com'];\r\n\r\nmap.each { key, value ->\r\n    println \"$key: $value\";\r\n};\r\n", result);
        }
    }
}
