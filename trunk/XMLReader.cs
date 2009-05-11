using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.XPath;

namespace MARVIN
{
    class XMLReader
    {
        static XmlDocument doc;
        public GlobalVariables global;

        public XMLReader(ref GlobalVariables g)
        {
            global = g;
        }


        /*
         * Parses an entire XML building document
         */
        public void parseXMLBuildingFile(String filename)
        {
            try
            {
                doc = new XmlDocument();
                doc.Load(filename);

                //Initializes the global attributes list
                global.attributes = new List<Attribute>();

                /*
                 * First we figure out what all of the attributes are
                 */
                XmlNodeList attributeXMLNodes = doc.GetElementsByTagName("Attribute");
                XmlElement thisXMLElement; 
                String thisAttributeName;
                int thisAttributeMinValue;
                int thisAttributeMaxValue;
                Attribute thisAttribute;
                Attribute undefinedAttribute = new Attribute("UNDEFINED", -1, -1, -1);
                for(int i=0; i < attributeXMLNodes.Count; i++)
                {
                    if(i >=8)
                    {
                        break;
                    }
                    
                    thisXMLElement = (XmlElement) attributeXMLNodes[i];

                    thisAttributeName = thisXMLElement.Attributes["name"].Value;
                    thisAttributeMinValue = Int32.Parse(thisXMLElement.Attributes["min"].Value);
                    thisAttributeMaxValue = Int32.Parse(thisXMLElement.Attributes["max"].Value);

                    thisAttribute = new Attribute(thisAttributeName,-1,thisAttributeMinValue,thisAttributeMaxValue);
                    global.attributes.Add(thisAttribute);
                }
                global.attributes.Sort();

                while (global.attributes.Count < 8)
                {
                    global.attributes.Add(undefinedAttribute);
                }


                /*
                 * Next we parse the individual buildings
                 */
                XmlNodeList buildingXMLNodes = doc.GetElementsByTagName("Building");
                int thisAttributeValue;
                String thisAttributeValueString;
                String thisBuildingName;
                for (int i = 0; i < buildingXMLNodes.Count; i++)
                {
                    thisXMLElement = (XmlElement) buildingXMLNodes[i];
                    thisBuildingName = thisXMLElement.Attributes["Name"].Value;

                    for (int j = 0; j < global.attributes.Count; j++)
                    {
                        thisAttributeName = global.attributes[j].name;
                        Console.WriteLine("thisAttributeName: " + thisAttributeName);

                        if (thisXMLElement.HasAttribute(thisAttributeName))
                        {
                            XmlAttribute temp = (XmlAttribute)thisXMLElement.Attributes[thisAttributeName];
                            thisAttributeValueString = (String)temp.Value;
                            thisAttributeValue = Int32.Parse(thisAttributeValueString);
                        }
                        else
                        {
                            thisAttributeValue = Attribute.UNDEFINED_VALUE;
                        }
                        
                        thisAttributeMinValue = global.attributes[j].min;
                        thisAttributeMaxValue = global.attributes[j].max;

                        thisAttribute = new Attribute(thisAttributeName, thisAttributeValue, thisAttributeMinValue, thisAttributeMaxValue);

                        //Now we have to update the attribute list of this building
                        //First we find this building in one of our two blocks
                        Building foundBuilding;
                        foundBuilding = global.block1.getBuilding(thisBuildingName);
                        if (foundBuilding.getBuildingName().CompareTo("Error") == 0) //if building not found in block 1
                        {
                            foundBuilding = global.block2.getBuilding(thisBuildingName);

                            if (foundBuilding.getBuildingName().CompareTo("Error") == 0) //if building not found in block 2
                            {
                                Console.WriteLine("ERROR: " + thisBuildingName + " in XML file not found in scene.");
                            }
                            else
                            {
                                foundBuilding.setAttribute(thisAttribute);
                                global.block2.removeBuilding(thisBuildingName);
                                global.block2.addBuilding(foundBuilding);
                            }
                        }
                        else
                        {
                            foundBuilding.setAttribute(thisAttribute);
                            global.block1.removeBuilding(thisBuildingName);
                            global.block1.addBuilding(foundBuilding);
                        }
                    }
                } //end for each building
            } //end try
            catch (XmlException xmle)
            {
                Console.WriteLine(xmle.Message);
            }
        } //end parseXMLBuildingFile(...)
    }
    //end class
}//end namespace
