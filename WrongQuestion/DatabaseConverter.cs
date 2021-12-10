using System;
using System.Collections.Generic;
using System.Text;

namespace WrongQuestion
{
    public class DatabaseConverter
    {
        private string _dateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        public DateTime StringToDateTime(string value)
        {
            try
            {
                int year = int.Parse(value.Substring(0, 4));
                int month = int.Parse(value.Substring(5, 2));
                int day = int.Parse(value.Substring(8, 2));
                int hour = int.Parse(value.Substring(11, 2));
                int minute = int.Parse(value.Substring(14, 2));
                int second = int.Parse(value.Substring(17, 2));

                return new DateTime(year, month, day, hour, minute, second);
            }
            catch
            {
                throw new Exception("Invalid value");
            }
        }

        public string DateTimeToString(DateTime value)
        {
            return value.ToString(_dateTimeFormat);
        }

        public Tracker StringToTracker(string value)
        {
            switch (value)
            {
                case "Defect":
                    return Tracker.Defect;
                case "Feature":
                    return Tracker.Feature;
                case "Patch":
                    return Tracker.Patch;
                default:
                    throw new Exception("Invalid value");
            }
        }

        public Status StringToStatus(string value)
        {
            switch (value)
            {
                case "New":
                    return Status.New;
                case "Resolved":
                    return Status.Resolved;
                default:
                    throw new Exception("Invalid value");
            }
        }
    }
}
