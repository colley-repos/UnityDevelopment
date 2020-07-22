using UnityEngine;

public class CharacterNavigationController : MonoBehaviour
{
    public float movementSpeed;
    public float rotationSpeed = 120f;
    public float stopDistance = 0.2f;
    Vector3 lastPosition;
    Vector3 velocity;
    public Vector3 destination;
    public bool reachedDestination = false;
    public Animator animator;
    [HideInInspector] public bool isRagdoll;
    private FloatTimer knockDownTimer = new FloatTimer(3f);

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (transform.position != destination)
        {
            Vector3 destinationDirection = destination - transform.position;
            destinationDirection.y = 0;

            float destinationDistance = destinationDirection.magnitude;

            movementSpeed = Random.Range(0.9f, 1f);

            if (destinationDistance >= stopDistance)
            {
                reachedDestination = false;
                Quaternion targetRotation = Quaternion.LookRotation(destinationDirection);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                transform.Translate(Vector3.forward * movementSpeed * Time.deltaTime);

                velocity = (transform.position - lastPosition) / Time.deltaTime;
                velocity.y = 0;
                var velocityMagnitude = velocity.magnitude;
                velocity = velocity.normalized;
                var fwdDotProduct = Vector3.Dot(transform.forward, velocity);
                var rightDotProduct = Vector3.Dot(transform.right, velocity);


                animator.SetFloat("MovementX", rightDotProduct);
                animator.SetFloat("MovementSpeed", -fwdDotProduct);

            }
            else
            {
                reachedDestination = true;
            }


        }
        else
        {
            reachedDestination = true;

        }

        if(isRagdoll)
        {
            knockDownTimer.Tick();
            Vector3 pos = this.transform.position;      //needs work, animator overrides character position after ragdoll

            if (knockDownTimer.ready)
            {
                isRagdoll = false;
                animator.enabled = true;

                transform.position = pos;
                knockDownTimer.Reset();
            }
        }
    }

    public void SetDestination(Vector3 destination)
    {
        this.destination = destination;
        reachedDestination = false;
    }
}