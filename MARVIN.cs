using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

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
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class MARVIN : Microsoft.Xna.Framework.Game
    {
        // static Scene scene;
        Scene scene;
        static GlobalVariables global;
        static Pointer pointer;
        static Notebook notebook;
        static XMLManager xmlManager;



        public MARVIN()
        {
            Content.RootDirectory = "Content";
            global = new GlobalVariables();
            global.graphics = new GraphicsDeviceManager(this);
            //global.Content = this.Content;
            //global.Content.RootDirectory = "Content";

            pointer = new Pointer(ref global);
            notebook = new Notebook(ref global);
            xmlManager = new XMLManager(ref global);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Initialize the GoblinXNA framework
            State.InitGoblin(global.graphics, Content, "manhattanville.xml");

            // Initialize the scene graph
            scene = new Scene(this);
            global.setScene(ref scene);

            //Initialize the labels
            global.initializeLabels();

            this.IsMouseVisible = true;

            // Use the newton physics engine to perform collision detection
            global.scene.PhysicsEngine = new NewtonPhysics();

            //Are we indoors or outdoors?
            global.outdoors = true;

            SpriteFont labelfont = Content.Load<SpriteFont>("Sample");
            SpriteFont uifont = Content.Load<SpriteFont>("UIFont");
            global.labelFont = labelfont;
            global.uiFont = uifont;

            // Set up optical marker tracking
            // Note that we don't create our own camera when we use optical marker
            // tracking. It'll be created automatically
            SetupMarkerTracking();

            pointer.createPointer();

            //global.notebookTopTexture = Content.Load<Texture2D>("Textures//steve");
            ModelLoader loader = new ModelLoader();
            global.notebookModel = (Model)loader.Load("", "3dm-pad");
            global.notebookModel.UseInternalMaterials = true;
            notebook.createNotebook();

            // Set up the lights used in the scene
            CreateLights();

            float factor = 135.0f / 1353;

            if (global.outdoors)
            {
                createBuildings2();// (factor);
            }
            else
            {
                string[] buildingSetBlock1 = new string[] { "3221_Broadway", "3229_Broadway", "3233_Broadway", "613_W129st", "623_W129st", "627_W129st", "651_W125st", "663_W125st", "635_W125st", "633_W125st", "628_W125st", "619_W125st" };//, "564_Riverside","603_W130st","615_W130st","617_W130st","625_W130st","631_W130st","632_W130st","641_W130st","604_W131st","605_W131st","609_W131st","614_W131st","615_W131st","620_W131st","622_W131st","624_W131st","630_W131st","635_W131st","636_W131st","638_W131st","641_W131st","653_W131st","640_W132st","2283_Joe_Dimaggio_Highway","2291_Joe_Dimaggio_Highway","2293_Joe_Dimaggio_Highway","2307_Joe_Dimaggio_Highway","2311_Joe_Dimaggio_Highway","2321_Joe_Dimaggio_Highway"  };

                CreateTerrain(factor);
                LoadDetailedBuildings(factor, buildingSetBlock1);
            }

            //            createBuildings(factor);
            // Create 3D terrain on top of the map layout
            //            CreateTerrain(factor);

            // Load plain buildings
            //            LoadPlainBuildings(factor);
            // Load detailed buildings
            //            LoadDetailedBuildings(factor);
            //createBuildings(factor);            createBuildings2();
            //Parses the XML building data document
            global.xmlFilename = "XMLFile3.xml";
            xmlManager.parseXMLBuildingFile(global.xmlFilename);
            //try            {                global.doc.Load(global.xmlFilename);            }            catch(XmlException xmle)            {                Console.WriteLine("ERROR: " + xmle.Message);            }           // xmlReader.parseXMLBuildingFile(global.xmlFilename);            xmlManager.parseXMLBuildingFile(global.xmlFilename);            // Show Frames-Per-Second on the screen for debugging
            //State.ShowFPS = true;
            State.ShowNotifications = true;
            GoblinXNA.UI.Notifier.FadeOutTime = 1;

            // Add a mouse click callback function to perform picking when mouse is clicked
            MouseInput.Instance.MouseClickEvent += new HandleMouseClick(MouseClickHandler);
            // Add a mouse click callback function to perform picking when mouse is clicked
            MouseInput.Instance.MousePressEvent += new HandleMousePress(MousePressHandler);

            MouseInput.Instance.MouseWheelMoveEvent +=new HandleMouseWheelMove(MouseScrollHandler );

            global.createBuilding2DGUI();
            global.createGradient2DGUI();

            base.Initialize();
        }

        private void CreateLights()
        {
            // Create a directional light source
            LightSource lightSource = new LightSource();

            if (global.outdoors)
            {
                lightSource.Direction = new Vector3(-1, -1, -1);
                lightSource.Diffuse = Color.White.ToVector4();
                lightSource.Specular = new Vector4(0.6f, 0.6f, 0.6f, 1);
            }
            else
            {
                lightSource.Direction = new Vector3(-1, -1, -1);
                lightSource.Diffuse = Color.White.ToVector4();
                lightSource.Specular = new Vector4(0.6f, 0.6f, 0.6f, 1);
            }
            // Create a light node to hold the light source
            LightNode lightNode = new LightNode();
            lightNode.LightSources.Add(lightSource);

            global.scene.RootNode.AddChild(lightNode);
        }

        private void SetupMarkerTracking()
        {
            // Create our video capture device that uses DirectShow library. Note that 
            // the combinations of resolution and frame rate that are allowed depend on 
            // the particular video capture device. Thus, setting incorrect resolution 
            // and frame rate values may cause exceptions or simply be ignored, depending 
            // on the device driver.  The values set here will work for a Microsoft VX 6000, 
            // and many other webcams.
            DirectShowCapture captureDevice = new DirectShowCapture();
            captureDevice.InitVideoCapture(3, FrameRate._30Hz, Resolution._800x600, ImageFormat.R8G8B8_24, false);            // captureDevice.InitVideoCapture(0, -1, FrameRate._30Hz, Resolution._640x480, false);            captureDevice.InitVideoCapture(0, FrameRate._30Hz, Resolution._640x480,ImageFormat.R8G8B8_24, false);            
            //captureDevice.InitVideoCapture(0, -1, FrameRate._30Hz, Resolution._640x480, false);
            // Add this video capture device to the scene so that it can be used for
            // the marker tracker
            global.scene.AddVideoCaptureDevice(captureDevice);

            // Create a optical marker tracker that uses ARTag library
            ARTagTracker tracker = new ARTagTracker();
            // Set the configuration file to look for the marker specifications
            tracker.InitTracker(638.052f, 633.673f, captureDevice.Width,
                captureDevice.Height, false, "manhattanville.cf");

            global.scene.MarkerTracker = tracker;

            // Create a marker node to track the ground marker arrays
            global.groundMarkerNode = new MarkerNode(global.scene.MarkerTracker, "ground");
            global.scene.RootNode.AddChild(global.groundMarkerNode);

            // Create a marker node to track the indoor world model
            global.indoorMarkerNode = new MarkerNode(global.scene.MarkerTracker, "toolbar3");
            global.scene.RootNode.AddChild(global.indoorMarkerNode);

            // Create a marker node to track a toolbar marker array. Since we expect that the 
            // toolbar marker array will move a lot, we use a large smoothing alpha.
            global.toolbar1MarkerNode = new MarkerNode(global.scene.MarkerTracker, "toolbar1");
            global.toolbar1MarkerNode.Smoother = new DESSmoother(0.8f, 0.8f);
            global.scene.RootNode.AddChild(global.toolbar1MarkerNode);

            // Display the camera image in the background
            global.scene.ShowCameraImage = true;
        }

        private void createBuildings(float factor)
        {
            Console.WriteLine("Creating buildings...");

            string[] buildingSetBlock1 = new string[] { "3221_Broadway", "3229_Broadway", "3233_Broadway", "613_W129st", "623_W129st", "627_W129st", "651_W125st", "663_W125st", "635_W125st", "633_W125st", "628_W125st", "619_W125st" };//, "564_Riverside","603_W130st","615_W130st","617_W130st","625_W130st","631_W130st","632_W130st","641_W130st","604_W131st","605_W131st","609_W131st","614_W131st","615_W131st","620_W131st","622_W131st","624_W131st","630_W131st","635_W131st","636_W131st","638_W131st","641_W131st","653_W131st","640_W132st","2283_Joe_Dimaggio_Highway","2291_Joe_Dimaggio_Highway","2293_Joe_Dimaggio_Highway","2307_Joe_Dimaggio_Highway","2311_Joe_Dimaggio_Highway","2321_Joe_Dimaggio_Highway"  };
            MarkerNode block1Marker = new MarkerNode(global.scene.MarkerTracker, "toolbar6");
            block1Marker.Smoother = new DESSmoother(0.8f, 0.8f);

            global.block1 = new Block(block1Marker, buildingSetBlock1, ref global);
            //    block1.setMarkerNode(block1Marker);
            //    Building block1TestBuilding = new Building("3221_Broadway");
            //    block1TestBuilding.loadBuildingModel(true, factor);

            //     block1.addBuilding(block1TestBuilding);
            // global.block1.setScaling(3.0f, 3.0f, 3.0f);
            // global.block1.setTranslation(30.0f, -244.25f, -210.0f);
            // global.block1.setRotation(10.0f, 0.0f, 0.0f);
            global.scene.RootNode.AddChild(global.block1.getMarkerNode());
            /*
                            //Brian's code below //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            //global.scene.getMarkerNode

                            string[] buildingSetBlock2 = new string[] { "564_Riverside" };
                            MarkerNode block2Marker = new MarkerNode(global.scene.MarkerTracker, "toolbar3");
                            block2Marker.Smoother = new DESSmoother(0.8f, 0.8f);

                            global.block2 = new Block(block2Marker, buildingSetBlock2);

                            global.block2.setScaling(0.75f, 0.75f, 0.75f);         //       block2.setRotation(0.0f, -20.0f, 0.0f);                block2.setTranslation(20.0f, -60.25f, -40.0f);                                global.scene.RootNode.AddChild(block2.getMarkerNode());            }
                        */
        }

        private void createBuildings2()
        {
            string[] buildingSetBlock1 = new string[] { "3229_Broadway", "3221_Broadway", "3233_Broadway", "613_W129st", "623_W129st", "627_W129st", "651_W125st", "663_W125st", "635_W125st", "633_W125st", "628_W125st", "619_W125st" };//, "564_Riverside","603_W130st","615_W130st","617_W130st","625_W130st","631_W130st","632_W130st","641_W130st","604_W131st","605_W131st","609_W131st","614_W131st","615_W131st","620_W131st","622_W131st","624_W131st","630_W131st","635_W131st","636_W131st","638_W131st","641_W131st","653_W131st","640_W132st","2283_Joe_Dimaggio_Highway","2291_Joe_Dimaggio_Highway","2293_Joe_Dimaggio_Highway","2307_Joe_Dimaggio_Highway","2311_Joe_Dimaggio_Highway","2321_Joe_Dimaggio_Highway"  };
            MarkerNode block1Marker = new MarkerNode(global.scene.MarkerTracker, "toolbar6");
            block1Marker.Smoother = new DESSmoother(0.9f, 0.9f);
            global.blockMarker = block1Marker;
            global.scene.RootNode.AddChild(global.blockMarker);

            global.buildingGeomNodes = new List<GeometryNode>(8);
            global.buildingTransNodes = new List<TransformNode>(8);

            global.scale = 0.01f;


            for (int i = 0; i < buildingSetBlock1.Length; i++)
            {
                String thisBuildingName = buildingSetBlock1[i];
                Building thisBuilding = new Building(thisBuildingName, ref global);
                thisBuilding.loadBuildingModel(true, 1.0f);

                GeometryNode thisGeometryNode = thisBuilding.getBuildingNode();

                if (i == 0)
                {
                    global.calibrateCoords = new Vector3(
                                    (thisGeometryNode.Model.MinimumBoundingBox.Max.X + thisGeometryNode.Model.MinimumBoundingBox.Min.X) / 2,
                                    (thisGeometryNode.Model.MinimumBoundingBox.Max.Y + thisGeometryNode.Model.MinimumBoundingBox.Min.Y) / 2,
                                    (thisGeometryNode.Model.MinimumBoundingBox.Max.Z + thisGeometryNode.Model.MinimumBoundingBox.Min.Z) / 2);
                }

                float x = global.scale * (((thisGeometryNode.Model.MinimumBoundingBox.Max.X + thisGeometryNode.Model.MinimumBoundingBox.Min.X) / 2) - global.calibrateCoords.X);
                float y = global.scale * (((thisGeometryNode.Model.MinimumBoundingBox.Max.Y + thisGeometryNode.Model.MinimumBoundingBox.Min.Y) / 2) - global.calibrateCoords.Y);
                float z = global.scale * (((thisGeometryNode.Model.MinimumBoundingBox.Max.Z + thisGeometryNode.Model.MinimumBoundingBox.Min.Z) / 2) - global.calibrateCoords.Z);

                TransformNode thisTransformNode = new TransformNode();
                thisTransformNode.Translation = new Vector3(x, y, z);
                thisTransformNode.Scale = new Vector3(0.0073f, 0.0073f, 0.0073f);
                // thisTransformNode.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.PiOver2);

                /*String thisBuildingName = buildingSetBlock1[i];                Building thisBuilding = new Building(thisBuildingName);                thisBuilding.loadBuildingModel(true, 1.0f);*/
                //GeometryNode thisGeometryNode = thisBuilding.getBuildingNode();
                //GeometryNode thisGeometryNode = new GeometryNode();
                //thisGeometryNode.Model = new Box(2);
                //GeometryNode thisGeometryNode = getBuildingNode(i);

                Material buildingMaterial = new Material();
                buildingMaterial.Diffuse = Color.AliceBlue.ToVector4();
                buildingMaterial.Specular = Color.AliceBlue.ToVector4();
                buildingMaterial.SpecularPower = 30;

               thisGeometryNode.Material = buildingMaterial;

                global.buildingGeomNodes.Add(thisGeometryNode);
                global.buildingTransNodes.Add(thisTransformNode);

            }

            global.blockTransNode = new TransformNode();
            global.blockTransNode.Translation = new Vector3(5.0f, -5.0f, -50.0f);
            //global.blockTransNode.Translation = new Vector3(5.0f, -5.0f, -50.0f);
            global.blockTransNode.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.PiOver2);
            global.blockTransNode.Rotation = Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians(-2.0f), MathHelper.ToRadians(0.0f), MathHelper.ToRadians(0.0f)) * global.blockTransNode.Rotation;
            global.blockMarker.AddChild(global.blockTransNode);

            for (int i = 0; i < 8; i++)
            {
                global.blockTransNode.AddChild(global.buildingTransNodes[i]);
                global.buildingTransNodes[i].AddChild(global.buildingGeomNodes[i]);
            }
        }

        public GeometryNode getBuildingNode(int index)
        {
            GeometryNode returnGeometryNode = new GeometryNode();
            /*returnGeometryNode.Model = new Box(2);

            Material thisMaterial = new Material();
            thisMaterial.Diffuse = global.colorPalette[index];
            returnGeometryNode.Material = thisMaterial;

            return returnGeometryNode;*/

            returnGeometryNode.Model = global.buildingGeomNodes[index].Model;
            Material thisMaterial = new Material();
            thisMaterial.Diffuse = Color.White.ToVector4();
            thisMaterial.Specular = Color.White.ToVector4();
            thisMaterial.SpecularPower = 30;
            returnGeometryNode.Material = thisMaterial;
            return returnGeometryNode;
        }

        /*        public GeometryNode loadBuildingModel(bool plainOrDetailed, float factor)
                {
                    GeometryNode buildingGeomNode;
                    FileStream file;
                    StreamReader sr;
                    if (plainOrDetailed)
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
                                    buildingGeomNode.Model.CastShadows = true;
                                    buildingGeomNode.Model.OffsetToOrigin = true; ///////////////////////////////////////////////////////////////////

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
        */
        private void CreateTerrain(float factor)
        {
            float y_gap = 120.0f / 6;
            float x_gap = 71.1f / 4;
            float tu_gap = 1.0f / 6;
            float tv_gap = 1.0f / 4;

            float[][] terrain_heights = new float[7][];
            for (int i = 0; i < 7; i++)
                terrain_heights[i] = new float[5];

            terrain_heights[0][0] = 28;
            terrain_heights[0][1] = 48;
            terrain_heights[0][2] = 57;
            terrain_heights[0][3] = 66;
            terrain_heights[0][4] = 68;
            terrain_heights[1][0] = 20;
            terrain_heights[1][1] = 29;
            terrain_heights[1][2] = 43;
            terrain_heights[1][3] = 53;
            terrain_heights[1][4] = 58;
            terrain_heights[2][0] = 16;
            terrain_heights[2][1] = 24;
            terrain_heights[2][2] = 32;
            terrain_heights[2][3] = 40;
            terrain_heights[2][4] = 46;
            terrain_heights[3][0] = 13;
            terrain_heights[3][1] = 18;
            terrain_heights[3][2] = 25;
            terrain_heights[3][3] = 29;
            terrain_heights[3][4] = 35;
            terrain_heights[4][0] = 10;
            terrain_heights[4][1] = 13;
            terrain_heights[4][2] = 17;
            terrain_heights[4][3] = 21;
            terrain_heights[4][4] = 26;
            terrain_heights[5][0] = 14;
            terrain_heights[5][1] = 15;
            terrain_heights[5][2] = 18;
            terrain_heights[5][3] = 23;
            terrain_heights[5][4] = 23;
            terrain_heights[6][0] = 44;
            terrain_heights[6][1] = 35;
            terrain_heights[6][2] = 32;
            terrain_heights[6][3] = 26;
            terrain_heights[6][4] = 24;

            for (int i = 0; i < 7; i++)
                for (int j = 0; j < 5; j++)
                    terrain_heights[i][j] *= factor;

            PrimitiveMesh terrain = new PrimitiveMesh();

            VertexPositionNormalTexture[] verts = new VertexPositionNormalTexture[35];

            int index = 0;
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    index = i * 7 + j;

                    verts[index].Position = new Vector3(j * y_gap + global.y_shift, i * x_gap + global.x_shift,
                        terrain_heights[j][i]);
                    verts[index].TextureCoordinate = new Vector2(j * tu_gap, 1 - i * tv_gap);
                    verts[index].Normal = Vector3.UnitZ;
                }
            }

            terrain.VertexBuffer = new VertexBuffer(global.graphics.GraphicsDevice,
                VertexPositionNormalTexture.SizeInBytes * 35, BufferUsage.None);
            terrain.SizeInBytes = VertexPositionNormalTexture.SizeInBytes;
            terrain.VertexDeclaration = new VertexDeclaration(global.graphics.GraphicsDevice,
                VertexPositionNormalTexture.VertexElements);
            terrain.VertexBuffer.SetData(verts);
            terrain.NumberOfVertices = 35;


            short[] indices = new short[6 * 4 * 2 * 3];
            int count = 0;

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    index = i * 7 + j;

                    indices[count++] = (short)index;
                    indices[count++] = (short)(index + 7);
                    indices[count++] = (short)(index + 1);

                    indices[count++] = (short)(index + 7);
                    indices[count++] = (short)(index + 8);
                    indices[count++] = (short)(index + 1);
                }
            }

            terrain.IndexBuffer = new IndexBuffer(global.graphics.GraphicsDevice, typeof(short), indices.Length,
                BufferUsage.WriteOnly);
            terrain.IndexBuffer.SetData(indices);

            terrain.PrimitiveType = PrimitiveType.TriangleList;
            terrain.NumberOfPrimitives = indices.Length / 3;

            Model terrainModel = new Model(terrain);

            GeometryNode terrainNode = new GeometryNode();
            terrainNode.Model = terrainModel;

            Material terrainMaterial = new Material();
            terrainMaterial.Diffuse = Color.White.ToVector4();
            terrainMaterial.Specular = Color.White.ToVector4();
            terrainMaterial.SpecularPower = 10;
            terrainMaterial.Texture = Content.Load<Texture2D>("Textures//Manhattanville");

            terrainNode.Material = terrainMaterial;

            TransformNode terrainTransNode = new TransformNode();
            terrainTransNode.Translation = new Vector3(7, 2.31f, 0);
            terrainTransNode.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ,
                MathHelper.PiOver2);

            global.indoorMarkerNode.AddChild(terrainTransNode);
            terrainTransNode.AddChild(terrainNode);

            CreateSkirf(terrain_heights, terrainTransNode);
        }

        private void CreateSkirf(float[][] terrainHeights, TransformNode transNode)
        {
            float y_gap = 120.0f / 6;
            float x_gap = 71.1f / 4;

            PrimitiveMesh skirf = new PrimitiveMesh();

            VertexPositionNormal[] verts = new VertexPositionNormal[48];

            int index = 0;

            for (int i = 0; i < 7; i++)
            {
                verts[index].Position = new Vector3(i * y_gap + global.y_shift, global.x_shift, terrainHeights[i][0]);
                verts[index].Normal = Vector3.UnitY;
                index++;

                verts[index].Position = new Vector3(i * y_gap + global.y_shift, global.x_shift, 0);
                verts[index].Normal = Vector3.UnitY;
                index++;

                verts[index].Position = new Vector3((6 - i) * y_gap + global.y_shift, 71.1f + global.x_shift,
                    terrainHeights[6 - i][4]);
                verts[index].Normal = -Vector3.UnitY;
                index++;

                verts[index].Position = new Vector3((6 - i) * y_gap + global.y_shift, 71.1f + global.x_shift, 0);
                verts[index].Normal = -Vector3.UnitY;
                index++;
            }

            for (int i = 0; i < 5; i++)
            {
                verts[index].Position = new Vector3(global.y_shift, (4 - i) * x_gap + global.x_shift, terrainHeights[0][(4 - i)]);
                verts[index].Normal = Vector3.UnitX;
                index++;

                verts[index].Position = new Vector3(global.y_shift, (4 - i) * x_gap + global.x_shift, 0);
                verts[index].Normal = Vector3.UnitX;
                index++;

                verts[index].Position = new Vector3(120.0f + global.y_shift, i * x_gap + global.x_shift, terrainHeights[6][i]);
                verts[index].Normal = -Vector3.UnitX;
                index++;

                verts[index].Position = new Vector3(120.0f + global.y_shift, i * x_gap + global.x_shift, 0);
                verts[index].Normal = -Vector3.UnitX;
                index++;
            }

            skirf.VertexBuffer = new VertexBuffer(global.graphics.GraphicsDevice,
                VertexPositionNormal.SizeInBytes * 48, BufferUsage.None);
            skirf.SizeInBytes = VertexPositionNormal.SizeInBytes;
            skirf.VertexDeclaration = new VertexDeclaration(global.graphics.GraphicsDevice,
                VertexPositionNormal.VertexElements);
            skirf.VertexBuffer.SetData(verts);
            skirf.NumberOfVertices = 48;


            short[] indices = new short[20 * 2 * 3];
            index = 0;

            for (int i = 0; i < 11; i++)
            {
                if (i != 6)
                {
                    indices[index++] = (short)(i * 4 + 1);
                    indices[index++] = (short)(i * 4);
                    indices[index++] = (short)((i + 1) * 4);

                    indices[index++] = (short)(i * 4 + 1);
                    indices[index++] = (short)((i + 1) * 4);
                    indices[index++] = (short)((i + 1) * 4 + 1);

                    indices[index++] = (short)(i * 4 + 3);
                    indices[index++] = (short)(i * 4 + 2);
                    indices[index++] = (short)((i + 1) * 4 + 2);

                    indices[index++] = (short)(i * 4 + 3);
                    indices[index++] = (short)((i + 1) * 4 + 2);
                    indices[index++] = (short)((i + 1) * 4 + 3);
                }
            }

            skirf.IndexBuffer = new IndexBuffer(global.graphics.GraphicsDevice, typeof(short), indices.Length,
                BufferUsage.WriteOnly);
            skirf.IndexBuffer.SetData(indices);

            skirf.PrimitiveType = PrimitiveType.TriangleList;
            skirf.NumberOfPrimitives = indices.Length / 3;

            Model skirfModel = new Model(skirf);

            GeometryNode skirfNode = new GeometryNode();
            skirfNode.Model = skirfModel;

            Material skirfMaterial = new Material();
            skirfMaterial.Diffuse = Color.White.ToVector4();
            skirfMaterial.Specular = Color.White.ToVector4();
            skirfMaterial.SpecularPower = 10;

            skirfNode.Material = skirfMaterial;

            transNode.AddChild(skirfNode);
        }
        /*private void transferToNotebook(string address, Block block)
        {

            GeometryNode buildingToTransfer = (block.getBuilding(address)).getBuildingNode();
            block.removeBuilding(address);
            TransformNode newTransform = new TransformNode();
            float scale = 0.0073f;
            newTransform.Scale = Vector3.One*scale;
            newTransform.Translation = new Vector3(0.0f, -64.25f, 0.0f);
  //          newTransform.Translation = new Vector3(33.5f, -54.25f, 0.0f);
            newTransform.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.PiOver2);
            newTransform.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ,
                                (float)(Math.PI / 180)) * Quaternion.CreateFromAxisAngle(Vector3.UnitX,
                                MathHelper.PiOver2) * newTransform.Rotation;
            newTransform.AddChild(buildingToTransfer);
            global.groundMarkerNode.AddChild(newTransform);

        }*/

        private void LoadPlainBuildings(float factor)
        {
            Console.WriteLine("Loading plain buildings..."); FileStream file = new FileStream("buildings_plain.csv", FileMode.Open,
     FileAccess.Read);
            StreamReader sr = new StreamReader(file);

            //global.buildings = new List<GeometryNode>();
            ModelLoader loader = new ModelLoader();

            float scale = 0.00728f;
            float zRot, x, y, z;
            String[] chunks;
            char[] seps = { ',' };

            String s = "";
            try
            {
                // Skip the first line which has column names
                sr.ReadLine();

                global.parentTrans = new TransformNode();
                global.parentTrans.Translation = new Vector3(-12.5f, -15.69f, 0);
                global.parentTrans.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 119 * MathHelper.Pi / 180);
                global.groundMarkerNode.AddChild(global.parentTrans);

                while (!sr.EndOfStream)
                {
                    s = sr.ReadLine();

                    if (s.Length > 0)
                    {
                        chunks = s.Split(seps);

                        GeometryNode building = new GeometryNode(chunks[0]);
                        building.Model = (Model)loader.Load("", "Plain/" + chunks[0]);
                        building.AddToPhysicsEngine = true;
                        building.Physics.Shape = ShapeType.Box;

                        // global.buildings.Add(building);

                        zRot = (float)Double.Parse(chunks[1]);
                        x = (float)Double.Parse(chunks[2]);
                        y = (float)Double.Parse(chunks[3]);
                        z = (float)Double.Parse(chunks[4]);

                        TransformNode transNode = new TransformNode();
                        transNode.Translation = new Vector3(x, y, z * factor);
                        transNode.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ,
                            (float)(zRot * Math.PI / 180)) * Quaternion.CreateFromAxisAngle(Vector3.UnitX,
                            MathHelper.PiOver2);
                        transNode.Scale = Vector3.One * scale;


                        Material buildingMaterial = new Material();
                        buildingMaterial.Diffuse = Color.White.ToVector4();
                        buildingMaterial.Specular = Color.White.ToVector4();
                        buildingMaterial.SpecularPower = 10;

                        building.Material = buildingMaterial;

                        global.parentTrans.AddChild(transNode);
                        transNode.AddChild(building);
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

        private void LoadDetailedBuildings(float factor, string[] address)
        {
            Console.WriteLine("Loading detailed buildings...");
            //   FileStream file = new FileStream("buildings_detailed.csv", FileMode.Open, FileAccess.Read);
            //   StreamReader sr = new StreamReader(file);

            //global.buildings = new List<GeometryNode>();
            ModelLoader loader = new ModelLoader();

            global.buildingGeomNodes = new List<GeometryNode>(address.Length);
            global.buildingTransNodes = new List<TransformNode>(address.Length);

            float scale = 0.0073f;
            //      String[] chunks;
            //      char[] seps = { ',' };

            //      String s = "";
            //      try
            //      {
            // Skip the first line which has column names
            //          sr.ReadLine();

            global.parentTrans = new TransformNode();
            global.parentTrans.Scale = Vector3.One * scale;
            global.parentTrans.Translation = new Vector3(-33.5f, -54.25f, 0);
            global.parentTrans.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.PiOver2);
            global.indoorMarkerNode.AddChild(global.parentTrans);

            for (int i = 0; i < address.Length; i++)
            {
                GeometryNode building = new GeometryNode(address[i]);
                building.Model = (Model)loader.Load("", "Detailed/" + address[i]);
                building.AddToPhysicsEngine = true;
                building.Physics.Shape = ShapeType.Box;

                TransformNode transNode = new TransformNode();
                transNode.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ,
                    (float)(0 * Math.PI / 180)) * Quaternion.CreateFromAxisAngle(Vector3.UnitX,
                    MathHelper.PiOver2);

                Material buildingMaterial = new Material();
                buildingMaterial.Diffuse = Color.White.ToVector4();
                buildingMaterial.Specular = Color.Beige.ToVector4();
                buildingMaterial.SpecularPower = 10;

                building.Material = buildingMaterial;

                global.parentTrans.AddChild(transNode);
                transNode.AddChild(building);

                global.buildingGeomNodes.Add(building);
                global.buildingTransNodes.Add(transNode);

            }
        }

        public void transferBuildingToNotebook()
        {
            //GeometryNode buildingToTransfer = (GeometryNode) global.buildingGeomNodes[global.indexOfObjectBeingHighlighted];
            GeometryNode buildingToTransfer = getBuildingNode(global.indexOfObjectBeingHighlighted);
         //   buildingToTransfer.Model.OffsetToOrigin = true;

            int count1 = global.notebookShowcaseTransNode.Children.Count;
            Console.WriteLine("Before: " + count1);
            //global.notebookShowcaseTransNode.RemoveChild(global.notebookShowcaseGeomNode); ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            global.notebookShowcaseModelTransNode.RemoveChildren();
            int count2 = global.notebookShowcaseTransNode.Children.Count;
            Console.WriteLine("After: " + count2);

            //global.notebookShowcaseModelTransNode.Translation = new Vector3(-20.0f, 0.0f, 7.0f); //(-20.0f, ...)            
            //global.notebookShowcaseModelTransNode.Scale = new Vector3(4.5f, 4.5f, 4.5f);///////////////Changed from 4.5 to 1.5            
            global.notebookShowcaseModelTransNode.Rotation = global.buildingTransNodes[global.indexOfObjectBeingHighlighted].Rotation;            
            global.notebookShowcaseModelTransNode.Rotation = Quaternion.Concatenate(global.notebookShowcaseModelTransNode.Rotation,                 
                Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.PiOver2));            
            global.notebookShowcaseModelTransNode.Scale = global.buildingTransNodes[global.indexOfObjectBeingHighlighted].Scale;
            global.notebookShowcaseModelTransNode.Translation = new Vector3(global.buildingTransNodes[global.indexOfObjectBeingHighlighted].Translation.X, global.buildingTransNodes[global.indexOfObjectBeingHighlighted].Translation.Y, 0);            
            global.notebookShowcaseModelTransNode.Translation += new Vector3(-20.0f, 0.0f, 7.0f);
            Console.WriteLine(global.notebookShowcaseModelTransNode.Translation.ToString());
            //buildingToTransfer.Model.OffsetToOrigin = true;
            global.notebookShowcaseModelTransNode.AddChild(buildingToTransfer);
            //global.notebookShowcaseGeomNode = buildingToTransfer;
            //global.notebookShowcaseTransNode.AddChild(global.leftConeTransNode);//////////////////////////////////////////////////////
            //global.leftConeTransNode.AddChild(global.leftConeGeomNode);//YYY******New!
            //global.notebookShowcaseTransNode.AddChild(global.rightConeTransNode);/////////////////////////////////////////////////////
            //global.rightConeTransNode.AddChild(global.rightConeGeomNode);//***********New!

            int count3 = global.notebookShowcaseTransNode.Children.Count;
            Console.WriteLine("After: " + count3);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // If the toolbar marker array is detected
            if (global.toolbar1MarkerNode.MarkerFound)
            {
                pointer.performPointing();
                //global.block1.printBuildingCoordinates();
            }

            if (global.groundMarkerNode.MarkerFound)
            {
                drawAttributeLabels();
            }
            else
            {
                hideAttributeLabels();
            }

            base.Draw(gameTime);

            // Draw a 2D text string at the top-left of the screen
            UI2DRenderer.WriteText(Vector2.Zero, global.label, global.labelColor,
                global.labelFont, GoblinEnums.HorizontalAlignment.Left, GoblinEnums.VerticalAlignment.Top);
        }


        private void drawAttributeLabels()
        {
            //First we refresh the labels' text
            G2DLabel myLabel;
            
            //Now we draw them on the screen
            for (int i = 0; i < 8; i++)
            {
                Vector4 homog_attributeWorldCoords = Vector4.Transform(global.ORIGIN, global.attributeTransNodes[i].WorldTransformation * global.groundMarkerNode.WorldTransformation);
                Vector3 inhomog_attributeWorldCoords = new Vector3(homog_attributeWorldCoords.X, homog_attributeWorldCoords.Y, homog_attributeWorldCoords.Z);
                Vector3 homog_attributeScreenCoords = global.graphics.GraphicsDevice.Viewport.Project(inhomog_attributeWorldCoords,
                    State.ProjectionMatrix, State.ViewMatrix, Matrix.Identity);

                if (i == 0)
                {
                    //GoblinXNA.UI.Notifier.AddMessage("Attribute 1 Screen Coords: " + homog_attributeScreenCoords);
                }

                global.labelPanels[i].Bounds = new Rectangle((int)homog_attributeScreenCoords.X + 15, (int)homog_attributeScreenCoords.Y - 15, 160, 25);
                global.attributeLabels[i].Bounds = new Rectangle(0, 0, 160, 25);

                myLabel = new G2DLabel(global.attributes[i].name);
                myLabel.Visible = true;
                myLabel.TextFont = global.uiFont;
                myLabel.TextColor = new Color(global.colorPalette[i]);

                global.labelPanels[i].AddChild(myLabel);
                global.labelPanels[i].Visible = true;
                
                global.attributeLabels[i].Visible = true;
            }
        }

        private void hideAttributeLabels()
        {
            //Now we draw them on the screen
            for (int i = 0; i < 8; i++)
            {
                global.labelPanels[i].Visible = false;
            }
        }

        private void MouseScrollHandler(int delta, int value)
        {
            global.indexOfBuildingBeingSelected += delta;
            global.indexOfBuildingBeingSelected = global.indexOfBuildingBeingSelected % 8;
        }

        private void MouseClickHandler(int button, Point mouseLocation)
        {
            if (button == MouseInput.LeftButton)
            {
                if (global.typeOfObjectBeingHighlighted == global.NOTHING)
                {
                    //Do nothing.
                }
                else if (global.typeOfObjectBeingHighlighted == global.BUILDING)
                {
                    global.typeOfObjectBeingSelected = global.BUILDING;
                    global.indexOfBuildingBeingSelected = global.indexOfObjectBeingHighlighted;
                    global.label = "Selection: " + global.buildingList[global.indexOfBuildingBeingSelected].getBuildingName();

                    global.resetObjectColors();

                    //Draw Bounding Box around building //////////////////////////////////////////////////////////////////////////////////////////////////
                    transferBuildingToNotebook();

                    global.refreshInfoPanel();
                    for (int i = 0; i < 10; i++)
                    {
                        global.markerLabels[i].Visible = false;
                    }
                    global.markerLabels[0].Visible = true;
                    global.nameField.Enabled = true;
                    global.addressField.Enabled = false;
                    global.storiesField.Enabled = false;
                    global.toxicSitesField.Enabled = false;
                    global.airRightsField.Enabled = false;
                    global.yearBuiltField.Enabled = false;
                    global.descriptionField.Enabled = false;
                    global.typeField.Enabled = false;
                    global.classField.Enabled = false;
                    global.saleDateField.Enabled = false;
                    global.nameField.Editable = true;
                    global.nameField.setFocused(true);
                    global.nameField.SelectAll();
                    global.buildingInfoIndex = 0;

                    if (global.attributeCurrentlyBeingViewed != global.NOTHING)
                    {
                        global.gradientNameLabel2.Text = "(Value for " + global.buildingList[global.indexOfBuildingBeingSelected].getBuildingName() + ": " + global.buildingList[global.indexOfBuildingBeingSelected].getAttributeValue(global.attributes[global.attributeCurrentlyBeingViewed].name) + ")";
                        global.gradientNameLabel2.Visible = true;
                    }
                    

                }
                else if (global.typeOfObjectBeingHighlighted == global.ATTRIBUTE)
                {
                    global.typeOfObjectBeingSelected = global.ATTRIBUTE;
                    //global.indexOfBuildingBeingSelected = global.indexOfObjectBeingHighlighted;

                    //if we're deselecting an attribute
                    if (global.indexOfObjectBeingHighlighted == global.attributeCurrentlyBeingViewed)
                    {
                        global.attributeCurrentlyBeingViewed = global.NOTHING;
                    }
                    else
                    {
                        global.attributeCurrentlyBeingViewed = global.indexOfObjectBeingHighlighted;
                    }
                    
                    global.resetObjectColors();
                }
                else
                {
                    //Do nothing. :)
                }
            }
            else if (button == MouseInput.RightButton)
            {
                if (global.toolbar1MarkerNode.MarkerFound)
                {
                    if (global.buildingInfoIndex == -1)
                    {
                        if (global.indexOfBuildingBeingSelected != global.NOTHING)
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                global.markerLabels[i].Visible = false;
                            }
                            global.markerLabels[0].Visible = true;
                            global.nameField.Enabled = true;
                            global.addressField.Enabled = false;
                            global.storiesField.Enabled = false;
                            global.toxicSitesField.Enabled = false;
                            global.airRightsField.Enabled = false;
                            global.yearBuiltField.Enabled = false;
                            global.descriptionField.Enabled = false;
                            global.typeField.Enabled = false;
                            global.classField.Enabled = false;
                            global.saleDateField.Enabled = false;
                            global.nameField.Editable = true;
                            global.nameField.setFocused(true);
                            global.nameField.SelectAll();
                            global.buildingInfoIndex = 0;
                            global.buildingInfoPanel.Visible = true;
                        }
                        else
                        {
                            //Do nothing.
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            global.markerLabels[i].Visible = false;
                        }

                        if (global.buildingInfoIndex == 0)
                        {
                            global.buildingList[global.indexOfBuildingBeingSelected].setBuildingName(global.nameField.Text);
                            global.nameField.Enabled = false;
                            global.nameField.Editable = false;
                            global.nameField.setFocused(false);
                        }
                        else if (global.buildingInfoIndex == 1)
                        {
                            global.buildingList[global.indexOfBuildingBeingSelected].setAddress(global.addressField.Text);
                            global.addressField.Enabled = false;
                            global.addressField.Editable = false;
                            global.addressField.setFocused(false);
                        }
                        else if (global.buildingInfoIndex == 2)
                        {
                            global.buildingList[global.indexOfBuildingBeingSelected].setStories(global.storiesField.Text);
                            global.storiesField.Enabled = false;
                            global.storiesField.Editable = false;
                            global.storiesField.setFocused(false);
                        }
                        else if (global.buildingInfoIndex == 3)
                        {
                            global.buildingList[global.indexOfBuildingBeingSelected].setToxicSites(global.toxicSitesField.Text);
                            global.toxicSitesField.Enabled = false;
                            global.toxicSitesField.Editable = false;
                            global.toxicSitesField.setFocused(false);
                        }
                        else if (global.buildingInfoIndex == 4)
                        {
                            global.buildingList[global.indexOfBuildingBeingSelected].setAirRights(global.airRightsField.Text);
                            global.airRightsField.Enabled = false;
                            global.airRightsField.Editable = false;
                            global.airRightsField.setFocused(false);
                        }
                        else if (global.buildingInfoIndex == 5)
                        {
                            global.buildingList[global.indexOfBuildingBeingSelected].setYearBuilt(global.yearBuiltField.Text);
                            global.yearBuiltField.Enabled = false;
                            global.yearBuiltField.Editable = false;
                            global.yearBuiltField.setFocused(false);
                        }
                        else if (global.buildingInfoIndex == 6)
                        {
                            global.buildingList[global.indexOfBuildingBeingSelected].setType(global.typeField.Text);
                            global.typeField.Enabled = false;
                            global.typeField.Editable = false;
                            global.typeField.setFocused(false);
                        }
                        else if (global.buildingInfoIndex == 7)
                        {
                            global.buildingList[global.indexOfBuildingBeingSelected].setClass(global.classField.Text);
                            global.classField.Enabled = false;
                            global.classField.Editable = false;
                            global.classField.setFocused(false);
                        }
                        else if (global.buildingInfoIndex == 8)
                        {
                            global.buildingList[global.indexOfBuildingBeingSelected].setSaleDate(global.saleDateField.Text);
                            global.saleDateField.Enabled = false;
                            global.saleDateField.Editable = false;
                            global.saleDateField.setFocused(false);
                        }
                        else// if (global.buildingInfoIndex == 9)
                        {
                            global.buildingList[global.indexOfBuildingBeingSelected].setDescription(global.descriptionField.Text);
                            global.descriptionField.Enabled = false;
                            global.descriptionField.Editable = false;
                            global.descriptionField.setFocused(false);
                        }

                        global.buildingInfoIndex = -1;
                        global.buildingInfoPanel.Visible = false;
                    }
                }
                else //if toolbar1 is not found
                {
                    if (global.buildingInfoIndex == -1)
                    {
                        //Do nothing.
                    }
                    else
                    {
                        global.markerLabels[global.buildingInfoIndex].Visible = false;
                        global.markerLabels[(global.buildingInfoIndex + 1) % 10].Visible = true;
                        
                        if (global.buildingInfoIndex == 0)
                        {
                            global.buildingList[global.indexOfBuildingBeingSelected].setBuildingName(global.nameField.Text);
                            global.nameField.Enabled = false;
                            global.nameField.Editable = false;
                            global.nameField.setFocused(false);
                            global.addressField.Enabled = true;
                            global.addressField.Editable = true;
                            global.addressField.setFocused(true);
                            global.addressField.SelectAll();
                        }
                        else if (global.buildingInfoIndex == 1)
                        {
                            global.buildingList[global.indexOfBuildingBeingSelected].setAddress(global.addressField.Text);
                            global.addressField.Enabled = false;
                            global.addressField.Editable = false;
                            global.addressField.setFocused(false);
                            global.storiesField.Enabled = true;
                            global.storiesField.Editable = true;
                            global.storiesField.setFocused(true);
                            global.storiesField.SelectAll();
                        }
                        else if (global.buildingInfoIndex == 2)
                        {
                            global.buildingList[global.indexOfBuildingBeingSelected].setStories(global.storiesField.Text);
                            global.storiesField.setFocused(false);
                            global.storiesField.Enabled = false;
                            global.storiesField.Editable = false;
                            global.toxicSitesField.Enabled = true;
                            global.toxicSitesField.Editable = true;
                            global.toxicSitesField.setFocused(true);
                        }
                        else if (global.buildingInfoIndex == 3)
                        {
                            global.buildingList[global.indexOfBuildingBeingSelected].setToxicSites(global.toxicSitesField.Text);
                            global.toxicSitesField.Enabled = false;
                            global.toxicSitesField.Editable = false;
                            global.toxicSitesField.setFocused(false);
                            global.airRightsField.Enabled = true;
                            global.airRightsField.Editable = true;
                            global.airRightsField.setFocused(true);
                        }
                        else if (global.buildingInfoIndex == 4)
                        {
                            global.buildingList[global.indexOfBuildingBeingSelected].setAirRights(global.airRightsField.Text);
                            global.airRightsField.Enabled = false;
                            global.airRightsField.Editable = false;
                            global.airRightsField.setFocused(false);
                            global.yearBuiltField.Enabled = true;
                            global.yearBuiltField.Editable = true;
                            global.yearBuiltField.setFocused(true);
                        }
                        else if (global.buildingInfoIndex == 5)
                        {
                            global.buildingList[global.indexOfBuildingBeingSelected].setYearBuilt(global.yearBuiltField.Text);
                            global.yearBuiltField.Enabled = false;
                            global.yearBuiltField.Editable = false;
                            global.yearBuiltField.setFocused(false);
                            global.typeField.Enabled = true;
                            global.typeField.Editable = true;
                            global.typeField.setFocused(true);
                        }
                        else if (global.buildingInfoIndex == 6)
                        {
                            global.buildingList[global.indexOfBuildingBeingSelected].setType(global.typeField.Text);
                            global.typeField.setFocused(false);
                            global.typeField.Enabled = false;
                            global.typeField.Editable = false;
                            global.classField.setFocused(true);
                            global.classField.Enabled = true;
                            global.classField.Editable = true;
                        }
                        else if (global.buildingInfoIndex == 7)
                        {
                            global.buildingList[global.indexOfBuildingBeingSelected].setClass(global.classField.Text);
                            global.classField.Enabled = false;
                            global.classField.Editable = false;
                            global.classField.setFocused(false);
                            global.saleDateField.Enabled = true;
                            global.saleDateField.Editable = true;
                            global.saleDateField.setFocused(true);
                        }
                        else if (global.buildingInfoIndex == 8)
                        {
                            global.buildingList[global.indexOfBuildingBeingSelected].setSaleDate(global.saleDateField.Text);
                            global.saleDateField.Enabled = false;
                            global.saleDateField.Editable = false;
                            global.saleDateField.setFocused(false);
                            global.descriptionField.Enabled = true;
                            global.descriptionField.Editable = true;
                            global.descriptionField.setFocused(true);
                        }
                        else// if (global.buildingInfoIndex == 9)
                        {
                            global.buildingList[global.indexOfBuildingBeingSelected].setDescription(global.descriptionField.Text);
                            global.descriptionField.Enabled = false;
                            global.descriptionField.Editable = false;
                            global.descriptionField.setFocused(false);
                            global.nameField.Enabled = true;
                            global.nameField.Editable = true;
                            global.nameField.setFocused(true);
                        }

                        global.buildingInfoIndex = (global.buildingInfoIndex + 1) % 10;
                    }
                }
            }
        }


        private void MousePressHandler(int button, Point mouseLocation)
        {
            if (button == MouseInput.LeftButton)
            {
                if (global.typeOfObjectBeingHighlighted == global.LEFT_CONE)
                {
                    Quaternion extraRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 0.1f);
                    global.notebookShowcaseModelTransNode.Rotation = Quaternion.Concatenate(global.notebookShowcaseModelTransNode.Rotation, extraRotation);
                }
                else if (global.typeOfObjectBeingHighlighted == global.RIGHT_CONE)
                {
                    Quaternion extraRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, -0.1f);
                    global.notebookShowcaseModelTransNode.Rotation = Quaternion.Concatenate(global.notebookShowcaseModelTransNode.Rotation, extraRotation);
                }
            }
        }

        public void finalize()
        {
            xmlManager.writeToXml("");
        }
    }
}
