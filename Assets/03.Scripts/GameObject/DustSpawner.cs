
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DustSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] dustPrefab;
    [SerializeField] Dictionary<GameObject, Vector3> spawnDust;
    [SerializeField] int maxSpawnCnt;
    [SerializeField] float width;
    [SerializeField] float height;
    [SerializeField] int spawnCount = 1;
    [SerializeField] int activeMaxDustCnt = 10;
    [SerializeField] float randomSec;
    [SerializeField] float velocity;

    const float duration = 1.0f;
    const float accelerationY = -600f;

    private bool isPaused = false;

    Vector2 initPos;
    int activeDustCnt;
    Queue<GameObject> order;
    const float spawnInterval = 2.0f;

    private bool _initialized = false;

    DustSpawner()
    {
        order = new Queue<GameObject>();
        spawnDust = new Dictionary<GameObject, Vector3>();
    }

    private void Awake()
    {
        InitOnce();
    }

    private void InitOnce()
    {
        if (_initialized) return;

        initPos = transform.position;
        activeDustCnt = 0;

        SpawnDust();          // ✅ 한 번만 생성
        _initialized = true;
    }

    private void OnEnable()
    {
        InitOnce();

        // 재진입 시 상태 리셋(기존 먼지들 숨김 + 카운트/큐 초기화)
        ResetRuntimeState();

        // anim_sleep일 때만 켜질 거라 기본은 Resume로 시작
        isPaused = false;
        StartSpawnLoop();
    }

    private void ResetRuntimeState()
    {
        CancelInvoke("DropRandom");
        StopAllCoroutines();

        order.Clear();
        activeDustCnt = 0;

        // 이미 만들어진 먼지 전부 비활성화
        foreach (var kv in spawnDust)
        {
            if (kv.Key != null) kv.Key.SetActive(false);
        }
    }

    void SpawnDust()
    {
        // (안전) 이미 뭔가 만들어져 있으면 중복 생성 방지
        if (spawnDust.Count > 0) return;

        for (int i = 0; i < maxSpawnCnt; i++)
        {
            float randomX = UnityEngine.Random.Range(initPos.x - width, initPos.x + width);
            Vector3 spawnPos = new Vector3(randomX, initPos.y + height, 0f);

            int idx = i / spawnCount;
            idx = Mathf.Clamp(idx, 0, dustPrefab.Length - 1);

            GameObject dustObj = Instantiate(dustPrefab[idx], spawnPos, Quaternion.identity, transform);
            dustObj.SetActive(false);
            spawnDust.Add(dustObj, spawnPos);
        }
    }

    void DropRandom()
    {
        if (activeDustCnt >= activeMaxDustCnt)
        {
            DeactiveOldestDust();
            return;
        }
        ActiveRandomDust();
    }

    void DeactiveOldestDust()
    {
        while (activeDustCnt >= activeMaxDustCnt && order.Count > 0)
        {
            GameObject dust = order.Dequeue();
            if (dust != null && dust.activeSelf) Deactive(dust);
        }
    }

    public void Deactive(GameObject gameObject)
    {
        if (gameObject == null) return;
        gameObject.SetActive(false);
        activeDustCnt = Mathf.Max(0, activeDustCnt - 1);
    }

    void ActiveRandomDust()
    {
        List<GameObject> dusts = GetDeactiveDusts();
        if (dusts.Count <= 0) return;

        // 기존 코드 RandomRange(1, count) 는 0번을 영원히 못 뽑고,
        // count-1 인덱스도 상황에 따라 누락될 수 있음. 0..count-1 로 고정.
        int randomIndex = UnityEngine.Random.Range(0, dusts.Count);

        GameObject randomDust = dusts[randomIndex];
        randomDust.transform.position = spawnDust[randomDust];
        randomDust.SetActive(true);
        order.Enqueue(randomDust);

        StartCoroutine(MoveDust(randomDust.transform, spawnDust[randomDust]));
        activeDustCnt++;
    }

    List<GameObject> GetDeactiveDusts()
    {
        List<GameObject> list = new List<GameObject>();
        foreach (var dust in spawnDust)
        {
            if (dust.Key != null && !dust.Key.activeSelf) list.Add(dust.Key);
        }
        return list;
    }

    IEnumerator MoveDust(Transform dust, Vector3 position)
    {
        float elapsedTime = 0f;
        Vector3 initPosLocal = position;
        int direction = UnityEngine.Random.Range(0, 2) * 2 - 1;

        while (elapsedTime < duration)
        {
            float displacementX = velocity / 2 * elapsedTime * direction;
            float displacementY = 0.8f * velocity * elapsedTime * elapsedTime;

            if (dust != null)
                dust.position = initPosLocal + new Vector3(displacementX, -displacementY, 0);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private void OnDisable()
    {
        CancelInvoke("DropRandom");
        StopAllCoroutines();
        // 여기서는 Destroy 안 함 (풀 유지)
        // 다음 OnEnable에서 ResetRuntimeState가 정리
    }

    public void PauseSpawner()
    {
        if (isPaused) return;
        isPaused = true;
        CancelInvoke("DropRandom");
    }

    public void ResumeSpawner()
    {
        if (!isPaused) return;
        isPaused = false;
        StartSpawnLoop();
    }

    void StartSpawnLoop()
    {
        if (isPaused) return;
        if (spawnDust.Count == 0) return;

        if (!IsInvoking("DropRandom"))
            InvokeRepeating("DropRandom", 0.5f, spawnInterval);
    }

    // 외부에서 완전 종료
    public void PauseAndClear()
    {
        isPaused = true;
        CancelInvoke("DropRandom");
        StopAllCoroutines();

        foreach (var kv in spawnDust)
        {
            if (kv.Key != null) kv.Key.SetActive(false);
        }
        order.Clear();
        activeDustCnt = 0;

        gameObject.SetActive(false);
    }
}
