using UnityEngine;

public class DrillContact : MonoBehaviour
{
    [SerializeField] private Collider drillCollider;

    private Mineral currentMineral;

    public Mineral Mineral
    {
        get { return currentMineral; }
    }

    private void OnCollisionStay(Collision collision)
    {
        Collider contactedCollider = collision.collider;
        Mineral foundMineral = contactedCollider.GetComponentInParent<Mineral>();

        if (foundMineral == null)
            return;

        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint contact = collision.GetContact(i);

            if (contact.thisCollider == drillCollider)
            {
                currentMineral = foundMineral;
                return;
            }
        }

        if (currentMineral == foundMineral)
            currentMineral = null;
    }

    private void OnCollisionExit(Collision collision)
    {
        Collider contactedCollider = collision.collider;
        Mineral foundMineral = contactedCollider.GetComponentInParent<Mineral>();

        if (currentMineral == foundMineral)
            currentMineral = null;
    }

    private void OnDisable()
    {
        currentMineral = null;
    }
}