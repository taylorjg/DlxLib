using System;

namespace DlxLibDemo3.Model
{
    public enum Orientation
    {
        North,
        South,
        East,
        West
    }

    public static class OrientationExtensions
    {
        public static Orientation NextOrientation(this Orientation orientation)
        {
            switch (orientation)
            {
                case Orientation.North:
                    return Orientation.East;

                case Orientation.South:
                    return Orientation.West;

                case Orientation.East:
                    return Orientation.South;

                case Orientation.West:
                    return Orientation.North;

                default:
                    throw new ArgumentOutOfRangeException("orientation");
            }
        }
    }
}
