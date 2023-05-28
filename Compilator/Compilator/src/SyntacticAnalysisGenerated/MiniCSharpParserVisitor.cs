//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.12.0
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from C:/Users/kp077/OneDrive/Escritorio/TEC/Quinto_Semestre/Compi/Compilator/Compilator\MiniCSharpParser.g4 by ANTLR 4.12.0

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

namespace SyntacticAnalysisGenerated {
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete generic visitor for a parse tree produced
/// by <see cref="MiniCSharpParser"/>.
/// </summary>
/// <typeparam name="Result">The return type of the visit operation.</typeparam>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.12.0")]
[System.CLSCompliant(false)]
public interface IMiniCSharpParserVisitor<Result> : IParseTreeVisitor<Result> {
	/// <summary>
	/// Visit a parse tree produced by the <c>programAST</c>
	/// labeled alternative in <see cref="MiniCSharpParser.program"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitProgramAST([NotNull] MiniCSharpParser.ProgramASTContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>usingAST</c>
	/// labeled alternative in <see cref="MiniCSharpParser.using"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitUsingAST([NotNull] MiniCSharpParser.UsingASTContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>varDeclAST</c>
	/// labeled alternative in <see cref="MiniCSharpParser.varDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitVarDeclAST([NotNull] MiniCSharpParser.VarDeclASTContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>classDeclAST</c>
	/// labeled alternative in <see cref="MiniCSharpParser.classDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitClassDeclAST([NotNull] MiniCSharpParser.ClassDeclASTContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>methodDeclAST</c>
	/// labeled alternative in <see cref="MiniCSharpParser.methodDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitMethodDeclAST([NotNull] MiniCSharpParser.MethodDeclASTContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>formParsAST</c>
	/// labeled alternative in <see cref="MiniCSharpParser.formPars"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFormParsAST([NotNull] MiniCSharpParser.FormParsASTContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>typeAST</c>
	/// labeled alternative in <see cref="MiniCSharpParser.type"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTypeAST([NotNull] MiniCSharpParser.TypeASTContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>assignStatementAST</c>
	/// labeled alternative in <see cref="MiniCSharpParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAssignStatementAST([NotNull] MiniCSharpParser.AssignStatementASTContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>ifStatementAST</c>
	/// labeled alternative in <see cref="MiniCSharpParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIfStatementAST([NotNull] MiniCSharpParser.IfStatementASTContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>forStatementAST</c>
	/// labeled alternative in <see cref="MiniCSharpParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitForStatementAST([NotNull] MiniCSharpParser.ForStatementASTContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>whileStatementAST</c>
	/// labeled alternative in <see cref="MiniCSharpParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitWhileStatementAST([NotNull] MiniCSharpParser.WhileStatementASTContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>breakStatementAST</c>
	/// labeled alternative in <see cref="MiniCSharpParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBreakStatementAST([NotNull] MiniCSharpParser.BreakStatementASTContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>returnStatementAST</c>
	/// labeled alternative in <see cref="MiniCSharpParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitReturnStatementAST([NotNull] MiniCSharpParser.ReturnStatementASTContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>readStatementAST</c>
	/// labeled alternative in <see cref="MiniCSharpParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitReadStatementAST([NotNull] MiniCSharpParser.ReadStatementASTContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>writeStatementAST</c>
	/// labeled alternative in <see cref="MiniCSharpParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitWriteStatementAST([NotNull] MiniCSharpParser.WriteStatementASTContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>blockStatementAST</c>
	/// labeled alternative in <see cref="MiniCSharpParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBlockStatementAST([NotNull] MiniCSharpParser.BlockStatementASTContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>blockCommentStatementAST</c>
	/// labeled alternative in <see cref="MiniCSharpParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBlockCommentStatementAST([NotNull] MiniCSharpParser.BlockCommentStatementASTContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>semicolonStatementAST</c>
	/// labeled alternative in <see cref="MiniCSharpParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitSemicolonStatementAST([NotNull] MiniCSharpParser.SemicolonStatementASTContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>blockAST</c>
	/// labeled alternative in <see cref="MiniCSharpParser.block"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBlockAST([NotNull] MiniCSharpParser.BlockASTContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>actParsAST</c>
	/// labeled alternative in <see cref="MiniCSharpParser.actPars"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitActParsAST([NotNull] MiniCSharpParser.ActParsASTContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>conditionAST</c>
	/// labeled alternative in <see cref="MiniCSharpParser.condition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitConditionAST([NotNull] MiniCSharpParser.ConditionASTContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>condTermAST</c>
	/// labeled alternative in <see cref="MiniCSharpParser.condTerm"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCondTermAST([NotNull] MiniCSharpParser.CondTermASTContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>condFactAST</c>
	/// labeled alternative in <see cref="MiniCSharpParser.condFact"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCondFactAST([NotNull] MiniCSharpParser.CondFactASTContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>castAST</c>
	/// labeled alternative in <see cref="MiniCSharpParser.cast"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCastAST([NotNull] MiniCSharpParser.CastASTContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>expressionAST</c>
	/// labeled alternative in <see cref="MiniCSharpParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpressionAST([NotNull] MiniCSharpParser.ExpressionASTContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>termAST</c>
	/// labeled alternative in <see cref="MiniCSharpParser.term"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTermAST([NotNull] MiniCSharpParser.TermASTContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>factorAST</c>
	/// labeled alternative in <see cref="MiniCSharpParser.factor"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFactorAST([NotNull] MiniCSharpParser.FactorASTContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>numFactorAST</c>
	/// labeled alternative in <see cref="MiniCSharpParser.factor"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitNumFactorAST([NotNull] MiniCSharpParser.NumFactorASTContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>charFactorAST</c>
	/// labeled alternative in <see cref="MiniCSharpParser.factor"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCharFactorAST([NotNull] MiniCSharpParser.CharFactorASTContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>stringFactorAST</c>
	/// labeled alternative in <see cref="MiniCSharpParser.factor"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStringFactorAST([NotNull] MiniCSharpParser.StringFactorASTContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>doubleFactorAST</c>
	/// labeled alternative in <see cref="MiniCSharpParser.factor"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDoubleFactorAST([NotNull] MiniCSharpParser.DoubleFactorASTContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>booleanFactorAST</c>
	/// labeled alternative in <see cref="MiniCSharpParser.factor"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBooleanFactorAST([NotNull] MiniCSharpParser.BooleanFactorASTContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>newFactorAST</c>
	/// labeled alternative in <see cref="MiniCSharpParser.factor"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitNewFactorAST([NotNull] MiniCSharpParser.NewFactorASTContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>parenFactorAST</c>
	/// labeled alternative in <see cref="MiniCSharpParser.factor"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitParenFactorAST([NotNull] MiniCSharpParser.ParenFactorASTContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>designatorAST</c>
	/// labeled alternative in <see cref="MiniCSharpParser.designator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDesignatorAST([NotNull] MiniCSharpParser.DesignatorASTContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>identAST</c>
	/// labeled alternative in <see cref="MiniCSharpParser.ident"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIdentAST([NotNull] MiniCSharpParser.IdentASTContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>relopAST</c>
	/// labeled alternative in <see cref="MiniCSharpParser.relop"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitRelopAST([NotNull] MiniCSharpParser.RelopASTContext context);
}
} // namespace SyntacticAnalysisGenerated
