using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Net;
using System.Linq;
using UnityEngine.Networking;
using Firebase;
using Firebase.Database;
using Firebase.Storage;
using Firebase.Unity.Editor;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.Events;

public class ManageEngine : MonoBehaviour
{
    //DatabaseReference reference;
    [SerializeField]
    public GameObject engine;
    [SerializeField]
    ARRaycastManager arRaycastManager;
    [SerializeField]
    ARPlaneManager arPlaneManager;
    List<ARRaycastHit> aRRaycastHits = new List<ARRaycastHit>();
    [SerializeField]
    GameObject selectedExperimentObj;
    [SerializeField]
    GameObject selectedPart;
    [SerializeField]
    GameObject prevSelectedPart;
    [SerializeField]
    string currentPartOut = "";
    [SerializeField]
    GameObject realworldView;
    [SerializeField]
    float speed;
    Pose hitPose;

    private void Start()
    {
        //// Set up the Editor before calling into the realtime database.
        //FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://xr-app-600b1.firebaseio.com/");
        //// Get the root reference location of the database.
        //reference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    // Update is called once per frame
    void Update()
    {
        if (selectedExperimentObj == null)
        {
            //selectedExperimentObj = Instantiate(engine);
            //getData();
            if (FindObjectsOfType<ARPlane>().Count() > 2)
            {
                realworldView.transform.GetChild(1).GetChild(0).GetComponent<Text>().text = "Tap to place the object";

                if (Input.touchCount > 0)
                {
                    Touch touch = Input.GetTouch(0);

                    if (touch.phase == TouchPhase.Began)
                    {
                        if (arRaycastManager.Raycast(touch.position, aRRaycastHits, TrackableType.PlaneWithinPolygon))
                        {
                            hitPose = aRRaycastHits[0].pose;
                            //realworldView.transform.GetChild(2).GetChild(1).GetComponent<Text>().text = experimentName;
                            selectedExperimentObj = Instantiate(engine, hitPose.position, hitPose.rotation);

                            arPlaneManager.enabled = false;
                            realworldView.transform.GetChild(1).gameObject.SetActive(false);

                            for (int i = 0; i < FindObjectsOfType<ARPlane>().Count(); i++)
                            {
                                Destroy(GameObject.FindGameObjectsWithTag("ARPlane")[i]);
                            }
                        }
                    }
                }
            }
        }

        if(selectedExperimentObj != null)
        {
            if (Input.touchCount > 0)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit))
                    {
                        Debug.Log(hit.collider.gameObject.name);
                        prevSelectedPart = selectedPart;
                        if (prevSelectedPart != null)
                        {
                            selectedExperimentObj.GetComponent<Animator>().Play(prevSelectedPart.name + "_IN");
                        }
                        for (int i = 0; i < selectedExperimentObj.transform.GetChild(0).GetChild(0).childCount; i++)
                        {
                            if (selectedExperimentObj.transform.GetChild(0).GetChild(0).GetChild(i).name == hit.collider.gameObject.name)
                            {
                                selectedPart = hit.collider.gameObject;
                            }
                        }
                    }
                }

            }

            if (prevSelectedPart != null)
            {
                if(currentPartOut == prevSelectedPart.name)
                {
                    if (selectedExperimentObj.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("idle"))
                    {
                        selectedExperimentObj.GetComponent<Animator>().Play(selectedPart.name + "_OUT");
                        currentPartOut = selectedPart.name;
                    }
                }
            }
            if (prevSelectedPart == null && currentPartOut == "" && selectedPart != null)
            {
                selectedExperimentObj.GetComponent<Animator>().Play(selectedPart.name + "_OUT");
                currentPartOut = selectedPart.name;
            }
        }

        manageArObjectScale();
    }

    void manageArObjectScale()
    {
        if (selectedExperimentObj != null)
        {
            for (int i = 0; i < FindObjectsOfType<ARPlane>().Count(); i++)
            {
                Destroy(GameObject.FindGameObjectsWithTag("ARPlane")[i]);
            }

            if (Input.touchCount == 2)
            {
                // Store both touches.
                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);

                // Find the position in the previous frame of each touch.
                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                // Find the magnitude of the vector (the distance) between the touches in each frame.
                float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

                // Find the difference in the distances between each frame.
                float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
                speed = selectedExperimentObj.transform.localScale.x / 1000;
                deltaMagnitudeDiff = deltaMagnitudeDiff * speed;
                selectedExperimentObj.transform.localScale -= new Vector3(deltaMagnitudeDiff, deltaMagnitudeDiff, deltaMagnitudeDiff);
                selectedExperimentObj.transform.localScale = new Vector3(Mathf.Clamp(selectedExperimentObj.transform.localScale.x, 0.00001f, 100),
                                                                         Mathf.Clamp(selectedExperimentObj.transform.localScale.y, 0.00001f, 100),
                                                                         Mathf.Clamp(selectedExperimentObj.transform.localScale.z, 0.00001f, 100));

            }
        }

    }
}
