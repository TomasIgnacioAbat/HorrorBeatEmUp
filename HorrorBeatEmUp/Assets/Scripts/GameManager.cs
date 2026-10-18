using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video; // Necesario para el video

public class GameManager : MonoBehaviour
{
    [Header("Progreso del Nivel")]
    public int enemigosRestantes = 3;
    public VideoPlayer reproductorVideo;
    public ControladorJugador jugador;
    public GameObject criaturaPerseguidora;
    public static GameManager Instance;
    public GameObject muroDerecha;

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
            IniciarTransicionRunner();
        }
    }

    public void IniciarTransicionRunner()
    {
        muroDerecha.SetActive(false);
        StartCoroutine(SecuenciaDeTerror());
    }

    IEnumerator SecuenciaDeTerror()
    {

        criaturaPerseguidora.SetActive(true);
        jugador.estadoActual = EstadoJugador.CinematicaHuida;
        print("Jugador comenzó la cinematica");
        jugador.anim.SetFloat("Velocidad", 1f);

        yield return new WaitForSeconds(1f); // Cambiar a 2f si agregamos la cinematica de aparicion del monstruo
        print("Jugador comenzó a correr");
        jugador.estadoActual = EstadoJugador.Runner;
    }
}