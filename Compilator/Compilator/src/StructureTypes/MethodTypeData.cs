using System;
using System.Collections.Generic;
using Antlr4.Runtime;

namespace Compilator;

/// <summary>
/// Clase que representa un tipo de datos método.
/// </summary>
public class MethodTypeData : TypeData
{
    private readonly string Type = "method";
    private int ParamsNum;
    private string returnType;
    
    
   
    public LinkedList<TypeData> parametersL;
    
    /// <summary>
    /// Constructor de la clase MethodType.
    /// </summary>
    /// <param name="tok">Token asociado al tipo de datos método.</param>
    /// <param name="level">Nivel de ámbito.</param>
    /// <param name="parN">Número de parámetros del método.</param>
    /// <param name="rt">Tipo de retorno del método.</param>
    /// <param name="parsList">Lista enlazada de parámetros del método.</param>
    public MethodTypeData(IToken tok, int level, int parN, string rt, LinkedList<TypeData> parsList,ParserRuleContext contxt) : base(tok, level, contxt)
    {
        this.ParamsNum = parN;
        this.returnType = rt;
        this.parametersL = parsList;
    }
    
    /// <summary>
    /// Imprime los detalles del tipo de datos método en la salida de depuración.
    /// </summary>
    public string PrintMethod()
    {
        string methodPrint = "";
        methodPrint+= "---Tipo de metodo "+ this.GetToken().Text +"\n";
        methodPrint+= " - Nivel: " + Level + "\n";
        methodPrint+= " - Tipo de retorno: " + returnType + "\n";
        methodPrint+= " - Numero de parametros: " + ParamsNum + "\n";
        if(parametersL.Count == 0)
            methodPrint+= " - Parametros: 0" + "\n";
        else
            methodPrint+= " - Parametros: " + "\n";
        
        foreach (var parameter in parametersL)
        {
            if (parameter is ClassVarTypeData classVarType)
            {
                 methodPrint+= $"     Token: {classVarType.GetToken().Text}\n";
                 methodPrint+=$"      - Tipo: {classVarType.Type}\n";
                 methodPrint+=$"      - Tipo de {classVarType.classType}\n";
                 methodPrint+=$"      - Nivel: {classVarType.Level}\n";
            }
            
            if (parameter is PrimaryTypeData primaryType)
            {
                methodPrint+=$"     Token: {primaryType.GetToken().Text}\n";
                methodPrint+=$"     - Tipo: {primaryType.TypeGetSet}\n";
                methodPrint+=$"     - Nivel: {primaryType.Level}\n";

            }

            if (parameter is ArrayTypeData arrType)
            {
                methodPrint+=$"     Token: {arrType.GetToken().Text}\n";
                methodPrint+=$"     - Tipo: {arrType.Type}\n";
                methodPrint+=$"     - Tipo de array: {arrType.GetSetArrType}\n";
                methodPrint+=$"     - Nivel: {arrType.Level}\n";
            }
            
            
        }
        methodPrint+="\n";

        return methodPrint;

    }

    /// <summary>
    /// Propiedad para obtener o establecer el tipo de retorno del método.
    /// </summary>
    public string ReturnTypeGetSet
    {
        get => returnType;
        set => returnType = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Retorna el tipo de estructura, que en este caso es el tipo de retorno del método.
    /// </summary>
    public override string GetStructureType()
    {
        return this.returnType;
    }
    
}