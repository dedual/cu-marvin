using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace MARVIN
{
    class XMLManager
    {
        static XmlDocument doc;
        public GlobalVariables global;

        public XMLManager(ref GlobalVariables g)
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

                global.buildingList = new List<Building>();

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
                    Building thisNewBuilding = new Building(thisBuildingName, ref global);

                    for (int j = 0; j < global.attributes.Count; j++)
                    {
                        thisAttributeName = global.attributes[j].name;
                        //Console.WriteLine("thisAttributeName: " + thisAttributeName);

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
                        /*Building foundBuilding;
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
                        }*/
                        
                        thisNewBuilding.setAttribute(thisAttribute);
                    }


                    global.buildingList.Add(thisNewBuilding);
                } //end for each building
            } //end try
            catch (XmlException xmle)
            {
                Console.WriteLine(xmle.Message);
            }
        } //end parseXMLBuildingFile(...)



        public void writeToXml(String output)
        {
            XmlDocument newXML = new XmlDocument();
            XmlDeclaration declaration = newXML.CreateXmlDeclaration("1.0", "utf-8", null);
            XmlElement rootNode = doc.CreateElement("'Root'");
            doc.InsertBefore(declaration, doc.DocumentElement);
            doc.AppendChild(rootNode);
            foreach (Attribute attrib in global.attributes)
            {
                XmlElement AttributeNode = doc.CreateElement("Attribute");
                AttributeNode.SetAttribute(attrib.name,attrib.value.ToString());
                AttributeNode.SetAttribute("min", attrib.min.ToString());
                AttributeNode.SetAttribute("max", attrib.max.ToString());
                rootNode.AppendChild(AttributeNode);
            }     
            
            foreach(XmlNode node in doc.GetElementsByTagName("Building"))
            {
                XmlElement elementNodeFromOldXml = (XmlElement)node;
                String buildingName = elementNodeFromOldXml.Attributes["Name"].Value;
                String buildingAddress = elementNodeFromOldXml.Attributes["Address"].Value;
                String buildingStories = elementNodeFromOldXml.Attributes["Stories"].Value;
                String yearBuilt = elementNodeFromOldXml.Attributes["Year_Built"].Value;
                String saleDate = elementNodeFromOldXml.Attributes["Sale_Date"].Value;
                String buildingType = elementNodeFromOldXml.Attributes["Building_Type"].Value;
                String buildingClass = elementNodeFromOldXml.Attributes["Building_Class"].Value;
                String toxicSites = elementNodeFromOldXml.Attributes["Toxic_Sites"].Value;
                String unusedBuildableSqFt = elementNodeFromOldXml.Attributes["Unused_Buildable_Square_Feet"].Value;

                //Now that we have the building name, lets use it to write to the newXML
                XmlElement buildingNode = doc.CreateElement("Building");
                buildingNode.SetAttribute("Name", buildingName);
                buildingNode.SetAttribute("Address",buildingAddress);
                buildingNode.SetAttribute("Stories", buildingStories);
                buildingNode.SetAttribute("Year_Built",yearBuilt);
                buildingNode.SetAttribute("Sale_Date",saleDate);
                buildingNode.SetAttribute("Building_Type",buildingType);
                buildingNode.SetAttribute("Building_Class",buildingClass);
                buildingNode.SetAttribute("Toxic_Sites",toxicSites);
                buildingNode.SetAttribute("Unused_Buildable_Square_Feet",unusedBuildableSqFt);

                foreach (Attribute buildingAttribute in global.attributes)
                {
                    String attributeName = buildingAttribute.value.ToString();
                    Building building = global.block1.getBuilding(buildingAddress);
                    buildingNode.SetAttribute(attributeName,building.getAttributeValue(attributeName).ToString());
                }
                rootNode.AppendChild(buildingNode);
            }
 
            newXML.Save("newXMLFile3.xml");
        }
    }
    //end class
}//end namespace
