using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;
using System.Collections.Generic;

[RequireComponent(typeof(ARRaycastManager),
    typeof(ARPlaneManager))]
public class PlaceObject : MonoBehaviour
{
    [SerializeField]
    private GameObject prefab;

    private ARRaycastManager aRRaycastManager;
    private ARPlaneManager  aRPlaneManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private void Awake() {
        aRRaycastManager = GetComponent<ARRaycastManager>();
        aRPlaneManager = GetComponent<ARPlaneManager>();
    }

    private void OnEnable() {
        EnhancedTouch.TouchSimulation.Enable();
        EnhancedTouch.EnhancedTouchSupport.Enable();
        EnhancedTouch.Touch.onFingerDown += FingerDown;
    }

    private void OnDisable() {
        EnhancedTouch.TouchSimulation.Disable();
        EnhancedTouch.EnhancedTouchSupport.Disable();
        EnhancedTouch.Touch.onFingerDown -= FingerDown;
    }

    private void FingerDown(EnhancedTouch.Finger finger) {
        if (finger.index != 0) return;

        if (aRRaycastManager.Raycast(finger.currentTouch.screenPosition, hits,
            TrackableType.PlaneWithinPolygon)) {
                foreach(ARRaycastHit hit in hits) {
                    Pose pose = hit.pose;
                    GameObject obj = Instantiate(prefab, pose.position, pose.rotation);

                    if (aRPlaneManager.GetPlane(hit.trackableId). alignment == PlaneAlignment.HorizontalUp) {
                        Vector3 position = obj.transform.position;
                        position.y = 0f;
                        Vector3 cameraPosition = Camera.main.transform.position;
                        cameraPosition.y = 0f;
                        Vector3 direction = cameraPosition - position;
                        Quaternion targetRotation = Quaternion.LookRotation
                            (direction);
                        obj.transform.rotation = targetRotation;
                    }
                }
        }
    }
}
