using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCommunitySite : MonoBehaviour
{
    readonly string[] url = { "https://discord.gg/gDnphBApmK", 
        "https://www.instagram.com/moonstew_games/", 
        "https://twitter.com/moonstew_games" };
    public enum Type : int
    {
        Discord,
        Instagram,
        Twitter,
        Invailed,
    };

    public void OnClick(string type)
    {
        Type urlType = Type.Invailed;
        switch (type)
        {
            case "Discord":
                urlType = Type.Discord;
                break;
            case "Instagram":
                urlType = Type.Instagram;
                break;
            case "Twitter":
                urlType = Type.Twitter;
                break;
        }
        if (urlType != Type.Invailed)
            OpenURL(urlType);
    }
    void OpenURL(Type type)
    {
        Application.OpenURL(url[(int)type]);
    }
}
