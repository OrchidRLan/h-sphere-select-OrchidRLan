using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// 定义接口，用于直接控制高亮显示（绕过 XR 事件系统）
public interface IMultiSelectHighlight
{
    void OnMultiSelectEnter();
    void OnMultiSelectExit();
}

public class SphereSelectLogic : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The interactor responsible for selecting an object. If the interactor reference is not assigned in the inspector, the component will search for an interactor on the same game object.")]
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

    public XRBaseInteractor Interactor => interactor;
    public float Radius { get; private set; }

    private SphereCollider sphereCollider;
    private Transform attachTransform;
    private Vector3 defaultCenterLocalPosition;

    // 多选物体管理
    private List<Transform> selectedObjects = new List<Transform>();
    private List<Vector3> selectedLocalOffsets = new List<Vector3>();
    private List<Quaternion> selectedLocalRotations = new List<Quaternion>();
    private List<Rigidbody> selectedRigidbodies = new List<Rigidbody>();
    private List<bool> originalKinematicStates = new List<bool>();
    private List<XRGrabInteractable> selectedInteractables = new List<XRGrabInteractable>();
    
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

        if (defaultCenter != null)
        {
            defaultCenterLocalPosition = interactor.transform.InverseTransformPoint(defaultCenter.position);
        }
        else
        {
            defaultCenterLocalPosition = Vector3.zero;
        }

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
        // XR Toolkit 只会选中一个物体，但我们要收集范围内的所有物体
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
        selectedRigidbodies.Clear();
        originalKinematicStates.Clear();
        selectedInteractables.Clear();

        // 创建虚拟父物体
        if (groupParent == null)
        {
            groupParent = new GameObject("MultiSelectGroup");
        }
        groupParent.transform.position = attachTransform.position;
        groupParent.transform.rotation = attachTransform.rotation;

        // 使用物理检测收集所有物体
        Collider[] colliders = Physics.OverlapSphere(
            interactor.transform.TransformPoint(sphereCollider.center),
            Radius
        );

        int collectedCount = 0;

        foreach (Collider col in colliders)
        {
            XRGrabInteractable interactable = col.GetComponent<XRGrabInteractable>();
            
            // 只处理有 XRGrabInteractable 的物体
            if (interactable != null)
            {
                Transform objTransform = col.transform;
                
                selectedObjects.Add(objTransform);
                selectedInteractables.Add(interactable);
                
                // 保存相对位置和旋转
                selectedLocalOffsets.Add(groupParent.transform.InverseTransformPoint(objTransform.position));
                selectedLocalRotations.Add(Quaternion.Inverse(groupParent.transform.rotation) * objTransform.rotation);
                
                // 禁用物理
                Rigidbody rb = col.GetComponent<Rigidbody>();
                selectedRigidbodies.Add(rb);
                if (rb != null)
                {
                    originalKinematicStates.Add(rb.isKinematic);
                    rb.isKinematic = true;
                }
                else
                {
                    originalKinematicStates.Add(false);
                }

                // 关键：手动通知物体进入多选状态
                NotifyMultiSelectEnter(objTransform);
                
                collectedCount++;
            }
        }

        Debug.Log($"Multi-selected {collectedCount} objects");
    }

    private void ReleaseObjects()
    {
        // 通知所有物体退出多选状态
        for (int i = 0; i < selectedObjects.Count; i++)
        {
            if (selectedObjects[i] != null)
            {
                NotifyMultiSelectExit(selectedObjects[i]);
            }
            
            if (selectedRigidbodies[i] != null)
            {
                selectedRigidbodies[i].isKinematic = originalKinematicStates[i];
            }
        }

        selectedObjects.Clear();
        selectedLocalOffsets.Clear();
        selectedLocalRotations.Clear();
        selectedRigidbodies.Clear();
        originalKinematicStates.Clear();
        selectedInteractables.Clear();

        if (groupParent != null)
        {
            Destroy(groupParent);
            groupParent = null;
        }
    }

    private void NotifyMultiSelectEnter(Transform obj)
    {
        // 方法1：通过自定义接口
        IMultiSelectHighlight highlight = obj.GetComponent<IMultiSelectHighlight>();
        if (highlight != null)
        {
            highlight.OnMultiSelectEnter();
        }
        
        // 方法2：通过 SendMessage（不推荐，但作为备用）
        obj.SendMessage("OnMultiSelectEnter", SendMessageOptions.DontRequireReceiver);
    }

    private void NotifyMultiSelectExit(Transform obj)
    {
        IMultiSelectHighlight highlight = obj.GetComponent<IMultiSelectHighlight>();
        if (highlight != null)
        {
            highlight.OnMultiSelectExit();
        }
        
        obj.SendMessage("OnMultiSelectExit", SendMessageOptions.DontRequireReceiver);
    }

    private void Update()
    {
        if (isSelecting && selectedObjects.Count > 0 && groupParent != null)
        {
            // 更新虚拟父物体
            groupParent.transform.position = attachTransform.position;
            groupParent.transform.rotation = attachTransform.rotation;

            // 更新所有物体的位置和旋转
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
        if (sphereCollider == null)
        {
            Debug.LogWarning("Sphere collider is null!");
            return;
        }

        if (float.IsNaN(Radius) || Radius <= 0f)
        {
            Debug.LogWarning("Invalid Radius value: " + Radius);
            Radius = defaultRadius;
        }

        Vector3 newCenter = defaultCenterLocalPosition;

        if (defaultCenter != null)
        {
            if (Radius < defaultRadius)
            {
                float offset = defaultRadius - Radius;
                newCenter = new Vector3(
                    defaultCenterLocalPosition.x - offset,
                    defaultCenterLocalPosition.y,
                    defaultCenterLocalPosition.z
                );
            }
            else
            {
                newCenter = defaultCenterLocalPosition;
            }
        }
        else
        {
            newCenter = Vector3.zero;
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