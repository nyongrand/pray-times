//--------------------- Copyright Block ----------------------
/*

PrayTimes.js: Prayer Times Calculator (ver 2.3)
Copyright (C) 2007-2011 PrayTimes.org

Ported to C# by: Nyong Grandong
Forked from PrayerTimes calculator by Jameel Haffejee 
Original JS Code By: Hamid Zarrabi-Zadeh

License: GNU LGPL v3.0

TERMS OF USE:
	Permission is granted to use this code, with or
	without modification, in any website or application
	provided that credit is given to the original work
	with a link back to PrayTimes.org.

This program is distributed in the hope that it will
be useful, but WITHOUT ANY WARRANTY.

PLEASE DO NOT REMOVE THIS COPYRIGHT BLOCK.

*/

using System;

namespace PrayTimes
{
    /// <summary>
    /// Prayer times calculator.
    /// </summary>
    public class PrayTimesCalculator
    {
        const int numIterations = 1;    // number of iterations needed to compute times, this should never be more than 1;
        const int dhuhrMinutes = 0;     // minutes after mid-day for Dhuhr

        private readonly double _latitude;      // latitude
        private readonly double _longitude;     // longitude
        private readonly double _elevation;     // elevation (sea level)

        private readonly Params _customMethods; // user specified custom method

        /// <summary>
        /// Initializes a new instance of PrayerTimesCalculator.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        public PrayTimesCalculator(double latitude, double longitude)
        {
            _latitude = latitude;
            _longitude = longitude;
            _elevation = 0;
            _customMethods = null;
        }

        /// <summary>
        /// Initializes a new instance of PrayerTimesCalculator.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="elevation"></param>
        public PrayTimesCalculator(double latitude, double longitude, double elevation)
        {
            _latitude = latitude;
            _longitude = longitude;
            _elevation = elevation;
            _customMethods = null;
        }

        /// <summary>
        /// Initializes a new instance of PrayerTimesCalculator.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="customParams"></param>
        public PrayTimesCalculator(double latitude, double longitude, Params customParams)
        {
            _latitude = latitude;
            _longitude = longitude;
            _elevation = 0;
            _customMethods = customParams;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="elevation"></param>
        /// <param name="customParams"></param>
        public PrayTimesCalculator(double latitude, double longitude, double elevation, Params customParams)
        {
            _latitude = latitude;
            _longitude = longitude;
            _elevation = elevation;
            _customMethods = customParams;
        }

        /// <summary>
        /// Gets or sets calculation method.
        /// </summary>
        public CalculationMethods CalculationMethod { get; set; }

        /// <summary>
        /// Gets Calculation Methods Params
        /// </summary>
        public Params MethodParams => _customMethods ?? Params.GetMethodsParams(CalculationMethod, _customMethods);

        /// <summary>
        /// Gets or sets juristic method for Asr.
        /// </summary>
        public AsrJuristicMethods AsrJuristicMethod { get; set; }

        /// <summary>
        /// Gets or sets adjustment method for higher latitudes.
        /// </summary>
        public HighLatitudeAdjustmentMethods HighLatitudeAdjustmentMethod { get; set; }

        /// <summary>
        /// Gets or sets manual minutes correction new double[] { 0, 0, 0, 0, 0, }
        /// </summary>
        public double[] Offset { get; set; } = new double[] { 0, 0, 0, 0, 0, };

        ///<summary>
        /// Returns the prayer times for a given date , the date format is specified as individual settings.
        /// </summary>
        /// <param name="date">Date time representing the date for which times should be calculated.</param>        
        /// <param name="timeZone">Time zone to use when calculating times. If omitted, time zone from date is used.</param>
        /// <returns>
        /// Times structure containing the Salaah times.
        /// </returns>
        public Times GetPrayerTimes(DateTimeOffset date, int? timeZone = null)
        {
            timeZone = EffectiveTimeZone(date, timeZone);
            var jDate = Julian(date.Date) - _longitude / (15 * 24);
            var times = ComputeTimes(jDate, timeZone.Value);
            times.Date = date;
            return times;
        }

        #region Calculation Functions

        /// <summary>
        /// compute mid-day time
        /// </summary>
        /// <param name="jDate"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        private double MidDay(double jDate, double time)
        {
            //midDay: function (time) {
            //	var eqt = this.sunPosition(jDate + time).equation;
            //	var noon = DMath.fixHour(12 - eqt);
            //	return noon;
            //}

            var eqt = this.SunPosition(jDate + time).Equation;
            var noon = this.FixHour(12 - eqt);
            return noon;
        }

        /// <summary>
        /// compute the time at which sun reaches a specific angle below horizon
        /// </summary>
        /// <param name="jDate"></param>
        /// <param name="angle"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        private double SunAngleTime(double jDate, double angle, double time)
        {
            //sunAngleTime: function (angle, time, direction) {
            //	var decl = this.sunPosition(jDate + time).declination;
            //	var noon = this.midDay(time);
            //	var t = 1 / 15 * DMath.arccos((-DMath.sin(angle) - DMath.sin(decl) * DMath.sin(lat)) /
            //		(DMath.cos(decl) * DMath.cos(lat)));
            //	return noon + (direction == 'ccw' ? -t : t);
            //}

            var decl = this.SunPosition(jDate + time).Declination;
            var noon = this.MidDay(jDate, time);
            var t = (1 / 15d) * this.DArcCos((-this.DSin(angle) - this.DSin(decl) * this.DSin(_latitude)) /
                (this.DCos(decl) * this.DCos(_latitude)));
            return noon + (angle > 90 ? -t : t);
        }

        /// <summary>
        /// compute asr time
        /// </summary>
        /// <param name="factor"></param>
        /// <param name="jDate"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        private double AsrTime(int factor, double jDate, double time)
        {
            //asrTime: function (factor, time) {
            //	var decl = this.sunPosition(jDate + time).declination;
            //	var angle = -DMath.arccot(factor + DMath.tan(Math.abs(lat - decl)));
            //	return this.sunAngleTime(angle, time);
            //}

            var decl = this.SunPosition(jDate + time).Declination;
            var angle = -this.DArcCot(factor + this.DTan(Math.Abs(_latitude - decl)));
            return this.SunAngleTime(jDate, angle, time);
        }

        /// <summary>
        /// compute declination angle of sun and equation of time
        /// Ref: http://aa.usno.navy.mil/faq/docs/SunApprox.php
        /// </summary>
        /// <param name="jd"></param>
        /// <returns></returns>
        private SunPos SunPosition(double jd)
        {
            //sunPosition: function (jd) {
            //	var D = jd - 2451545.0;
            //	var g = DMath.fixAngle(357.529 + 0.98560028 * D);
            //	var q = DMath.fixAngle(280.459 + 0.98564736 * D);
            //	var L = DMath.fixAngle(q + 1.915 * DMath.sin(g) + 0.020 * DMath.sin(2 * g));

            //	var R = 1.00014 - 0.01671 * DMath.cos(g) - 0.00014 * DMath.cos(2 * g);
            //	var e = 23.439 - 0.00000036 * D;

            //	var RA = DMath.arctan2(DMath.cos(e) * DMath.sin(L), DMath.cos(L)) / 15;
            //	var eqt = q / 15 - DMath.fixHour(RA);
            //	var decl = DMath.arcsin(DMath.sin(e) * DMath.sin(L));

            //	return { declination: decl, equation: eqt };
            //}

            var D = jd - 2451545.0;
            var g = this.FixAngle(357.529 + 0.98560028 * D);
            var q = this.FixAngle(280.459 + 0.98564736 * D);
            var L = this.FixAngle(q + 1.915 * this.DSin(g) + 0.020 * this.DSin(2 * g));

            var R = 1.00014 - 0.01671 * this.DCos(g) - 0.00014 * this.DCos(2 * g);
            var e = 23.439 - 0.00000036 * D;

            var RA = this.DArcTan2(this.DCos(e) * this.DSin(L), this.DCos(L)) / 15;
            var eqt = q / 15 - this.FixHour(RA);
            var decl = this.DArcSin(this.DSin(e) * this.DSin(L));

            return new SunPos { Declination = decl, Equation = eqt };
        }

        /// <summary>
        /// calculate julian date from a calendar date
        /// Ref: Astronomical Algorithms by Jean Meeus
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <returns></returns>
        private double Julian(int year, int month, int day)
        {
            //julian: function (year, month, day) {
            //	if (month <= 2) {
            //		year -= 1;
            //		month += 12;
            //	};
            //	var A = Math.floor(year / 100);
            //	var B = 2 - A + Math.floor(A / 4);

            //	var JD = Math.floor(365.25 * (year + 4716)) + Math.floor(30.6001 * (month + 1)) + day + B - 1524.5;
            //	return JD;
            //}

            if (month <= 2)
            {
                year -= 1;
                month += 12;
            }

            var A = Math.Floor((double)(year / 100));
            var B = 2 - A + Math.Floor(A / 4);

            var JD = Math.Floor(365.25 * (year + 4716)) + Math.Floor(30.6001 * (month + 1)) + day + B - 1524.5;

            return JD;
        }

        /// <summary>
        /// calculate julian date from a calendar date (use .NET ticks, shorter)
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private double Julian(DateTime date)
        {
            return (date.Date.Ticks / 864000000000) + 1721425.5;
        }

        #endregion

        #region Compute Prayer Times

        /// <summary>
        /// compute prayer times at given julian date
        /// </summary>
        /// <param name="jDate"></param>
        /// <param name="times"></param>
        /// <returns></returns>
        private double[] ComputePrayerTimes(double jDate, double[] times)
        {
            //computePrayerTimes: function (times) {
            //	times = this.dayPortion(times);
            //	var params = setting;

            //	var imsak = this.sunAngleTime(this.eval(params.imsak), times.imsak, 'ccw');
            //	var fajr = this.sunAngleTime(this.eval(params.fajr), times.fajr, 'ccw');
            //	var sunrise = this.sunAngleTime(this.riseSetAngle(), times.sunrise, 'ccw');
            //	var dhuhr = this.midDay(times.dhuhr);
            //	var asr = this.asrTime(this.asrFactor(params.asr), times.asr);
            //	var sunset = this.sunAngleTime(this.riseSetAngle(), times.sunset);;
            //	var maghrib = this.sunAngleTime(this.eval(params.maghrib), times.maghrib);
            //	var isha = this.sunAngleTime(this.eval(params.isha), times.isha);

            //	return {
            //		imsak: imsak, fajr: fajr, sunrise: sunrise, dhuhr: dhuhr,
            //		asr: asr, sunset: sunset, maghrib: maghrib, isha: isha
            //	};
            //}

            times = DayPortion(times);

            var fajr = SunAngleTime(jDate, 180 - MethodParams.FajrAngle, times[0]);
            var sunrise = SunAngleTime(jDate, 180 - 0.833, times[1]);
            var dhuhr = MidDay(jDate, times[2]);
            var asr = AsrTime(AsrFactor(AsrJuristicMethod), jDate, times[3]);
            var sunset = SunAngleTime(jDate, RiseSetAngle(), times[4]); ;
            var maghrib = SunAngleTime(jDate, MethodParams.MaghribParameter, times[5]);
            var isha = SunAngleTime(jDate, MethodParams.IshaParameter, times[6]);

            return new double[] { fajr, sunrise, dhuhr, asr, sunset, maghrib, isha };
        }

        /// <summary>
        /// compute prayer times at given julian date
        /// </summary>
        /// <param name="jDate"></param>
        /// <param name="timeZone"></param>
        /// <returns></returns>
        private Times ComputeTimes(double jDate, int timeZone)
        {
            //computeTimes: function () {
            //	// default times
            //	var times = {
            //		imsak: 5, fajr: 5, sunrise: 6, dhuhr: 12,
            //		asr: 13, sunset: 18, maghrib: 18, isha: 18
            //	};

            //	// main iterations
            //	for (var i = 1; i <= numIterations; i++)
            //		times = this.computePrayerTimes(times);

            //	times = this.adjustTimes(times);

            //	// add midnight time
            //	times.midnight = (setting.midnight == 'Jafari') ?
            //		times.sunset + this.timeDiff(times.sunset, times.fajr) / 2 :
            //		times.sunset + this.timeDiff(times.sunset, times.sunrise) / 2;

            //	times = this.tuneTimes(times);
            //	return this.modifyFormats(times);
            //}

            //default times
            double[] times = new double[] { 5, 6, 12, 13, 18, 18, 18 };

            // main iterations
            for (var i = 1; i <= numIterations; i++)
                times = ComputePrayerTimes(jDate, times);

            times = AdjustTimes(timeZone, times);
            return ModifyFormat(TuneTimes(times));
        }

        /// <summary>
        /// adjust times in a prayer time array
        /// </summary>
        /// <param name="timeZone"></param>
        /// <param name="times"></param>
        /// <returns></returns>
        private double[] AdjustTimes(int timeZone, double[] times)
        {
            //adjustTimes: function (times) {
            //	var params = setting;
            //	for (var i in times)
            //		times[i] += timeZone - lng / 15;

            //	if (params.highLats != 'None')
            //		times = this.adjustHighLats(times);

            //	if (this.isMin(params.imsak))
            //		times.imsak = times.fajr - this.eval(params.imsak) / 60;
            //	if (this.isMin(params.maghrib))
            //		times.maghrib = times.sunset + this.eval(params.maghrib) / 60;
            //	if (this.isMin(params.isha))
            //		times.isha = times.maghrib + this.eval(params.isha) / 60;
            //	times.dhuhr += this.eval(params.dhuhr) / 60;

            //	return times;
            //}

            for (var i = 0; i < times.Length; i++)
                times[i] += timeZone - _longitude / 15;

            if (HighLatitudeAdjustmentMethod != HighLatitudeAdjustmentMethods.None)
                times = AdjustHighLatTimes(times);

            // Maghrib
            if (MethodParams.MaghribSelector == Params.Kind.Minute)
                times[5] = times[4] + MethodParams.MaghribParameter / 60;

            // Isha
            if (MethodParams.IshaSelector == Params.Kind.Minute)
                times[6] = times[5] + MethodParams.IshaParameter / 60;

            // Dhuhr
            times[2] += dhuhrMinutes / 60;

            return times;
        }

        /// <summary>
        /// get asr shadow factor
        /// </summary>
        /// <returns></returns>
        private int AsrFactor(AsrJuristicMethods asrJuristicMethod)
        {
            //asrFactor: function (asrParam) {
            //	var factor = { Standard: 1, Hanafi: 2 }[asrParam];
            //	return factor || this.eval(asrParam);
            //}

            // Shafii: factor=1, Hanafi: factor=2
            switch (asrJuristicMethod)
            {
                case AsrJuristicMethods.Shafii:
                    return 1;

                case AsrJuristicMethods.Hanafi:
                    return 2;

                default:
                    throw new ArgumentOutOfRangeException(nameof(AsrJuristicMethod));
            }
        }

        /// <summary>
        /// return sun angle for sunset/sunrise
        /// </summary>
        /// <returns></returns>
        private double RiseSetAngle()
        {
            //riseSetAngle: function () {
            //	//var earthRad = 6371009; // in meters
            //	//var angle = DMath.arccos(earthRad/(earthRad+ elv));
            //	var angle = 0.0347 * Math.sqrt(elv); // an approximation
            //	return 0.833 + angle;
            //}

            //var earthRad = 6371009; // in meters
            //var angle = this.darccos(earthRad/(earthRad + _elv));
            var angle = 0.0347 * Math.Sqrt(_elevation); // an approximation
            return 0.833 + angle;
        }

        /// <summary>
        /// apply offsets to the times
        /// </summary>
        /// <param name="times"></param>
        /// <returns></returns>
        private double[] TuneTimes(double[] times)
        {
            //tuneTimes: function (times) {
            //	for (var i in times)
            //		times[i] += offset[i] / 60;
            //	return times;
            //}
            var offset = new double[]
            {
                Offset.Length > 0 ? Offset[0] : 0,
                0,
                Offset.Length > 1 ? Offset[1] : 0,
                Offset.Length > 2 ? Offset[2] : 0,
                0,
                Offset.Length > 3 ? Offset[3] : 0,
                Offset.Length > 4 ? Offset[4] : 0,
            };

            for (int i = 0; i < times.Length; i++)
            {
                times[i] += offset[i] / 60;
            }
            return times;
        }

        /// <summary>
        /// convert times array to given time format
        /// </summary>
        /// <param name="times"></param>
        /// <returns></returns>
        private Times ModifyFormat(double[] times)
        {
            //modifyFormats: function (times) {
            //	for (var i in times)
            //		times[i] = this.getFormattedTime(times[i], timeFormat);
            //	return times;
            //}

            return new Times
            {
                Fajr = FloatToTimeSpan(times[0]),
                Sunrise = FloatToTimeSpan(times[1]),
                Dhuhr = FloatToTimeSpan(times[2]),
                Asr = FloatToTimeSpan(times[3]),
                Sunset = FloatToTimeSpan(times[4]),
                Maghrib = FloatToTimeSpan(times[5]),
                Isha = FloatToTimeSpan(times[6])
            };
        }

        /// <summary>
        /// adjust Fajr, Isha and Maghrib for locations in higher latitudes
        /// </summary>
        /// <param name="times"></param>
        /// <returns></returns>
        private double[] AdjustHighLatTimes(double[] times)
        {
            //// adjust times for locations in higher latitudes
            //adjustHighLats: function (times) {
            //	var params = setting;
            //	var nightTime = this.timeDiff(times.sunset, times.sunrise);

            //	times.imsak = this.adjustHLTime(times.imsak, times.sunrise, this.eval(params.imsak), nightTime, 'ccw');
            //	times.fajr = this.adjustHLTime(times.fajr, times.sunrise, this.eval(params.fajr), nightTime, 'ccw');
            //	times.isha = this.adjustHLTime(times.isha, times.sunset, this.eval(params.isha), nightTime);
            //	times.maghrib = this.adjustHLTime(times.maghrib, times.sunset, this.eval(params.maghrib), nightTime);

            //	return times;
            //}

            //// adjust a time for higher latitudes
            //adjustHLTime: function (time, base, angle, night, direction) {
            //	var portion = this.nightPortion(angle, night);
            //	var timeDiff = (direction == 'ccw') ?
            //		this.timeDiff(time, base) :
            //		this.timeDiff(base, time);
            //	if (isNaN(time) || timeDiff > portion)
            //		time = base + (direction == 'ccw' ? -portion : portion);
            //	return time;
            //}

            var nightTime = TimeDiff(times[4], times[1]); // sunset to sunrise

            // Adjust Fajr
            var FajrDiff = NightPortion(MethodParams.FajrAngle) * nightTime;
            if (double.IsNaN(times[0]) || TimeDiff(times[0], times[1]) > FajrDiff)
                times[0] = times[1] - FajrDiff;

            // Adjust Isha
            var IshaAngle = (MethodParams.IshaSelector == Params.Kind.Angle) ? MethodParams.IshaParameter : 18;
            var IshaDiff = NightPortion(IshaAngle) * nightTime;
            if (double.IsNaN(times[6]) || TimeDiff(times[4], times[6]) > IshaDiff)
                times[6] = times[4] + IshaDiff;

            // Adjust Maghrib
            var MaghribAngle = (MethodParams.MaghribSelector == Params.Kind.Angle) ? MethodParams.MaghribParameter : 4;
            var MaghribDiff = NightPortion(MaghribAngle) * nightTime;
            if (double.IsNaN(times[5]) || TimeDiff(times[4], times[5]) > MaghribDiff)
                times[5] = times[4] + MaghribDiff;

            return times;
        }

        /// <summary>
        /// the night portion used for adjusting times in higher latitudes
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        private double NightPortion(double angle)
        {
            //nightPortion: function (angle, night) {
            //	var method = setting.highLats;
            //	var portion = 1 / 2 // MidNight
            //	if (method == 'AngleBased')
            //		portion = 1 / 60 * angle;
            //	if (method == 'OneSeventh')
            //		portion = 1 / 7;
            //	return portion * night;
            //}

            switch (HighLatitudeAdjustmentMethod)
            {
                case HighLatitudeAdjustmentMethods.AngleBased:
                    return 1 / 60 * angle;

                case HighLatitudeAdjustmentMethods.MidNight:
                    return 1 / 2d;

                case HighLatitudeAdjustmentMethods.OneSeventh:
                    return 1 / 7d;

                default:
                    return 0;
            }
        }

        /// <summary>
        /// convert hours to day portions 
        /// </summary>
        /// <param name="times"></param>
        /// <returns></returns>
        private double[] DayPortion(double[] times)
        {
            //dayPortion: function (times) {
            //	for (var i in times)
            //		times[i] /= 24;
            //	return times;
            //}

            for (int i = 0; i < times.Length; i++)
            {
                times[i] /= 24;
            }
            return times;
        }

        #endregion

        #region Time Zone Functions

        /// <summary>
        /// return effective timezone for a given date
        /// </summary>
        /// <param name="date"></param>
        /// <param name="timeZone"></param>
        /// <returns></returns>
        private int EffectiveTimeZone(DateTimeOffset date, int? timeZone)
        {
            //// get local time zone
            //getTimeZone: function (date) {
            //	var year = date[0];
            //	var t1 = this.gmtOffset([year, 0, 1]);
            //	var t2 = this.gmtOffset([year, 6, 1]);
            //	return Math.min(t1, t2);
            //}


            //// get daylight saving for a given date
            //getDst: function (date) {
            //	return 1 * (this.gmtOffset(date) != this.getTimeZone(date));
            //},


            //// GMT offset for a given date
            //gmtOffset: function (date) {
            //	var localDate = new Date(date[0], date[1] - 1, date[2], 12, 0, 0, 0);
            //	var GMTString = localDate.toGMTString();
            //	var GMTDate = new Date(GMTString.substring(0, GMTString.lastIndexOf(' ') - 1));
            //	var hoursDiff = (localDate - GMTDate) / (1000 * 60 * 60);
            //	return hoursDiff;
            //}

            int dstOffset = 0;
            if (date.LocalDateTime.IsDaylightSavingTime())
            {
                dstOffset = -1;
            }

            return (timeZone ?? date.Offset.Hours) - dstOffset;
        }

        #endregion

        #region Misc Functions

        /// <summary>
        /// convert float hours to 24h format
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private TimeSpan FloatToTimeSpan(double time)
        {
            time = FixHour(time + 0.5 / 60);  // add 0.5 minutes to round
            var hours = Math.Floor(time);
            var minutes = Math.Floor((time - hours) * 60);
            return new TimeSpan((int)hours, (int)minutes, 0);
        }

        /// <summary>
        /// compute the difference between two times
        /// </summary>
        /// <param name="time1"></param>
        /// <param name="time2"></param>
        /// <returns></returns>
        private double TimeDiff(double time1, double time2)
        {
            return FixHour(time2 - time1);
        }

        #endregion

        #region Degree-Based Math Class

        /// degree sin
        private double DSin(double d) => Math.Sin(DtR(d));

        /// degree cos
        private double DCos(double d) => Math.Cos(this.DtR(d));

        /// degree tan
        private double DTan(double d) => Math.Tan(this.DtR(d));

        /// degree arcsin
        private double DArcSin(double x) => this.RtD(Math.Asin(x));

        /// degree arccos
        private double DArcCos(double x) => this.RtD(Math.Acos(x));

        /// degree arctan
        private double DArcTan(double x) => this.RtD(Math.Atan(x));

        /// degree arctan2
        private double DArcTan2(double y, double x) => this.RtD(Math.Atan2(y, x));

        /// degree arccot
        private double DArcCot(double x) => this.RtD(Math.Atan(1 / x));

        /// degree to radian
        private double DtR(double d) => (d * Math.PI) / 180.0;

        /// radian to degree
        private double RtD(double r) => (r * 180.0) / Math.PI;

        /// range reduce angle in degrees.
        private double FixAngle(double a)
        {
            a -= 360.0 * (Math.Floor(a / 360.0));
            a = a < 0 ? a + 360.0 : a;
            return a;
        }

        /// range reduce hours to 0..23
        private double FixHour(double a)
        {
            a -= 24.0 * (Math.Floor(a / 24.0));
            a = a < 0 ? a + 24.0 : a;
            return a;
        }

        #endregion

        #region Helper Class

        private class SunPos
        {
            public double Declination { get; set; }
            public double Equation { get; set; }
        }

        #endregion
    }
}
