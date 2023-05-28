using System.Collections.Generic;
using Antlr4.Runtime;

namespace Compilator;

/// <summary>
/// Clase que representa un tipo de clase.
/// </summary>
public class ClassTypeData: TypeData
{
    public readonly string Type = "class";
    public  LinkedList<TypeData> parametersL = new LinkedList<TypeData>();
    
    /// <summary>
    /// Constructor de la clase ClassType.
    /// </summary>
    /// <param name="type">Token asociado al tipo de clase.</param>
    /// <param name="lvl">Nivel de anidamiento.</param>
    public ClassTypeData(IToken type, int lvl, ParserRuleContext contxt) : base(type, lvl, contxt)// constructor del padre
    {
        
    }

    /// <summary>
    /// Imprime los detalles del tipo de clase en la salida de depuración.
    /// </summary>
    public string PrintClass(IToken s, int level, string type)
    {
        string classPrint = "";
        
        classPrint += $"---Token: {s.Text}\n";
        classPrint += $" - Tipo de dato: {type}\n";
        classPrint += $" - Nivel de scope: {level}\n";
        return classPrint;

    }
    
    /// <summary>
    /// Busca un atributo dentro de la lista de parámetros de la clase.
    /// </summary>
    /// <param name="name">Nombre del atributo a buscar.</param>
    /// <returns>True si se encuentra el atributo, False en caso contrario.</returns>
    public bool BuscarAtributo(string name)
    {
        foreach (var attribute in parametersL)
        {
            if (attribute.GetToken().Text.Equals(name))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Retorna el tipo de estructura, que en este caso es el nombre de la clase.
    /// </summary>
    public override string GetStructureType()
    {
        return this.GetToken().Text;
    }
}