using Godot;
using System;

public class singletonMusica : Node
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    
    public AudioStreamPlayer musica;
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        var audio = ResourceLoader.Load("res://Musica y VFX/Batman Theme.ogg") as AudioStream;//busco el audio en los archivos
        musica = new AudioStreamPlayer();//creo un nuevo nodo audiostream player
        AddChild(musica);//agrego como hijo de root
        musica.Stream = audio;//agrego la musica a este audistreamplayer
        musica.Play();//doy play a la musica
        musica.Bus = "musica";//hago que el bus sea el de Música
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
