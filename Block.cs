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
    class Block : Microsoft.Xna.Framework.Game 
    { 
        List<Building> buildingsInBlocks; //List of buildings
        
        //Markers
        MarkerNode blockNode;
        TransformNode blockTransNode;
        
        ARTagTracker _tracker; //ARTracker information

        public Block(string[] names)
        {
            float scale = 0.0073f;
            blockTransNode = new TransformNode();
            blockTransNode.Scale = Vector3.One * scale;
            blockTransNode.Translation = new Vector3(-33.5f, -54.25f, 0);
            blockTransNode.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.PiOver2);
            blockNode.AddChild(blockTransNode);

            foreach (string s in names)
            {
                //Note, this needs to be changed so we are able to add different buildings to a marker

                Building tempBuilding = new Building(s);
                tempBuilding.loadBuildingModel(true, 1.0); //change scale factor if necessary
                buildingsInBlocks.Add(tempBuilding);
                blockTransNode.AddChild(tempBuilding.getBuildingTransNode());
            }
        }
        public Block(string[] names, float scale)
        {
            blockTransNode = new TransformNode();
            blockTransNode.Scale = Vector3.One * scale;
            blockTransNode.Translation = new Vector3(-33.5f, -54.25f, 0);
            blockTransNode.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.PiOver2);
            blockNode.AddChild(blockTransNode);

            foreach (string s in names)
            {
                //Note, this needs to be changed so we are able to add different buildings to a marker

                Building tempBuilding = new Building(s);
                tempBuilding.loadBuildingModel(true, 1.0); //change scale factor if necessary
                buildingsInBlocks.Add(tempBuilding);
                blockTransNode.AddChild(tempBuilding.getBuildingTransNode());
            }
        }
        public Block()
        {
        }
        //Associate building marker with an ARTag
        public void setupBlockMarker(int width, int height, System.String markerName)
        {
            _tracker = new ARTagTracker();
            //Must modify these parameters
            _tracker.InitTracker(410.0f, 410.0f, width, height, false, "manhattanville.cf");

            //setup marker
            blockNode = new MarkerNode(_tracker, markerName);
            blockNode.Smoother = new DESSmoother(0.8f, 0.8f); //uncomment, if necessary
        }
        public void addBuilding(Building _building)
        {
            buildingsInBlocks.Add(_building);
        }
        //Detect presence of our marker
        public bool isMarkerPresent()
        {
            return blockNode.MarkerFound;
        }

        public void setBuildings(List<Building> buildings)
        {
            buildingsInBlocks = buildings;
        }
        public void placeBuildings()
        {

        }
    }
}
