using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FlysasClient
{

    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    public class OptionParserAttribute : Attribute
    {
        public string OptionName { get; private set; }        
        public OptionParserAttribute(string optionName)
        {
            this.OptionName = optionName;
        }
    }

    public class OptionsParser
    {
        public string Help()
        {
            string s="";
            foreach(var prop in this.GetType().GetProperties(BindingFlags.Public|BindingFlags.Instance))
            {
                var attr = prop.GetCustomAttribute(typeof(OptionParserAttribute)) as OptionParserAttribute;
                if(attr!=null)
                {
                    s += attr.OptionName + " " + prop.GetValue(this).ToString() + " ";
                }
            }
            return s;
        }
        public bool Parse(string s)
        {
            var stack = new Stack<string>(s.ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Reverse());
            if (stack.Any() && stack.Pop() == "set")
            {
                while (stack.Count >= 2)
                {
                    var option = stack.Pop();
                    var sVal = stack.Pop();
                    foreach (var prop in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        var attr = prop.GetCustomAttribute(typeof(OptionParserAttribute)) as OptionParserAttribute;
                        if (attr != null && attr.OptionName == option)
                        {
                            if(prop.PropertyType == typeof(bool))
                                prop.SetValue(this, myBool(sVal));
                            if (prop.PropertyType == typeof(string))
                                prop.SetValue(this, sVal);
                        }
                    }
                }
                return true;
            }
            else return false;
        }
        bool myBool(string s)
        {
            return s == "on" || s == "true" || s == "1" || s == "yes";
        }

        
    }

    


    public class Options : OptionsParser
    {
        [OptionParser("bookingclass")]
        public bool OutputBookingClass { get; private set; } = false;
        [OptionParser("equipment")]
        public bool OutputEquipment { get; private set; } = false;
        [OptionParser("table")]
        public bool Table { get; private set; } = false;
        [OptionParser("username")]
        public string UserName { get; set; }
        [OptionParser("pwd")]
        public string Password { get; set; }
    }
}
