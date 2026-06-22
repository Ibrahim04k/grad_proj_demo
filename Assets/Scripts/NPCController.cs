using UnityEngine;

[RequireComponent(typeof(Animator))]
public class NPCController : MonoBehaviour
{
    private Animator animator;
    private readonly int isWalkingHash = Animator.StringToHash("IsWalking");

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // استدعِ هذه الدالة لبدء المشي
    public void StartWalking()
    {
        animator.SetBool(isWalkingHash, true);
    }

    // استدعِ هذه الدالة للوقوف (العودة لـ Idle)
    public void StopWalking()
    {
        animator.SetBool(isWalkingHash, false);
    }
}