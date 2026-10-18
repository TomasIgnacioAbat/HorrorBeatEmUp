using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Nombre de la escena de juego")]
    public string nombreEscenaJuego = "EscenaDeJugador";

    void Update()
    {
        if (Input.anyKeyDown)
        {
            Jugar();
        }
    }

    public void Jugar()
    {
        SceneManager.LoadScene(nombreEscenaJuego);
    }

    public void Salir()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}