public static class Debug{   
    public static void DisassembleChunk(Chunk chunk, string name){
        Console.WriteLine("== " + name + " ==");
        for (int offset = 0; offset < chunk.Count;){
            offset = DisassembleInstruction(chunk,offset);
        }
    }

    public static int ConstantInstruction(string name, Chunk chunk,int offset){
        byte constant = chunk.Code![offset + 1];
        Console.Write($"{name,-16} {constant,4} '");
        ValueArray.PrintValue(chunk.Constants.Values![constant]);
        Console.WriteLine("'");
        return offset + 2;
    }

    public static int DisassembleInstruction(Chunk chunk, int offset){
        Console.Write(offset.ToString("D4") + " ");
        if (offset > 0 && chunk.Lines[offset] == chunk.Lines[offset - 1]){
            Console.Write("   | ");
        }
        else{
            Console.Write($"{chunk.Lines![offset],4} ");
        }
        byte instruction = chunk.Code![offset];
        switch ((OpCode)instruction){
            case OpCode.OP_RETURN:
                return SimpleInstruction("OP_RETURN", offset);
            case OpCode.OP_CONSTANT:
                return ConstantInstruction("OP_CONSTANT",chunk,offset);
            case OpCode.OP_NEGATE:
                return SimpleInstruction("OP_NEGATE", offset);
            case OpCode.OP_ADD:
                return SimpleInstruction("OP_ADD", offset);
            case OpCode.OP_SUBTRACT:
                return SimpleInstruction("OP_SUBTRACT", offset);
            case OpCode.OP_MULTIPLY:
                return SimpleInstruction("OP_MULTIPLY", offset);
            case OpCode.OP_DIVIDE:
                return SimpleInstruction("OP_DIVIDE", offset);
            case OpCode.OP_NIL:
                return SimpleInstruction("OP_NIL", offset);
            case OpCode.OP_TRUE:
                return SimpleInstruction("OP_TRUE", offset);
            case OpCode.OP_FALSE:
                return SimpleInstruction("OP_FALSE", offset);
            case OpCode.OP_NOT:
                return SimpleInstruction("OP_NOT",offset);
            case OpCode.OP_EQUAL:
                return SimpleInstruction("OP_EQUAL", offset);
            case OpCode.OP_GREATER:
                return SimpleInstruction("OP_GREATER", offset);
            case OpCode.OP_LESS:
                return SimpleInstruction("OP_LESS",offset);
            default:
                Console.WriteLine("Unknown opcode " + instruction);
                return offset + 1;
        }
    }
    
    public static int SimpleInstruction(string name, int offset){
        Console.WriteLine(name);
        return offset + 1;
    }

}

public static class DebugSettings
{
    public const bool DEBUG_PRINT_CODE = true;
    public const bool DEBUG_TRACE_EXECUTION = true;
}