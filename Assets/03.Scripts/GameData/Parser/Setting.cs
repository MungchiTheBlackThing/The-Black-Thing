using System.Collections;
using System.Collections.Generic;
using System;
using JetBrains.Annotations;
using UnityEngine;

[System.Serializable]
public class MainMenu
{
    public List<string> title;
    public List<string> howto;
    public List<string> progress;
    public List<string> mypage;
}

[System.Serializable]
public class MenuMyPage
{
    public List<string> settings;
    public List<string> community;
    public List<string> credit;
}

[System.Serializable]
public class NameSetting
{
    public List<string> title;
    public List<string> placeholder;
    public List<string> no;
    public List<string> yes;
}

[System.Serializable]
public class Pushoff
{
    public List<string> title;
    public List<string> no;
    public List<string> yes;
}

[System.Serializable]
public class Settings
{
    public List<string> name;
    public List<string> BGM;
    public List<string> SFX;
    public List<string> alert;
    public List<string> namechanged;
    public NameSetting namesetting;
    public Pushoff pushoff;
    public List<string> language;
}

[System.Serializable]
public class Community
{
    public List<string> instagram;
    public List<string> discord;
    public List<string> discordTip;
    public List<string> x;
    public List<string> url;
}

[System.Serializable]
public class Credit
{
}

[System.Serializable]
public class RadioOn
{
    public List<string> text;
    public List<string> earth;
    public List<string> moon;
    public List<string> exit;
}

[System.Serializable]
public class RadioOff
{
    public List<string> text;
    public List<string> yes;
    public List<string> no;
}


[System.Serializable]
public class MoonRadioMain
{
    public RadioOn radioOn;
    public RadioOff radioOff;
}
[System.Serializable]
public class MoonRadioEarth
{
    public List<string> placeholder;
    public List<string> alert;
    public List<string> exit;
    public List<string> send;
    public List<string> popupExit;
    public List<string> yes;
    public List<string> no;
}

[System.Serializable]
public class Checklist
{
    public List<string> phase1;
    public List<string> phase2;
    public List<string> phase3;
    public List<string> phase4;
}

[System.Serializable]
public class TimeSkip
{
    public List<string> title;
    public List<string> no;
    public List<string> yes;
}

[System.Serializable]
public class Alert
{
    public List<string> moonRadio;
    public List<string> diary;
}

[System.Serializable]
public class SettingInfo
{
    public MainMenu menu;
    public MenuMyPage menuMyPage;
    public Settings settings;
    public Community community;
    public TimeSkip timeSkip;
    public MoonRadioMain moonRadioMain;
    public MoonRadioEarth moonRadioEarth;
    public Checklist checklist;
    public Alert alert;
}
