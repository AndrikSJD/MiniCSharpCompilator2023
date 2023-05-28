using System;
using Antlr4.Runtime;

namespace Compilator;

/// <summary>
/// Clase abstracta base para representar tipos en la estructura del programa.
/// </summary>
public  abstract class TypeData
{

    private IToken _token;
    private int level;
    private ParserRuleContext _context;

    /// <summary>
    /// Constructor de la clase Type.
    /// </summary>
    /// <param name="token">Token asociado al tipo.</param>
    /// <param name="level">Nivel de ámbito del tipo.</param>
    protected TypeData(IToken token, int level, ParserRuleContext context)
    {
        this._token = token;
        this.level = level;
        this._context = context;
    }

    /// <summary>
    /// Obtiene el token asociado al tipo.
    /// </summary>
    /// <returns>Token asociado al tipo.</returns>
    public IToken GetToken()
    {
        return _token;
    }

    /// <summary>
    /// Obtiene o establece el nivel de ámbito del tipo.
    /// </summary>
    public int Level
    {
        get => level;
        set => level = value;
    }

    /// <summary>
    /// Método abstracto para obtener el tipo de estructura.
    /// </summary>
    /// <returns>Tipo de estructura.</returns>
    public abstract string GetStructureType();


    public ParserRuleContext ContextGetSet
    {
        get => _context;
        set => _context = value ?? throw new ArgumentNullException(nameof(value));
    }
}