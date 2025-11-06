using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SphereSelectFeedback : MonoBehaviour
{
    private SphereSelectLogic sphereSelectLogic;

    [SerializeField]
    [Tooltip("The material assigned to the sphere.")]
    private Material sphereMaterial;

    [SerializeField]
    [Tooltip("Color of the sphere when the interactor is not hovering over any object.")]
    private Color color = new Color(0.5f, 0.5f, 0.5f, 0.3f);

    [SerializeField]
    [Tooltip("Color of the sphere when the interactor is hovering over an object.")]
    private Color hoverColor = new Color(1.0f, 0.0f, 0.0f, 0.5f);

    private GameObject sphereVisual;
    private MeshRenderer sphereRenderer;
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor;
    private SphereCollider sphereCollider;
    private bool isHovering = false;
    private bool isSelecting = false;

    private void Awake()
    {
        if (sphereSelectLogic == null)
        {
            sphereSelectLogic = GetComponent<SphereSelectLogic>();
        }

        interactor = sphereSelectLogic.Interactor;
        sphereCollider = interactor.GetComponent<SphereCollider>();

        CreateSphereVisual();

        interactor.hoverEntered.AddListener(OnHoverEntered);
        interactor.hoverExited.AddListener(OnHoverExited);
        interactor.selectEntered.AddListener(OnSelectEntered);
        interactor.selectExited.AddListener(OnSelectExited);
    }

    private void OnDestroy()
    {
        if (interactor != null)
        {
            interactor.hoverEntered.RemoveListener(OnHoverEntered);
            interactor.hoverExited.RemoveListener(OnHoverExited);
            interactor.selectEntered.RemoveListener(OnSelectEntered);
            interactor.selectExited.RemoveListener(OnSelectExited);
        }
    }

    private void CreateSphereVisual()
    {
        sphereVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphereVisual.name = "SphereVisual";
        sphereVisual.transform.SetParent(interactor.transform);
        sphereVisual.transform.localRotation = Quaternion.identity;

        Collider col = sphereVisual.GetComponent<Collider>();
        if (col != null)
        {
            Destroy(col);
        }

        sphereRenderer = sphereVisual.GetComponent<MeshRenderer>();
        if (sphereMaterial != null)
        {
            sphereRenderer.material = new Material(sphereMaterial);
        }

        UpdateColor();
        UpdateSphereTransform();
    }

    private void Update()
    {
        UpdateSphereTransform();
        UpdateVisibility();
    }

    private void UpdateSphereTransform()
    {
        if (sphereVisual != null && sphereCollider != null)
        {
            float diameter = sphereSelectLogic.Radius * 2f;
            sphereVisual.transform.localScale = Vector3.one * diameter;
            sphereVisual.transform.localPosition = sphereCollider.center;
        }
    }

    private void UpdateVisibility()
    {
        if (sphereVisual != null)
        {
            sphereVisual.SetActive(!isSelecting);
        }
    }

    private void UpdateColor()
    {
        if (sphereRenderer != null && sphereRenderer.material != null)
        {
            Color targetColor = isHovering ? hoverColor : color;
            sphereRenderer.material.color = targetColor;
        }
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        isHovering = true;
        UpdateColor();
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        isHovering = interactor.hasHover;
        UpdateColor();
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        isSelecting = true;
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        isSelecting = interactor.hasSelection;
    }
}