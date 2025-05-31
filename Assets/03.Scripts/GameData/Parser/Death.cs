using System.Collections;
using System.Collections.Generic;
using System;
using JetBrains.Annotations;
using UnityEngine;

[System.Serializable]
public class Lines
{
    public List<string> One;
    public List<string> Two;
    public List<string> Three;
    public List<string> Four;
}

[Serializable]
public class Death
{
    public Lines Sun;
    public Lines Moon;
}
