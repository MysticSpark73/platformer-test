using UnityEngine;

namespace PlayerControl
{
    public interface IPlayerObject
    {
        void KillZoneEntered();
        Transform GetTransform();
    }
}
