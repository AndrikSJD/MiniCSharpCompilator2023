using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using SyntacticAnalysisGenerated;



namespace Compilator;

public class CodeGeneration: MiniCSharpParserBaseVisitor<Object>
{
    private Type _pointType = null;
    private string asmFileName = "test.exe";
    private AssemblyName myAsmName = new AssemblyName();

    private AppDomain currentDom = Thread.GetDomain();
    private AssemblyBuilder myAsmBldr;

    private ModuleBuilder myModuleBldr;

    private TypeBuilder myTypeBldr;
    private ConstructorInfo objCtor=null;

    private MethodInfo writeMI, writeMS;

    private MethodBuilder pointMainBldr, currentMethodBldr;

    private List<MethodBuilder> metodosGlobales; 

    private bool isArgument = false;



    public CodeGeneration(string fileName)
    {
        metodosGlobales = new List<MethodBuilder>();
            
        myAsmName.Name = "TestASM";
        myAsmBldr = currentDom.DefineDynamicAssembly(myAsmName, AssemblyBuilderAccess.RunAndSave);
        myModuleBldr = myAsmBldr.DefineDynamicModule(asmFileName);
        myTypeBldr = myModuleBldr.DefineType("TestClass");
            
        Type objType = Type.GetType("System.Object");
        objCtor = objType.GetConstructor(new Type[0]);
            
        Type[] ctorParams = new Type[0];
        ConstructorBuilder pointCtor = myTypeBldr.DefineConstructor(
            MethodAttributes.Public,
            CallingConventions.Standard,
            ctorParams);
        ILGenerator ctorIL = pointCtor.GetILGenerator();
        ctorIL.Emit(OpCodes.Ldarg_0);
        ctorIL.Emit(OpCodes.Call, objCtor);
        ctorIL.Emit(OpCodes.Ret);
            
        //inicializar writeline para string
            
        writeMI = typeof(Console).GetMethod(
            "WriteLine",
            new Type[] { typeof(int) });
        writeMS = typeof(Console).GetMethod(
            "WriteLine",
            new Type[] { typeof(string) });

    }
    
    public override object VisitProgramAST(MiniCSharpParser.ProgramASTContext context)
    {


        foreach (var node in context.children)
        {
            Visit(node);

        }
        
        _pointType = myTypeBldr.CreateType(); //crea la clase para ser luego instanciada
        myAsmBldr.SetEntryPoint(pointMainBldr);
        myAsmBldr.Save(asmFileName);
            
        return _pointType;
        

    }

    public override object VisitUsingAST(MiniCSharpParser.UsingASTContext context)
    {
        return base.VisitUsingAST(context);
    }

    // Método auxiliar para obtener el tipo a partir de una cadena de texto
    private Type GetTypeFromString(string typeName)
    {
        switch (typeName)
        {
            case "int":
                return typeof(int);
            case "char":
                return typeof(char);
            case "string":
                return typeof(string);
            case "double":
                return typeof(double);
            case "boolean":
                return typeof(Boolean);
            default:
                // Si el tipo no se reconoce, se asume que es un tipo definido por el usuario
                return Type.GetType(typeName);
        }
    }
    public override object VisitVarDeclAST(MiniCSharpParser.VarDeclASTContext context)
    {
       //  string varType = context.type().GetText(); // Obtener el tipo de la variable
       //  
       // // MiniCSharpParser.IdentASTContext[] identifiers = context.ident(); // Obtener los identificadores de las variables
       //
       //  List<MiniCSharpParser.IdentASTContext> identList = new List<MiniCSharpParser.IdentASTContext>(identifiers);
       //
       //  
       //  foreach (var identifier in identifiers)
       //  {
       //      string varName = identifier.GetText(); // Obtener el nombre de la variable
       //  
       //      // Generar el campo o variable local en el tipo actual
       //      if (isArgument)
       //      {
       //          // Si es un argumento de método, generamos un parámetro
       //          ParameterBuilder paramBldr = currentMethodBldr.DefineParameter(
       //              identifier.declPointer.Symbol.TokenIndex + 1, // El índice del token para el argumento
       //              ParameterAttributes.None,
       //              varName);
       //          paramBldr.SetCustomAttribute(new CustomAttributeBuilder(typeof(ParamArrayAttribute).GetConstructor(Type.EmptyTypes), new object[0]));
       //      }
       //      else
       //      {
       //          // Si es una variable local, generamos un campo en el tipo actual
       //          FieldBuilder fieldBldr = myTypeBldr.DefineField(
       //              varName,
       //              GetTypeFromString(varType),
       //              FieldAttributes.Private);
       //      }
       //  }

       
       Type varType = (Type)Visit(context.type());
       
       
       foreach (var ident in context.ident())
       {
           System.Diagnostics.Debug.WriteLine("VisitVarDeclAST");
           System.Diagnostics.Debug.WriteLine("Ident: "+ ident.declPointer.GetText());
           System.Diagnostics.Debug.WriteLine("isLocal: "+context.isLocal);
           System.Diagnostics.Debug.WriteLine("indexVar: "+context.indexVar);

       }
       
      
         
    
        return null;
    }

    public override object VisitClassDeclAST(MiniCSharpParser.ClassDeclASTContext context)
    {

        foreach (var var in context.varDecl())
        {
            Visit(var);
        }
        return base.VisitClassDeclAST(context);
    }

    public override object VisitMethodDeclAST(MiniCSharpParser.MethodDeclASTContext context)
    {
        return base.VisitMethodDeclAST(context);
    }

    public override object VisitFormParsAST(MiniCSharpParser.FormParsASTContext context)
    {
        return base.VisitFormParsAST(context);
    }

    public override object VisitTypeAST(MiniCSharpParser.TypeASTContext context)
    {
        return base.VisitTypeAST(context);
    }

    public override object VisitAssignStatementAST(MiniCSharpParser.AssignStatementASTContext context)
    {
        return base.VisitAssignStatementAST(context);
    }

    public override object VisitIfStatementAST(MiniCSharpParser.IfStatementASTContext context)
    {
        return base.VisitIfStatementAST(context);
    }

    public override object VisitForStatementAST(MiniCSharpParser.ForStatementASTContext context)
    {
        return base.VisitForStatementAST(context);
    }

    public override object VisitWhileStatementAST(MiniCSharpParser.WhileStatementASTContext context)
    {
        return base.VisitWhileStatementAST(context);
    }

    public override object VisitBreakStatementAST(MiniCSharpParser.BreakStatementASTContext context)
    {
        return base.VisitBreakStatementAST(context);
    }

    public override object VisitReturnStatementAST(MiniCSharpParser.ReturnStatementASTContext context)
    {
        return base.VisitReturnStatementAST(context);
    }

    public override object VisitReadStatementAST(MiniCSharpParser.ReadStatementASTContext context)
    {
        return base.VisitReadStatementAST(context);
    }

    public override object VisitWriteStatementAST(MiniCSharpParser.WriteStatementASTContext context)
    {
        return base.VisitWriteStatementAST(context);
    }

    public override object VisitBlockStatementAST(MiniCSharpParser.BlockStatementASTContext context)
    {
        return base.VisitBlockStatementAST(context);
    }

    public override object VisitBlockCommentStatementAST(MiniCSharpParser.BlockCommentStatementASTContext context)
    {
        return base.VisitBlockCommentStatementAST(context);
    }

    public override object VisitSemicolonStatementAST(MiniCSharpParser.SemicolonStatementASTContext context)
    {
        return base.VisitSemicolonStatementAST(context);
    }

    public override object VisitBlockAST(MiniCSharpParser.BlockASTContext context)
    {
        return base.VisitBlockAST(context);
    }

    public override object VisitActParsAST(MiniCSharpParser.ActParsASTContext context)
    {
        return base.VisitActParsAST(context);
    }

    public override object VisitConditionAST(MiniCSharpParser.ConditionASTContext context)
    {
        return base.VisitConditionAST(context);
    }

    public override object VisitCondTermAST(MiniCSharpParser.CondTermASTContext context)
    {
        return base.VisitCondTermAST(context);
    }

    public override object VisitCondFactAST(MiniCSharpParser.CondFactASTContext context)
    {
        return base.VisitCondFactAST(context);
    }

    public override object VisitCastAST(MiniCSharpParser.CastASTContext context)
    {
        return base.VisitCastAST(context);
    }

    public override object VisitExpressionAST(MiniCSharpParser.ExpressionASTContext context)
    {
        return base.VisitExpressionAST(context);
    }

    public override object VisitTermAST(MiniCSharpParser.TermASTContext context)
    {
        return base.VisitTermAST(context);
    }

    public override object VisitFactorAST(MiniCSharpParser.FactorASTContext context)
    {
        return base.VisitFactorAST(context);
    }

    public override object VisitNumFactorAST(MiniCSharpParser.NumFactorASTContext context)
    {
        return base.VisitNumFactorAST(context);
    }

    public override object VisitCharFactorAST(MiniCSharpParser.CharFactorASTContext context)
    {
        return base.VisitCharFactorAST(context);
    }

    public override object VisitStringFactorAST(MiniCSharpParser.StringFactorASTContext context)
    {
        return base.VisitStringFactorAST(context);
    }

    public override object VisitDoubleFactorAST(MiniCSharpParser.DoubleFactorASTContext context)
    {
        return base.VisitDoubleFactorAST(context);
    }

    public override object VisitBooleanFactorAST(MiniCSharpParser.BooleanFactorASTContext context)
    {
        return base.VisitBooleanFactorAST(context);
    }

    public override object VisitNewFactorAST(MiniCSharpParser.NewFactorASTContext context)
    {
        return base.VisitNewFactorAST(context);
    }

    public override object VisitParenFactorAST(MiniCSharpParser.ParenFactorASTContext context)
    {
        return base.VisitParenFactorAST(context);
    }

    public override object VisitDesignatorAST(MiniCSharpParser.DesignatorASTContext context)
    {
        return base.VisitDesignatorAST(context);
    }

    public override object VisitIdentAST(MiniCSharpParser.IdentASTContext context)
    {
        return base.VisitIdentAST(context);
    }

    public override object VisitRelopAST(MiniCSharpParser.RelopASTContext context)
    {
        return base.VisitRelopAST(context);
    }
    
}