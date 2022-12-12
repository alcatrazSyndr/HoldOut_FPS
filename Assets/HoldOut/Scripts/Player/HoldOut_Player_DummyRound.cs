using UnityEngine;

public class HoldOut_Player_DummyRound : MonoBehaviour
{

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("InvisibleWall"))
        {
            print("Touch wall");
            Destroy(gameObject);
        }
        else if (other.CompareTag("DamageColliderHead") || other.CompareTag("DamageColliderBody"))
        {
            print("Touch");
            Destroy(gameObject);
        }
    }
}
