using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioSource audioFondo;
    public AudioSource audioSFX;
    public float volumenCancion;
    public float volumenSFX;
    

    void Awake()
    {
        if(Instance != null)
        {
            Destroy(this.gameObject); // Destruye el objeto para no dejar basura
        } 
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    void Update()
    {
        audioFondo.volume = volumenCancion;
        audioSFX.volume = volumenSFX;
    }



    public void ReproducirMusica(AudioClip cancionAReproducir)
    {
        if(cancionAReproducir == null)
        {
            print ("No se encontró la música");
            return;
        }
        
        if(audioFondo.isPlaying && audioFondo.clip == cancionAReproducir)
        {
            return;
        }

        audioFondo.clip = cancionAReproducir;
        audioFondo.volume = volumenCancion;
        audioFondo.loop = true;
        audioFondo.Play();
    }
    
    public void DetenerMusica()
    {
        audioFondo.loop = false;
        audioFondo.Stop();
    }

    public void ReproducirSonido(AudioClip sonidoAReproducir, float unPitch)
    {
        if(sonidoAReproducir == null)
        {
            print("Sonido no encontrado");
            return;
        }

        audioSFX.PlayOneShot(sonidoAReproducir);
        audioSFX.volume = volumenSFX;
        audioSFX.pitch = unPitch;
    }

    public void DetenerSonido()
    {
        audioSFX.Stop();
    }
    public void AsignarVolumen(float volumenDeMusica, float volumenDeSFX)
    {
        volumenCancion = volumenDeMusica;
        volumenSFX = volumenDeSFX;
    }
}
