using System.Collections;
using UnityEngine;

public class Enemigo : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float velocidadX = 3.0f;
    public float multiplicadorDeProfundidad = 0.6f;
    public float distanciaAtaqueBase = 0.85f;

    [Header("Ataque y Salud")]
    public float tiempoEntreAtaques = 2.0f;
    public int danoAtaque = 1; 
    public int vidaMaxima = 3;
    private int vidaActual;
    private float tiempoSiguienteAtaque = 0f;

    [Header("Impacto, Knockback y Muerte")]
    public float fuerzaKnockback = 1.2f;            
    public float duracionStun = 0.3f;               
    public float tiempoEsperarParaDestruir = 1.0f;  
    private bool estaAturdido = false;
    private bool estaMuerto = false;

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
        float offsetX = Random.Range(-0.4f, 0.4f); 
        float offsetY = Random.Range(-0.3f, 0.3f);
        offsetPuestoAtaque = new Vector2(offsetX, offsetY);
    }

    private void Update()
    {
        if (estaMuerto || estaAturdido) return;

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

        float distanciaObjetivoX = distanciaAtaqueBase + Mathf.Abs(offsetPuestoAtaque.x);
        float diferenciaX = (jugadorTransform.position.x + offsetPuestoAtaque.x) - transform.position.x;
        float diferenciaY = (jugadorTransform.position.y + offsetPuestoAtaque.y) - transform.position.y;

        bool cercaEnX = Mathf.Abs(diferenciaX) <= distanciaObjetivoX;
        bool cercaEnY = Mathf.Abs(diferenciaY) <= 0.25f;

        if (cercaEnX && cercaEnY)
        {
            if (animator != null)
            {
                animator.SetBool("EstaCaminando", false);
            }
            float direccionMirada = Mathf.Sign(jugadorTransform.position.x - transform.position.x);
            Vector3 escala = transform.localScale;
            escala.x = Mathf.Abs(escala.x) * -direccionMirada;
            transform.localScale = escala;

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

                if (Mathf.Abs(diferenciaX) > 0.1f)
                {
                    Vector3 escala = transform.localScale;
                    escala.x = Mathf.Abs(escala.x) * -direccionX;
                    transform.localScale = escala;
                }

                estaCaminando = true;
            }
            posicionActual += CalcularSeparacionDeOtrosEnemigos() * Time.deltaTime;

            transform.position = posicionActual;

            if (animator != null)
            {
                animator.SetBool("EstaCaminando", estaCaminando);
            }
        }
    }

    private Vector3 CalcularSeparacionDeOtrosEnemigos()
    {
        Vector3 empuje = Vector3.zero;
        float radioSeparacion = 0.6f; 

        Collider2D[] vecinos = Physics2D.OverlapCircleAll(transform.position, radioSeparacion);

        foreach (Collider2D col in vecinos)
        {
            if (col.gameObject != this.gameObject && (col.CompareTag("Enemy") || col.GetComponent<Enemigo>() != null))
            {
                Vector3 diferencia = transform.position - col.transform.position;
                float distancia = diferencia.magnitude;
                if (distancia > 0)
                {
                    empuje += diferencia.normalized / distancia;
                }
            }
        }

        return empuje * 0.8f; 
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

        if (jugadorTransform != null)
        {
            ControladorJugador jugador = jugadorTransform.GetComponent<ControladorJugador>();
            if (jugador != null)
            {
                jugador.RecibirDaño(danoAtaque);
            }
        }
    }

    public void RecibirDano(int cantidad)
    {
        if (estaMuerto) return;

        vidaActual -= cantidad;

        if (vidaActual <= 0)
        {
            Morir();
        }
        else
        {
            if (animator != null)
            {
                animator.SetTrigger("hurt");
            }

            StartCoroutine(ProcesarImpactoYKnockback());
        }
    }

    private IEnumerator ProcesarImpactoYKnockback()
    {
        estaAturdido = true;
        StartCoroutine(EfectoDanoColor());

        if (jugadorTransform != null)
        {
            float direccionEmpuje = Mathf.Sign(transform.position.x - jugadorTransform.position.x);
            Vector3 posicionObjetivo = transform.position + new Vector3(direccionEmpuje * fuerzaKnockback, 0, 0);

            float tiempoPasado = 0f;
            float duracionEmpuje = 0.12f;
            Vector3 posicionInicial = transform.position;

            while (tiempoPasado < duracionEmpuje)
            {
                transform.position = Vector3.Lerp(posicionInicial, posicionObjetivo, tiempoPasado / duracionEmpuje);
                tiempoPasado += Time.deltaTime;
                yield return null;
            }
        }

        yield return new WaitForSeconds(duracionStun);
        estaAturdido = false;
    }

    private void Morir()
    {
        estaMuerto = true;
        if (animator != null)
        {
            animator.SetTrigger("die"); 
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        StartCoroutine(EfectoDanoColor());
        Destroy(gameObject, tiempoEsperarParaDestruir);
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