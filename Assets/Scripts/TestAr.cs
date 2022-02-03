using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
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
using System.Threading.Tasks;

public class TestAr : MonoBehaviour
{
    public StorageReference storageReference;

    public GameObject selectedObject;

    [SerializeField]
    float speed;
    string className;
    string experimentName;
    List<string> processes;
    int currentSelectedIndex;

    [SerializeField]
    ARRaycastManager arRaycastManager;
    [SerializeField]
    ARPlaneManager arPlaneManager;
    List<ARRaycastHit> aRRaycastHits = new List<ARRaycastHit>();
    [SerializeField]
    Pose hitPose;
    [SerializeField]
    GameObject objToPlane;
    [SerializeField]
    Transform objectInstTrans;
    [SerializeField]
    GameObject selectedExperimentObj;
    [SerializeField]
    GameObject realworldView;

    [SerializeField]
    int i = -1;

    bool changingModel = false;

    private void Start()
    {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://xr-app-600b1.firebaseio.com/");
        storageReference = FirebaseStorage.DefaultInstance.RootReference;

        className = "Test";
        experimentName = "";
        //experimentName = "Make your own magnet";
        //experimentName = "Sound is produced by vibration";

        processes = new List<string>();
        //processes.Add("Rainbow Scene");
        //processes.Add("Glass");
        //processes.Add("Prism");
        //processes.Add("Reverse Prism");
        //processes.Add("Spin Top");
        //processes.Add("Train Scene");
        processes.Add("Map 2");

        for (int i = 0; i < processes.Count; i++)
        {
            GetExperimentFromFirebase(i,processes[i]);
        }
        
    }

    async void GetExperimentFromFirebase(string className, string experimentName)
    {

        Debug.Log("Downloading asset bundle : " + className + "-" + experimentName);
        string imagePath = Application.persistentDataPath + "/" + experimentName.ToLower().Replace(" ", "_");
        // Start downloading a file

        Task task = storageReference.Child("TeachingInAr").Child("models").Child(className).Child(experimentName.ToLower().Replace(" ", "_")).GetFileAsync(imagePath,
              new StorageProgress<DownloadState>((DownloadState state) => {
                  // called periodically during the download
                  Debug.Log(state.BytesTransferred * 100 / state.TotalByteCount);
              }), System.Threading.CancellationToken.None);

        await task.ContinueWith(resultTask => {
            if (!resultTask.IsFaulted && !resultTask.IsCanceled)
            {
                Debug.Log("Downloaded asset bundle : " + className + "-" + experimentName);
            }
            else
            {
                Debug.Log(resultTask.Exception);
            }

        });

        StartCoroutine(LoadFromMemoryAsyncN(Application.persistentDataPath + "/" + experimentName.ToLower().Replace(" ", "_")));
    }

    async void GetExperimentFromFirebase(int index , string processName)
    {
        Debug.Log("Downloading asset bundle : " + className + "-" + processName);
        string imagePath = Application.persistentDataPath + "/" + processName.ToLower().Replace(" ", "_");
        // Start downloading a file

        Task task = storageReference.Child("TeachingInAr").Child("models").Child(className).Child(processName.ToLower().Replace(" ", "_")).GetFileAsync(imagePath,
              new StorageProgress<DownloadState>((DownloadState state) => {
                  // called periodically during the download
                  //Debug.Log(state.BytesTransferred * 100 / state.TotalByteCount);
              }), System.Threading.CancellationToken.None);

        await task.ContinueWith(resultTask => {
            if (!resultTask.IsFaulted && !resultTask.IsCanceled)
            {
                Debug.Log("Downloaded asset bundle : " + className + "-" + experimentName);
            }
            else
            {
                Debug.Log(resultTask.Exception);
            }

        });

        if(index == 0)
        {
            currentSelectedIndex = 0;
            StartCoroutine(LoadFromMemoryAsyncN(Application.persistentDataPath + "/" + processName.ToLower().Replace(" ", "_")));
        }
    }

    public void changeProcess()
    {
        GameObject selectedProcess = EventSystem.current.currentSelectedGameObject;
        experimentName = selectedProcess.name;
        Debug.Log(selectedProcess.name);
        Destroy(selectedExperimentObj);
        arPlaneManager.enabled = true;
        realworldView.transform.GetChild(1).gameObject.SetActive(true);
        realworldView.transform.GetChild(1).GetChild(0).GetComponent<Text>().text = "Scan the surface";
        realworldView.transform.GetChild(2).GetChild(1).GetComponent<Text>().text = "";
        i = -1;
        StartCoroutine(LoadFromMemoryAsyncN(Application.persistentDataPath + "/" + selectedProcess.name.ToLower().Replace(" ", "_")));
    }

    IEnumerator LoadFromMemoryAsyncN(string path)
    {
        yield return new WaitForSeconds(2f);
        Destroy(selectedExperimentObj);
        changingModel = false;
        AssetBundle.UnloadAllAssetBundles(true);
        AssetBundleCreateRequest createRequest = AssetBundle.LoadFromMemoryAsync(File.ReadAllBytes(path));
        yield return createRequest;
        AssetBundle assetBundle = createRequest.assetBundle;
        objToPlane = assetBundle.LoadAsset<GameObject>(processes[currentSelectedIndex].ToLower().Replace(" ", "_"));
    }

    /*
    public void PlayTheNextAnimation()
    {
        if (selectedExperimentObj != null && animationName.Count != i + 1)
        {
            i += 1;

            selectedExperimentObj.GetComponent<Animator>().Play(animationName[i], 0);

            if (i == animationName.Count - 1 && selectedExperimentObj.GetComponent<AudioSource>() != null)
            {
                selectedExperimentObj.GetComponent<AudioSource>().Play();
            }
        }
    }

    public void PlayThePreviousAnimation()
    {
        if (selectedExperimentObj != null && i - 1 != -1)
        {
            i -= 1;

            selectedExperimentObj.GetComponent<Animator>().Play(animationName[i], 0);

        }
    }
    */
    // Update is called once per frame
    void Update()
    {
        if (objToPlane != null)
        {
            if (currentSelectedIndex > 0)
            {
                selectedExperimentObj = Instantiate(objToPlane, hitPose.position, hitPose.rotation);
                //selectedExperimentObj.transform.localScale = new Vector3(3, 3, 3);
                selectedExperimentObj.transform.rotation = objToPlane.transform.rotation;
                Debug.Log(objToPlane.transform.rotation);
                objToPlane = null;
            }
            else
            {
                //selectedExperimentObj = Instantiate(objToPlane);
                ////selectedExperimentObj.transform.localScale = new Vector3(3, 3, 3);
                ////selectedExperimentObj.transform.Rotate(new Vector3(0, 90, 0));
                //objToPlane = null;
                //return;
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
                            //realworldView.transform.GetChild(2).GetChild(1).GetComponent<Text>().text = experimentName;
                            selectedExperimentObj = Instantiate(objToPlane, hitPose.position, hitPose.rotation);
                            selectedExperimentObj.transform.rotation = objToPlane.transform.rotation;
                            Debug.Log(objToPlane.transform.rotation);
                            //selectedExperimentObj.transform.localScale = new Vector3(3, 3, 3);
                            //selectedExperimentObj.transform.Rotate(new Vector3(0, -90, 0));
                            //objectInstTrans.position = hitPose.position;
                            //objectInstTrans.rotation = hitPose.rotation;
                            objToPlane = null;
                            
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

        if (selectedExperimentObj != null && !changingModel)
        {
            for (int i = 0; i < FindObjectsOfType<ARPlane>().Count(); i++)
            {
                Destroy(GameObject.FindGameObjectsWithTag("ARPlane")[i]);
            }
            
            if (selectedExperimentObj.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("idle"))
            {   
                if(currentSelectedIndex + 1 < processes.Count)
                {
                    Debug.Log(currentSelectedIndex);
                    currentSelectedIndex += 1;
                    changingModel = true;
                    StartCoroutine(LoadFromMemoryAsyncN(Application.persistentDataPath + "/" + processes[currentSelectedIndex].ToLower().Replace(" ", "_")));
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
                selectedExperimentObj.transform.localScale = new Vector3(Mathf.Clamp(selectedExperimentObj.transform.localScale.x, 0.00001f, 100),
                                                                         Mathf.Clamp(selectedExperimentObj.transform.localScale.y, 0.00001f, 100),
                                                                         Mathf.Clamp(selectedExperimentObj.transform.localScale.z, 0.00001f, 100));

            }
        }
    }
}
