public class Obj{
    public ObjType Type;

    public Obj(ObjType type){
        Type = type;
    }
}

public enum ObjType{
    OBJ_STRING,
}

public class ObjString : Obj{
    public int Length;
    public string Chars;

    public ObjString(string chars) : base(ObjType.OBJ_STRING){
        Chars = chars;
        Length = chars.Length;
    }
}

public static class ObjectMethods{
    public static bool IsObjType(Value value, ObjType type){
        return value.IsObj && value.AsObj.Type == type;
    }

    public static bool IsString(Value value){
        return IsObjType(value, ObjType.OBJ_STRING);
    }

    public static ObjString AsString(Value value){
        return (ObjString)value.AsObj;
    }

    public static string AsCString(Value value){
        return ((ObjString)value.AsObj).Chars;
    }
}