using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    public Collider[] colliders;

    void OnTriggerEnter(Collider other)
    {
        CharacterNavigationController npc = other.GetComponent<CharacterNavigationController>();

        if (npc != null)
        {
            npc.animator.enabled = false;
            npc.isRagdoll = true;   //should pass variable based on hit velocity

            Collider[] hitColliders = Physics.OverlapSphere(other.transform.position, 0.5f, 18, QueryTriggerInteraction.Ignore);

            foreach (Collider c in hitColliders)
            {
                c.GetComponent<Rigidbody>().AddExplosionForce(520f, transform.position - new Vector3(0f, 0.15f, 0f), 1f);
            }
        }
    }
}