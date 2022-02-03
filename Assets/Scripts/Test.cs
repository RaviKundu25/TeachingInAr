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

public class Test : MonoBehaviour
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

    [SerializeField]
    List<string> animationName = new List<string>(){ "object_appear", "needle_growing" , "cap_growing" , "magnet" , "rubbing_of_needle" , "rotation_of_needle" , "mark_the_location" , "again_rotation_oneedle" };
    [SerializeField]
    GameObject next;
    [SerializeField]
    int i = -1;

    private void Start()
    {
        className = "Class 6";
        experimentName = "Make your own magnet";
    }

    public void shareScreenLink()
    {
        if (transform.GetComponent<ShareScreen>() == null)
        {
            transform.gameObject.AddComponent<ShareScreen>();
        }
        StartCoroutine(generateLinkAndShare());
    }

    IEnumerator generateLinkAndShare()
    {
        yield return new WaitForSeconds(1.5f);
        string url = "https://ravikundu.github.io/xealisticAR/ArLearning.html?" + "channel=" + transform.GetComponent<ShareScreen>().channelName;
        new NativeShare().SetText(url).Share();
    }

    IEnumerator LoadFromMemoryAsyncN(string path)
    {
        AssetBundleCreateRequest createRequest = AssetBundle.LoadFromMemoryAsync(File.ReadAllBytes(path));
        yield return createRequest;
        AssetBundle assetBundle = createRequest.assetBundle;
        objToPlane = assetBundle.LoadAsset<GameObject>(experimentName.ToLower().Replace(" ", "_"));
    }

    public void PlayTheNextAnimation()
    {
        if(selectedExperimentObj != null)
        {
            i += 1;

            selectedExperimentObj.GetComponent<Animator>().Play(animationName[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (objToPlane != null)
        {
            if (FindObjectsOfType<ARPlane>().Count() > 2)
            {
                realworldView.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = "Tap to place the object";

                if (Input.touchCount > 0)
                {
                    Touch touch = Input.GetTouch(0);

                    if (touch.phase == TouchPhase.Began)
                    {
                        if (arRaycastManager.Raycast(touch.position, aRRaycastHits, TrackableType.PlaneWithinPolygon))
                        {
                            Pose hitPose = aRRaycastHits[0].pose;

                            selectedExperimentObj = Instantiate(objToPlane, hitPose.position, hitPose.rotation);
                            selectedExperimentObj.transform.localScale = new Vector3(3, 3, 3);
                            selectedExperimentObj.transform.Rotate(new Vector3(0, 90, 0));

                            objToPlane = null;

                            arPlaneManager.enabled = false;
                            realworldView.transform.GetChild(0).gameObject.SetActive(false);

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
                selectedExperimentObj.transform.localScale = new Vector3(Mathf.Clamp(selectedExperimentObj.transform.localScale.x, 1f, 100),
                                                                   Mathf.Clamp(selectedExperimentObj.transform.localScale.y, 1f, 100),
                                                                   Mathf.Clamp(selectedExperimentObj.transform.localScale.z, 1f, 100));

            }
        }
    }

    async void metadata(string className, string experimentName)
    {
        await storageReference.Child("TeachingInAr").Child("models").Child(className).GetMetadataAsync().ContinueWith((Task<StorageMetadata> task) =>
        {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                Firebase.Storage.StorageMetadata meta = task.Result;
                Debug.Log(meta.Bucket);
                Debug.Log(task.Result);
            }
            else
            {
                Debug.Log(task.Exception);
            }
        });

        //Task task = storageReference.Child("TeachingInAr").Child("models").Child(className).Child(experimentName).GetDownloadUrlAsync();

        //await task.ContinueWith(resultTask => {
        //    if (!resultTask.IsFaulted && !resultTask.IsCanceled)
        //    {
        //        Debug.Log(resultTask);

        //    }
        //    else
        //    {
        //        Debug.Log(resultTask);
        //    }

        //});
    }

    async void downloadFile(string className,string experimentName)
    {
        Debug.Log("Downloading marker image...");
        string imagePath = Application.persistentDataPath + "/" + experimentName;
        // Start downloading a file

        Task task = storageReference.Child("TeachingInAr").Child("models").Child(className).Child(experimentName).GetFileAsync(imagePath,
              new StorageProgress<DownloadState>((DownloadState state) => {
                  // called periodically during the download
                  Debug.Log( state.BytesTransferred * 100 / state.TotalByteCount);
              }), System.Threading.CancellationToken.None);

        await task.ContinueWith(resultTask => {
            if (!resultTask.IsFaulted && !resultTask.IsCanceled)
            {
                Debug.Log("Marker image downloaded");

            }
            else
            {
                Debug.Log(resultTask.Exception);
            }

        });

    }


    IEnumerator GetDisplayBundleRoutine(string assetName, UnityAction<GameObject> callback)
    {

        string bundleURL = "http://thecampusentrepreneur.com/xealistic/models/class6/anemometer";

        Debug.Log("Requesting bundle at " + bundleURL);

        //request asset bundle
        UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(bundleURL);
        yield return www.SendWebRequest();

        while (!www.isDone)
        {
            Debug.Log((www.downloadProgress * 100) / 100);
        }

        Debug.Log("Download Done : " + www.isDone);
        Debug.Log("Progress : " + www.downloadProgress);
        Debug.Log("Bytes : " + www.downloadedBytes);

        if (www.isNetworkError)
        {
            Debug.Log("Network error");
        }
        else
        {
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
            if (bundle != null)
            {
                string rootAssetPath = bundle.GetAllAssetNames()[0];
                GameObject arObject = Instantiate(bundle.LoadAsset(rootAssetPath) as GameObject);
                bundle.Unload(false);
                callback(arObject);
            }
            else
            {
                Debug.Log("Not a valid asset bundle");
            }
        }
    }

    public IEnumerator DL()
    {
        string downloadlink = "http://thecampusentrepreneur.com/xealistic/models/class6/electroplating";
        string filepath = Application.persistentDataPath + "/electroplating";
        //Download
        UnityWebRequest dlreq = new UnityWebRequest(downloadlink);
        dlreq.downloadHandler = new DownloadHandlerFile(filepath);
        dlreq.timeout = 15;

        UnityWebRequestAsyncOperation op = dlreq.SendWebRequest();

        while (!op.isDone)
        {
            //here you can see download progress
            Debug.Log(dlreq.downloadedBytes / 1000 + "KB");

            yield return null;
        }

        if (dlreq.isNetworkError || dlreq.isHttpError)
        {
            Debug.Log(dlreq.error);
        }
        else
        {
            Debug.Log("download success");

            StartCoroutine(LoadFromMemoryAsync(filepath));
        }

        dlreq.Dispose();

        yield return null;

    }

    IEnumerator customWebRequest()
    {
        UnityWebRequest webRequest;
        //Pre-allocate memory so that this is not done each time data is received
        byte[] bytes = new byte[200000];

        string url = "https://drive.google.com/file/d/1OGyrB4-MQfo-HVom9ENvV4dn312_wL4Q/view?usp=sharing";
        webRequest = new UnityWebRequest(url);
        webRequest.downloadHandler = new CustomWebRequest(bytes);
        webRequest.SendWebRequest();
        yield return webRequest;
    }

    void getAndSetAsset()
    {
        string readPath = Application.dataPath + "/AssetBundles/electroplating";
        string savePath = Application.persistentDataPath + "/electroplating";
        System.IO.File.WriteAllBytes(savePath, File.ReadAllBytes(readPath));
    }

    IEnumerator GetAssetBundle2(string className, string experimentName)
    {
        Debug.Log("Fetching asset...");
        //UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle("http://thecampusentrepreneur.com/xealistic/models/class6/anemometer");
        UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle("https://drive.google.com/file/d/1LwxC1oZ1IsiiufVPFc63sdRUpShvIQW9/view?usp=sharing", 36, 0);
        yield return www.SendWebRequest();

        while (!www.isDone)
        {
            Debug.Log((www.downloadProgress * 100) / 100);
        }

        Debug.Log("Download Done : " + www.isDone);
        Debug.Log("Progress : " + www.downloadProgress);
        Debug.Log("Bytes : " + www.downloadedBytes);

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Fetched asset.");
            string savePath = Application.persistentDataPath + "/" + experimentName + "N";
            //System.IO.File.WriteAllBytes(savePath, www.downloadHandler.data);
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
            
            GameObject prefab = bundle.LoadAsset<GameObject>(experimentName);
            selectedObject = Instantiate(prefab);
            Debug.Log("Fetched asset Instantiated");
        }
    }

    IEnumerator GetText(string className, string experimentName)
    {
        //string url = "http://thecampusentrepreneur.com/xealistic/models/" + className + "/" + experimentName;
        string url = "https://drive.google.com/file/d/1LwxC1oZ1IsiiufVPFc63sdRUpShvIQW9/view?usp=sharing";
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.Send();

            while (!www.isDone)
            {
                Debug.Log((www.downloadProgress * 100) / 100);
            }

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                string savePath = Application.persistentDataPath + "/" + experimentName + "N";
                System.IO.File.WriteAllBytes(savePath, www.downloadHandler.data);

                //AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
                //GameObject prefab = bundle.LoadAsset<GameObject>("anemometer");
                //selectedObject = Instantiate(prefab);
                //Debug.Log("Fetched asset Instantiated");

                //StartCoroutine(LoadFromMemoryAsync(savePath));
            }
        }
    }

    byte[] MyDecription(byte[] binary)
    {
        byte[] decrypted = new byte[1024];
        return decrypted;
    }

    IEnumerator GetAssetBundleWWW(string className, string experimentName)
    {
        //Load the assetBundle file from Cache if it exists with the same version or download and store it in the cache.
        using (WWW www = new WWW("https://drive.google.com/file/d/1OGyrB4-MQfo-HVom9ENvV4dn312_wL4Q/view?usp=sharing"))
        {
            yield return www;
            if (www.error != null)
                Debug.Log("WWW download had an error:" + www.error);
            AssetBundle bundle = www.assetBundle;
            if (experimentName == "")
                Instantiate(bundle.mainAsset);
            else
                Instantiate(bundle.LoadAsset(experimentName));
            // Unload the AssetBundles compressed contents to conserve memory
            bundle.Unload(false);

        }

    }


    IEnumerator GetAssetBundle(string className, string experimentName)
    {
        Debug.Log("Fetching asset...");
        UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle("http://thecampusentrepreneur.com/xealistic/models/" + className + "/" + experimentName);
        yield return www.SendWebRequest();

        while (!www.isDone)
        {
            Debug.Log((www.downloadProgress * 100) / 100);
        }

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            AssetBundleCreateRequest createRequest = AssetBundle.LoadFromMemoryAsync(www.downloadHandler.data);
            yield return createRequest;
            AssetBundle bundle = createRequest.assetBundle;
            GameObject prefab = bundle.LoadAsset<GameObject>("electroplating");
            selectedObject = Instantiate(prefab);
            Debug.Log("Fetched asset.");
        }
    }

    IEnumerator GetDataUnityWebRequest(string className, string experimentName)
    {
        Debug.Log("Fetching asset...");
        //string url = "http://thecampusentrepreneur.com/xealistic/models/" + className + "/" + experimentName;
        string url = "https://drive.google.com/file/d/1OGyrB4-MQfo-HVom9ENvV4dn312_wL4Q/view?usp=sharing";
        UnityWebRequest www = new UnityWebRequest(url);
        www.downloadHandler = new DownloadHandlerBuffer();
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Fetched asset : " + www.downloadedBytes);
            AssetBundleCreateRequest createRequest = AssetBundle.LoadFromMemoryAsync(www.downloadHandler.data);
            yield return createRequest;
            AssetBundle bundle = createRequest.assetBundle;
            //AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
            GameObject prefab = bundle.LoadAsset<GameObject>(experimentName);
            selectedObject = Instantiate(prefab);
            Debug.Log("Fetched asset Instantiated");
        }
    }

    /*
    void Update()
    {

        if (Input.touchCount == 0)
        {
            Touch touch = Input.touches[0];
            Ray ray = Camera.main.ScreenPointToRay(touch.position);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f))
            {
                selectedObject = hit.collider.gameObject;
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
            deltaMagnitudeDiff = deltaMagnitudeDiff * speed;
            selectedObject.transform.localScale -= new Vector3(deltaMagnitudeDiff, deltaMagnitudeDiff, deltaMagnitudeDiff);
            selectedObject.transform.localScale = new Vector3( Mathf.Clamp(selectedObject.transform.localScale.x, 0.01f, 100),
                                                               Mathf.Clamp(selectedObject.transform.localScale.y, 0.01f, 100),
                                                               Mathf.Clamp(selectedObject.transform.localScale.z, 0.01f, 100));

        }
    }
    */
    IEnumerator LoadFromMemoryAsync(string path)
    {
        AssetBundleCreateRequest createRequest = AssetBundle.LoadFromMemoryAsync(File.ReadAllBytes(path));
        yield return createRequest;
        AssetBundle bundle = createRequest.assetBundle;
        GameObject prefab = bundle.LoadAsset<GameObject>("anemometer");
        selectedObject = Instantiate(prefab);
        Debug.Log("Fetched asset.");
    }
}
