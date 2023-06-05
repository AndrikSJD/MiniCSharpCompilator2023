using System;
using System.Windows;

namespace Compilator;

public partial class InputWind : Window
{
    public string InputValue { get; private set; }

    public InputWind(string name)
    {
        InitializeComponent();
        this.Title = "Input Variable: " +name;
        this.Output.Text = "Enter value for: "+ name;
    }

    private void Button_Click(object sender, EventArgs e)
    {
        InputValue = InputConsole.Text;
        InputConsole.Clear();
        this.Close();
    }
}