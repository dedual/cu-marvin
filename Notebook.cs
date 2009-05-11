using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class Notebook
    {
        public GlobalVariables global;

        public Notebook(ref GlobalVariables g)
        {
            global = g;
        }

        public void createNotebook()
        {
            // Create a geometry node of a loaded notebook model
            global.notebookBoxNode = new GeometryNode("Notebook");
            global.notebookBoxNode.Model = global.notebookModel;
            // This notebook model has material definitions in the model file, so instead
            // of creating a material node for this police car model, we simply use its internal materials
            global.notebookBoxNode.Model.UseInternalMaterials = true;

            // Create a transform node to define the transformation of the notebook box
            global.notebookBoxTransNode = new TransformNode();
            Matrix notebookRotation = (Matrix.CreateRotationZ((float)(Math.PI / 2.0)));
            global.notebookBoxTransNode.Rotation = Quaternion.CreateFromRotationMatrix(notebookRotation);
            global.notebookBoxTransNode.Translation = new Vector3(0, 30, 0);
            global.notebookBoxTransNode.Scale = new Vector3(3, 3, 3);

            global.notebookBoxNode.Physics.Shape = GoblinXNA.Physics.ShapeType.ConvexHull;
            global.notebookBoxNode.Physics.Pickable = true;
            global.notebookBoxNode.AddToPhysicsEngine = true;

            // Now add the box nodes to the scene graph in the appropriate order
            global.groundMarkerNode.AddChild(global.notebookBoxTransNode);
            global.notebookBoxTransNode.AddChild(global.notebookBoxNode);

            /*
             * Now we define the notebookShowcase nodes to stick the building model & rotator cones in 
             */
            global.notebookShowcaseTransNode = new TransformNode();
            global.notebookShowcaseGeomNode = new GeometryNode();
            global.notebookShowcaseModelTransNode = new TransformNode();
            global.notebookBoxTransNode.AddChild(global.notebookShowcaseTransNode);
            global.leftConeTransNode = new TransformNode();
            global.rightConeTransNode = new TransformNode();
            float coneHeight = 3.0f;

            Matrix leftConeRotation = (Matrix.CreateRotationZ((float)-Math.PI/2.0f));
            global.leftConeTransNode.Scale = new Vector3(0.35f, 0.35f, 0.35f);
            global.leftConeTransNode.Rotation = Quaternion.CreateFromRotationMatrix(leftConeRotation);
            global.leftConeTransNode.Translation = new Vector3(-14.0f, -3.0f, 6.0f);

            Matrix rightConeRotation = (Matrix.CreateRotationZ((float)Math.PI/2.0f));
            global.rightConeTransNode.Scale = new Vector3(0.35f, 0.35f, 0.35f);
            global.rightConeTransNode.Rotation = Quaternion.CreateFromRotationMatrix(rightConeRotation);
            global.rightConeTransNode.Translation = new Vector3(-27.0f, -3.0f, 6.0f);

            global.leftConeGeomNode = new GeometryNode("Left Cone");
            global.leftConeGeomNode.Model = new Cylinder(1.8f, 0.05f, coneHeight, 12);
            Material leftConeMaterial = new Material();
            leftConeMaterial.Emissive = Color.RosyBrown.ToVector4();
            global.leftConeGeomNode.Material = leftConeMaterial;

            global.rightConeGeomNode = new GeometryNode("Right Cone");
            global.rightConeGeomNode.Model = new Cylinder(1.8f, 0.05f, coneHeight, 12);
            Material rightConeMaterial = new Material();
            rightConeMaterial.Emissive = Color.RosyBrown.ToVector4();
            global.rightConeGeomNode.Material = rightConeMaterial;

            global.notebookShowcaseTransNode.AddChild(global.notebookShowcaseModelTransNode);
            global.notebookShowcaseTransNode.AddChild(global.leftConeTransNode);
            global.leftConeTransNode.AddChild(global.leftConeGeomNode);
            global.notebookShowcaseTransNode.AddChild(global.rightConeTransNode);
            global.rightConeTransNode.AddChild(global.rightConeGeomNode);

            /*
             *  Now it's time to create the box buttons on the right portion of the notebook
             */
            global.attributeBoxes = new List<GeometryNode>();
            global.attributeTransNodes = new List<TransformNode>();
            global.attributeMaterials = new List<Material>();
            GeometryNode thisAttributeBox;
            Material thisMaterial;
            TransformNode thisAttributeTransNode;
            Vector3 thisTranslationVector;
            //for(int i = 0; i < global.NUMBER_OF_ATTRIBUTES; i++)
            for(int i = 0; i < 8; i++)
            {
                thisAttributeBox = new GeometryNode("Attribute " + i);
                thisAttributeBox.Model = new Box(3);
                thisMaterial = new Material();
                thisMaterial.Diffuse = global.colorPalette[i];
                //thisMaterial.Specular = Color.White.ToVector4();
                //thisMaterial.SpecularPower = 10;
                global.attributeMaterials.Add(thisMaterial);
                thisAttributeBox.Material = global.attributeMaterials[i];
                thisAttributeTransNode = new TransformNode();

                thisTranslationVector = new Vector3(-32.0f+9.25f*i, 11.0f, 6.0f); //reading down notebook is +x direction
                                                                                 //reading to the right is +y direction
                thisAttributeTransNode.Translation = thisTranslationVector;

                global.attributeBoxes.Add(thisAttributeBox);
                global.attributeTransNodes.Add(thisAttributeTransNode);
                global.groundMarkerNode.AddChild(global.attributeTransNodes[i]);
                global.attributeTransNodes[i].AddChild(global.attributeBoxes[i]);
            }
        }
    }
}