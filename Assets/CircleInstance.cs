using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CircleInstance : MonoBehaviour
{

    public GameObject CircleInstancePrefab;
    [HideInInspector]
    public ValueProvider ValueProvider;
    [HideInInspector]
    public float CircleDensity;
    [HideInInspector]
    public float MaxRadius;
    [HideInInspector]
    public float2 IntersectionPoint;
    private float _currentRadius;
    private LineRenderer _lineDrawer;
    private float _density = 0f;
    [HideInInspector]
    public List<GameObject> InteractableObjects = new List<GameObject>();
    [HideInInspector]
    public Color CircleColor;
    private float SpawnTimer;

    void Start()
    {
        _lineDrawer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        DrawCircle();
        SpawnTimer -= Time.deltaTime;
    }

    void DrawCircle()
    {
        _currentRadius = ValueProvider.GetDistanceToNearestGO(ValueProvider.ConvertVectorTofloat2(transform.position), MaxRadius, InteractableObjects);
        _density = 0f;
        var size = (int)((1f / CircleDensity) + 1f);
        _lineDrawer.positionCount = size;
        for (int i = 0; i < size; i++)
        {
            _density += (2.0f * Mathf.PI * CircleDensity);
            float x = _currentRadius * Mathf.Cos(_density);
            float y = _currentRadius * Mathf.Sin(_density);
            //Add the current location of the transform, otherwise it will always stay in the middle of screen
            var parentPos = GetComponentInParent<Transform>();
            _lineDrawer.SetPosition(i, new Vector2(transform.position.x + x, transform.position.y + y));
            _lineDrawer.startWidth = 0.05f;
            _lineDrawer.endWidth = 0.05f;
            // If we wanted to use random color foreach circle, use the randomColor variable
            // WARNING: This might cause Epilepsie. I'm not joking, it's disturbing
            var randomColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
            _lineDrawer.startColor = CircleColor;
        }
    }

    public IEnumerator CalculateNextCircle(Controller controllerInstance)
    {

        yield return new WaitForSeconds(0.0f);

        // float2 intersectionPoint can be null - then no intersections are found
        float2? intersectionPoint =
                        ValueProvider.GetClosestIntersectionOfLineAndCircle(
                        transform.position.x, transform.position.y, _currentRadius,
                        controllerInstance.GetCurrentPosition(),
                        controllerInstance.GetCurrentEndPointOfRay()
                        );

        // If we surpass the limit of the List, do nothing. We do NOT want an infinite amount of circles,
        // otherwise it will crash.
        if (controllerInstance.InstantiatedCircles.Count < controllerInstance.CircleAmount)
        {
            CircleInstance script = CircleInstancePrefab.GetComponent<CircleInstance>();
            if (intersectionPoint != null)
            {
                var circle = Instantiate(CircleInstancePrefab,
                             new Vector3(intersectionPoint.Value.x, intersectionPoint.Value.y, 0),
                             Quaternion.identity,
                             controllerInstance.gameObject.transform);

                // Track the amount of instantiated circles
                controllerInstance.InstantiatedCircles.Add(this.gameObject);
                script.ValueProvider = this.ValueProvider;
                script.InteractableObjects = this.InteractableObjects;
                script.MaxRadius = this.MaxRadius;
                script.CircleDensity = this.CircleDensity;
                script.CircleColor = this.CircleColor;

                StartCoroutine(script.CalculateNextCircle(controllerInstance));
            }
        }

    }
}
