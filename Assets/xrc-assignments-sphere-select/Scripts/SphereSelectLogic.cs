using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SphereSelectLogic : MonoBehaviour
{
    [SerializeField, Tooltip("Interactor component (e.g., XRDirectInteractor)")]
    private MonoBehaviour interactorComponent;

    private IXRHoverInteractor hoverInteractor;
    private IXRSelectInteractor selectInteractor;

    [Header("Sphere Settings")]
    [SerializeField] private Transform defaultCenterTransform;
    [SerializeField] private SphereCollider sphereCollider;

    [Header("Radius Settings")]
    public float defaultRadius = 1f;
    public float minRadius = 0.05f;
    public float maxRadius = 2f;

    private float currentRadius;
    private Vector3 calculatedCenterOffset;

    public float CurrentRadius => currentRadius;
    public Vector3 CalculatedCenterOffset => calculatedCenterOffset;

    private void Awake()
    {
        currentRadius = defaultRadius;

        if (interactorComponent != null)
        {
            hoverInteractor = interactorComponent as IXRHoverInteractor;
            selectInteractor = interactorComponent as IXRSelectInteractor;

            if (hoverInteractor == null && selectInteractor == null)
                Debug.LogWarning("Interactor component does not implement XR interfaces (IXRHoverInteractor / IXRSelectInteractor).");
        }
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
    public bool IsHovering =>
        hoverInteractor != null &&
        hoverInteractor.interactablesHovered != null &&
        hoverInteractor.interactablesHovered.Count > 0;

    public bool IsSelected =>
        selectInteractor != null &&
        selectInteractor.interactablesSelected != null &&
        selectInteractor.interactablesSelected.Count > 0;
}
