using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class TapToPlaceOrSelect : MonoBehaviour
{
    [Header("AR Setup")]
    [SerializeField] private Camera arCamera;
    [SerializeField] private GameObject doorwayZonePrefab;
    [SerializeField] private GameObject savePromptPanel;
    [SerializeField] private UnityEngine.UI.Image doorModeButtonImage;
    [SerializeField] private Color furnitureModeColor = Color.black;
    [SerializeField] private Color doorwayModeColor = new Color(0.8f, 0.2f, 0.2f);


    private ARRaycastManager raycastManager;
    private List<ARRaycastHit> arHits = new List<ARRaycastHit>();
    private GameObject selectedObject;

    private float speedModifier = 0.0005f;
    private float rotationSpeed = 0.2f;
    private bool isDragging = false;
    private bool isColliding = false;
    private Color originalColor;
    private Material originalMaterial;
    private Renderer selectedRenderer;

    private bool hasUnsavedChanges = false;
    private bool hasSaved = false;

    private enum PlacementMode { Furniture, Doorway }
    private PlacementMode currentMode = PlacementMode.Furniture;

    void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
    }

    void Update()
    {
        if (Input.touchCount == 0) return;
        Touch touch = Input.GetTouch(0);

        if (IsTouchOverUI(touch.position))
        {
            isDragging = false;
            return;
        }

        Vector2 touchPosition = touch.position;

        if (touch.phase == TouchPhase.Moved && selectedObject != null)
        {
            isDragging = true;
            Vector3 forward = new Vector3(arCamera.transform.forward.x, 0f, arCamera.transform.forward.z).normalized;
            Vector3 right = new Vector3(arCamera.transform.right.x, 0f, arCamera.transform.right.z).normalized;
            selectedObject.transform.Translate(forward * touch.deltaPosition.y * speedModifier, Space.World);
            selectedObject.transform.Translate(right * touch.deltaPosition.x * speedModifier, Space.World);
            hasUnsavedChanges = true;
            hasSaved = false;
        }

        if (touch.phase == TouchPhase.Ended)
        {
            if (isDragging)
            {
                isDragging = false;
                return;
            }

            if (isColliding) return;

            Ray ray = arCamera.ScreenPointToRay(touchPosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("Object") && currentMode == PlacementMode.Furniture)
                {
                    SelectObject(hit.collider.gameObject);
                    return;
                }
                else if (hit.collider.CompareTag("DoorwayZone") && currentMode == PlacementMode.Doorway)
                {
                    SelectObject(hit.collider.gameObject);
                    return;
                }
            }

            if (raycastManager.Raycast(touchPosition, arHits, TrackableType.PlaneWithinPolygon))
            {
                Pose pose = arHits[0].pose;

                if (currentMode == PlacementMode.Doorway)
                {
                    GameObject zone = Instantiate(doorwayZonePrefab, pose.position, pose.rotation);
                    zone.tag = "DoorwayZone";
                    SelectObject(zone);
                    hasUnsavedChanges = true;
                    hasSaved = false;
                    return;
                }

                GameObject prefabToPlace = ARModelCatalogUI.SelectedModel;
                if (prefabToPlace == null) return;

                GameObject obj = Instantiate(prefabToPlace, pose.position, pose.rotation);
                obj.tag = "Object";
                SelectObject(obj);
                hasUnsavedChanges = true;
                hasSaved = false;
            }
        }

        if (Input.touchCount == 2 && selectedObject != null)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);
            if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
            {
                Vector2 delta = touch0.deltaPosition - touch1.deltaPosition;
                float rotationAmount = delta.x * rotationSpeed;
                selectedObject.transform.Rotate(0, rotationAmount, 0, Space.World);
                hasUnsavedChanges = true;
                hasSaved = false;
            }
        }
    }

    bool IsTouchOverUI(Vector2 screenPosition)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current) { position = screenPosition };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }

    private void SelectObject(GameObject obj)
    {
        DeselectObject();
        selectedObject = obj;

        selectedRenderer = selectedObject.GetComponent<Renderer>();
        if (selectedRenderer != null)
        {
            originalMaterial = selectedRenderer.material;
            originalColor = selectedRenderer.material.color;
            selectedRenderer.material.color = Color.yellow;
        }

        Collider col = selectedObject.GetComponent<Collider>();
        if (col == null) col = selectedObject.AddComponent<BoxCollider>();
        col.isTrigger = true;

        if (selectedObject.GetComponent<CollisionHighlighter>() == null)
            selectedObject.AddComponent<CollisionHighlighter>().Init(this);
    }

    private void DeselectObject()
    {
        if (selectedObject == null) return;

        if (selectedRenderer != null)
        {
            selectedRenderer.material.color = originalColor;
            selectedRenderer = null;
        }

        selectedObject = null;
        isColliding = false;
    }

    private void UpdateModeButtonColor()
    {
        if (doorModeButtonImage == null) return;

        if (currentMode == PlacementMode.Doorway)
            doorModeButtonImage.color = doorwayModeColor;
        else
            doorModeButtonImage.color = furnitureModeColor;
    }

    public void SetCollisionState(bool colliding)
    {
        isColliding = colliding;
        if (selectedRenderer != null)
            selectedRenderer.material.color = colliding ? Color.red : Color.yellow;
    }

    [System.Serializable]
    public class ObjectData
    {
        public string prefabName;
        public Vector3 position;
        public Quaternion rotation;
    }

    [System.Serializable]
    public class SaveData
    {
        public List<ObjectData> objects = new List<ObjectData>();
    }

    public void SaveLayout()
    {
        SaveData data = new SaveData();
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Object"))
        {
            ObjectData od = new ObjectData
            {
                prefabName = obj.name.Replace("(Clone)", "").Trim(),
                position = obj.transform.position,
                rotation = obj.transform.rotation
            };
            data.objects.Add(od);
        }
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("ARLayout", json);
        PlayerPrefs.Save();
        hasUnsavedChanges = false;
        hasSaved = true;
    }

    public void LoadLayout()
    {
        if (!PlayerPrefs.HasKey("ARLayout")) return;
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Object"))
        {
            Destroy(obj);
        }
        string json = PlayerPrefs.GetString("ARLayout");
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        foreach (var od in data.objects)
        {
            GameObject prefab = ARModelCatalogUI.GetPrefabByName(od.prefabName);
            if (prefab != null)
            {
                GameObject obj = Instantiate(prefab, od.position, od.rotation);
                obj.tag = "Object";
            }
        }
        hasUnsavedChanges = false;
        hasSaved = true;
    }

    public void DeleteSelectedObject()
    {
        if (selectedObject != null)
        {
            Destroy(selectedObject);
            DeselectObject();
            hasUnsavedChanges = true;
            hasSaved = false;
        }
    }

    public void TryExitARScene()
    {
        if (hasUnsavedChanges && !hasSaved)
        {
            if (savePromptPanel != null)
                savePromptPanel.SetActive(true);
        }
        else ExitToHome();
    }

    public void OnSaveAndExit()
    {
        SaveLayout();
        ExitToHome();
    }

    public void OnExitWithoutSaving()
    {
        ExitToHome();
    }

    public void OnCancelExit()
    {
        if (savePromptPanel != null)
            savePromptPanel.SetActive(false);
    }

    private void ExitToHome()
    {
        SceneManager.LoadScene("Homepage");
    }

    public void TogglePlacementMode()
    {
        currentMode = currentMode == PlacementMode.Furniture ? PlacementMode.Doorway : PlacementMode.Furniture;
        DeselectObject();
        UpdateModeButtonColor();
    }
}

public class CollisionHighlighter : MonoBehaviour
{
    private TapToPlaceOrSelect controller;

    public void Init(TapToPlaceOrSelect ctrl)
    {
        controller = ctrl;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Object") || other.CompareTag("DoorwayZone"))
            controller.SetCollisionState(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Object") || other.CompareTag("DoorwayZone"))
            controller.SetCollisionState(false);
    }
}
