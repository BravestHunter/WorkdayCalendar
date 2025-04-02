namespace WorkdayNet.Tests
{
    public class WorkdayCalendarTest
    {
        public static IEnumerable<object[]> Data =>
            new List<object[]>
            {
                new object[] { new DateTime(2004, 5, 24, 18, 5, 0), -5.5m, new DateTime(2004, 5, 14, 12, 0, 0) },
                new object[] { new DateTime(2004, 5, 24, 19, 3, 0), 44.723656, new DateTime(2004, 7, 27, 13, 47, 0) },
                new object[] { new DateTime(2004, 5, 24, 18, 3, 0), -6.7470217, new DateTime(2004, 5, 13, 10, 2, 0) },
                new object[] { new DateTime(2004, 5, 24, 8, 3, 0), 12.782709, new DateTime(2004, 6, 10, 14, 18, 0) },
                new object[] { new DateTime(2004, 5, 24, 7, 3, 0), 8.276628, new DateTime(2004, 6, 4, 10, 12, 0) },

                new object[] { new DateTime(2004, 5, 24, 7, 0, 0), 0, new DateTime(2004, 5, 24, 7, 0, 0) },
                new object[] { new DateTime(2004, 5, 24, 8, 0, 0), 0.5, new DateTime(2004, 5, 24, 12, 0, 0) },
                new object[] { new DateTime(2004, 5, 24, 7, 0, 0), 0.5, new DateTime(2004, 5, 24, 12, 0, 0) },
            };

        private IWorkdayCalendar Calendar
        {
            get
            {
                IWorkdayCalendar calendar = new WorkdayCalendar();
                calendar.SetWorkdayStartAndStop(8, 0, 16, 0);
                calendar.SetRecurringHoliday(5, 17);
                calendar.SetHoliday(new DateTime(2004, 5, 27));

                return calendar;
            }
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void GetWorkdayIncrement_ReturnsExpectedResult(DateTime start, decimal increment, DateTime expected)
        {
            var calendar = Calendar;

            var incrementedDate = calendar.GetWorkdayIncrement(start, increment);

            Assert.Equal(expected, incrementedDate);
        }
    }
}
