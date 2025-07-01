using UnityEngine;

public class FootprintManager : MonoBehaviour
{
    [Header("Settings")]
    public GameObject catPrefab;
    public Camera arCamera;
    public float followDistance = 1.5f;

    private GameObject spawnedCat;

    void Start()
    {
        if (arCamera == null)
            arCamera = Camera.main;
    }

    public void OnSpawnCatButton()
    {
        if (spawnedCat != null) return;  // 중복 생성 방지

        // 고양이 인스턴스 생성
        spawnedCat = Instantiate(catPrefab);
        
        // 카메라 추종 스크립트 붙이고 파라미터 세팅
        var follower = spawnedCat.AddComponent<CatFollower>();
        spawnedCat.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        follower.arCamera = arCamera;
        follower.distance = followDistance;
    }
}
