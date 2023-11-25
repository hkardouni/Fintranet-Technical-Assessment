/*
 * Code Review results:
*** Millisecond Comparison: Calculating the time difference in milliseconds (date.Millisecond - intervalStart.Millisecond) won't give us the actual difference in time between two DateTime objects.
   Instead, we need to consider using TimeSpan to calculate the difference between two DateTime objects.
*** Logic in GetTax method: The logic within the GetTax method might need reevaluation. It seems to calculate fees based on time differences and then modifies the total fee.
    The approach might need a closer review to ensure the correct calculation of fees based on intervals.
*** Toll-Free Vehicle Check: The IsTollFreeVehicle method checks for toll-free vehicles based on a specific set of vehicle types.
    We need to ensure that this list is exhaustive and includes all types that should be toll-free.
*** Date Comparison: The IsTollFreeDate method seems to check for specific dates that are toll-free.
    We need to ensure that the dates and conditions are accurate and cover all exempted dates.
*** Return Type for Time-Related Methods: We need to consider using TimeSpan or a more appropriate data type instead of returning fees directly in minutes or milliseconds.
    It would make the code more expressive and easier to understand.
*** Consistency in Naming Conventions: We need to make sure the naming conventions for methods and variables are consistent throughout the codebase.
*** Error Handling: The code does not have explicit error handling for scenarios like dates being empty or GetTollFee being unable to determine a fee.
*** There is a conflict due to the same name being used for both the interface and the class 'Vehicle'. To resolve this conflict, we can either rename the interface or the class to avoid ambiguity.
 */


using System;

namespace congestion.calculator
{
    public class MyVehicle : IVehicle
    {
        public string GetVehicleType()
        {
            // Implementation of logic to return the vehicle type
            // Example:
            return "Car";
        }
    }

    public class CongestionTaxCalculator
    {
        public int GetTax(IVehicle vehicle, DateTime[] dates)
        {
            if (vehicle == null || dates == null || dates.Length == 0)
            {
                throw new ArgumentException("Invalid vehicle or date array");
            }

            int totalFee = 0;
            DateTime intervalStart = dates[0];
            try
            {
                foreach (DateTime date in dates)
                {
                    int nextFee = GetTollFee(date, vehicle);
                    int tempFee = GetTollFee(intervalStart, vehicle);

                    TimeSpan timeDifference = date - intervalStart;
                    double minutes = timeDifference.TotalMinutes;

                    if (minutes <= 60)
                    {
                        if (totalFee > 0) totalFee -= tempFee;
                        if (nextFee >= tempFee) tempFee = nextFee;
                        totalFee += tempFee;
                    }
                    else
                    {
                        totalFee += nextFee;
                    }

                    intervalStart = date; // Update intervalStart for the next iteration
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error calculating tax. Details: " + ex.Message);
            }
            // Ensure total fee doesn't exceed the daily cap of 60
            return Math.Min(totalFee, 60);
        }

        private bool IsTollFreeVehicle(IVehicle vehicle)
        {
            if (vehicle == null) return false;

            string vehicleType = vehicle.GetVehicleType();
            TollFreeVehicles[] tollFreeTypes = {
            TollFreeVehicles.Motorcycle, TollFreeVehicles.Tractor,
            TollFreeVehicles.Emergency, TollFreeVehicles.Diplomat,
            TollFreeVehicles.Foreign, TollFreeVehicles.Military
        };

            return Array.Exists(tollFreeTypes, type => type.ToString() == vehicleType);
        }

        public int GetTollFee(DateTime date, IVehicle vehicle)
        {
            if (IsTollFreeDate(date) || IsTollFreeVehicle(vehicle)) return 0;

            int hour = date.Hour;
            int minute = date.Minute;

            if ((hour == 6 && minute <= 29) || (hour == 6 && minute >= 30) || hour == 7 || (hour == 8 && minute <= 29))
            {
                return hour == 6 ? 8 : 13;
            }
            else if ((hour >= 8 && hour <= 14 && minute >= 30) || (hour == 15 && minute <= 29) || (hour == 15 && minute >= 0) || (hour == 16))
            {
                return hour == 8 ? 8 : 13;
            }
            else if ((hour == 17) || (hour == 18 && minute <= 29))
            {
                return 13;
            }
            else
            {
                return 0;
            }
        }

        private bool IsTollFreeDate(DateTime date)
        {
            int year = date.Year;
            int month = date.Month;
            int day = date.Day;

            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                return true;
            }

            if (year == 2013)
            {
                return (month == 1 && day == 1) || (month == 3 && (day == 28 || day == 29))
                    || (month == 4 && (day == 1 || day == 30)) || (month == 5 && (day == 1 || day == 8 || day == 9))
                    || (month == 6 && (day == 5 || day == 6 || day == 21)) || (month == 7) || (month == 11 && day == 1)
                    || (month == 12 && (day == 24 || day == 25 || day == 26 || day == 31));
            }

            return false;
        }

        private enum TollFreeVehicles
        {
            Motorcycle,
            Tractor,
            Emergency,
            Diplomat,
            Foreign,
            Military
        }
    }
}