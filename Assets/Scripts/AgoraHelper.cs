using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

using agora_gaming_rtc;
using agora_utilities;
using UnityEngine.EventSystems;
using System;


// this is an example of using Agora Unity SDK
// It demonstrates:
// How to enable video
// How to join/leave channel
// 
public class AgoraHelper
{

    // instance of agora engine
    private IRtcEngine mRtcEngine;

    // load agora engine
    public void loadEngine(string appId)
    {
        // start sdk
        Debug.Log("initializeEngine");

        if (mRtcEngine != null)
        {
            Debug.Log("Engine exists. Please unload it first!");
            return;
        }

        // init engine
        mRtcEngine = IRtcEngine.GetEngine(appId);

        // enable log
        mRtcEngine.SetLogFilter(LOG_FILTER.DEBUG | LOG_FILTER.INFO | LOG_FILTER.WARNING | LOG_FILTER.ERROR | LOG_FILTER.CRITICAL);
    }

    public void join(string channel)
    {
        Debug.Log("calling join (channel = " + channel + ")");

        if (mRtcEngine == null)
            return;

        // set callbacks (optional)
        mRtcEngine.OnJoinChannelSuccess = onJoinChannelSuccess;
        mRtcEngine.OnUserJoined = onUserJoined;
        mRtcEngine.OnUserOffline = onUserOffline;

        // enable video
        mRtcEngine.EnableVideo();
        // allow camera output callback
        mRtcEngine.EnableVideoObserver();

        // join channel
        mRtcEngine.JoinChannel(channel, null, 0).ToString();

        // Optional: if a data stream is required, here is a good place to create it
        int streamID = mRtcEngine.CreateDataStream(true, true);
        Debug.Log("initializeEngine done, data stream id = " + streamID);
    }

    public string getSdkVersion()
    {
        string ver = IRtcEngine.GetSdkVersion();
        if (ver == "2.9.1.45")
        {
            ver = "2.9.2";  // A conversion for the current internal version#
        }
        else
        {
            if (ver == "2.9.1.46")
            {
                ver = "2.9.2.2";  // A conversion for the current internal version#
            }
        }
        return ver;
    }

    public void leave()
    {
        Debug.Log("calling leave");

        if (mRtcEngine == null)
            return;

        // leave channel
        mRtcEngine.LeaveChannel();
        // deregister video frame observers in native-c code
        mRtcEngine.DisableVideoObserver();
    }

    // unload agora engine
    public void unloadEngine()
    {
        Debug.Log("calling unloadEngine");

        // delete
        if (mRtcEngine != null)
        {
            IRtcEngine.Destroy();  // Place this call in ApplicationQuit
            mRtcEngine = null;
        }
    }


    public void EnableVideo(bool pauseVideo)
    {
        if (mRtcEngine != null)
        {
            if (!pauseVideo)
            {
                mRtcEngine.EnableVideo();
            }
            else
            {
                mRtcEngine.DisableVideo();
            }
        }
    }

    // accessing GameObject in Scnene1
    // set video transform delegate for statically created GameObject
    public void onSceneHelloVideoLoaded()
    {
        // Attach the SDK Script VideoSurface for video rendering

        GameObject userView = GameObject.Instantiate(GameObject.Find("GameController").GetComponent<AgoraManager>().userView);
        //userView.transform.parent = GameObject.Find("Panel").transform;
        userView.transform.parent = GameObject.Find("ListContent").transform;
        userView.name = "LocalPlayer";
        userView.GetComponent<Button>().onClick.AddListener(changeMainVideoFeed);
        if (ReferenceEquals(userView, null))
        {
            Debug.Log("BBBB: failed to find Quad");
            return;
        }
        else
        {
            userView.AddComponent<VideoSurface>();
            GameObject userGView = GameObject.Instantiate(GameObject.Find("GameController").GetComponent<AgoraManager>().userView);
            userGView.transform.parent = GameObject.Find("GridView").transform;
            userGView.name = "LocalPlayer";
            userGView.AddComponent<VideoSurface>();
        }
        GameObject.Find("GameController").GetComponent<AgoraManager>().selectedUsers.Add("LocalPlayer");
        Color selectedColor = new Color(140/255f, 140/255f, 140/255f);
        userView.GetComponent<RawImage>().color = selectedColor;
    }

    // implement engine callbacks
    private void onJoinChannelSuccess(string channelName, uint uid, int elapsed)
    {
        GameObject.Find("GameController").GetComponent<AgoraManager>().localUserID = "" + uid;
        //GameObject.Find("LocalPlayer").name = "" + uid;
        //Debug.Log(GameObject.Find("GameController").GetComponent<AgoraManager>().localUserID);
        Debug.Log("JoinChannelSuccessHandler: uid = " + uid);
        GameObject textVersionGameObject = GameObject.Find("VersionText");
    }

    private void firstUserJoin()
    {
        //yield return new WaitForSeconds(4f);
        GameObject gridView = GameObject.Find("GridView");
        GameObject userView = GameObject.Instantiate(GameObject.Find("GameController").GetComponent<AgoraManager>().userView);
        userView.transform.parent = gridView.transform;
        userView.GetComponent<RawImage>().texture = GameObject.Find("LocalPlayer").GetComponent<RawImage>().texture;
        Debug.Log("Texture Changed to : " + GameObject.Find("LocalPlayer").GetComponent<RawImage>().texture.name);
    }

    // When a remote user joined, this delegate will be called. Typically
    // create a GameObject to render video on it
    private void onUserJoined(uint uid, int elapsed)
    {
        Debug.Log("onUserJoined: uid = " + uid + " elapsed = " + elapsed);
        // this is called in main thread

        // find a game object to render video stream from 'uid'
        GameObject userView = GameObject.Instantiate(GameObject.Find("GameController").GetComponent<AgoraManager>().userView);
        userView.name = uid.ToString();
        //userView.transform.parent = GameObject.Find("Panel").transform;
        userView.transform.parent = GameObject.Find("ListContent").transform;
        userView.GetComponent<Button>().onClick.AddListener(changeMainVideoFeed);
        // create a GameObject and assign to this new user
        VideoSurface videoSurface = userView.AddComponent<VideoSurface>();
        if (!ReferenceEquals(videoSurface, null))
        {
            // configure videoSurface
            videoSurface.SetForUser(uid);
            videoSurface.SetEnable(true);
            videoSurface.SetVideoSurfaceType(AgoraVideoSurfaceType.RawImage);
            videoSurface.SetGameFps(30);
        }

        GameObject userGView = GameObject.Instantiate(GameObject.Find("GameController").GetComponent<AgoraManager>().userView);
        userGView.name = uid.ToString();
        userGView.SetActive(false);
        userGView.transform.parent = GameObject.Find("UsersNotSelected").transform;
        VideoSurface videoSurfaceG = userGView.AddComponent<VideoSurface>();
        if (!ReferenceEquals(videoSurfaceG, null))
        {
            // configure videoSurface
            videoSurfaceG.SetForUser(uid);
            videoSurfaceG.SetEnable(true);
            videoSurfaceG.SetVideoSurfaceType(AgoraVideoSurfaceType.RawImage);
            videoSurfaceG.SetGameFps(30);
        }
    }

    public VideoSurface makePlaneSurface(string goName)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Plane);

        if (go == null)
        {
            return null;
        }
        go.name = goName;
        // set up transform
        go.transform.Rotate(-90.0f, 0.0f, 0.0f);
        float yPos = UnityEngine.Random.Range(3.0f, 5.0f);
        float xPos = UnityEngine.Random.Range(-2.0f, 2.0f);
        go.transform.position = new Vector3(xPos, yPos, 0f);
        go.transform.localScale = new Vector3(0.25f, 0.5f, .5f);

        // configure videoSurface
        VideoSurface videoSurface = go.AddComponent<VideoSurface>();
        return videoSurface;
    }

    private const float Offset = 100;
    public VideoSurface makeImageSurface(string goName)
    {
        GameObject go = new GameObject();

        if (go == null)
        {
            return null;
        }

        go.name = goName;

        // to be renderered onto
        go.AddComponent<RawImage>();

        // make the object draggable
        go.AddComponent<UIElementDragger>();
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            go.transform.parent = canvas.transform;
        }
        // set up transform
        go.transform.Rotate(0f, 0.0f, 180.0f);
        float xPos = UnityEngine.Random.Range(Offset - Screen.width / 2f, Screen.width / 2f - Offset);
        float yPos = UnityEngine.Random.Range(Offset, Screen.height / 2f - Offset);
        go.transform.localPosition = new Vector3(xPos, yPos, 0f);
        go.transform.localScale = new Vector3(3f, 4f, 1f);

        // configure videoSurface
        VideoSurface videoSurface = go.AddComponent<VideoSurface>();
        return videoSurface;
    }
    // When remote user is offline, this delegate will be called. Typically
    // delete the GameObject for this user
    private void onUserOffline(uint uid, USER_OFFLINE_REASON reason)
    {
        // remove video stream
        Debug.Log("onUserOffline: uid = " + uid + " reason = " + reason);
        // this is called in main thread
        GameObject go = GameObject.Find(uid.ToString());
        if (!ReferenceEquals(go, null))
        {
            UnityEngine.Object.Destroy(go);
        }
    }
    void changeMainVideoFeed()
    {
        AgoraManager agoraManager = GameObject.Find("GameController").GetComponent<AgoraManager>();
        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
        if (!agoraManager.selectedUsers.Contains(selectedObject.name))
        {
            selectedObject.GetComponent<RawImage>().color = new Color(140/255f, 140/255f, 140/255f);
            agoraManager.selectedUsers.Add(selectedObject.name);
        }
        else
        {
            selectedObject.GetComponent<RawImage>().color = new Color(1, 1, 1);
            agoraManager.selectedUsers.Remove(selectedObject.name);
        }
    }
}
