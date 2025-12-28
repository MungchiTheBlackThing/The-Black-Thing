using Assets.Script.TimeEnum;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static ObjectPool;
using Assets.Script.Reward;

[Serializable]
public class GooglePath
{
    [SerializeField]
    public SITime Time;
    [SerializeField]
    public string path;
}

public class ObjectManager : MonoBehaviour
{
    //모든 상태가 오브젝트들을 공유한다.
    private ObjectPool pool;

    List<GameObject>  watches;

    [SerializeField]
    GameObject skipSleep;

    [SerializeField]
    Dictionary<string, GameObject> mains;

    public delegate void ActiveSystemUIDelegate(bool InActive);

    public ActiveSystemUIDelegate activeSystemUIDelegate;

    bool isObjectLoadComplete;
    float loadProgress;

    string currentTime;

    PlayerController pc;

    [SerializeField]
    List<GooglePath> googlePath;

    //Dictionary<현재 시간, FileID> FileID; 제공
    public ObjectManager()
    {
        pool = new ObjectPool();
        mains = new Dictionary<string, GameObject>();
        watches = new List<GameObject>();
    }

    private void Start()
    {
        pc = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        pc.successSubDialDelegate += SuccessSubDial;
    }

    void SuccessSubDial(int phases, string subTitle)
    {
        Debug.Log("서브 타이틀:" + subTitle);
        //Player의 subSuccessOrNot을 가져와서 해당 idx true 시킨다.

        int chapter = pc.GetChapter();

        string reward = "reward"+subTitle.Substring(subTitle.IndexOf('_'));
        Debug.Log(reward);

        EReward eReward;

        if(Enum.TryParse<EReward>(reward,true,out eReward))
        {
            Debug.Log("데이터매니저 챕터리스트 리워드 길이" + DataManager.Instance.ChapterList.chapters[chapter].reward.Length);
            for (int i = 0; i < DataManager.Instance.ChapterList.chapters[chapter].reward.Length; i++)
            {
                string tmp = DataManager.Instance.ChapterList.chapters[chapter].reward[i];

                if (tmp.Contains(reward))
                {
                    string path = "Reward/" + currentTime + "/"+reward;

                     //호출 Resource에서 해당 Time부분에 있는 reward 업로드
                     GameObject rewardObj = Resources.Load<GameObject>(path);
                     GameObject realObj = Instantiate(rewardObj, this.transform);
                     pool.InsertMemory(realObj);
                }
            }
        }
        //실패하면 보상없음
    }

    public GameObject SetMain(string background)
    {
        Debug.Log("setmain 배경: " + background);
        if(mains.ContainsKey(background))
        {
            foreach (var w in mains)
            {
                w.Value.SetActive(false);
            }

            mains[background].SetActive(true);

            return mains[background];
        }

        return null;
    }

    // 로드 완료 여부를 반환
    public bool IsLoadObjectComplete()
    {
        return isObjectLoadComplete;
    }

    // 현재 로드 진행 상황을 반환
    public float GetLoadProgress()
    {
        return loadProgress;
    }

    private void LoadMainBackground(AssetBundle bundle)
    {

        if(bundle != null)
        {
            GameObject[] prefab = bundle.LoadAllAssets<GameObject>();
            

            foreach(GameObject pf in prefab)
            {
                GameObject realObj = Instantiate(pf,this.transform);

                string name = realObj.name.Substring(0, realObj.name.IndexOf("(")); 
                realObj.name = name; //(clone)을 찾아냄.
                mains.Add(name,realObj);
                realObj.SetActive(false);
            }

            bundle.Unload(false);
        }
    }

    // 비동기 로드를 위한 코루틴
    public IEnumerator LoadObjectAsync(string path, int chapter)
    {
        string tmpPath = "";
        for (int idx = 0; idx < googlePath.Count; idx++)
        {
            if (googlePath[idx].Time.ToString() == path)
            {
                tmpPath = googlePath[idx].path;
                break;
            }
        }

        yield return StartCoroutine(LoadAssetBundleFromLocal(tmpPath, LoadMainBackground));

        //string MainPath = "https://drive.google.com/uc?export=download&id="+ tmpPath;
        //Action<AssetBundle> callback = LoadMainBackground;
        // yield return StartCoroutine(pool.LoadFromMemoryAsync(MainPath, callback));

        isObjectLoadComplete = false;  // 로드가 시작되므로 false로 설정
        loadProgress = 0f;  // 진행 상황 초기화

        // 동기적으로 경로에서 모든 리소스를 먼저 가져옵니다. 
        // 이것은 경로에 어떤 오브젝트가 있는지 확인하는 단계일 뿐, 아직 오브젝트를 로드하지 않음.

        //메인 로드를 여기서 로드하자

        System.Object[] allObjects = Resources.LoadAll(path, typeof(GameObject));
        int totalObjects = allObjects.Length;
        currentTime = path;
        int i = 0;

        
        bool isCompleted = pc.GetPlayerInfo().isDay8SleepEventCompleted;

        foreach (GameObject obj in allObjects)
        {
            // 각 오브젝트를 비동기적으로 로드
            ResourceRequest resourceRequest = Resources.LoadAsync<GameObject>(path + "/" + obj.name);
            
            while (!resourceRequest.isDone)
            {
                loadProgress = (i + resourceRequest.progress) / totalObjects;  // 진행률 업데이트
                yield return null;
            }

            if (resourceRequest.asset != null)
            {
                GameObject obj2 = resourceRequest.asset as GameObject;

                // Instantiate를 통해 오브젝트 생성 후 삽입
                GameObject newObj = Instantiate(obj2, this.transform);

                // "(Clone)" 제거
                string name = newObj.name.Substring(0, newObj.name.IndexOf("("));
                newObj.name = name;

                // InsertMemory 내 삽입
                pool.InsertMemory(newObj);

                var baseObj = newObj.GetComponent<BaseObject>();
                bool active = baseObj != null && baseObj.IsCurrentChapter(chapter);

                // Day8 곰팡이 예외 처리
                var ch_bread = newObj.GetComponent<ChBreadObject>();
                if (ch_bread != null)
                {
                    active = active && ch_bread.ShouldBeActive(isCompleted);
                }

                newObj.SetActive(active);
            }
            
            i++;

        }

        List<EReward> Reward = pc.GetRewards();
        for (int rewardIdx = 0; rewardIdx < Reward.Count; rewardIdx++)
        {
            tmpPath = "Reward/" + currentTime + "/" + Reward[rewardIdx].ToString();

            //호출 Resource에서 해당 Time부분에 있는 reward 업로드
            ResourceRequest resourceRequest = Resources.LoadAsync<GameObject>(tmpPath);

            while (!resourceRequest.isDone)
            {
                loadProgress = (i + resourceRequest.progress) / totalObjects;  // 진행률 업데이트
                yield return null;
            }

            if (resourceRequest.asset != null)
            {
                GameObject obj2 = resourceRequest.asset as GameObject;

                // Instantiate를 통해 오브젝트 생성 후 삽입
                GameObject newObj = Instantiate(obj2, this.transform);
                pool.InsertMemory(newObj);
 
            }

        }

        yield return new WaitForSeconds(1f);
        isObjectLoadComplete = true;
    }

    private IEnumerator LoadAssetBundleFromLocal(string bundleFileName, Action<AssetBundle> onLoaded)
    {
        //Assets/StreamingAssets/NewAssetBundles
        string candidateStreaming = Path.Combine(Application.streamingAssetsPath, "NewAssetBundles", bundleFileName);
        string fullPath = candidateStreaming;

        // 파일에서 에셋번들 비동기 로드
        var request = AssetBundle.LoadFromFileAsync(fullPath);
        yield return request;

        var bundle = request.assetBundle;
        if (bundle == null)
        {
            Debug.LogError($"[ObjectManager] AssetBundle 로드 실패: {fullPath}");
            yield break;
        }

        onLoaded?.Invoke(bundle);
    }

    public void LoadObject(string path, int chapter)
    {
        GameObject[] obj = Resources.LoadAll<GameObject>(path);
        foreach (GameObject obj2 in obj)
        {
            //Instantiate를 통해서 InsertMemory내 삽입
            GameObject newObj = Instantiate(obj2, this.transform);
            string name = newObj.name.Substring(0, newObj.name.IndexOf("("));
            newObj.name = name; //(clone)을 찾아냄.
            //newObj의 clone을 제거 
            pool.InsertMemory(newObj);
        }
    }

    //한 챕터를 넘겼을 때 호출되는 함수, 즉 Phase Watching일 때 호출한다. 
    public void SettingChapter(int chapter)
    {
        List<GameObject> values = pool.GetValues();

        // 루프 밖에서 1회만 조회
        bool isCompleted = pc.GetPlayerInfo().isDay8SleepEventCompleted;

        foreach (GameObject value in values)
        {
            var baseObj = value.GetComponent<BaseObject>();
            bool active = baseObj != null && baseObj.IsCurrentChapter(chapter);

            // 빵 곰팡이 예외 처리
            var ch_bread = value.GetComponent<ChBreadObject>();
            if (ch_bread != null)
            {
                active = active && ch_bread.ShouldBeActive(isCompleted);
            }

            value.SetActive(active);
        }
    }

    public IWatchingInterface GetWatchingObject(EWatching type)
    {
        IWatchingInterface search = null;
        List<GameObject> values;
        if (watches.Count == 0)
        {
            //모든 Obj를 가져와서 검사해야한다.
            values = pool.GetValues();
        }
        else
        {
            values = watches;
        }

        // watches 리스트에 추가해야 할 오브젝트들을 임시 저장할 리스트
        List<GameObject> toAdd = new List<GameObject>();

        foreach (GameObject value in values)
        {
            IWatchingInterface watching = value.GetComponent<IWatchingInterface>();

            if (watching != null && watching.IsCurrentPattern(type))
            {
                if (watches.Count < 2 && !watches.Contains(value))
                {
                    toAdd.Add(value);
                }
                search = watching;
            }
        }
        // 루프가 끝난 후 리스트에 추가
        foreach (var item in toAdd)
        {
            Debug.Log(item.GetComponent<IWatchingInterface>().ToString());
            watches.Add(item);
        }
        return search;
    }

    public ISleepingInterface GetSleepingObject()
    {
        List<GameObject> values = pool.GetValues();

        foreach (GameObject value in values)
        {
            ISleepingInterface search = value.GetComponent<ISleepingInterface>();

            if (search != null)
            {
                return search;
            }
        
        }

        return null;
    }

    public void PlayThinking()
    {
        GameObject bookPile=pool.SearchMemory("phase_bookpile");

        if(bookPile)
        {
            bookPile.SetActive(false);
        }
    }

    public void ShowDiary(bool isActive)
    {

        GameObject diary = pool.SearchMemory("phase_diary");

        if (diary)
        {
            diary.SetActive(isActive);
        }
    }
    public void Translate(LANGUAGE language)
    {
        Debug.Log("게임 오브젝트 번역합니다.\n");
    }

    public void SkipSleeping(bool isActive)
    {
        pc.SetMoonRadioIdx(1); //한 phase가 지나면 1로 리셋
        skipSleep.SetActive(isActive);//이거 대신 video controller 사용 예정
    }

    public void RewardGlow(EReward eReward)
    {
        Transform rewardTransform = this.gameObject.transform.Find(eReward.ToString()+"(Clone)");

        if (rewardTransform != null)
        {
            GameObject reward = rewardTransform.gameObject;
            Debug.Log("밝게 빛날 물체 :" + reward.ToString());
            //StartCoroutine(Glow(reward));
        }
        else
        {
            Debug.Log("못찾음");
        }
    }
    IEnumerator Glow(GameObject reward)
    {
        var outline = reward.AddComponent<Outline>();
        outline.OutlineMode = Outline.Mode.OutlineAll;
        outline.OutlineColor = Color.yellow;
        outline.OutlineWidth = 10f;
        yield return new WaitForSeconds(5f);
        outline.OutlineMode = Outline.Mode.OutlineHidden;
    }
}
