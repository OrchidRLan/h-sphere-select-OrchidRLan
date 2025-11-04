using UnityEngine;
using UnityEngine.InputSystem;

public class SphereSelectInput : MonoBehaviour
{
    [Tooltip("The core logic component responsible for clamping the radius and calculating the center offset.")]
    [SerializeField]
    private SphereSelectLogic logic;

    [Tooltip("The 2D Axis input action reference for the right controller's thumbstick.")]
    [SerializeField]
    private InputActionReference thumbstickInput;


    [Tooltip("The scale factor applied to the user input when changing the radius value. Determines the speed of resizing.")]
    [SerializeField]
    private float inputSensitivity = 1f; 

    [SerializeField]
    private float currentRadius; 
    public float CurrentRadius => currentRadius;
    private void Update()
    {
        if (thumbstickInput == null || logic == null || !thumbstickInput.action.enabled) 
        {
            return;
        }
        
        Vector2 stickValue = thumbstickInput.action.ReadValue<Vector2>();
        float yInput = stickValue.y;
        float deltaRadius = yInput * inputSensitivity * Time.deltaTime;
        float newTargetRadius = logic.CurrentRadius + deltaRadius;
        logic.SetTargetRadius(newTargetRadius);
    }
    
    private void OnEnable()
    {
        if (thumbstickInput != null && thumbstickInput.action != null)
        {
            thumbstickInput.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (thumbstickInput != null && thumbstickInput.action != null)
        {
            thumbstickInput.action.Disable();
        }
    }
}