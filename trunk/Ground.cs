using System;
using System.Collections.Generic;
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
using GoblinXNA.Device.Capture;
using GoblinXNA.Device.Vision;
using GoblinXNA.Device.Vision.Marker;
using GoblinXNA.Device.Util;
using GoblinXNA.Physics;
using GoblinXNA.Helpers;

namespace Manhattanville
{
    class Ground: Microsoft.Xna.Framework.Game
    {
        //Nodes and trackers
        MarkerNode groundNode; //Building Marker
        GeometryNode GroundGeomNode; //Geometry Node
        Material groundMaterial; //Building Material
        TransformNode groundTransNode; //Transform node for this building 
        ARTagTracker _tracker; //ARTracker information

        public Ground()
        {
        }

    }
}
