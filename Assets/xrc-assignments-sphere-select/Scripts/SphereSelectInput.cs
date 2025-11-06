using UnityEngine;
using UnityEngine.InputSystem;

public class SphereSelectInput : MonoBehaviour
{
    private SphereSelectLogic sphereSelectLogic;

    [SerializeField]
    [Tooltip("The scale factor applied to the user input when changing the radius value.")]
    private float inputSensitivity = 0.5f;

    [SerializeField]
    [Tooltip("")]
    private InputActionProperty changeRadius;

    private void Awake()
    {
        if (sphereSelectLogic == null)
        {
            sphereSelectLogic = GetComponent<SphereSelectLogic>();
        }
    }

    private void OnEnable()
    {
        changeRadius.action?.Enable();
    }

    private void OnDisable()
    {
        changeRadius.action?.Disable();
    }

    private void Update()
    {
        if (sphereSelectLogic == null)
            return;

        Vector2 inputValue = changeRadius.action?.ReadValue<Vector2>() ?? Vector2.zero;
        float yInput = inputValue.y;

        if (Mathf.Abs(yInput) > 0.1f)
        {
            float radiusChange = yInput * inputSensitivity * Time.deltaTime;
            sphereSelectLogic.ChangeRadius(radiusChange);
        }
    }
}