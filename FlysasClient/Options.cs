using System;
using System.Collections.Generic;
using System.Linq;

namespace FlysasClient
{
    public class Options
    {
        public bool OutputBookingClass { get; private set; } = false;
        public bool OutputEquipment { get; private set; } = false;


        public bool Parse(string s)
        {
            var stack = new Stack<string>(s.ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Reverse());
            if (stack.Any() && stack.Pop() == "set")
            {
                while (stack.Count >= 2)
                {
                    var option = stack.Pop();
                    var sVal = stack.Pop();
                    switch (option)
                    {
                        case "bookingclass": OutputBookingClass = myBool(sVal); break;
                        case "equipment": OutputEquipment = myBool(sVal); break;
                    }
                }
                return true;
            }
            else return false;
        }
        public bool myBool(string s)
        {
            return s == "on" || s == "true" || s == "1" || s == "yes";
        }

    }
}
