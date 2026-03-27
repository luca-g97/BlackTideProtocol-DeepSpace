using PrimeTween;
using UnityEngine;

public class PrimeTweenSettings : MonoBehaviour
{
    private void Awake()
    {
        PrimeTweenConfig.warnEndValueEqualsCurrent = false;
        PrimeTweenConfig.SetTweensCapacity(300);
    }
}
