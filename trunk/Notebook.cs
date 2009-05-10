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
            global.notebookBoxNode.Physics.Pickable = false;
            global.notebookBoxNode.AddToPhysicsEngine = true;

            // Now add the box nodes to the scene graph in the appropriate order
            global.groundMarkerNode.AddChild(global.notebookBoxTransNode);
            global.notebookBoxTransNode.AddChild(global.notebookBoxNode);

            /*
             *  Now it's time to create the box buttons on the right potion of the notebook
             */

        }
    }
}