using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemigo : MonoBehaviour
{
    public int vida = 3;

    // Esta función es la que el jugador va a llamar cuando acierte un golpe
    public void RecibirDano(int daño)
    {
        vida -= daño;
        Debug.Log("¡Auch! Vida restante: " + vida);

        // Feedback visual rápido para la Game Jam (cambia de color un instante)
        // anim.SetTrigger("Hit"); 

        if (vida <= 0)
        {
            Debug.Log("Enemigo derrotado");
            Destroy(gameObject); // O pasarlo al estado de muerto
        }
    }
}
