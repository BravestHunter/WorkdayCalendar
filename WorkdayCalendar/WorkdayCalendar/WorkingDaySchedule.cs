namespace WorkdayNet
{
    internal class WorkingDaySchedule
    {
        public TimeOnly StartTime { get; init; }
        public TimeOnly StopTime { get; init; }
        public TimeSpan Duration => StopTime - StartTime;

        public WorkingDaySchedule(TimeOnly start, TimeOnly stop)
        {
            if (start >= stop)
            {
                throw new ArgumentException("Start time should be less than stop time", nameof(start));
            }

            StartTime = start;
            StopTime = stop;
        }

        public bool IsWithinSchedule(TimeOnly time)
        {
            return time >= StartTime && time <= StopTime;
        }

        public bool IsWithinSchedule(DateTime dateTime)
        {
            return IsWithinSchedule(TimeOnly.FromDateTime(dateTime));
        }
    }
}
