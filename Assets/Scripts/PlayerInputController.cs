using UnityEngine;

[RequireComponent(typeof(EntityMovement))] // Zwingt Unity dazu, das Movement-Skript am gleichen Objekt zu haben
public class PlayerInputController : MonoBehaviour
{
    private EntityMovement moveScript;

    void Awake()
    {
        // Wir suchen uns den Motor, der an diesem Objekt hängt
        moveScript = GetComponent<EntityMovement>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // 1. Tastatur-Eingaben an den Motor senden
        moveScript.inputX = Input.GetAxisRaw("Horizontal");
        moveScript.inputY = Input.GetAxisRaw("Vertical");
        moveScript.inputJumping = Input.GetButton("Jump");
        moveScript.inputCrouching = Input.GetKey(KeyCode.LeftControl);
        
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            moveScript.StartDance();
        }
        

        // 2. Maus-Eingaben an den Motor senden
        moveScript.inputMouseX = Input.GetAxis("Mouse X");
        moveScript.inputMouseY = Input.GetAxis("Mouse Y");

        // 3. Wenn die Ducken-Taste genau JETZT gedrückt oder losgelassen wird, 
        // rufen wir die Methoden im Motor auf.
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            moveScript.StartCrouch();
        }
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            moveScript.StopCrouch();
        }
        
        // Dance
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            moveScript.StartDance();
        }
        bool movementInput =
            Input.GetAxisRaw("Horizontal") != 0 ||
            Input.GetAxisRaw("Vertical") != 0 ||
            Input.GetButtonDown("Jump") ||
            Input.GetKeyDown(KeyCode.LeftControl) ||
            Input.GetKey(KeyCode.LeftShift);

        if (movementInput)
        {
            moveScript.StopDance();
        }
        


    }
}