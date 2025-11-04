using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SphereSelectLogic : MonoBehaviour
{
    [Tooltip("The Interactor component responsible for selecting objects (e.g., XR Direct Interactor).")]
    [SerializeField]
    private XRBaseInteractor interactor;

    [Tooltip("The Transform representing the default/fixed center of the sphere, typically the controller's position.")]
    [SerializeField]
    private Transform defaultCenterTransform; 

    [Tooltip("The default radius of the selection sphere.")]
    [SerializeField]
    public float defaultRadius = 1f;

    [Tooltip("The minimum radius the sphere can shrink to.")]
    [SerializeField]
    public float minRadius = 0.05f;

    [Tooltip("The maximum radius the sphere can expand to.")]
    [SerializeField]
    public float maxRadius = 2.0f;
    
    [Tooltip("The Sphere Collider component used for interaction detection.")]
    [SerializeField]
    private SphereCollider sphereCollider;


    [Header("Runtime Data (Read Only)")]
    [Tooltip("The current, clamped radius of the sphere. Updated by SphereSelectInput.")]
    [SerializeField]
    private float currentRadius; 
    public float CurrentRadius => currentRadius; 

    [Tooltip("The calculated local position offset for the sphere center.")]
    [SerializeField]
    private Vector3 calculatedCenterOffset; 
    public Vector3 CalculatedCenterOffset => calculatedCenterOffset;

    private void Awake()
    {
        currentRadius = defaultRadius; 
    }
    
    public void SetTargetRadius(float newRadius) 
    {
        currentRadius = Mathf.Clamp(newRadius, minRadius, maxRadius);
        UpdateCenterOffset();
    }
    
    private void UpdateCenterOffset()
    {
        if (currentRadius < defaultRadius)
        {
            float shiftDistance = defaultRadius - currentRadius;
            calculatedCenterOffset = Vector3.back * shiftDistance; 
        }
        else
        {
            // Default and large radius behavior: The center stays at the controller origin.
            calculatedCenterOffset = Vector3.zero; 
        }
    }
    private void LateUpdate()
    {
        if (sphereCollider != null)
        {
            sphereCollider.radius = currentRadius;
            sphereCollider.center = calculatedCenterOffset;
        }
    }
    
    public bool IsHovering => interactor != null && interactor.isHovering;
    public bool IsSelected => interactor != null && interactor.isSelectActive;
}