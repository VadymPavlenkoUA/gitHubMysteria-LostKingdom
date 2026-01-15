using UnityEngine;

public enum Surface
{
    Default,
    Grass,
    Gravel,
    Stone,
    Wood,
    MetalV1,
    MetalV2
}

public class SurfaceType : MonoBehaviour
{
    public Surface surface = Surface.Default;
}
