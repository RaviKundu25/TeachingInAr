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

public class MainSceneManager : MonoBehaviour
{
    DatabaseReference reference;
    StorageReference storageReference;

    [SerializeField]
    float speed;
    [SerializeField]
    Dropdown classOption;
    [SerializeField]
    Dropdown subjectOption;
    [SerializeField]
    Dropdown experimentOption;

    string selectedClass = "None";
    string selectedSubject = "None";
    string selectedExperiment = "None";
    Dictionary<string ,SchoolClass> schoolClasses;

    // UI Canvas
    [SerializeField]
    GameObject appGround;
    [SerializeField]
    GameObject UIView;
    //[SerializeField]
    //GameObject realworldView;
    [SerializeField]
    GameObject statusCanvas;

    //Save Data offline
    BinaryFormatter bf;
    FileStream file;

    private void Start()
    {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://xr-app-600b1.firebaseio.com/");
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        storageReference = FirebaseStorage.DefaultInstance.RootReference;

        getApplicationData();
    }

    async void getApplicationData()
    {
        statusCanvas.SetActive(true);
        statusCanvas.transform.GetChild(0).GetChild(1).GetComponent<Text>().text = "Loading application data...";

        classOption.ClearOptions();
        schoolClasses = new Dictionary<string, SchoolClass>();

        await reference.Child("TeachingInAr").GetValueAsync().ContinueWith(task =>
        {

            if (task.IsFaulted)
            {
                Debug.Log("Error occured.");
                return;
            }
            else if (task.IsCompleted)
            {
                DataSnapshot classes = task.Result;

                foreach (DataSnapshot classInst in classes.Children)
                {
                    SchoolClass schoolClass = new SchoolClass();
                    schoolClass.subjects = new Dictionary<string, Subject>();

                    foreach (DataSnapshot subjectInst in classInst.Children)
                    {
                        Subject subject = new Subject();
                        subject.experiments = new Dictionary<string, Experiment>();

                        foreach (DataSnapshot experimentInst in subjectInst.Children)
                        {
                            Experiment experiment = new Experiment();
                            experiment.processes = new Dictionary<string, Process>();
                            foreach (DataSnapshot processInst in experimentInst.Children)
                            {
                                Process process = new Process();

                                if (processInst.Child("ObjectsRequired").Exists)
                                {
                                    process.objectsRequired = new List<string>();
                                    foreach (DataSnapshot objectRequired in processInst.Child("ObjectsRequired").Children)
                                    {
                                        process.objectsRequired.Add(objectRequired.Value.ToString());
                                    }
                                }

                                if (processInst.Child("Steps").Exists)
                                {
                                    process.steps = new Dictionary<string, List<Dictionary<string, string>>>();
                                    foreach (DataSnapshot stepInst in processInst.Child("Steps").Children)
                                    {
                                        Dictionary<string, string> stepInfo = new Dictionary<string, string>();
                                        stepInfo.Add("stepInfo", stepInst.Child("stepInfo").Value.ToString());
                                        Dictionary<string, string> stepAudio = new Dictionary<string, string>();
                                        stepAudio.Add("stepAudio", stepInst.Child("stepAudio").Value.ToString());
                                        List<Dictionary<string, string>> step = new List<Dictionary<string, string>>();
                                        step.Add(stepInfo);
                                        step.Add(stepAudio);
                                        process.steps.Add(stepInst.Value.ToString(), step);
                                    }
                                }

                                if (processInst.Child("Conclusion").Exists)
                                {
                                    process.conclusion = processInst.Child("Conclusion").Value.ToString();
                                }

                                process.index = int.Parse(processInst.Child("Index").Value.ToString());

                                experiment.processes.Add(processInst.Key.ToString(), process);
                            }
                            subject.experiments.Add(experimentInst.Key.ToString(), experiment);
                        }
                        schoolClass.subjects.Add(subjectInst.Key.ToString(), subject);
                    }
                    schoolClasses.Add(classInst.Key.ToString(), schoolClass);
                }
            }
        });

        List<string> classNames = new List<string>();

        foreach (string className in schoolClasses.Keys.ToList())
        {
            classNames.Add(className);
            string schoolPath = Application.persistentDataPath + "/" + className.ToLower().Replace(" ", "_");
            if (!Directory.Exists(schoolPath))
            {
                Directory.CreateDirectory(schoolPath);
            }
            foreach (string subjectName in schoolClasses[className].subjects.Keys.ToList())
            {
                string subjectPath = schoolPath + "/" + subjectName.ToLower().Replace(" ", "_");
                if (!Directory.Exists(subjectPath))
                {
                    Directory.CreateDirectory(subjectPath);
                }

                foreach (string experimentName in schoolClasses[className].subjects[subjectName].experiments.Keys.ToList())
                {
                    string experimentPath = subjectPath + "/" + experimentName.ToLower().Replace(" ", "_");
                    if (!Directory.Exists(experimentPath))
                    {
                        Directory.CreateDirectory(experimentPath);
                    }

                    foreach (string processName in schoolClasses[className].subjects[subjectName].experiments[experimentName].processes.Keys.ToList())
                    {
                        string processPath = experimentPath + "/" + processName.ToLower().Replace(" ", "_");
                        schoolClasses[className].subjects[subjectName].experiments[experimentName].downloaded = "No";

                        if (File.Exists(processPath))
                        {
                            schoolClasses[className].subjects[subjectName].experiments[experimentName].downloaded = "Yes";
                        }
                    }
                }
            }
            
        }

        saveUserData();

        classNames.Insert(0, "None");
        classOption.AddOptions(classNames);

        statusCanvas.SetActive(false);
        appGround.SetActive(false);
        UIView.SetActive(true);
    }

    void saveUserData()
    {
        bf = new BinaryFormatter();

        file = File.Create(Application.persistentDataPath + "/Data.dat");
        bf.Serialize(file, schoolClasses);
        file.Close();

        if (selectedClass != "None")
        {
            startARScene();
        }
    }

    public void classSelected(int index)
    {
        Debug.Log("Class selected : " + classOption.options[index].text);

        selectedClass = classOption.options[index].text;

        try
        {
            List<string> subjectsNameForSelectedClass = new List<string>();
            subjectOption.ClearOptions();
            subjectsNameForSelectedClass = schoolClasses[classOption.options[index].text].subjects.Keys.ToList();
            subjectsNameForSelectedClass.Insert(0, "None");
            subjectOption.AddOptions(subjectsNameForSelectedClass);

            List<string> experimentsNameForSelectedClass = new List<string>();
            experimentOption.ClearOptions();
            experimentsNameForSelectedClass.Insert(0, "None");
            experimentOption.AddOptions(experimentsNameForSelectedClass);

        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public void subjectSelected(int index)
    {
        Debug.Log("Subject of " + selectedClass + " selected : " + subjectOption.options[index].text);

        selectedSubject = subjectOption.options[index].text;

        try
        {
            List<string> experimentsNameForSelectedClass = new List<string>();
            experimentOption.ClearOptions();
            experimentsNameForSelectedClass = schoolClasses[selectedClass].subjects[selectedSubject].experiments.Keys.ToList();
            experimentsNameForSelectedClass.Insert(0, "None");
            experimentOption.AddOptions(experimentsNameForSelectedClass);

        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public void experimentSelected(int index)
    {
        Debug.Log("Experiment of " + selectedSubject + " of " + selectedClass + " selected : " + experimentOption.options[index].text);

        selectedExperiment = experimentOption.options[index].text;

        if (schoolClasses[selectedClass].subjects[selectedSubject].experiments[selectedExperiment].downloaded == "Yes")
        {
            Debug.Log(selectedClass + "-" + selectedSubject + "-" + selectedExperiment + " already downloaded.");
            startARScene();
        }
        else
        {
            foreach (string processName in schoolClasses[selectedClass].subjects[selectedSubject].experiments[selectedExperiment].processes.Keys.ToList())
            {
                Debug.Log(selectedClass + "-" + selectedSubject + "-" + selectedExperiment + " not downloaded.");
                GetExperimentFromFirebase(selectedClass, selectedSubject, selectedExperiment, processName);
            }
        }
    }

    async void GetExperimentFromFirebase(string className, string subjectName, string experimentName, string processName)
    {
        statusCanvas.SetActive(true);
        statusCanvas.transform.GetChild(0).GetChild(1).GetComponent<Text>().text = "Loading experiment data...";

        Debug.Log("Downloading asset bundle : " + className + "-" + subjectName + "-" + experimentName + "-" + processName);
        string assetPath = Application.persistentDataPath + "/" + className + "/" + subjectName + "/" + experimentName + "/" + processName.ToLower().Replace(" ", "_");

        // Start downloading a file
        Task task = storageReference.Child("TeachingInAr").Child("models").Child(className).Child(subjectName).Child(experimentName).Child(processName.ToLower().Replace(" ", "_")).GetFileAsync(assetPath,
              new StorageProgress<DownloadState>((DownloadState state) => {
                  // called periodically during the download
                  Debug.Log(state.BytesTransferred * 100 / state.TotalByteCount);
              }), System.Threading.CancellationToken.None);

        await task.ContinueWith(resultTask => {
            if (!resultTask.IsFaulted && !resultTask.IsCanceled)
            {
                Debug.Log("Downloaded asset bundle : " + className + "-" + subjectName + "-" + experimentName + "-" + processName);
            }
            else
            {
                Debug.Log(resultTask.Exception);
            }

        });

        string lastProcessName = schoolClasses[selectedClass].subjects[selectedSubject].experiments[selectedExperiment].processes.ElementAt(
                                 schoolClasses[selectedClass].subjects[selectedSubject].experiments[selectedExperiment].processes.Count - 1).Key;

        if (processName == lastProcessName)
        {
            schoolClasses[selectedClass].subjects[selectedSubject].experiments[selectedExperiment].downloaded = "Yes";
            Debug.Log(className + "-" + subjectName + "-" + experimentName + " completely downloaded.");
            saveUserData();
        }
    }

    void startARScene()
    {
        PlayerPrefs.SetString("className", selectedClass);
        PlayerPrefs.SetString("subjectName", selectedSubject);
        PlayerPrefs.SetString("experimentName", selectedExperiment);
        Application.LoadLevel(1);
    }
}
