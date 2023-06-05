using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

using Compilator;

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

    private MethodInfo writeIntDouble, writeString, writeChar, writeBoolean;

    private MethodBuilder pointMainBldr, currentMethodBldr;

    private List<MethodBuilder> globalMethods;

    private bool isArgument = false;

    //Diccionarios para guardar las variables locales y globales
    private Dictionary<string, FieldBuilder> globalVariables;
    private Dictionary<string, LocalBuilder> localVariables;
    
    
    //Diccionario para guardar clases
    private Dictionary<string, TypeBuilder> classesList;
    
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
        
        //Inicializacion de las lista y diccionarios
        globalMethods = new List<MethodBuilder>();
        globalVariables = new Dictionary<string, FieldBuilder>();
        localVariables = new Dictionary<string, LocalBuilder>();
        classesList = new Dictionary<string, TypeBuilder>();


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

        

        writeIntDouble = typeof(Console).GetMethod(
            "WriteLine",
            new Type[] { typeof(double) });
        writeString = typeof(Console).GetMethod(
            "WriteLine",
            new Type[] { typeof(string) });
        writeChar = typeof(Console).GetMethod(
            "WriteLine",
            new Type[] { typeof(char) });
        
        writeBoolean = typeof(Console).GetMethod(
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


        pointType = myTypeBldr.CreateType();//se crea el tipo
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
        
        //Verificar si la variable esta dentro de una clase para guardarla localmente
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
            
            //Si es global
            if (!IsLocal)
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
            else //Si es local
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
            
        }
       

        return null;
    }

    public override object VisitClassDeclAST(MiniCSharpParser.ClassDeclASTContext context)
    {
        //Indicar que se esta dentro de una clase
        isOnClass = true;

        //Crear un nuevo TypeBuilder para la clase
        classBuilder = myModuleBldr.DefineType(context.ident().GetText(), TypeAttributes.Public);
        
        // Visitar las declaraciones de variables dentro de la clase
        foreach (var var in context.varDecl())
        {
            Visit(var);
        }

        //Crea
        classBuilder.CreateType(); //Crea un objeto Type para esta clase. Después de definir los campos y métodos en la clase,
                                   //se llama a CreateType para cargar su objeto Type.
        
        // Almacenar en la lista de clases
        classesList.Add(context.ident().GetText(), classBuilder);

        
        //Indicar que se salio de la clase
        isOnClass = false;

        return null;

    }

    public override object VisitMethodDeclAST(MiniCSharpParser.MethodDeclASTContext context)
    {
        //Define el tipo del metodo
        Type typeMethod = null;
        
        
        //Se obtiene el tipo del método
        if (context.type() != null)
            typeMethod = GetTypeFromString((string)Visit(context.type()));
        
        //Se establece si es de tipo void
        else if (context.VOID() != null)
            typeMethod = typeof(void);

        //Se define el método
        currentMethodBldr = myTypeBldr.DefineMethod(context.ident().GetText(),
            MethodAttributes.Public |
            MethodAttributes.Static,
            typeMethod,
            null); //los parámetros son null porque se tiene que visitar despues de declarar el método

        
        
        //se visitan los parámetros para definir el arreglo de tipos de cada uno de los parámetros 
        Type[] parameters = null;
        if (context.formPars() != null)
            parameters = (Type[])Visit(context.formPars());

        //después de visitar los parámetros, se establecen
        currentMethodBldr.SetParameters(parameters);

        //se visita el cuerpo del método
        Visit(context.block());

        //Se obtiene el generador de IL del método actual
        ILGenerator currentIL = currentMethodBldr.GetILGenerator();
        currentIL.Emit(OpCodes.Ret);

        //Se agrega el método recién creado a la lista de mpetodos globales para no perder su referencia cuando se creen más métodos
        globalMethods.Add(currentMethodBldr);
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
        //Se obtiene el tipo de la variable
        return context.ident().GetText();
    }

    public override object VisitAssignStatementAST(MiniCSharpParser.AssignStatementASTContext context)
    {

        //Se obtiene el generador de IL del método actual
        ILGenerator currentIL = currentMethodBldr.GetILGenerator();
        
        //obtiene el nombre del designator
        string name = context.designator().GetText();
        
        
        //Si es una asignacion
        if (context.expr() != null)
        {
            //Si es una variable local
            if (localVariables.ContainsKey(name))
            {
                //se obtiene el localbuilder de la variable local que contiene el indice de declaracion
                LocalBuilder local = localVariables[name];
                //se visita la expresión para generar el bytecode correspondiente (queda el valor de la expresion arriba de la pila)
                Visit(context.expr());
                //se guarda el valor en la variable local
                currentIL.Emit(OpCodes.Stloc, local.LocalIndex);
                
                
            }
            //Si es una variable global
            else if (globalVariables.ContainsKey(name))
            {
                //se obtiene el fieldbuilder de la variable global que contiene el indice de declaracion
                FieldBuilder global = globalVariables[name];
                //se visita la expresión para generar el bytecode correspondiente (queda el valor de la expresion arriba de la pila)
                Visit(context.expr());
                //se guarda el valor en la variable global
                currentIL.Emit(OpCodes.Stsfld, global);
            }
            
        }
        
        //Si es un incremento o decremento
        else if (context.INC() !=null || context.DEC() !=null)
        {
            //Si es una variable local
            if (localVariables.ContainsKey(name))
            {
                //se obtiene el localbuilder de la variable local
                LocalBuilder local = localVariables[name];
                //se visita la expresión para generar el bytecode correspondiente (queda el valor de la expresion arriba de la pila)
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
            //Si es una variable global
            else if (globalVariables.ContainsKey(name))
            {
                //se obtiene el fieldbuilder de la variable global
                FieldBuilder global = globalVariables[name];
                
                //se visita la expresión para generar el bytecode correspondiente (queda el valor de la expresion arriba de la pila)
                Visit(context.designator());
                
                //se carga un uno en la pila
                currentIL.Emit(OpCodes.Ldc_R8, 1.0);
                
                //se hace la operacione en la pila ya sea restar o sumar
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
        
        //Se obtiene el generador de IL del método actual
        ILGenerator currentIL = currentMethodBldr.GetILGenerator();
        
        

        // Visita la condición 
        Visit(context.condition());

        // Define la etiqueta para para saltar depues de if en caso de que no se cumpla la condicion
        Label endIfLabel = currentIL.DefineLabel();
        currentIL.Emit(OpCodes.Brfalse, endIfLabel);
        

        // Visita el bloque dentro del if
        Visit(context.statement(0));
            

        //en caso de que exista un else
        if (context.ELSE() != null)
        {
            //define la etiqueta para el salto al bloque de código dentro del else
            Label elseLabel = currentIL.DefineLabel();
            currentIL.Emit(OpCodes.Br, elseLabel);

            // Marcar la etiqueta para que sepa donde debe saltar si no se cumple el if
            currentIL.MarkLabel(endIfLabel);

            // Visitar el statement del else
           Visit(context.statement(1));
           

            // Marcar la etiqueta para el final del bloque de código else
            currentIL.MarkLabel(elseLabel);
        }
        else
        {
           
            // Marcar la etiqueta para el final del bloque de código del if
            currentIL.MarkLabel(endIfLabel);
        }

        return null;
    }

    public override object VisitForStatementAST(MiniCSharpParser.ForStatementASTContext context)
    {
        //Establecer que se esta en un for
        onFor = true;
        // Obtener el ILGenerator del método actual
        forIlGen = currentMethodBldr.GetILGenerator();

        //Se define la etiqueta del for para que haga un ciclo
        Label forLabel = forIlGen.DefineLabel();

        //Se define la etiqueta para que salga del for
        fBreakLabel = forIlGen.DefineLabel();

        //Se marca donde esta la etiqueta del for
        forIlGen.MarkLabel(forLabel);

        //Se visita los statements del for en caso de que vengan ambos
        if (context.statement().Length == 2)
        {
           Visit(context.statement(1));
            
           Visit(context.statement(0));
            
        }
        else //si solo viene un statement
        {
            Visit(context.statement(0));
        }
        
        // Visitar la condición del bucle for
        Visit(context.condition());
      
        // Saltar a la etiqueta del de final del for si la condición no se cumple
        forIlGen.Emit(OpCodes.Brfalse, fBreakLabel);
        
        // Saltar a la etiqueta del for para repetir el ciclo
        forIlGen.Emit(OpCodes.Br, forLabel);
    
        // Marcar la etiqueta de salida del bucle for
        forIlGen.MarkLabel(fBreakLabel);
        
        //Se establece que ya no se esta en un for
        onFor = false;
        return null;
    }

    public override object VisitWhileStatementAST(MiniCSharpParser.WhileStatementASTContext context)
    {
        
        //Establecer que se esta en un while
        onWhile = true;
        
        
        // Obtener el ILGenerator del método actual
        whileIlGen = currentMethodBldr.GetILGenerator();
    
        // Definir la etiqueta para el bucle while
        Label whileLoopLabel = whileIlGen.DefineLabel();
        whileIlGen.MarkLabel(whileLoopLabel);
    
        // Visitar la condición del bucle while
        Visit(context.condition());
    
        // Definir la etiqueta de salida del bucle while
        wBreakLabel = whileIlGen.DefineLabel();

        // Saltar a la etiqueta de salida del bucle while si la condición no se cumple
        whileIlGen.Emit(OpCodes.Brfalse, wBreakLabel);
    
        // Visitar el statement del bucle while
       Visit(context.statement());
    
       // Volver al inicio del bucle while
        whileIlGen.Emit(OpCodes.Br, whileLoopLabel);
    
        // Marcar la etiqueta de salida del bucle while
        whileIlGen.MarkLabel(wBreakLabel);
             
        //Se establece que ya no se esta en un while
        onWhile = false;
        return null;
    }

    public override object VisitBreakStatementAST(MiniCSharpParser.BreakStatementASTContext context)
    {
        //si esta dentro de un for
        if (onFor)
        {
            //Salta a la etiqueta de salida del for
            forIlGen.Emit(OpCodes.Br, fBreakLabel);
        }
        //si esta dentro de un while
        if (onWhile)
        {
            //Salta a la etiqueta de salida del while
            whileIlGen.Emit(OpCodes.Br, wBreakLabel);
        }

        return null;
    }

    public override object VisitReturnStatementAST(MiniCSharpParser.ReturnStatementASTContext context)
    {

        // Verificar si hay una expresión de retorno
        if (context.expr() != null)
        {
            // Visitar la expresión de retorno para que quede el valor en la pila
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
        //read statement
        ILGenerator currentIL = currentMethodBldr.GetILGenerator();
        
        //Nombre del designator
        string name = context.designator().GetText();
        
        //Verificamos el tipo de la variable ingresada desde las variables locales del contexto
        Type type = context.typeVInput;
        
        if (type == typeof(int))
        {
            //se obtiene el valor de entrada del input se le agrega .0 para que sea double ya que da problemas al imprimir
            string intEntry = context.valueInput +".0";
            //se reemplaza el punto por una coma
            intEntry= intEntry.Replace(".", ",");
            //se convierte a double
            double.TryParse(intEntry, out double valorSalida);
            //se agrega el valor a la pila
            currentIL.Emit(OpCodes.Ldc_R8 , valorSalida);
            
            //se obtiene el localbuilder de la variable local
            LocalBuilder local = localVariables[name];
           
            //se guarda el valor en la variable local
            currentIL.Emit(OpCodes.Stloc, local.LocalIndex);
            
        }
        else if (type == typeof(string))
        {
            //se carga a la pila el valor de entrada del input
            currentIL.Emit(OpCodes.Ldstr, context.valueInput);
            
            //se obtiene el localbuilder de la variable local
            LocalBuilder local = localVariables[name];
           
            //se guarda el valor en la variable local
            currentIL.Emit(OpCodes.Stloc, local.LocalIndex);
        }
        else if (type == typeof(double))
        {
            
            //se obtiene el valor de entrada del input
            string doubleEntry = context.valueInput;
            //se reemplaza el . por , para que el double sea valido el parseo
            doubleEntry= doubleEntry.Replace(".", ",");
           
            double.TryParse(doubleEntry, out double convertedDouble);
            //se agrega el valor a la pila
            currentIL.Emit(OpCodes.Ldc_R8 , convertedDouble);
            
            //se obtiene el localbuilder de la variable local
            LocalBuilder local = localVariables[name];
           
            //se guarda el valor en la variable local
            currentIL.Emit(OpCodes.Stloc, local.LocalIndex);
            
            
        }
        else if (type == typeof(bool))
        {
       
            //se obtiene el valor de entrada 0 false 1 true
            int num = context.valueInput == "false" ? 0 : 1;

            //se agrega el valor a la pila como entero 1 o 0
            currentIL.Emit(OpCodes.Ldc_I4, num);
            
            //se obtiene el localbuilder de la variable local
            LocalBuilder local = localVariables[name];
           
            //se guarda el valor en la variable local
            currentIL.Emit(OpCodes.Stloc, local.LocalIndex);
        }
        else if (type == typeof(char))
        {
            
            //lo convierte a char ya que viene desde los locals como un string
            char convertedChar = char.TryParse( context.valueInput, out convertedChar) ? convertedChar : ' ';
            
            //se agrega el valor a la pila como un entero equivalente al char
            currentIL.Emit(OpCodes.Ldc_I4_S, convertedChar);
            
            //se obtiene el localbuilder de la variable local
            LocalBuilder local = localVariables[name];
            
            //se guarda el valor en la variable local
            currentIL.Emit(OpCodes.Stloc, local.LocalIndex);
            
            
        }

        return null;
    }

    public override object VisitWriteStatementAST(MiniCSharpParser.WriteStatementASTContext context)
    {
        // Obtener el generador de código IL actual
        ILGenerator currentIL = currentMethodBldr.GetILGenerator();
        

        // Obtener el tipo de la expresión
        Type expressionType = (Type) Visit(context.expr());
        
        
        
        // En caso de que la expresion sea un double que puede ser entero o double ya que si imprimimos como int da problemas
        if (expressionType == typeof(double)) 
        {
            currentIL.EmitCall(OpCodes.Call, writeIntDouble, null);
        }
        //En caso de que la expresion sea un booleano
        else if (expressionType == typeof(bool)) 
        {
            currentIL.EmitCall(OpCodes.Call, writeBoolean, null);
        }
        //En caso de que la expresion sea un char
        else if (expressionType == typeof(char)) 
        {
            currentIL.EmitCall(OpCodes.Call, writeChar, null);
        }
        // En caso de que la expresion sea un string
        else if (expressionType == typeof(string)) 
        {
            currentIL.EmitCall(OpCodes.Call, writeString, null);
        }
        
        
        return null;
   
    }

    public override object VisitBlockStatementAST(MiniCSharpParser.BlockStatementASTContext context)
    {
        //Visitar el bloque
       Visit(context.block());
       
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
     
        //Visitar cada sentencia del bloque
        foreach (var node in context.children)
        {
           Visit(node);
            
        }

        return null;
    }

    public override object VisitActParsAST(MiniCSharpParser.ActParsASTContext context)
    {
        //Visitar cada expresión
        for (int i = 0; context.expr().Length > i; i++)
        {
            Visit(context.expr(i));
        }
        //Se establece que no es un argumento
        isArgument = false;
        return null;
    }

    public override object VisitConditionAST(MiniCSharpParser.ConditionASTContext context)
    {
        //Visitar cada termino de la condicion
        Visit(context.condTerm(0));
        for (int i = 1; i < context.condTerm().Length; i++)
        {
            Visit(context.condTerm(i));
        }
        return null;
    }

    public override object VisitCondTermAST(MiniCSharpParser.CondTermASTContext context)
    {
        //se visita cada factor de la condicion
        Visit(context.condFact(0));
        for (int i = 1; i < context.condFact().Length; i++)
        {
            Visit(context.condFact(i));
        }
        return null;
    }

    public override object VisitCondFactAST(MiniCSharpParser.CondFactASTContext context)
    {
        //se visita la primera expresion, luego la segunda y se aplica el operador
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
        //Se define el tipo de la expresion
        Type typeTerm = null;

        if (context.cast() is null)
        {
            //Se visita el termino
            typeTerm = (Type)Visit(context.term(0));

            
            //En caso de que la expresion sea compleja
            if (context.term().Length > 1)
            {
             
                for (int i = 1; context.term().Length > i; i++)
                {
                    //Se visita el termino y luego se aplica el operador
                    Visit(context.term(i));
                    Visit(context.addop(i - 1));

                }
            }

        }
        
        return typeTerm;
    }

    public override object VisitAddopAST(MiniCSharpParser.AddopASTContext context)
    {
        //Se obtiene el generador de codigo IL actual
        ILGenerator currentIL = currentMethodBldr.GetILGenerator();
        //Es una suma
        if (context.ADD() != null)
            currentIL.Emit(OpCodes.Add);
        //Es una resta
        else if (context.SUB() != null)
            currentIL.Emit(OpCodes.Sub);

        return null;

    }

    public override object VisitTermAST(MiniCSharpParser.TermASTContext context)
    {

        //Se obtiene el tipo del factor
        Type typeFactor = (Type)Visit(context.factor(0));
        //En caso de que la expresion sea compleja
        if (context.factor().Length>1)
        {
            for (int i = 1; i < context.factor().Length; i++)
            {
                //Se visita el factor y luego se aplica el operador
                Visit(context.factor(i));
                Visit(context.muldimod(i-1));
            }
        }
        
        return typeFactor;
    }

    public override object VisitMuldimodAST(MiniCSharpParser.MuldimodASTContext context)
    {
        //Se obtiene el generador de codigo IL actual
        ILGenerator currentIL = currentMethodBldr.GetILGenerator();
        //Es una multiplicacion
        if (context.MUL() != null)
            currentIL.Emit(OpCodes.Mul);
        //Es un modulo
        else if (context.MOD() != null)
        {
            currentIL.Emit(OpCodes.Rem);
        }
        //Es una division
        else if (context.DIV() != null)
            currentIL.Emit(OpCodes.Div);
        
        return null;
    }

    public override object VisitFactorAST(MiniCSharpParser.FactorASTContext context)
        {
            //Se obtiene el tipo del designador
            Type typeDsg = (Type)Visit(context.designator());

            //se visitan los parametros
            if (context.actPars() != null)
            {
                Visit(context.actPars());
            }
            
            return typeDsg;
        }

        public override object VisitNumFactorAST(MiniCSharpParser.NumFactorASTContext context)
        {
            //se instancia el generador de codigo
            ILGenerator currentIL = currentMethodBldr.GetILGenerator();

            //se obtiene el valor del entero y se le agrega el .0 para que sea double y no de problemas de impresion
            string intEntry = context.INTCONST().GetText() +".0";
             intEntry= intEntry.Replace(".", ",");
            //se convierte el valor a double
             double.TryParse(intEntry, out double intConverted);
            //se agrega el valor a la pila
            currentIL.Emit(OpCodes.Ldc_R8 , intConverted);

            return typeof(double);
            
            
        }

        public override object VisitCharFactorAST(MiniCSharpParser.CharFactorASTContext context)
        {
            //se instancia el generador de codigo
            ILGenerator currentIL = currentMethodBldr.GetILGenerator();
        
            //se agrega el valor a la pila del char
            currentIL.Emit(OpCodes.Ldc_I4_S, context.CHARCONST().GetText()[1]);

            return typeof(char);
           
        }

        public override object VisitStringFactorAST(MiniCSharpParser.StringFactorASTContext context)
        {
            //se instancia el generador de codigo
            ILGenerator currentIL = currentMethodBldr.GetILGenerator();
            
            //se agrega el valor a la pila del string
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
         
            //se convierte el valor a double
            double.TryParse(doubleEntry, out double convertedDouble);
            //se agrega el valor a la pila
            currentIL.Emit(OpCodes.Ldc_R8 , convertedDouble);
            
            return typeof(double);
            
        }

        public override object VisitBooleanFactorAST(MiniCSharpParser.BooleanFactorASTContext context)
        {
            
            //se instancia el generador de codigo
            ILGenerator currentIL = currentMethodBldr.GetILGenerator();
            //se declara el valor del boolean
            int flag;
            //en caso de que venga un true
            if (context.TrueCONST() != null)
            {
                flag = 1 ;
            }
            else//en caso de que sea false
            { flag = 0;
            }

            //se agrega el valor a la pila como entero 1 o 0
            currentIL.Emit(OpCodes.Ldc_I4, flag);
        
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

            //se obtiene el generador de codigo
            ILGenerator currentIL = currentMethodBldr.GetILGenerator();

            //En caso de que sea un arreglo
            if (context.LBRACK() != null)
            {
                foreach (var xp in context.expr())
                {
                    Visit(xp);
                }
            }

            //indicador de para saber si es variable local o global

            
            //cuando solo viene un id
            if (context.ident().Length == 1)
            {
                //se verifica si es local
                if (localVariables.ContainsKey(context.ident(0).GetText()))
                {
                    //se agrega el valor a la pila
                    currentIL.Emit(OpCodes.Ldloc, localVariables[context.ident(0).GetText()]);
                    //cambia el indicador a true es decir es local
     
                    //se retorna el tipo de la variable
                    return localVariables[context.ident(0).GetText()].LocalType;
                }
                else
                {
                    //se agrega el valor a la pila
                    currentIL.Emit(OpCodes.Ldsfld, globalVariables[context.ident(0).GetText()]);
                    
                    //se retorna el tipo de la variable
                    return globalVariables[context.ident(0).GetText()].FieldType;
                }
            }


            return null;
        }

        public override object VisitIdentAST(MiniCSharpParser.IdentASTContext context)
        {
            //se retorna el nombre del identificador
            return context.ID().GetText();
        }

        public override object VisitRelopAST(MiniCSharpParser.RelopASTContext context)
        {
            //se obtiene el generador de codigo
            ILGenerator currentIL = currentMethodBldr.GetILGenerator();
            
            //si es menor que
            if (context.LT() != null)
                currentIL.Emit(OpCodes.Clt);
            
            //si no es igual
            else if (context.NOTEQUAL() != null)
            {
                currentIL.Emit(OpCodes.Ceq);
                currentIL.Emit(OpCodes.Ldc_I4_0);
                currentIL.Emit(OpCodes.Ceq);
            
            }
            //si es igual
            else if(context.EQUAL() !=null)
            {
                currentIL.Emit(OpCodes.Ceq);
            }
            
            //si es menor o igual
            else if (context.LE() != null)
            {
                currentIL.Emit(OpCodes.Cgt);
                currentIL.Emit(OpCodes.Ldc_I4_0);
                currentIL.Emit(OpCodes.Ceq);
            }
            //si es mayor o igual
            else if (context.GE() != null)
            {
                currentIL.Emit(OpCodes.Clt);
                currentIL.Emit(OpCodes.Ldc_I4_0);
                currentIL.Emit(OpCodes.Ceq);
            }
            //si es mayor que
            else if(context.GT()!=null)
                currentIL.Emit(OpCodes.Cgt);
            
            return null;
        }
}