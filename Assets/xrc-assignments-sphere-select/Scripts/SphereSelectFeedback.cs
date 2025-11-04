using UnityEngine;

public class SphereSelectFeedback : MonoBehaviour
{
    [Tooltip("The core logic component, to read radius, offset, and state (Hover/Select).")]
    [SerializeField]
    private SphereSelectLogic logic;

    [Tooltip("The Mesh Renderer for the visual representation of the sphere.")]
    [SerializeField]
    private MeshRenderer sphereMeshRenderer;


    
    [Header("Visual Properties")]
    [Tooltip("The material assigned to the sphere.")]
    [SerializeField]
    private Material sphereMaterial;

    [Tooltip("The fixed color of the selection sphere.")]
    [SerializeField]
    private Color sphereFixedColor = new Color(1.0f, 0.0f, 0.0f, 0.4f); 

    private void Start()
    {
        if (sphereMeshRenderer == null)
        {
            Debug.LogError("SphereMeshRenderer reference is missing on " + gameObject.name + ". Feedback disabled.");
            enabled = false;
            return;
        }
        
        sphereMeshRenderer.material = sphereMaterial;
        if (sphereMaterial != null)
        {
             sphereMaterial.color = sphereFixedColor;
        }
    }

    private void Update()
    {
        if (logic == null || sphereMeshRenderer == null || sphereMaterial == null) return;
        float currentRadius = logic.CurrentRadius;
        Vector3 centerOffset = logic.CalculatedCenterOffset;
        bool isSelected = logic.IsSelected;
        
        sphereMeshRenderer.enabled = !isSelected;
        
        if (sphereMeshRenderer.enabled)
        {
            float diameter = currentRadius * 2f;
            sphereMeshRenderer.transform.localScale = Vector3.one * diameter;
            sphereMeshRenderer.transform.localPosition = centerOffset;
            sphereMaterial.color = sphereFixedColor;
        }
    }
}