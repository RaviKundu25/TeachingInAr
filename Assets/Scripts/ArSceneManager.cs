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
using System.Runtime.Serialization.Formatters.Binary;

public class ArSceneManager : MonoBehaviour
{
    [SerializeField]
    float speed;
    string className;
    string subjectName;
    string experimentName;
    [SerializeField]
    int currentSelectedProcessIndex;
    [SerializeField]
    GameObject processToPlace;
    [SerializeField]
    GameObject selectedExperimentObj;
    [SerializeField]
    GameObject realworldView;

    // Read Data offline
    BinaryFormatter bf;
    FileStream file;
    Dictionary<string, SchoolClass> schoolClasses;

    // AR System
    [SerializeField]
    ARRaycastManager arRaycastManager;
    [SerializeField]
    ARPlaneManager arPlaneManager;
    List<ARRaycastHit> aRRaycastHits = new List<ARRaycastHit>();
    [SerializeField]
    Pose hitPose;

    private void Start()
    {
        className = PlayerPrefs.GetString("className");
        subjectName = PlayerPrefs.GetString("subjectName");
        experimentName = PlayerPrefs.GetString("experimentName");

        //Read offline data
        bf = new BinaryFormatter();
        file = File.Open(Application.persistentDataPath + "/data.dat", FileMode.Open);
        schoolClasses = bf.Deserialize(file) as Dictionary<string, SchoolClass>;
        file.Close();
        realworldView.transform.GetChild(2).GetChild(1).GetComponent<Text>().text = experimentName;

        currentSelectedProcessIndex = 0;

        foreach (string processName in schoolClasses[className].subjects[subjectName].experiments[experimentName].processes.Keys.ToList())
        {
            if(currentSelectedProcessIndex == schoolClasses[className].subjects[subjectName].experiments[experimentName].processes[processName].index)
            {
                Debug.Log("Process : " + processName + " loading...");
                string processPath = Application.persistentDataPath + "/" + className + "/" + subjectName + "/" + experimentName + "/" +
                             processName.ToLower().Replace(" ", "_");
                StartCoroutine(LoadFromMemoryAsync(processPath, processName));
                break;
            }
        }

        if(currentSelectedProcessIndex == 0)
        {
            realworldView.transform.GetChild(0).GetChild(1).GetComponent<Button>().interactable = false;
        }
        else
        {
            realworldView.transform.GetChild(0).GetChild(1).GetComponent<Button>().interactable = true;
        }

        if(currentSelectedProcessIndex + 1 == schoolClasses[className].subjects[subjectName].experiments[experimentName].processes.Keys.ToList().Count)
        {
            realworldView.transform.GetChild(0).GetChild(3).GetComponent<Button>().interactable = false;
        }
        else
        {
            realworldView.transform.GetChild(0).GetChild(3).GetComponent<Button>().interactable = true;
        }
    }

    IEnumerator LoadFromMemoryAsync(string path,string assetName)
    {
        yield return new WaitForSeconds(2f);
        Destroy(selectedExperimentObj);
        AssetBundle.UnloadAllAssetBundles(true);
        AssetBundleCreateRequest createRequest = AssetBundle.LoadFromMemoryAsync(File.ReadAllBytes(path));
        yield return createRequest;
        AssetBundle assetBundle = createRequest.assetBundle;
        processToPlace = assetBundle.LoadAsset<GameObject>(assetName.ToLower().Replace(" ", "_"));
        realworldView.transform.GetChild(0).GetChild(2).GetComponent<Text>().text = "Step " + (currentSelectedProcessIndex+1) + "/" +
                                                                                    schoolClasses[className].subjects[subjectName].experiments[experimentName].processes.Keys.ToList().Count;
    }

    public void prevProcess()
    {
        currentSelectedProcessIndex -= 1;
        
        foreach (string processName in schoolClasses[className].subjects[subjectName].experiments[experimentName].processes.Keys.ToList())
        {
            if (currentSelectedProcessIndex == schoolClasses[className].subjects[subjectName].experiments[experimentName].processes[processName].index)
            {
                Debug.Log("Process : " + processName + " loading...");
                string processPath = Application.persistentDataPath + "/" + className + "/" + subjectName + "/" + experimentName + "/" +
                             processName.ToLower().Replace(" ", "_");
                StartCoroutine(LoadFromMemoryAsync(processPath, processName));
                break;
            }
        }

        if (currentSelectedProcessIndex == 0)
        {
            realworldView.transform.GetChild(0).GetChild(1).GetComponent<Button>().interactable = false;
        }
        else
        {
            realworldView.transform.GetChild(0).GetChild(1).GetComponent<Button>().interactable = true;
        }

        if (currentSelectedProcessIndex + 1 == schoolClasses[className].subjects[subjectName].experiments[experimentName].processes.Keys.ToList().Count)
        {
            realworldView.transform.GetChild(0).GetChild(3).GetComponent<Button>().interactable = false;
        }
        else
        {
            realworldView.transform.GetChild(0).GetChild(3).GetComponent<Button>().interactable = true;
        }
    }

    public void nextProcess()
    {
        currentSelectedProcessIndex += 1;
        
        foreach (string processName in schoolClasses[className].subjects[subjectName].experiments[experimentName].processes.Keys.ToList())
        {
            if (currentSelectedProcessIndex == schoolClasses[className].subjects[subjectName].experiments[experimentName].processes[processName].index)
            {
                Debug.Log("Process : " + processName + " loading...");
                string processPath = Application.persistentDataPath + "/" + className + "/" + subjectName + "/" + experimentName + "/" +
                             processName.ToLower().Replace(" ", "_");
                StartCoroutine(LoadFromMemoryAsync(processPath, processName));
                break;
            }
        }

        if (currentSelectedProcessIndex == 0)
        {
            realworldView.transform.GetChild(0).GetChild(1).GetComponent<Button>().interactable = false;
        }
        else
        {
            realworldView.transform.GetChild(0).GetChild(1).GetComponent<Button>().interactable = true;
        }

        if (currentSelectedProcessIndex + 1 == schoolClasses[className].subjects[subjectName].experiments[experimentName].processes.Keys.ToList().Count)
        {
            realworldView.transform.GetChild(0).GetChild(3).GetComponent<Button>().interactable = false;
        }
        else
        {
            realworldView.transform.GetChild(0).GetChild(3).GetComponent<Button>().interactable = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (processToPlace != null)
        {
            if(hitPose != null)
            {
                selectedExperimentObj = Instantiate(processToPlace, hitPose.position, hitPose.rotation);
                processToPlace = null;
            }
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
                            selectedExperimentObj = Instantiate(processToPlace, hitPose.position, hitPose.rotation);
                            processToPlace = null;
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

            if (selectedExperimentObj.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("idle") && currentSelectedProcessIndex + 1 < schoolClasses[className].subjects[subjectName].experiments[experimentName].processes.Keys.ToList().Count)
            {
                currentSelectedProcessIndex += 1;

                foreach (string processName in schoolClasses[className].subjects[subjectName].experiments[experimentName].processes.Keys.ToList())
                {
                    if (currentSelectedProcessIndex == schoolClasses[className].subjects[subjectName].experiments[experimentName].processes[processName].index)
                    {
                        Debug.Log("Process : " + processName + " loading...");
                        string processPath = Application.persistentDataPath + "/" + className + "/" + subjectName + "/" + experimentName + "/" +
                                     processName.ToLower().Replace(" ", "_");
                        StartCoroutine(LoadFromMemoryAsync(processPath, processName));
                        break;
                    }
                }

                if (currentSelectedProcessIndex == 0)
                {
                    realworldView.transform.GetChild(0).GetChild(1).GetComponent<Button>().interactable = false;
                }
                else
                {
                    realworldView.transform.GetChild(0).GetChild(1).GetComponent<Button>().interactable = true;
                }

                if (currentSelectedProcessIndex + 1 == schoolClasses[className].subjects[subjectName].experiments[experimentName].processes.Keys.ToList().Count)
                {
                    realworldView.transform.GetChild(0).GetChild(3).GetComponent<Button>().interactable = false;
                }
                else
                {
                    realworldView.transform.GetChild(0).GetChild(3).GetComponent<Button>().interactable = true;
                }
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
                selectedExperimentObj.transform.localScale = new Vector3(Mathf.Clamp(selectedExperimentObj.transform.localScale.x, 1f, 100),
                                                                   Mathf.Clamp(selectedExperimentObj.transform.localScale.y, 1f, 100),
                                                                   Mathf.Clamp(selectedExperimentObj.transform.localScale.z, 1f, 100));

            }
        }
    }

    public void back()
    {
        AssetBundle.UnloadAllAssetBundles(true);
        Application.LoadLevel(0);
    }

    public void shareScreen()
    {
        StartCoroutine(generateLinkAndShare());
    }

    IEnumerator generateLinkAndShare()
    {
        yield return new WaitForSeconds(1.5f);
        string url = "https://ravikundu.github.io/xealisticAR/ArLearning.html?" + "channel=" + transform.GetComponent<ShareScreen>().channelName;
        new NativeShare().SetText(url).Share();
    }
}
