using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Antlr4.Runtime;

namespace TYTCapstone;

public static class GrammarPredicates
{
    private static readonly HashSet<int> KW_SET = new HashSet<int>
    {
        GroovyParser.KW_ABSTRACT, GroovyParser.KW_AS, GroovyParser.KW_ASSERT,
        GroovyParser.KW_BREAK, GroovyParser.KW_CASE, GroovyParser.KW_CATCH,
        GroovyParser.KW_CLASS, GroovyParser.KW_CONST, GroovyParser.KW_CONTINUE,
        GroovyParser.KW_DEF, GroovyParser.KW_DEFAULT, GroovyParser.KW_DO,
        GroovyParser.KW_ELSE, GroovyParser.KW_ENUM, GroovyParser.KW_EXTENDS,
        GroovyParser.KW_FALSE, GroovyParser.KW_FINAL, GroovyParser.KW_FINALLY,
        GroovyParser.KW_FOR, GroovyParser.KW_GOTO, GroovyParser.KW_IF,
        GroovyParser.KW_IMPLEMENTS, GroovyParser.KW_IMPORT, GroovyParser.KW_IN,
        GroovyParser.KW_INSTANCEOF, GroovyParser.KW_INTERFACE, GroovyParser.KW_NATIVE,
        GroovyParser.KW_NEW, GroovyParser.KW_NULL, GroovyParser.KW_PACKAGE,
        GroovyParser.KW_RETURN, GroovyParser.KW_STATIC, GroovyParser.KW_STRICTFP,
        GroovyParser.KW_SUPER, GroovyParser.KW_SWITCH, GroovyParser.KW_SYNCHRONIZED,
        GroovyParser.KW_THIS, GroovyParser.KW_THREADSAFE, GroovyParser.KW_THROW,
        GroovyParser.KW_THROWS, GroovyParser.KW_TRANSIENT, GroovyParser.KW_TRAIT,
        GroovyParser.KW_TRUE, GroovyParser.KW_TRY, GroovyParser.KW_VOLATILE,
        GroovyParser.KW_WHILE, GroovyParser.BUILT_IN_TYPE, GroovyParser.VISIBILITY_MODIFIER
    };

    public static bool isInvalidMethodDeclaration(ITokenStream tokenStream)
    {
        int tokenType = tokenStream.LT(1).Type;

        return (tokenType == GroovyParser.IDENTIFIER || tokenType == GroovyParser.STRING)
            && (tokenStream.LT(2).Type == GroovyParser.LPAREN);
    }

    public static bool isClassName(ITokenStream nameOrPath)
    {
        return isClassName(nameOrPath, 1);
    }

    public static bool isClassName(ITokenStream nameOrPath, int nextPosition)
    {
        int index = nextPosition;
        IToken token = nameOrPath.LT(index);

        while (nameOrPath.LT(index + 1).Type == GroovyParser.DOT)
        {
            index += 2;
            token = nameOrPath.LT(index);
        }

        return token.Type == GroovyParser.BUILT_IN_TYPE
            || token.Type == GroovyParser.KW_CLASS
            || char.IsUpper(token.Text, 0);
    }

    public static bool isKeyword(ITokenStream tokenStream, params int[] excludedKeywords)
    {
        int tokenType = tokenStream.LT(1).Type;

        foreach (int kw in excludedKeywords)
        {
            if (tokenType == kw)
            {
                return false;
            }
        }

        return KW_SET.Contains(tokenType);
    }

    public static bool isCurrentClassName(ITokenStream tokenStream, string currentClassName)
    {
        int index = tokenStream.LT(1).Type == GroovyParser.VISIBILITY_MODIFIER ? 2 : 1;
        return tokenStream.LT(index).Text.Equals(currentClassName);
    }

    public static bool isFollowedByJavaLetterInGString(ICharStream cs)
    {
        int c1 = cs.LA(1);
        if (c1 == IntStreamConstants.EOF)
            return false;
        char ch1 = (char)c1;

        if (Regex.IsMatch(ch1.ToString(), "[a-zA-Z_{]"))
        {
            return true;
        }

        if (Regex.IsMatch(ch1.ToString(), @"[^\u0000-\u007F\uD800-\uDBFF]")
            && isJavaIdentifierPart(c1))
        {
            return true;
        }

        int c2 = cs.LA(2);
        if (c2 == IntStreamConstants.EOF)
            return false;
        char ch2 = (char)c2;

        if (Regex.IsMatch(ch1.ToString(), @"[\uD800-\uDBFF]")
            && Regex.IsMatch(ch2.ToString(), @"[\uDC00-\uDFFF]"))
        {
            int codePoint = char.ConvertToUtf32(ch1, ch2);
            if (isJavaIdentifierPart(codePoint))
            {
                return true;
            }
        }

        return false;
    }

    private static bool isJavaIdentifierPart(int codePoint)
    {
        UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(codePoint);
        switch (category)
        {
            case UnicodeCategory.UppercaseLetter:
            case UnicodeCategory.LowercaseLetter:
            case UnicodeCategory.TitlecaseLetter:
            case UnicodeCategory.ModifierLetter:
            case UnicodeCategory.OtherLetter:
            case UnicodeCategory.LetterNumber:
            case UnicodeCategory.NonSpacingMark:
            case UnicodeCategory.SpacingCombiningMark:
            case UnicodeCategory.DecimalDigitNumber:
            case UnicodeCategory.ConnectorPunctuation:
                return true;
            default:
                return false;
        }
    }

    public static bool isFollowedByLPAREN(ITokenStream tokenStream)
    {
        int index = 1;
        IToken token = tokenStream.LT(index);
        int tokenType = token.Type;

        if (tokenType == GroovyParser.GSTRING_START) // GString
        {
            index = consumeTokenPair(tokenStream, index, GroovyParser.GSTRING_START, GroovyParser.GSTRING_END);
        }
        else if (tokenType == GroovyParser.LCURVE) // Closure
        {
            index = consumeTokenPair(tokenStream, index, GroovyParser.LCURVE, GroovyParser.RCURVE);
        }
        else if (tokenType == GroovyParser.LPAREN) // Parentheses expression
        {
            index = consumeTokenPair(tokenStream, index, GroovyParser.LPAREN, GroovyParser.RPAREN);
        }

        if (index == -1) // EOF reached
        {
            return false;
        }

        // Ignore newlines
        do
        {
            token = tokenStream.LT(++index);
            tokenType = token.Type;
        } while (tokenType == GroovyParser.NL);

        return tokenType == GroovyParser.LPAREN;
    }

    private static int consumeTokenPair(ITokenStream tokenStream, int index, int beginTokenType, int endTokenType)
    {
        int tokenCnt = 1;

        IToken token;
        int tokenType;

        do
        {
            token = tokenStream.LT(++index);
            tokenType = token.Type;

            if (tokenType == TokenConstants.EOF)
            {
                return -1;
            }
            else if (tokenType == beginTokenType)
            {
                tokenCnt++;
            }
            else if (tokenType == endTokenType)
            {
                tokenCnt--;
            }
        } while (tokenCnt != 0);

        return index;
    }
}
