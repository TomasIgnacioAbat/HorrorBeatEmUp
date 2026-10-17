using System.Collections;
using UnityEngine;

public class Enemigo : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float velocidadX = 3.0f;
    public float multiplicadorDeProfundidad = 0.6f;
    public float distanciaAtaqueBase = 1.3f;

    [Header("Ataque y Salud")]
    public float tiempoEntreAtaques = 2.0f;
    public int vidaMaxima = 3;
    private int vidaActual;
    private float tiempoSiguienteAtaque = 0f;

    [Header("Referencias")]
    public Transform jugadorTransform;
    public Animator animator; 

    private float velocidadY;
    private Vector2 offsetPuestoAtaque; 

    private void Start()
    {
        velocidadY = velocidadX * multiplicadorDeProfundidad;
        if (animator == null)
            animator = GetComponent<Animator>();

        vidaActual = vidaMaxima;
        float offsetX = Random.Range(0.0f, 0.8f); 
        float offsetY = Random.Range(-0.6f, 0.6f);
        offsetPuestoAtaque = new Vector2(offsetX, offsetY);
    }

    private void Update()
    {
        if (jugadorTransform == null)
        {
            GameObject jugador = GameObject.FindWithTag("Player");
            if (jugador != null)
            {
                jugadorTransform = jugador.transform;
            }
            else
            {
                return; 
            }
        }
        float direccionXJugador = Mathf.Sign(jugadorTransform.position.x - transform.position.x);
        float distanciaObjetivoX = distanciaAtaqueBase + offsetPuestoAtaque.x;
        float diferenciaX = jugadorTransform.position.x - transform.position.x;
        float diferenciaY = (jugadorTransform.position.y + offsetPuestoAtaque.y) - transform.position.y;

        bool cercaEnX = Mathf.Abs(diferenciaX) <= distanciaObjetivoX;
        bool cercaEnY = Mathf.Abs(diferenciaY) <= 0.2f;
        if (cercaEnX && cercaEnY)
        {
            if (animator != null)
            {
                animator.SetBool("EstaCaminando", false);
            }

            if (Time.time >= tiempoSiguienteAtaque)
            {
                Atacar();
                tiempoSiguienteAtaque = Time.time + tiempoEntreAtaques;
            }
        }
        else 
        {
            Vector3 posicionActual = transform.position;
            bool estaCaminando = false;
            if (!cercaEnY)
            {
                float direccionY = Mathf.Sign(diferenciaY);
                posicionActual.y += direccionY * velocidadY * Time.deltaTime;
                estaCaminando = true;
            }
            if (!cercaEnX)
            {
                float direccionX = Mathf.Sign(diferenciaX);
                posicionActual.x += direccionX * velocidadX * Time.deltaTime;

                if (Mathf.Abs(diferenciaX) > 0.15f)
                {
                    Vector3 escala = transform.localScale;
                    escala.x = Mathf.Abs(escala.x) * -direccionX;
                    transform.localScale = escala;
                }

                estaCaminando = true;
            }

            transform.position = posicionActual;

            if (animator != null)
            {
                animator.SetBool("EstaCaminando", estaCaminando);
            }
        }
        // PRUEBA TEMPORAL: Presionar 'K' para aplicarle daño al enemigo
if (Input.GetKeyDown(KeyCode.K))
{
    RecibirDano(1);
}
    }

    private void Atacar()
    {
        if (animator != null)
        {
            AnimatorStateInfo estadoActual = animator.GetCurrentAnimatorStateInfo(0);
        if (!estadoActual.IsName("punch"))
        {
            animator.SetTrigger("punch");
        }
        }
    }

    public void RecibirDano(int cantidad)
    {
        vidaActual -= cantidad;
        if (animator != null)
        {
            animator.SetTrigger("hurt");
        }
        StartCoroutine(EfectoDanoColor());
        if (vidaActual <= 0)
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator EfectoDanoColor()
{
    SpriteRenderer sprite = GetComponent<SpriteRenderer>();
    if (sprite != null)
    {
        sprite.color = Color.red; 
        yield return new WaitForSeconds(0.15f); 
        sprite.color = Color.white; 
    }
}
}