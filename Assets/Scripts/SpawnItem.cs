using UnityEngine;

public class SpawnItem : MonoBehaviour
{
    [Header("설정")]
    public GameObject[] itemPrefabsList;    // 튀어나올 아이템 Prefab 목록
    public float spawnInterval = 10f;       // 몇 m마다 스폰할지
    public float spawnForceHeight = 5f;    // 위쪽 임펄스
    public float spawnForceRange = 2f;      // 측면 임펄스 범위
    
    private float nextSpawnDistance;
    private int currentItemIndex;
    
    private CalculateDistance calcDist;
    private CatFollower catFollower;

    void Start()
    {
        // CalculateDistance, CatFollower는 씬 내 아무 오브젝트에 붙여두고,
        // 여기서는 싱글턴 또는 Find로 가져오기
        calcDist     = FindObjectOfType<CalculateDistance>();
        catFollower  = FindObjectOfType<CatFollower>();

        nextSpawnDistance = spawnInterval;
        currentItemIndex  = 0;
    }

    void Update()
    {
        if (calcDist == null || catFollower == null) return;
        
        float moved = calcDist.movedDistance;
        // 목표 거리에 도달했을 때만 한 번 실행
        if (moved >= nextSpawnDistance)
        {
            SpawnAndLaunch(currentItemIndex);

            // 다음 목표 거리 갱신
            nextSpawnDistance += spawnInterval;
            // 순환 인덱스
            currentItemIndex = (currentItemIndex + 1) % itemPrefabsList.Length;
        }
    }

    private void SpawnAndLaunch(int index)
    {
        // 1) 고양이 앞 위치 계산
        Vector3 spawnPos = catFollower.arCamera.transform.position
                         + catFollower.arCamera.transform.forward * catFollower.distance;

        // 2) 프리팹 인스턴스 생성
        GameObject go = Instantiate(itemPrefabsList[index], spawnPos, Quaternion.identity);
        Rigidbody rb  = go.GetComponent<Rigidbody>();
        if (rb == null) return;

        // 3) 기존 속도 초기화
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // 4) 임의 방향의 힘 주기
        Vector3 impulse = new Vector3(
            Random.Range(-spawnForceRange, spawnForceRange),
            spawnForceHeight,
            Random.Range(-spawnForceRange, spawnForceRange)
        );
        rb.AddForce(impulse, ForceMode.Impulse);

        // 5) 회전 토크
        rb.AddTorque(new Vector3(
            Random.Range(-spawnForceRange, spawnForceRange),
            Random.Range(-spawnForceRange, spawnForceRange),
            Random.Range(-spawnForceRange, spawnForceRange)
        ), ForceMode.Impulse);
    }
}
