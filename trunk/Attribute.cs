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

                return name.CompareTo(temp.name);
            }

            throw new ArgumentException("Invalid entries in attribute list.");  
        }
    }
}
