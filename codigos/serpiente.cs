using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public class serpiente : Spatial
{
    [Export]
    private PackedScene Bloque;//referencia a la escena bloque precargada

    [Export]
    private PackedScene Item;//referencia a la escena que tiene el item
    private RigidBody item;//esto es para manejar el item ya instanciado en la escena
    //esto es para el tamaño del ecenario
    
    [Export]
    private int ancho = 37;//ancho del escenario
    [Export]
    private int alto = 21;//alto del escneario

    TipoCasilla casillaAOcupar;//guarda la posición de la casilla mientras no sea Game over



    [Export]
    private float FuerzaExplocion = 30f;//fuerza que sera aplicada al morir

    private Camera camara_principal;//para posicionar la camara en el centro de la escena

    //esto es para la posición inicial de la serpiente
    float posicionInicialX;//posicion en X de la serpiente inicial
    float posicionInicialY;//posicion en y de la serpiente inicial

    private RigidBody Bloque_nuevo;//referencia para guardar los bloques luego de instanciarlos
    private RigidBody cubo_personaje;//es para crear el cubo que se mueve
    private Spatial Escenario;//variable para guardar el nodo escenario
    
    private float Horizontal;//para mover horizontalmente
    private float Vertical;//para mover verticalmente

    private Vector3 DireccionSeleccionada;
    private Vector3 dirección = new Vector3(1,0,0);//inicializa moviendoce hacia la derecha


    //esto es para mover el cuerpo
    private Vector3 nuevaPosicion;//posicion nueva
    private RigidBody parteCuerpo;//parte de cuerpo
    private Spatial Cabeza;//cabeza
    
    //private Godot.Collections.Array<Spatial> cuerpo;//DE esta forma se crea un arreglo de objetos tipo spatial
    //Spatial[] cuerpo;
    private Array<RigidBody> cuerpo = new Array<RigidBody>();
    
    
    private int indice = 0;//indice para manejar el arreglo

    [Export]
    private int largoSerpiente = 16;//esto es para determinar el largo de la serpiente por defecto

    //esto es para las posiciones de la serpiente
    //enumeracion definimos un tipo de dato para un conjunto de datos en concreto
    private enum TipoCasilla
    {
        Vacio,Obstaculo,Item //3 tipos de estados
    }
    
    private TipoCasilla[,] mapa;//array bidimensional de tipo casilla

    
    //esto es para manejar el texto
    private Label Puntuacion;//referencia al nodo puntos
    private int Puntos = 0;//variable para llevar la cuenta de los puntos

    private bool GameOver = false;

    private WorldEnvironment mundo;

    //esto es para tener audio en el escenario
    private AudioStreamPlayer morirAudio;
    private AudioStreamPlayer movimientoAudio;
    private AudioStreamPlayer tomarItemAudio;

    



    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
        morirAudio = (AudioStreamPlayer)GetTree().GetNodesInGroup("morirAudio")[0];
        movimientoAudio = (AudioStreamPlayer)GetTree().GetNodesInGroup("movimientoAudio")[0];
        tomarItemAudio = (AudioStreamPlayer)GetTree().GetNodesInGroup("tomarItemAudio")[0];

        var singletonMusica = (singletonMusica)GetNode("/root/singletonMusica");//uso el singleton tendria que usar una variable global,pero no recuerdo ahora como hacer la referencia a la clase
        if(singletonMusica.musica.Playing == false)//sino se reproduce sonido
        {
            singletonMusica.musica.Play();//reprodusco sonido
        }


        Godot.GD.Randomize();//crea una semilla para que en cada inicio sea diferente la aleatoriedad
        Puntuacion = (Label)GetTree().GetNodesInGroup("puntos")[0];//busco el texto y lo inicializo en ready
        mapa = new TipoCasilla[ancho,alto];//inicializamos el array que contiene todas las posiciones con el alto y ancho
        Escenario = (Spatial)GetTree().GetNodesInGroup("Escenario")[0];//precargo el nodo escenario en una variable para tener mejor rendimiento
        CrearMuros();//llamo a la función para crear los muros
        posicionInicialX = ancho / 2;//determina el centro del esceneario
        posicionInicialY = alto / 2;//determina el centro del esceneario
        
        mundo = (WorldEnvironment)GetTree().GetNodesInGroup("mundo")[0];//referencia al mundo
        mundo.Environment.BackgroundColor = Godot.Colors.DodgerBlue;//por defecto tiene un color celeste en la función ready
        
        //por alguna razón no puedo acceder a la camara,lo dejo asi por ahora
        //camara_principal = GetNode<Camera>("camara_principal");//guardo la camara en una variable
        //camara_principal.Translation = new Vector3(posicionInicialY,posicionInicialY,36);
        
        //esto es para crear la serpiente por defecto tiene 16 cubos
        while(largoSerpiente > 0)
        {
            largoSerpiente -= 1;
            NuevoBloque(posicionInicialX-largoSerpiente,posicionInicialY);//creo un nuevo bloque teniendo en cuenta el largo de la serpiente
        }

        Cabeza = NuevoBloque(posicionInicialX,posicionInicialY);//instancio el cubo con la posición al centro y lo guardo en la variable
       
        InstanciarItemEnPosicionAleatoria();//con esto el item aparece en una posición aletoria en la escena
    }


    private void IncrementarPuntos()//función para incrementar los puntos
    {
        Puntos ++;//aumento puntos
        Puntuacion.Text = Puntos.ToString();//coloco la puntuación..valor númerico a string
    }

    private void InstanciarItemEnPosicionAleatoria()//funcion que instanciara en posicion aletaria el item
    {
        Vector3 posicion = ObtenerPosicionVacia();//esta función nos devuelvo la X y Y de la posición
        item = NuevoItem((int)posicion.x,(int)posicion.y);//tengo la referencia al nuevo item
    }

    private Vector3 ObtenerPosicionVacia()//recorre todas las posiciones que tiene el array mapa y guardar en una lista que esten marcadas vacias
    {
        List<Vector3> posicionesVacias = new List<Vector3>();//lista que contendra las posiciones vacias
        //recorremos las posiciones
        for (int x = 0; x < ancho; x++)//recorre posiciones en X
        {
            for (int y = 0; y < alto; y++)//recorre posiciones en y
            {
                if (mapa[x,y] == TipoCasilla.Vacio)//si hay posiciones vacias
                {
                    posicionesVacias.Add(new Vector3(x,y,0));//tenemos todas las posiciones vacias
                }
            }
        }
        return posicionesVacias[(int)Godot.GD.RandRange(0,posicionesVacias.Count)];//la posicion aleatoria depende de la cantidad de objetos que tiene la lista
    }

    private RigidBody NuevoItem(int x,int y)//esto instancia un nuevo item al escenario

    {
        RigidBody nuevo = (RigidBody)Item.Instance();//instancio un nuevo item
        Escenario.AddChild(nuevo);//lo agrego como hijo de escenario
        nuevo.Translation = new Vector3(x,y,0);//la posición la recibo por parametro
        nuevo.RotationDegrees = Godot.Vector3.Zero;//la rotación es igual a cero
        EstablecerMapa(nuevo.Translation,TipoCasilla.Item);//en esta posición hay un ITEM
        return nuevo;//devuelvo ese item
    }
    
    private void MoverItemAPosicionAleatoria()//mueve el item a una nueva posición
    {
        Vector3 posicion = ObtenerPosicionVacia();//obtenemos la posición vacia
        item.Translation = posicion;//es la nueva posición del item
        EstablecerMapa(item.Translation,TipoCasilla.Item);//en cuanto movemos a un sitio nuevo marcamos como una posición de item

    }    

    private TipoCasilla ObtenerMapa(Vector3 posicion)
    {
        return mapa[Convert.ToInt16(posicion.x),Convert.ToInt16(posicion.y)];//obtenemos lo que hay en el mapa en base a una posición con numero entero
        
    }

    private void EstablecerMapa(Vector3 posicion,TipoCasilla valor)
    {
        mapa[Convert.ToInt16(posicion.x),Convert.ToInt16(posicion.y)] = valor; //accedemos a la posición que sea igual al valor para determinar si es obstaculo o vacio
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)//funcion que se procesa 60 veces por segundo
    {
        Horizontal = Input.GetActionStrength("d") - Input.GetActionStrength("a");//si se mueve izquierda o derecha
        Vertical = Input.GetActionStrength("w") - Input.GetActionStrength("s");//si muevo arriba o abajo
        DireccionSeleccionada = new Vector3(Horizontal,Vertical,0);//vector para mover 
        if (DireccionSeleccionada != Godot.Vector3.Zero)//si la dirección es distinta de cero
        {
            movimientoAudio.Stop();//cuando muevo la serpiente no hay audio
            dirección = DireccionSeleccionada;//la dirección es igual a dirección seleccionada
            //GD.Print(dirección);//veo el mensaje para comproar la posición al presionar las teclas
        }

    }

    private void CrearMuros() //Función para crear los muros
    {
        //dibujar el escenario
        for (int x = 0; x < ancho; x++)
        {
            for (int y = 0; y < alto; y++)
            {
                //para dibujar solo los bordes
                if(x==0 || x==ancho-1 || y==0 || y==alto-1)
                {
                    Bloque_nuevo = (RigidBody)Bloque.Instance();//instancio la escena pre cargada
                    Escenario.AddChild(Bloque_nuevo);//hago que esta escena sea hija de escenario
                    Vector3 posicion = new Vector3(x,y,0);//posición de cada cubo
                    Bloque_nuevo.Translation = posicion;//posición del cubo
                    Bloque_nuevo.RotationDegrees = Godot.Vector3.Zero;//la rotación es cero

                    EstablecerMapa(posicion,TipoCasilla.Obstaculo);//marcamos esta parte del mapa como obstaculo
                }
            }
        }
    }


    private RigidBody NuevoBloque(float x,float y)//función que crea la cabeza y retorna ese bloque
    {
        cubo_personaje = (RigidBody)Bloque.Instance();//instancio el bloque
        this.AddChild(cubo_personaje);//agrego como hijo a la serpiente,osea es la cola
        cubo_personaje.Translation = new Vector3(x,y,0);//la posición la recibo por parametro
        cubo_personaje.RotationDegrees = Godot.Vector3.Zero;//la rotación es cero
        cuerpo.Add(cubo_personaje);//agrego el cubo al arreglo para tenerlo como referencia
        
        EstablecerMapa(cubo_personaje.Translation,TipoCasilla.Obstaculo);//las partes del cuerpo creadas son un obstaculo

        return cubo_personaje;
    }


    //hago que la función sea de tipo async para poder deterner el flujo del codigo por un tiempo antes de morir
    //vemos que se repite muchas veces perdiste,pero por ahora lo dejamos asi
    //ya que funciona correctamente
    private async void _on_TimerMovimiento_timeout() //de esta forma creo la señal en Godot c#
    {
        
        nuevaPosicion = Cabeza.Translation + dirección;//posición nueva del cubo
        
        if(!GameOver)//sino no es game over
        {
            casillaAOcupar = ObtenerMapa(nuevaPosicion);//obtenemos la posición en el mapa
        }
        
        if(casillaAOcupar == TipoCasilla.Obstaculo)//si nos posicionamos ensima de un obstaculo
        {
            if(!GameOver)//sino es game over
            {
                
                Muerto();//funcion que determina cuando estoy muerto
                GameOver = true;
                await ToSignal(GetTree().CreateTimer(5.0f),"timeout");//esperamos 2 segundos antes de reiniciar
                GetTree().ReloadCurrentScene();//reinicio la escena
                
            }
            
        }

        else //sino nos movimos a un obstaculo
        {
            
            //cuando agarramos un item la serpiente crece
            if(casillaAOcupar == TipoCasilla.Item)//si hay un item en esa posición
            {
       
                tomarItemAudio.Play();//reproduce audio tomar item
                parteCuerpo = NuevoBloque(nuevaPosicion.x,nuevaPosicion.y);//se crea justo donde la serpiente debe de moverse
                MoverItemAPosicionAleatoria();//creo un nuevo item en una posición aleatoria
                IncrementarPuntos();//si agarro un item incremento los puntos
            }
            
            //para que se mueva la parte de la cola agregada despues de tomar el ITEM no hay que usar el else
            parteCuerpo = cuerpo[indice];//uso el indice para saber que parte del arreglo tengo que mover
            EstablecerMapa(parteCuerpo.Translation,TipoCasilla.Vacio);//donde se movera la serpiente esta vacio
            parteCuerpo.Translation = nuevaPosicion;//con esto muevo el cuerpo
            EstablecerMapa(parteCuerpo.Translation,TipoCasilla.Obstaculo);//una ves que se movio a la nueva posición es un obstaculo
            cuerpo.Add(parteCuerpo);//vuelvo a colocarlo en la lista    
            
            Cabeza = parteCuerpo;//esta parte del cuerpo sera la nueva cabeza
            indice += 1;//sumamos para mover la proxima parte de cuerpo/por ahora funciona,luego capas hay que correguirlo
            movimientoAudio.Play();//reproduce audio movimiento
        
        }    
    }

    private void Muerto()//este metodo hace que todas las piezas exploten,cada bloque fue cambiado a rigidbody y por defecto no se aplica fuerzas
    {
       //llamo al metodo explotar con un array de rigidbody
       //tengo todos los riggidbody de todas las piezas de la serpiente
       Explotar(GetChildren());//hago explotar todos los objetos que son hija de serpiente
       Explotar(Escenario.GetChildren());//hace que explote todos los objetos que son hijos del nodo escenario incluyendo el item
       mundo.Environment.BackgroundColor = Godot.Colors.Red;//cambio el color del mundo a rojo usando los colores pre establecidos de Godot 
       Puntuacion.Set("custom_colors/font_color",new Color(1,1,1,0.8f));//accedo al parametro custom color y lo hago más visible
       GD.Print("perdiste");//muestro por consola un mensaje diciendo que perdiste
       var singletonMusica = (singletonMusica)GetNode("/root/singletonMusica");
       singletonMusica.musica.Stop();//detengo la musica
       morirAudio.Play();//reproduce sonido de morir
    }

                
    
       

    private void Explotar(Godot.Collections.Array CuerposRigidos)//obtengo lo cuerpos rigidos
    {
        //para recorrer el arreglo de colecciones Godot
        //uso un foreach pero como tipo dato usamos "var"
        foreach (var i in CuerposRigidos) 
        {
            (i as RigidBody).Sleeping = false;//tiene efectos de gravedad
            
            (i as RigidBody).ApplyCentralImpulse(new Vector3(
                (float)Godot.GD.RandRange(-FuerzaExplocion,FuerzaExplocion),//aplica un impulso aleatorio entre -10 y 10 eje X
                (float)Godot.GD.RandRange(-FuerzaExplocion,FuerzaExplocion),//aplica un impulso aleatorio entre -2 y 10 eje y
                0));//*/
            
            //hago que gire aleatoriamente con un impulso
            (i as RigidBody).ApplyTorqueImpulse
            (new Vector3(
                (float)Godot.GD.RandRange(-2,2),//aplica un impulso aleatorio entre -10 y 10 eje X
                (float)Godot.GD.RandRange(-2,2),//aplica un impulso aleatorio entre -2 y 10 eje y
                0));//*/
        }
    }
}

   

        
                
            
            

