using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum EstadoJugador
{
    Normal,
    Atacando,
    RecibiendoDaño,
    CinematicaHuida,
    Runner
}

public class ControladorJugador : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 5;

    [Range(0.1f, 1f)]
    public float multiplicadorDeProfundidad = 0.6f; 
    private float multiplicadorDeVelocidadAlCorrer = 1f;
    private Vector2 direccionDeInput;
    private Rigidbody2D rbJugador;

    [Header("Salto Falso")]
    public Collider2D colisionadorPies; // Arrastra aquí el objeto "Pies" (o el collider de los pies)
    public Transform visualTransform;
    public float fuerzaDeSalto = 10f;
    public float gravedadFalsa = 25f;
    
    private bool estaSaltando = false;
    private float velocidadVerticalActual = 0f;
    private float posicionYVisualInicial;

    [Header("Maquina de Estados")]
    public EstadoJugador estadoActual = EstadoJugador.Normal;
    public float duracionAtaque = 0.5f; // 1f son 1000 milisegundos.

    [Header("Combate")]
    public Transform puntoDeAtaque; 
    public float radioDeAtaque = 0.5f; 
    public int dañoDeAtaque = 1;
    public LayerMask capaEnemigos; 

    [Header("Combate")]
    public float rangoDeProfundidad = 0.3f; // Tolerancia en el eje Y para conectar el golpe

    [Header("Salud")]
    public int vidaActual = 5;
    public int vidaMaxima = 5;
    public float tiempoDeAturdimiento = 0.4f; 
    public float tiempoDeInvulnerabilidad = 1f; 
    private bool esInvulnerable = false;

    [Header("Animación")]
    public Animator anim;

    [Header("Modo Runner")]
    public float velocidadRunner = 7f;
    public float velocidadCambioCarril = 15f;
    public float[] posicionesYCarriles = new float[3] { 1.5f, 0f, -1.5f }; // Arriba, Medio, Abajo
    private int carrilActual = 1; // 0 = Arriba, 1 = Medio, 2 = Abajo

    [Header("Sonidos")]
    public AudioClip sonidoDeGolpe;

    void Start()
    {
        rbJugador = GetComponent<Rigidbody2D>();
        vidaActual = vidaMaxima;
        if(UIManager.Instance != null)
        { 
            UIManager.Instance.ActualizarBarraVida(1f);
        }
        
        posicionYVisualInicial = visualTransform.localPosition.y; 
    }

    void Update()
    {

        if(estadoActual == EstadoJugador.Atacando || estadoActual == EstadoJugador.RecibiendoDaño)
        {
            direccionDeInput = Vector2.zero;
            return;
        }
        
        // Lógica para el estado Normal o Huyendo.
        float movX = Input.GetAxisRaw("Horizontal");
        float movY = Input.GetAxisRaw("Vertical");

        Vector2 direccionBase = new Vector2(movX, movY).normalized;


        direccionDeInput = new Vector2(direccionBase.x, direccionBase.y * multiplicadorDeProfundidad);

        if (Input.GetKey(KeyCode.L)) {
            multiplicadorDeVelocidadAlCorrer = 2f; 
        } else {
            multiplicadorDeVelocidadAlCorrer = 1f;
        }

        if (movX > 0 && !(estadoActual == EstadoJugador.Runner))
        {
            visualTransform.localScale = new Vector3(1, 1, 1);
        }
        else if (movX < 0 && !(estadoActual == EstadoJugador.Runner))
        {
            visualTransform.localScale = new Vector3(-1, 1, 1);
        }

        ManejarSaltoFalso();

        
        if (Input.GetKeyDown(KeyCode.J) && estadoActual != EstadoJugador.CinematicaHuida && estadoActual != EstadoJugador.Runner)
        {
            IniciarAtaque();
        }

        if (estadoActual == EstadoJugador.Runner)
        {
            // 1. Cambio de carril con Arriba/Abajo o W/S
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (carrilActual > 0) carrilActual--;
            }
            else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (carrilActual < 2) carrilActual++;
            }

            // 2. El salto falso (aprovechamos la función que ya tenías)
            ManejarSaltoFalso();
        }

        // --- CONTROL DE ANIMACIONES ---
        if (estadoActual == EstadoJugador.Runner || estadoActual == EstadoJugador.CinematicaHuida)
        {
            // En modo Runner o Cinematica, forzamos la animación de correr siempre
            anim.SetFloat("Velocidad", 1f); 
        }
        else
        {
            // En modo Normal (Beat 'em up), depende de si el jugador toca las teclas
            anim.SetFloat("Velocidad", direccionDeInput.magnitude); 
        }
    }

    void FixedUpdate()
    {
        if (estadoActual == EstadoJugador.Normal)
        {
            // Tu movimiento Beat 'em up original
            rbJugador.MovePosition(rbJugador.position + direccionDeInput * (velocidad * multiplicadorDeVelocidadAlCorrer) * Time.fixedDeltaTime);
        }
        else if (estadoActual == EstadoJugador.Runner || estadoActual == EstadoJugador.CinematicaHuida)
        {
            // MOVIMIENTO RUNNER: Corre a la derecha sin parar
            Vector2 nuevaPos = rbJugador.position;
            nuevaPos.x += velocidadRunner * Time.fixedDeltaTime;
            
            // Se mueve suavemente hacia el carril seleccionado en Y
            float yObjetivo = posicionesYCarriles[carrilActual];
            nuevaPos.y = Mathf.MoveTowards(nuevaPos.y, yObjetivo, velocidadCambioCarril * Time.fixedDeltaTime);
            
            rbJugador.MovePosition(nuevaPos);
        }
    }

    void ManejarSaltoFalso()
    {
        // 1. Al iniciar el salto
        if ((Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.Space)) && !estaSaltando)
        {
            estaSaltando = true;
            velocidadVerticalActual = fuerzaDeSalto;
            anim.SetTrigger("Salto");

            // NUEVO: Desactivamos el colisionador para que los obstáculos lo ignoren en el aire
            if (colisionadorPies != null) colisionadorPies.enabled = false; 
        }

        if (estaSaltando)
        {
            velocidadVerticalActual -= gravedadFalsa * Time.deltaTime;
            
            Vector3 nuevaPosicionLocal = visualTransform.localPosition;
            nuevaPosicionLocal.y += velocidadVerticalActual * Time.deltaTime;
            visualTransform.localPosition = nuevaPosicionLocal;

            // 2. Al tocar el suelo de nuevo
            if (visualTransform.localPosition.y <= posicionYVisualInicial)
            {
                visualTransform.localPosition = new Vector3(visualTransform.localPosition.x, posicionYVisualInicial, visualTransform.localPosition.z);
                estaSaltando = false;
                velocidadVerticalActual = 0f;
                anim.SetTrigger("EstaEnElSuelo");

                // NUEVO: Volvemos a activar el colisionador para que pueda chocar de nuevo
                if (colisionadorPies != null) colisionadorPies.enabled = true; 
            }
        }
    }

    void IniciarAtaque()
    {
        estadoActual = EstadoJugador.Atacando;

        anim.SetTrigger("Ataque");

        AudioManager.Instance.ReproducirSonido(sonidoDeGolpe, 1);

        StartCoroutine(FinalizarAtaque());
    }

    IEnumerator FinalizarAtaque()
    {

        yield return new WaitForSeconds(duracionAtaque / 2f);

        Collider2D[] enemigosGolpeados = Physics2D.OverlapCircleAll(puntoDeAtaque.position, radioDeAtaque, capaEnemigos);

        foreach (Collider2D enemigo in enemigosGolpeados)
        {

            float distanciaEnProfundidad = Mathf.Abs(transform.position.y - enemigo.transform.position.y);

            // Solo aplica daño si están en la misma "línea" de profundidad
            if (distanciaEnProfundidad <= rangoDeProfundidad)
            {
                enemigo.GetComponent<Enemigo>().RecibirDano(dañoDeAtaque);
            }
        }

        yield return new WaitForSeconds(duracionAtaque / 2f);
        
        estadoActual = EstadoJugador.Normal;
    }

    // Esto es un truco buenísimo: Dibuja un círculo rojo en el editor de Unity 
    // para que puedas ver el tamaño exacto de tu golpe sin tener que adivinar.
    void OnDrawGizmosSelected()
    {
        if (puntoDeAtaque == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(puntoDeAtaque.position, radioDeAtaque);
    }

    public void RecibirDaño(int daño)
    {
        if (esInvulnerable || estadoActual == EstadoJugador.CinematicaHuida || estadoActual == EstadoJugador.Runner) return;

        vidaActual -= daño;
        Debug.Log("Vida del jugador: " + vidaActual);
        if (UIManager.Instance != null)
        { 
            float porcentajeVida = (float)vidaActual / vidaMaxima;
            UIManager.Instance.ActualizarBarraVida(porcentajeVida);
        }

        if (vidaActual <= 0)
    {
        Debug.Log("¡Jugador Derrotado!");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.MostrarGameOver();
        }
        return; 
    }
        
        StartCoroutine(ProcesarImpacto());
    }

    IEnumerator ProcesarImpacto()
    {
        estadoActual = EstadoJugador.RecibiendoDaño;
        esInvulnerable = true;

        anim.SetTrigger("Daño");
        
        SpriteRenderer sprite = visualTransform.GetComponent<SpriteRenderer>();
        sprite.color = Color.red;
        

        yield return new WaitForSeconds(tiempoDeAturdimiento);

        estadoActual = EstadoJugador.Normal;
        sprite.color = new Color(1f, 1f, 1f, 0.5f); // Lo hace semitransparente

        // 4. Esperamos el resto del tiempo de gracia (iFrames)
        yield return new WaitForSeconds(tiempoDeInvulnerabilidad);

        esInvulnerable = false;
        sprite.color = Color.white;
    }

}