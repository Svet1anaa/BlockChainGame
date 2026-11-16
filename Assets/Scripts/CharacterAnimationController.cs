
using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{
    [SerializeField] private Animator[] animators;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float rotationSpeed = 10f;

    private Vector3 lastPosition;

    private static readonly int IsWalking =
        Animator.StringToHash("IsWalking");

    void Start()
    {
        if (playerTransform == null)
            playerTransform = transform.parent;

        if (playerTransform != null)
            lastPosition = playerTransform.position;
    }

    void LateUpdate()
    {
        if (playerTransform == null) return;

        Vector3 movement = playerTransform.position - lastPosition;
        movement.y = 0f;

        bool walking = movement.sqrMagnitude > 0.000001f;

        foreach (Animator animator in animators)
        {
            if (animator != null)
                animator.SetBool(IsWalking, walking);
        }

        if (walking)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(movement.normalized);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        lastPosition = playerTransform.position;
    }
}
