using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class ARMeasurementTool : MonoBehaviour
{
    [SerializeField] private GameObject pointPrefab;
    [SerializeField] private GameObject distanceTextPrefab;

    private ARRaycastManager arRaycastManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private List<GameObject> points = new List<GameObject>();
    private List<GameObject> distanceTexts = new List<GameObject>();
    private LineRenderer lineRenderer;

    public static ARMeasurementTool Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        arRaycastManager = GetComponent<ARRaycastManager>();

        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.enabled = false;
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.material = Resources.Load<Material>("AlwaysOnTopMaterial");
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        lineRenderer.sortingOrder = 10;
    }

    void Update()
    {
        Vector2 inputPosition = Vector2.zero;
        bool inputDetected = false;

#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0))
        {
            inputPosition = Input.mousePosition;
            inputDetected = true;
        }
#else
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            inputPosition = Input.GetTouch(0).position;
            inputDetected = true;

            if (IsTouchOverUI(inputPosition))
                return;
        }
#endif

        if (inputDetected)
        {
            if (arRaycastManager.Raycast(inputPosition, hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = hits[0].pose;
                Vector3 adjustedPosition = hitPose.position;
                adjustedPosition.y += 0.002f;

                GameObject newPoint = Instantiate(pointPrefab, adjustedPosition, Quaternion.identity);
                points.Add(newPoint);

                if (points.Count > 1)
                {
                    lineRenderer.enabled = true;
                    DrawLineAndMeasure();
                }
            }
        }
    }

    void DrawLineAndMeasure()
    {
        lineRenderer.positionCount = points.Count;

        for (int i = 0; i < points.Count; i++)
        {
            Vector3 adjustedPosition = points[i].transform.position;
            adjustedPosition.y += 0.002f;
            lineRenderer.SetPosition(i, adjustedPosition);
        }

        if (points.Count >= 2)
        {
            Vector3 pointA = points[points.Count - 2].transform.position;
            Vector3 pointB = points[points.Count - 1].transform.position;

            float segmentDistance = Vector3.Distance(pointA, pointB);
            float segmentDistanceInInches = segmentDistance * 39.3701f;

            Vector3 midpoint = new Vector3(
                (pointA.x + pointB.x) / 2,
                (pointA.y + pointB.y) / 2,
                (pointA.z + pointB.z) / 2
            );
            midpoint.y += 0.1f;

            GameObject newDistanceText = Instantiate(distanceTextPrefab, midpoint, Quaternion.identity);
            newDistanceText.GetComponent<TextMeshPro>().text = $"{segmentDistanceInInches:F2} in";
            distanceTexts.Add(newDistanceText);
        }
    }

    public void ClearAllMeasurements()
    {
        foreach (var point in points)
            Destroy(point);

        foreach (var text in distanceTexts)
            Destroy(text);

        points.Clear();
        distanceTexts.Clear();

        lineRenderer.positionCount = 0;
        lineRenderer.enabled = false;
    }

    private bool IsTouchOverUI(Vector2 position)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = position
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }
}
