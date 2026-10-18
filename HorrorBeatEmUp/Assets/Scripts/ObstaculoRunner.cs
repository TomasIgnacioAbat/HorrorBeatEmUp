using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstaculoRunner : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D otro)
    {
        // Si el objeto que entra tiene la etiqueta del jugador
        if (otro.gameObject.CompareTag("PiesJugador")) 
        {
            GameManager.Instance.JugadorChocaObstaculo();
        }
    }
}