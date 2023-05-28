using System.IO;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;
using SyntacticAnalysisGenerated;
using Antlr4.Runtime;

namespace Compilator;

/// <summary>
/// Clase que implementa el error listener para el parser.
/// </summary>
public class ParserErrorListener : BaseErrorListener
{
    private ArrayList<string> errorMsgs;

    /// <summary>
    /// Constructor de la clase ParserErrorListener.
    /// </summary>
    public ParserErrorListener()
    {
        this.errorMsgs = new ArrayList<string>();
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
    public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line,
        int charPositionInLine, string msg, RecognitionException e)
    {

        if (recognizer is MiniCSharpParser)
            errorMsgs.Add($"Error en el parser en la linea {line} y  columna {charPositionInLine} {msg}");
        else
            errorMsgs.Add("Error fuera del parser");

    }

    /// <summary>
    /// Método que verifica si hay errores en la lista de mensajes.
    /// </summary>
    /// <returns>True si hay errores, False en caso contrario.</returns>
    public bool HasErrors()
    {
        return errorMsgs.Count > 0;
    }

    /// <summary>
    /// Sobrescribe el método ToString para obtener una representación en cadena de los mensajes de error.
    /// </summary>
    /// <returns>Cadena de texto que representa los mensajes de error.</returns>
    public override string ToString()
    {
        if (!HasErrors()) return "No hay errores";
        var builder = new System.Text.StringBuilder();
        foreach (string s in errorMsgs)
        {
            builder.AppendLine(s);
        }

        return builder.ToString();
    }
    public override void ReportAmbiguity(Parser recognizer, DFA dfa, int startIndex, int stopIndex, bool exact, BitSet ambigAlts,
        ATNConfigSet configs)
    {
        base.ReportAmbiguity(recognizer, dfa, startIndex, stopIndex, exact, ambigAlts, configs);
    }

    public override void ReportAttemptingFullContext(Parser recognizer, DFA dfa, int startIndex, int stopIndex, BitSet conflictingAlts,
        SimulatorState conflictState)
    {
        base.ReportAttemptingFullContext(recognizer, dfa, startIndex, stopIndex, conflictingAlts, conflictState);
    }

    public override void ReportContextSensitivity(Parser recognizer, DFA dfa, int startIndex, int stopIndex, int prediction,
        SimulatorState acceptState)
    {
        base.ReportContextSensitivity(recognizer, dfa, startIndex, stopIndex, prediction, acceptState);
    }

}