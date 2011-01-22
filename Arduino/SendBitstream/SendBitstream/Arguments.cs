// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace SendBitstream
{
    public class Arguments
    {
        public static string[] Parse(string[] args, object target)
        {
            var returnStrings = new List<string>();

            for (int counter = 0; counter < args.Length; ++counter)
            {
                if (args[counter].StartsWith("/"))
                {
                    var argumentName = args[counter].Substring(1);
                    object argumentValue;

                    var property = (from p in target.GetType().GetProperties()
                                    from a in p.GetCustomAttributes(typeof(ArgumentAttribute), true)
                                    where (((ArgumentAttribute)a).Name == argumentName)
                                    select p).First();

                    if (property.PropertyType == typeof(bool))
                    {
                        argumentValue = true;
                    }
                    else
                    {
                        argumentValue = args[counter + 1];

                        ++counter;

                        if (property.PropertyType != typeof(string))
                        {
                            argumentValue = property.PropertyType.GetMethod("Parse", new Type[] { typeof(string) }).Invoke(null, new object[] { argumentValue });
                        }
                    }
                    property.SetValue(target, argumentValue, null);
                }
                else
                {
                    returnStrings.Add(args[counter]);
                }
            }

            return returnStrings.ToArray();
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
