public class Compiler{
    public static Parser parser = new Parser();
    public static Chunk CompilingChunk;
    public static Scanner scanner = new Scanner();

    static string Source;
    public static bool Compile(string source, Chunk chunk){
        Source = source;
        CompilingChunk = chunk;
        scanner.InitScanner(source);
        parser.hadError = false;
        parser.panicMode = false;
        Advance();
        expression();
        Consume(TokenType.TOKEN_EOF,"Expect end of expression.");  
        EndCompiler();
        return !parser.hadError; 
    }

    static void expression()
    {
        ParsePrecedence(Precedence.PREC_ASSIGNMENT);
    }

    public static void Advance(){
        parser.Previous = parser.Current;
        for (; ;){
            parser.Current = Scanner.ScanToken();
            if (parser.Current.type != TokenType.TOKEN_ERROR) break;
            ErrorAtCurrent(Source.Substring(parser.Current.Start,parser.Current.Length));
        }
    }

    public static void Consume(TokenType type, string message)
    {
        if (parser.Current.type == type)
        {
            Advance();
            return;
        }
        ErrorAtCurrent(message);
    }

    public static void EmitByte(byte value)
    {
        Chunk.WriteChunk(CurrentChunk(),value,parser.Previous.line);
    }

    static void EndCompiler()
    {
        EmitReturn();
        if (DebugSettings.DEBUG_PRINT_CODE && !parser.hadError)
        {
            Debug.DisassembleChunk(CurrentChunk(), "code");
        }
    }

    static void Binary()
    {
        TokenType operatorType = parser.Previous.type;
        ParseRule rule = GetRule(operatorType);
        ParsePrecedence((Precedence)((int)rule.Precedence + 1));

        switch (operatorType)
        {
            case TokenType.TOKEN_PLUS: EmitByte((byte)OpCode.OP_ADD); break;
            case TokenType.TOKEN_MINUS: EmitByte((byte)OpCode.OP_SUBTRACT); break;
            case TokenType.TOKEN_STAR: EmitByte((byte)OpCode.OP_MULTIPLY); break;
            case TokenType.TOKEN_SLASH: EmitByte((byte)OpCode.OP_DIVIDE); break;
            case TokenType.TOKEN_BANG_EQUAL: EmitBytes((byte)OpCode.OP_EQUAL,(byte)OpCode.OP_NOT); break;
            case TokenType.TOKEN_EQUAL_EQUAL: EmitByte((byte)OpCode.OP_EQUAL); break;
            case TokenType.TOKEN_GREATER: EmitByte((byte)OpCode.OP_GREATER); break;
            case TokenType.TOKEN_GREATER_EQUAL: EmitBytes((byte)OpCode.OP_LESS,(byte)OpCode.OP_NOT); break;
            case TokenType.TOKEN_LESS: EmitByte((byte)OpCode.OP_LESS); break;
            case TokenType.TOKEN_LESS_EQUAL: EmitBytes((byte)OpCode.OP_GREATER,(byte)OpCode.OP_NOT); break;
            default: return;
        }
    }

    public static void Literal()
    {
        switch (parser.Previous.type)
        {
            case TokenType.TOKEN_FALSE: 
                EmitByte((byte)OpCode.OP_FALSE); break;
            case TokenType.TOKEN_NIL: 
                EmitByte((byte)OpCode.OP_NIL); break;
            case TokenType.TOKEN_TRUE: 
                EmitByte((byte)OpCode.OP_TRUE); break;
            default:
                return;
        }
    }

    public static void Grouping()
    {
        expression();
        Consume(TokenType.TOKEN_RIGHT_PAREN,"Expect ')' after expression.");
    }

    static void Number()
    {
        string text = Source.Substring(
            parser.Previous.Start,
            parser.Previous.Length
        );

        double value = double.Parse(text);

        EmitConstant(Value.NumberVal(value));
    }

    static void String(){
        string text = Source.Substring(
            parser.Previous.Start + 1,
            parser.Previous.Length - 2
        );

        EmitConstant(Value.ObjVal(ObjectMethods.CopyString(text)));
    }

    static void Unary()
    {
        TokenType operatorType = parser.Previous.type;
        ParsePrecedence(Precedence.PREC_UNARY);

        switch (operatorType)
        {
            case TokenType.TOKEN_MINUS:
                EmitByte((byte)OpCode.OP_NEGATE); break;
            case TokenType.TOKEN_BANG:
                EmitByte((byte)OpCode.OP_NOT); break;
            default: return;
        }
    }

    public static ParseRule[] rules =
    {
        new ParseRule(Grouping, null, Precedence.PREC_NONE),     // TOKEN_LEFT_PAREN
        new ParseRule(null, null, Precedence.PREC_NONE),         // TOKEN_RIGHT_PAREN
        new ParseRule(null, null, Precedence.PREC_NONE),         // TOKEN_LEFT_BRACE
        new ParseRule(null, null, Precedence.PREC_NONE),         // TOKEN_RIGHT_BRACE
        new ParseRule(null, null, Precedence.PREC_NONE),         // TOKEN_COMMA
        new ParseRule(null, null, Precedence.PREC_NONE),         // TOKEN_DOT
        new ParseRule(Unary, Binary, Precedence.PREC_TERM),      // TOKEN_MINUS
        new ParseRule(null, Binary, Precedence.PREC_TERM),       // TOKEN_PLUS
        new ParseRule(null, null, Precedence.PREC_NONE),         // TOKEN_SEMICOLON
        new ParseRule(null, Binary, Precedence.PREC_FACTOR),     // TOKEN_SLASH
        new ParseRule(null, Binary, Precedence.PREC_FACTOR),     // TOKEN_STAR
        new ParseRule(Unary, null, Precedence.PREC_NONE),        // TOKEN_BANG
        new ParseRule(null, Binary, Precedence.PREC_EQUALITY),   // TOKEN_BANG_EQUAL
        new ParseRule(null, null, Precedence.PREC_NONE),         // TOKEN_EQUAL
        new ParseRule(null, Binary, Precedence.PREC_EQUALITY),   // TOKEN_EQUAL_EQUAL
        new ParseRule(null, Binary, Precedence.PREC_COMPARISON), // TOKEN_GREATER
        new ParseRule(null, Binary, Precedence.PREC_COMPARISON), // TOKEN_GREATER_EQUAL
        new ParseRule(null, Binary, Precedence.PREC_COMPARISON), // TOKEN_LESS
        new ParseRule(null, Binary, Precedence.PREC_COMPARISON), // TOKEN_LESS_EQUAL
        new ParseRule(null, null, Precedence.PREC_NONE),         // TOKEN_IDENTIFIER
        new ParseRule(String, null, Precedence.PREC_NONE),       // TOKEN_STRING
        new ParseRule(Number, null, Precedence.PREC_NONE),       // TOKEN_NUMBER
        new ParseRule(null, null, Precedence.PREC_NONE),         // TOKEN_AND
        new ParseRule(null, null, Precedence.PREC_NONE),         // TOKEN_CLASS
        new ParseRule(null, null, Precedence.PREC_NONE),         // TOKEN_ELSE
        new ParseRule(Literal, null, Precedence.PREC_NONE),      // TOKEN_FALSE
        new ParseRule(null, null, Precedence.PREC_NONE),         // TOKEN_FOR
        new ParseRule(null, null, Precedence.PREC_NONE),         // TOKEN_FUN
        new ParseRule(null, null, Precedence.PREC_NONE),         // TOKEN_IF
        new ParseRule(Literal, null, Precedence.PREC_NONE),      // TOKEN_NIL
        new ParseRule(null, null, Precedence.PREC_NONE),         // TOKEN_OR
        new ParseRule(null, null, Precedence.PREC_NONE),         // TOKEN_PRINT
        new ParseRule(null, null, Precedence.PREC_NONE),         // TOKEN_RETURN
        new ParseRule(null, null, Precedence.PREC_NONE),         // TOKEN_SUPER
        new ParseRule(null, null, Precedence.PREC_NONE),         // TOKEN_THIS
        new ParseRule(Literal, null, Precedence.PREC_NONE),      // TOKEN_TRUE
        new ParseRule(null, null, Precedence.PREC_NONE),         // TOKEN_VAR
        new ParseRule(null, null, Precedence.PREC_NONE),         // TOKEN_WHILE
        new ParseRule(null, null, Precedence.PREC_NONE),         // TOKEN_ERROR
        new ParseRule(null, null, Precedence.PREC_NONE)          // TOKEN_EOF
    };

    static void ParsePrecedence(Precedence precedence)
    {
        Advance();
        ParseFn prefixRule = GetRule(parser.Previous.type).Prefix;
        if (prefixRule == null)
        {
            Console.Error.WriteLine("Expect expression.");
            return;
        }

        prefixRule();
        while ((int)precedence <= (int)GetRule(parser.Current.type).Precedence)
        {
            Advance();
            ParseFn infixRule = GetRule(parser.Previous.type).Infix;
            infixRule();
        }
    }

    static ParseRule GetRule(TokenType type)
    {
        return rules[(int)type];
    }

    static void EmitReturn()
    {
        EmitByte((byte)OpCode.OP_RETURN);
    }

    static void EmitConstant(Value value)
    {
        EmitBytes((byte)OpCode.OP_CONSTANT, MakeConstant(value));
    }

    static byte MakeConstant(Value value)
    {
        int Constant = Chunk.AddConstant(CurrentChunk(), value);
        if (Constant > byte.MaxValue)
        {
            Error("Too many constants in one chunk.");
            return 0;
        }
        return (byte)Constant;
    }

    static void EmitBytes(byte byte1, byte byte2)
    {
        EmitByte(byte1);
        EmitByte(byte2);
    }

    static Chunk CurrentChunk()
    {
        return CompilingChunk;
    }


    public static void ErrorAtCurrent(string message){
        ErrorAt(parser.Current,message,Source);
    }

    public static void Error(string message){
        ErrorAt(parser.Previous,message,Source);
    }

    public static void ErrorAt(Token token, string message, string source)
    {
        if (parser.panicMode) return;
        parser.panicMode = true;
        Console.Error.Write($"[line {token.line}] Error");
        if (token.type == TokenType.TOKEN_EOF)
        {
            Console.Error.Write(" at end");
        }
        else if (token.type == TokenType.TOKEN_ERROR)
        {
            // Nothing.
        }
        else
        {
            Console.Error.Write(
                $" at '{source.Substring(token.Start, token.Length)}'"
            );
        }
        Console.Error.WriteLine($": {message}");
        parser.hadError = true;
    }
}

public class Parser{
    public Token Current;
    public Token Previous;
    internal bool hadError;
    internal bool panicMode;
}

public enum Precedence
{
    PREC_NONE,
    PREC_ASSIGNMENT,  // =
    PREC_OR,          // or
    PREC_AND,         // and
    PREC_EQUALITY,    // == !=
    PREC_COMPARISON,  // < > <= >=
    PREC_TERM,        // + -
    PREC_FACTOR,      // * /
    PREC_UNARY,       // ! -
    PREC_CALL,        // . ()
    PREC_PRIMARY
}

public delegate void ParseFn();

public class ParseRule
{
    public ParseFn Prefix;
    public ParseFn Infix;
    public Precedence Precedence;

    public ParseRule(ParseFn prefix, ParseFn infix, Precedence precedence)
    {
        Prefix = prefix;
        Infix = infix;
        Precedence = precedence;
    }
}

