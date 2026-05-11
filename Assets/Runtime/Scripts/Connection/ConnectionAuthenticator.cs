using System;
using UnityEngine;

namespace NetworkBaseRuntime
{
    public class ConnectionAuthenticator : MonoBehaviour
    {
        public static event Action<ulong> onClientSuccessfullyConnected;

    }
}
