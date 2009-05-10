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
    public class GlobalVariables
    {
        public GraphicsDeviceManager graphics; //Graphics Device
        //public ContentManager Content;

        public Scene scene; //Scene
        public MarkerNode groundMarkerNode, toolbar1MarkerNode; //Ground and Pointer Markers
    //    public MarkerNode block1MarkerNode, block2MarkerNode; //Markers for each block
        public List<GeometryNode> buildings;
        public Texture2D notebookTopTexture;        public TransformNode parentTrans;
        public TransformNode toolBar1OccluderTransNode;
        public TransformNode notebookBoxTransNode;


        public GeometryNode pointerTip;
        public GeometryNode pointerSegment;
        public GeometryNode toolbar1Node;
        public GeometryNode notebookBoxNode;

        public Material pointerMaterial;
        public Material notebookTopMaterial;

        public Model notebookModel;

        public Vector4 ORIGIN = new Vector4(0, 0, 0, 1);
        public String selectedBuildingName = null;

        public float y_shift = -62;
        public float x_shift = -28.0f;

        public GlobalVariables()
        {
            //Do nothing
            //graphics = new GraphicsDeviceManager(this);
        }
    }
}
