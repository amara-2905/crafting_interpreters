#define DEBUG_TRACE_EXECUTION

public class VM{
    const int STACK_MAX = 256;
    public Chunk chunk;
    public int ip;
    public Value[] stack = new Value[STACK_MAX];
    public int stackTop = 0;
}

public enum InterpretResult{
    INTERPRET_OK,
    INTERPRET_COMPILE_ERROR,
    INTERPRET_RUNTIME_ERROR
}

public class VirtualMachine{
    public static VM vm;

    public static void InitVM(){
        vm = new VM();
        vm.stack = new Value[256]; 
        ResetStack();
    }

    public static void ResetStack(){
        vm.stackTop = 0;
    }


    public static void FreeVM(){
        
    }

    public static void Push(Value value){
        vm.stack[vm.stackTop] = value;
        vm.stackTop++;
    }

    public static Value Pop(){
        vm.stackTop--;
        return vm.stack[vm.stackTop];
    }

    public static void BinaryAdd(){
        double b = Pop().AsNumber;
        double a = Pop().AsNumber;
        Push(Value.NumberVal(a + b));
    }

    public static void BinarySubtract(){
        double b = Pop().AsNumber;
        double a = Pop().AsNumber;
        Push(Value.NumberVal(a - b));
    }

    public static void BinaryMultiply(){
        double b = Pop().AsNumber;
        double a = Pop().AsNumber;
        Push(Value.NumberVal(a * b));
    }

    public static void BinaryDivide(){
        double b = Pop().AsNumber;
        double a = Pop().AsNumber;
        Push(Value.NumberVal(a / b));
    }


    public static InterpretResult Run(){
        for (;;){
            #if DEBUG_TRACE_EXECUTION
                Console.Write("          ");
                for (int i = 0; i < vm.stackTop; i++)
                {
                    Console.Write("[ ");
                    ValueArray.PrintValue(vm.stack[i]);
                    Console.Write(" ]");
                }
                Console.WriteLine();
                Debug.DisassembleInstruction(vm.chunk, vm.ip);
            #endif
            byte instruction = vm.chunk.Code![vm.ip++];
            switch ((OpCode)instruction){
                case OpCode.OP_CONSTANT:
                byte constantIndex = vm.chunk.Code[vm.ip++];
                Value Constant = vm.chunk.Constants.Values[constantIndex];
                    Push(Constant);
                    break;
                case OpCode.OP_NIL:
                    Push(Value.NilVal()); break;
                case OpCode.OP_TRUE:
                    Push(Value.BoolVal(true)); break;
                case OpCode.OP_FALSE:
                    Push(Value.BoolVal(false)); break;
                case OpCode.OP_NOT:
                    Push(Value.BoolVal(IsFalsey(Pop()))); break;
                case OpCode.OP_EQUAL:
                    Value a = Pop();
                    Value b = Pop();
                    Push(Value.BoolVal(Value.ValuesEqual(a,b))); break;
                case OpCode.OP_GREATER:
                    if (!Peek(0).IsNumber || !Peek(1).IsNumber){
                        RuntimeError("Operands must be numbers.");
                        return InterpretResult.INTERPRET_RUNTIME_ERROR;
                    }
                    double greaterB = Pop().AsNumber;
                    double greaterA = Pop().AsNumber;
                    Push(Value.BoolVal(greaterA > greaterB));
                    break;

                case OpCode.OP_LESS:
                    if (!Peek(0).IsNumber || !Peek(1).IsNumber){
                        RuntimeError("Operands must be numbers.");
                        return InterpretResult.INTERPRET_RUNTIME_ERROR;
                    }
                    double lessB = Pop().AsNumber;
                    double lessA = Pop().AsNumber;
                    Push(Value.BoolVal(lessA < lessB));
                    break;
                case OpCode.OP_RETURN:
                    ValueArray.PrintValue(Pop());
                    Console.WriteLine();
                    return InterpretResult.INTERPRET_OK;
                case OpCode.OP_NEGATE:
                    if (!Peek(0).IsNumber){
                        RuntimeError("Operand must be a number.");
                        return InterpretResult.INTERPRET_RUNTIME_ERROR;
                    }
                    Push(Value.NumberVal(-Pop().AsNumber));
                    break;
                case OpCode.OP_ADD:
                    BinaryAdd();
                    break;
                case OpCode.OP_SUBTRACT:
                    BinarySubtract();
                    break;
                case OpCode.OP_MULTIPLY:
                    BinaryMultiply();
                    break;
                case OpCode.OP_DIVIDE:
                    BinaryDivide();
                    break;
            }
        }
    }

    public static InterpretResult Interpret(string source){
        Chunk chunk = new Chunk();
        Chunk.InitChunk(chunk);
        if (!Compiler.Compile(source, chunk)){
            Chunk.FreeChunk(chunk);
            return InterpretResult.INTERPRET_COMPILE_ERROR;
        }
        vm.chunk = chunk;
        vm.ip = 0;
        InterpretResult result = Run();
        Chunk.FreeChunk(chunk);
        return result;
    }
    public static Value Peek(int distance){
        return vm.stack[vm.stackTop - 1 - distance];
    }

    public static bool IsFalsey(Value value)
    {
        return value.IsNil || (value.IsBool && !value.AsBool);
    }

    public static void RuntimeError(string message){
        Console.Error.WriteLine(message);
        int instruction = vm.ip - 1;
        int line = vm.chunk.Lines[instruction];
        Console.Error.WriteLine($"[line {line}] in script");
        ResetStack();
    }
}