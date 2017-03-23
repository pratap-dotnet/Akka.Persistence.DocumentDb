using System;

namespace Akka.Persistence.DocumentDb
{
    public class DateTimeJsonObject
    {
        public int Date { get; set; }
        public long Ticks { get; set; }
        public string TotalTicks { get; set; }
        
        public DateTimeJsonObject(DateTime dateTime)
        {
            var year = dateTime.Year.ToString().PadLeft(4, '0');
            var month = dateTime.Month.ToString().PadLeft(2, '0');
            var date = dateTime.Day.ToString().PadLeft(2, '0');

            Date = Convert.ToInt32($"{year}{month}{date}");
            Ticks = dateTime.Subtract(dateTime.Date).Ticks;
            //Since we are calculating Ticks only since start of the date it would be less than 2^53 which is json stored
            TotalTicks = dateTime.Ticks.ToString();
        }
        public DateTimeJsonObject()
        {

        }

        public DateTime ToDateTime()
        {
            return new DateTime(long.Parse(TotalTicks));
        }
    }
}
