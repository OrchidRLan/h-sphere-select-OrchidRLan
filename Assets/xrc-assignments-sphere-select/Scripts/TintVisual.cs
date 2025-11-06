using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class TintVisual : MonoBehaviour, IMultiSelectHighlight
{
    [SerializeField] 
    private Color normalColor = Color.grey;
    
    [SerializeField] 
    private Color highlightColor = Color.yellow;
    
    private Renderer objectRenderer;
    private XRGrabInteractable interactable;
    private Material materialInstance; 
    private bool isMultiSelected = false;

    private void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
        interactable = GetComponent<XRGrabInteractable>();
        
        materialInstance = new Material(objectRenderer.material);
        objectRenderer.material = materialInstance;
        
        if (interactable != null)
        {
            interactable.hoverEntered.AddListener(OnHoverEntered);
            interactable.hoverExited.AddListener(OnHoverExited);
            interactable.selectEntered.AddListener(OnSelectEntered);
            interactable.selectExited.AddListener(OnSelectExited);
        }
        
        SetColor(normalColor);
    }

    private void OnDestroy()
    {
        if (interactable != null)
        {
            interactable.hoverEntered.RemoveListener(OnHoverEntered);
            interactable.hoverExited.RemoveListener(OnHoverExited);
            interactable.selectEntered.RemoveListener(OnSelectEntered);
            interactable.selectExited.RemoveListener(OnSelectExited);
        }
        
        if (materialInstance != null)
        {
            Destroy(materialInstance);
        }
    }
    
    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        if (!isMultiSelected)
            SetColor(highlightColor);
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        if (!isMultiSelected && !interactable.isSelected)
            SetColor(normalColor);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (!isMultiSelected)
            SetColor(highlightColor);
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        if (!isMultiSelected)
            SetColor(normalColor);
    }

    public void OnMultiSelectEnter()
    {
        isMultiSelected = true;
        SetColor(highlightColor);
    }

    public void OnMultiSelectExit()
    {
        isMultiSelected = false;
        SetColor(normalColor);
    }

    private void SetColor(Color color)
    {
        if (materialInstance != null)
        {
            materialInstance.color = color;
        }
    }
}