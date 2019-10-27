using System;
using System.Collections.Generic;
using System.Text;

namespace FlysasLib
{
    public class CalendarPrinter
    {
        /// <summary>
        /// Writes out a single reservation to an Ical file
        /// </summary>
        /// <param name="iCalFolder">Write to Desktop if NULL</param>
        /// <param name="myReservation"></param>
        public void WriteICal(ReservationsResult.Reservation myReservation)
        {

            var s = System.IO.Path.Combine(System.IO.Path.Combine(System.AppContext.BaseDirectory, "Export", myReservation.AirlineBookingReference + ".ics"));

            System.IO.FileInfo iCalPath = new System.IO.FileInfo(s);
            if (iCalPath.Exists) iCalPath.Delete();
            System.IO.TextWriter iCwt = System.IO.File.CreateText(iCalPath.FullName);
            iCwt.WriteLine("BEGIN:VCALENDAR");
            iCwt.WriteLine("PRODID:Calendar");
            iCwt.WriteLine("VERSION:2.0");
            //for each Connection: a full flight with possible multiple connections
            foreach (ReservationsResult.Connection con in myReservation.Connections)
            {
                //  for each single flight in your trip  
                foreach (ReservationsResult.FlightSegment FS in con.FlightSegments)
                {
                    //Write Checkin
                    iCwt.WriteLine("BEGIN:VEVENT");
                    iCwt.Write("UID:");// make UID out of reproducible data
                    iCwt.Write(myReservation.AirlineBookingReference);
                    iCwt.Write(con.Destination.AirportCode);
                    iCwt.Write(FS.Arrival.AirportCode);
                    iCwt.WriteLine("C");
                    iCwt.WriteLine("CLASS:PUBLIC");
                    iCwt.WriteLine("X-MICROSOFT-CDO-BUSYSTATUS:FREE");
                    iCwt.Write("SUMMARY;LANGUAGE=en-us:");
                    iCwt.Write("Open for check in: ");
                    iCwt.Write(FS.OperatingCarrier.Name);
                    iCwt.Write(" ");
                    iCwt.Write(FS.OperatingCarrier.Code);
                    iCwt.Write(FS.OperatingCarrier.FlightNumber.ToString());
                    iCwt.WriteLine(".");//end of summary
                    iCwt.Write("DTSTAMP;VALUE=DATE-TIME:");
                    iCwt.WriteLine(System.DateTime.Now.ToString("yyyyMMddTHHmmss"));
                    iCwt.Write("DTSTART;VALUE=DATE-TIME:");
                    iCwt.WriteLine(FS.CheckInStartDateGmt.ToString("yyyyMMddTHHmmssZ"));
                    iCwt.Write("DTEND;VALUE=DATE-TIME:");
                    iCwt.WriteLine(FS.CheckInCloseDateGmt.ToString("yyyyMMddTHHmmssZ"));
                    iCwt.Write("LOCATION:");
                    iCwt.Write(FS.Departure.AirportName);
                    iCwt.Write(@"\, ");
                    iCwt.Write(FS.Departure.CityName);
                    iCwt.Write(@"\, ");
                    iCwt.WriteLine(FS.Departure.CountryName);
                    iCwt.Write("DESCRIPTION: Booking reference:");
                    iCwt.Write(FS.BookingReference);
                    iCwt.Write(@"\n ");
                    iCwt.Write("Airline:");
                    iCwt.Write(FS.OperatingCarrier.Name);
                    iCwt.Write(" to: ");
                    iCwt.WriteLine(FS.Arrival.CityName);
                    iCwt.WriteLine("TRANSP:TRANSPARENT");
                    iCwt.WriteLine("END:VEVENT");

                    //Write Flight
                    iCwt.WriteLine("BEGIN:VEVENT");
                    iCwt.Write("UID:");// make UID out of reproducible data
                    iCwt.Write(myReservation.AirlineBookingReference);
                    iCwt.Write(con.Destination.AirportCode);
                    iCwt.Write(FS.Arrival.AirportCode);
                    iCwt.WriteLine("F");
                    iCwt.WriteLine("CLASS:PUBLIC");
                    iCwt.WriteLine("X-MICROSOFT-CDO-BUSYSTATUS:OOF");
                    iCwt.WriteLine();
                    iCwt.WriteLine(GetCategory());
                    iCwt.Write("SUMMARY;LANGUAGE=en-us:");
                    iCwt.Write("Flight: ");
                    iCwt.Write(FS.OperatingCarrier.Name);
                    iCwt.Write(" ");
                    iCwt.Write(FS.OperatingCarrier.Code);
                    iCwt.Write(FS.OperatingCarrier.FlightNumber.ToString());
                    iCwt.WriteLine(".");//end of summary
                    iCwt.Write("DTSTAMP;VALUE=DATE-TIME:");
                    iCwt.WriteLine(System.DateTime.Now.ToString("yyyyMMddTHHmmss"));
                    iCwt.Write("DTSTART;VALUE=DATE-TIME:");
                    iCwt.WriteLine(FS.ScheduledDepartureDateGmt.ToString("yyyyMMddTHHmmssZ"));
                    iCwt.Write("DTEND;VALUE=DATE-TIME:");
                    iCwt.WriteLine(FS.ScheduledArrivalDateGmt.ToString("yyyyMMddTHHmmssZ"));
                    iCwt.Write("LOCATION:");
                    iCwt.Write(FS.Departure.AirportName);
                    iCwt.Write(@"\, ");
                    iCwt.Write(FS.Departure.CityName);
                    iCwt.Write(@"\, ");
                    iCwt.WriteLine(FS.Departure.CountryName);
                    iCwt.Write("DESCRIPTION: Departure (Local time):");
                    iCwt.Write(FS.ScheduledDepartureDateLocal.ToString("g"));
                    iCwt.Write(@" \n ");
                    iCwt.Write("Arrival (Local time):");
                    iCwt.Write(FS.ScheduledArrivalDateLocal.ToString("g"));
                    iCwt.Write(@" \n ");
                    iCwt.Write("Airline: ");
                    iCwt.Write(FS.OperatingCarrier.Name);
                    iCwt.Write(" to: ");
                    iCwt.Write(FS.Arrival.CityName);
                    iCwt.Write(@"\n Duration: ");
                    try
                    {
                        TimeSpan ts = System.Xml.XmlConvert.ToTimeSpan(FS.Duration);
                        if (ts.TotalMinutes <= 90)
                        {
                            iCwt.Write(ts.TotalMinutes.ToString("f0"));
                            iCwt.WriteLine(" minutes.");
                        }
                        else
                        {
                            iCwt.Write(ts.TotalHours.ToString("f1"));
                            iCwt.WriteLine(" hours.");
                        }

                    }
                    catch (Exception)
                    {

                        iCwt.WriteLine(FS.Duration);
                    }


                    iCwt.WriteLine("TRANSP:TRANSPARENT");
                    iCwt.WriteLine("END:VEVENT");




                }
            }

            iCwt.WriteLine("END:VCALENDAR");
            iCwt.Flush();




        }

        private string GetCategory()
        {
            System.Globalization.CultureInfo currentCulture = System.Threading.Thread.CurrentThread.CurrentUICulture;
            string lan = currentCulture.TwoLetterISOLanguageName;
            string r;
            switch (lan)

            {
                case "en":
                r = "CATEGORIES;LANGUAGE=en:Travel";
                    break;
                case "dk":
                    r = "CATEGORIES;LANGUAGE=dk:Rejse";
                    break;
                case "no":
                    r = "CATEGORIES;LANGUAGE=no:Tur";
                    break;
                case "se":
                    r = "CATEGORIES;LANGUAGE=se:Resa";
                    break;
                case "fi":
                    r = "CATEGORIES;LANGUAGE=fi:Matka";
                    break;
                case "fr":
                    r = "CATEGORIES;LANGUAGE=fr:Voyage";
                    break;
                case "nl":
                    r = "CATEGORIES;LANGUAGE=nl:Reizen";
                    break;
                case "de":
                    r = "CATEGORIES;LANGUAGE=de:Reise";
                    break;
                default:
                    r = "CATEGORIES;LANGUAGE=en:Travel";
                    break;
            }
            return r;
        }

    }
}
