public enum ValueType{
    VAL_BOOL,
    VAL_NIL,
    VAL_NUMBER,
    VAL_OBJ,
}

public struct Value{
    public ValueType Type;

    public bool Boolean;
    public double Number;
    public Obj obj;
    public bool IsBool => Type == ValueType.VAL_BOOL;
    public bool IsObj => Type == ValueType.VAL_OBJ;
    public bool IsNil => Type == ValueType.VAL_NIL;
    public bool IsNumber => Type == ValueType.VAL_NUMBER;
    public bool AsBool => Boolean;
    public double AsNumber => Number;
    public Obj AsObj => obj;
    public static Value BoolVal(bool value){
        return new Value{
            Type = ValueType.VAL_BOOL,
            Boolean = value
        };
    }

    public static Value NumberVal(double value){
        return new Value{
            Type = ValueType.VAL_NUMBER,
            Number = value
        };
    }

    public static Value NilVal(){
        return new Value{
            Type = ValueType.VAL_NIL
        };
    }

    public static Value ObjVal(){
        return new Value{
            Type = ValueType.VAL_OBJ
        };
    }

    public static bool ValuesEqual(Value a, Value b)
    {
        if (a.Type != b.Type) return false;
        switch (a.Type)
        {
            case ValueType.VAL_BOOL: return a.AsBool == b.AsBool;
            case ValueType.VAL_NIL: return true;
            case ValueType.VAL_NUMBER: return a.AsNumber == b.AsNumber;
            default: return false;
        }
    }
}

public class ValueArray{
    public int Capacity;
    public int Count;
    public Value[] Values;

    public static void InitValueArray(ValueArray array){
        array.Values = Array.Empty<Value>();
        array.Capacity = 0;
        array.Count = 0;
    }

    public static void WriteValueArray(ValueArray array, Value value){
        if (array.Capacity < array.Count + 1){
            int oldCapacity = array.Capacity;
            array.Capacity = Memory.GrowCapacity(oldCapacity);
            Array.Resize(ref array.Values, array.Capacity);
        }
        array.Values[array.Count] = value;
        array.Count++;
    }

    public static void FreeValueArray(ValueArray array){
        array.Values = Array.Empty<Value>();
        array.Capacity = 0;
        array.Count = 0;
    }

    public static void PrintValue(Value value){
        switch (value.Type){
            case ValueType.VAL_BOOL:
                Console.Write(value.Boolean ? "true" : "false");
                break;
            case ValueType.VAL_NIL:
                Console.Write("nil");
                break;
            case ValueType.VAL_NUMBER:
                Console.Write(value.Number);
                break;
        }
    }
}