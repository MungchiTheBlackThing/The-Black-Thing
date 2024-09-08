
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
    [Tooltip("��ġ�� �������� ���� �������� �ʺ�ŭ �̵�")]
    float width;

    [SerializeField]
    [Tooltip("��ġ�� �������� ��ܸ�ŭ �̵�")]
    float height;

    [SerializeField]
    [Tooltip("���� ����� ���� ����")]
    int spawnCount = 1;

    [SerializeField]
    [Tooltip("���� Ȱ��ȭ�� ������� �ִ� ����")]
    int activeMaxDustCnt = 10;

    [SerializeField]
    [Tooltip("Ȱ��ȭ�ϱ� ���� �ð� ��")]
    float randomSec;

    [SerializeField]
    [Tooltip("��ӵ�/��ӵ��� ����� ������ ��� �ӵ���")]
    float velocity;

    const float duration = 1.0f;

    const float accelerationY = -600f;


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
     ����� ���� ����
     */
    void SpawnDust()
    {
        for (int i = 0; i < maxSpawnCnt; i++)
        {
            float randomX = Random.Range(initPos.x - width, initPos.x + width); //��� ��ġ�� �������� ����, �������� �ǹ�
            Vector3 spawnPos = new Vector3(randomX, initPos.y + height, 0f); //2D�̱� ������ Z���� ����.
            int idx = i / spawnCount;
            GameObject dustObj = Instantiate(dustPrefab[idx], spawnPos, Quaternion.identity, transform);
            dustObj.SetActive(false);
            spawnDust.Add(dustObj, spawnPos); //�ʱ�ȭ ��ġ�� ����
        }
    }

    /*
     * ������� �������� Ȱ��ȭ, ��Ȱ��ȭ �Ѵ�.
     * �뷫 n�ʿ� �ѹ���
     */
    void DropRandom()
    {

        //���� Ȱ��ȭ�� ������ �ִ� ���� ������, ��Ȱ��ȭ ��Ų��.
        if (activeDustCnt >= activeMaxDustCnt)
        {
            DeactiveOldestDust();
            return;
        }
        //��Ȱ��ȭ�� ������Ʈ �� �������� Ȱ��ȭ ��Ų��.
        ActiveRandomDust();
    }


    /*
     *  ���� ������ Ȱ��ȭ ������Ʈ�� ��Ȱ��ȭ�� �����.
     */
    void DeactiveOldestDust()
    {
        while(activeDustCnt >= activeMaxDustCnt)
        {
            GameObject dust = order.Dequeue(); //������ �̹� Ȱ��ȭ�� �������� ���� ����.
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
     * �������� ������Ʈ�� Ȱ��ȭ ��Ų��. 
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
        //Ȱ��ȭ ��Ű�� ���� �ش� ���� ��ġ�� ����
        randomDust.transform.position = spawnDust[randomDust];
        randomDust.SetActive(true);
        order.Enqueue(randomDust);
        //�����δ�.
        StartCoroutine(MoveDust(randomDust.transform, spawnDust[randomDust]));
        activeDustCnt++;
    }

    /*
     * ��Ȱ��ȭ�� dust ������Ʈ�� ��� �����´�.
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
     * ��ӵ� ��� �����ؼ� �ڿ������� �������� ����
     */

    IEnumerator MoveDust(Transform dust, Vector3 position)
    {
        float elapsedTime = 0f;
        Vector3 initPos = position;

        int direction = Random.Range(0, 2) * 2 - 1; //����, �������� ���� ����

        while(elapsedTime < duration)
        {
            float displacementX = velocity/2 * elapsedTime * direction; //�ӵ� * �ð� * ���� , ���� ������ ��ӵ� ��� ����
            float displacementY = 0.8f * velocity * elapsedTime * elapsedTime; //������������, ���� ���ӵ��� ���� ��ӵ� ��� ����

            dust.position = initPos + new Vector3(displacementX, -displacementY, 0);

            elapsedTime += Time.deltaTime; //���� �ð�

            yield return null;
        }
    }

    private void OnDisable()
    {
        spawnDust.Clear();
        order.Clear();

        //�ڷ�ƾ�� �����Ѵ�.
        StopAllCoroutines();
        CancelInvoke("DropRandom");
    }
}
