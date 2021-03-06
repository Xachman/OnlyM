﻿namespace OnlyM.MediaElementAdaption
{
    using System;

    public class PositionChangedEventArgs : EventArgs
    {
        public TimeSpan Position { get; }
        
        public TimeSpan OldPosition { get; }

        public PositionChangedEventArgs(TimeSpan oldPosition, TimeSpan position)
        {
            OldPosition = oldPosition;
            Position = position;
        }
    }
}
