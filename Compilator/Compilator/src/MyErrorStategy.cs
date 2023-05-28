using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using System;
using Antlr4.Runtime;

namespace Compilator;

/// <summary>
  /// This is the default implementation of
  /// <see cref="T:Antlr4.Runtime.IAntlrErrorStrategy" />
  /// used for
  /// error reporting and recovery in ANTLR parsers.
  /// </summary>
  public class MyErrorStrategy : DefaultErrorStrategy
  {
    /// <summary>
    /// Indicates whether the error strategy is currently "recovering from an
    /// error".
    /// </summary>
    /// <remarks>
    /// Indicates whether the error strategy is currently "recovering from an
    /// error". This is used to suppress reporting multiple error messages while
    /// attempting to recover from a detected syntax error.
    /// </remarks>
    /// <seealso cref="M:Antlr4.Runtime.DefaultErrorStrategy.InErrorRecoveryMode(Antlr4.Runtime.Parser)" />
    protected internal bool errorRecoveryMode;
    /// <summary>The index into the input stream where the last error occurred.</summary>
    /// <remarks>
    /// The index into the input stream where the last error occurred.
    /// This is used to prevent infinite loops where an error is found
    /// but no token is consumed during recovery...another error is found,
    /// ad nauseum.  This is a failsafe mechanism to guarantee that at least
    /// one token/tree node is consumed for two errors.
    /// </remarks>
    protected internal int lastErrorIndex = -1;
    protected internal IntervalSet lastErrorStates;
    /// This field is used to propagate information about the lookahead following
    ///             the previous match. Since prediction prefers completing the current rule
    ///             to error recovery efforts, error reporting may occur later than the
    ///             original point where it was discoverable. The original context is used to
    ///             compute the true expected sets as though the reporting occurred as early
    ///             as possible.
    protected ParserRuleContext nextTokensContext;
    /// @see #nextTokensContext
    protected int nextTokensState;

    /// <summary>
    /// <inheritDoc />
    /// <p>The default implementation simply calls
    /// <see cref="M:Antlr4.Runtime.DefaultErrorStrategy.EndErrorCondition(Antlr4.Runtime.Parser)" />
    /// to
    /// ensure that the handler is not in error recovery mode.</p>
    /// </summary>
    public override void Reset(Parser recognizer) => this.EndErrorCondition(recognizer);

    /// <summary>
    /// This method is called to enter error recovery mode when a recognition
    /// exception is reported.
    /// </summary>
    /// <remarks>
    /// This method is called to enter error recovery mode when a recognition
    /// exception is reported.
    /// </remarks>
    /// <param name="recognizer">the parser instance</param>
    protected internal virtual void BeginErrorCondition(Parser recognizer) => this.errorRecoveryMode = true;

    /// <summary><inheritDoc /></summary>
    public override bool InErrorRecoveryMode(Parser recognizer) => this.errorRecoveryMode;

    /// <summary>
    /// This method is called to leave error recovery mode after recovering from
    /// a recognition exception.
    /// </summary>
    /// <remarks>
    /// This method is called to leave error recovery mode after recovering from
    /// a recognition exception.
    /// </remarks>
    /// <param name="recognizer" />
    protected internal virtual void EndErrorCondition(Parser recognizer)
    {
      this.errorRecoveryMode = false;
      this.lastErrorStates = (IntervalSet) null;
      this.lastErrorIndex = -1;
    }

    /// <summary>
    /// <inheritDoc />
    /// <p>The default implementation simply calls
    /// <see cref="M:Antlr4.Runtime.DefaultErrorStrategy.EndErrorCondition(Antlr4.Runtime.Parser)" />
    /// .</p>
    /// </summary>
    public override void ReportMatch(Parser recognizer) => this.EndErrorCondition(recognizer);

    /// <summary>
    /// <inheritDoc />
    /// <p>The default implementation returns immediately if the handler is already
    /// in error recovery mode. Otherwise, it calls
    /// <see cref="M:Antlr4.Runtime.DefaultErrorStrategy.BeginErrorCondition(Antlr4.Runtime.Parser)" />
    /// and dispatches the reporting task based on the runtime type of
    /// <paramref name="e" />
    /// according to the following table.</p>
    /// <ul>
    /// <li>
    /// <see cref="T:Antlr4.Runtime.NoViableAltException" />
    /// : Dispatches the call to
    /// <see cref="M:Antlr4.Runtime.DefaultErrorStrategy.ReportNoViableAlternative(Antlr4.Runtime.Parser,Antlr4.Runtime.NoViableAltException)" />
    /// </li>
    /// <li>
    /// <see cref="T:Antlr4.Runtime.InputMismatchException" />
    /// : Dispatches the call to
    /// <see cref="M:Antlr4.Runtime.DefaultErrorStrategy.ReportInputMismatch(Antlr4.Runtime.Parser,Antlr4.Runtime.InputMismatchException)" />
    /// </li>
    /// <li>
    /// <see cref="T:Antlr4.Runtime.FailedPredicateException" />
    /// : Dispatches the call to
    /// <see cref="M:Antlr4.Runtime.DefaultErrorStrategy.ReportFailedPredicate(Antlr4.Runtime.Parser,Antlr4.Runtime.FailedPredicateException)" />
    /// </li>
    /// <li>All other types: calls
    /// <see cref="M:Antlr4.Runtime.Parser.NotifyErrorListeners(System.String)" />
    /// to report
    /// the exception</li>
    /// </ul>
    /// </summary>
    public override void ReportError(Parser recognizer, RecognitionException e)
    {
      if (this.InErrorRecoveryMode(recognizer))
        return;
      this.BeginErrorCondition(recognizer);
      switch (e)
      {
        case NoViableAltException _:
          this.ReportNoViableAlternative(recognizer, (NoViableAltException) e);
          break;
        case InputMismatchException _:
          this.ReportInputMismatch(recognizer, (InputMismatchException) e);
          break;
        case FailedPredicateException _:
          this.ReportFailedPredicate(recognizer, (FailedPredicateException) e);
          break;
        default:
          Console.Error.WriteLine("tipo de error de reconocimiento desconocido: " + e.GetType().FullName);
          this.NotifyErrorListeners(recognizer, e.Message, e);
          break;
      }
    }

    protected internal virtual void NotifyErrorListeners(
      Parser recognizer,
      string message,
      RecognitionException e)
    {
      recognizer.NotifyErrorListeners(e.OffendingToken, message, e);
    }

    /// <summary>
    /// <inheritDoc />
    /// <p>The default implementation resynchronizes the parser by consuming tokens
    /// until we find one in the resynchronization set--loosely the set of tokens
    /// that can follow the current rule.</p>
    /// </summary>
    public override void Recover(Parser recognizer, RecognitionException e)
    {
      if (this.lastErrorIndex == recognizer.InputStream.Index && this.lastErrorStates != null && this.lastErrorStates.Contains(recognizer.State))
        recognizer.Consume();
      this.lastErrorIndex = recognizer.InputStream.Index;
      if (this.lastErrorStates == null)
        this.lastErrorStates = new IntervalSet(Array.Empty<int>());
      this.lastErrorStates.Add(recognizer.State);
      IntervalSet errorRecoverySet = this.GetErrorRecoverySet(recognizer);
      this.ConsumeUntil(recognizer, errorRecoverySet);
    }

    /// <summary>
    /// The default implementation of
    /// <see cref="M:Antlr4.Runtime.IAntlrErrorStrategy.Sync(Antlr4.Runtime.Parser)" />
    /// makes sure
    /// that the current lookahead symbol is consistent with what were expecting
    /// at this point in the ATN. You can call this anytime but ANTLR only
    /// generates code to check before subrules/loops and each iteration.
    /// <p>Implements Jim Idle's magic sync mechanism in closures and optional
    /// subrules. E.g.,</p>
    /// <pre>
    /// a : sync ( stuff sync )* ;
    /// sync : {consume to what can follow sync} ;
    /// </pre>
    /// At the start of a sub rule upon error,
    /// <see cref="M:Antlr4.Runtime.DefaultErrorStrategy.Sync(Antlr4.Runtime.Parser)" />
    /// performs single
    /// token deletion, if possible. If it can't do that, it bails on the current
    /// rule and uses the default error recovery, which consumes until the
    /// resynchronization set of the current rule.
    /// <p>If the sub rule is optional (
    /// <c>(...)?</c>
    /// ,
    /// <c>(...)*</c>
    /// , or block
    /// with an empty alternative), then the expected set includes what follows
    /// the subrule.</p>
    /// <p>During loop iteration, it consumes until it sees a token that can start a
    /// sub rule or what follows loop. Yes, that is pretty aggressive. We opt to
    /// stay in the loop as long as possible.</p>
    /// <p><strong>ORIGINS</strong></p>
    /// <p>Previous versions of ANTLR did a poor job of their recovery within loops.
    /// A single mismatch token or missing token would force the parser to bail
    /// out of the entire rules surrounding the loop. So, for rule</p>
    /// <pre>
    /// classDef : 'class' ID '{' member* '}'
    /// </pre>
    /// input with an extra token between members would force the parser to
    /// consume until it found the next class definition rather than the next
    /// member definition of the current class.
    /// <p>This functionality cost a little bit of effort because the parser has to
    /// compare token set at the start of the loop and at each iteration. If for
    /// some reason speed is suffering for you, you can turn off this
    /// functionality by simply overriding this method as a blank { }.</p>
    /// </summary>
    /// <exception cref="T:Antlr4.Runtime.RecognitionException" />
    public override void Sync(Parser recognizer)
    {
      ATNState state = recognizer.Interpreter.atn.states[recognizer.State];
      if (this.InErrorRecoveryMode(recognizer))
        return;
      int el = recognizer.InputStream.LA(1);
      IntervalSet intervalSet = recognizer.Atn.NextTokens(state);
      if (intervalSet.Contains(el))
      {
        this.nextTokensContext = (ParserRuleContext) null;
        this.nextTokensState = -1;
      }
      else if (intervalSet.Contains(-2))
      {
        if (this.nextTokensContext != null)
          return;
        this.nextTokensContext = recognizer.Context;
        this.nextTokensState = recognizer.State;
      }
      else
      {
        switch (state.StateType)
        {
          case StateType.BlockStart:
          case StateType.PlusBlockStart:
          case StateType.StarBlockStart:
          case StateType.StarLoopEntry:
            if (this.SingleTokenDeletion(recognizer) != null)
              break;
            throw new InputMismatchException(recognizer);
          case StateType.StarLoopBack:
          case StateType.PlusLoopBack:
            this.ReportUnwantedToken(recognizer);
            IntervalSet set = recognizer.GetExpectedTokens().Or((IIntSet) this.GetErrorRecoverySet(recognizer));
            this.ConsumeUntil(recognizer, set);
            break;
        }
      }
    }

    /// <summary>
    /// This is called by
    /// <see cref="M:Antlr4.Runtime.DefaultErrorStrategy.ReportError(Antlr4.Runtime.Parser,Antlr4.Runtime.RecognitionException)" />
    /// when the exception is a
    /// <see cref="T:Antlr4.Runtime.NoViableAltException" />
    /// .
    /// </summary>
    /// <seealso cref="M:Antlr4.Runtime.DefaultErrorStrategy.ReportError(Antlr4.Runtime.Parser,Antlr4.Runtime.RecognitionException)" />
    /// <param name="recognizer">the parser instance</param>
    /// <param name="e">the recognition exception</param>
    protected internal virtual void ReportNoViableAlternative(
      Parser recognizer,
      NoViableAltException e)
    {
      ITokenStream inputStream = (ITokenStream) recognizer.InputStream;
      string message = "ninguna alternativa viable a la entrada " + this.EscapeWSAndQuote(inputStream == null ? "<entrada desconocida>" : (e.StartToken.Type != -1 ? inputStream.GetText(e.StartToken, e.OffendingToken) : "<EOF>"));
      this.NotifyErrorListeners(recognizer, message, (RecognitionException) e);
    }

    /// <summary>
    /// This is called by
    /// <see cref="M:Antlr4.Runtime.DefaultErrorStrategy.ReportError(Antlr4.Runtime.Parser,Antlr4.Runtime.RecognitionException)" />
    /// when the exception is an
    /// <see cref="T:Antlr4.Runtime.InputMismatchException" />
    /// .
    /// </summary>
    /// <seealso cref="M:Antlr4.Runtime.DefaultErrorStrategy.ReportError(Antlr4.Runtime.Parser,Antlr4.Runtime.RecognitionException)" />
    /// <param name="recognizer">the parser instance</param>
    /// <param name="e">the recognition exception</param>
    protected internal virtual void ReportInputMismatch(Parser recognizer, InputMismatchException e)
    {
      string message = "entrada incorrecta " + this.GetTokenErrorDisplay(e.OffendingToken) + " esperado " + e.GetExpectedTokens().ToString(recognizer.Vocabulary);
      this.NotifyErrorListeners(recognizer, message, (RecognitionException) e);
    }

    /// <summary>
    /// This is called by
    /// <see cref="M:Antlr4.Runtime.DefaultErrorStrategy.ReportError(Antlr4.Runtime.Parser,Antlr4.Runtime.RecognitionException)" />
    /// when the exception is a
    /// <see cref="T:Antlr4.Runtime.FailedPredicateException" />
    /// .
    /// </summary>
    /// <seealso cref="M:Antlr4.Runtime.DefaultErrorStrategy.ReportError(Antlr4.Runtime.Parser,Antlr4.Runtime.RecognitionException)" />
    /// <param name="recognizer">the parser instance</param>
    /// <param name="e">the recognition exception</param>
    protected internal virtual void ReportFailedPredicate(
      Parser recognizer,
      FailedPredicateException e)
    {
      string message = "regla " + recognizer.RuleNames[recognizer.RuleContext.RuleIndex] + " " + e.Message;
      this.NotifyErrorListeners(recognizer, message, (RecognitionException) e);
    }

    /// <summary>
    /// This method is called to report a syntax error which requires the removal
    /// of a token from the input stream.
    /// </summary>
    /// <remarks>
    /// This method is called to report a syntax error which requires the removal
    /// of a token from the input stream. At the time this method is called, the
    /// erroneous symbol is current
    /// <c>LT(1)</c>
    /// symbol and has not yet been
    /// removed from the input stream. When this method returns,
    /// <paramref name="recognizer" />
    /// is in error recovery mode.
    /// <p>This method is called when
    /// <see cref="M:Antlr4.Runtime.DefaultErrorStrategy.SingleTokenDeletion(Antlr4.Runtime.Parser)" />
    /// identifies
    /// single-token deletion as a viable recovery strategy for a mismatched
    /// input error.</p>
    /// <p>The default implementation simply returns if the handler is already in
    /// error recovery mode. Otherwise, it calls
    /// <see cref="M:Antlr4.Runtime.DefaultErrorStrategy.BeginErrorCondition(Antlr4.Runtime.Parser)" />
    /// to
    /// enter error recovery mode, followed by calling
    /// <see cref="M:Antlr4.Runtime.Parser.NotifyErrorListeners(System.String)" />
    /// .</p>
    /// </remarks>
    /// <param name="recognizer">the parser instance</param>
    protected internal virtual void ReportUnwantedToken(Parser recognizer)
    {
      if (this.InErrorRecoveryMode(recognizer))
        return;
      this.BeginErrorCondition(recognizer);
      IToken currentToken = recognizer.CurrentToken;
      string msg = "entradas extrañas " + this.GetTokenErrorDisplay(currentToken) + " esperado " + this.GetExpectedTokens(recognizer).ToString(recognizer.Vocabulary);
      recognizer.NotifyErrorListeners(currentToken, msg, (RecognitionException) null);
    }

    /// <summary>
    /// This method is called to report a syntax error which requires the
    /// insertion of a missing token into the input stream.
    /// </summary>
    /// <remarks>
    /// This method is called to report a syntax error which requires the
    /// insertion of a missing token into the input stream. At the time this
    /// method is called, the missing token has not yet been inserted. When this
    /// method returns,
    /// <paramref name="recognizer" />
    /// is in error recovery mode.
    /// <p>This method is called when
    /// <see cref="M:Antlr4.Runtime.DefaultErrorStrategy.SingleTokenInsertion(Antlr4.Runtime.Parser)" />
    /// identifies
    /// single-token insertion as a viable recovery strategy for a mismatched
    /// input error.</p>
    /// <p>The default implementation simply returns if the handler is already in
    /// error recovery mode. Otherwise, it calls
    /// <see cref="M:Antlr4.Runtime.DefaultErrorStrategy.BeginErrorCondition(Antlr4.Runtime.Parser)" />
    /// to
    /// enter error recovery mode, followed by calling
    /// <see cref="M:Antlr4.Runtime.Parser.NotifyErrorListeners(System.String)" />
    /// .</p>
    /// </remarks>
    /// <param name="recognizer">the parser instance</param>
    protected internal virtual void ReportMissingToken(Parser recognizer)
    {
      if (this.InErrorRecoveryMode(recognizer))
        return;
      this.BeginErrorCondition(recognizer);
      IToken currentToken = recognizer.CurrentToken;
      string msg = "falta " + this.GetExpectedTokens(recognizer).ToString(recognizer.Vocabulary) + " en " + this.GetTokenErrorDisplay(currentToken);
      recognizer.NotifyErrorListeners(currentToken, msg, (RecognitionException) null);
    }

    /// <summary>
    /// <inheritDoc />
    /// <p>The default implementation attempts to recover from the mismatched input
    /// by using single token insertion and deletion as described below. If the
    /// recovery attempt fails, this method throws an
    /// <see cref="T:Antlr4.Runtime.InputMismatchException" />
    /// .</p>
    /// <p><strong>EXTRA TOKEN</strong> (single token deletion)</p>
    /// <p>
    /// <c>LA(1)</c>
    /// is not what we are looking for. If
    /// <c>LA(2)</c>
    /// has the
    /// right token, however, then assume
    /// <c>LA(1)</c>
    /// is some extra spurious
    /// token and delete it. Then consume and return the next token (which was
    /// the
    /// <c>LA(2)</c>
    /// token) as the successful result of the match operation.</p>
    /// <p>This recovery strategy is implemented by
    /// <see cref="M:Antlr4.Runtime.DefaultErrorStrategy.SingleTokenDeletion(Antlr4.Runtime.Parser)" />
    /// .</p>
    /// <p><strong>MISSING TOKEN</strong> (single token insertion)</p>
    /// <p>If current token (at
    /// <c>LA(1)</c>
    /// ) is consistent with what could come
    /// after the expected
    /// <c>LA(1)</c>
    /// token, then assume the token is missing
    /// and use the parser's
    /// <see cref="T:Antlr4.Runtime.ITokenFactory" />
    /// to create it on the fly. The
    /// "insertion" is performed by returning the created token as the successful
    /// result of the match operation.</p>
    /// <p>This recovery strategy is implemented by
    /// <see cref="M:Antlr4.Runtime.DefaultErrorStrategy.SingleTokenInsertion(Antlr4.Runtime.Parser)" />
    /// .</p>
    /// <p><strong>EXAMPLE</strong></p>
    /// <p>For example, Input
    /// <c>i=(3;</c>
    /// is clearly missing the
    /// <c>')'</c>
    /// . When
    /// the parser returns from the nested call to
    /// <c>expr</c>
    /// , it will have
    /// call chain:</p>
    /// <pre>
    /// stat → expr → atom
    /// </pre>
    /// and it will be trying to match the
    /// <c>')'</c>
    /// at this point in the
    /// derivation:
    /// <pre>
    /// =&gt; ID '=' '(' INT ')' ('+' atom)* ';'
    /// ^
    /// </pre>
    /// The attempt to match
    /// <c>')'</c>
    /// will fail when it sees
    /// <c>';'</c>
    /// and
    /// call
    /// <see cref="M:Antlr4.Runtime.DefaultErrorStrategy.RecoverInline(Antlr4.Runtime.Parser)" />
    /// . To recover, it sees that
    /// <c>LA(1)==';'</c>
    /// is in the set of tokens that can follow the
    /// <c>')'</c>
    /// token reference
    /// in rule
    /// <c>atom</c>
    /// . It can assume that you forgot the
    /// <c>')'</c>
    /// .
    /// </summary>
    /// <exception cref="T:Antlr4.Runtime.RecognitionException" />
    public override IToken RecoverInline(Parser recognizer)
    {
      IToken token = this.SingleTokenDeletion(recognizer);
      if (token != null)
      {
        recognizer.Consume();
        return token;
      }
      return this.SingleTokenInsertion(recognizer) ? this.GetMissingSymbol(recognizer) : throw new InputMismatchException(recognizer);
    }

    /// <summary>
    /// This method implements the single-token insertion inline error recovery
    /// strategy.
    /// </summary>
    /// <remarks>
    /// This method implements the single-token insertion inline error recovery
    /// strategy. It is called by
    /// <see cref="M:Antlr4.Runtime.DefaultErrorStrategy.RecoverInline(Antlr4.Runtime.Parser)" />
    /// if the single-token
    /// deletion strategy fails to recover from the mismatched input. If this
    /// method returns
    /// <see langword="true" />
    /// ,
    /// <paramref name="recognizer" />
    /// will be in error recovery
    /// mode.
    /// <p>This method determines whether or not single-token insertion is viable by
    /// checking if the
    /// <c>LA(1)</c>
    /// input symbol could be successfully matched
    /// if it were instead the
    /// <c>LA(2)</c>
    /// symbol. If this method returns
    /// <see langword="true" />
    /// , the caller is responsible for creating and inserting a
    /// token with the correct type to produce this behavior.</p>
    /// </remarks>
    /// <param name="recognizer">the parser instance</param>
    /// <returns>
    /// 
    /// <see langword="true" />
    /// if single-token insertion is a viable recovery
    /// strategy for the current mismatched input, otherwise
    /// <see langword="false" />
    /// </returns>
    protected internal virtual bool SingleTokenInsertion(Parser recognizer)
    {
      int el = recognizer.InputStream.LA(1);
      ATNState target = recognizer.Interpreter.atn.states[recognizer.State].Transition(0).target;
      if (!recognizer.Interpreter.atn.NextTokens(target, (RuleContext) recognizer.RuleContext).Contains(el))
        return false;
      this.ReportMissingToken(recognizer);
      return true;
    }

    /// <summary>
    /// This method implements the single-token deletion inline error recovery
    /// strategy.
    /// </summary>
    /// <remarks>
    /// This method implements the single-token deletion inline error recovery
    /// strategy. It is called by
    /// <see cref="M:Antlr4.Runtime.DefaultErrorStrategy.RecoverInline(Antlr4.Runtime.Parser)" />
    /// to attempt to recover
    /// from mismatched input. If this method returns null, the parser and error
    /// handler state will not have changed. If this method returns non-null,
    /// <paramref name="recognizer" />
    /// will <em>not</em> be in error recovery mode since the
    /// returned token was a successful match.
    /// <p>If the single-token deletion is successful, this method calls
    /// <see cref="M:Antlr4.Runtime.DefaultErrorStrategy.ReportUnwantedToken(Antlr4.Runtime.Parser)" />
    /// to report the error, followed by
    /// <see cref="M:Antlr4.Runtime.Parser.Consume" />
    /// to actually "delete" the extraneous token. Then,
    /// before returning
    /// <see cref="M:Antlr4.Runtime.DefaultErrorStrategy.ReportMatch(Antlr4.Runtime.Parser)" />
    /// is called to signal a successful
    /// match.</p>
    /// </remarks>
    /// <param name="recognizer">the parser instance</param>
    /// <returns>
    /// the successfully matched
    /// <see cref="T:Antlr4.Runtime.IToken" />
    /// instance if single-token
    /// deletion successfully recovers from the mismatched input, otherwise
    /// <see langword="null" />
    /// </returns>
    [return: Nullable]
    protected internal virtual IToken SingleTokenDeletion(Parser recognizer)
    {
      int el = recognizer.InputStream.LA(2);
      if (!this.GetExpectedTokens(recognizer).Contains(el))
        return (IToken) null;
      this.ReportUnwantedToken(recognizer);
      recognizer.Consume();
      IToken currentToken = recognizer.CurrentToken;
      this.ReportMatch(recognizer);
      return currentToken;
    }

    /// <summary>Conjure up a missing token during error recovery.</summary>
    /// <remarks>
    /// Conjure up a missing token during error recovery.
    /// The recognizer attempts to recover from single missing
    /// symbols. But, actions might refer to that missing symbol.
    /// For example, x=ID {f($x);}. The action clearly assumes
    /// that there has been an identifier matched previously and that
    /// $x points at that token. If that token is missing, but
    /// the next token in the stream is what we want we assume that
    /// this token is missing and we keep going. Because we
    /// have to return some token to replace the missing token,
    /// we have to conjure one up. This method gives the user control
    /// over the tokens returned for missing tokens. Mostly,
    /// you will want to create something special for identifier
    /// tokens. For literals such as '{' and ',', the default
    /// action in the parser or tree parser works. It simply creates
    /// a CommonToken of the appropriate type. The text will be the token.
    /// If you change what tokens must be created by the lexer,
    /// override this method to create the appropriate tokens.
    /// </remarks>
    [return: NotNull]
    protected internal virtual IToken GetMissingSymbol(Parser recognizer)
    {
      IToken currentToken = recognizer.CurrentToken;
      int minElement = this.GetExpectedTokens(recognizer).MinElement;
      string tokenText = minElement != -1 ? "<falta " + recognizer.Vocabulary.GetDisplayName(minElement) + ">" : "<falta EOF>";
      IToken current = currentToken;
      IToken token = ((ITokenStream) recognizer.InputStream).LT(-1);
      if (current.Type == -1 && token != null)
        current = token;
      return this.ConstructToken(((ITokenStream) recognizer.InputStream).TokenSource, minElement, tokenText, current);
    }

    protected internal virtual IToken ConstructToken(
      ITokenSource tokenSource,
      int expectedTokenType,
      string tokenText,
      IToken current)
    {
      return tokenSource.TokenFactory.Create(Tuple.Create<ITokenSource, ICharStream>(tokenSource, current.TokenSource.InputStream), expectedTokenType, tokenText, 0, -1, -1, current.Line, current.Column);
    }

    [return: NotNull]
    protected internal virtual IntervalSet GetExpectedTokens(Parser recognizer) => recognizer.GetExpectedTokens();

    /// <summary>
    /// How should a token be displayed in an error message? The default
    /// is to display just the text, but during development you might
    /// want to have a lot of information spit out.
    /// </summary>
    /// <remarks>
    /// How should a token be displayed in an error message? The default
    /// is to display just the text, but during development you might
    /// want to have a lot of information spit out.  Override in that case
    /// to use t.toString() (which, for CommonToken, dumps everything about
    /// the token). This is better than forcing you to override a method in
    /// your token objects because you don't have to go modify your lexer
    /// so that it creates a new Java type.
    /// </remarks>
    protected internal virtual string GetTokenErrorDisplay(IToken t) => t == null ? "<sin token>" : this.EscapeWSAndQuote(this.GetSymbolText(t) ?? (this.GetSymbolType(t) != -1 ? "<" + this.GetSymbolType(t).ToString() + ">" : "<EOF>"));

    protected internal virtual string GetSymbolText(IToken symbol) => symbol.Text;

    protected internal virtual int GetSymbolType(IToken symbol) => symbol.Type;

    [return: NotNull]
    protected internal virtual string EscapeWSAndQuote(string s)
    {
      s = s.Replace("\n", "\\n");
      s = s.Replace("\r", "\\r");
      s = s.Replace("\t", "\\t");
      return "'" + s + "'";
    }

    [return: NotNull]
    protected internal virtual IntervalSet GetErrorRecoverySet(Parser recognizer)
    {
      ATN atn = recognizer.Interpreter.atn;
      RuleContext ruleContext = (RuleContext) recognizer.RuleContext;
      IntervalSet errorRecoverySet = new IntervalSet(Array.Empty<int>());
      for (; ruleContext != null && ruleContext.invokingState >= 0; ruleContext = ruleContext.Parent)
      {
        RuleTransition ruleTransition = (RuleTransition) atn.states[ruleContext.invokingState].Transition(0);
        IntervalSet set = atn.NextTokens(ruleTransition.followState);
        errorRecoverySet.AddAll((IIntSet) set);
      }
      errorRecoverySet.Remove(-2);
      return errorRecoverySet;
    }

    /// <summary>Consume tokens until one matches the given token set.</summary>
    /// <remarks>Consume tokens until one matches the given token set.</remarks>
    protected internal virtual void ConsumeUntil(Parser recognizer, IntervalSet set)
    {
      for (int el = recognizer.InputStream.LA(1); el != -1 && !set.Contains(el); el = recognizer.InputStream.LA(1))
        recognizer.Consume();
    }
  }