using System.Collections.Generic;
[System.Serializable]
public class Process
{
    public List<string> objectsRequired { get; set; }
    public Dictionary<string,List<Dictionary<string,string>>> steps { get; set; }
    public string conclusion { get; set; }
    public int index;
}