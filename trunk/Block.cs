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
        
        //Global references.

        public GlobalVariables global;

        //Markers
        MarkerNode blockNode;
        TransformNode blockTransNode;

        public Block(MarkerNode markerNode, string[] names, ref GlobalVariables g)
        {
            global = g;
        
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

                Building tempBuilding = new Building(s, ref global);
                tempBuilding.loadBuildingModel(true, 1.0f); //change scale factor if necessary
                buildingsInBlocks.Add(tempBuilding);

                blockTransNode.AddChild(tempBuilding.getBuildingNode());  /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////                //  blockTransNode.Scale = new Vector3(blockTransNode.Scale.X * 1.0f, blockTransNode.Scale.Y * 1.0f, blockTransNode.Scale.Z * 1.0f);
            }
        }
        public Block(MarkerNode markerNode, string[] names, float scale, ref GlobalVariables g)
        {
            global = g;
        
            buildingsInBlocks = new List<Building>();

            blockNode = markerNode;

            blockTransNode = new TransformNode();
            blockTransNode.Scale = Vector3.One*scale;
            //////////////////////////////////////////////////////////////////////blockTransNode.Translation = new Vector3(0.0f, -64.25f, 0.0f);
            //blockTransNode.Translation = new Vector3(0.0f, -64.25f, 0.0f);  //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //blockTransNode.Translation = new Vector3(-33.5f, -54.25f, 0);
            blockTransNode.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.PiOver2);
            blockNode.AddChild(blockTransNode);

            foreach (string s in names)
            {
                //Note, this needs to be changed so we are able to add different buildings to a marker

                Building tempBuilding = new Building(s, ref global);
                tempBuilding.loadBuildingModel(true, 1.0f); //change scale factor if necessary
                buildingsInBlocks.Add(tempBuilding);
                //blockTransNode.AddChild(tempBuilding.getBuildingNode()); /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
              //  blockTransNode.Scale = new Vector3(blockTransNode.Scale.X * 1.0f, blockTransNode.Scale.Y * 1.0f, blockTransNode.Scale.Z * 1.0f);
            //    blockNode.AddChild(blockTransNode);
            }
        }
        public void printBuildingCoordinates()
        {
            Vector4 homog_modelWorldCoords;// = Vector4.Transform(global.ORIGIN, global.pointerTip.WorldTransformation * global.toolbar1MarkerNode.WorldTransformation);
            Vector3 inhomog_modelWorldCoords;// = new Vector3(homog_pointerWorldCoords.X, homog_pointerWorldCoords.Y, homog_pointerWorldCoords.Z);
            Vector3 homog_modelScreenCoords;// = global.graphics.GraphicsDevice.Viewport.Project(inhomog_pointerWorldCoords,
                //State.ProjectionMatrix, State.ViewMatrix, Matrix.Identity);

           // GoblinXNA.UI.Notifier.AddMessage("" + global.ORIGIN.ToString());
            //GoblinXNA.UI.Notifier.AddMessage("" + blockNode.MarkerFound);
            GeometryNode tempBuilding;
            
            foreach (Building s in buildingsInBlocks)
            {
                tempBuilding = s.getBuildingNode();

                //homog_modelWorldCoords = Vector4.Transform(global.ORIGIN, blockNode.WorldTransformation);
                homog_modelWorldCoords = Vector4.Transform(global.ORIGIN, tempBuilding.WorldTransformation * blockTransNode.WorldTransformation * blockNode.WorldTransformation);
                //homog_modelWorldCoords = Vector4.Transform(global.ORIGIN, tempBuilding.Model.OffsetTransform * tempBuilding.WorldTransformation * blockTransNode.WorldTransformation * blockNode.WorldTransformation);
                inhomog_modelWorldCoords = new Vector3(homog_modelWorldCoords.X, homog_modelWorldCoords.Y, homog_modelWorldCoords.Z);
                homog_modelScreenCoords = global.graphics.GraphicsDevice.Viewport.Project(inhomog_modelWorldCoords,
                State.ProjectionMatrix, State.ViewMatrix, Matrix.Identity);

                GoblinXNA.UI.Notifier.AddMessage("Building " + s.getBuildingName() + " Screen Coords: " + homog_modelScreenCoords);
            }

        }

        public Block(ref GlobalVariables g)
        {
            global = g;
        
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
            blockTransNode.AddChild(_building.getBuildingNode()); ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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
            Building temp = new Building(ref global);
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
        public TransformNode getTransNode()
        {
            return blockTransNode;
        }
        public void placeBuildings()
        {
            
        }
    }
}
