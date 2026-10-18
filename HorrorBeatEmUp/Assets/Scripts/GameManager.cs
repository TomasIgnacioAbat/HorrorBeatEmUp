using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Necesario para el cuadro de diálogo

public class GameManager : MonoBehaviour
{
    [Header("Progreso del Nivel")]
    public int enemigosRestantes = 3;
    public ControladorJugador jugador;
    public GameObject criaturaPerseguidora;
    public GameObject muroDerecha;
    
    [Header("Cinemática UI")]
    public GameObject panelDistorsion; // El panel rojo que tapa la pantalla
    public TextMeshProUGUI textoDialogo;

    [Header("Actores Cinemática")]
    public Transform puntoDestinoJugador; // NUEVO: A dónde camina el jugador al ganar
    public float velocidadCaminataJugador = 3f; // NUEVO: Qué tan rápido va a su marca
    public Transform puntoAparicionCriatura; 
    public Transform puntoEntradaCriatura; 
    public float velocidadCaminataCriatura = 4f;

    public static GameManager Instance;

    void Start()
    {
        if(Instance != null)
        {
            Destroy(this.gameObject); 
        } 
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject); 
        }
    }

    public void EnemigoDerrotado()
    {
        enemigosRestantes--;
        Debug.Log("Enemigo eliminado. Restantes: " + enemigosRestantes);

        if (enemigosRestantes <= 0)
        {
            IniciarTransicionRunner();
        }
    }

    public void IniciarTransicionRunner()
    {
        muroDerecha.SetActive(false); // Abrimos el camino
        StartCoroutine(SecuenciaDeTerror());
    }

    IEnumerator SecuenciaDeTerror()
    {
        print("Jugador comenzó la cinematica");
        
        // --- 1. BLOQUEAR AL JUGADOR Y APAGAR FÍSICAS ---
        jugador.estadoActual = EstadoJugador.CinematicaHuida;
        
        // Truco vital: Apagamos el Rigidbody para que el script del jugador no intente
        // hacerlo correr automáticamente hacia la derecha mientras lo guiamos a su marca.
        jugador.enabled = false;
        jugador.GetComponent<Rigidbody2D>().simulated = false;

        // --- 1.5. EL JUGADOR CAMINA A SU MARCA ---
        // Hacemos que mire hacia la marca (por si estaba mirando al otro lado al dar el último golpe)
        if (puntoDestinoJugador.position.x < jugador.transform.position.x)
            jugador.visualTransform.localScale = new Vector3(-1, 1, 1);
        else
            jugador.visualTransform.localScale = new Vector3(1, 1, 1);

        jugador.anim.SetFloat("Velocidad", 1f); // Activamos animación de caminar

        // Lo movemos hasta la coordenada exacta
        while (Vector2.Distance(jugador.transform.position, puntoDestinoJugador.position) > 0.1f)
        {
            jugador.transform.position = Vector3.MoveTowards(
                jugador.transform.position, 
                puntoDestinoJugador.position, 
                velocidadCaminataJugador * Time.deltaTime
            );
            yield return null; 
        }

        // Llegó a la marca, lo frenamos
        jugador.anim.SetFloat("Velocidad", 0f); 

        // --- 2. EFECTO DISTORSIÓN (ESTÁTICA ROJA) ---
        if (panelDistorsion != null) panelDistorsion.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        if (panelDistorsion != null) panelDistorsion.SetActive(false);

        // --- 3. JUGADOR MIRA A LA IZQUIERDA Y HABLA ---
        jugador.visualTransform.localScale = new Vector3(-1, 1, 1);
        if (textoDialogo != null)
        {
            textoDialogo.text = "¿Qué está pasando?";
            print("Aparece Texto");
            textoDialogo.gameObject.SetActive(true);
        }
        yield return new WaitForSeconds(1f);

        // --- 4. LA CRIATURA ENTRA A ESCENA ---
        criaturaPerseguidora.transform.position = puntoAparicionCriatura.position;
        criaturaPerseguidora.SetActive(true);
        
        while (Vector2.Distance(criaturaPerseguidora.transform.position, puntoEntradaCriatura.position) > 0.1f)
        {
            criaturaPerseguidora.transform.position = Vector3.MoveTowards(
                criaturaPerseguidora.transform.position, 
                puntoEntradaCriatura.position, 
                velocidadCaminataCriatura * Time.deltaTime
            );
            yield return null; 
        }

        yield return new WaitForSeconds(1f); // Pausa dramática cuando se miran

        // --- 5. JUGADOR DICE OH NO ---
        if (textoDialogo != null) textoDialogo.text = "¡Oh no!";
        yield return new WaitForSeconds(1f);

        // --- 6. COMIENZA LA PERSECUCIÓN ---
        if (textoDialogo != null) textoDialogo.gameObject.SetActive(false);
        jugador.visualTransform.localScale = new Vector3(1, 1, 1); // Vuelve a mirar a la derecha
        
        print("Jugador comenzó a correr");
        
        // Devolvemos las físicas a la normalidad para que el Runner funcione bien
        jugador.estadoActual = EstadoJugador.Runner;
        jugador.enabled = true;
        jugador.GetComponent<Rigidbody2D>().simulated = true;
    }
}