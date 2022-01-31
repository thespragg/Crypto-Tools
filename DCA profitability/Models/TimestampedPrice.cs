using System;

namespace DCA_profitability.Models
{
    public class TimestampedPrice
    {
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public TimestampedPrice(decimal timestamp, decimal price) => (Date, Price) = (UnixTimeStampToDateTime((long)timestamp), price);

        private DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            DateTimeOffset dateTimeOffSet = DateTimeOffset.FromUnixTimeMilliseconds(unixTimeStamp);
            DateTime dateTime = dateTimeOffSet.DateTime;
            return dateTime;
        }
    }
}