using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum EstadoJugador
{
    Normal,
    Atacando,
    Huyendo
}

public class ControladorJugador : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 5;

    [Range(0.1f, 1f)]
    public float multiplicadorDeProfundidad = 0.6f; // 0.6 suele sentirse perfecto
    private float multiplicadorDeVelocidadAlCorrer = 1f;
    private Vector2 direccionDeInput;
    private Rigidbody2D rbJugador;

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
    public Transform puntoDeAtaque; // Un objeto vacío posicionado frente al jugador
    public float radioDeAtaque = 0.5f; // Qué tan grande es el golpe
    public int dañoDeAtaque = 1;
    public LayerMask capaEnemigos; // Para no golpearnos a nosotros mismos o a la pared

    [Header("Combate")]
    public float rangoDeProfundidad = 0.3f; // Tolerancia en el eje Y para conectar el golpe

    void Start()
    {
        rbJugador = GetComponent<Rigidbody2D>();
        
        
        posicionYVisualInicial = visualTransform.localPosition.y; 
    }

    void Update()
    {

        // Lógica para el estado Atacando (Para que no se mueva)
        if(estadoActual == EstadoJugador.Atacando)
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

        // 3. Chequeo de Ataque (Ejemplo con tecla J o Botón de Mouse)
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

        // Aquí dispararías la animación:
        // anim.SetTrigger("Atacar");

        StartCoroutine(FinalizarAtaque());
    }

    IEnumerator FinalizarAtaque()
    {
        // 1. Esperamos un poquito para que la animación del puño llegue a su extensión máxima
        yield return new WaitForSeconds(duracionAtaque / 2f);

        // 2. CREAMOS LA HITBOX: Detectamos a todos los enemigos en el área
        Collider2D[] enemigosGolpeados = Physics2D.OverlapCircleAll(puntoDeAtaque.position, radioDeAtaque, capaEnemigos);

        foreach (Collider2D enemigo in enemigosGolpeados)
        {
            // Calculamos la diferencia absoluta en el eje Y (profundidad)
            // Usamos transform.position.y porque es la posición de la raíz/pies, no del sprite que salta
            float distanciaEnProfundidad = Mathf.Abs(transform.position.y - enemigo.transform.position.y);

            // Solo aplicamos daño si están en la misma "línea" de profundidad
            if (distanciaEnProfundidad <= rangoDeProfundidad)
            {
                enemigo.GetComponent<EnemigoBase>().RecibirDaño(dañoDeAtaque);
            }
        }

        // 4. Esperamos el resto de la animación
        yield return new WaitForSeconds(duracionAtaque / 2f);
        
        // Devolvemos el control
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
}