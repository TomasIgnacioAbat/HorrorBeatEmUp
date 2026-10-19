using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Nombre de la escena de juego")]
    public string nombreEscenaJuego = "EscenaDeJugador";
    public AudioClip musicaMenu;
    public AudioClip sonidoPlay;

    void Start()
    {
        AudioManager.Instance.ReproducirMusica(musicaMenu);
    }

    void Update()
    {
        if (Input.anyKeyDown && !Input.GetKeyDown(KeyCode.Escape))
        {
            Jugar();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Salir();
        }
    }

    public void Jugar()
    {
        AudioManager.Instance.ReproducirSonido(sonidoPlay, 1);
        SceneManager.LoadScene(nombreEscenaJuego);
    }

    public void Salir()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}