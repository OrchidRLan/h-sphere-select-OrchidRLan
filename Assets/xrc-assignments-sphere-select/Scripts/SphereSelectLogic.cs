using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public interface IMultiSelectHighlight
{
    void OnMultiSelectEnter();
    void OnMultiSelectExit();
}

public class SphereSelectLogic : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The interactor responsible for selecting an object.")]
    private XRBaseInteractor interactor;

    [SerializeField]
    [Tooltip("The minimum radius of the sphere.")]
    private float minRadius = 0.01f;

    [SerializeField]
    [Tooltip("The maximum radius of the sphere.")]
    private float maxRadius = 0.3f;

    [SerializeField]
    [Tooltip("The default radius value when starting the application.")]
    private float defaultRadius = 0.04f;

    [SerializeField]
    [Tooltip("Transform indicating the default position for the sphere center.")]
    private Transform defaultCenter;

    public float Radius { get; private set; }
    public XRBaseInteractor Interactor => interactor;

    private SphereCollider sphereCollider;
    private Transform attachTransform;
    private Vector3 defaultCenterLocalPosition;

    private List<Transform> selectedObjects = new List<Transform>();
    private List<Vector3> selectedLocalOffsets = new List<Vector3>();
    private List<Quaternion> selectedLocalRotations = new List<Quaternion>();
    
    private bool isSelecting = false;
    private GameObject groupParent;

    private void Awake()
    {
        if (interactor == null)
        {
            interactor = GetComponent<XRBaseInteractor>();
        }

        sphereCollider = interactor.GetComponent<SphereCollider>();
        if (sphereCollider == null)
        {
            sphereCollider = interactor.gameObject.AddComponent<SphereCollider>();
        }
        sphereCollider.isTrigger = true;

        if (interactor.attachTransform == null)
        {
            GameObject attachObj = new GameObject("AttachTransform");
            attachObj.transform.SetParent(interactor.transform);
            interactor.attachTransform = attachObj.transform;
        }
        attachTransform = interactor.attachTransform;

        defaultCenterLocalPosition = defaultCenter != null 
            ? interactor.transform.InverseTransformPoint(defaultCenter.position) 
            : Vector3.zero;

        interactor.selectEntered.AddListener(OnSelectEntered);
        interactor.selectExited.AddListener(OnSelectExited);

        SetRadius(defaultRadius);
    }

    private void OnDestroy()
    {
        if (interactor != null)
        {
            interactor.selectEntered.RemoveListener(OnSelectEntered);
            interactor.selectExited.RemoveListener(OnSelectExited);
        }

        if (groupParent != null)
        {
            Destroy(groupParent);
        }
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (!isSelecting)
        {
            isSelecting = true;
            CollectObjects();
        }
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        if (isSelecting)
        {
            ReleaseObjects();
            isSelecting = false;
        }
    }

    private void CollectObjects()
    {
        selectedObjects.Clear();
        selectedLocalOffsets.Clear();
        selectedLocalRotations.Clear();

        if (groupParent == null)
        {
            groupParent = new GameObject("MultiSelectGroup");
        }
        groupParent.transform.position = attachTransform.position;
        groupParent.transform.rotation = attachTransform.rotation;

        Collider[] colliders = Physics.OverlapSphere(
            interactor.transform.TransformPoint(sphereCollider.center),
            Radius
        );

        foreach (Collider col in colliders)
        {
            XRGrabInteractable interactable = col.GetComponent<XRGrabInteractable>();
            
            if (interactable != null)
            {
                Transform objTransform = col.transform;
                
                selectedObjects.Add(objTransform);
                selectedLocalOffsets.Add(groupParent.transform.InverseTransformPoint(objTransform.position));
                selectedLocalRotations.Add(Quaternion.Inverse(groupParent.transform.rotation) * objTransform.rotation);

                IMultiSelectHighlight highlight = objTransform.GetComponent<IMultiSelectHighlight>();
                if (highlight != null)
                {
                    highlight.OnMultiSelectEnter();
                }
            }
        }
    }

    private void ReleaseObjects()
    {
        foreach (Transform obj in selectedObjects)
        {
            if (obj != null)
            {
                IMultiSelectHighlight highlight = obj.GetComponent<IMultiSelectHighlight>();
                if (highlight != null)
                {
                    highlight.OnMultiSelectExit();
                }
            }
        }

        selectedObjects.Clear();
        selectedLocalOffsets.Clear();
        selectedLocalRotations.Clear();

        if (groupParent != null)
        {
            Destroy(groupParent);
            groupParent = null;
        }
    }

    private void Update()
    {
        if (isSelecting && selectedObjects.Count > 0 && groupParent != null)
        {
            groupParent.transform.position = attachTransform.position;
            groupParent.transform.rotation = attachTransform.rotation;

            for (int i = 0; i < selectedObjects.Count; i++)
            {
                if (selectedObjects[i] != null)
                {
                    selectedObjects[i].position = groupParent.transform.TransformPoint(selectedLocalOffsets[i]);
                    selectedObjects[i].rotation = groupParent.transform.rotation * selectedLocalRotations[i];
                }
            }
        }
    }

    public void SetRadius(float radiusValue)
    {
        Radius = Mathf.Clamp(radiusValue, minRadius, maxRadius);

        if (sphereCollider != null)
        {
            sphereCollider.radius = Radius;
        }

        UpdateCenterPosition();
    }

    private void UpdateCenterPosition()
    {
        Vector3 newCenter = defaultCenterLocalPosition;

        if (defaultCenter != null && Radius < defaultRadius)
        {
            float offset = defaultRadius - Radius;
            newCenter = new Vector3(
                defaultCenterLocalPosition.x - offset,
                defaultCenterLocalPosition.y,
                defaultCenterLocalPosition.z
            );
        }

        sphereCollider.center = newCenter;

        if (attachTransform != null)
        {
            attachTransform.localPosition = newCenter;
            attachTransform.localRotation = Quaternion.identity;
        }
    }

    public void ChangeRadius(float delta)
    {
        SetRadius(Radius + delta);
    }
}