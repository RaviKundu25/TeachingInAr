using System.Collections.Generic;
[System.Serializable]
public class Experiment
{
    public Dictionary<string, Process> processes { get; set; }
    public string downloaded { get; set; }
}