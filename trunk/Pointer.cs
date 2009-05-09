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
    public class Pointer
    {
        public GlobalVariables global;

        public Pointer(ref GlobalVariables g)
        {
            global = g;
        }

        public void createPointer()
        {
            global.toolbar1Node = new GeometryNode("Toolbar1");
            global.toolbar1Node.Model = new Box(18, 28, 0.1f); //I think toolbar itself is 20x8
            // Set this toolbar model to act as an occluder so that it appears transparent
            global.toolbar1Node.IsOccluder = true;
            // Make the toolbar model to receive shadow casted by other objects with
            // CastShadows set to true
            global.toolbar1Node.Model.ReceiveShadows = true;
            Material toolbar1Material = new Material();
            toolbar1Material.Diffuse = Color.Gray.ToVector4();
            toolbar1Material.Specular = Color.White.ToVector4();
            toolbar1Material.SpecularPower = 20;
            global.toolbar1Node.Material = toolbar1Material;
            global.toolBar1OccluderTransNode = new TransformNode();
            global.toolBar1OccluderTransNode.Translation = new Vector3(3, 10, 0);
            global.toolbar1MarkerNode.AddChild(global.toolBar1OccluderTransNode);
            global.toolBar1OccluderTransNode.AddChild(global.toolbar1Node);

            //Now we create the 3D arrow pointer on top of toolbar 1
            TransformNode pointerTipTransNode = new TransformNode();
            float pointerConeHeight = 3.0f;
            Matrix pointerTipRotation = (Matrix.CreateRotationX((float)Math.PI));
            pointerTipTransNode.Rotation = Quaternion.CreateFromRotationMatrix(pointerTipRotation);
            pointerTipTransNode.Translation = new Vector3(4.0f, -3.0f, 1.3f);
            global.pointerTip = new GeometryNode("Pointer Tip");
            global.pointerTip.Model = new Cylinder(1.8f, 0.05f, pointerConeHeight, 12);
            global.pointerMaterial = new Material();
            global.pointerMaterial.Emissive = Color.Red.ToVector4();
            global.pointerTip.Material = global.pointerMaterial;

            global.pointerSegment = new GeometryNode("Pointer Segment");
            global.pointerSegment.Model = new Cylinder(1.0f, 1.0f, 2.4f * pointerConeHeight, 12);
            TransformNode pointerSegmentTransNode = new TransformNode();
            pointerSegmentTransNode.Translation = new Vector3(0.0f, -3.6f, 0.0f);
            global.pointerSegment.Material = global.pointerMaterial;
            global.toolbar1MarkerNode.AddChild(pointerTipTransNode);
            pointerTipTransNode.AddChild(global.pointerTip);
            pointerSegmentTransNode.AddChild(global.pointerSegment);
            pointerTipTransNode.AddChild(pointerSegmentTransNode);

            // Create a marker node to track a toolbar marker array. Since we expect that the 
            // toolbar marker array will move a lot, we use a large smoothing alpha.
            global.toolbar1MarkerNode = new MarkerNode(global.scene.MarkerTracker, "toolbar1");
            global.toolbar1MarkerNode.Smoother = new DESSmoother(0.8f, 0.8f);
            global.scene.RootNode.AddChild(global.toolbar1MarkerNode);
        }

        public void performPointing()
        {
            Vector4 homog_pointerWorldCoords = Vector4.Transform(global.ORIGIN, global.pointerTip.WorldTransformation * global.toolbar1MarkerNode.WorldTransformation);
            Vector3 inhomog_pointerWorldCoords = new Vector3(homog_pointerWorldCoords.X, homog_pointerWorldCoords.Y, homog_pointerWorldCoords.Z);
            Vector3 homog_pointerScreenCoords = global.graphics.GraphicsDevice.Viewport.Project(inhomog_pointerWorldCoords,
                State.ProjectionMatrix, State.ViewMatrix, Matrix.Identity);

            //GoblinXNA.UI.Notifier.AddMessage("Pointer Screen Coords: " + homog_pointerScreenCoords);

            // 0 means on the near clipping plane, and 1 means on the far clipping plane
            Vector3 nearSource = new Vector3(homog_pointerScreenCoords.X, homog_pointerScreenCoords.Y, 0);
            Vector3 farSource = new Vector3(homog_pointerScreenCoords.X, homog_pointerScreenCoords.Y, 1);

            // Now convert the near and far source to actual near and far 3D points based on our eye location
            // and view frustum
            Vector3 nearPoint = global.graphics.GraphicsDevice.Viewport.Unproject(nearSource,
                State.ProjectionMatrix, State.ViewMatrix, Matrix.Identity);
            Vector3 farPoint = global.graphics.GraphicsDevice.Viewport.Unproject(farSource,
                State.ProjectionMatrix, State.ViewMatrix, Matrix.Identity);

            // Have the physics engine intersect the pick ray defined by the nearPoint and farPoint with
            // the physics objects in the scene (which we have set up to approximate the model geometry).
            List<PickedObject> pickedObjects = ((NewtonPhysics)global.scene.PhysicsEngine).PickRayCast(
                nearPoint, farPoint);

            // If one or more objects intersect with our ray vector
            if (pickedObjects.Count > 0)
            {
                // Since PickedObject can be compared (which means it implements IComparable), we can sort it in 
                // the order of closest intersected object to farthest intersected object
                pickedObjects.Sort();

                // We only care about the closest picked object for now, so we'll simply display the name 
                // of the closest picked object whose container is a geometry node
                global.selectedBuildingName = ((GeometryNode)pickedObjects[0].PickedPhysicsObject.Container).Name;
                //label = selectedBuildingName + " is selected";

                //previouslySelectedBuilding = selectedBuilding;
                //selectedBuilding = (GeometryNode)scene.GetNode(selectedBuildingName);
            }
            else
            {
                //label = "Nothing is selected";
                //previouslySelectedBuilding = selectedBuilding;
                global.selectedBuildingName = null;
                //selectedBuilding = null;
            }
        }
    }
}
