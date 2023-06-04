using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Windows.Media;

namespace SyntacticAnalysisGenerated;

public class CodeGeneration : MiniCSharpParserBaseVisitor<Object>
{

    private Type pointType = null;
    private string asmFileName = "test.exe";
    private AssemblyName myAsmName = new AssemblyName();

    private AppDomain currentDom = Thread.GetDomain();
    private AssemblyBuilder myAsmBldr;

    private ModuleBuilder myModuleBldr;

    private TypeBuilder myTypeBldr;
    private ConstructorInfo objCtor = null;

    private MethodInfo writeMI, writeMS, writeMC, writeMB;

    private MethodBuilder pointMainBldr, currentMethodBldr;

    private List<MethodBuilder> metodosGlobales;

    private bool isArgument = false;

    //Diccionarios para guardar las variables locales y globales
    private Dictionary<string, FieldBuilder> globalVariables;
    private Dictionary<string, LocalBuilder> localVariables;
    
    
    //Diccionario para guardar clases
    private Dictionary<string, TypeBuilder> clases;
    
    //Variable para saber si estamos dentro de un metodo
    private bool isOnClass = false;
    
    //Variable para construir la clase
    private  TypeBuilder classBuilder;


    // breakLabel on loops
    Label fBreakLabel,wBreakLabel;

    //IlGenerator for & while
    ILGenerator forIlGen,whileIlGen;

    //Variable para saber si estamos dentro de un for o while
    bool onFor = false, onWhile = false;


    
    





    public CodeGeneration()
    {
        metodosGlobales = new List<MethodBuilder>();
        globalVariables = new Dictionary<string, FieldBuilder>();
        localVariables = new Dictionary<string, LocalBuilder>();
        clases = new Dictionary<string, TypeBuilder>();
        classBuilder = null;


        myAsmName.Name = "TestASM";
        myAsmBldr = currentDom.DefineDynamicAssembly(myAsmName, AssemblyBuilderAccess.RunAndSave);
        myModuleBldr = myAsmBldr.DefineDynamicModule(asmFileName);
        myTypeBldr = myModuleBldr.DefineType("Program");

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
            new Type[] { typeof(double) });
        writeMS = typeof(Console).GetMethod(
            "WriteLine",
            new Type[] { typeof(string) });
        writeMC = typeof(Console).GetMethod(
            "WriteLine",
            new Type[] { typeof(char) });
        
        writeMB = typeof(Console).GetMethod(
            "WriteLine",
            new Type[] { typeof(bool) });

    }




    // Método auxiliar para obtener el tipo a partir de una cadena de texto
    private Type GetTypeFromString(string typeName)
    {
        switch (typeName)
        {
            case "int":
                return typeof(double);
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


    public override object VisitProgramAST(MiniCSharpParser.ProgramASTContext context)
    {

        //Visita el identificador del programa
        Visit(context.ident());

        
        //Visita los elementos del programa (clases, métodos, variables)
        foreach (var state in context.children)
        {
            if (state is MiniCSharpParser.ClassDeclASTContext)
            {
                Visit(state);
            }
            else if (state is MiniCSharpParser.MethodDeclASTContext)
            {
                Visit(state);
            }
            else if (state is MiniCSharpParser.VarDeclASTContext)
            {
                Visit(state);
            }
        }


        pointType = myTypeBldr.CreateType(); //crea la clase para ser luego instanciada
        myAsmBldr.SetEntryPoint(pointMainBldr); //se define el punto de entrada
        myAsmBldr.Save(asmFileName); //se guarda el ensamblado en un archivo

        return pointType;
    }

    public override object VisitUsingAST(MiniCSharpParser.UsingASTContext context)
    {
        return base.VisitUsingAST(context);
    }

    public override object VisitVarDeclAST(MiniCSharpParser.VarDeclASTContext context)
    {

        if (isOnClass)
        {
            for (int i = 0; i < context.ident().Length; i++)
            {
                string name = context.ident(i).GetText();
                Type varType = GetTypeFromString((string)Visit(context.type()));

                //guardar como variable local
                FieldBuilder currentFieldBldr =
                    myTypeBldr.DefineField(name, varType, FieldAttributes.Public | FieldAttributes.Static);
                globalVariables.Add(name, currentFieldBldr);

            }
        }
        else
        {
            //Verificar si la variable es local o global
            bool IsLocal = context.isLocal;
            if (IsLocal)
            {

                ILGenerator currentIL = currentMethodBldr.GetILGenerator();

                for (int i = 0; i < context.ident().Length; i++)
                {
                    string name = context.ident(i).GetText();
                    Type varType = GetTypeFromString((string)Visit(context.type()));
                    LocalBuilder localVAR = currentIL.DeclareLocal(varType);
                    localVariables.Add(name, localVAR);
                

                }
            }
            else
            {

                for (int i = 0; i < context.ident().Length; i++)
                {

                    string name = context.ident(i).GetText();
                    Type varType = GetTypeFromString((string)Visit(context.type()));

                    //guardar como variable local
                    FieldBuilder currentFieldBldr =
                        myTypeBldr.DefineField(name, varType, FieldAttributes.Public | FieldAttributes.Static);
                    globalVariables.Add(name, currentFieldBldr);

                }

            }
            
        }
       

        return null;
    }

    public override object VisitClassDeclAST(MiniCSharpParser.ClassDeclASTContext context)
    {
        
        isOnClass = true;

        //Crear un nuevo TypeBuilder para la clase
        classBuilder = myModuleBldr.DefineType(context.ident().GetText(), TypeAttributes.Public);
        
        // Visitar las declaraciones de variables dentro de la clase
        foreach (var var in context.varDecl())
        {
            Visit(var);
        }

        classBuilder.CreateType();
        // Almacenar en la lista de clases
        clases.Add(context.ident().GetText(), classBuilder);

        isOnClass = false;

        return null;

    }

    public override object VisitMethodDeclAST(MiniCSharpParser.MethodDeclASTContext context)
    {
        //Obtiene el tipo del metodo
        Type typeMethod = null;
        if (context.type() != null)
            typeMethod = GetTypeFromString((string)Visit(context.type()));
        else if (context.VOID() != null)
            typeMethod = typeof(void);

        //Se define el método
        currentMethodBldr = myTypeBldr.DefineMethod(context.ident().GetText(),
            MethodAttributes.Public |
            MethodAttributes.Static,
            typeMethod,
            null); //los parámetros son null porque se tiene que visitar despues de declarar el método

        //se visitan los parámetros para definir el arreglo de tipos de cada uno de los parámetros formales si es que hay (not null)
        Type[] parameters = null;
        if (context.formPars() != null)
            parameters = (Type[])Visit(context.formPars());

        //después de visitar los parámetros, se cambia el signatura que requiere la definición del método
        currentMethodBldr.SetParameters(parameters);

        //se visita el cuerpo del método para generar el código que llevará el "currentMethodBldr" de turno
        Visit(context.block());

        ILGenerator currentIL = currentMethodBldr.GetILGenerator();
        currentIL.Emit(OpCodes.Ret);

        //Se agrega el método recién creado a la lista de mpetodos globales para no perder su referencia cuando se creen más métodos
        metodosGlobales.Add(currentMethodBldr);
        if (context.ident().GetText().Equals("Main"))
        {
            //el puntero al metodo principal se setea cuando es el Main quien se declara
            pointMainBldr = currentMethodBldr;
        }


        return null;
    }

    public override object VisitFormParsAST(MiniCSharpParser.FormParsASTContext context)
    {
        //Se obtiene el generador de IL del método actual
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
        return context.ident().GetText();
    }

    public override object VisitAssignStatementAST(MiniCSharpParser.AssignStatementASTContext context)
    {

        //Se obtiene el generador de IL del método actual
        ILGenerator currentIL = currentMethodBldr.GetILGenerator();
        
        //obtiene el nombre del designator
        string name = context.designator().GetText();
        
        
        if (context.expr() != null)
        {
            if (localVariables.ContainsKey(name))
            {
                //se obtiene el localbuilder de la variable local
                LocalBuilder local = localVariables[name];
                //se visita la expresión para generar el bytecode correspondiente (QUEDARÁ EN EL TOPE DE LA PILA EL VALOR A ASIGNAR)
                Visit(context.expr());
                //se guarda el valor en la variable local
                currentIL.Emit(OpCodes.Stloc, local.LocalIndex);
                
                
            }
            else if (globalVariables.ContainsKey(name))
            {
                //se obtiene el fieldbuilder de la variable global
                FieldBuilder global = globalVariables[name];
                //se visita la expresión para generar el bytecode correspondiente (QUEDARÁ EN EL TOPE DE LA PILA EL VALOR A ASIGNAR)
                Visit(context.expr());
                //se guarda el valor en la variable global
                currentIL.Emit(OpCodes.Stsfld, global);
            }
            
        }
        
        else if (context.INC() !=null || context.DEC() !=null)
        {
            if (localVariables.ContainsKey(name))
            {
                //se obtiene el localbuilder de la variable local
                LocalBuilder local = localVariables[name];
                //se visita la designator para generar el bytecode correspondiente (QUEDARÁ EN EL TOPE DE LA PILA EL VALOR A ASIGNAR)
                Visit(context.designator());
                
                //se carga un uno en la pila
                currentIL.Emit(OpCodes.Ldc_R8, 1.0);
                
                //se hace la operacione en la pila ya sea restar o sumar
                if (context.INC() != null)
                    currentIL.Emit(OpCodes.Add);
                else 
                    currentIL.Emit(OpCodes.Sub);
                
                
                //se guarda el valor en la variable local
                currentIL.Emit(OpCodes.Stloc, local.LocalIndex);
          


            }
            else if (globalVariables.ContainsKey(name))
            {
                //se obtiene el fieldbuilder de la variable global
                FieldBuilder global = globalVariables[name];
                //se visita la designator para generar el bytecode correspondiente (QUEDARÁ EN EL TOPE DE LA PILA EL VALOR A ASIGNAR)
                Visit(context.designator());
                
                //se carga un uno en la pila
                currentIL.Emit(OpCodes.Ldc_R8, 1.0);
                
                
                if (context.INC() != null)
                    currentIL.Emit(OpCodes.Add);
                else 
                    currentIL.Emit(OpCodes.Sub);
                
                
                //se guarda el valor en la variable global
                currentIL.Emit(OpCodes.Stsfld, global);
                
                
            }
            
            
        }

        return null;
    }

    public override object VisitIfStatementAST(MiniCSharpParser.IfStatementASTContext context)
    {
        
        // If statement
        ILGenerator currentIL = currentMethodBldr.GetILGenerator();

        
        //Definimos la variables de retorno en caso de que venga un break en el statement



        // Visita la condición 
        Visit(context.condition());

        // Define la etiqueta para el salto al bloque de código dentro del if
        Label ifLabel = currentIL.DefineLabel();
        currentIL.Emit(OpCodes.Brfalse, ifLabel);
        

        // Visita el bloque dentro del if
           Visit(context.statement(0));
            

        //si esxiste un else
        if (context.ELSE() != null)
        {
            //define la etiqueta para el salto al bloque de código dentro del else
            Label elseLabel = currentIL.DefineLabel();
            currentIL.Emit(OpCodes.Br, elseLabel);

            // Marcar la etiqueta para que sepa donde debe saltar si no se cumple el if
            currentIL.MarkLabel(ifLabel);

            // Visitar el statement del else
           Visit(context.statement(1));
           

            // Marcar la etiqueta para el final del bloque de código else
            currentIL.MarkLabel(elseLabel);
        }
        else
        {
           
            // Marcar la etiqueta para el final del bloque de código del if
            currentIL.MarkLabel(ifLabel);
        }

        return null;
    }

    public override object VisitForStatementAST(MiniCSharpParser.ForStatementASTContext context)
    {
        
        onFor = true;
        forIlGen = currentMethodBldr.GetILGenerator();

        Label loopLabel = forIlGen.DefineLabel();

        fBreakLabel = forIlGen.DefineLabel();

        forIlGen.MarkLabel(loopLabel);

        if (context.statement().Length == 2)
        {
           Visit(context.statement(1));
            
           Visit(context.statement(0));
            
        }
        else
        {
            Visit(context.statement(0));
        }
        
        Visit(context.condition());
      
        forIlGen.Emit(OpCodes.Brfalse, fBreakLabel);
        
        forIlGen.Emit(OpCodes.Br, loopLabel);
    
        // Marcar la etiqueta de salida del bucle while
        forIlGen.MarkLabel(fBreakLabel);
        
        onFor = false;
        return null;
    }

    public override object VisitWhileStatementAST(MiniCSharpParser.WhileStatementASTContext context)
    {
        onWhile = true;
        // Obtener el ILGenerator del método actual
        whileIlGen = currentMethodBldr.GetILGenerator();
    
        // Definir la etiqueta para el bucle while
        Label loopLabel = whileIlGen.DefineLabel();
        whileIlGen.MarkLabel(loopLabel);
    
        // Visitar la condición del bucle while
        Visit(context.condition());
    
        // Definir la etiqueta de salida del bucle while
        wBreakLabel = whileIlGen.DefineLabel();

        whileIlGen.Emit(OpCodes.Brfalse, wBreakLabel);
    
        // Visitar el statement del bucle while
       Visit(context.statement());
    
       // Volver al inicio del bucle while
        whileIlGen.Emit(OpCodes.Br, loopLabel);
    
        // Marcar la etiqueta de salida del bucle while
        whileIlGen.MarkLabel(wBreakLabel);
             
        onWhile = false;
        return null;
    }

    public override object VisitBreakStatementAST(MiniCSharpParser.BreakStatementASTContext context)
    {
        
        if (onFor)
        {
            forIlGen.Emit(OpCodes.Br, fBreakLabel);
        }

        if (onWhile)
        {
            whileIlGen.Emit(OpCodes.Br, wBreakLabel);
        }

        return null;
    }

    public override object VisitReturnStatementAST(MiniCSharpParser.ReturnStatementASTContext context)
    {
        // return statement
        // Verificar si hay una expresión de retorno
        if (context.expr() != null)
        {
            // Visitar la expresión de retorno
            Visit(context.expr());
        }
    
        // Obtener el generador de código IL actual
        ILGenerator currentIL = currentMethodBldr.GetILGenerator();
    
        // Emitir la instrucción de retorno
        currentIL.Emit(OpCodes.Ret);
    
        return null;
    }

    public override object VisitReadStatementAST(MiniCSharpParser.ReadStatementASTContext context)
    {
        return base.VisitReadStatementAST(context);
    }

    public override object VisitWriteStatementAST(MiniCSharpParser.WriteStatementASTContext context)
    {
        // Obtener el generador de código IL actual
        ILGenerator currentIL = currentMethodBldr.GetILGenerator();
        

        Type expressionType = (Type) Visit(context.expr());
        
        System.Diagnostics.Debug.WriteLine("Tipo de expresión: " + expressionType);
        
       
        if (expressionType == typeof(double)) // Tanto para int como para double
        {
            currentIL.EmitCall(OpCodes.Call, writeMI, null);
        }
        else if (expressionType == typeof(string)) // Para string
        {
            currentIL.EmitCall(OpCodes.Call, writeMS, null);
        }
        else if (expressionType == typeof(char)) // Para char
        {
            currentIL.EmitCall(OpCodes.Call, writeMC, null);
        }
        else if (expressionType == typeof(bool)) // Para bool
        {
            currentIL.EmitCall(OpCodes.Call, writeMB, null);
        }

        return null;
   
    }

    public override object VisitBlockStatementAST(MiniCSharpParser.BlockStatementASTContext context)
    {
        
        string result = (string) Visit(context.block());

        if (result!=null && result.Equals("break"))
        {
            return "break";
        }
        return null;
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
        String result = "";
        foreach (var node in context.children)
        {
            result = (string)Visit(node);
            if (result != null && result.Equals("break"))
            {
                return "break";
            }
        }

        return null;
    }

    public override object VisitActParsAST(MiniCSharpParser.ActParsASTContext context)
    {
        
        for (int i = 0; context.expr().Length > i; i++)
        {
            Visit(context.expr(i));
        }
        isArgument = false;
        return null;
    }

    public override object VisitConditionAST(MiniCSharpParser.ConditionASTContext context)
    {
        Visit(context.condTerm(0));
        for (int i = 1; i < context.condTerm().Length; i++)
        {
            Visit(context.condTerm(i));
        }
        return null;
    }

    public override object VisitCondTermAST(MiniCSharpParser.CondTermASTContext context)
    {
        Visit(context.condFact(0));
        for (int i = 1; i < context.condFact().Length; i++)
        {
            Visit(context.condFact(i));
        }
        return null;
    }

    public override object VisitCondFactAST(MiniCSharpParser.CondFactASTContext context)
    {
        Visit(context.expr(0));
        Visit(context.expr(1));
        Visit(context.relop());
        
        return null;
    }

    public override object VisitCastAST(MiniCSharpParser.CastASTContext context)
    {
        return base.VisitCastAST(context);
    }

    public override object VisitExpressionAST(MiniCSharpParser.ExpressionASTContext context)
    {

        Type typeTerm = null;

        if (context.cast() is null)
        {
            
            typeTerm = (Type)Visit(context.term(0));

            if (context.term().Length > 1)
            {
                ILGenerator currentIL = currentMethodBldr.GetILGenerator();
                for (int i = 1; context.term().Length > i; i++)
                {
                    Visit(context.term(i));
                    Visit(context.addop(i - 1));

                }
            }

        }

        return typeTerm;
    }

    public override object VisitAddopAST(MiniCSharpParser.AddopASTContext context)
    {
        ILGenerator currentIL = currentMethodBldr.GetILGenerator();
        if (context.ADD() != null)
            currentIL.Emit(OpCodes.Add);
        else if (context.SUB() != null)
            currentIL.Emit(OpCodes.Sub);

        return null;

    }

    public override object VisitTermAST(MiniCSharpParser.TermASTContext context)
    {

        Type typeFact = (Type)Visit(context.factor(0));
        if (context.factor().Length>1)
        {
            for (int i = 1; i < context.factor().Length; i++)
            {
                Visit(context.factor(i));
                Visit(context.muldimod(i-1));
            }
        }
        
        return typeFact;
    }

    public override object VisitMuldimodAST(MiniCSharpParser.MuldimodASTContext context)
    {
        ILGenerator currentIL = currentMethodBldr.GetILGenerator();
        if (context.MUL() != null)
            currentIL.Emit(OpCodes.Mul);
        else if (context.DIV() != null)
            currentIL.Emit(OpCodes.Div);
        else if (context.MOD() != null)
        {
            currentIL.Emit(OpCodes.Rem);
        }
        
        return null;
    }

    public override object VisitFactorAST(MiniCSharpParser.FactorASTContext context)
        {
            //factor
            Type typeDesignator = (Type)Visit(context.designator());

            if (context.actPars() != null)
            {
                Visit(context.actPars());
            }
            
            return typeDesignator;
        }

        public override object VisitNumFactorAST(MiniCSharpParser.NumFactorASTContext context)
        {
            
            ILGenerator currentIL = currentMethodBldr.GetILGenerator();

            //se obtiene el valor del entero y se le agrega el .0 para que sea double
            string intEntry = context.INTCONST().GetText() +".0";
             intEntry= intEntry.Replace(".", ",");
            double OutVal;
            double.TryParse(intEntry, out OutVal);
            //se agrega el valor a la pila
            currentIL.Emit(OpCodes.Ldc_R8 , OutVal);

            return typeof(double);
            
            
        }

        public override object VisitCharFactorAST(MiniCSharpParser.CharFactorASTContext context)
        {
            //se instancia el generador de codigo
            ILGenerator currentIL = currentMethodBldr.GetILGenerator();
        
            //se agrega el valor a la pila
            currentIL.Emit(OpCodes.Ldc_I4_S, context.CHARCONST().GetText()[1]);

            return typeof(char);
           
        }

        public override object VisitStringFactorAST(MiniCSharpParser.StringFactorASTContext context)
        {
            ILGenerator currentIL = currentMethodBldr.GetILGenerator();
            
            //se agrega el valor a la pila
            currentIL.Emit(OpCodes.Ldstr, context.STRINGCONST().GetText());

            return typeof(string);
          
        }

        public override object VisitDoubleFactorAST(MiniCSharpParser.DoubleFactorASTContext context)
        {
            
            
            //se instancia el generador de codigo
            ILGenerator currentIL = currentMethodBldr.GetILGenerator();

            //se obtiene el valor del double 
            string doubleEntry = context.DOUBLECONST().GetText();
            //se reemplaza el . por , para que el double sea valido el parseo
            doubleEntry= doubleEntry.Replace(".", ",");
            double convertedDouble;
            double.TryParse(doubleEntry, out convertedDouble);
            //se agrega el valor a la pila
            currentIL.Emit(OpCodes.Ldc_R8 , convertedDouble);
            
            return typeof(double);
            
        }

        public override object VisitBooleanFactorAST(MiniCSharpParser.BooleanFactorASTContext context)
        {
            
            //se instancia el generador de codigo
            ILGenerator currentIL = currentMethodBldr.GetILGenerator();
            int num;
            //en caso de que venga un true
            if (context.TrueCONST() != null)
            {
                num = context.TrueCONST().GetText() == "true" ? 1 : 0;
            }
            else//en caso de que sea false
            { num = 0;
            }

            //se agrega el valor a la pila como entero 1 o 0
            currentIL.Emit(OpCodes.Ldc_I4, num);
        
            return typeof(bool);
            
            
   
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

            //visitdesignator
            ILGenerator currentIL = currentMethodBldr.GetILGenerator();


            if (context.LBRACK() != null)
            {
                foreach (var xp in context.expr())
                {
                    Visit(xp);
                }
            }

            //indicador de para saber si es variable local o global
            bool flag = false;
            
            //cuando solo viene un id
            if (context.ident().Length == 1)
            {
                //se verifica si es local
                if (localVariables.ContainsKey(context.ident(0).GetText()))
                {
                    //se agrega el valor a la pila
                    currentIL.Emit(OpCodes.Ldloc, localVariables[context.ident(0).GetText()]);
                    //cambia el indicador a true es decir es local
                    flag = true;
                }
                else
                {
                    //se agrega el valor a la pila
                    currentIL.Emit(OpCodes.Ldsfld, globalVariables[context.ident(0).GetText()]);
                }
            }
            
            //retorna el tipo de la variable local o global
            return flag ? localVariables[context.ident(0).GetText()].LocalType : globalVariables[context.ident(0).GetText()].FieldType;
        }

        public override object VisitIdentAST(MiniCSharpParser.IdentASTContext context)
        {
            return context.ID().GetText();
        }

        public override object VisitRelopAST(MiniCSharpParser.RelopASTContext context)
        {
            ILGenerator currentIL = currentMethodBldr.GetILGenerator();
            if (context.LT() != null)
                currentIL.Emit(OpCodes.Clt);
            
            else if (context.NOTEQUAL() != null)
            {
                currentIL.Emit(OpCodes.Ceq);
                currentIL.Emit(OpCodes.Ldc_I4_0);
                currentIL.Emit(OpCodes.Ceq);
            
            }
            else if(context.EQUAL() !=null)
            {
                currentIL.Emit(OpCodes.Ceq);
            }
            
            else if (context.LE() != null)
            {
                currentIL.Emit(OpCodes.Cgt);
                currentIL.Emit(OpCodes.Ldc_I4_0);
                currentIL.Emit(OpCodes.Ceq);
            }
            else if (context.GE() != null)
            {
                currentIL.Emit(OpCodes.Clt);
                currentIL.Emit(OpCodes.Ldc_I4_0);
                currentIL.Emit(OpCodes.Ceq);
            }
            else if(context.GT()!=null)
                currentIL.Emit(OpCodes.Cgt);
            
            
            return null;
        }
}