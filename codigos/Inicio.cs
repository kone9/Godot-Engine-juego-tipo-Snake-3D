using Godot;
using System;

public class Inicio : Spatial
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";proximo
    [Export]
    PackedScene escenaPrincipal;

    private Timer tiempo;
    private ColorRect colorFondo;
    private Label Cuenta;
    private int tiempoRestante;


    bool reproducirSonido = true;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        colorFondo = GetNode<ColorRect>("ColorRect");//tomo el nodo color rect
        colorFondo.Color = Godot.Colors.DodgerBlue;//cambio el color del fondo
        tiempo = GetNode<Timer>("TimerConteo");//tomo el nodo timer
        Cuenta = GetNode<Label>("Cuenta");//tomo el nodo de tipo texto label 
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        tiempoRestante = (int)tiempo.TimeLeft + 1;//para que el tiempo sea un número entero
        Cuenta.Text = tiempoRestante.ToString();//el texto sera el tiempo que va el timer
        

    }


    public void _on_TimerConteo_timeout()
    {
        
        GetTree().ChangeSceneTo(escenaPrincipal);
    }
}
