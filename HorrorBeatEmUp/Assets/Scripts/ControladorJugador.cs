using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum EstadoJugador
{
    Normal,
    Atacando,
    RecibiendoDaño,
    Huyendo
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
    public Transform pies;

    [Header("Salto Falso")]
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
    public float tiempoDeAturdimiento = 0.4f; 
    public float tiempoDeInvulnerabilidad = 1f; 
    private bool esInvulnerable = false;

    void Start()
    {
        rbJugador = GetComponent<Rigidbody2D>();
        
        
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

        if (movX > 0)
        {
            visualTransform.localScale = new Vector3(1, 1, 1);
        }
        else if (movX < 0)
        {
            visualTransform.localScale = new Vector3(-1, 1, 1);
        }

        ManejarSaltoFalso();

        
        if (Input.GetKeyDown(KeyCode.J))
        {
            IniciarAtaque();
        }
    }

    void FixedUpdate()
    {
        
        rbJugador.MovePosition(rbJugador.position + direccionDeInput * (velocidad * multiplicadorDeVelocidadAlCorrer) * Time.fixedDeltaTime);
    }

    void ManejarSaltoFalso()
    {
        
        if (Input.GetKeyDown(KeyCode.K) && !estaSaltando)
        {
            estaSaltando = true;
            velocidadVerticalActual = fuerzaDeSalto;
        }

        if (estaSaltando)
        {
            
            velocidadVerticalActual -= gravedadFalsa * Time.deltaTime;
            
            
            Vector3 nuevaPosicionLocal = visualTransform.localPosition;
            nuevaPosicionLocal.y += velocidadVerticalActual * Time.deltaTime;
            visualTransform.localPosition = nuevaPosicionLocal;

            
            if (visualTransform.localPosition.y <= posicionYVisualInicial)
            {
                
                visualTransform.localPosition = new Vector3(visualTransform.localPosition.x, posicionYVisualInicial, visualTransform.localPosition.z);
                estaSaltando = false;
                velocidadVerticalActual = 0f;
            }
        }
    }

    void IniciarAtaque()
    {
        estadoActual = EstadoJugador.Atacando;

        // Para hacer la animación de ataque
        // anim.SetTrigger("Atacar");

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

        // 4. Espera el resto de la animación
        yield return new WaitForSeconds(duracionAtaque / 2f);
        
        // Devuelve el control del personaje
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
        // Si ya está invulnerable, o si ya pasó a la fase de terror/huida, ignora el golpe
        if (esInvulnerable || estadoActual == EstadoJugador.Huyendo) return;

        vidaActual -= daño;
        Debug.Log("Vida del jugador: " + vidaActual);

        if (vidaActual <= 0)
        {
            // Luego llamamos al GameManager o parecido para la pantalla de Game Over
            Debug.Log("¡Jugador Derrotado!");
            return;
        }

        StartCoroutine(ProcesarImpacto());
    }

    IEnumerator ProcesarImpacto()
    {
        // 1. Bloquea al jugador y lo hace invulnerable
        estadoActual = EstadoJugador.RecibiendoDaño;
        esInvulnerable = true;
        
        // Feedback visual rápido: pintamos el sprite de rojo
        SpriteRenderer sprite = visualTransform.GetComponent<SpriteRenderer>();
        sprite.color = Color.red;
        
        // Acá dispararía la animación: anim.SetTrigger("RecibirDaño");

        // 2. Espera a que pase el aturdimiento
        yield return new WaitForSeconds(tiempoDeAturdimiento);

        // 3. Devolvemos el control, pero sigue invulnerable
        estadoActual = EstadoJugador.Normal;
        sprite.color = new Color(1f, 1f, 1f, 0.5f); // Lo hace semitransparente

        // 4. Esperamos el resto del tiempo de gracia (iFrames)
        yield return new WaitForSeconds(tiempoDeInvulnerabilidad);

        // 5. Vuelve completamente a la normalidad
        esInvulnerable = false;
        sprite.color = Color.white;
    }

}