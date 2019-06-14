public enum BezierControlPointMode
{
    Free,
    Aligned,
    Mirrored
}

public enum Directions
{
    North,
    NorthWest,
    West,
    SouthWest,
    South,
    SouthEast,
    East,
    NorthEast
}

public enum TrafficSize
{
    High,
    MidHigh,
    Average,
    BelowAverage,
    Low
}

public enum DriverYield
{
    Normal,
    RightOfWay,
    GiveWay
}

public enum IntersectionDirection
{
    Straight,
    Left,
    Right
}

public enum SpeedLimits
{
    KMH_20,
    KMH_30,
    KMH_40,
    KMH_50,
    KMH_60,
    KMH_70,
    KMH_80,
    KMH_90,
    KMH_100,
    KMH_120
}

public enum TagColorScheme
{
    /* Each lane (left / right 1-3) has its own color.
     * Bus lanes are marked yellow
     * Connect points are larger squares, other nodes are smaller rounds
     * Unconnected end nodes are larger RED squares
     */
    ByLaneNumber
}

public enum IconColor
{
    Gray,
    Blue,
    Jade,
    Green,
    Yellow,
    Orange,
    Red,
    Purple
}

public enum NodeInOut
{
    NotUsed,
    InNode,
    OutNode
}