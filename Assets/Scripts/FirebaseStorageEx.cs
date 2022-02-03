using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Storage;
using Firebase.Unity.Editor;
using System.IO;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Threading.Tasks;

public class FirebaseStorageEx : MonoBehaviour
{
    public StorageReference storageReference;
    public RawImage rawImage;
    public Slider slider;

    public Texture2D texture2D;

    string UID;
    string projectID;

    // Start is called before the first frame update
    void Start()
    {
        storageReference = FirebaseStorage.DefaultInstance.RootReference;

        UID = "UID";
        projectID = "ProjectDemo";

        //downloadMarkerFileProgress();
        downloadMarkerFileProgress();
    }

    async void downloadMarkerFile()
    {
        string path = Application.dataPath;
        await storageReference.Child("IMG_20180829_182157.jpg").GetFileAsync("file:///image.jpeg").ContinueWith(task =>
        {
            if (!task.IsFaulted && !task.IsCanceled)
            {                
                Debug.Log("File downloaded : " + task.ToString());
            }
        });

    }

    async void downloadMarkerFileProgress()
    {
        Debug.Log("Downloading marker image...");
        string imagePath = Application.persistentDataPath + "/marker.jpeg";
        // Start downloading a file
        Task task = storageReference.Child("UserData").Child("xH0MOVaQw6RaF2ldzKg5xN1UzV12").Child("-MFKhAg-0chpMvdQv1v-").Child("Custom").GetFileAsync(imagePath,
              new StorageProgress<DownloadState>((DownloadState state) => {
                  // called periodically during the download
                  slider.value = state.BytesTransferred * 100 / state.TotalByteCount;
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

        downloadMarker();
    }

    async void downloadMarker()
    {
        Debug.Log("Setting up marker...");
        const long maxAllowedSize = 10 * 1024 * 1024;
        byte[] fileContents = null;

        await storageReference.Child("UserData").Child("xH0MOVaQw6RaF2ldzKg5xN1UzV12").Child("-MFKhAg-0chpMvdQv1v-").Child("Custom")
            .GetBytesAsync(maxAllowedSize).ContinueWith((Task<byte[]> task) =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.Log(task.Exception.ToString());
                    // Uh-oh, an error occurred!
                }
                else
                {
                    fileContents = task.Result;
                    Debug.Log("Marker set.");
                }
            });

        texture2D = new Texture2D((int)rawImage.uvRect.width, (int)rawImage.uvRect.height);
        texture2D.LoadImage(fileContents);
        texture2D.Apply();

        rawImage.texture = texture2D;
    }

    /*
    IEnumerator FileExplorer()
    {
        string path = EditorUtility.OpenFilePanel("Select Image Target", "", "jpg");
        if (path != null)
        {
            WWW www = new WWW("file:///" + path);
            yield return www;
            //uploadMarker(www.bytes);
            Debug.Log(www.texture.name);

        }
        else
        {
            Debug.Log("Couldn't get file path");
        }
    }
    */
    
    async void uploadMarker()
    {
        /*
        await storageReference.Child("UserData").Child("xH0MOVaQw6RaF2ldzKg5xN1UzV12").Child("-MFKhAg-0chpMvdQv1v-").Child("CustomBytes").PutBytesAsync(texture2D.EncodeToJPG()).ContinueWith((Task<StorageMetadata> task) =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.Log(task.Exception.ToString());
                
            }
            else
            {
                Debug.Log("Finished uploading.");
            }
        });
        */
        await storageReference.Child("Default Data").Child("Marker").Child("DefaultBytes").PutBytesAsync(texture2D.EncodeToJPG()).ContinueWith((Task<StorageMetadata> task) =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.Log(task.Exception.ToString());

            }
            else
            {
                Debug.Log("Finished uploading.");
            }
        });
    }

    public async void uploadDataFile()
    {
        /*
        await storageReference.Child("UserData").Child(UID).Child(projectID).Child("Custom").PutFileAsync(path).ContinueWith((task) =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.Log("Error in loading user details for sharing project.");
                Debug.Log(task.Exception);
                return;
            }
            else
            {
                Debug.Log("Finished uploading marker to storage for project : " + projectID);
            }
        });*/

        Task task = storageReference.Child("Default Data").Child("Marker").Child("Default").PutFileAsync("Default.jpg", null,
        new StorageProgress<UploadState>(state => {
            // called periodically during the upload
            Debug.Log((state.BytesTransferred * 10 / state.TotalByteCount) * 10 + "%");
        }), System.Threading.CancellationToken.None, null);

        await task.ContinueWith(resultTask => {
            if (!resultTask.IsFaulted && !resultTask.IsCanceled)
            {
                Debug.Log("Upload finished.");
            }
        });
    }
}
