using System;
using Xunit;

namespace PrayTimes.Test
{
    public class PrayTimesTests
    {
        [Fact]
        public void TestTimeForISNA()
        {
            PrayTimesCalculator calc = new PrayTimesCalculator(47.660918, -122.136371);
            calc.CalculationMethod = CalculationMethods.ISNA;
            calc.AsrJuristicMethod = AsrJuristicMethods.Shafii;
            var times = calc.GetPrayerTimes(new DateTime(2015, 8, 3), -7);

            Assert.Equal(new DateTime(2015, 8, 3), times.Date);
            Assert.Equal(new TimeSpan(4, 1, 0), times.Fajr);
            Assert.Equal(new TimeSpan(5, 48, 0), times.Sunrise);
            Assert.Equal(new TimeSpan(13, 15, 0), times.Dhuhr);
            Assert.Equal(new TimeSpan(17, 18, 0), times.Asr);
            Assert.Equal(new TimeSpan(20, 40, 0), times.Maghrib);
            Assert.Equal(new TimeSpan(20, 40, 0), times.Sunset);
            Assert.Equal(new TimeSpan(22, 28, 0), times.Isha);
        }

        [Fact]
        public void TestTimeForKemenag()
        {
            PrayTimesCalculator calc = new PrayTimesCalculator(47.660918, -122.136371);
            calc.CalculationMethod = CalculationMethods.ISNA;
            calc.AsrJuristicMethod = AsrJuristicMethods.Shafii;
            var times = calc.GetPrayerTimes(new DateTime(2015, 8, 3, 0, 0, 0, DateTimeKind.Local));

            Assert.Equal(new DateTime(2015, 8, 3, 0, 0, 0, DateTimeKind.Local), times.Date);
            Assert.Equal(new TimeSpan(4, 1, 0), times.Fajr);
            Assert.Equal(new TimeSpan(5, 48, 0), times.Sunrise);
            Assert.Equal(new TimeSpan(13, 15, 0), times.Dhuhr);
            Assert.Equal(new TimeSpan(17, 18, 0), times.Asr);
            Assert.Equal(new TimeSpan(20, 40, 0), times.Maghrib);
            Assert.Equal(new TimeSpan(20, 40, 0), times.Sunset);
            Assert.Equal(new TimeSpan(22, 28, 0), times.Isha);
        }
    }
}
