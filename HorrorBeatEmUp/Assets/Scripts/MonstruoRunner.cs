using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonstruoRunner : MonoBehaviour
{
    private bool persiguiendo = false;
    private float velocidadActual = 0f;

    // El GameManager llamará a esta función
    public void EmpezarAPerseguir(float velocidadDelJugador)
    {
        velocidadActual = velocidadDelJugador;
        persiguiendo = true;
    }

    void Update()
    {
        if (persiguiendo)
        {
            // Se mueve constantemente hacia la derecha a la misma velocidad que el jugador
            transform.Translate(Vector2.right * velocidadActual * Time.deltaTime);
        }
    }
}