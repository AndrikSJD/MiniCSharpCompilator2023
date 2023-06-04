using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using SyntacticAnalysisGenerated;



namespace Compilator;

public class codeGen1st: MiniCSharpParserBaseVisitor<Object>
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
    
    private FieldBuilder currentFieldBldr;
    
    private List<FieldBuilder> variablesGlobales;
    
    private List<FieldBuilder> variablesLocales;
    
    


    public codeGen1st()
    {
        metodosGlobales = new List<MethodBuilder>();
        variablesGlobales = new List<FieldBuilder>();
        variablesLocales = new List<FieldBuilder>();

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
    }


    public override object VisitProgramAST(MiniCSharpParser.ProgramASTContext context)
    {


        foreach (var node in context.children)
        {
            Visit(node);

        }
        
        _pointType = myTypeBldr.CreateType(); //crea la clase para ser luego instanciada
        myAsmBldr.SetEntryPoint(pointMainBldr); //establece el punto de entrada
        myAsmBldr.Save(asmFileName); //guarda el ensamblado en un archivo
            
        return null;
        

    }

    public override object VisitUsingAST(MiniCSharpParser.UsingASTContext context)
    {
        return null;
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
                return typeof(bool);
            case "void":
                return typeof(void);
            default:
                // Si el tipo no se reconoce, se asume que es un tipo definido por el usuario
                return Type.GetType(typeName);
        }
    }
    public override object VisitVarDeclAST(MiniCSharpParser.VarDeclASTContext context)
    {
        
        bool IsLocal = context.isLocal;
        if (IsLocal)
        {
        
            for (int i = 0; i < context.ident().Length ; i++)
            {
                string name = context.ident(i).GetText();
                Type varType = GetTypeFromString((string)Visit(context.type()));
                
                //guardar como variable global
                currentFieldBldr=   myTypeBldr.DefineField(name, varType, FieldAttributes.Public|FieldAttributes.Static);
                variablesGlobales.Add(currentFieldBldr);
                 
            }
        }
        else
        {
            
            for (int i = 0; i < context.ident().Length ; i++)
            {
                
                string name = context.ident(i).GetText();
                Type varType = GetTypeFromString((string)Visit(context.type()));
                
                //guardar como variable local
                currentFieldBldr=   myTypeBldr.DefineField(name, varType, FieldAttributes.Public|FieldAttributes.Static);
                variablesLocales.Add(currentFieldBldr);
        
            }
            
        }
        
        
        
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

       
       
       
       // foreach (var ident in context.ident())
       // {
       //     System.Diagnostics.Debug.WriteLine("VisitVarDeclAST");
       //     System.Diagnostics.Debug.WriteLine("Ident: "+ ident.declPointer.GetText());
       //     System.Diagnostics.Debug.WriteLine("isLocal: "+context.isLocal);
       //     System.Diagnostics.Debug.WriteLine("indexVar: "+context.indexVar);
       //
       // }
       return null;
    }

    public override object VisitClassDeclAST(MiniCSharpParser.ClassDeclASTContext context)
    {

        //visita las declaraciones de variables
        foreach (var var in context.varDecl())
        {
            Visit(var);
        }
        return null;
    }

    public override object VisitMethodDeclAST(MiniCSharpParser.MethodDeclASTContext context)
    {
        // Obtener el tipo de retorno del método
        Type typeMethod = null;
        if(context.type() != null)
            typeMethod = GetTypeFromString((string)Visit(context.type()));
        else
            typeMethod = typeof(void);

        //Declaramos el método
        currentMethodBldr = myTypeBldr.DefineMethod(
            context.ident().GetText(),
            MethodAttributes.Public | MethodAttributes.Static,
            typeMethod,
            null); //los parametros se añaden en el siguiente visit
        
        //Hacemos el arreglo con los tipos de los parametros
        Type[] parameterTypes = null;

        if (context.formPars() != null)
            parameterTypes = (Type[]) Visit(context.formPars());
        
        //Se agregan los parametros al metodo
        currentMethodBldr.SetParameters(parameterTypes);
        
        Visit(context.block());
        
        //Se agrega el metodo a la lista de metodos
        metodosGlobales.Add(currentMethodBldr);

        if (context.ident().GetText().Equals("Main"))
        {
            pointMainBldr = currentMethodBldr;
        }
        return null;
    }

    public override object VisitFormParsAST(MiniCSharpParser.FormParsASTContext context)
    {
        ILGenerator currentIL = currentMethodBldr.GetILGenerator();
        isArgument = true;
        
        //Se crea el arreglo con los tipos de los parametros
        Type[] parameterTypes = new Type[context.ident().Length];

        for (int i = 0; i < context.ident().Length; i++)
        {
            parameterTypes[i] = GetTypeFromString((string)Visit(context.type(i)));
            currentIL.Emit(OpCodes.Ldarg, i);
            currentIL.DeclareLocal(parameterTypes[i]);
            currentIL.Emit(OpCodes.Stloc, i);
        }
        
        isArgument = false;
        
        return parameterTypes;
    }

    public override object VisitTypeAST(MiniCSharpParser.TypeASTContext context)
    {
        
        return (string) context.ident().GetText();
        
        
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
        foreach (var node in context.children)
        {
            if (node is MiniCSharpParser.VarDeclASTContext)
            {
                Visit(node);
            }
            else if (node is MiniCSharpParser.StatementContext)
            {
                Visit(node);
            }
        }
        return null;
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
        return GetTypeFromString(context.ID().GetText());
    }

    public override object VisitRelopAST(MiniCSharpParser.RelopASTContext context)
    {
        return base.VisitRelopAST(context);
    }
    
}