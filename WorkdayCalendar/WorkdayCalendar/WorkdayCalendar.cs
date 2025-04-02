namespace WorkdayNet
{
    public class WorkdayCalendar : IWorkdayCalendar
    {
        private static readonly TimeOnly DefaultWorkDayStartTime = new(9, 0);
        private static readonly TimeOnly DefaultWorkDayFinishTime = new(17, 0);

        private readonly HashSet<DateOnly> _singleHolidays = new();
        private readonly HashSet<(int Month, int Day)> _recurrentHolidays = new();
        private WorkingDaySchedule _workingDaySchedule;

        public WorkdayCalendar()
        {
            _workingDaySchedule = new WorkingDaySchedule(DefaultWorkDayStartTime, DefaultWorkDayFinishTime);
        }

        public DateTime GetWorkdayIncrement(DateTime startDate, decimal incrementInWorkdays)
        {
            if (incrementInWorkdays == 0)
            {
                return startDate;
            }

            decimal remainingDays = Math.Abs(incrementInWorkdays);
            int direction = Math.Sign(incrementInWorkdays);
            var currentDate = NormalizeDate(startDate, direction);

            while (remainingDays > 1)
            {
                remainingDays -= 1;
                currentDate = FindNextWorkingDay(currentDate, direction);
            }

            if (remainingDays > 0)
            {
                var offset = TimeSpan.FromHours((double)remainingDays * _workingDaySchedule.Duration.TotalHours);

                bool offsetOutOfWorkingHours = direction > 0
                    ? currentDate.TimeOfDay + offset > _workingDaySchedule.StopTimeSpan
                    : currentDate.TimeOfDay - offset < _workingDaySchedule.StartTimeSpan;
                if (offsetOutOfWorkingHours)
                {
                    currentDate = FindNextWorkingDay(currentDate, direction);

                    if (direction > 0)
                    {
                        currentDate = currentDate.Date.Add(_workingDaySchedule.StartTimeSpan + (offset - (_workingDaySchedule.StopTimeSpan - currentDate.TimeOfDay)));
                    }
                    else
                    {
                        currentDate = currentDate.Date.Add(_workingDaySchedule.StopTimeSpan - (offset - (currentDate.TimeOfDay - _workingDaySchedule.StartTimeSpan)));
                    }
                }
                else
                {
                    currentDate = currentDate.Date.Add(currentDate.TimeOfDay + direction * offset);
                }
            }

            // Round the time to minute based on the direction
            double totalMinutes = currentDate.TimeOfDay.TotalMinutes;
            var roundedTime = TimeSpan.FromMinutes(direction > 0 ? Math.Floor(totalMinutes) : Math.Ceiling(totalMinutes));
            currentDate = currentDate.Date.Add(roundedTime);

            return currentDate;
        }

        public void SetHoliday(DateTime date)
        {
            var dateOnly = DateOnly.FromDateTime(date);
            _singleHolidays.Add(dateOnly);
        }

        public void SetRecurringHoliday(int month, int day)
        {
            if (month < 1 || month > 12)
            {
                throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");
            }

            const int yearForDaysInMonth = 2024; // Leap year to get the maximum number of days in a month
            int daysInMonth = DateTime.DaysInMonth(yearForDaysInMonth, month);
            if (day < 1 || day > daysInMonth)
            {
                throw new ArgumentOutOfRangeException(nameof(day), $"Day must be between 1 and {daysInMonth} for the given month.");
            }

            _recurrentHolidays.Add((month, day));
        }

        public void SetWorkdayStartAndStop(int startHours, int startMinutes, int stopHours, int stopMinutes)
        {
            var start = new TimeOnly(startHours, startMinutes);
            var stop = new TimeOnly(stopHours, stopMinutes);
            _workingDaySchedule = new WorkingDaySchedule(start, stop);
        }

        private bool IsWorkingDay(DateTime dateTime)
        {
            if (dateTime.DayOfWeek == DayOfWeek.Saturday || dateTime.DayOfWeek == DayOfWeek.Sunday)
            {
                return false;
            }

            if (_singleHolidays.Contains(DateOnly.FromDateTime(dateTime)))
            {
                return false;
            }

            if (_recurrentHolidays.Contains((dateTime.Month, dateTime.Day)))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Adjusts the date to the nearest valid working time based on the direction.
        /// </summary>
        private DateTime NormalizeDate(DateTime date, int direction)
        {
            DateTime normalizedDate = date;

            if (direction > 0)
            {
                if (TimeOnly.FromTimeSpan(normalizedDate.TimeOfDay) > _workingDaySchedule.StopTime)
                {
                    normalizedDate = normalizedDate.Date
                        .AddDays(1)
                        .Add(_workingDaySchedule.StartTimeSpan);
                }
                else if (TimeOnly.FromTimeSpan(normalizedDate.TimeOfDay) < _workingDaySchedule.StartTime)
                {
                    normalizedDate = normalizedDate.Date
                        .Add(_workingDaySchedule.StartTimeSpan);
                }
            }
            else
            {
                if (TimeOnly.FromTimeSpan(normalizedDate.TimeOfDay) > _workingDaySchedule.StopTime)
                {
                    normalizedDate = normalizedDate.Date
                        .Add(_workingDaySchedule.StopTimeSpan);
                }
                else if (TimeOnly.FromTimeSpan(normalizedDate.TimeOfDay) < _workingDaySchedule.StartTime)
                {
                    normalizedDate = normalizedDate.Date
                        .AddDays(-1)
                        .Add(_workingDaySchedule.StopTimeSpan);
                }
            }

            return normalizedDate;
        }

        /// <summary>
        /// Finds the next working day in the specified direction, skipping non-working days.
        /// </summary>
        private DateTime FindNextWorkingDay(DateTime date, int direction)
        {
            DateTime nextDate = date.AddDays(direction);
            while (!IsWorkingDay(nextDate))
            {
                nextDate = nextDate.AddDays(direction);
            }
            return nextDate;
        }
    }
}
