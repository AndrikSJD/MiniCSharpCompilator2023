using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Antlr4.Runtime;
using Compilator;
using Microsoft.Win32;
using SyntacticAnalysisGenerated;
using System.Windows.Shapes;


namespace Proyecto
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    ///
    /// Esta interfaz fue creada por Julio Cesar Castro Y Josue Orozco
    /// Se utilizo el lenguaje de programacion C# y el framework de WPF
    /// Tome en cuenta que esta interfaz es solo una propuesta, puede ser modificada
    /// Las funciones aqui implementadas son solo para mostrar como se puede implementar
    /// Las mismas estan vacias dentro de ellas esta el nombre de la funcion y un comentario que indica que es lo que hace la funcion
    /// 
    /// 
    /// </summary>
    public partial class MainWindow
    {
        
        string nombreArchivo = "";
        
        public MainWindow() 
        {
            // El System.Diagnostics.Debug.WriteLine es para imprimir en la consola del debugger.
            System.Diagnostics.Debug.WriteLine("System Diagnostics Debug");
            
            InitializeComponent();
        }

        private void Add_Tab_Button_Click(object sender, EventArgs e)
        {
            TabItem nuevaTab = new TabItem();
            TextBox nuevoTextBox = new TextBox();
            nuevoTextBox.IsReadOnly = true;
            
            // Crea un cuadro de diálogo para abrir archivos
            var openFileDialog = new OpenFileDialog
            {
                Title = "Seleccione un archivo de texto",
                Filter = "Archivos de texto (*.txt)|*.txt"
            };

            // Muestra el cuadro de diálogo y si se seleccionó un archivo, lo carga en el TextBox
            if (openFileDialog.ShowDialog() == true)
            {
                // Establece el encabezado del TabItem como el nombre del archivo seleccionada
                nuevaTab.Header = openFileDialog.SafeFileName;
                
                // Guarda el nombre del archivo seleccionado
                string archivoTexto = openFileDialog.FileName;

                // Lee el contenido del archivo seleccionado y lo asigna al TextBox
                TextReader leer = new StreamReader(archivoTexto);
                nuevoTextBox.Text = leer.ReadToEnd();
                
                // Agrega el TextBox al TabItem
                nuevaTab.Content = nuevoTextBox;

                // Agrega el nuevo TabItem al TabControl
                Tab.Items.Add(nuevaTab);
            }
        }
        
        private void closeButton_Click(object sender, EventArgs e)
        {
            //En caso de que tenga ahorita 
            if (Tab.SelectedIndex == 0)
            {
                nombreArchivo = "";
               Pantalla.Text = ""; 
            }
            else
            {
                // Obtiene el TabItem seleccionado
                TabItem selectedTab = Tab.SelectedItem as TabItem;
                // Verifica que el TabItem seleccionado no sea nulo
                if (selectedTab != null)
                {
                    //Elimina el contenido del tab item seleccionado
                    selectedTab.Content = null;
                    // Elimina el TabItem seleccionado
                    Tab.Items.Remove(selectedTab);
                }
            }
            
           
        }

        private void Pantalla_SelectionChanged(object sender, EventArgs e)
        {
            UpdateCursorPosition();
        }
        private void Pantalla_TextChanged(object sender, EventArgs e)
        {
            UpdateCursorPosition();
        }   
        
        private void UpdateCursorPosition()
        {
            int index = Pantalla.SelectionStart;
            int line = Pantalla.GetLineIndexFromCharacterIndex(index) + 1;
            int column = index - Pantalla.GetCharacterIndexFromLineIndex(Pantalla.GetLineIndexFromCharacterIndex(index)) + 1;
         
            // Actualiza el texto de un label o de otro TextBox con el número de línea y columna.
            Output.Content = $"Línea: {line} \nColumna: {column}";
        }
        
        
        public void Upload_File_Button_Click(object? sender, RoutedEventArgs e) 
        {
             
            // Aqui va la logica para subir un archivo a la ventana principal
            OpenFileDialog openFileDialog = new OpenFileDialog();
            // Establecer el directorio inicial del diálogo en la carpeta de documentos del usuario
            string documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            openFileDialog.InitialDirectory = documentsFolder;
            // Añadimos un nombre para el cuadro de subir archivo
            openFileDialog.Title = "Seleccione un archivo de texto";
            // Abre un cuadro de dialogo para subir un archivo
            openFileDialog.ShowDialog();
            // Obtiene el nombre del archivo
            string archivoTexto = openFileDialog.FileName;
            try
            {
                if (File.Exists(openFileDialog.FileName))
                {
                    //Cambiamos la ruta del arhivo abierto actualmente
                    nombreArchivo = openFileDialog.FileName;
                    //Lee el archivo y coloca la informacion en el text de la pantalla principal
                    TextReader leer = new StreamReader(archivoTexto);
                    Pantalla.Text = leer.ReadToEnd();
                    leer.Close();
                }
            }
            catch (Exception exception )
            {
                System.Diagnostics.Debug.WriteLine("Error al subir el archivo");
                System.Diagnostics.Debug.WriteLine(exception);
                throw;
            }
            
        }
        
        private void Run_Button_Click(object? sender, RoutedEventArgs e) 
        {
            if (!string.IsNullOrEmpty(nombreArchivo))
            {
                try
                {
                    // Guardar el contenido del TextBox en el archivo
                    using (StreamWriter escribir = new StreamWriter(nombreArchivo))
                    {
                        escribir.Write(Pantalla.Text);
                    }

                    
                    
                   
                }
                catch (Exception exception)
                {
                    System.Diagnostics.Debug.WriteLine("Error al guardar el archivo");
                    System.Diagnostics.Debug.WriteLine(exception);
                    throw;
                }
                
                
            //Aqui va la logica para ejecutar el codigo
            try
            {
                //Extraer texto de la pantalla principal
                ICharStream input = CharStreams.fromString(Pantalla.Text);
            
                //Escanea el texto que se le manda del input
                MiniCSharpScanner scanner = new MiniCSharpScanner(input);
                // Organiza los tokens streams
                CommonTokenStream tokens = new CommonTokenStream(scanner);
                //Le manda los tokens al parser
                MiniCSharpParser parser = new MiniCSharpParser(tokens);
                // Manejo de errores en español
                MyErrorStrategy errorStrategy = new MyErrorStrategy();
                //Remueve los error listeners
                scanner.RemoveErrorListeners();
                parser.RemoveErrorListeners();
                //Crea un nuevo error listener
                ParserErrorListener parserErrorListener = new ParserErrorListener();
                ScannerErrorListener scannerErrorListener = new ScannerErrorListener();
                //Agrega los error listener
                scanner.AddErrorListener(scannerErrorListener);
                parser.AddErrorListener(parserErrorListener);
                parser.ErrorHandler = errorStrategy;
                //Obtiene el resultado
                MiniCSharpParser.ProgramContext tree = parser.program();
            
                
                //Mostrar texto de salida
                Consola salida = new Consola();
                //Verifica si hay errores

                try
                {
                    if (parserErrorListener.HasErrors() || scannerErrorListener.HasErrors())
                    {
                        salida.SalidaConsola.Text = "Errorer de parser: " + parserErrorListener.ToString()  +
                                                    "\nErrores de escaner: "+ scannerErrorListener.ToString();
                    }
                    else
                    {
                    
                        //Analisis contextual
                        AContextual contextA = new AContextual(salida);
                        contextA.Visit(tree);
                        if (salida.SalidaConsola.Text.Contains("Error:"))
                        {
                            MessageBox.Show("Error en el analisis contextual del codigo, no se puede generar el ejecutable","Error", MessageBoxButton.OK, MessageBoxImage.Error); 
                        }
                        else
                        {
                            CodeGeneration codeGeneration = new CodeGeneration(nombreArchivo);
                            codeGeneration.Visit(tree);
                        }
                            
                    }

                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    throw;
                }
               
                salida.Show();
            }
            catch (Exception exception)
            { 
                MessageBox.Show("Error al ejecutar el codigo","Error!!", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
            }
            else
            {
                MessageBox.Show("Por favor, abra un archivo antes de ejecutar el codigo","Advertencia!!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
        private void Exit_Button_Click(object? sender, RoutedEventArgs e)
        {
            // Cierra la aplicacion
            Application.Current.Shutdown();
        }
    }
    
}
