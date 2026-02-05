using UnityEngine;
using UnityEngine.InputSystem;

// Assicura che l'oggetto abbia un CharacterController: requisito per il movimento
[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    // --- Movement ---
    [Header("Movement")]
    // Velocità di movimento orizzontale base (m/s)
    [SerializeField] float moveSpeed = 5f;
    // Moltiplicatore della velocità quando ci si accovaccia
    [SerializeField] float crouchSpeedMultiplier = 0.5f;

    // --- View (visuale/camera) ---
    [Header("View")]
    // Trasform che contiene la camera (rotazione pitch applicata qui)
    [SerializeField] Transform cameraRoot;
    // Sensibilità del mouse (scala applicata all'input)
    [SerializeField] float mouseSensitivity = 25f;
    // Limite massimo di rotazione verso l'alto (gradi)
    [SerializeField] float maxLookUp = 80f;
    // Limite massimo di rotazione verso il basso (gradi, negativo)
    [SerializeField] float maxLookDown = -80f;

    // --- Jump & Gravity ---
    [Header("Jump & Gravity")]
    // Altezza del salto (metrica usata per calcolare la velocità iniziale y)
    [SerializeField] float jumpHeight = 1.5f;
    // Valore della gravità applicata (dev'essere negativo)
    [SerializeField] float gravity = -9.81f;

    // --- Crouch ---
    [Header("Crouch")]
    // Altezza del CharacterController quando si è accovacciati
    [SerializeField] float crouchHeight = 1.0f;

    // Riferimento al CharacterController presente sull'oggetto
    CharacterController controller;
    // Classe generata dall'Input System (mappa input del giocatore)
    PlayerInputActions inputActions;

    // Stato degli input raccolti dai callback
    Vector2 moveInput;     // direzione WASD / stick (x = destra/sinistra, y = avanti/indietro)
    Vector2 lookInput;     // input per la camera (mouse / stick)
    float verticalVelocity; // velocità verticale usata per salto e gravità
    float xRotation;       // rotazione verticale accumulata della camera (pitch)

    // Valori originali del controller per ripristinare dopo il crouch
    float originalHeight;
    Vector3 originalCenter;
    bool isCrouching;      // flag che indica se il player è in stato di crouch

    void Awake()
    {
        // Ottengo il CharacterController e salvo i valori iniziali
        controller = GetComponent<CharacterController>();
        originalHeight = controller.height;
        originalCenter = controller.center;

        // Istanzio la classe generata dall'Input System
        inputActions = new PlayerInputActions();

        // --- MOVE ---
        // Quando l'azione Move viene eseguita leggo il vettore 2D e lo salvo
        inputActions.Player.Move.performed += ctx => // ctx = CallbackContext
        {
            moveInput = ctx.ReadValue<Vector2>();
        };
        // Quando l'azione viene annullata (tasto rilasciato) azzero l'input di movimento
        inputActions.Player.Move.canceled += ctx =>
        {
            moveInput = Vector2.zero;
        };

        // --- LOOK ---
        // Ricevo input per la rotazione della camera (mouse o stick)
        inputActions.Player.Look.performed += ctx =>
        {
            lookInput = ctx.ReadValue<Vector2>();
        };
        // Azzeramento quando l'input di look viene rilasciato
        inputActions.Player.Look.canceled += ctx =>
        {
            lookInput = Vector2.zero;
        };

        // --- JUMP ---
        // Al trigger dell'azione Jump provo a saltare
        inputActions.Player.Jump.performed += ctx =>
        {
            TryJump();
        };

        // --- CROUCH (tenere premuto) ---
        // Quando si preme Crouch inizio l'accovacciamento
        inputActions.Player.Crouch.performed += ctx =>
        {
            StartCrouch();
        };
        // Quando si rilascia Crouch torno in piedi
        inputActions.Player.Crouch.canceled += ctx =>
        {
            StopCrouch();
        };
    }

    void OnEnable()
    {
        // Abilita le azioni input (necessario per ricevere eventi)
        inputActions.Enable();
    }

    void OnDisable()
    {
        // Disabilita le azioni quando l'oggetto non è attivo per evitare callback non volute
        inputActions.Disable();
    }

    void Update()
    {
        // Update principale: processa movimento e rotazione ogni frame
        Movement();
        Look();
    }

    void Look()
    {
        // Applico la sensibilità e Time.deltaTime per rendere la risposta frame-independent
        Vector2 look = lookInput * mouseSensitivity * Time.deltaTime;

        // Rotazione orizzontale del player (yaw) usando l'asse Y del mondo locale
        transform.Rotate(Vector3.up * look.x);

        // Rotazione verticale accumulata (pitch): sottraggo look.y perché input positivo alza lo sguardo
        xRotation -= look.y;
        // Clampo la rotazione verticale ai limiti definiti per evitare rotazioni eccessive
        xRotation = Mathf.Clamp(xRotation, maxLookDown, maxLookUp);

        if (cameraRoot != null)
        {
            // Applico la rotazione pitch solo alla camera (evita di inclinare il capsule collider)
            cameraRoot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
        else
        {
            // Messaggio di avvertimento se non è stata assegnata la cameraRoot nel Inspector
            Debug.LogWarning("FirstPersonController: cameraRoot non assegnato.");
        }
    }

    void Movement()
    {
        // Controllo se siamo a terra tramite il CharacterController
        bool isGrounded = controller.isGrounded;

        // Se siamo a terra resetto leggermente la velocità verticale per mantenerci stabili sul suolo
        // (evita che piccoli valori negativi continuino a sommare gravità)
        if (isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        // Calcolo la direzione di movimento locale in base all'orientamento del player
        // moveInput.x => destra/sinistra, moveInput.y => avanti/indietro
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        // Determino la velocità corrente: più lenta se accovacciati
        float currentSpeed = isCrouching ? moveSpeed * crouchSpeedMultiplier : moveSpeed;
        move *= currentSpeed;

        // Applico la gravità alla velocità verticale
        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        // Movimento effettivo tramite CharacterController (moltiplico per deltaTime)
        controller.Move(move * Time.deltaTime);
    }

    void TryJump()
    {
        // Il salto è permesso solo se siamo a terra e non siamo in stato di crouch
        if (controller.isGrounded && !isCrouching)
        {
            // Imposto la velocità verticale necessaria per raggiungere jumpHeight
            // formula: v = sqrt(2 * g * h), con g negativo => v = sqrt(h * -2 * g)
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
       }
    }

    void StartCrouch()
    {
        // Se siamo già accovacciati non facciamo nulla
        if (isCrouching) return;

        isCrouching = true;
        // Ridimensiono l'altezza del CharacterController per il crouch
        controller.height = crouchHeight;

        // Abbasso il centro del controller per evitare che il capsule "flutti" sopra il pavimento
        controller.center = new Vector3(
            originalCenter.x,
            crouchHeight / 2f,
            originalCenter.z
        );
    }

    void StopCrouch()
    {
        // Se non siamo accovacciati non facciamo nulla
        if (!isCrouching) return;

        isCrouching = false;
        // Ripristino altezza e centro originali del controller
        controller.height = originalHeight;
        controller.center = originalCenter;
    }
}