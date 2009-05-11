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
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class MARVIN : Microsoft.Xna.Framework.Game
    {
        static GlobalVariables global;
        static Pointer pointer;
        static Notebook notebook;
        static XMLReader xmlReader;

        

        public MARVIN()
        {
            Content.RootDirectory = "Content";
            global = new GlobalVariables();
            global.graphics = new GraphicsDeviceManager(this);
            //global.Content = this.Content;
            //global.Content.RootDirectory = "Content";

            pointer = new Pointer(ref global);
            notebook = new Notebook(ref global);
            xmlReader = new XMLReader(ref global);
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
            global.scene = new Scene(this);

            // Use the newton physics engine to perform collision detection
            global.scene.PhysicsEngine = new NewtonPhysics();

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

//            createBuildings(factor);
            // Create 3D terrain on top of the map layout
//            CreateTerrain(factor);

            // Load plain buildings
//            LoadPlainBuildings(factor);
            // Load detailed buildings
//            LoadDetailedBuildings(factor);
            createBuildings(factor);

            //Parses the XML building data document
            global.xmlFilename = "XMLFile2.xml";
            xmlReader.parseXMLBuildingFile(global.xmlFilename);

            // Show Frames-Per-Second on the screen for debugging
            State.ShowFPS = true;
            State.ShowNotifications = true;
            GoblinXNA.UI.Notifier.FadeOutTime = 1;

            base.Initialize();
        }

        private void CreateLights()
        {
            // Create a directional light source
            LightSource lightSource = new LightSource();
            lightSource.Direction = new Vector3(-1, -1, -1);
            lightSource.Diffuse = Color.White.ToVector4();
            lightSource.Specular = new Vector4(0.6f, 0.6f, 0.6f, 1);

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
            captureDevice.InitVideoCapture(3, FrameRate._30Hz, Resolution._800x600,ImageFormat.R8G8B8_24, false);            
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
            if (global.outdoors)
            {

                string[] buildingSetBlock1 = new string[] { "3221_Broadway", "3229_Broadway", "3233_Broadway", "613_W129st", "623_W129st", "627_W129st", "651_W125st", "663_W125st", "635_W125st", "633_W125st", "628_W125st", "619_W125st" };//, "564_Riverside","603_W130st","615_W130st","617_W130st","625_W130st","631_W130st","632_W130st","641_W130st","604_W131st","605_W131st","609_W131st","614_W131st","615_W131st","620_W131st","622_W131st","624_W131st","630_W131st","635_W131st","636_W131st","638_W131st","641_W131st","653_W131st","640_W132st","2283_Joe_Dimaggio_Highway","2291_Joe_Dimaggio_Highway","2293_Joe_Dimaggio_Highway","2307_Joe_Dimaggio_Highway","2311_Joe_Dimaggio_Highway","2321_Joe_Dimaggio_Highway"  };
                MarkerNode block1Marker = new MarkerNode(global.scene.MarkerTracker, "toolbar6");
                block1Marker.Smoother = new DESSmoother(0.8f, 0.8f);

                global.block1 = new Block(block1Marker, buildingSetBlock1);
                //    block1.setMarkerNode(block1Marker);
                //    Building block1TestBuilding = new Building("3221_Broadway");
                //    block1TestBuilding.loadBuildingModel(true, factor);

                //     block1.addBuilding(block1TestBuilding);
                global.block1.setScaling(3.0f, 3.0f, 3.0f);
                global.block1.setTranslation(30.0f, -244.25f, -210.0f);
                global.block1.setRotation(10.0f, 0.0f, 0.0f);
                global.scene.RootNode.AddChild(global.block1.getMarkerNode());

   /*             string[] buildingSetBlock2 = new string[] { "564_Riverside" };
                MarkerNode block2Marker = new MarkerNode(global.scene.MarkerTracker, "toolbar3");
                block2Marker.Smoother = new DESSmoother(0.8f, 0.8f);

                global.block2 = new Block(block2Marker, buildingSetBlock2);

                global.block2.setScaling(0.75f, 0.75f, 0.75f);         //       block2.setRotation(0.0f, -20.0f, 0.0f);                block2.setTranslation(20.0f, -60.25f, -40.0f);                                global.scene.RootNode.AddChild(block2.getMarkerNode());            }
    */        }
            else //We're indoors, setup differently
            {

            }
        }

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
            //Texture2D mville = Content.Load<Texture2D>("Textures//Manhattanville");
            //terrainMaterial.Texture = mville;
            terrainMaterial.Texture = Content.Load<Texture2D>("Textures//Manhattanville");
            //global.notebookTopTexture = mville;
            //notebook.createNotebook(mville);

            terrainNode.Material = terrainMaterial;

            TransformNode terrainTransNode = new TransformNode();
            terrainTransNode.Translation = new Vector3(7, 2.31f, 0);
            terrainTransNode.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ,
                MathHelper.PiOver2);

            global.groundMarkerNode.AddChild(terrainTransNode);
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
                    terrainHeights[6-i][4]);
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

        private void LoadPlainBuildings(float factor)
        {
            FileStream file = new FileStream("buildings_plain.csv", FileMode.Open,
                FileAccess.Read);
            StreamReader sr = new StreamReader(file);

            global.buildings = new List<GeometryNode>();
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

                        global.buildings.Add(building);

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

        private void LoadDetailedBuildings(float factor)
        {
            FileStream file = new FileStream("buildings_detailed.csv", FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(file);

            global.buildings = new List<GeometryNode>();
            ModelLoader loader = new ModelLoader();

            float scale = 0.0073f;
            float zRot, x, y, z;
            String[] chunks;
            char[] seps = { ',' };

            String s = "";
            try
            {
                // Skip the first line which has column names
                sr.ReadLine();

                global.parentTrans = new TransformNode();
                global.parentTrans.Scale = Vector3.One * scale;
                global.parentTrans.Translation = new Vector3(-33.5f, -54.25f, 0);
                global.parentTrans.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.PiOver2);
                global.groundMarkerNode.AddChild(global.parentTrans);

                while (!sr.EndOfStream)
                {
                    s = sr.ReadLine();

                    if (s.Length > 0)
                    {
                        chunks = s.Split(seps);

                        GeometryNode building = new GeometryNode(chunks[0]);
                        building.Model = (Model)loader.Load("", "Detailed/" + chunks[0]);
                        building.AddToPhysicsEngine = true;
                        building.Physics.Shape = ShapeType.Box;

                        global.buildings.Add(building);

                        zRot = (float)Double.Parse(chunks[1]);
                        x = (float)Double.Parse(chunks[2]);
                        y = (float)Double.Parse(chunks[3]);
                        z = (float)Double.Parse(chunks[4]);

                        TransformNode transNode = new TransformNode();
                        transNode.Translation = new Vector3(x, y, z * factor);
                        transNode.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ,
                            (float)(zRot * Math.PI / 180)) * Quaternion.CreateFromAxisAngle(Vector3.UnitX,
                            MathHelper.PiOver2);

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
            }
            else
            {

            }
            
            base.Draw(gameTime);

            // Draw a 2D text string at the top-left of the screen
            UI2DRenderer.WriteText(Vector2.Zero, global.label, global.labelColor,
                global.labelFont, GoblinEnums.HorizontalAlignment.Center, GoblinEnums.VerticalAlignment.Top);
        }

        
    }
}
