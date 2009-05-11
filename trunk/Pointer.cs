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

            double minDistanceSoFar = 10000000.0f;
            int highlightedObjectType = -1;
            int highlightedObjectIndex = -1;
            double distanceToPointer;

            for (int i = 0; i < 8; i++) //for each building
            {
                Vector3 homog_buildingScreenCoords = getScreenCoords(i, global.BUILDING);
                distanceToPointer = screenDistance(homog_pointerScreenCoords, homog_buildingScreenCoords);

                if (i == 0)
                {
                    //GoblinXNA.UI.Notifier.AddMessage("Building 0 Screen Coords: " + homog_buildingScreenCoords);
                }

                if (distanceToPointer < minDistanceSoFar)
                {
                    highlightedObjectType = global.BUILDING;
                    highlightedObjectIndex = i;
                    minDistanceSoFar = distanceToPointer;
                }
            }

            //Now do "for each attribute"/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            //Now we do left cone
            Vector3 homog_leftConeScreenCoords = getScreenCoords(-1, global.LEFT_CONE);
            distanceToPointer = screenDistance(homog_pointerScreenCoords, homog_leftConeScreenCoords);
            if (distanceToPointer < minDistanceSoFar)
            {
                highlightedObjectType = global.LEFT_CONE;
                highlightedObjectIndex = -1;
                minDistanceSoFar = distanceToPointer;
            }

            //Now we do right cone
            Vector3 homog_rightConeScreenCoords = getScreenCoords(-1, global.RIGHT_CONE);
            distanceToPointer = screenDistance(homog_pointerScreenCoords, homog_rightConeScreenCoords);
            if (distanceToPointer < minDistanceSoFar)
            {
                highlightedObjectType = global.RIGHT_CONE;
                highlightedObjectIndex = -1;
                minDistanceSoFar = distanceToPointer;
            }


            if (minDistanceSoFar <= 30.0f) //if something's close enough to be selected
            {
                global.typeOfObjectBeingHighlighted = highlightedObjectType;
                global.indexOfObjectBeingHighlighted = highlightedObjectIndex;
                
                global.resetObjectColors();
                global.pointerMaterial = new Material();
                global.pointerMaterial.Emissive = Color.Yellow.ToVector4();
                global.pointerTip.Material = global.pointerMaterial;
                global.pointerSegment.Material = global.pointerMaterial;

                global.highlight(global.indexOfObjectBeingHighlighted, global.typeOfObjectBeingHighlighted, Color.Yellow);
                global.label = "Select " + global.indexOfObjectBeingHighlighted + "?";
            }
            else
            {
                global.typeOfObjectBeingHighlighted = global.NOTHING;
                global.indexOfObjectBeingHighlighted = global.NOTHING;
                
                global.resetObjectColors();
                global.pointerMaterial = new Material();
                global.pointerMaterial.Emissive = Color.Red.ToVector4();
                global.pointerTip.Material = global.pointerMaterial;
                global.pointerSegment.Material = global.pointerMaterial;
                global.label = "Nothing is highlighted.";
            }


            


            /*// Have the physics engine intersect the pick ray defined by the nearPoint and farPoint with
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
                global.label = global.selectedBuildingName + " is selected";

                //previouslySelectedBuilding = selectedBuilding;
                //selectedBuilding = (GeometryNode)scene.GetNode(selectedBuildingName);
            }
            else
            {
                global.label = "Nothing is selected";
                //previouslySelectedBuilding = selectedBuilding;
                global.selectedBuildingName = null;
                //selectedBuilding = null;
            }*/
        }

        public Vector3 getScreenCoords(int index, int typeOfNode)
        {
            if (typeOfNode == global.BUILDING)
            {
                Vector4 homog_buildingWorldCoords = Vector4.Transform(global.ORIGIN, global.buildingTransNodes[index].WorldTransformation * global.blockMarker.WorldTransformation);
                Vector3 inhomog_buildingWorldCoords = new Vector3(homog_buildingWorldCoords.X, homog_buildingWorldCoords.Y, homog_buildingWorldCoords.Z);
                Vector3 homog_buildingScreenCoords = global.graphics.GraphicsDevice.Viewport.Project(inhomog_buildingWorldCoords,
                    State.ProjectionMatrix, State.ViewMatrix, Matrix.Identity);
                return homog_buildingScreenCoords;
            }
            else if (typeOfNode == global.ATTRIBUTE)
            {
                Vector4 homog_attributeWorldCoords = Vector4.Transform(global.ORIGIN, global.attributeTransNodes[index].WorldTransformation * global.groundMarkerNode.WorldTransformation);
                Vector3 inhomog_attributeWorldCoords = new Vector3(homog_attributeWorldCoords.X, homog_attributeWorldCoords.Y, homog_attributeWorldCoords.Z);
                Vector3 homog_attributeScreenCoords = global.graphics.GraphicsDevice.Viewport.Project(inhomog_attributeWorldCoords,
                    State.ProjectionMatrix, State.ViewMatrix, Matrix.Identity);

                return homog_attributeScreenCoords;
            }
            else if (typeOfNode == global.LEFT_CONE)
            {
                Vector4 homog_coneWorldCoords = Vector4.Transform(global.ORIGIN, global.leftConeTransNode.WorldTransformation * global.notebookShowcaseTransNode.WorldTransformation * 
                    global.notebookBoxTransNode.WorldTransformation * global.groundMarkerNode.WorldTransformation);
                Vector3 inhomog_coneWorldCoords = new Vector3(homog_coneWorldCoords.X, homog_coneWorldCoords.Y, homog_coneWorldCoords.Z);
                Vector3 homog_coneScreenCoords = global.graphics.GraphicsDevice.Viewport.Project(inhomog_coneWorldCoords,
                    State.ProjectionMatrix, State.ViewMatrix, Matrix.Identity);
                return homog_coneScreenCoords;
            }
            else //if (typeOfNode == global.RIGHT_CONE)
            {
                Vector4 homog_coneWorldCoords = Vector4.Transform(global.ORIGIN, global.rightConeTransNode.WorldTransformation * global.notebookShowcaseTransNode.WorldTransformation * 
                    global.notebookBoxTransNode.WorldTransformation * global.groundMarkerNode.WorldTransformation);
                Vector3 inhomog_coneWorldCoords = new Vector3(homog_coneWorldCoords.X, homog_coneWorldCoords.Y, homog_coneWorldCoords.Z);
                Vector3 homog_coneScreenCoords = global.graphics.GraphicsDevice.Viewport.Project(inhomog_coneWorldCoords,
                    State.ProjectionMatrix, State.ViewMatrix, Matrix.Identity);
                return homog_coneScreenCoords;
            }
        }


        public double screenDistance(Vector3 screenCoords1, Vector3 screenCoords2)
        {
            return Math.Sqrt( Math.Pow(screenCoords1.X - screenCoords2.X, 2.0f) + Math.Pow(screenCoords1.Y - screenCoords2.Y, 2.0f));
        }
    }
}
