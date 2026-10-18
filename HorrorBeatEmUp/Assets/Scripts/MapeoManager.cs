using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MapeoManager : MonoBehaviour
{
    [Header("Referencias de UI")]
    public TextMeshProUGUI textoParpadeante;

    [Header("Configuración")]
    public float velocidadParpadeo = 1.5f;
    public string nombreEscenaJuego = "EscenaDeJugador";

    private bool cambiandoEscena = false;

    private void Start()
    {
        if (textoParpadeante != null)
        {
            StartCoroutine(EfectoParpadeo());
        }
    }

    private void Update()
    {
        if (!cambiandoEscena && (Input.GetMouseButtonDown(0) || Input.anyKeyDown))
        {
            cambiandoEscena = true;
            SceneManager.LoadScene(nombreEscenaJuego);
        }
    }
    private IEnumerator EfectoParpadeo()
    {
        while (true)
        {
            float alpha = Mathf.PingPong(Time.time * velocidadParpadeo, 1.0f);
            Color c = textoParpadeante.color;
            c.a = alpha;
            textoParpadeante.color = c;
            yield return null;
        }
    }
}