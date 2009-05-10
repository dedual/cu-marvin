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
            // Create a geometry node with a model of a box for the notebook 
            //global.notebookBoxNode = new GeometryNode("Box");
            //global.notebookBoxNode.Model = new Box(50, 100, 10);

            // Loads a textured model of a notebook
            //ModelLoader loader = new ModelLoader();
            //Model notebookModel = (Model)loader.Load("", "opened_book");

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
            global.notebookBoxNode.Physics.Pickable = false;
            global.notebookBoxNode.AddToPhysicsEngine = true;

            // Create a material to apply to the box model
            //global.notebookTopMaterial = new Material();
            //global.notebookTopMaterial.Diffuse = Color.White.ToVector4();
            //global.notebookTopMaterial.Specular = Color.White.ToVector4();
            //global.notebookTopMaterial.SpecularPower = 10;
            //global.notebookTopMaterial.Texture = tex;
            //Console.WriteLine("Root directory: " + global.Content.RootDirectory);
            //Console.WriteLine("Content: " + global.Content.ToString());

            //global.notebookBoxNode.Material = global.notebookTopMaterial;

            // Now add the box nodes to the scene graph in the appropriate order
            global.groundMarkerNode.AddChild(global.notebookBoxTransNode);
            global.notebookBoxTransNode.AddChild(global.notebookBoxNode);
        }
    }
}