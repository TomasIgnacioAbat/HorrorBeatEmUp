using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video; // Necesario para el video

public class GameManager : MonoBehaviour
{
    [Header("Progreso del Nivel")]
    public int enemigosRestantes = 3; // Ponemos 3 porque son los que tienes en tu nivel
    public VideoPlayer reproductorVideo;
    public ControladorJugador jugador;
    public GameObject criaturaPerseguidora;
    public static GameManager Instance;
    public GameObject canvasVideo; // Arrastra el Canvas aquí en el Inspector

    void Start()
    {
        if(Instance != null)
        {
            Destroy(this);
        } else
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
        }
    }

    public void EnemigoDerrotado()
    {
        enemigosRestantes--;
        Debug.Log("Enemigo eliminado. Restantes: " + enemigosRestantes);

        if (enemigosRestantes <= 0)
        {
            // ¡Ya no quedan enemigos! Empezamos la cinemática
            IniciarTransicionRunner();
        }
    }

    // Llama a esto cuando empiece el terror
    public void IniciarTransicionRunner()
    {
        StartCoroutine(SecuenciaDeTerror());
    }

    IEnumerator SecuenciaDeTerror()
    {

        criaturaPerseguidora.SetActive(true);
        jugador.estadoActual = EstadoJugador.CinematicaHuida;
        jugador.anim.SetFloat("Velocidad", 1f);

        // 5. Esperamos 2 segundos de huida automática
        yield return new WaitForSeconds(2f);

        // 6. ¡Devolvemos el control al jugador para que esquive!
        jugador.estadoActual = EstadoJugador.Runner;
    }
}