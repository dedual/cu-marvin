using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

using GoblinXNA;
using GoblinXNA.Graphics;
using GoblinXNA.SceneGraph;
using Model = GoblinXNA.Graphics.Model;
using GoblinXNA.Graphics.Geometry;
using GoblinXNA.Device.Generic;
using GoblinXNA.Device.Util;
using GoblinXNA.Physics;
using GoblinXNA.Helpers;

using GoblinXNA.UI.UI2D;
using GoblinXNA.UI.Events;

using GoblinXNA.Device.Capture;
using GoblinXNA.Device.Vision;
using GoblinXNA.Device.Vision.Marker;

namespace MARVIN
{
    public class GlobalVariables
    {
        public XmlDocument doc;
        public bool outdoors;

        public GraphicsDeviceManager graphics; //Graphics Device
        //public ContentManager Content;

        public Scene scene; //Scene
        public MarkerNode groundMarkerNode, toolbar1MarkerNode, indoorMarkerNode; //Ground and Pointer Markers
    //    public MarkerNode block1MarkerNode, block2MarkerNode; //Markers for each block
        //public List<GeometryNode> buildings;
        public Texture2D notebookTopTexture;        
        public TransformNode parentTrans;
        public TransformNode toolBar1OccluderTransNode;

        public TransformNode notebookBoxTransNode;
        public List<TransformNode> attributeTransNodes;
        public List<GeometryNode> attributeBoxes;
        public List<Material> attributeMaterials;

        public MarkerNode blockMarker;
        public TransformNode blockTransNode;       
        public List<TransformNode> buildingTransNodes;        
        public List<GeometryNode> buildingGeomNodes;
        public List<Building> buildingList;

        public List<Attribute> attributes; //size 8
        public String xmlFilename;

        public String label = "Welcome to MARVIN!";
        public Color labelColor = Color.Goldenrod;
        public SpriteFont labelFont;
        public SpriteFont uiFont;
        public List<G2DLabel> attributeLabels;
        public List<G2DPanel> labelPanels;

        public Vector3 calibrateCoords;

        public GeometryNode pointerTip;
        public GeometryNode pointerSegment;
        public GeometryNode toolbar1Node;
        public GeometryNode notebookBoxNode;
        public TransformNode notebookShowcaseTransNode;
        public GeometryNode notebookShowcaseGeomNode;
        public TransformNode notebookShowcaseModelTransNode;
        public int typeOfObjectBeingHighlighted = -1;
        public int indexOfObjectBeingHighlighted = -1;
        public int typeOfObjectBeingSelected = -1;
        public int indexOfBuildingBeingSelected = -1;
        public int attributeCurrentlyBeingViewed = -1;
        public int NOTHING = -1;
        public int BUILDING = 1;
        public int ATTRIBUTE = 2;
        public int LEFT_CONE = 3;
        public int RIGHT_CONE = 4;

        public TransformNode leftConeTransNode;
        public TransformNode rightConeTransNode;
        public GeometryNode leftConeGeomNode;
        public GeometryNode rightConeGeomNode;

        public Material pointerMaterial;
        public Material notebookTopMaterial;

        public Model notebookModel;

        public Vector4[] colorPalette = new Vector4[8];
        public Vector4 ORIGIN = new Vector4(0, 0, 0, 1);
        public String selectedBuildingName = null;

        public G2DPanel buildingInfoPanel;
        public int buildingInfoIndex = -1;
        public G2DLabel nameLabel;
        public G2DLabel descriptionLabel;
        public G2DLabel addressLabel;
        public G2DLabel storiesLabel;
        public G2DLabel toxicSitesLabel;
        public G2DLabel airRightsLabel;
        public G2DLabel yearBuiltLabel;
        public G2DLabel typeLabel;
        public G2DLabel classLabel;
        public G2DLabel saleDateLabel;
        public List<G2DLabel> markerLabels;

        public G2DPanel gradientPanel = new G2DPanel();
        public G2DLabel firstGradientLabel = new G2DLabel();
        public G2DLabel secondGradientLabel = new G2DLabel();
        public G2DLabel thirdGradientLabel = new G2DLabel();
        public G2DLabel fourthGradientLabel = new G2DLabel();
        public G2DLabel fifthGradientLabel = new G2DLabel();
        public G2DLabel firstGradientValueLabel = new G2DLabel();
        public G2DLabel thirdGradientValueLabel = new G2DLabel();
        public G2DLabel fifthGradientValueLabel = new G2DLabel();
        public G2DLabel gradientNameLabel = new G2DLabel();
        public G2DLabel gradientNameLabel2 = new G2DLabel();

        public G2DTextField nameField;
        public G2DTextField descriptionField;
        public G2DTextField addressField;
        public G2DTextField storiesField;
        public G2DTextField toxicSitesField;
        public G2DTextField airRightsField;
        public G2DTextField yearBuiltField;
        public G2DTextField typeField;
        public G2DTextField classField;
        public G2DTextField saleDateField;


        public Block block1, block2;

        public float y_shift = -62;
        public float x_shift = -28.0f;
        public float scale;

        public GlobalVariables()
        {
            //Do nothing
            //graphics = new GraphicsDeviceManager(this);

            colorPalette[0] = Color.Red.ToVector4();
            colorPalette[1] = Color.CornflowerBlue.ToVector4();
            colorPalette[2] = Color.Maroon.ToVector4();
            colorPalette[3] = Color.Blue.ToVector4();
            colorPalette[4] = Color.Green.ToVector4();
            colorPalette[5] = Color.Aqua.ToVector4();
            colorPalette[6] = Color.Purple.ToVector4();
            colorPalette[7] = Color.DeepPink.ToVector4();
        }

        public void initializeLabels()
        {
            attributeLabels = new List<G2DLabel>();
            labelPanels = new List<G2DPanel>();
            G2DLabel thisLabel;
            G2DPanel thisPanel;
            for (int i = 0; i < 8; i++)
            {
                thisLabel = new G2DLabel("Attribute Label " + i);
                thisLabel.TextFont = uiFont;
                thisLabel.TextColor = Color.Black;
                thisLabel.Visible = true;
                thisLabel.TextTransparency = 1.0f;
                attributeLabels.Add(thisLabel);
                
                thisPanel = new G2DPanel();
                thisPanel.Border = GoblinEnums.BorderFactory.LineBorder;
                thisPanel.Transparency = 0.7f;
                labelPanels.Add(thisPanel);
                scene.UIRenderer.Add2DComponent(labelPanels[i]);
                labelPanels[i].AddChild(attributeLabels[i]);
            }

            // Create the main panel which holds all other GUI components
            /*frame = new G2DPanel();
            frame.Bounds = new Rectangle(615, 350, 170, 110);
            frame.Border = GoblinEnums.BorderFactory.LineBorder;*/
        }

        public void setScene(ref Scene s)
        {
            scene = s;
        }

        public void resetObjectColors()
        {
            //Recolor attributes
            for (int i = 0; i < 8; i++)
            {
                attributeBoxes[i].Model = new Box(3);
                attributeBoxes[i].Material.Diffuse = colorPalette[i];
                attributeTransNodes[i].Translation = new Vector3(-32.0f + 9.25f * i, 11.0f, 6.0f);
            }
            
            if (attributeCurrentlyBeingViewed == NOTHING)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (i == indexOfBuildingBeingSelected)
                    {
                        buildingGeomNodes[i].Material.Diffuse = Color.Goldenrod.ToVector4();
                    }
                    else
                    {
                        buildingGeomNodes[i].Material.Diffuse = Color.White.ToVector4();
                    }
                }

                leftConeGeomNode.Material.Emissive = Color.RosyBrown.ToVector4();
                rightConeGeomNode.Material.Emissive = Color.RosyBrown.ToVector4();

                gradientPanel.Visible = false;
            }
            else
            {
                Attribute currentlyViewedAttribute = attributes[attributeCurrentlyBeingViewed];

                if (currentlyViewedAttribute.name != "UNDEFINED")
                {
                    for (int i = 0; i < 8; i++) //for each building
                    {
                        //PUT GRADIENT STUFF HERE!!!!!!!!!
                        //buildingGeomNodes[i].Material.Diffuse = Color.White.ToVector4();

                        //if this building does not have a value assigned for the currently viewed attribute
                        Attribute buildingAttribute = buildingList[i].getAttribute(currentlyViewedAttribute.name);
                        if (buildingAttribute == null)
                        {
                            buildingGeomNodes[i].Material.Diffuse = Color.White.ToVector4();
                        }
                        else
                        {
                            buildingGeomNodes[i].Material.Diffuse = getGradientColor(buildingAttribute.value,
                                currentlyViewedAttribute.min, currentlyViewedAttribute.max);
                        }
                    }

                    gradientPanel.Visible = true;
                    gradientNameLabel.Text = currentlyViewedAttribute.name + ":";
                    if (indexOfBuildingBeingSelected != NOTHING)
                    {
                        gradientNameLabel2.Text = "(Value for " + buildingList[indexOfBuildingBeingSelected].getBuildingName() + ": " + buildingList[indexOfBuildingBeingSelected].getAttributeValue(currentlyViewedAttribute.name) + ")";
                        gradientNameLabel2.Visible = true;
                    }
                    else
                    {
                        gradientNameLabel2.Visible = false;
                    }
                    firstGradientValueLabel.Text = "" + currentlyViewedAttribute.min;
                    thirdGradientValueLabel.Text = "" + (int) ((currentlyViewedAttribute.min + currentlyViewedAttribute.max) / 2.0f);
                    fifthGradientValueLabel.Text = "" + currentlyViewedAttribute.max;
                }
                else
                {
                    for (int i = 0; i < 8; i++) //for each building
                    {
                        if (i == indexOfBuildingBeingSelected)
                        {
                            buildingGeomNodes[i].Material.Diffuse = Color.Goldenrod.ToVector4();
                        }
                        else
                        {
                            buildingGeomNodes[i].Material.Diffuse = Color.White.ToVector4();
                        }
                    }
                }
                
                

                for (int i = 0; i < 8; i++)
                {
                    attributeBoxes[i].Material.Diffuse = colorPalette[i];
                }

                attributeBoxes[attributeCurrentlyBeingViewed].Model = new Box(7);
                attributeTransNodes[attributeCurrentlyBeingViewed].Translation = new Vector3(-32.0f + 9.25f * attributeCurrentlyBeingViewed, 11.0f, 8.0f);
                
                
                leftConeGeomNode.Material.Emissive = Color.RosyBrown.ToVector4();
                rightConeGeomNode.Material.Emissive = Color.RosyBrown.ToVector4();
            }
        }

        public void highlight(int index, int typeOfGeomNode, Color color)
        {
            if (typeOfGeomNode == BUILDING)
            {
                buildingGeomNodes[index].Material.Diffuse = color.ToVector4();
            }
            else if(typeOfGeomNode == ATTRIBUTE)
            {
                attributeBoxes[index].Material.Diffuse = color.ToVector4();
            }
            else if (typeOfGeomNode == LEFT_CONE)
            {
                leftConeGeomNode.Material.Emissive = color.ToVector4();
            }
            else //if (typeOfGeomNode == RIGHT_CONE)
            {
                rightConeGeomNode.Material.Emissive = color.ToVector4();
            }
        }

        public Vector4 getGradientColor(double value, double min, double max)
        {
            Vector4 returnColor;

            if (value <= min)
            {
                returnColor = Color.Green.ToVector4();
            }
            else if (value >= max)
            {
                returnColor = Color.Red.ToVector4();
            }
            else
            {
                if (value <= (min + max) / 2.0f)
                {
                    double range = ((min + max) / 2.0f) - min;
                    double valueAboveMin = value - min;
                    float weightOfMax = (float) (valueAboveMin / range);


                    returnColor.X = ((1 - weightOfMax) * Color.LightGreen.ToVector4().X) + (weightOfMax * Color.Yellow.ToVector4().X);
                    returnColor.Y = ((1 - weightOfMax) * Color.LightGreen.ToVector4().Y) + (weightOfMax * Color.Yellow.ToVector4().Y);
                    returnColor.Z = ((1 - weightOfMax) * Color.LightGreen.ToVector4().Z) + (weightOfMax * Color.Yellow.ToVector4().Z);
                    returnColor.W = 1;
                }
                else
                {
                    double range = max - ((min + max) / 2.0f);
                    double valueAboveMedian = value - ((min + max) / 2.0f);
                    float weightOfMax = (float)(valueAboveMedian / range);


                    returnColor.X = ((1 - weightOfMax) * Color.Yellow.ToVector4().X) + (weightOfMax * Color.Red.ToVector4().X);
                    returnColor.Y = ((1 - weightOfMax) * Color.Yellow.ToVector4().Y) + (weightOfMax * Color.Red.ToVector4().Y);
                    returnColor.Z = ((1 - weightOfMax) * Color.Yellow.ToVector4().Z) + (weightOfMax * Color.Red.ToVector4().Z);
                    returnColor.W = 1;
                }
            }

            return returnColor;
        }


        public void createGradient2DGUI()
        {
            gradientPanel.Bounds = new Rectangle(5, 495, 173, 100);
            gradientPanel.Border = GoblinEnums.BorderFactory.LineBorder;
            gradientPanel.Transparency = 0.4f;  // Ranges from 0 (fully transparent) to 1 (fully opaque)

            gradientNameLabel = new G2DLabel("Gradient Name");
            gradientNameLabel.TextFont = uiFont;
            gradientNameLabel.Bounds = new Rectangle(60, 5, 60, 20);
            gradientNameLabel.Text = "";
            gradientNameLabel.Visible = true;
            gradientNameLabel.HorizontalAlignment = GoblinEnums.HorizontalAlignment.Center;

            gradientNameLabel2 = new G2DLabel("Gradient Name 2");
            gradientNameLabel2.TextFont = uiFont;
            gradientNameLabel2.Bounds = new Rectangle(60, 25, 60, 20);
            gradientNameLabel2.Text = "";
            gradientNameLabel2.Visible = false;
            gradientNameLabel2.HorizontalAlignment = GoblinEnums.HorizontalAlignment.Center;

            firstGradientLabel = new G2DLabel("1st Gradient");
            firstGradientLabel.TextFont = uiFont;
            firstGradientLabel.Bounds = new Rectangle(25, 55, 20, 20);
            firstGradientLabel.BackgroundColor = Color.Green;
            firstGradientLabel.DrawBackground = true;
            firstGradientLabel.Text = "";
            firstGradientLabel.Visible = true;

            firstGradientValueLabel = new G2DLabel("1st Gradient Value");
            firstGradientValueLabel.TextFont = uiFont;
            firstGradientValueLabel.Bounds = new Rectangle(15, 75, 40, 20);
            firstGradientValueLabel.Text = "";
            firstGradientValueLabel.TextColor = Color.Green;
            firstGradientValueLabel.Visible = true;
            firstGradientValueLabel.HorizontalAlignment = GoblinEnums.HorizontalAlignment.Center;

            secondGradientLabel = new G2DLabel("1st Gradient");
            secondGradientLabel.TextFont = uiFont;
            secondGradientLabel.Bounds = new Rectangle(50, 55, 20, 20);
            secondGradientLabel.BackgroundColor = Color.YellowGreen;
            secondGradientLabel.DrawBackground = true;
            secondGradientLabel.Text = "";
            secondGradientLabel.Visible = true;

            thirdGradientLabel = new G2DLabel("1st Gradient");
            thirdGradientLabel.TextFont = uiFont;
            thirdGradientLabel.Bounds = new Rectangle(75, 55, 20, 20);
            thirdGradientLabel.BackgroundColor = Color.Yellow;
            thirdGradientLabel.DrawBackground = true;
            thirdGradientLabel.Text = "";
            thirdGradientLabel.Visible = true;

            thirdGradientValueLabel = new G2DLabel("1st Gradient Value");
            thirdGradientValueLabel.TextFont = uiFont;
            thirdGradientValueLabel.Bounds = new Rectangle(65, 75, 40, 20);
            thirdGradientValueLabel.Text = "";
            thirdGradientValueLabel.TextColor = Color.Yellow;
            thirdGradientValueLabel.Visible = true;
            thirdGradientValueLabel.HorizontalAlignment = GoblinEnums.HorizontalAlignment.Center;

            fourthGradientLabel = new G2DLabel("1st Gradient");
            fourthGradientLabel.TextFont = uiFont;
            fourthGradientLabel.Bounds = new Rectangle(100, 55, 20, 20);
            fourthGradientLabel.BackgroundColor = Color.Orange;
            fourthGradientLabel.DrawBackground = true;
            fourthGradientLabel.Text = "";
            fourthGradientLabel.Visible = true;

            fifthGradientLabel = new G2DLabel("1st Gradient");
            fifthGradientLabel.TextFont = uiFont;
            fifthGradientLabel.Bounds = new Rectangle(125, 55, 20, 20);
            fifthGradientLabel.BackgroundColor = Color.Red;
            fifthGradientLabel.DrawBackground = true;
            fifthGradientLabel.Text = "";
            fifthGradientLabel.Visible = true;

            fifthGradientValueLabel = new G2DLabel("1st Gradient Value");
            fifthGradientValueLabel.TextFont = uiFont;
            fifthGradientValueLabel.Bounds = new Rectangle(115, 75, 40, 20);
            fifthGradientValueLabel.Text = "";
            fifthGradientValueLabel.TextColor = Color.Red;
            fifthGradientValueLabel.Visible = true;
            fifthGradientValueLabel.HorizontalAlignment = GoblinEnums.HorizontalAlignment.Center;

            gradientPanel.AddChild(gradientNameLabel);
            gradientPanel.AddChild(gradientNameLabel2);
            gradientPanel.AddChild(firstGradientLabel);
            gradientPanel.AddChild(secondGradientLabel);
            gradientPanel.AddChild(thirdGradientLabel);
            gradientPanel.AddChild(fourthGradientLabel);
            gradientPanel.AddChild(fifthGradientLabel);
            gradientPanel.AddChild(firstGradientValueLabel);
            gradientPanel.AddChild(thirdGradientValueLabel);
            gradientPanel.AddChild(fifthGradientValueLabel);

            scene.UIRenderer.Add2DComponent(gradientPanel);
            gradientPanel.Visible = false;
        }


        public void createBuilding2DGUI()
        {
            markerLabels = new List<G2DLabel>();
            
            
            // Create the main panel which holds all other GUI components
            buildingInfoPanel = new G2DPanel();
            buildingInfoPanel.Bounds = new Rectangle(530, 5, 265, 475);
            //frame.Bounds = new Rectangle(615, 350, 170, 220);
            buildingInfoPanel.Border = GoblinEnums.BorderFactory.LineBorder;
            buildingInfoPanel.Transparency = 0.4f;  // Ranges from 0 (fully transparent) to 1 (fully opaque)

            nameLabel = new G2DLabel("Name");
            nameLabel.TextFont = uiFont;
            nameLabel.Bounds = new Rectangle(25, 5, 230, 20);
            nameLabel.Text = "Name:";
            nameLabel.Visible = true;

            nameField = new G2DTextField("Enter text here", 200);
            nameField.TextFont = uiFont;
            nameField.Text = "Enter text here";
            nameField.SelectAll();
            nameField.Enabled = false;
            nameField.Editable = false;
            nameField.Bounds = new Rectangle(25, 25, 230, 20);
            nameField.Visible = true;
            nameField.setFocused(false);

            addressLabel = new G2DLabel("Address:");
            addressLabel.TextFont = uiFont;
            addressLabel.Bounds = new Rectangle(25, 45, 230, 20);
            addressLabel.Text = "Address:";
            addressLabel.Visible = true;

            addressField = new G2DTextField("Enter text here", 200);
            addressField.TextFont = uiFont;
            addressField.Text = "Enter text here";
            addressField.SelectAll();
            addressField.Enabled = false;
            addressField.Editable = false;
            addressField.Bounds = new Rectangle(25, 65, 230, 20);
            addressField.Visible = true;
            addressField.setFocused(false);

            storiesLabel = new G2DLabel("Number of Stories:");
            storiesLabel.TextFont = uiFont;
            storiesLabel.Bounds = new Rectangle(25, 85, 230, 20);
            storiesLabel.Text = "Number of Stories:";
            storiesLabel.Visible = true;

            storiesField = new G2DTextField("Enter text here", 200);
            storiesField.TextFont = uiFont;
            storiesField.Text = "Enter text here";
            storiesField.SelectAll();
            storiesField.Enabled = false;
            storiesField.Editable = false;
            storiesField.Bounds = new Rectangle(25, 105, 230, 20);
            storiesField.Visible = true;
            storiesField.setFocused(false);

            toxicSitesLabel = new G2DLabel("Toxic Site Report:");
            toxicSitesLabel.TextFont = uiFont;
            toxicSitesLabel.Bounds = new Rectangle(25, 125, 230, 20);
            toxicSitesLabel.Text = "Toxic Site Report:";
            toxicSitesLabel.Visible = true;

            toxicSitesField = new G2DTextField("Enter text here", 200);
            toxicSitesField.TextFont = uiFont;
            toxicSitesField.Text = "Enter text here";
            toxicSitesField.SelectAll();
            toxicSitesField.Enabled = false;
            toxicSitesField.Editable = false;
            toxicSitesField.Bounds = new Rectangle(25, 145, 230, 20);
            toxicSitesField.Visible = true;
            toxicSitesField.setFocused(false);

            airRightsLabel = new G2DLabel("Air Rights Information:");
            airRightsLabel.TextFont = uiFont;
            airRightsLabel.Bounds = new Rectangle(25, 165, 230, 20);
            airRightsLabel.Text = "Air Rights Information:";
            airRightsLabel.Visible = true;

            airRightsField = new G2DTextField("Enter text here", 200);
            airRightsField.TextFont = uiFont;
            airRightsField.Text = "Enter text here";
            airRightsField.SelectAll();
            airRightsField.Enabled = false;
            airRightsField.Editable = false;
            airRightsField.Bounds = new Rectangle(25, 185, 230, 20);
            airRightsField.Visible = true;
            airRightsField.setFocused(false);

            yearBuiltLabel = new G2DLabel("Year Constructed:");
            yearBuiltLabel.TextFont = uiFont;
            yearBuiltLabel.Bounds = new Rectangle(25, 205, 230, 20);
            yearBuiltLabel.Text = "Year Constructed:";
            yearBuiltLabel.Visible = true;

            yearBuiltField = new G2DTextField("Enter text here", 200);
            yearBuiltField.TextFont = uiFont;
            yearBuiltField.Text = "Enter text here";
            yearBuiltField.SelectAll();
            yearBuiltField.Enabled = false;
            yearBuiltField.Editable = false;
            yearBuiltField.Bounds = new Rectangle(25, 225, 230, 20);
            yearBuiltField.Visible = true;
            yearBuiltField.setFocused(false);

            typeLabel = new G2DLabel("Zone Type:");
            typeLabel.TextFont = uiFont;
            typeLabel.Bounds = new Rectangle(25, 245, 230, 20);
            typeLabel.Text = "Zone Type:";
            typeLabel.Visible = true;

            typeField = new G2DTextField("Enter text here", 200);
            typeField.TextFont = uiFont;
            typeField.Text = "Enter text here";
            typeField.SelectAll();
            typeField.Enabled = false;
            typeField.Editable = false;
            typeField.Bounds = new Rectangle(25, 265, 230, 20);
            typeField.Visible = true;
            typeField.setFocused(false);

            classLabel = new G2DLabel("Building Class:");
            classLabel.TextFont = uiFont;
            classLabel.Bounds = new Rectangle(25, 285, 230, 20);
            classLabel.Text = "Building Class:";
            classLabel.Visible = true;

            classField = new G2DTextField("Enter text here", 200);
            classField.TextFont = uiFont;
            classField.Text = "Enter text here";
            classField.SelectAll();
            classField.Enabled = false;
            classField.Editable = false;
            classField.Bounds = new Rectangle(25, 305, 230, 20);
            classField.Visible = true;
            classField.setFocused(false);

            saleDateLabel = new G2DLabel("Sale Date:");
            saleDateLabel.TextFont = uiFont;
            saleDateLabel.Bounds = new Rectangle(25, 325, 230, 20);
            saleDateLabel.Text = "Sale Date:";
            saleDateLabel.Visible = true;

            saleDateField = new G2DTextField("Enter text here", 200);
            saleDateField.TextFont = uiFont;
            saleDateField.Text = "Enter text here";
            saleDateField.SelectAll();
            saleDateField.Enabled = false;
            saleDateField.Editable = false;
            saleDateField.Bounds = new Rectangle(25, 345, 230, 20);
            saleDateField.Visible = true;
            saleDateField.setFocused(false);

            descriptionLabel = new G2DLabel("Notes:");
            descriptionLabel.TextFont = uiFont;
            descriptionLabel.Bounds = new Rectangle(25, 365, 230, 20);
            descriptionLabel.Text = "Notes:";
            descriptionLabel.Visible = true;

            descriptionField = new G2DTextField("Enter text here", 200);
            descriptionField.TextFont = uiFont;
            descriptionField.Text = "Enter text here";
            descriptionField.SelectAll();
            descriptionField.Enabled = false;
            descriptionField.Editable = false;
            descriptionField.Bounds = new Rectangle(25, 385, 230, 80);
            descriptionField.Visible = true;
            descriptionField.setFocused(false);







            /*G2DTextField buildingInfoField = new G2DTextField("Enter text here", 200);
            buildingInfoField.TextFont = uiFont;
            buildingInfoField.Text = "Enter text here";
            buildingInfoField.SelectAll();
            buildingInfoField.Enabled = true;
            buildingInfoField.Editable = true;
            buildingInfoField.Bounds = new Rectangle(5, 25, 230, 90);
            buildingInfoField.Visible = true;
            buildingInfoField.setFocused(true);*/



            buildingInfoPanel.AddChild(nameLabel);
            buildingInfoPanel.AddChild(nameField);
            buildingInfoPanel.AddChild(addressLabel);
            buildingInfoPanel.AddChild(addressField);
            buildingInfoPanel.AddChild(storiesLabel);
            buildingInfoPanel.AddChild(storiesField);
            buildingInfoPanel.AddChild(toxicSitesLabel);
            buildingInfoPanel.AddChild(toxicSitesField);
            buildingInfoPanel.AddChild(airRightsLabel);
            buildingInfoPanel.AddChild(airRightsField);
            buildingInfoPanel.AddChild(yearBuiltLabel);
            buildingInfoPanel.AddChild(yearBuiltField);
            buildingInfoPanel.AddChild(typeLabel);
            buildingInfoPanel.AddChild(typeField);
            buildingInfoPanel.AddChild(classLabel);
            buildingInfoPanel.AddChild(classField);
            buildingInfoPanel.AddChild(saleDateLabel);
            buildingInfoPanel.AddChild(saleDateField);
            buildingInfoPanel.AddChild(descriptionLabel);
            buildingInfoPanel.AddChild(descriptionField);

            for (int i = 0; i < 10; i++)
            {
                G2DLabel thisMarkerLabel = new G2DLabel("Marker " + i);
                thisMarkerLabel.TextFont = uiFont;
                thisMarkerLabel.Bounds = new Rectangle(5, 25 + i * 40, 230, 20);
                thisMarkerLabel.Text = ">>";
                thisMarkerLabel.Visible = false;
                markerLabels.Add(thisMarkerLabel);
            }
            for (int i = 0; i < 10; i++)
            {
                markerLabels[i].Visible = false;
                buildingInfoPanel.AddChild(markerLabels[i]);
            }
            scene.UIRenderer.Add2DComponent(buildingInfoPanel);
            buildingInfoPanel.Visible = false;


            // Create an action listener
            //TestActionListener listener = new TestActionListener();
        }

        public void refreshInfoPanel()
        {
            nameField.Text = buildingList[indexOfBuildingBeingSelected].getBuildingName();
            addressField.Text = buildingList[indexOfBuildingBeingSelected].getAddress();
            storiesField.Text = buildingList[indexOfBuildingBeingSelected].getStories();
            toxicSitesField.Text = buildingList[indexOfBuildingBeingSelected].getToxicSites();
            airRightsField.Text = buildingList[indexOfBuildingBeingSelected].getAirRights();
            yearBuiltField.Text = buildingList[indexOfBuildingBeingSelected].getYearBuilt();
            typeField.Text = buildingList[indexOfBuildingBeingSelected].getType();
            classField.Text = buildingList[indexOfBuildingBeingSelected].getClass();
            saleDateField.Text = buildingList[indexOfBuildingBeingSelected].getSaleDate();
            descriptionField.Text = buildingList[indexOfBuildingBeingSelected].getDescription();

            buildingInfoIndex = 0;

            buildingInfoPanel.Visible = true;

            markerLabels[0].Visible = false;
            for (int i = 0; i < 10; i++)
            {
                markerLabels[i].Visible = false;
            }

            buildingInfoPanel.Visible = true;
            for (int i = 0; i < 10; i++)
            {
                markerLabels[i].Visible = false;
            }
            markerLabels[0].Visible = false;
        }

    }
}
