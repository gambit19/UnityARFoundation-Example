using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
public class ObjectPlacement : MonoBehaviour
{
    //This is the object which we will place in the AR view, assign it in the Unity Ediot Inspector
    [SerializeField]
    private GameObject modelToPlace = null;
    //This is the object we want to parent the instantiated model to..
    [SerializeField]
    private GameObject modelParent = null;

    //This is a reference to the object we have spawned, in case we need to use it for further modification like scaling, etc..
    private GameObject placedObject;
    private ARRaycastManager aRRaycastManager;

    //This is the touch position on the screen
    private Vector2 tapPosition;
    //Reference to the list of raycast hits
    static List<ARRaycastHit> _hits = new List<ARRaycastHit>();

    private void Awake()
    {
        //Getting the ARRaycast Manager component from the Gameobject
        aRRaycastManager = GetComponent<ARRaycastManager>();
        //Disabling multi touch as we require only one touch at a time
        Input.multiTouchEnabled = false;
    }

    //Method to get the current touch position
    private bool GetTouchPosition(out Vector2 tapPosition)
    {
        if(Input.touchCount > 0)
        {
            //Number of touches is greater than zero, we are getting the current touch position on the screen
            tapPosition = Input.GetTouch(0).position;
            return true;
        }
        //Number of touches is zero, returning false..
        tapPosition = default;
        return false;
    }

    private void Update()
    {
        //No touches detected..
        if (!GetTouchPosition(out Vector2 tapPosition))
            return;
        
        if(aRRaycastManager.Raycast(tapPosition, _hits, TrackableType.PlaneWithinPolygon))
        {
            //Getting current touch position from raycast
            var _hitPose = _hits[0].pose;
            //Setting model parent position to current touch position
            modelParent.transform.position = _hitPose.position;

            if(placedObject == null)
            {
                //No objects spawned yet, instantiate the model we want to create at given position and rotation
                placedObject = Instantiate(modelToPlace, _hitPose.position, _hitPose.rotation);
            }
            else
            {
                //Object already exists, change position
                placedObject.transform.position = _hitPose.position;
            }
            //Setting instantiated model's parent to model parent
            placedObject.gameObject.transform.parent = modelParent.transform;
        }
    }
}
