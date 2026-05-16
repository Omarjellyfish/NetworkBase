using UnityEngine;

namespace NetworkBaseRuntime
{
    public class TestPlayer : MonoBehaviour,ITargetable
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
        public Transform GetTransform()
        {
            return transform;
        }
        public Vector3 GetVelocity()
        {
            return new Vector3(Random.Range(-10f, 15f), Random.Range(-10f, 15f), Random.Range(-15f, 15f));
        }
    }
}
