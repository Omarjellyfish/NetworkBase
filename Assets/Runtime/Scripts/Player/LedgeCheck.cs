using UnityEngine;

namespace NetworkBaseRuntime
{
    public class LedgeCheck : MonoBehaviour
    {
        [SerializeField] private Transform _ledgeCheckPoint;
        [SerializeField] private float LedgeCheckDistance = 1f;
        [SerializeField] private Vector3 boxCastSize = new Vector3(0.5f, 0.5f, 0.5f);

        public bool IsLedge { get; private set; }
        void Check()
        {
            _ledgeCheckPoint.position = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z);
            // We will raycast to check for ledges
            // A box cast going horizontally, if it collides with something, we will box cast again but this time going upwards, if it collides with something, we know we can't ledge grab here

            RaycastHit hit;
             if (Physics.BoxCast(_ledgeCheckPoint.position, boxCastSize, transform.forward, out hit, Quaternion.identity, LedgeCheckDistance))
             {
                 // we hit something in front of us, now we check if we can grab the ledge
                 if (!Physics.BoxCast(_ledgeCheckPoint.position + transform.forward * LedgeCheckDistance, boxCastSize, Vector3.up, out hit, Quaternion.identity, LedgeCheckDistance))
                 {
                     IsLedge = true;
                 }
            }
        }
    }
}
