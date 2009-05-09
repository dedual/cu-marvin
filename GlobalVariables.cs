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
        public GraphicsDeviceManager graphics;

        public Scene scene;
        public MarkerNode groundMarkerNode, toolbar1MarkerNode;
        public List<GeometryNode> buildings;
        public TransformNode parentTrans;
        public TransformNode toolBar1OccluderTransNode;
        public GeometryNode pointerTip;
        public GeometryNode pointerSegment;
        public GeometryNode toolbar1Node;
        public Material pointerMaterial;
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
