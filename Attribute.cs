using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MARVIN
{
    public class Attribute : IComparable
    {
        public String name;
        public int value;
        public int min;
        public int max;

        public static int UNDEFINED_VALUE = -10000000;

        //public static Attribute UNDEFINED; //name will be "UNDEFINED" if the attribute is undefined
        
        public Attribute(String attributeName, int val, int minVal, int maxVal)
        {
            name = attributeName;
            value = val;
            min = minVal;
            max = maxVal;
        }

        public int CompareTo(Object obj)
        {
            if (obj is Attribute)
            {
                Attribute temp = (Attribute) obj;

                if ((name.CompareTo("UNDEFINED") == 0) && (temp.name.CompareTo("UNDEFINED") == 0))
                {
                    return 0;
                }
                else if (name.CompareTo("UNDEFINED") == 0)
                {
                    return -1;
                }
                else if (temp.name.CompareTo("UNDEFINED") == 0)
                {
                    return 1;
                }
                else
                {
                    return name.CompareTo(temp.name);
                }
            }

            throw new ArgumentException("Invalid entries in attribute list.");  
        }
    }
}
