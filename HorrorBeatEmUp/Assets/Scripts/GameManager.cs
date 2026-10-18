using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Necesario para el cuadro de diálogo
using UnityEngine.SceneManagement;

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
    public SpriteRenderer fondo;
    public Sprite fondo2;

    [Header("Sistema Runner (Obstáculos y Meta)")]
    public Transform checkpointJugador;
    public Transform checkpointCriatura;
    public GameObject panelGameOver;
    public GameObject panelVictoria;

    [Header("Escenas")]
    public string nombreEscenaMenu = "MenuPrincipal"; // Pon el nombre exacto de la escena aquí

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

        yield return new WaitForSeconds(1f);

        if (panelDistorsion != null) panelDistorsion.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        if (panelDistorsion != null) panelDistorsion.SetActive(false);

        yield return new WaitForSeconds(1f);

        if (panelDistorsion != null) panelDistorsion.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        if (panelDistorsion != null) panelDistorsion.SetActive(false);

        if (textoDialogo != null)
        {
            textoDialogo.text = "¿Qué?";
            textoDialogo.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(1.5f);

        if (panelDistorsion != null) panelDistorsion.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        if (panelDistorsion != null) panelDistorsion.SetActive(false);

        // --- 3. JUGADOR MIRA A LA IZQUIERDA Y HABLA ---
        jugador.visualTransform.localScale = new Vector3(-1, 1, 1);
        if (textoDialogo != null)
        {
            textoDialogo.text = "¿Qué está pasando?";
            textoDialogo.gameObject.SetActive(true);
        }
        yield return new WaitForSeconds(1f);

        if (panelDistorsion != null) panelDistorsion.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        if (panelDistorsion != null) panelDistorsion.SetActive(false);
        fondo.sprite = fondo2;

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

        if (panelDistorsion != null) panelDistorsion.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        if (panelDistorsion != null) panelDistorsion.SetActive(false);

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
        criaturaPerseguidora.GetComponent<MonstruoRunner>().EmpezarAPerseguir(jugador.velocidadRunner);
    }

    // Esta función la llamarán los obstáculos al tocarte
    public void JugadorChocaObstaculo()
    {
        // 1. Restamos vida directamente
        jugador.vidaActual--;
        Debug.Log("Vida restante: " + jugador.vidaActual);

        // Actualizamos la UI si existe
        if (UIManager.Instance != null)
        {
            float porcentajeVida = (float)jugador.vidaActual / jugador.vidaMaxima;
            UIManager.Instance.ActualizarBarraVida(porcentajeVida);
        }

        // 2. Comprobamos si perdimos
        if (jugador.vidaActual <= 0)
        {
            MostrarGameOver();
        }
        else
        {
            // 3. Si aún hay vida, regresamos a todos al Checkpoint
            jugador.transform.position = checkpointJugador.position;
            criaturaPerseguidora.transform.position = checkpointCriatura.position;
            
            // Opcional: Feedback visual rápido
            print("¡Choque! Regresando al Checkpoint...");
        }
    }

    public void NivelCompletado()
    {
        print("¡El jugador escapó!");
        DetenerActores(); // Apagamos a los personajes en vez de congelar el tiempo
        if (panelVictoria != null) panelVictoria.SetActive(true);
    }

    public void MostrarGameOver()
    {
        print("¡El monstruo te atrapó!");
        DetenerActores(); 
        if (panelGameOver != null) panelGameOver.SetActive(true);
    }

    // Nueva función que congela solo a los personajes
    private void DetenerActores()
    {
        // 1. Frenamos y bloqueamos al jugador
        jugador.anim.SetFloat("Velocidad", 0f);
        jugador.enabled = false; 
        jugador.GetComponent<Rigidbody2D>().simulated = false;

        // 2. Apagamos el motor del monstruo para que deje de avanzar
        if (criaturaPerseguidora != null)
        {
            MonstruoRunner scriptMonstruo = criaturaPerseguidora.GetComponent<MonstruoRunner>();
            if (scriptMonstruo != null) scriptMonstruo.enabled = false;
        }
    }

    // Esta función la vas a conectar a los botones de los paneles de Victoria y Derrota
    public void IrAlMenuPrincipal()
    {
        print("Cambio de escena");
        SceneManager.LoadScene(nombreEscenaMenu);
    }
}