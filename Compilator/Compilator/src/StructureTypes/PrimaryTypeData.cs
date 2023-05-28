using Antlr4.Runtime;

namespace Compilator;

/// <summary>
/// Clase que representa un tipo de datos primitivo.
/// </summary>
public class PrimaryTypeData: TypeData
{
    public enum PrimaryTypes
    {
        Int,
        Char,
        Boolean,
        Double,
        String,
        Unknown,
    }

    private PrimaryTypes Type;

    /// <summary>
    /// Constructor de la clase PrimaryType.
    /// </summary>
    /// <param name="t">Token asociado al tipo de datos primitivo.</param>
    /// <param name="pt">Tipo de datos primitivo.</param>
    /// <param name="lvl">Nivel de ámbito.</param>
    public PrimaryTypeData(IToken t, PrimaryTypes pt, int lvl, ParserRuleContext contxt) : base(t, lvl, contxt)
    {
        Type = pt;
    }

    /// <summary>
    /// Método estático para obtener el tipo de datos primitivo a partir de una cadena.
    /// </summary>
    /// <param name="type">Cadena que representa el tipo de datos.</param>
    /// <returns>Tipo de datos primitivo correspondiente.</returns>
    public static PrimaryTypes showType(string type)
    {
        return type switch
        {
            "int" => PrimaryTypes.Int,
            "double" => PrimaryTypes.Double,
            "char" => PrimaryTypes.Char,
            "boolean" => PrimaryTypes.Boolean,
            "string" => PrimaryTypes.String,
            _ => PrimaryTypes.Unknown,
        };
    }

    /// <summary>
    /// Propiedad para obtener o establecer el tipo de datos primitivo.
    /// </summary>
    public PrimaryTypes TypeGetSet
    {
        get => Type;
        set => Type = value;
    }

    /// <summary>
    /// Imprime los detalles del tipo de datos primitivo en la salida de depuración.
    /// </summary>
    /// <param name="s">Token asociado al tipo de datos.</param>
    /// <param name="level">Nivel de ámbito.</param>
    /// <param name="type">Tipo de datos primitivo.</param>
    public string PrintPrimaryType(IToken s, int level, PrimaryTypes type)
    {
        string primaryTypePrint = "";
        primaryTypePrint += $"---Token: {s.Text}\n";
        primaryTypePrint += $" - Nivel: {level}\n";
        primaryTypePrint += $" - Tipo de dato: {type}\n";
        return primaryTypePrint;

    }

    /// <summary>
    /// Retorna el tipo de estructura, que en este caso es el tipo de datos primitivo.
    /// </summary>
    /// <returns>Tipo de datos primitivo como cadena.</returns>
    public override string GetStructureType()
    {
        return this.Type.ToString();
    }
}