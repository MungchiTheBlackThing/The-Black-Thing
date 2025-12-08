
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DustSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject[] dustPrefab;

    [SerializeField]
    Dictionary<GameObject, Vector3> spawnDust;

    [SerializeField]
    int maxSpawnCnt;

    [SerializeField]
    [Tooltip("뭉치를 기준으로 좌측 우측으로 너비만큼 이동")]
    float width;

    [SerializeField]
    [Tooltip("뭉치를 기준으로 상단만큼 이동")]
    float height;

    [SerializeField]
    [Tooltip("실제 잠먼지 생성 개수")]
    int spawnCount = 1;

    [SerializeField]
    [Tooltip("실제 활성화된 잠먼지의 최대 개수")]
    int activeMaxDustCnt = 10;

    [SerializeField]
    [Tooltip("활성화하기 위한 시간 초")]
    float randomSec;

    [SerializeField]
    [Tooltip("등속도/등가속도에 사용할 일정한 운동의 속도값")]
    float velocity;

    const float duration = 1.0f;

    const float accelerationY = -600f;

    private bool isPaused = false;


    Vector2 initPos;

    int activeDustCnt;

    Queue<GameObject> order;
    const float spawnInterval = 2.0f;
    DustSpawner() 
    {
        order = new Queue<GameObject>();
        spawnDust = new Dictionary<GameObject, Vector3>();
    }

    private void OnEnable()
    {
        initPos = transform.position;
        Debug.Log(initPos);

        activeDustCnt = 0;

        SpawnDust();

        if (spawnDust.Count != 0)
            InvokeRepeating("DropRandom", 0.5f, spawnInterval);
    }

    /*
     잠먼지 생성 과정
     */
    void SpawnDust()
    {
        for (int i = 0; i < maxSpawnCnt; i++)
        {
            float randomX = Random.Range(initPos.x - width, initPos.x + width); //가운데 뭉치를 기준으로 왼쪽, 오른쪽을 의미
            Vector3 spawnPos = new Vector3(randomX, initPos.y + height, 0f); //2D이기 때문에 Z값은 없음.
            int idx = i / spawnCount;
            GameObject dustObj = Instantiate(dustPrefab[idx], spawnPos, Quaternion.identity, transform);
            dustObj.SetActive(false);
            spawnDust.Add(dustObj, spawnPos); //초기화 위치도 저장
        }
    }

    /*
     * 잠먼지가 랜덤으로 활성화, 비활성화 한다.
     * 대략 n초에 한번씩
     */
    void DropRandom()
    {

        //실제 활성화된 개수가 최대 값을 넘으면, 비활성화 시킨다.
        if (activeDustCnt >= activeMaxDustCnt)
        {
            DeactiveOldestDust();
            return;
        }
        //비활성화된 오브젝트 중 랜덤으로 활성화 시킨다.
        ActiveRandomDust();
    }


    /*
     *  가장 오래된 활성화 오브젝트를 비활성화로 만든다.
     */
    void DeactiveOldestDust()
    {
        while(activeDustCnt >= activeMaxDustCnt)
        {
            GameObject dust = order.Dequeue(); //눌러서 이미 활성화가 꺼져있을 수도 있음.
            if (dust.activeSelf)
                Deactive(dust);
        }
    }

    public void Deactive(GameObject gameObject)
    {
        gameObject.SetActive(false);
        activeDustCnt--;
    }

    /*
     * 랜덤으로 오브젝트를 활성화 시킨다. 
     */

    void ActiveRandomDust()
    {
        List<GameObject> dusts = GetDeactiveDusts();

        if(dusts.Count <= 0)
        {
            Debug.LogError("Current Deactive Dust is Null");
            return;
        }

        int randomIndex = Random.RandomRange(1, dusts.Count);

        GameObject randomDust = dusts[randomIndex];
        //활성화 시키기 전에 해당 값의 위치로 세팅
        randomDust.transform.position = spawnDust[randomDust];
        randomDust.SetActive(true);
        order.Enqueue(randomDust);
        //움직인다.
        StartCoroutine(MoveDust(randomDust.transform, spawnDust[randomDust]));
        activeDustCnt++;
    }

    /*
     * 비활성화인 dust 오브젝트를 모두 가져온다.
     */
    
    List<GameObject> GetDeactiveDusts()
    {
        List<GameObject> list = new List<GameObject>();

        foreach(var dust in spawnDust)
        {
            if(dust.Key.activeSelf == false)
            {
                list.Add(dust.Key);
            }
        }

        return list;
    }

    /*
     * 등속도 운동을 적용해서 자연스러운 떨어짐을 구현
     */

    IEnumerator MoveDust(Transform dust, Vector3 position)
    {
        float elapsedTime = 0f;
        Vector3 initPos = position;

        int direction = Random.Range(0, 2) * 2 - 1; //왼쪽, 오른쪽을 방향 설정

        while(elapsedTime < duration)
        {
            float displacementX = velocity/2 * elapsedTime * direction; //속도 * 시간 * 방향 , 수평 방향은 등속도 운동을 진행
            float displacementY = 0.8f * velocity * elapsedTime * elapsedTime; //수직방향으로, 일정 가속도에 의해 등가속도 운동을 진행

            dust.position = initPos + new Vector3(displacementX, -displacementY, 0);

            elapsedTime += Time.deltaTime; //진행 시간

            yield return null;
        }
    }

    private void OnDisable()
    {
        spawnDust.Clear();
        order.Clear();

        //코루틴을 종료한다.
        StopAllCoroutines();
        CancelInvoke("DropRandom");
    }

    public void PauseSpawner()
    {
        if (isPaused)
            return;

        isPaused = true;
        CancelInvoke("DropRandom");
    }

    public void ResumeSpawner()
    {
        if (!isPaused)
            return;

        isPaused = false;
        StartSpawnLoop();
    }

    void StartSpawnLoop()
    {
        if (isPaused)
            return;

        if (!IsInvoking("DropRandom"))
            InvokeRepeating("DropRandom", 0.5f, spawnInterval);
    }
}
