using UnityEngine;

public interface ITargetable
{
    Transform GetTransform();
    Vector3 GetVelocity();
    // We can add priority weights here later for decoys
}