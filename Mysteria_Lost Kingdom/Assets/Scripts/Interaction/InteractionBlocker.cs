using System.Collections.Generic;

public enum InteractionBlockReason
{
    Menu,
    Dialogue,
    Education,
    Cutscene,
    UsingItem,
    Trade
}

public static class InteractionBlocker
{
    private static HashSet<InteractionBlockReason> reasons = new();

    public static bool IsBlocked => reasons.Count > 0;

    public static void Block(InteractionBlockReason reason)
    {
        reasons.Add(reason);
    }

    public static void Unblock(InteractionBlockReason reason)
    {
        reasons.Remove(reason);
    }
}
