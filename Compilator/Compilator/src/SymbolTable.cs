using System;
using Antlr4.Runtime;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using SyntacticAnalysisGenerated;

namespace Compilator;

/// <summary>
/// Clase que representa la tabla de símbolos.
/// </summary>
public class SymbolTable
{
    LinkedList<Object>table;
    public int currentLevel;
    public MethodTypeData currentMethod;
    public int currentMethodIndex;
    public ClassTypeData? currentClass;
    

    /// <summary>
    /// Obtiene el nivel actual de anidamiento.
    /// </summary>
    /// <returns>Nivel actual de anidamiento.</returns>
    public int getLevel() {
        return this.currentLevel;
    }
    
    /// <summary>
    /// Constructor de la clase SymbolTable.
    /// </summary>
    public SymbolTable()
    {
        table = new LinkedList<Object>();
        currentLevel = -1;
    }

    /// <summary>
    /// Inserta una estructura de tipo en la tabla de símbolos.
    /// </summary>
    /// <param name="typeDataStruct">Estructura de tipo a insertar.</param>
    public void Insert(TypeData typeDataStruct)
    {

        table.AddLast(typeDataStruct);
        
    }

    /// <summary>
    /// Busca una estructura de tipo por su nombre en la tabla de símbolos.
    /// </summary>
    /// <param name="name">Nombre de la estructura de tipo a buscar.</param>
    /// <returns>Estructura de tipo encontrada o nulo si no se encuentra.</returns>
    public TypeData? Search(string name)
    {
        foreach (TypeData tok in table)
        {
            if (tok.GetToken().Text.Equals(name))
            {
                return tok;
            }
        }
        return null;
    }

    /// <summary>
    /// Busca una variable personalizada por su identificador en la tabla de símbolos.
    /// </summary>
    /// <param name="id">Identificador de la variable personalizada.</param>
    /// <returns>Variable personalizada encontrada o nulo si no se encuentra.</returns>
    public TypeData? SearchClassVariable(string id)
    {
        foreach (TypeData? tok in table)
        {
            if (tok.GetToken().Text.Equals(id) && tok is ClassVarTypeData)
                return tok;
        }
        return null;
    }

    /// <summary>
    /// Busca el índice de un método en la tabla de símbolos.
    /// </summary>
    /// <param name="nombreMetodo">Nombre del método a buscar.</param>
    /// <returns>Índice del método en la tabla o 0 si no se encuentra.</returns>
    public int searchIndex(string nombreMetodo)
    {
        // Busco la posicion del metodo en la tabla
        for (int i = 0; i < table.Count; i++)
        {
            if (((TypeData)table.ElementAt(i)).GetToken().Text.Equals(nombreMetodo))
            {
                return i;
            }
        }

        return 0;
    }

    
    public int getTableSize()
    {
        return table.Count;
    }

    /// <summary>
    /// Busca un parametro en la tabla de simbolos que se haya creado dentro del metodo actual
    /// </summary>
    /// <param name="par"></param>
    /// <returns></returns>
    public TypeData getRepeatedParameter(string par)
    {
        for (int i = table.Count-1; i > currentMethodIndex; i--)
        {
            if (((TypeData)table.ElementAt(i)).GetToken().Text == par)
            {
                return (TypeData)table.ElementAt(i);
            }
        }

        return null;
    }
    
    
    
    
    
    /// <summary>
    /// Elimina los parámetros y el cuerpo de un método de la tabla de símbolos.
    /// </summary>
    /// <param name="nombreMetodo">Nombre del método a eliminar.</param>
    public void DeleteParametersBody(string nombreMetodo)
    {
        int posMethod = searchIndex(nombreMetodo);
        LinkedList<object> slicedList = new LinkedList<object>();

        if (posMethod != 0)
        {
            // Agrego a la lista los elementos que estan antes del metodo
            for (int j = 0; j <= posMethod; j++)
            {
                slicedList.AddLast(table.ElementAt(j));
            }

            table = slicedList;

            return;
        }
        
        System.Diagnostics.Debug.WriteLine("No se encontro el metodo");
    }

    /// <summary>
    /// Abre un nuevo ámbito en la tabla de símbolos.
    /// </summary>
    public void OpenScope()
    {
        currentLevel++;
    }
  

    /// <summary>
    /// Cierra el ámbito actual en la tabla de símbolos.
    /// </summary>
    public void CloseScope()
    {
        table.Remove(new Func<TypeData, bool>(n => n.Level == currentLevel));
        currentLevel--;
    }

    /// <summary>
    /// Imprime el contenido de la tabla de símbolos.
    /// </summary>
    public string Print()
    {
        string printTable = "";
        printTable += "\n--------------- TABLA DE SIMBOLOS ---------------\n";
        for (int i = 0; i < table.Count; i++)
        {
            IToken s = ((TypeData)table.ElementAt(i)).GetToken();
            if(table.ElementAt(i).GetType() == typeof(ClassTypeData))
            {
                printTable += ((ClassTypeData)table.ElementAt(i)).PrintClass(s, ((TypeData)table.ElementAt(i)).Level, ((ClassTypeData)table.ElementAt(i)).Type) + "\n";
            }
            else if(table.ElementAt(i).GetType() == typeof(ClassVarTypeData))
            {
                printTable += ((ClassVarTypeData)table.ElementAt(i)).PrintClassVarType(s, ((TypeData)table.ElementAt(i)).Level, 
                    ((ClassVarTypeData)table.ElementAt(i)).Type, ((ClassVarTypeData)table.ElementAt(i)).Type) + "\n";
            }
            else if(table.ElementAt(i).GetType() == typeof(MethodTypeData))
            {
                printTable += ((MethodTypeData)table.ElementAt(i)).PrintMethod() + "\n";
            }
            else if(table.ElementAt(i).GetType() == typeof(PrimaryTypeData))
            {
                printTable += ((PrimaryTypeData)table.ElementAt(i)).PrintPrimaryType(s, ((TypeData)table.ElementAt(i)).Level, ((PrimaryTypeData)table.ElementAt(i)).TypeGetSet) + "\n";
            }
            else if (table.ElementAt(i).GetType() == typeof(ArrayTypeData))
            {
                printTable += ((ArrayTypeData)table.ElementAt(i)).PrintArrayType(s, ((TypeData)table.ElementAt(i)).Level, 
                    ((ArrayTypeData)table.ElementAt(i)).Type, ((ArrayTypeData)table.ElementAt(i)).GetSetArrType) + "\n";
            }
        }
        printTable += "--------------- FIN DE TABLA ---------------";

        return printTable;
    }
}