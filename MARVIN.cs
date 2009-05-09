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
        GraphicsDeviceManager graphics;

        Scene scene;
        MarkerNode groundMarkerNode, toolbar1MarkerNode;
        List<GeometryNode> buildings;
        TransformNode parentTrans;
        TransformNode toolBar1OccluderTransNode;
        GeometryNode pointerTip;
        GeometryNode pointerSegment;
        GeometryNode toolbar1Node;
        Material pointerMaterial;
        Vector4 ORIGIN = new Vector4(0, 0, 0, 1);
        String selectedBuildingName = null;

        float y_shift = -62;
        float x_shift = -28.0f;

        public MARVIN()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
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
            State.InitGoblin(graphics, Content, "manhattanville.xml");

            // Initialize the scene graph
            scene = new Scene(this);

            // Use the newton physics engine to perform collision detection
            scene.PhysicsEngine = new NewtonPhysics();

            // Set up optical marker tracking
            // Note that we don't create our own camera when we use optical marker
            // tracking. It'll be created automatically
            SetupMarkerTracking();

            createPointer();

            // Set up the lights used in the scene
            CreateLights();

            float factor = 135.0f / 1353;

            // Create 3D terrain on top of the map layout
            CreateTerrain(factor);

            // Load plain buildings
            //LoadPlainBuildings(factor);
            // Load detailed buildings
            LoadDetailedBuildings(factor);

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

            scene.RootNode.AddChild(lightNode);
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
            //captureDevice.InitVideoCapture(0, FrameRate._30Hz, Resolution._640x480, 
            //    ImageFormat.R8G8B8_24, false);
            captureDevice.InitVideoCapture(0, -1, FrameRate._30Hz, Resolution._640x480, false);

            // Add this video capture device to the scene so that it can be used for
            // the marker tracker
            scene.AddVideoCaptureDevice(captureDevice);

            // Create a optical marker tracker that uses ARTag library
            ARTagTracker tracker = new ARTagTracker();
            // Set the configuration file to look for the marker specifications
            tracker.InitTracker(638.052f, 633.673f, captureDevice.Width,
                captureDevice.Height, false, "manhattanville.cf");

            scene.MarkerTracker = tracker;

            // Create a marker node to track the ground marker arrays
            groundMarkerNode = new MarkerNode(scene.MarkerTracker, "ground");
            scene.RootNode.AddChild(groundMarkerNode);

            // Create a marker node to track a toolbar marker array. Since we expect that the 
            // toolbar marker array will move a lot, we use a large smoothing alpha.
            toolbar1MarkerNode = new MarkerNode(scene.MarkerTracker, "toolbar1");
            toolbar1MarkerNode.Smoother = new DESSmoother(0.8f, 0.8f);
            scene.RootNode.AddChild(toolbar1MarkerNode);

            // Display the camera image in the background
            scene.ShowCameraImage = true;
        }

        private void createPointer()
        {
            toolbar1Node = new GeometryNode("Toolbar1");
            toolbar1Node.Model = new Box(18, 28, 0.1f); //I think toolbar itself is 20x8
            // Set this toolbar model to act as an occluder so that it appears transparent
            toolbar1Node.IsOccluder = true;
            // Make the toolbar model to receive shadow casted by other objects with
            // CastShadows set to true
            toolbar1Node.Model.ReceiveShadows = true;
            Material toolbar1Material = new Material();
            toolbar1Material.Diffuse = Color.Gray.ToVector4();
            toolbar1Material.Specular = Color.White.ToVector4();
            toolbar1Material.SpecularPower = 20;
            toolbar1Node.Material = toolbar1Material;
            toolBar1OccluderTransNode = new TransformNode();
            toolBar1OccluderTransNode.Translation = new Vector3(3, 10, 0);
            toolbar1MarkerNode.AddChild(toolBar1OccluderTransNode);
            toolBar1OccluderTransNode.AddChild(toolbar1Node);

            //Now we create the 3D arrow pointer on top of toolbar 1
            TransformNode pointerTipTransNode = new TransformNode();
            float pointerConeHeight = 3.0f;
            Matrix pointerTipRotation = (Matrix.CreateRotationX((float)Math.PI));
            pointerTipTransNode.Rotation = Quaternion.CreateFromRotationMatrix(pointerTipRotation);
            pointerTipTransNode.Translation = new Vector3(4.0f, -3.0f, 1.3f);
            pointerTip = new GeometryNode("Pointer Tip");
            pointerTip.Model = new Cylinder(1.8f, 0.05f, pointerConeHeight, 12);
            pointerMaterial = new Material();
            pointerMaterial.Emissive = Color.Red.ToVector4();
            pointerTip.Material = pointerMaterial;

            pointerSegment = new GeometryNode("Pointer Segment");
            pointerSegment.Model = new Cylinder(1.0f, 1.0f, 2.4f * pointerConeHeight, 12);
            TransformNode pointerSegmentTransNode = new TransformNode();
            pointerSegmentTransNode.Translation = new Vector3(0.0f, -3.6f, 0.0f);
            pointerSegment.Material = pointerMaterial;
            toolbar1MarkerNode.AddChild(pointerTipTransNode);
            pointerTipTransNode.AddChild(pointerTip);
            pointerSegmentTransNode.AddChild(pointerSegment);
            pointerTipTransNode.AddChild(pointerSegmentTransNode);

            // Create a marker node to track a toolbar marker array. Since we expect that the 
            // toolbar marker array will move a lot, we use a large smoothing alpha.
            toolbar1MarkerNode = new MarkerNode(scene.MarkerTracker, "toolbar1");
            toolbar1MarkerNode.Smoother = new DESSmoother(0.8f, 0.8f);
            scene.RootNode.AddChild(toolbar1MarkerNode);
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

                    verts[index].Position = new Vector3(j * y_gap + y_shift, i * x_gap + x_shift, 
                        terrain_heights[j][i]);
                    verts[index].TextureCoordinate = new Vector2(j * tu_gap, 1 - i * tv_gap);
                    verts[index].Normal = Vector3.UnitZ;
                }
            }

            terrain.VertexBuffer = new VertexBuffer(graphics.GraphicsDevice,
                VertexPositionNormalTexture.SizeInBytes * 35, BufferUsage.None);
            terrain.SizeInBytes = VertexPositionNormalTexture.SizeInBytes;
            terrain.VertexDeclaration = new VertexDeclaration(graphics.GraphicsDevice,
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

            terrain.IndexBuffer = new IndexBuffer(graphics.GraphicsDevice, typeof(short), indices.Length,
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

            groundMarkerNode.AddChild(terrainTransNode);
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
                verts[index].Position = new Vector3(i * y_gap + y_shift, x_shift, terrainHeights[i][0]);
                verts[index].Normal = Vector3.UnitY;
                index++;

                verts[index].Position = new Vector3(i * y_gap + y_shift, x_shift, 0);
                verts[index].Normal = Vector3.UnitY;
                index++;

                verts[index].Position = new Vector3((6 - i) * y_gap + y_shift, 71.1f + x_shift, 
                    terrainHeights[6-i][4]);
                verts[index].Normal = -Vector3.UnitY;
                index++;

                verts[index].Position = new Vector3((6 - i) * y_gap + y_shift, 71.1f + x_shift, 0);
                verts[index].Normal = -Vector3.UnitY;
                index++;
            }

            for (int i = 0; i < 5; i++)
            {
                verts[index].Position = new Vector3(y_shift, (4 - i) * x_gap + x_shift, terrainHeights[0][(4 - i)]);
                verts[index].Normal = Vector3.UnitX;
                index++;

                verts[index].Position = new Vector3(y_shift, (4 - i) * x_gap + x_shift, 0);
                verts[index].Normal = Vector3.UnitX;
                index++;

                verts[index].Position = new Vector3(120.0f + y_shift, i * x_gap + x_shift, terrainHeights[6][i]);
                verts[index].Normal = -Vector3.UnitX;
                index++;

                verts[index].Position = new Vector3(120.0f + y_shift, i * x_gap + x_shift, 0);
                verts[index].Normal = -Vector3.UnitX;
                index++;
            }

            skirf.VertexBuffer = new VertexBuffer(graphics.GraphicsDevice,
                VertexPositionNormal.SizeInBytes * 48, BufferUsage.None);
            skirf.SizeInBytes = VertexPositionNormal.SizeInBytes;
            skirf.VertexDeclaration = new VertexDeclaration(graphics.GraphicsDevice,
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

            skirf.IndexBuffer = new IndexBuffer(graphics.GraphicsDevice, typeof(short), indices.Length,
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

            buildings = new List<GeometryNode>();
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

                parentTrans = new TransformNode();
                parentTrans.Translation = new Vector3(-12.5f, -15.69f, 0);
                parentTrans.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 119 * MathHelper.Pi / 180);
                groundMarkerNode.AddChild(parentTrans);

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

                        buildings.Add(building);

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

                        parentTrans.AddChild(transNode);
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

            buildings = new List<GeometryNode>();
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

                parentTrans = new TransformNode();
                parentTrans.Scale = Vector3.One * scale;
                parentTrans.Translation = new Vector3(-33.5f, -54.25f, 0);
                parentTrans.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.PiOver2);
                groundMarkerNode.AddChild(parentTrans);

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

                        buildings.Add(building);

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

                        parentTrans.AddChild(transNode);
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
            if (toolbar1MarkerNode.MarkerFound)
            {
                performPointing();
            }
            else
            {

            }
            
            base.Draw(gameTime);
        }

        private void performPointing()
        {
            Vector4 homog_pointerWorldCoords = Vector4.Transform(ORIGIN, pointerTip.WorldTransformation * toolbar1MarkerNode.WorldTransformation);
            Vector3 inhomog_pointerWorldCoords = new Vector3(homog_pointerWorldCoords.X, homog_pointerWorldCoords.Y, homog_pointerWorldCoords.Z);
            Vector3 homog_pointerScreenCoords = graphics.GraphicsDevice.Viewport.Project(inhomog_pointerWorldCoords, 
                State.ProjectionMatrix, State.ViewMatrix, Matrix.Identity);

            //GoblinXNA.UI.Notifier.AddMessage("Pointer Screen Coords: " + homog_pointerScreenCoords);

            // 0 means on the near clipping plane, and 1 means on the far clipping plane
            Vector3 nearSource = new Vector3(homog_pointerScreenCoords.X, homog_pointerScreenCoords.Y, 0);
            Vector3 farSource = new Vector3(homog_pointerScreenCoords.X, homog_pointerScreenCoords.Y, 1);

            // Now convert the near and far source to actual near and far 3D points based on our eye location
            // and view frustum
            Vector3 nearPoint = graphics.GraphicsDevice.Viewport.Unproject(nearSource,
                State.ProjectionMatrix, State.ViewMatrix, Matrix.Identity);
            Vector3 farPoint = graphics.GraphicsDevice.Viewport.Unproject(farSource,
                State.ProjectionMatrix, State.ViewMatrix, Matrix.Identity);

            // Have the physics engine intersect the pick ray defined by the nearPoint and farPoint with
            // the physics objects in the scene (which we have set up to approximate the model geometry).
            List<PickedObject> pickedObjects = ((NewtonPhysics)scene.PhysicsEngine).PickRayCast(
                nearPoint, farPoint);

            // If one or more objects intersect with our ray vector
            if (pickedObjects.Count > 0)
            {
                // Since PickedObject can be compared (which means it implements IComparable), we can sort it in 
                // the order of closest intersected object to farthest intersected object
                pickedObjects.Sort();

                // We only care about the closest picked object for now, so we'll simply display the name 
                // of the closest picked object whose container is a geometry node
                selectedBuildingName = ((GeometryNode)pickedObjects[0].PickedPhysicsObject.Container).Name;
                //label = selectedBuildingName + " is selected";

                //previouslySelectedBuilding = selectedBuilding;
                //selectedBuilding = (GeometryNode)scene.GetNode(selectedBuildingName);
            }
            else
            {
                //label = "Nothing is selected";
                //previouslySelectedBuilding = selectedBuilding;
                selectedBuildingName = null;
                //selectedBuilding = null;
            }
        }
    }
}
