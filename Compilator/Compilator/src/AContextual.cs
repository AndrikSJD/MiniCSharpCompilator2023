using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Antlr4.Runtime;
using Proyecto;
using SyntacticAnalysisGenerated;

namespace Compilator;

/// <summary>
/// Clase que realiza el análisis contextual en el contexto del lenguaje MiniCSharp.
/// </summary>
public class AContextual : MiniCSharpParserBaseVisitor<object> {
    
    private SymbolTable _symbolTable;
    private Consola consola;
    
    /// <summary>
    /// Constructor de la clase AContextual.
    /// </summary>
    /// <param name="consola">Instancia de la consola para la impresión de erros en caso de haberlos.</param>
    public AContextual(Consola consola)
    {
        this.consola = consola;
        _symbolTable = new SymbolTable();
    }
    
    /// <summary>
    /// Devuelve la línea y la columna del token especificado.
    /// </summary>
    /// <param name="token">Token para mostrar línea y columna.</param>
    /// <returns>Cadena que representa la línea y columna del token especificado.</returns>
    private string ShowToken(IToken token)
    {
        return $"[Línea: {token.Line}, Columna: {token.Column}]";
    }

    public override object VisitProgramAST(MiniCSharpParser.ProgramASTContext context)
    {
        try
        {
            // Abrimos un nuevo ámbito para el programa
            _symbolTable.OpenScope();
        
            
           
            
            // Visitamos el identificador del programa y creamos un objeto ClassType para representarlo
            IToken token = (IToken)Visit(context.ident());
            ClassTypeData classTypeData = new ClassTypeData(token, _symbolTable.currentLevel,context);
        
            // Insertamos la clase principal en la tabla de símbolos
            _symbolTable.Insert(classTypeData);
        
            // Visitamos todos los hijos del programa
            foreach (var child in context.children)
            {
                Visit(child);
            }
        
            // Cerramos el ámbito del programa
            _symbolTable.CloseScope();
            if (String.IsNullOrEmpty(consola.SalidaConsola.Text))
            {
                consola.SalidaConsola.AppendText("Compilación exitosa");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    
        // Imprimimos la tabla de símbolos (opcional, puedes eliminar esta línea si no es necesaria)
        consola.SalidaConsola.AppendText(_symbolTable.Print());   

        return null;
    }



    public override object VisitUsingAST(MiniCSharpParser.UsingASTContext context)
    {
        return null;
    }

    // Método que visita un nodo VarDeclAST en el árbol de análisis sintáctico
    public override object VisitVarDeclAST(MiniCSharpParser.VarDeclASTContext context)
    {
        // Obtener el token actual
        IToken currentToken = context.Start;

        // Obtener el texto del tipo de la variable
        string typeText = context.type().GetText();

        // Declarar variables y banderas
        PrimaryTypeData.PrimaryTypes varType;
        bool isArray = typeText.Contains("[]");
        bool isClassVarType = false;
        bool isError = false;

        // Verificar si el tipo es un array
        if (isArray)
        {
            // Obtener el texto del tipo de elemento del array
            string elementTypeText = typeText.Substring(0, typeText.Length - 2).Trim();
            
            // Obtener el tipo de elemento del array
            varType = PrimaryTypeData.showType(elementTypeText);

            // Verificar si el tipo de elemento es desconocido y si existe en la tabla de símbolos
            if (varType is PrimaryTypeData.PrimaryTypes.Unknown && _symbolTable.Search(elementTypeText) != null)
            {
                isClassVarType = true;
            }
            // Verificar si el tipo de elemento no es char ni int
            else if (varType != PrimaryTypeData.PrimaryTypes.Char && varType != PrimaryTypeData.PrimaryTypes.Int)
            {
                // Mostrar mensaje de error en la consola
                consola.SalidaConsola.AppendText($"Error: El tipo de datos del array es incorrecto. Se requiere un tipo válido, como int o char. {ShowToken(currentToken)} \n");
                isError = true;
            }
        }
        else
        {
            // Obtener el tipo de la variable
            varType = PrimaryTypeData.showType(typeText);
            
            // Verificar si el tipo es desconocido y si existe en la tabla de símbolos
            if (varType is PrimaryTypeData.PrimaryTypes.Unknown && _symbolTable.Search(typeText) != null)
            {
                isClassVarType = true;
            }
            // Verificar si el tipo es desconocido y no existe en la tabla de símbolos
            else if (varType is PrimaryTypeData.PrimaryTypes.Unknown && _symbolTable.Search(typeText) == null)
            {
                isError = true;
            }
        }

        // Verificar si no hay errores en el tipo de variable
        if (!isError)
        {

            // Verificar otros errores en la variable y mostrar mensajes en la consola
            CheckVariableErrors(context, currentToken, isArray, varType, isClassVarType);
        }
        else
        {
            // Mostrar mensaje de error en la consola
            consola.SalidaConsola.AppendText($"Error: El tipo de declaración de la variable no es válido. {ShowToken(currentToken)} \n");
        }

        return null;
    }


    public void CheckVariableErrors(MiniCSharpParser.VarDeclASTContext context, IToken currentToken, bool isArray,
        PrimaryTypeData.PrimaryTypes varType, bool isClassVarType)
    {
       
        // Recorrer todos los identificadores en la declaración de variables
        foreach (var ident in context.ident())
            {      
                
                
                
                // Obtener el token del identificador
                IToken token = (IToken)Visit(ident);
                
                //verificamos si es un array
                if(isArray) 
                {

                    if (_symbolTable.currentClass != null)
                    {
                        // Mostrar error si se intenta declarar un array en una clase
                        consola.SalidaConsola.AppendText($"Error: Solo se permiten variables de tipos básicos dentro de la clase. {ShowToken(currentToken)} \n");
                    }
                    else if(_symbolTable.currentMethod!= null) //es una variable local dentro de un metodo
                    {
                     
                        TypeData typeDataVariable = _symbolTable.Search(token.Text);
                        TypeData methodRepeatedVar = _symbolTable.getRepeatedParameter(token.Text);
                       
       

                        if (typeDataVariable != null && typeDataVariable.Level == 0)
                        {
                            consola.SalidaConsola.AppendText($"Error: La variable \"{token.Text}\" ya ha sido declarada previamente en el scope global. {ShowToken(currentToken)}\n");
                        }

                        else if (methodRepeatedVar != null)
                        {
                            if (methodRepeatedVar.Level < _symbolTable.currentLevel)
                            {
                                consola.SalidaConsola.AppendText($"Error: La variable \"{token.Text}\" ya ha sido declarada localmente. {ShowToken(currentToken)}\n");
                            }

                            else if (methodRepeatedVar.Level == _symbolTable.currentLevel)
                            {
                                consola.SalidaConsola.AppendText($"Error: La variable \"{token.Text}\" ya ha sido declarada localmente. {ShowToken(currentToken)}\n");
                            }
                            {
                                
                            }
                        }
                        
                        else
                        {
                            // Crear y insertar un nuevo ArrayType en la tabla de símbolos
                            if (varType is PrimaryTypeData.PrimaryTypes.Int)
                            {
                                ArrayTypeData array = new ArrayTypeData(token, _symbolTable.currentLevel, ArrayTypeData.ArrTypes.Int, context);
                                _symbolTable.Insert(array);
                                ident.declPointer = context;

                            }
                            else if (varType is PrimaryTypeData.PrimaryTypes.Char ) //al validar el tipo de la variable, si no es int, es char anteriormente se valido si no era valido isError = true
                            {
                                ArrayTypeData array = new ArrayTypeData(token, _symbolTable.currentLevel, ArrayTypeData.ArrTypes.Char, context);
                                _symbolTable.Insert(array);
                                ident.declPointer = context;
                            }
                        }
                    }
                    else if (_symbolTable.currentClass == null && _symbolTable.currentMethod == null) //es una variable global
                    {
                        
                       
                        IToken tok = (IToken)Visit(ident);


                        TypeData typeDataVariable = _symbolTable.Search(token.Text);
                        
                        // Verificar si la variable ya ha sido declarada
                        if (typeDataVariable != null )
                        {
                            // Mostrar error si la variable ya ha sido declarada previamente en el mismo ámbito
                            consola.SalidaConsola.AppendText($"Error: La variable \"{tok.Text}\" ya ha sido declarada previamente. {ShowToken(currentToken)}\n");
                        }
                        else
                        {
                            // Crear y insertar un nuevo ArrayType en la tabla de símbolos
                            if (varType is PrimaryTypeData.PrimaryTypes.Int)
                            {
                                ArrayTypeData array = new ArrayTypeData(token, _symbolTable.currentLevel, ArrayTypeData.ArrTypes.Int, context);
                                _symbolTable.Insert(array); 
                                ident.declPointer = context;
                                
                            }
                            else if (varType is PrimaryTypeData.PrimaryTypes.Char ) //al validar el tipo de la variable, si no es int, es char anteriormente se valido si no era valido isError = true
                            {
                                ArrayTypeData array = new ArrayTypeData(token, _symbolTable.currentLevel, ArrayTypeData.ArrTypes.Char, context);
                                _symbolTable.Insert(array);
                                ident.declPointer = context;
                                
                            }
                        }
                    }
                }
                else // No es un array
                {
                    //si la variable es de un tipo de clase
                    if(isClassVarType)
                    {
                        // Verificar si está dentro en una clase
                        if(_symbolTable.currentClass != null) 
                        {
                            // Mostrar error si el tipo de la variable no es básico
                            consola.SalidaConsola.AppendText($"Error: Solo se permiten variables de tipos básicos dentro de una clase. {ShowToken(currentToken)}\n");
                        }
                        else if(_symbolTable.currentMethod != null) //es una variable local dentro de un metodo
                        {
                            //se establece el contexto de la variable como local
                            context.isLocal = true;
                            
                            TypeData typeDataVariable = _symbolTable.Search(token.Text);
                            TypeData methodRepeatedVar = _symbolTable.getRepeatedParameter(token.Text);
                         
                            ClassVarTypeData element = new ClassVarTypeData(token, _symbolTable.currentLevel, context.type().GetText(), context);
                            
                         
                            
                            System.Console.WriteLine("DECLARACION VARIABLE: " + token.Text);
                            if (typeDataVariable != null && typeDataVariable.Level == 0)
                            {
                                consola.SalidaConsola.AppendText($"Error: La variable \"{token.Text}\" ya ha sido declarada previamente en el scope global. {ShowToken(currentToken)}\n");
                            }
                            
                            else if (methodRepeatedVar != null)
                            {
                                if (methodRepeatedVar.Level < _symbolTable.currentLevel )
                                {
                                    consola.SalidaConsola.AppendText($"Error: La variable \"{token.Text}\" ya ha sido declarada localmente. {ShowToken(currentToken)}\n");
                                }

                                else if (methodRepeatedVar.Level == _symbolTable.currentLevel)
                                {
                                    consola.SalidaConsola.AppendText($"Error: La variable \"{token.Text}\" ya ha sido declarada localmente. {ShowToken(currentToken)}\n");
                                }
                                {
                                
                                }
                            }
                            
                            else
                            {
                                
                                _symbolTable.Insert(element);
                                ident.declPointer = context;
                            }

                           
                            
                        }
                        else if (_symbolTable.currentClass == null && _symbolTable.currentMethod == null) //es una variable global
                        {
                            System.Diagnostics.Debug.WriteLine("ES UNA VARIABLE DE CLASE GLOBAL");
                            // IToken tok = (IToken)Visit(ident);
                            TypeData typeDataVariable = _symbolTable.Search(token.Text);
                            
                            // Verificar si la variable ya ha sido declarada
                            if (typeDataVariable != null )
                            {
                                // Mostrar error si la variable ya fue declarada
                                consola.SalidaConsola.AppendText($"Error: La variable \"{token.Text}\" ya ha sido declarada previamente. {ShowToken(currentToken)}\n");
                            }
                            else
                            {
                                ClassVarTypeData element = new ClassVarTypeData(token, _symbolTable.currentLevel, context.type().GetText(), context);
                                _symbolTable.Insert(element);
                                ident.declPointer = context;
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("NO ENTRO A NINGUNO");
                        }
                        
      
                    }
                    else //es de un tipo primario
                    {
                        PrimaryTypeData element = new PrimaryTypeData(token,varType, _symbolTable.currentLevel, context);
                        
                        if(_symbolTable.currentClass != null) //si estamos dentro de una clase
                        {

                            // Verificar si la variable ya ha sido declarada
                            if (!_symbolTable.currentClass.BuscarAtributo(element.GetToken().Text))
                            {
                                //se establece el contexto de la variable como local
                                context.isLocal = true;
                                
                                _symbolTable.Insert(element);
                                _symbolTable.currentClass.parametersL.AddLast(element);
                                ident.declPointer = context;
                            }
                            else
                            { 
                                // Mostrar un error si la variable ya fue declarada
                                consola.SalidaConsola.AppendText($"Error: La variable \"{token.Text}\" ya ha sido declarada en la clase. {ShowToken(currentToken)}\n");
                            }
                           
                            
                        }
                        else if(_symbolTable.currentMethod!= null) //es una variable local dentro de un metodo
                        {
                            
                            //se establece el contexto de la variable como local
                            context.isLocal = true;
                            
                            TypeData typeDataVariable = _symbolTable.Search(token.Text);
                            TypeData methodRepeatedVar = _symbolTable.getRepeatedParameter(token.Text);
                            

                            
                            if (typeDataVariable != null && typeDataVariable.Level == 0)
                            {
                                consola.SalidaConsola.AppendText($"Error: La variable \"{token.Text}\" ya ha sido declarada como variable global. {ShowToken(currentToken)}\n");
                            }
                            else if (methodRepeatedVar != null)
                            {
                                
                               
                                if(methodRepeatedVar.Level < _symbolTable.currentLevel)
                                {
                                    consola.SalidaConsola.AppendText($"Error: La variable \"{token.Text}\" ya ha sido declarada localmente. {ShowToken(currentToken)}\n");
                                }

                                else if (methodRepeatedVar.Level == _symbolTable.currentLevel)
                                {
                                    consola.SalidaConsola.AppendText($"Error: La variable \"{token.Text}\" ya ha sido declarada localmente. {ShowToken(currentToken)}\n");
                                }
                                {
                                
                                }
                            }
                            
                            else
                            {
                                _symbolTable.Insert(element);
                                ident.declPointer = context;
                            }
                        }
                        else if (_symbolTable.currentClass == null && _symbolTable.currentMethod == null) //es una variable global
                        {
                            
                            TypeData variableglobal = _symbolTable.Search(token.Text);
                            
                            // Verificar si el identificador es existente
                            if (variableglobal!= null )
                            {
                                // Mostrar error si el identificador existe
                                consola.SalidaConsola.AppendText($"Error: El identificador \"{token.Text}\" ya existe y es de tipo \"{variableglobal.GetStructureType()}\". {ShowToken(currentToken)}\n");
                            }
                            else
                            {
                                _symbolTable.Insert(element);
                                ident.declPointer = context;
                            }
                        }

                    }

                }
            }
    }

    public override object VisitClassDeclAST(MiniCSharpParser.ClassDeclASTContext context)
    {
        // Obtenemos el token asociado al contexto
        IToken currentToken = context.Start;

        // Verificamos si la clase ya ha sido declarada anteriormente
        if (_symbolTable.Search(context.ident().GetText()) != null)
        {
            consola.SalidaConsola.AppendText($"Error: La clase \"{context.ident().GetText()}\" ya ha sido declarada anteriormente. {ShowToken(currentToken)}\n");

            return null;
        }
        
        //TODO REVISAR: GUARDAMOS IDENT DE CLASS DECLARATION
        IToken ident = (IToken)Visit(context.ident());
    
        // Creamos un objeto ClassType para representar la clase actual
        ClassTypeData classDcl = new ClassTypeData(ident, _symbolTable.currentLevel, context);
    
        // Insertamos la clase en la tabla de símbolos
        _symbolTable.Insert(classDcl);
    
        // Establecemos la clase actual en la tabla de símbolos
        _symbolTable.currentClass = classDcl; //saber en la clase actual sobre la que estoy trabajando
    
        // Abrimos un nuevo ámbito para la clase
        _symbolTable.OpenScope();
    
        // Visitamos las declaraciones de variables de la clase, si las hay
        if(context.varDecl()!= null)
        {
            foreach (var child in context.varDecl())
            {
                Visit(child);
            }
        }
   
        // Cerramos el ámbito de la clase
        _symbolTable.CloseScope();
    
        // Volvemos nula la clase actual en la tabla de símbolos
        _symbolTable.currentClass = null; //volvemos null la clase actual
    
        return null;
    }

    
    public override object VisitMethodDeclAST(MiniCSharpParser.MethodDeclASTContext context)
    {
        // Obtener el token actual
        IToken currentToken = context.Start;  
        
        // Obtenemos el nombre del método
        Visit(context.ident());
        IToken token = (IToken)Visit(context.ident());
        
        
        string methodName = token.Text;
        
        TypeData? existingType = _symbolTable.Search(methodName);

        // Verificamos si ya existe un método con el mismo nombre en el contexto actual
        if (existingType != null && existingType is MethodTypeData)
        {
            // Mostrar error si el metodo ya existe
            consola.SalidaConsola.AppendText($"Error: La declaración del método \"{methodName}\" ya existe en el contexto actual. {ShowToken(currentToken)}\n");

            return null;
        }

        // Inicializamos en unknown para validar que el tipo de retorno sea valido y que nos permita accesar cuando
        PrimaryTypeData.PrimaryTypes methodType = PrimaryTypeData.PrimaryTypes.Unknown;
        bool isArray = false;
        bool isClassVarType = false;
        bool isError = false;

        
        if (context.type()!= null)
        {
            // Verificamos si es un array
            if(context.type().GetText().Contains("[]"))
            {
                isArray = true;
                
                // Se quitan los corchetes y se obtiene el tipo base del arreglo
                methodType = PrimaryTypeData.showType(context.type().GetText().Substring(0, context.type().GetText().Length - 2).Trim());
                if (methodType is PrimaryTypeData.PrimaryTypes.Unknown && 
                    _symbolTable.Search(context.type().GetText().Substring(0, context.type().GetText().Length - 2 )
                        .Trim()) != null)
                {
                    isClassVarType = true;
                }
                // Verificar si el tipo base no es 'char' ni 'int'
                else if (methodType != PrimaryTypeData.PrimaryTypes.Char && methodType != PrimaryTypeData.PrimaryTypes.Int)
                {
                    consola.SalidaConsola.AppendText($"Error: El tipo del arreglo debe ser \"int\" o \"char\", pero se encontró un tipo no válido. {ShowToken(currentToken)}\n");
                    isError = true;
                }
            
            }
            else
            {
                // No es un arreglo, se obtiene el tipo de retorno directamente
                methodType = PrimaryTypeData.showType(context.type().GetText());
                
                // Verificar si el tipo es desconocido y si existe en la tabla de símbolos
                if (methodType is PrimaryTypeData.PrimaryTypes.Unknown && 
                    _symbolTable.Search(context.type().GetText()) != null)
                {
                    isClassVarType = true;
                }
                // Verificar si el tipo es desconocido y no existe en la tabla de símbolos
                else if (methodType is PrimaryTypeData.PrimaryTypes.Unknown && 
                         _symbolTable.Search(context.type().GetText()) == null)
                {
                    isError = true;
                }
            }
        }

        // Si no hubo error en la obtención del tipo de retorno
        if (!isError)
        {

            if (methodType != null && context.block().GetText().Equals("{}"))
            {
                consola.SalidaConsola.AppendText($"Error: en el metodo {context.ident().GetText()} falta el retorno de tipo {methodType.ToString()}. {ShowToken(currentToken)} \n");
            } else if (!(context.block().GetText().Contains("return")) && methodType != null && (context.VOID()== null))
            {
                consola.SalidaConsola.AppendText($"Error: en el metodo {context.ident().GetText()} falta el retorno de tipo {methodType.ToString()}. {ShowToken(currentToken)} \n");
             }
            
            
            
            
            LinkedList<TypeData> parameters = new LinkedList<TypeData>();
            
            // Verificamos si se especificaron parámetros
            if(context.formPars() != null)
            {
                // Abrimos un nuevo alcance en la tabla de símbolos
                _symbolTable.OpenScope();
                
                // Obtenemos los parámetros visitando el nodo formPars
                parameters = (LinkedList<TypeData>)Visit(context.formPars());
                
                // Cerramos el alcance actual en la tabla de símbolos
                _symbolTable.CloseScope();
            
            }
            MethodTypeData method;
            if (context.type() != null)
            {
                if(isArray)
                {
                    // Creamos un nuevo objeto MethodType para el método con tipo de retorno de arreglo
                    method = new MethodTypeData(token, _symbolTable.currentLevel, parameters.Count, methodType.ToString() + "[]", parameters, context);
                    _symbolTable.Insert(method);
                    _symbolTable.currentMethod = method;
                    _symbolTable.currentMethodIndex = _symbolTable.getTableSize() - 1;
                }
                else
                {
                    // Creamos un nuevo objeto MethodType para el método con tipo de retorno normal
                    method = new MethodTypeData(token, _symbolTable.currentLevel, parameters.Count, methodType.ToString() , parameters, context);
                    _symbolTable.Insert(method);
                    _symbolTable.currentMethod = method;
                    _symbolTable.currentMethodIndex = _symbolTable.getTableSize() - 1;
                    
                }
                
            }
            else
            {
                // Creamos un nuevo objeto MethodType para el método con tipo de retorno "Void"
                method = new MethodTypeData(token, _symbolTable.currentLevel, parameters.Count, "Void", parameters, context);
                _symbolTable.Insert(method);
                // Establecemos el método actual en la tabla de símbolos
                _symbolTable.currentMethod = method;
                //Obtenemos el indice del metodo actual
                _symbolTable.currentMethodIndex = _symbolTable.getTableSize() - 1;
                // Guardamos el block del metodo actual
                
               
                
            }
        
            // Insertamos los parámetros en la tabla de símbolos
            foreach (var child in parameters)
            {
                _symbolTable.Insert(child);
            }
 
            // Abrimos un nuevo alcance en la tabla de símbolos
            _symbolTable.OpenScope();
            
            // Visitamos el bloque de código del método
            Visit(context.block());
            
            // Cerramos el alcance actual en la tabla de símbolos
            _symbolTable.CloseScope();
        
            // Eliminamos los parámetros y el cuerpo del método de la tabla de símbolos
            _symbolTable.DeleteParametersBody(token.Text);

        }
        else
        {
            // Si se encontró un tipo de método no válido
            consola.SalidaConsola.AppendText($"El tipo del metodo no es valido. {ShowToken(currentToken)} \n");
        }

        // Establecemos el método actual como nulo en la tabla de símbolos
        _symbolTable.currentMethod = null;
        _symbolTable.currentMethodIndex = 0;
        
        return null;
    }

    public override object VisitFormParsAST(MiniCSharpParser.FormParsASTContext context)
    {
        // Obtenemos el token asociado al contexto
        IToken currentToken = context.Start;  
        
        // Creamos una lista enlazada para almacenar los parámetros
        LinkedList<TypeData> parameters = new LinkedList<TypeData>();
        
        // Iteramos sobre los identificadores y tipos de los parámetros
        for (int i = 0; i < context.ident().Length; i++)
        {
            
            //TODO:REVISAR CAMBIOS
          
            // Obtenemos el token del identificador
             IToken token = (IToken)Visit(context.ident(i));

      
            
            // Obtenemos el tipo del parámetro como cadena
            string type = context.type(i).GetText();
            
            // Verificamos si es un array
            if (type.Contains("[]"))
            {
                // Obtenemos el tipo del arreglo eliminando los corchetes
                PrimaryTypeData.PrimaryTypes varType = PrimaryTypeData.showType(type.Substring(0, type.Length - 2).Trim());
                
                // Verificamos el tipo del arreglo y creamos un objeto ArrayType correspondiente
                if (varType is PrimaryTypeData.PrimaryTypes.Int)
                {
                    parameters.AddLast(new ArrayTypeData(token, _symbolTable.currentLevel, ArrayTypeData.ArrTypes.Int, context));
                }
                else if (varType is PrimaryTypeData.PrimaryTypes.Char)
                {
                    parameters.AddLast(new ArrayTypeData(token, _symbolTable.currentLevel, ArrayTypeData.ArrTypes.Char, context));
                }
                else
                {
                    consola.SalidaConsola.AppendText($"Error: El tipo del arreglo debe ser int o char. El tipo actual no es válido. {ShowToken(currentToken)}\n");
                }
            }
            else
            {
                // Obtener el tipo del parámetro como PrimaryType
                PrimaryTypeData.PrimaryTypes varType = PrimaryTypeData.showType(type);
                
                // Verificamos si es un tipo de clase buscándolo en la tabla de símbolos
                TypeData? paramT = _symbolTable.Search(type);
                
                if (varType is PrimaryTypeData.PrimaryTypes.Unknown && paramT != null)
                {
                    // Si es un tipo desconocido y existe en la tabla de símbolos, es una clase
                    parameters.AddLast(new ClassVarTypeData(token, _symbolTable.currentLevel, type, context));
                    // También puedes agregarlo a la tabla de símbolos
                }
                else if (varType is PrimaryTypeData.PrimaryTypes.Unknown && paramT == null)
                {
                    consola.SalidaConsola.AppendText($"Error: El tipo de parámetro \"{token.Text}\" no es válido. {ShowToken(currentToken)}\n");
                }
                else
                {
                    // Es un tipo primario conocido
                    parameters.AddLast(new PrimaryTypeData(token, varType, _symbolTable.currentLevel, context));
                }
            }
        }
        
        return parameters;
    }



    public override object VisitTypeAST(MiniCSharpParser.TypeASTContext context)
    {
        //TODO: REVISAR
        IToken type = (IToken)Visit(context.ident());


        //se retorna el identificador del tipo
        return type.Text;

    }

    public override object VisitAssignStatementAST(MiniCSharpParser.AssignStatementASTContext context)
    {
        // Obtener el token actual
        IToken currentToken = context.Start;   
    
        string designatorType = (string)Visit(context.designator());
        
       
        // Verificar si es una asignación
        if(context.expr()!=null)
        {
            string expressionType = ((string)Visit(context.expr())); //tolower para que no haya problemas con mayusculas y minusculas
            
            //verificamos si el tipodesignator viene nulo es decir el indice no es null
            if (designatorType != null && expressionType != null)
            {
                expressionType = expressionType.ToLower();//tolower para que no haya problemas con mayusculas y minusculas
                designatorType= designatorType.ToLower();//tolower para que no haya problemas con mayusculas y minusculas
                
                
                //Aqui se verifica si es un arreglo y se le asigna un arreglo
                if (designatorType.Contains("[]") && expressionType.Contains("[]"))
                {
                    consola.SalidaConsola.AppendText($"Error de asignación: No se pueden asignar una lista \"{designatorType}\" a otra \"{expressionType.ToLower()}\".  {ShowToken(currentToken)}\n");
                    return null;
                }
                
                // Verificar si es un nuevo arreglo
                if (designatorType.Contains("[]") && context.expr().GetText().Contains("new"))
                {
          
                    if (!(designatorType.Contains(expressionType)))
                    {
                        consola.SalidaConsola.AppendText($"Error de asignación: El tipo \"{designatorType}\" no coincide con el tipo de la expresión \"{expressionType.ToLower()}\". {ShowToken(currentToken)}\n");
                    }
                    
                    return null;
                }
                
            
                // Los tipos no son compatibles
                if (designatorType != expressionType)
                {
                    
                    consola.SalidaConsola.AppendText($"Error de asignación: El tipo \"{designatorType}\" no es compatible con el tipo de la expresión \"{expressionType}\". {ShowToken(currentToken)}\n");

                    return null;
                }
                else
                {
                    return null;
                }
                
            }
            else
            {
                // Si los tipos de designador y expresión no son válidos
                consola.SalidaConsola.AppendText($"Error en la asignación: Los tipos del designador y de la expresión no son válidos. {ShowToken(currentToken)}\n");

                return null;
            }
            
        }
        else if (context.LPARENT() != null) // si es llamada a metodo
        {
            TypeData? type = _symbolTable.Search(context.designator().GetText());
            
            
            if (type is MethodTypeData method)
            {
                if (context.actPars() != null)
                {
                    LinkedList<TypeData> parameters = (LinkedList<TypeData>)Visit(context.actPars());

                    if (parameters.Count == method.parametersL.Count)
                    {
                        for (int i = 0; i < method.parametersL.Count; i++)
                        {
                            if (method.parametersL.ElementAt(i).GetStructureType().ToString() !=
                                parameters.ElementAt(i).GetStructureType())
                            {
                                // Los tipos de parámetros no coinciden
                                consola.SalidaConsola.AppendText($"Error de asignación: El tipo \"{method.parametersL.ElementAt(i).GetStructureType()}\" del parámetro {i} no coincide con el tipo de la expresión \"{parameters.ElementAt(i)}\". {ShowToken(currentToken)}\n");
                            }
                        }
                    }
                    else
                    {
                        // Si la cantidad de parámetros no es correcta
                        consola.SalidaConsola.AppendText($"Error de asignación: El número de parámetros ({method.parametersL.Count}) no coincide con el número de expresiones ({parameters.Count}). {ShowToken(currentToken)}\n");
                    }
                }
                else if (method.parametersL.Count > 0)
                {
                    // Si se carece de parámetros
                    consola.SalidaConsola.AppendText($"Error de parámetros: Faltan parámetros en el método \"{method.GetToken().Text}\". {ShowToken(currentToken)}\n");
                }
            }
            
            else if (context.designator().GetText() == "add")
            {
                if (context.actPars() != null)
                {
                    // Obtener lista de parametros
                    LinkedList<TypeData> parameters = (LinkedList<TypeData>)Visit(context.actPars());
                    //Recibe dos parametros, la lista, el indice

                    if (parameters.Count == 2)
                    {
                        if (!(parameters.ElementAt(0).GetStructureType()
                                .Equals(parameters.ElementAt(1).GetStructureType())))
                        {
                            // Si los parametros no son correctos
                            consola.SalidaConsola.AppendText($"Error de parámetros: Los tipos de parámetros no coinciden en el método add. {ShowToken(currentToken)}\n");
                        }
                    }
                    else if (parameters.Count != 2)
                    {
                        // Si se exede la cantidad de parámetros
                        consola.SalidaConsola.AppendText($"Error en los parámetros: Cantidad incorrecta de parámetros para el método add. {ShowToken(currentToken)}\n");
                    }
                }
                else
                {
                    // Si carece de parámetros
                    consola.SalidaConsola.AppendText($"Error en el método: Faltan los parámetros para el método add. {ShowToken(currentToken)}\n");
                }
                
            }
            
            else if (context.designator().GetText() == "len")
            {
                if (context.actPars() != null)
                {
                    // Obtener lista de parametros
                    LinkedList<TypeData> lenPars = (LinkedList<TypeData>)Visit(context.actPars());

                    if (lenPars.Count == 1)
                    {
                        if (!(lenPars.ElementAt(0) is ArrayTypeData))
                        {
                            // Si los parametros no son correctos
                            consola.SalidaConsola.AppendText($"Error de parámetros: Los tipos de parámetros no coinciden en el método len. {ShowToken(currentToken)}\n");
                        }
                    }
                    else
                    {
                        // Si se exede la cantidad de parámetros
                        consola.SalidaConsola.AppendText($"Error en los parámetros: Cantidad incorrecta de parámetros para el método len. {ShowToken(currentToken)}\n");
                    }
                    
                }
                else
                { 
                    // Si carece de parámetros
                    consola.SalidaConsola.AppendText($"Error en el método: Faltan los parámetros para el método len. {ShowToken(currentToken)}\n");
                }
            }
            
            else if (context.designator().GetText() == "del")
            {
                if (context.actPars() != null)
                {
                    // Obtener lista de parametros
                    LinkedList<TypeData> parameters = (LinkedList<TypeData>)Visit(context.actPars());
                    //Recibe dos parametros, la lista, el indice

                    if (parameters.Count == 2)
                    {
                        if (!(parameters.ElementAt(0) is ArrayTypeData &&
                             parameters.ElementAt(1).GetStructureType().Equals("Int")))
                        {
                            // Si los parametros no son correctos
                            consola.SalidaConsola.AppendText($"Error de parámetros: Los tipos de parámetros no coinciden en el método del. {ShowToken(currentToken)}\n");
                        }
                    }
                    else if (parameters.Count != 2)
                    {
                        // Si se exede la cantidad de parámetros
                        consola.SalidaConsola.AppendText($"Error en los parámetros: Cantidad incorrecta de parámetros para el método del. {ShowToken(currentToken)}\n");
                    }

                }
                else
                {
                    // Si carece de parámetros
                    consola.SalidaConsola.AppendText($"Error en el método: Faltan los parámetros para el método del. {ShowToken(currentToken)}\n");
                }
                
            }
            else
            {
                // Si no es un método
                consola.SalidaConsola.AppendText($"Error de asignación: \"{context.designator().GetText()}\" no es un método. {ShowToken(currentToken)}\n");
            }
            
           
        }
        else if (context.DEC()!=null)
        {
            if(designatorType.ToLower() != "int")
            {
                // Si el tipo de dato no es int
                consola.SalidaConsola.AppendText($"Error de asignación: \"{context.designator().GetText()}\" solo se puede decrementar variables enteras. {ShowToken(currentToken)}\n");
            }
            
        }
        
        else if (context.INC()!=null)
        {
            if(designatorType.ToLower() != "int")
            {
                // Si el tipo de dato no es int
                consola.SalidaConsola.AppendText($"Error de asignación: \"{context.designator().GetText()}\" solo se puede incrementar variables enteras. {ShowToken(currentToken)}\n");
            }
            
        }
        
        
        return null;
    }
    

    public override object VisitIfStatementAST(MiniCSharpParser.IfStatementASTContext context)
    {
        // Obtenemos el token asociado al contexto
        IToken currentToken = context.Start;
    
        // Abrimos un nuevo ámbito en la tabla de símbolos
        _symbolTable.OpenScope();
    
        // Evaluamos la condición del if
        bool conditionValue = (bool)Visit(context.condition());
    
        // Verificamos si la condición es verdadera
        if (conditionValue)
        {
            // Visitamos la primera declaración en el cuerpo del if
            Visit(context.statement(0));
        }
        else
        {
            consola.SalidaConsola.AppendText($"Error: El tipo en la condición del if es falsa: \"{context.condition().GetText()}\". {ShowToken(currentToken)}\n");
        
            // Verificamos si hay una segunda declaración en el cuerpo del if
            if (context.statement(1) != null)
            {
                // Abrimos un nuevo ámbito en la tabla de símbolos
                _symbolTable.OpenScope();
            
                // Visitamos la segunda declaración en el cuerpo del if
                Visit(context.statement(1));
            
                // Cerramos el ámbito actual en la tabla de símbolos
                _symbolTable.CloseScope();
            }
        }
    
        // Cerramos el ámbito actual en la tabla de símbolos
        _symbolTable.CloseScope();
        return null;
    }


    public override object VisitForStatementAST(MiniCSharpParser.ForStatementASTContext context)
    {
        // Obtenemos el token asociado al contexto
        IToken currentToken = context.Start;
        
        // Abrimos un nuevo ámbito en la tabla de símbolos
        _symbolTable.OpenScope();
        
        // Visitamos la expresión del for
        Visit(context.expr());
        
        // Verificamos si hay una condición en el for
        if (context.condition() != null)
        {
            // Evaluamos la condición del for
            bool conditionValue = (bool)Visit(context.condition());
            
            // Verificamos si la condición es verdadera
            if (conditionValue)
            {
                // Verificamos si hay múltiples declaraciones en el cuerpo del for
                if (context.statement().Length > 1)
                {
                    // Visitamos las dos declaraciones en orden
                    Visit(context.statement(0));
                    Visit(context.statement(1));
                }
                else
                {
                    // Visitamos la única declaración en el cuerpo del for
                    Visit(context.statement(0));
                }
            }
            else
            {
                consola.SalidaConsola.AppendText($"Error: El tipo en la condición del for es falsa: \"{context.condition().GetText()}\". {ShowToken(currentToken)}\n");
            }
            
            // Cerramos el ámbito actual en la tabla de símbolos
            _symbolTable.CloseScope();
            return null;
        }
        
        // No hay una condición en el for, verificamos si hay múltiples declaraciones en el cuerpo del for
        if (context.statement().Length > 1)
        {
            // Visitamos las dos declaraciones en orden
            Visit(context.statement(0));
            Visit(context.statement(1));
        }
        else
        {
            // Visitamos la única declaración en el cuerpo del for
            Visit(context.statement(0));
        }
        
        // Cerramos el ámbito actual en la tabla de símbolos
        _symbolTable.CloseScope();
        return null;
    }


    public override object VisitWhileStatementAST(MiniCSharpParser.WhileStatementASTContext context)
    {
        // Obtenemos el token asociado al contexto
        IToken currentToken = context.Start;
    
        // Abrimos un nuevo ámbito en la tabla de símbolos
        _symbolTable.OpenScope();
    
        // Evaluamos la condición del while
        bool conditionValue = (bool)Visit(context.condition());
    
        // Verificamos si la condición es verdadera
        if (conditionValue)
        {
            // Visitamos el cuerpo del while
            Visit(context.statement());
        }
        else
        {
            consola.SalidaConsola.AppendText($"Error: El tipo en la condición del while es falsa: \"{context.condition().GetText()}\". {ShowToken(currentToken)}\n");
        }
    
        // Cerramos el ámbito actual en la tabla de símbolos
        _symbolTable.CloseScope();
    
        // Retornamos null, ya que no hay un valor específico de retorno
        return null;
    }


    public override object VisitBreakStatementAST(MiniCSharpParser.BreakStatementASTContext context)
    {
        return null;
    }
    
    public override object VisitReturnStatementAST(MiniCSharpParser.ReturnStatementASTContext context)
    {
        // Obtenemos el token asociado al contexto
        IToken currentToken = context.Start;

        // Verificamos si hay una expresión de retorno
        if (context.expr() != null)
        {
            // Obtenemos el tipo de retorno de la expresión
            string returnType = (string)Visit(context.expr());

            // Verificamos si el método actual es de tipo "void"
            if (_symbolTable.currentMethod.ReturnTypeGetSet == "void")
            {
                consola.SalidaConsola.AppendText($"Error de Retorno: El método \"{_symbolTable.currentMethod.GetToken().Text}\" es de tipo void y no puede tener un valor de retorno. {ShowToken(currentToken)}\n");
            }
            // Verificamos si el tipo de retorno de la expresión es válido
            else if (!IsReturnTypeValid(returnType))
            {
                consola.SalidaConsola.AppendText($"Error de Retorno: El método \"{_symbolTable.currentMethod.GetToken().Text}\" no puede retornar un valor de tipo \"{returnType}\". {ShowToken(currentToken)}\n");
            }
        }
        else
        {
            consola.SalidaConsola.AppendText($"Error de Retorno: El método no tiene un valor de retorno válido. {ShowToken(currentToken)}\n");
        }

        // Retornamos null, ya que el retorno no tiene un valor específico
        return null;
    }

    private bool IsReturnTypeValid(string returnType)
    {
        // Verificamos si el tipo de retorno es nulo
        if (returnType == null)
        {
            return false;
        }

        // Comparamos el tipo de retorno con el tipo de retorno del método actual (ignorando mayúsculas y minúsculas)
        return string.Equals(returnType, _symbolTable.currentMethod.ReturnTypeGetSet, StringComparison.OrdinalIgnoreCase);
    }


    public override object VisitReadStatementAST(MiniCSharpParser.ReadStatementASTContext context)
    {
        Visit(context.designator());
        return null;
    }

    public override object VisitWriteStatementAST(MiniCSharpParser.WriteStatementASTContext context)
    {
        Visit(context.expr());
        return null;
    }

    public override object VisitBlockStatementAST(MiniCSharpParser.BlockStatementASTContext context)
    {
        Visit(context.block());
        return null;
    }

    public override object VisitBlockCommentStatementAST(MiniCSharpParser.BlockCommentStatementASTContext context)
    {
        return null;
    }

    public override object VisitBlockAST(MiniCSharpParser.BlockASTContext context)
    {
        // Obtenemos el token actual asociado al contexto
        IToken currentToken = context.Start;
    
        // Validamos en orden y verificamos el tipo que deben tener
        foreach (var child in context.children)
        {
            // Ignoramos los tokens de llave izquierda y llave derecha
            if (child.Equals(context.LBRACE()) || child.Equals(context.RBRACE()))
                continue;

            if (child is MiniCSharpParser.StatementContext)
            {
                // Visitamos una sentencia
                Visit(child);
            }
            else if (child is MiniCSharpParser.VarDeclASTContext)
            {
                // Visitamos una declaración de variable
                Visit(child);
            }
            else
            {
                // Si no es una declaración de variable ni una sentencia, mostramos un mensaje de error
                consola.SalidaConsola.AppendText($"Error en el bloque: Se esperaba una declaración de variable o una sentencia en VisitBlockAST. {ShowToken(currentToken)}\n");
            }
        }

        // Retornamos null, ya que el bloque no tiene un valor de retorno específico
        return null;
    }


    public override object VisitActParsAST(MiniCSharpParser.ActParsASTContext context)
    {
        // LinkedList para almacenar los tipos de los parámetros
        LinkedList<TypeData> parametersTypes = new LinkedList<TypeData>();

        // Iteramos sobre cada expresión en los argumentos
        foreach (var expression in context.expr())
        {
            // Obtenemos el tipo de la expresión
            string expressionType = (string)Visit(expression);

            // Buscamos el tipo en la tabla de símbolos
            TypeData? tableType = _symbolTable.Search(expression.GetText());
            
            if (tableType != null)
            {
                // Agregamos el tipo de la tabla de símbolos a la lista de tipos de parámetros
                parametersTypes.AddLast(tableType);
            }
            // Verificamos si el tipo de la expresión es válido
            else if (expressionType != null)
            {
                // Creamos un nuevo objeto PrimaryType y lo agregamos a la lista de tipos de parámetros
                parametersTypes.AddLast(new PrimaryTypeData(expression.Start, PrimaryTypeData.showType(expressionType.ToLower()),
                    _symbolTable.currentLevel, context));
            }
            // Verificamos si el tipo se encuentra en la tabla de símbolos
            
        }

        // Devolvemos la lista de tipos de parámetros
        return parametersTypes;
    }

    
    public override object VisitConditionAST(MiniCSharpParser.ConditionASTContext context)
    {
        // Obtenemos el token asociado al contexto
        IToken currentToken = context.Start;

        // Variable para indicar si hay una condición válida
        bool hasValidCondition = false;

        // Iteramos sobre cada término de condición
        foreach (var term in context.condTerm())
        {
            // Evaluamos el término de condición
            bool conditionType = (bool)Visit(term);

            // Verificamos si el término de condición es verdadero
            if (conditionType)
            {
                // Hay una condición válida, actualizamos la bandera y salimos del bucle
                hasValidCondition = true;
                break;
            }
        }

        // Verificamos si hay una condición válida
        if (hasValidCondition)
        {
            // Hay una condición válida, retornamos true
            return true;
        }
        else
        {
            // No hay una condición válida, mostramos un mensaje de error
            consola.SalidaConsola.AppendText($"Error: Los tipos que se están comparando no son compatibles. {ShowToken(currentToken)}\n");

            // Retornamos false para indicar que no se cumple ninguna condición válida
            return false;
        }
    }


    public override object VisitCondTermAST(MiniCSharpParser.CondTermASTContext context)
    {
        // Obtenemos el token actual asociado al contexto
        IToken currentToken = context.Start;

        // Variable para almacenar si todas las condiciones son verdaderas
        bool allConditionsTrue = true;

        // Iteramos sobre cada factor de condición
        foreach (var factor in context.condFact())
        {
            // Evaluamos el factor de condición
            bool conditionType = (bool)Visit(factor);

            // Verificamos si la condición es falsa
            if (!conditionType)
            {
                // La condición es falsa, mostramos un mensaje de error
                consola.SalidaConsola.AppendText($"Error: Los tipos no coinciden en VisitConditionTermAST: {conditionType}. {ShowToken(currentToken)}\n");

                // Retornamos false para indicar que no se cumplen todas las condiciones
                return false;
            }
        }

        // Retornamos true para indicar que todas las condiciones se cumplieron
        return allConditionsTrue;
    }


    public override object VisitCondFactAST(MiniCSharpParser.CondFactASTContext context)
    {
        // Obtenemos el token asociado al contexto
        IToken currentToken = context.Start;

        // Evaluamos la expresión del primer operando
        string firstExprType = (string)Visit(context.expr(0));

        // Visitamos el operador relacional
        Visit(context.relop());

        // Evaluamos la expresión del segundo operando
        string secondExprType = (string)Visit(context.expr(1));

        // Verificamos si la comparación es válida
        if (IsComparisonValid(firstExprType, secondExprType))
        {
            // La comparación es válida
            return true;
        }
        else
        {
            // La comparación no es válida, mostramos un mensaje de error
            consola.SalidaConsola.AppendText(GetErrorComparisonMessage(firstExprType, secondExprType,currentToken));

            // Retornamos false para indicar que la comparación no es válida
            return false;
        }
    }

    private bool IsComparisonValid(string firstType, string secondType)
    {
        // Verificamos si alguno de los tipos es nulo
        if (firstType == null || secondType == null)
        {
            // La comparación no es válida si uno de los tipos es nulo
            return false;
        }

        // Comparamos los tipos
        System.Diagnostics.Debug.WriteLine($"Comparando {firstType} con {secondType}");
        return firstType == secondType;
    }

    private string GetErrorComparisonMessage(string expectedType, string actualType, IToken currentToken)
    {
        // Verificamos si alguno de los tipos es nulo
        if (expectedType == null || actualType == null)
        {
            // Si alguno de los tipos es nulo, mostramos un mensaje de error específico
            return $"Error: No se puede comparar el tipo de condición con null. {ShowToken(currentToken)}\n";
        }

        // Mostramos un mensaje de error indicando los tipos esperado y actual
        return $"Error: Los tipos de condición no coinciden. Se esperaba {expectedType} pero se encontró {actualType}. {ShowToken(currentToken)}\n";
    }


    public override object VisitCastAST(MiniCSharpParser.CastASTContext context)
    {
        // Obtenemos el token actual asociado al contexto
        IToken currentToken = context.Start;

        // Obtenemos el tipo del casting
        string type = (string)Visit(context.type());

        // Verificamos si el tipo es nulo
        if (type == null)
        {
            consola.SalidaConsola.AppendText($"Error en el cast: El valor a castear es nulo. {ShowToken(currentToken)}\n");
        }

        // Retornamos el tipo del casting
        return type;
    }



    public override object VisitExpressionAST(MiniCSharpParser.ExpressionASTContext context)
    {
        // Obtenemos el token actual asociado al contexto
        IToken currentToken = context.Start;

        // Verificamos si hay una operación de casting en la expresión
        if (context.cast() != null)
        {
            // Obtenemos el tipo de casting
            string castType = (string)Visit(context.cast());

            // Retornamos el tipo de casting
            return castType;
        }

        // Obtenemos el tipo del primer término de la expresión
        string termType = (string)Visit(context.term(0));

        // Verificamos si el tipo del primer término es nulo
        if (termType == null)
        {
            consola.SalidaConsola.AppendText($"Error: Tipo no válido de la expresión. Se encontró null. {ShowToken(currentToken)}\n");

            // Retornamos null para indicar un tipo no válido
            return null;
        }
 
        // Verificamos los tipos de los términos restantes
        for (int i = 1; i < context.term().Length; i++)
        {
            // Obtenemos el tipo del término actual
            string currentTerm = (string)Visit(context.term(i));

            // Verificamos si el tipo del término actual es diferente al tipo del primer término
            if (termType != currentTerm)
            {
                consola.SalidaConsola.AppendText($"Error de tipos: Todos los tipos en la expresión deben ser iguales. {ShowToken(currentToken)}\n");

                // Retornamos null para indicar un error de tipos
                return null;
            }
        }
    
        // Retornamos el tipo del primer término
        return termType;
    }

    
    public override object VisitTermAST(MiniCSharpParser.TermASTContext context)
    {
        // Obtenemos el token del inicio del contexto
        IToken currentToken = context.Start;

        // Obtenemos el tipo del primer factor
        string factorType = (string)Visit(context.factor(0));

        // Si hay más de un factor, comprobamos si tienen el mismo tipo
        if (context.factor().Length > 1)
        {
            int i = 1;
            while (i < context.factor().Length && factorType == (string)Visit(context.factor(i)))
            {
                i++;
            }

            // Si hay un factor con un tipo diferente, se muestra un error y se devuelve null
            if (i < context.factor().Length)
            {
                consola.SalidaConsola.AppendText($"Error de tipos: Los tipos son diferentes en term. {ShowToken(currentToken)}\n");

                return null;
            }
        }

        // Devolvemos el tipo del factor
        return factorType;
    }


    // Método que visita un nodo FactorAST en el árbol de análisis sintáctico
    public override object VisitFactorAST(MiniCSharpParser.FactorASTContext context)
    {
        // Obtener el token actual
        IToken currentToken = context.Start;

        // Obtener el tipo de designador al visitar el nodo designator
        string designatorType = (string)Visit(context.designator());

        // Verificar si el factor es una llamada a método
        if (context.LPARENT() != null)
        {
            
            // Buscar el método en la tabla de símbolos
            string designator = context.designator().GetText();
            TypeData? metodo = (TypeData)_symbolTable.Search(designator);
            
            // Verificar si es el caso del metodo predefinido len
            if (designator == "len")
            {
                return "Int";
            }
            if (context.actPars() != null)
            {
                // Obtener la lista de tipos de los parámetros al visitar el nodo actPars
                LinkedList<TypeData> listOfTypes = (LinkedList<TypeData>)Visit(context.actPars());
                
                if (metodo is MethodTypeData method)
                {
                    // Verificar si la cantidad de parámetros es incorrecta
                    if (!(method.parametersL.Count == listOfTypes.Count))
                    {
                        consola.SalidaConsola.AppendText($"Error: Cantidad de parámetros incorrecta. {ShowToken(currentToken)}\n");

                        return null;
                    }
                    else
                    {
                        // Verificar si los tipos de los parámetros son correctos
                        for (int i = 0; i < method.parametersL.Count; i++)
                        {
                            if (((MethodTypeData)metodo).parametersL.ElementAt(i).GetStructureType() !=
                                listOfTypes.ElementAt(i).GetStructureType())
                            {
                                consola.SalidaConsola.AppendText($"Error: Tipo de parámetro incorrecto. Se esperaba: {((MethodTypeData)metodo).parametersL.ElementAt(i).GetStructureType()}, se obtuvo: {listOfTypes.ElementAt(i).GetStructureType()} {ShowToken(currentToken)}\n");

                                return null;
                            }
                        }

                        // Devolver el tipo de retorno del método
                        return method.ReturnTypeGetSet;
                    }
                }
                else
                {
                    // Mostrar mensaje de error si no se encuentra el método en la tabla de símbolos
                    consola.SalidaConsola.AppendText($"Error: No se encontró el método. {ShowToken(currentToken)}\n");

                    return null;
                }
                
            }
            else if (metodo != null)
            {
                if (metodo is MethodTypeData method)
                {
                    if(method.parametersL.Count() >0)
                    {
                        consola.SalidaConsola.AppendText($"Error: Cantidad de parámetros incorrecta, faltan parametros en llamada metodo: {method.GetToken().Text}. {ShowToken(currentToken)}\n");
                        return null;
                    }
                    return method.ReturnTypeGetSet;
                }
            }
            else
            {
                // Mostrar mensaje de error si no se encuentra el método en la tabla de símbolos
                consola.SalidaConsola.AppendText($"Error: No se encontró el método. {ShowToken(currentToken)}\n");

                return null;
            }
            {
                
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("Visita factor designator: " + context.designator().GetText()+" tipo: "+designatorType);
            // Devolver el tipo de designador
            return designatorType;
        }

        return null;
    }

    

    public override object VisitNumFactorAST(MiniCSharpParser.NumFactorASTContext context)
    {
        return "Int";
    }

    public override object VisitCharFactorAST(MiniCSharpParser.CharFactorASTContext context)
    {
        return "Char";
    }

    public override object VisitStringFactorAST(MiniCSharpParser.StringFactorASTContext context)
    {
        return "String";
    }

    public override object VisitBooleanFactorAST(MiniCSharpParser.BooleanFactorASTContext context)
    {
        return "Boolean";
    }

    public override object VisitNewFactorAST(MiniCSharpParser.NewFactorASTContext context)
    {
        // Obtenemos el token actual asociado al contexto
        IToken currentToken = context.Start;

        // Obtenemos el identificador del tipo
        IToken tok = (IToken)Visit(context.ident());
        

        if (context.LBRACK() != null)
        {
            string type = (string) Visit(context.expr());
            if (type != "Int")
            {
                consola.SalidaConsola.AppendText($"Error de tipos: El indice  del 'new' no es un entero, es de tipo: {type}. {ShowToken(currentToken)}\n");
                return null;
            }
            
        }

        // Buscamos en la tabla de símbolos para verificar si es una clase
        TypeData? classType = _symbolTable.Search(tok.Text);

        if (classType != null)
        {
            // Es una clase, devolvemos el tipo de estructura de la clase
            return classType.GetStructureType();
        }

        // Verificamos si es un arreglo de tipo básico (int o char)
        ArrayTypeData.ArrTypes arrType = ArrayTypeData.showType(tok.Text);

        if (arrType != ArrayTypeData.ArrTypes.Unknown)
        {
            // Es un arreglo de tipo básico, devolvemos el tipo del arreglo
            return arrType.ToString();
        }

        // Si no es una clase ni un arreglo de tipo básico, mostramos un mensaje de error
        consola.SalidaConsola.AppendText($"Error de tipos: El tipo del 'new' no existe en la tabla de símbolos ni es un arreglo de tipo básico. {ShowToken(currentToken)}\n");

        // Retornamos null para indicar un error de tipo
        return null;
    }


    public override object VisitParenFactorAST(MiniCSharpParser.ParenFactorASTContext context)
    {
        // Evaluamos la expresión contenida entre paréntesis
        string expressionType = (string)Visit(context.expr());

        // Verificamos si el tipo de la expresión es válido
        if (expressionType != null)
        {
            // Retornamos el tipo de la expresión
            return expressionType;
        }
    
        // Si el tipo de la expresión es nulo, retornamos también nulo
        return null;
    }
    

    public override object? VisitDesignatorAST(MiniCSharpParser.DesignatorASTContext context)
    {   
        IToken currentToken = context.Start;

        //preguntar si tiene parentesis es decir es una llamda a un metodo de funcion
        
        // System.Diagnostics.Debug.WriteLine("DESIGNATOR TEXTO: "+context.ident(0).GetText());
        
        
        // Buscar el tipo de la variable en la tabla de símbolos
        TypeData? typeIdent= _symbolTable.Search(context.ident(0).GetText());
        
        if(typeIdent == null)
        {
            System.Diagnostics.Debug.WriteLine($"Error: No se encontró el identificador. {ShowToken(currentToken)}\n");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("DESIGNATOR TYPE ENCONTRADO: "+ typeIdent.GetToken().Text + " y es de tipo: " + typeIdent.GetStructureType());
        }
      

        // if (typeIdent != null)
        // {
        //     //TODO: Se hace el puntero del ident hacia el contexto del var declaration
        //     context.ident(0).declPointer = typeIdent.ContextGetSet;
        //
        // }

        
        // Si el tipo de la variable es un arreglo y solo hay una expresión
        if (typeIdent is ArrayTypeData && context.expr().Length == 1) //cuando es arreglo
        {
            string typeExpr = (string) Visit(context.expr(0));
            
            // Comprobar si el tipo de la expresión es válido para un arreglo
            if (typeExpr != null)
            {
                if(typeExpr.Equals("Int"))
                {
                    return typeIdent.GetStructureType().ToLower();
                }
                // Error de tipos: El índice del arreglo no es de tipo Int
                consola.SalidaConsola.AppendText($"Error de tipos: El índice del arreglo no es de tipo Int. {ShowToken(currentToken)}\n");
            }
            
            return null;
        }

        // Si solo hay un identificador
        if (context.ident().Length == 1) 
        {
            if (context.ident(0).GetText().Equals("len"))
            {
                return "Int";
            }
            // Palabras clave especiales
            if (context.ident(0).GetText().Equals("del"))
            {
                return "Boolean";
            }
            if (context.ident(0).GetText().Equals("add"))
            {
                return "Boolean";
            }
            
            // Si el tipo de la variable es un arreglo
            if(typeIdent is ArrayTypeData)
            {
                return typeIdent.GetStructureType().ToLower()+"[]"; //int[] o char[]
            }

            // Si se encontró el tipo de la variable en la tabla de símbolos
            if (typeIdent!= null)
            {
                return typeIdent.GetStructureType();
            }
            // Si la variable no se encuentra
            consola.SalidaConsola.AppendText($"No se encontró en la tabla la variable: {context.ident(0).GetText()}. {ShowToken(currentToken)}\n");

            return null;
        }
        // Si hay más de un identificador (más de un nivel de acceso)
        if(context.ident().Length > 1) // mas de un id
        {
            // Buscar el tipo de la variable personalizada en la tabla de símbolos
            TypeData? classVar = _symbolTable.SearchClassVariable(context.ident(0).GetText());
            
            // Si se encontró el tipo de la variable
            if (classVar != null)
            {
                ClassVarTypeData tipo = (ClassVarTypeData)classVar;
                ClassTypeData classTypeData = (ClassTypeData)_symbolTable.Search(tipo.GetStructureType());
                
                // Si se encontró el tipo de la clase
                if (classTypeData != null)
                {
                    // Buscar el parámetro correspondiente en la clase
                    foreach (var parameter in  classTypeData.parametersL)
                    {
                        if (parameter.GetToken().Text.Equals(context.ident(1).GetText()))
                        {
                            return parameter.GetStructureType();
                        }
                    }
                    
                    // Si no se encontró el parámetro en la clase
                    consola.SalidaConsola.AppendText($"No se encontró la variable '{context.ident(1).GetText()}' en la clase. {ShowToken(currentToken)}\n");
                    
                    return null;
                
                }
            }
           
            // Si no se encontró el tipo de la clase o el parámetro en la clase
            consola.SalidaConsola.AppendText($"No se encontró en dicha clase '{context.ident(context.ident().Length - 2).GetText()}'. {ShowToken(currentToken)}\n");

            return null;
        }
        return null;
    }
    
    public override object VisitRelopAST(MiniCSharpParser.RelopASTContext context)
    {
        return null;
    }

    
    public override object VisitIdentAST(MiniCSharpParser.IdentASTContext context)
    {
        
  
        return context.ID().Symbol;
    }

    public override object VisitDoubleFactorAST(MiniCSharpParser.DoubleFactorASTContext context)
    {
       
        return "Double";
    }

    public override object VisitSemicolonStatementAST(MiniCSharpParser.SemicolonStatementASTContext context)
    {
        return null;
        
    }
    
}