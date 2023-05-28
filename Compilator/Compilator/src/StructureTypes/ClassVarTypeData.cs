using Antlr4.Runtime;

namespace Compilator;

/// <summary>
/// Clase que representa un tipo de datos de clase.
/// </summary>
public class ClassVarTypeData : TypeData
{
    public readonly string Type = "ClassType";
    public readonly string classType;


    /// <summary>
    /// Constructor de la clase ClassVarType.
    /// </summary>
    /// <param name="tk">Token asociado al tipo de datos de clase.</param>
    /// <param name="lvl">Nivel de ámbito.</param>
    /// <param name="tf">Tipo de clase.</param>
    public ClassVarTypeData(IToken tk, int lvl, string tf, ParserRuleContext contxt) : base(tk, lvl, contxt)
    {
        classType = tf;
        
    }

    /// <summary>
    /// Imprime los detalles del tipo de datos de clase en la salida de depuración.
    /// </summary>
    public string PrintClassVarType(IToken s, int level, string type, string fatherType)
    {
        string classPrint = "";
        classPrint+= "---Token: " + s.Text + "\n";
        classPrint+= " - Nivel: " + level + "\n";
        classPrint+= " - Tipo de dato: " + type + "\n";
        classPrint+= " - Tipo de padre: " + fatherType + "\n";
        return classPrint;
        

    }
    
    /// <summary>
    /// Retorna el tipo de estructura, que en este caso es el tipo de clase.
    /// </summary>
    public override string GetStructureType()
    {
        return this.classType;
    }
}