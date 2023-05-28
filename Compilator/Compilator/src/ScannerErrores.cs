using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Antlr4.Runtime;
using SyntacticAnalysisGenerated;

namespace Compilator;

/// <summary>
/// Clase que implementa la interfaz IAntlrErrorListener para manejar los errores del escáner.
/// </summary>
public class ScannerErrorListener : IAntlrErrorListener<int>
{
    
    public LinkedList<string> mensajesError;

    /// <summary>
    /// Constructor de la clase ScannerErrorListener.
    /// </summary>
    public ScannerErrorListener ( )
    {
        mensajesError = new LinkedList<string>();
    }
    
    /// <summary>
    /// Método que se llama cuando se encuentra un error de sintaxis.
    /// </summary>
    /// <param name="output">Escritor de texto para la salida.</param>
    /// <param name="recognizer">Reconocedor del error.</param>
    /// <param name="offendingSymbol">Símbolo ofensivo.</param>
    /// <param name="line">Número de línea donde ocurrió el error.</param>
    /// <param name="charPositionInLine">Posición del carácter en la línea donde ocurrió el error.</param>
    /// <param name="msg">Mensaje de error.</param>
    /// <param name="e">Excepción asociada al error.</param>
    public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine,
        string msg, RecognitionException e)
    {
        if (recognizer.GetType() == typeof(MiniCSharpScanner))
        {
            mensajesError.AddFirst("Error en el escaner en la linea "+line+":"+charPositionInLine + " " + "Mensaje de error: "+ msg);
        }
        else
        {
            mensajesError.AddFirst("Otro error");
        }
    }
    
    /// <summary>
    /// Método que verifica si hay errores en la lista de mensajes.
    /// </summary>
    /// <returns>True si hay errores, False en caso contrario.</returns>
    public bool HasErrors ( )
    {
        return mensajesError.Count > 0;
    }

    /// <summary>
    /// Sobrescribe el método ToString para obtener una representación en cadena de los mensajes de error.
    /// </summary>
    /// <returns>Cadena de texto que representa los mensajes de error.</returns>
    public override string ToString()
    {
        if ( !HasErrors() ) return "0 errores";
        StringBuilder builder = new StringBuilder();
        foreach (var error in mensajesError)
        {
            Console.WriteLine(error);
            
            builder.Append(error + "\n");
        }
        return builder.ToString();
    }
}