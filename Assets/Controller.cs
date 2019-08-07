using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Controller : MonoBehaviour
{
    /* The purpose of this script, step by step:
     *  
     *  Step 1) Instantiate the start point of the ray
     *  Step 2) Instantiate the first circle, by changing the radius of the circle to the nearest GO
     *  Step 3) Get the intersection of the ray and the circle
     *  Step 4) At that intersection, instantiate a new circle. The intersection point will be the middle of the circle.
     *  Step 5) Loop the creation of circles. Every circle has one "follow-up" circle, which all have the same functions:
     *          -> Radius  = nearest GO.
     *          -> Point of Creation is the intersection point of the afore instantiated circle with the ray
     * 
     * CAUTION: For the Ray, I use the Debug.DrawRay Method, which is only rendered when the "Gizmos" is activated in Unity!
     */

     // The ValueProvider does all the math for us
    private ValueProvider _valueProvider;

    [Header("Ray Properties")]
    public float MaxRayLength = 5;
    public float Lifespan;
    public UnityEngine.Color RayColor;
    Vector3 endPointOfHitRay;

    [Header("Circle Properties")]
    [SerializeField]

    private float RotationSpeed;
    public float MaxRadius;
    public GameObject CircleInstancePrefab;
    public float CircleDensity = 0.01f;
    private List<GameObject> _interactableObjects = new List<GameObject>();
    [HideInInspector]
    public List<GameObject> InstantiatedCircles = new List<GameObject>();
    public int CircleAmount;
    public Color CircleColor;

    public float2 GetCurrentEndPointOfRay()
    {
        return _valueProvider.ConvertVectorTofloat2(endPointOfHitRay);
    }
    public float2 GetCurrentPosition()
    {
        return _valueProvider.ConvertVectorTofloat2(transform.position);
    }


    void Start()
    {
        _valueProvider = GetComponent<ValueProvider>();
        LoadAllInteractableShapes();
        SetupLocation();
    }


    void Update()
    {
        RotateObject();
        DrawRay();
    }
    void RotateObject()
    {
        this.transform.Rotate(new Vector2(1, 0), RotationSpeed);
    }

    void SetupLocation()
    {
        var randomX = UnityEngine.Random.Range(-3, 3);
        var randomY = UnityEngine.Random.Range(-3, 3);

        // The GameObject that this script is attached to is the source of the ray.
        // So randomize the spawn location once, then draw the ray in the updateMethod.
        this.transform.position = new Vector3(randomX, randomY, 0);

        // Also, since we are doing 2D here, turn object rotation to 90°, so that the ray faces
        // the camera in a 90° angle. Otherwise it wont be rendered
        this.transform.Rotate(new Vector2(0, 90));

        // Make sure the the circle is rendered AFTER we randomized the start position.
        DrawInitialCircle();
    }

    void LoadAllInteractableShapes()
    {
        // We need to define every object, that we need to calculate the distance to for the circle radius.
        var allShapes = FindObjectsOfType(typeof(GameObject));
        _interactableObjects.Clear();
        //Filter them by tags with the static List of registered Tags
        foreach (var o in allShapes)
        {
            GameObject castedO = (GameObject)o;
            if (RegisteredTags.WellKnownInteractableTags.Contains(castedO.transform.tag))
            {
                _interactableObjects.Add(castedO);
            }
        }
    }

    void UpdateCircleProperties()
    { }

    void DrawRay()
    {
        RaycastHit hit;

        // Does the ray intersect any objects
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, MaxRayLength))
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, RayColor, Lifespan);
            endPointOfHitRay = transform.TransformDirection(Vector3.forward) * hit.distance;
        }
        // If not, ray length = maxLength
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * MaxRayLength, RayColor, Lifespan);
            endPointOfHitRay = transform.TransformDirection(Vector3.forward) * MaxRayLength;
        }
    }

    void DrawInitialCircle()
    {
        // We initialize the first circle here. 
        // The CircleInstancePrefab has its own calculations-power, so all we do is define the spawn position.
        // and feed the variables of the circle with information.

        // We keep controll over the instantiated circle by creating it as a child of this gameObject
        // and storing the instance into a variable
        var circle = Instantiate(CircleInstancePrefab, this.transform.position, Quaternion.identity, this.transform);
        var script = circle.GetComponent<CircleInstance>();
        script.ValueProvider = this._valueProvider;
        script.InteractableObjects = this._interactableObjects;
        script.MaxRadius = this.MaxRadius;
        script.CircleDensity = this.CircleDensity;
        script.CircleColor = this.CircleColor;

        // After the circle is rendered, we call the Method CalculateNextCircle() from the instance.
        // This method will then calculate the next circle while also calling the CalculateNextCircle() Method
        // from its own instance. We now have a loop
        // Also: We pass the Controller Class intro the loop, so we have full access to every circle that is instantiated.
        // We need to limit the amount of circles and therefore the loop, otherwise it will jsut freeze. So we track every
        // instantiated circle in to the InstantiatedCircles List
        StartCoroutine(script.CalculateNextCircle(this.GetComponent<Controller>()));
    }
}
