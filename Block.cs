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
    public class Block : Microsoft.Xna.Framework.Game 
    { 
        List<Building> buildingsInBlocks; //List of buildings
        
        //Markers
        MarkerNode blockNode;
        TransformNode blockTransNode;

        public Block(MarkerNode markerNode, string[] names)
        {
            buildingsInBlocks = new List<Building>();

            blockNode = markerNode;

            float scale = 0.0073f;
            blockTransNode = new TransformNode();
            blockTransNode.Scale = Vector3.One * scale;
 //           blockTransNode.Translation = new Vector3(0.0f, -64.25f, 0.0f);
 //           blockTransNode.Translation = new Vector3(-33.5f, -54.25f, 0);
            blockTransNode.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.PiOver2);
            blockNode.AddChild(blockTransNode);

            foreach (string s in names)
            {
                //Note, this needs to be changed so we are able to add different buildings to a marker

                Building tempBuilding = new Building(s);
                tempBuilding.loadBuildingModel(true, 1.0f); //change scale factor if necessary
                buildingsInBlocks.Add(tempBuilding);
                blockTransNode.AddChild(tempBuilding.getBuildingNode());
                //  blockTransNode.Scale = new Vector3(blockTransNode.Scale.X * 1.0f, blockTransNode.Scale.Y * 1.0f, blockTransNode.Scale.Z * 1.0f);
            }
        }
        public Block(MarkerNode markerNode, string[] names, float scale)
        {
            buildingsInBlocks = new List<Building>();

            blockNode = markerNode;

            blockTransNode = new TransformNode();
            blockTransNode.Scale = Vector3.One*scale;
            blockTransNode.Translation = new Vector3(0.0f, -64.25f, 0.0f); 
            //blockTransNode.Translation = new Vector3(-33.5f, -54.25f, 0);
            blockTransNode.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.PiOver2);
            blockNode.AddChild(blockTransNode);

            foreach (string s in names)
            {
                //Note, this needs to be changed so we are able to add different buildings to a marker

                Building tempBuilding = new Building(s);
                tempBuilding.loadBuildingModel(true, 1.0f); //change scale factor if necessary
                buildingsInBlocks.Add(tempBuilding);
                blockTransNode.AddChild(tempBuilding.getBuildingNode());
              //  blockTransNode.Scale = new Vector3(blockTransNode.Scale.X * 1.0f, blockTransNode.Scale.Y * 1.0f, blockTransNode.Scale.Z * 1.0f);
            //    blockNode.AddChild(blockTransNode);
            }
        }
        public Block()
        {
            float scale = 0.0073f;
         //   float scale = 0.073f;

            buildingsInBlocks = new List<Building>();
            blockTransNode = new TransformNode();
            blockTransNode.Scale = Vector3.One*scale;
            blockTransNode.Translation = new Vector3(0.0f, -64.25f, 0.0f);
          //  blockTransNode.Translation = new Vector3(33.5f, -54.25f, 0.0f);
            blockTransNode.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.PiOver2);

        }
        //Associate building marker with an ARTag
        public void setMarkerNode(MarkerNode markerName)
        {
            blockNode = markerName;
        }
        public void addBuilding(Building _building)
        {
            buildingsInBlocks.Add(_building);
            blockTransNode.AddChild(_building.getBuildingNode());
        }
        public void setBuildingMaterial(string address, Material newMat)
        {
            foreach (Building b in buildingsInBlocks)
            {
                if (b.getBuildingName() == address)
                {
                    b.setBuildingMaterial(newMat);
                }
            }
        }
        public Building getBuilding(string address)
        {
            Building temp = new Building();
            temp.setBuildingName("Error");

            foreach (Building b in buildingsInBlocks)
            {
                if(b.getBuildingName() == address)
                {
                    return b;
                }
            }
            return temp;
        }

        public bool removeBuilding(string address)
        {
            foreach (Building b in buildingsInBlocks)
            {
                if (b.getBuildingName() == address)
                {
                    buildingsInBlocks.Remove(b);
                    blockTransNode.RemoveChild(b.getBuildingNode());
                    return true;
                }
            }
            return false;
        }

        //Detect presence of our marker
        public bool isMarkerPresent()
        {
            return blockNode.MarkerFound;
        }
        public MarkerNode getMarkerNode()
        {
            return blockNode;
        }
        public void setBuildings(List<Building> buildings)
        {
            buildingsInBlocks = buildings;

        }
        public void setTranslation(float x, float y, float z)
        {
            blockTransNode.Translation = new Vector3(x, y, z);
        }
        public void setScaling(float x, float y, float z)
        {
            blockTransNode.Scale = new Vector3(blockTransNode.Scale.X * x, blockTransNode.Scale.Y * y, blockTransNode.Scale.Z * z);
        }
        public void setRotation(float xDegrees, float yDegrees, float zDegrees)
        {
            blockTransNode.Rotation = Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians(xDegrees),MathHelper.ToRadians(yDegrees),MathHelper.ToRadians(zDegrees)) * blockTransNode.Rotation;
        }
        public void placeBuildings()
        {
            
        }
    }
}
