using UnityEngine;

public class controllerPrimaPersona : MonoBehaviour
{
    [Header("Movimento_Personaggio")]
    public float velocitaCamminata = 5.0f;
    public float moltiplicatoreCorsa = 1,5f;

    [Header("Camera")]
    public Transform cameraTransform;
    public float sensibilità = 3f;

    private bool staCorrendo;
    private float xRotazione = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        Sprint();
        Move(); 
        
        Look();
    }
}
