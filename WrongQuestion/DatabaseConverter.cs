using System;
using System.Collections.Generic;
using System.Text;

namespace WrongQuestion
{
    public class DatabaseConverter
    {
        private readonly string _dateTimeFormat = "yyyy-MM-dd HH:mm:ss";

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
    }
}
