using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimacionObstaculo : MonoBehaviour
{
    private Animator anim;
    private bool yaSeActivo = false;

    void Start()
    {
        // Buscamos el Animator que está en este mismo obstáculo
        anim = GetComponent<Animator>();
    }

    // Esta función de Unity se dispara sola en el frame exacto en el que 
    // el SpriteRenderer entra en la pantalla (la cámara lo ve)
    private void OnBecameVisible()
    {
        // Verificamos que no se haya activado antes, por si la cámara retrocede
        if (!yaSeActivo && anim != null)
        {
            anim.SetTrigger("Activar");
            yaSeActivo = true;
        }
    }
}