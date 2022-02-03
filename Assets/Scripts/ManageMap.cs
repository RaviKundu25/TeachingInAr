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

public class ManageMap : MonoBehaviour
{
    DatabaseReference reference;
    [SerializeField]
    Dictionary<string, Dictionary<string, long>> data = new Dictionary<string, Dictionary<string, long>>();
    Dictionary<string,int> dataSubCategory = new Dictionary<string, int>() { { "Confirmed" , 500000 }, { "Active" , 5000 },
                                                                             { "Recovered" , 500000 }, { "Deceased" , 5000 },
                                                                             { "Tested" , 10000000 }, {"Vaccine Doses Administered" , 1000000 } };
    [SerializeField]
    GameObject UIComp;
    public  GameObject map;
    [SerializeField]
    GameObject poll;
    [SerializeField]
    GameObject pollInst;
    [SerializeField]
    int standardDivider = 500000;
    [SerializeField]
    string projectionToSee = "Confirmed";
    [SerializeField]
    ARRaycastManager arRaycastManager;
    [SerializeField]
    ARPlaneManager arPlaneManager;
    List<ARRaycastHit> aRRaycastHits = new List<ARRaycastHit>();
    [SerializeField]
    GameObject selectedExperimentObj;
    [SerializeField]
    GameObject selectedStateObj;
    [SerializeField]
    GameObject realworldView;
    [SerializeField]
    float speed;
    Pose hitPose;

    private void Start()
    {
        // Set up the Editor before calling into the realtime database.
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://xr-app-600b1.firebaseio.com/");
        // Get the root reference location of the database.
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        getData();
    }

    // Update is called once per frame
    void Update()
    {
        if (selectedExperimentObj == null)
        {
            //selectedExperimentObj = Instantiate(map);
            //realworldView.SetActive(false);
            //UIComp.transform.gameObject.SetActive(true);
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
                            selectedExperimentObj = Instantiate(map, hitPose.position, hitPose.rotation);
                            UIComp.transform.gameObject.SetActive(true);
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

        if (!UIComp.transform.GetChild(1).GetComponent<Toggle>().isOn)
        {
            if (Input.touchCount > 0)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit))
                    {
                        selectedStateObj = hit.collider.gameObject.transform.parent.gameObject;
                        if (selectedStateObj != null)
                        {
                            if (pollInst != null)
                            {
                                for (int i = 0; i < pollInst.transform.parent.childCount; i++)
                                {
                                    if(pollInst.transform.parent.GetChild(i).name.Contains("nameTag"))
                                    {
                                        float yScale = pollInst.transform.parent.GetChild(pollInst.transform.parent.childCount - 1).gameObject.transform.localScale.y;
                                        Vector3 nameTagPos = new Vector3(pollInst.transform.parent.GetChild(i).transform.localPosition.x,
                                                                         pollInst.transform.parent.GetChild(i).transform.localPosition.y - yScale - 0.002f,
                                                                         pollInst.transform.parent.GetChild(i).transform.localPosition.z);
                                        pollInst.transform.parent.GetChild(i).transform.localPosition = nameTagPos;
                                        pollInst.transform.parent.GetChild(i).transform.localEulerAngles = new Vector3(0, 0, 0);
                                    }
                                }
                                Destroy(pollInst);
                            }
                            selectedState(selectedStateObj.name);
                        }
                    }
                }
            }
        }

        UIComp.transform.GetChild(2).GetComponent<Text>().text = "1 UNIT : " + standardDivider;
    }

    public async void getData()
    {
        await reference.Child("Map").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("Error occured.");
                return;
            }
            else if (task.IsCompleted)
            {
                DataSnapshot states = task.Result;

                foreach (DataSnapshot state in states.Children)
                {
                    data.Add(state.Key.ToString().ToLower().Replace(" ","_"), new Dictionary<string, long>());
                    Dictionary<string, long> ObjectData = new Dictionary<string, long>();
                    ObjectData["Confirmed"] = convertData(state.Child("Confirmed").Value.ToString());
                    ObjectData["Active"] = convertData(state.Child("Active").Value.ToString());
                    ObjectData["Recovered"] = convertData(state.Child("Recovered").Value.ToString());
                    ObjectData["Deceased"] = convertData(state.Child("Deceased").Value.ToString());
                    ObjectData["Tested"] = convertData(state.Child("Tested").Value.ToString());
                    ObjectData["Vaccine Doses Administered"] = convertData(state.Child("Vaccine Doses Administered").Value.ToString());
                    data[state.Key.ToString().ToLower().Replace(" ", "_")] = ObjectData;
                }
            }
        });

        //foreach (string state in data.Keys.ToList())
        //{
        //    selectedState(state);
        //}
    }

    long convertData(string dataString)
    {
        long dataValue = 0;
        dataString = dataString.Replace(",", "").Replace(".", "");
        if (dataString.Contains("K"))
        {
            dataString = dataString.Replace("K", "000");
        }
        if (dataString.Contains("L"))
        {
            dataString = dataString.Replace("L", "00000");
        }
        if (dataString.Contains("Cr"))
        {
            dataString = dataString.Replace("Cr", "0000000");
        }
        if(dataString == "-")
        {
            dataString = "0";
        }
        dataValue = long.Parse(dataString);
        return dataValue;
    }

    void selectedState(string selectedStateName)
    {
        for (int i = 0; i < selectedExperimentObj.transform.GetChild(0).GetChild(0).childCount; i++)
        {
            if(selectedExperimentObj.transform.GetChild(0).GetChild(0).GetChild(i).name == selectedStateName)
            {
                pollInst = Instantiate(poll);
                pollInst.transform.SetParent(selectedExperimentObj.transform.GetChild(0).GetChild(0).GetChild(i));
                float yScale = (data[selectedStateName][projectionToSee] /(float)(standardDivider*100));
                pollInst.transform.localScale = new Vector3(poll.transform.localScale.x, yScale, poll.transform.localScale.z);
                float yPos = (float)yScale / 2f + poll.transform.position.y;//+ hitPose.position.y;
                pollInst.transform.localPosition = new Vector3(selectedExperimentObj.transform.GetChild(0).GetChild(0).GetChild(i).GetChild(2).localPosition.x,
                                                      yPos,
                                                      selectedExperimentObj.transform.GetChild(0).GetChild(0).GetChild(i).GetChild(2).localPosition.z);
                for (int j = 0; j < selectedExperimentObj.transform.GetChild(0).GetChild(0).GetChild(i).childCount; j++)
                {
                    if (selectedExperimentObj.transform.GetChild(0).GetChild(0).GetChild(i).GetChild(j).name.Contains("nameTag"))
                    {
                        Vector3 nameTagPos = new Vector3(selectedExperimentObj.transform.GetChild(0).GetChild(0).GetChild(i).GetChild(j).transform.localPosition.x,
                                                         selectedExperimentObj.transform.GetChild(0).GetChild(0).GetChild(i).GetChild(j).transform.localPosition.y + yScale + 0.002f,
                                                         selectedExperimentObj.transform.GetChild(0).GetChild(0).GetChild(i).GetChild(j).transform.localPosition.z);
                        selectedExperimentObj.transform.GetChild(0).GetChild(0).GetChild(i).GetChild(j).transform.localPosition = nameTagPos;
                        if (selectedStateName == "delhi")
                        {
                            selectedExperimentObj.transform.GetChild(0).GetChild(0).GetChild(i).GetChild(j).transform.localEulerAngles = new Vector3(270, 0, 0);
                        }
                        else
                        {
                            selectedExperimentObj.transform.GetChild(0).GetChild(0).GetChild(i).GetChild(j).transform.localEulerAngles = new Vector3(90, 0, 0);
                        }
                    }
                }
            }
        }
    }

    public void selectedCategory(int index)
    {
        projectionToSee = UIComp.transform.GetChild(0).GetComponent<Dropdown>().options[index].text;
        standardDivider = dataSubCategory[projectionToSee];

        if (UIComp.transform.GetChild(1).GetComponent<Toggle>().isOn)
        {
            cleanMap();
            foreach (string state in data.Keys.ToList())
            {
                selectedState(state);
            }
        }
        else
        {
            if (pollInst != null)
            {
                for (int i = 0; i < pollInst.transform.parent.childCount; i++)
                {
                    if (pollInst.transform.parent.GetChild(i).name.Contains("nameTag"))
                    {
                        float yScale = pollInst.transform.parent.GetChild(pollInst.transform.parent.childCount - 1).gameObject.transform.localScale.y;
                        Vector3 nameTagPos = new Vector3(pollInst.transform.parent.GetChild(i).transform.localPosition.x,
                                                         pollInst.transform.parent.GetChild(i).transform.localPosition.y - yScale - 0.002f,
                                                         pollInst.transform.parent.GetChild(i).transform.localPosition.z);
                        pollInst.transform.parent.GetChild(i).transform.localPosition = nameTagPos;
                        pollInst.transform.parent.GetChild(i).transform.localEulerAngles = new Vector3(0, 0, 0);
                    }
                }
                Destroy(pollInst);
            }

            selectedState(selectedStateObj.name);
        }
    }

    public void toggleChanged(bool allStateSelected)
    {
        cleanMap();
        if (UIComp.transform.GetChild(1).GetComponent<Toggle>().isOn)
        {
            foreach (string state in data.Keys.ToList())
            {
                selectedState(state);
            }
        }
        else
        {
            for (int i = 0; i < selectedExperimentObj.transform.GetChild(0).GetChild(0).childCount; i++)
            {
                if (selectedExperimentObj.transform.GetChild(0).GetChild(0).GetChild(i).name == "haryana")
                {
                    selectedStateObj = selectedExperimentObj.transform.GetChild(0).GetChild(0).GetChild(i).gameObject;
                }
            }
            selectedState("haryana");
        }
    }

    void cleanMap()
    {
        for (int i = 0; i < selectedExperimentObj.transform.GetChild(0).GetChild(0).childCount; i++)
        {
            if (selectedExperimentObj.transform.GetChild(0).GetChild(0).GetChild(i).GetChild(selectedExperimentObj.transform.GetChild(0).GetChild(0).GetChild(i).childCount - 1).name.Split("("[0])[0] == "Cube")
            {
                for (int j = 0; j < selectedExperimentObj.transform.GetChild(0).GetChild(0).GetChild(i).childCount; j++)
                {
                    if (selectedExperimentObj.transform.GetChild(0).GetChild(0).GetChild(i).GetChild(j).name.Contains("nameTag"))
                    {
                        float yScale = selectedExperimentObj.transform.GetChild(0).GetChild(0).GetChild(i).GetChild(selectedExperimentObj.transform.GetChild(0).GetChild(0).GetChild(i).childCount - 1).gameObject.transform.localScale.y;
                        Vector3 nameTagPos = new Vector3(selectedExperimentObj.transform.GetChild(0).GetChild(0).GetChild(i).GetChild(j).transform.localPosition.x,
                                                             selectedExperimentObj.transform.GetChild(0).GetChild(0).GetChild(i).GetChild(j).transform.localPosition.y - yScale - 0.002f,
                                                             selectedExperimentObj.transform.GetChild(0).GetChild(0).GetChild(i).GetChild(j).transform.localPosition.z);
                        selectedExperimentObj.transform.GetChild(0).GetChild(0).GetChild(i).GetChild(j).transform.localPosition = nameTagPos;
                        selectedExperimentObj.transform.GetChild(0).GetChild(0).GetChild(i).GetChild(j).transform.localEulerAngles = new Vector3(0, 0, 0);
                    }
                }
                Destroy(selectedExperimentObj.transform.GetChild(0).GetChild(0).GetChild(i).GetChild(selectedExperimentObj.transform.GetChild(0).GetChild(0).GetChild(i).childCount - 1).gameObject);
            }
        }
    }
}
