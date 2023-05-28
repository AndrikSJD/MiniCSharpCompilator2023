using Antlr4.Runtime;

namespace Compilator;

/// <summary>
/// Clase que representa un tipo de arreglo.
/// </summary>
public class ArrayTypeData : TypeData
{
    public readonly string Type = "array";
    public int size = 0;
    
    public enum ArrTypes
    {
        Char,
        Int,
        Unknown,
    }
    

    private ArrTypes arrType;

    /// <summary>
    /// Constructor de la clase ArrayType.
    /// </summary>
    /// <param name="tok">Token asociado al tipo de arreglo.</param>
    /// <param name="lvl">Nivel de anidamiento.</param>
    /// <param name="arr">Tipo base del arreglo.</param>
    public ArrayTypeData(IToken tok, int lvl, ArrTypes arr, ParserRuleContext contxt) : base(tok, lvl, contxt)
    {
        arrType = arr;
    }

    /// <summary>
    /// Devuelve el tipo de arreglo correspondiente a partir de una cadena de texto.
    /// </summary>
    /// <param name="type">Cadena de texto que representa el tipo de arreglo.</param>
    /// <returns>Tipo de arreglo correspondiente.</returns>
    public static ArrTypes showType(string type)
    {
        return type switch
        {
            "char" => ArrTypes.Char,
            "int" => ArrTypes.Int,
            _ => ArrTypes.Unknown,
        };
    }

    /// <summary>
    /// Propiedad que permite obtener o establecer el tipo base del arreglo.
    /// </summary>
    public ArrTypes GetSetArrType
    {
        get => arrType;
        set => arrType = value;
    }
    
    /// <summary>
    /// Imprime los detalles del tipo de arreglo en la salida de depuración.
    /// </summary>
    public string  PrintArrayType(IToken s, int level, string type, ArrTypes baseType)
    {
        string arrayPrint = "";
        arrayPrint += $"---Token: {s.Text}\n";
        arrayPrint += $" - Tipo de dato: {type}\n";
        arrayPrint += $" - Arreglo de: {baseType}\n";
        arrayPrint += $" - Nivel de scope: {level}\n";
        
        return arrayPrint;
    }

    /// <summary>
    /// Retorna el tipo de estructura, que en este caso es el tipo de arreglo.
    /// </summary>
    public override string GetStructureType()
    {
        return this.arrType.ToString();
    }
}