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

public class TestArScene : MonoBehaviour
{
    public StorageReference storageReference;

    public GameObject selectedObject;

    [SerializeField]
    float speed;
    string className;
    string experimentName;

    [SerializeField]
    ARRaycastManager arRaycastManager;
    [SerializeField]
    ARPlaneManager arPlaneManager;
    List<ARRaycastHit> aRRaycastHits = new List<ARRaycastHit>();
    [SerializeField]
    GameObject objToPlane;
    [SerializeField]
    GameObject selectedExperimentObj;
    [SerializeField]
    GameObject realworldView;

    private void Start()
    {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://xr-app-600b1.firebaseio.com/");
        storageReference = FirebaseStorage.DefaultInstance.RootReference;

        className = "Test";
        //experimentName = "Make your own magnet";
        //experimentName = "Sound is produced by vibration";
        experimentName = "Glass";

        GetExperimentFromFirebase(className, experimentName);
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

    IEnumerator LoadFromMemoryAsyncN(string path)
    {
        AssetBundle.UnloadAllAssetBundles(true);
        AssetBundleCreateRequest createRequest = AssetBundle.LoadFromMemoryAsync(File.ReadAllBytes(path));
        yield return createRequest;
        AssetBundle assetBundle = createRequest.assetBundle;
        objToPlane = assetBundle.LoadAsset<GameObject>(experimentName.ToLower().Replace(" ", "_"));
        Debug.Log(experimentName);
    }

    // Update is called once per frame
    void Update()
    {
        if (objToPlane != null)
        {
            //selectedExperimentObj = Instantiate(objToPlane);
            //selectedExperimentObj.transform.localScale = new Vector3(0.0001f, 0.0001f, 0.0001f);
            //selectedExperimentObj.transform.Rotate(new Vector3(0, 90, 0));
            //objToPlane = null;
            //return;
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
                            Pose hitPose = aRRaycastHits[0].pose;
                            selectedExperimentObj = Instantiate(objToPlane, hitPose.position, hitPose.rotation);
                            selectedExperimentObj.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
                            //selectedExperimentObj.transform.Rotate(new Vector3(0, 90, 0));

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
                deltaMagnitudeDiff = deltaMagnitudeDiff * speed;
                selectedExperimentObj.transform.localScale -= new Vector3(deltaMagnitudeDiff, deltaMagnitudeDiff, deltaMagnitudeDiff);
                selectedExperimentObj.transform.localScale = new Vector3(Mathf.Clamp(selectedExperimentObj.transform.localScale.x, 0.00001f, 100),
                                                                   Mathf.Clamp(selectedExperimentObj.transform.localScale.y, 0.00001f, 100),
                                                                   Mathf.Clamp(selectedExperimentObj.transform.localScale.z, 0.00001f, 100));

            }
        }
    }
}
