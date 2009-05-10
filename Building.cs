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

namespace MARVIN
{
    public class Building : Microsoft.Xna.Framework.Game
    {
        //Nodes and trackers
        GeometryNode buildingGeomNode; //Geometry Node
        Material buildingMaterial; //Building Material
        TransformNode buildingTransNode; //Transform node for this building.
       
        //Building properties
        System.String buildingName; //Building Name
        System.String buildingInfo; //Building info

        //Constructors
        public Building()
        {
            buildingGeomNode = new GeometryNode();
            buildingMaterial = new Material();
            buildingTransNode = new TransformNode();
        }
        public Building(System.String name)
        {
            buildingName = name;

            buildingGeomNode = new GeometryNode();
            buildingMaterial = new Material();
            buildingTransNode = new TransformNode();
        }
        public GeometryNode getBuildingNode()
        {
            return buildingGeomNode;
        }
        public TransformNode getBuildingTransNode()
        {
            return buildingTransNode;
        }
        public void setBuildingMaterial(Material material)
        {
            buildingMaterial = material;
            buildingGeomNode.Material = buildingMaterial;
        }
        public void setTransforms(TransformNode thisTransform)
        {
            buildingTransNode = thisTransform;
            buildingTransNode.AddChild(buildingGeomNode);
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

        public void loadBuildingModel(bool plainOrDetailed, float factor)
        {
            FileStream file;
            StreamReader sr;
            if(plainOrDetailed)
            {
                file = new FileStream("buildings_detailed.csv", FileMode.Open, FileAccess.Read);
                sr = new StreamReader(file);
            }
            else
            {
            file = new FileStream("buildings_plain.csv", FileMode.Open,
                FileAccess.Read);
            sr = new StreamReader(file);
            }
            ModelLoader loader = new ModelLoader();
            float zRot, x, y, z;
            String[] chunks;
            char[] seps = { ',' };

            String s = "";
            try
            {
                // Skip the first line which has column names
                sr.ReadLine();

                while (!sr.EndOfStream)
                {
                    s = sr.ReadLine();

                    if (s.Length > 0)
                    {
                        chunks = s.Split(seps);

                        if (chunks[0] == buildingName)
                        {
                            buildingGeomNode = new GeometryNode(chunks[0]);
                            buildingGeomNode.Model = (Model)loader.Load("", "Detailed/" + chunks[0]);
                            buildingGeomNode.AddToPhysicsEngine = true;
                            buildingGeomNode.Physics.Shape = ShapeType.Box;

                          //  buildings.Add(building);

                            zRot = (float)Double.Parse(chunks[1]);
                            x = (float)Double.Parse(chunks[2]);
                            y = (float)Double.Parse(chunks[3]);
                            z = (float)Double.Parse(chunks[4]);

             //               buildingTransNode = new TransformNode();
             //               buildingTransNode.Translation = new Vector3(x, y, z * factor);
             //               buildingTransNode.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ,
             //                   (float)(zRot * Math.PI / 180)) * Quaternion.CreateFromAxisAngle(Vector3.UnitX,
             //                   MathHelper.PiOver2);

                            buildingMaterial = new Material();
                            buildingMaterial.Diffuse = Color.White.ToVector4();
                            buildingMaterial.Specular = Color.White.ToVector4();
                            buildingMaterial.SpecularPower = 10;

                            buildingGeomNode.Material = buildingMaterial;
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine("buildings.csv has wrong format: " + s);
            }

            sr.Close();
            file.Close();
        }

        public void setBuildingOffset()
        {

        }

    }
}
