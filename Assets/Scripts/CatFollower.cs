using UnityEngine;

public class CatFollower : MonoBehaviour
{
    public Camera arCamera;
    public float distance = 1.5f;
    public Animator animator;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 항상 카메라 앞에 위치만 갱신
        Vector3 frontPos = arCamera.transform.position + arCamera.transform.forward * distance;
        transform.position = new Vector3(frontPos.x, frontPos.y - 0.2f, frontPos.z);

        // 무한 걷기 모션만 반복
        animator.Play("walk");
    }
}