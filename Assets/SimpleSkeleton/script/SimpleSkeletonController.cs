using UnityEngine;
using UnityEngine.AI;

public class SimpleSkeletonController : MonoBehaviour
{
    private static readonly int MovingHash = Animator.StringToHash("Moving");
    private static readonly int SidefixHash = Animator.StringToHash("sidefix");
    private static readonly int RunningHash = Animator.StringToHash("running");
    private static readonly int PunchRHash = Animator.StringToHash("punch_R");
    private static readonly int PunchLHash = Animator.StringToHash("punch_L");
    private static readonly int KnockdownHash = Animator.StringToHash("knockdown");
    private static readonly int JumpHash = Animator.StringToHash("jump");
    private static readonly int LyingHash = Animator.StringToHash("lying");
    private static readonly int VerticalHash = Animator.StringToHash("Vertical");
    private static readonly int HorizontalHash = Animator.StringToHash("Horizontal");
    Animator animator;
    SkinnedMeshRenderer skinnedMesh;

    NavMeshAgent agent;
    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponentInParent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        bool isMoving = agent.velocity.magnitude > 0.01f;
        animator.SetBool(MovingHash, isMoving);


        if (Input.GetKey(KeyCode.L))
            animator.SetTrigger(LyingHash);
        else if (Input.GetKeyDown(KeyCode.Space))
            animator.SetTrigger(JumpHash);
        else if (Input.GetKeyDown(KeyCode.K))
            animator.SetTrigger(KnockdownHash);
        else if (Input.GetKeyDown(KeyCode.Mouse0))
            animator.SetTrigger(PunchLHash);
        else if (Input.GetKeyDown(KeyCode.Mouse1))
            animator.SetTrigger(PunchRHash);

        animator.SetFloat(VerticalHash, agent.velocity.magnitude);
        animator.SetFloat(HorizontalHash, agent.velocity.magnitude);

        if(Input.GetKey(KeyCode.LeftShift))
            animator.SetBool(RunningHash, true);
        else
            animator.SetBool(RunningHash, false);
        if (Input.GetKey(KeyCode.LeftControl))
            animator.SetBool(SidefixHash, true);
        else
            animator.SetBool(SidefixHash, false);
    }
}
