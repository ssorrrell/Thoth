using System;


namespace Thoth.Helpers
{
    public class PodcastPositionHelper
    {
        public static string ConvertPosition(decimal position)
        {
            var returnValue = "";
            //Convert Milliseconds into Hour:Minute:Second
            //Hours
            int hr = 0;
            var hasHr = false;
            var convertedPosition = position;
            if (convertedPosition > 3600)
            {
                var hours = convertedPosition / 3600;
                hr = (int)Math.Truncate(hours);
                convertedPosition = (hours - hr) * 3600;
                hasHr = true;
            }
            //Minutes
            int min = 0;
            var hasMin = false;
            if (convertedPosition > 60)
            {
                var minutes = convertedPosition / 60;
                min = (int)Math.Truncate(minutes);
                convertedPosition = (minutes - min) * 60;
                hasMin = true;
            }
            //Seconds
            int sec = 0;
            if (convertedPosition > 1)
            {
                sec = (int)Math.Truncate(convertedPosition);
            }
            //Aggregation of numbers
            //Hours
            if (hasHr)
            {
                returnValue += hr;
            }
            //Minutes
            if (hasMin)
            {
                if (hasHr)
                {
                    returnValue += ":";
                    returnValue += min.ToString("D2"); //2 decimal
                }
                else
                {
                    returnValue += min; //1 or  decimal
                }
            }
            //Seconds
            if (hasMin)
            {
                returnValue += ":";
                returnValue += sec.ToString("D2"); //2 decimal
            }
            else
            {
                returnValue += sec; //1 or  decimal
            }            

            return returnValue;
        }
    }
}
