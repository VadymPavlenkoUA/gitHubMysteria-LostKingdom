using UnityEngine;

public enum Surface
{
    Default,
    Grass,
    Gravel,
    Stone,
    Wood
}

public class SurfaceType : MonoBehaviour
{
    public Surface surface = Surface.Default;
}
