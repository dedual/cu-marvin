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
        public Color labelColor = Color.Red;
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


        public Block block1, block2;

        public float y_shift = -62;
        public float x_shift = -28.0f;
        public float scale;

        public GlobalVariables()
        {
            //Do nothing
            //graphics = new GraphicsDeviceManager(this);

            colorPalette[0] = Color.Red.ToVector4();
            colorPalette[1] = Color.Green.ToVector4();
            colorPalette[2] = Color.Yellow.ToVector4();
            colorPalette[3] = Color.Blue.ToVector4();
            colorPalette[4] = Color.DarkOrange.ToVector4();
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

    }
}
