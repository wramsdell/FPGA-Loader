// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using System;
using System.Linq;
using System.Text;

namespace SendBitstream
{
    public class Arguments
    {
        public static void Parse(string[] args, object target)
        {
            for (int counter = 0; counter < args.Length; ++counter)
            {
                if (args[counter].StartsWith("/"))
                {
                    var argumentName = args[counter].Substring(1);
                    var argumentValue = args[counter + 1];
                    object finalValue = argumentValue;

                    var property = (from p in target.GetType().GetProperties()
                                    from a in p.GetCustomAttributes(typeof(ArgumentAttribute), true)
                                    where (((ArgumentAttribute)a).Name == argumentName)
                                    select p).First();

                    if (property.PropertyType != typeof(string))
                    {
                        finalValue = property.PropertyType.GetMethod("Parse", new Type[] { typeof(string) } ).Invoke(null, new object[] { argumentValue });
                    }
                    property.SetValue(target, finalValue, null);
                }
            }
        }

        public static string GetDescriptionText(object target)
        {
            var builder = new StringBuilder();
            var helpLines = (from p in target.GetType().GetProperties()
                             from a in p.GetCustomAttributes(typeof(ArgumentAttribute), true)
                             select "/" + ((ArgumentAttribute) a).Name + " " + ((ArgumentAttribute) a).Description);

            foreach (var line in helpLines)
            {
                builder.Append(line);
                builder.Append(Environment.NewLine);
            }

            return builder.ToString();
        }
    }
}
