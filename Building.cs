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
    class Building : Microsoft.Xna.Framework.Game
    {
        //Nodes and trackers
        MarkerNode buildingNode; //Building Marker
        GeometryNode buildingGeomNode; //Geometry Node
        Material buildingMaterial; //Building Material
        TransformNode buildingTransNode; //Transform node for this building 
        ARTagTracker _tracker; //ARTracker information
       
        //Building properties
        System.String buildingName; //Building Name
        System.String buildingInfo; //Building info

        //Constructors
        public Building()
        {

        }
        public Building(System.String name)
        {
            buildingName = name;
        }

        //Associate building marker with an ARTag
        public void setupBuildingMarker(int width, int height, System.String markerName)
        {
            _tracker = new ARTagTracker();
            //Must modify these parameters
            _tracker.InitTracker(410.0f, 410.0f, width, height, false, "manhattanville.cf");

            //setup marker
            buildingNode = new MarkerNode(_tracker, markerName);
            //buildingMarker.Smoother = new DESSmoother(0.8f, 0.8f); //uncomment, if necessary
        }

        public MarkerNode getBuildingNode()
        {
            return buildingNode;
        }

        public void setTransforms(TransformNode thisTransform)
        {
            buildingTransNode = thisTransform;
            buildingTransNode.AddChild(buildingNode);
        }

        //Get & set building name
        public System.String getBuildingName()
        {
            return buildingName;
        }
        public void setBuildingName(System.String _name)
        {
            buildingName = _name;
        }

        //Detect presence of our marker
        public bool isMarkerPresent()
        {
            return buildingNode.MarkerFound;
        }

        public void setBuildingOffset()
        {
        }

    }
}
