// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;

namespace PrayerTimes
{
    public class Params
    {
        /// <summary>
        /// time different between imsak and fajr, usually 10 minutes
        /// </summary>
        public double ImsakTime { get; set; }

        /// <summary>
        /// fajr angle
        /// </summary>
        public double FajrAngle { get; set; }

        /// <summary>
        /// maghrib selector (0 = angle; 1 = minutes after sunset)
        /// </summary>
        public Kind MaghribSelector { get; set; }

        /// <summary>
        /// maghrib parameter value (in angle or minutes)
        /// </summary>
        public double MaghribParameter { get; set; }

        /// <summary>
        /// isha selector (0 = angle; 1 = minutes after maghrib)
        /// </summary>
        public Kind IshaSelector { get; set; }

        /// <summary>
        /// isha parameter value (in angle or minutes)
        /// </summary>
        public double IshaParameter { get; set; }

        /// <summary>
        /// (0 = angle; 1 = minutes after sunset)
        /// </summary>
        public enum Kind
        {
            Angle,
            Minute,
        }

        public static Params GetMethodsParams(CalculationMethods calculationMethods, Params customParams)
        {
            switch (calculationMethods)
            {
                case CalculationMethods.Jafari:
                    // { 16, 0, 4, 0, 14 };
                    return new Params
                    {
                        ImsakTime = 10,
                        FajrAngle = 16,
                        MaghribSelector = Kind.Angle,
                        MaghribParameter = 4,
                        IshaSelector = Kind.Angle,
                        IshaParameter = 14,
                    };


                case CalculationMethods.Karachi:
                    // { 18, 1, 0, 0, 18 };
                    return new Params
                    {
                        ImsakTime = 10,
                        FajrAngle = 18,
                        MaghribSelector = Kind.Minute,
                        MaghribParameter = 0,
                        IshaSelector = Kind.Angle,
                        IshaParameter = 18,
                    };


                case CalculationMethods.ISNA:
                    // { 15, 1, 0, 0, 15 };
                    return new Params
                    {
                        ImsakTime = 10,
                        FajrAngle = 15,
                        MaghribSelector = Kind.Minute,
                        MaghribParameter = 0,
                        IshaSelector = Kind.Angle,
                        IshaParameter = 15,
                    };


                case CalculationMethods.MWL:
                    // { 18, 1, 0, 0, 17 };
                    return new Params
                    {
                        ImsakTime = 10,
                        FajrAngle = 18,
                        MaghribSelector = Kind.Minute,
                        MaghribParameter = 0,
                        IshaSelector = Kind.Angle,
                        IshaParameter = 17,
                    };


                case CalculationMethods.Makkah:
                    // { 19, 1, 0, 1, 90 };
                    return new Params
                    {
                        ImsakTime = 10,
                        FajrAngle = 19,
                        MaghribSelector = Kind.Minute,
                        MaghribParameter = 0,
                        IshaSelector = Kind.Minute,
                        IshaParameter = 90,
                    };


                case CalculationMethods.Egypt:
                    // { 19.5, 1, 0, 0, 17.5 };
                    return new Params
                    {
                        ImsakTime = 10,
                        FajrAngle = 19.5,
                        MaghribSelector = Kind.Minute,
                        MaghribParameter = 0,
                        IshaSelector = Kind.Angle,
                        IshaParameter = 17.5,
                    };


                case CalculationMethods.Kemenag:
                    // { 20, 0, 0, 0, 18 };
                    return new Params
                    {
                        ImsakTime = 10,
                        FajrAngle = 20,
                        MaghribSelector = Kind.Angle,
                        MaghribParameter = 0,
                        IshaSelector = Kind.Angle,
                        IshaParameter = 18,
                    };


                case CalculationMethods.Custom:
                    // { 18, 1, 0, 0, 17 };
                    return customParams ?? new Params
                    {
                        ImsakTime = 10,
                        FajrAngle = 18,
                        MaghribSelector = Kind.Minute,
                        MaghribParameter = 0,
                        IshaSelector = Kind.Angle,
                        IshaParameter = 17,
                    };


                default:
                    throw new ArgumentOutOfRangeException(nameof(calculationMethods));
            }
        }
    }
}
