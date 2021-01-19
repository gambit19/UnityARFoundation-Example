using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

    //Sliders to control the movement of the instantiated object..
    [SerializeField]
    private Slider horizontalSlider = null;
    [SerializeField]
    private Slider verticalSlider = null;
    private bool isSliderClicked = false;

    //Getting current and new slider values to move the instantiated object..
    float currentHSliderValue, newHSliderValue;
    float currentVSliderValue, newVSliderValue;
   
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

        //Adding listeners to the sliders
        horizontalSlider.onValueChanged.AddListener(delegate { SliderValueChanged(0); });
        verticalSlider.onValueChanged.AddListener(delegate { SliderValueChanged(1); });

        //Getting current value of slider value
        currentHSliderValue = horizontalSlider.value;
        currentVSliderValue = verticalSlider.value;
    }

    //Method to get the current touch position
    private bool GetTouchPosition(out Vector2 tapPosition)
    {
        if (Input.touchCount > 0)
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
        {
            return;
        }

        if (aRRaycastManager.Raycast(tapPosition, _hits, TrackableType.PlaneWithinPolygon))
        {
            if (!isSliderClicked)
            {
                //Getting current touch position from raycast
                var _hitPose = _hits[0].pose;
                //Setting model parent position to current touch position
                modelParent.transform.position = _hitPose.position;

                if (placedObject == null)
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
            else
            {
                Debug.Log("-->> Bool is false");
            }
        }
    }

    //Method to move the instantiated object based on the slider value..
    private void SliderValueChanged(int value)
    {
        //Getting value to move the instantiated object..
        newHSliderValue = horizontalSlider.value - currentHSliderValue;
        newVSliderValue = verticalSlider.value - currentVSliderValue;
        //Switching between horizontal and vertical sliders
        switch (value)
        {
            case 0:
                //Moving the model..
                modelParent.transform.position += new Vector3((newHSliderValue), 0f, 0f);
                //Setting the new slider value as current value
                currentHSliderValue = horizontalSlider.value;
                break;
            case 1:
                //Moving the model..
                modelParent.transform.position += new Vector3(0f, (newVSliderValue), 0f);
                //Setting the new slider value as current value
                currentVSliderValue = verticalSlider.value;
                break;
        }
    }

    //Method to disable and enable the placement of object in Update function
    public void OnSliderClickDetected(int value)
    {
        switch (value)
        {
            case 0:
                isSliderClicked = true;
                break;
            case 1:
                StopAllCoroutines();
                StartCoroutine(EnableTouch());
                break;
        }
    }

    //Coroutine to enable touch again..
    private IEnumerator EnableTouch()
    {
        yield return new WaitForEndOfFrame();
        isSliderClicked = false;
    }
}
