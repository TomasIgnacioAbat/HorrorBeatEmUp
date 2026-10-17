using UnityEngine;

public class Enemigo : MonoBehaviour
{
    [Header("Configuracion de Movimiento")]
    public float velocidadX = 3.0f;
    public float multiplicadorDeProfundidad = 0.6f;
    public float distanciaAtaque = 1.2f;

    [Header("Referencias")]
    public Transform jugadorTransform;
    public Animator animator; 

    private float velocidadY;

    private void Awake()
    {
        velocidadY = velocidadX * multiplicadorDeProfundidad;
        if (animator == null)
            animator = GetComponent<Animator>();
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

        float diferenciaX = jugadorTransform.position.x - transform.position.x;
        float diferenciaY = jugadorTransform.position.y - transform.position.y;

        Vector3 posicionActual = transform.position;
        bool estaCaminando = false;

        if (Mathf.Abs(diferenciaY) > 0.1f)
        {
            float direccionY = Mathf.Sign(diferenciaY);
            posicionActual.y += direccionY * velocidadY * Time.deltaTime;
            estaCaminando = true;
        }

        if (Mathf.Abs(diferenciaX) > distanciaAtaque)
        {
            float direccionX = Mathf.Sign(diferenciaX);
            posicionActual.x += direccionX * velocidadX * Time.deltaTime;

            Vector3 escala = transform.localScale;
            escala.x = Mathf.Abs(escala.x) * -direccionX;
            transform.localScale = escala;

            estaCaminando = true;
        }

        transform.position = posicionActual;

        if (animator != null)
        {
            animator.SetBool("EstaCaminando", estaCaminando);
        }
    }
}