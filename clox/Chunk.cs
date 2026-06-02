public enum OpCode{
    OP_CONSTANT,
    OP_NIL, OP_TRUE, OP_FALSE,
    OP_EQUAL, OP_GREATER, OP_LESS,
    OP_ADD, OP_SUBTRACT, OP_MULTIPLY, OP_DIVIDE,
    OP_NOT,
    OP_NEGATE,
    OP_RETURN,
}

public class Chunk{
    public int Count;
    public int Capacity;
    public byte[] Code;
    public int[] Lines;
    public ValueArray Constants;

    public Chunk(int Count, int Capacity, byte[] Code){
        this.Capacity = Capacity;
        this.Count = Count;
        this.Code = Code;   
    }

    public Chunk(){}

    public static void InitChunk(Chunk chunk){
        chunk.Capacity = 0;
        chunk.Count = 0;
        chunk.Code = null;
        chunk.Lines = null;
        chunk.Constants = new ValueArray();
        ValueArray.InitValueArray(chunk.Constants);
    }

    public static void WriteChunk(Chunk chunk, byte value, int Line){
        if (chunk.Capacity < chunk.Count + 1){
            int OldCapacity = chunk.Capacity;
            chunk.Capacity = Memory.GrowCapacity(OldCapacity);
            chunk.Code = Memory.GrowArray<byte>(chunk.Code, OldCapacity, chunk.Capacity);
            chunk.Lines = Memory.GrowArray<int>(chunk.Lines,OldCapacity,chunk.Capacity);
        }
        chunk.Code[chunk.Count] = value;
        chunk.Lines[chunk.Count] = Line;
        chunk.Count++;
    }

    public static int AddConstant(Chunk chunk, Value value){
        ValueArray.WriteValueArray(chunk.Constants,value);
        return chunk.Constants.Count - 1;
    }

    public static void FreeChunk(Chunk chunk){
        Memory.FreeArray(ref chunk.Code);
        Memory.FreeArray(ref chunk.Lines);
        ValueArray.FreeValueArray(chunk.Constants);
        InitChunk(chunk);
    }
}