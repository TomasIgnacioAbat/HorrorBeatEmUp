using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI Jugador")]
    public Image barraVidaAmarilla;

    [Header("UI Enemigo")]
    public GameObject panelUIEnemigo; 
    public Image barraVidaEnemigo;   

    private Coroutine corrutinaOcultarUI;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (panelUIEnemigo != null)
        {
            panelUIEnemigo.SetActive(false);
        }
    }

    public void ActualizarBarraVida(float porcentaje)
    {
        if (barraVidaAmarilla != null)
        {
            barraVidaAmarilla.fillAmount = porcentaje;
        }
    }
    public void ActualizarVidaEnemigo(int vidaActual, int vidaMaxima)
    {
        if (panelUIEnemigo == null) return;

        panelUIEnemigo.SetActive(true);
        if (barraVidaEnemigo != null)
        {
            float porcentaje = (float)vidaActual / vidaMaxima;
            barraVidaEnemigo.fillAmount = porcentaje;
        }
        if (corrutinaOcultarUI != null)
        {
            StopCoroutine(corrutinaOcultarUI);
        }

        float tiempoEspera = (vidaActual <= 0) ? 1f : 3.5f;
        corrutinaOcultarUI = StartCoroutine(OcultarUIEnemigoConRetraso(tiempoEspera));
    }

    private IEnumerator OcultarUIEnemigoConRetraso(float tiempo)
    {
        yield return new WaitForSeconds(tiempo);
        if (panelUIEnemigo != null)
        {
            panelUIEnemigo.SetActive(false);
        }
    }
}