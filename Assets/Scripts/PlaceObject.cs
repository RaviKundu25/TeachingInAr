using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaceObject : MonoBehaviour
{
    [SerializeField]
    ARRaycastManager arRaycastManager;

    List<ARRaycastHit> aRRaycastHits = new List<ARRaycastHit>();

    [SerializeField]
    GameObject objToPlane;

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if(touch.phase == TouchPhase.Began)
            {
                if (arRaycastManager.Raycast(touch.position, aRRaycastHits, TrackableType.PlaneWithinPolygon))
                {
                    Pose hitPose = aRRaycastHits[0].pose;

                    Instantiate(objToPlane, hitPose.position, hitPose.rotation);
                }
            }
        }
    }
}
