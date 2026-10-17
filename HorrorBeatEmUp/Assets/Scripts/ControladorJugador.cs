using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControladorJugador : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 5;

    [Range(0.1f, 1f)]
    public float multiplicadorDeProfundidad = 0.6f; // 0.6 suele sentirse perfecto
    private float multiplicadorDeVelocidadAlCorrer = 1f;
    private Vector2 direccionDeInput;
    private Rigidbody2D rbJugador;

    [Header("Salto Falso")]
    public Transform visualTransform;
    public float fuerzaDeSalto = 10f;
    public float gravedadFalsa = 25f;
    
    private bool estaSaltando = false;
    private float velocidadVerticalActual = 0f;
    private float posicionYVisualInicial;

    void Start()
    {
        rbJugador = GetComponent<Rigidbody2D>();
        
        
        posicionYVisualInicial = visualTransform.localPosition.y; 
    }

    void Update()
    {
        
        float movX = Input.GetAxisRaw("Horizontal");
        float movY = Input.GetAxisRaw("Vertical");

        Vector2 direccionBase = new Vector2(movX, movY).normalized;


        direccionDeInput = new Vector2(direccionBase.x, direccionBase.y * multiplicadorDeProfundidad);

        if (Input.GetKey(KeyCode.L)) {
            multiplicadorDeVelocidadAlCorrer = 2f; 
        } else {
            multiplicadorDeVelocidadAlCorrer = 1f;
        }

        
        ManejarSaltoFalso();
    }

    void FixedUpdate()
    {
        
        rbJugador.MovePosition(rbJugador.position + direccionDeInput * (velocidad * multiplicadorDeVelocidadAlCorrer) * Time.fixedDeltaTime);
    }

    void ManejarSaltoFalso()
    {
        
        if (Input.GetKeyDown(KeyCode.K) && !estaSaltando)
        {
            estaSaltando = true;
            velocidadVerticalActual = fuerzaDeSalto;
        }

        if (estaSaltando)
        {
            
            velocidadVerticalActual -= gravedadFalsa * Time.deltaTime;
            
            
            Vector3 nuevaPosicionLocal = visualTransform.localPosition;
            nuevaPosicionLocal.y += velocidadVerticalActual * Time.deltaTime;
            visualTransform.localPosition = nuevaPosicionLocal;

            
            if (visualTransform.localPosition.y <= posicionYVisualInicial)
            {
                
                visualTransform.localPosition = new Vector3(visualTransform.localPosition.x, posicionYVisualInicial, visualTransform.localPosition.z);
                estaSaltando = false;
                velocidadVerticalActual = 0f;
            }
        }
    }
}