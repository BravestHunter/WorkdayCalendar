namespace WorkdayNet
{
    internal class WorkingDaySchedule
    {
        public TimeOnly StartTime { get; init; }
        public TimeOnly StopTime { get; init; }
        public TimeSpan Duration => StopTime - StartTime;
        public TimeSpan StartTimeSpan => StartTime.ToTimeSpan();
        public TimeSpan StopTimeSpan => StopTime.ToTimeSpan();

        public WorkingDaySchedule(TimeOnly start, TimeOnly stop)
        {
            if (start >= stop)
            {
                throw new ArgumentException("Start time should be less than stop time", nameof(start));
            }

            StartTime = start;
            StopTime = stop;
        }
    }
}
