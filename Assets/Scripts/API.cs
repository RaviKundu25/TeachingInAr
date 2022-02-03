using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class API : MonoBehaviour {

    const string BundleFolder = "gs://xr-app-600b1.appspot.com/";
    //StorageReference storageReference;

    private void Start()
    {
        // Set up the Editor before calling into the realtime database.
        //FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://xr-app-600b1.firebaseio.com/");

        // Get the root reference location of the database.
        //storageReference = FirebaseStorage.DefaultInstance.RootReference;
    }

    public void GetBundleObject(string assetName, UnityAction<GameObject> callback, Transform bundleParent) {
        //StartCoroutine(GetDisplayBundleRoutine(assetName, callback, bundleParent));
        downloadObjectFromFirebase(assetName, callback, bundleParent);
    }

    async void downloadObjectFromFirebase(string assetName, UnityAction<GameObject> callback, Transform bundleParent)
    {
        string imagePath = "water cycle";
        string path = Path.Combine(Application.dataPath, imagePath);
        /*
        Task task = storageReference.Child("water-cycle-Android")
            .GetFileAsync(path, new StorageProgress<DownloadState>((DownloadState state) =>
            {
                // called periodically during the download
                Debug.Log((state.BytesTransferred * 10 / state.TotalByteCount) * 10 + "%");
            }), System.Threading.CancellationToken.None);

        await task.ContinueWith(resultTask => {
            if (!resultTask.IsFaulted && !resultTask.IsCanceled)
            {
                Debug.Log("Download finished.");
            }

        });*/

        StartCoroutine(LoadFromMemoryAsync(path));
    }

    IEnumerator LoadFromMemoryAsync(string path)
    {
        AssetBundleCreateRequest createRequest = AssetBundle.LoadFromMemoryAsync(File.ReadAllBytes(path));
        yield return createRequest;
        AssetBundle bundle = createRequest.assetBundle;
        GameObject prefab = bundle.LoadAsset<GameObject>("water cycle");
        Instantiate(prefab);
    }

    IEnumerator GetDisplayBundleRoutine(string assetName, UnityAction<GameObject> callback, Transform bundleParent) {

        string bundleURL = BundleFolder + assetName + "-";

        //append platform to asset bundle name
#if UNITY_ANDROID
        bundleURL += "Android";
#else
        bundleURL += "IOS";
#endif

        Debug.Log("Requesting bundle at " + bundleURL);

        //request asset bundle
        UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(bundleURL);
        yield return www.SendWebRequest();

        if (www.isNetworkError) {
            Debug.Log("Network error");
        } else {
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
            if (bundle != null) {
                string rootAssetPath = bundle.GetAllAssetNames()[0];
                GameObject arObject = Instantiate(bundle.LoadAsset(rootAssetPath) as GameObject,bundleParent);
                bundle.Unload(false);
                callback(arObject);
            } else {
                Debug.Log("Not a valid asset bundle");
            }
        }
    }
}
