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
            decimal remainingDays = Math.Abs(incrementInWorkdays);
            var currentDate = startDate;
            int direction = Math.Sign(incrementInWorkdays);

            while (remainingDays > 0)
            {
                currentDate = currentDate.AddDays(direction);

                if (!IsWorkingDay(currentDate))
                {
                    continue;
                }

                remainingDays -= 1;
                if (remainingDays >= 1)
                {
                    continue;
                }

                var offset = TimeSpan.FromHours((double)remainingDays * _workingDaySchedule.Duration.TotalHours);

                if (direction > 0)
                {
                    if (TimeOnly.FromTimeSpan(startDate.TimeOfDay) > _workingDaySchedule.StopTime)
                    {
                        currentDate = currentDate.Date
                            .AddDays(1)
                            .Add(_workingDaySchedule.StartTime.ToTimeSpan());
                    }
                    else if (TimeOnly.FromTimeSpan(startDate.TimeOfDay) < _workingDaySchedule.StartTime)
                    {
                        currentDate = currentDate.Date
                            .Add(_workingDaySchedule.StartTime.ToTimeSpan());
                    }

                    if (currentDate.TimeOfDay + offset > _workingDaySchedule.StopTime.ToTimeSpan())
                    {
                        // Move to the next working day and apply the remaining time
                        currentDate = currentDate.Date.AddDays(1);
                        while (!IsWorkingDay(currentDate))
                        {
                            currentDate = currentDate.AddDays(1);
                        }
                        currentDate = currentDate.Date.Add(_workingDaySchedule.StartTime.ToTimeSpan() + (offset - (_workingDaySchedule.StopTime.ToTimeSpan() - currentDate.TimeOfDay)));
                    }
                    else
                    {
                        currentDate = currentDate.Date.Add(currentDate.TimeOfDay + offset);
                    }
                }
                else
                {
                    if (TimeOnly.FromTimeSpan(startDate.TimeOfDay) > _workingDaySchedule.StopTime)
                    {
                        currentDate = currentDate.Date
                            .Add(_workingDaySchedule.StopTime.ToTimeSpan());
                    }
                    else if (TimeOnly.FromTimeSpan(startDate.TimeOfDay) < _workingDaySchedule.StartTime)
                    {
                        currentDate = currentDate.Date
                            .AddDays(-1)
                            .Add(_workingDaySchedule.StopTime.ToTimeSpan());
                    }

                    if (currentDate.TimeOfDay - offset < _workingDaySchedule.StartTime.ToTimeSpan())
                    {
                        // Move to the previous working day and apply the remaining time
                        currentDate = currentDate.Date.AddDays(-1);
                        while (!IsWorkingDay(currentDate))
                        {
                            currentDate = currentDate.AddDays(-1);
                        }
                        currentDate = currentDate.Date.Add(_workingDaySchedule.StopTime.ToTimeSpan() - (offset - (currentDate.TimeOfDay - _workingDaySchedule.StartTime.ToTimeSpan())));
                    }
                    else
                    {
                        currentDate = currentDate.Date.Add(currentDate.TimeOfDay - offset);
                    }
                }

                remainingDays = 0;
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
    }
}
