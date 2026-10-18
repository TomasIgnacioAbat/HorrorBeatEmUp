using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetaRunner : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D otro)
    {
        if (otro.gameObject.CompareTag("PiesJugador"))
        {
            GameManager.Instance.NivelCompletado();
        }
    }
}