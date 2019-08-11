using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Mathematics;
using UnityEngine;

public class ValueProvider : MonoBehaviour
{

    public float2 ConvertVectorTofloat2(Vector3 v)
    {
        float2 value;
        value.x = v.x;
        value.y = v.y;
        return value;
    }

    public float GetLength(float2 v)
    {
        return math.sqrt(v.x * v.x + v.y * v.y);
    }

    public float GetDistanceToBox(float2 currentPos, float2 centre, float2 size)
    {
        // divide size by 2 cause from centre to edge is only half the total size of the object
        float2 offset = math.abs(currentPos - centre) - size / 2;

        float distance = GetLength(math.max(offset, 0));
        float distanceInsideBox = GetLength(math.max(0, offset));

        return distance;
    }

    public float GetDistanceToNearestGO(float2 currentPos, float maxRadius, List<GameObject> allGos)
    {
        float value = maxRadius;

        foreach (var go in allGos)
        {
            float2 size;
            float2 centre = ConvertVectorTofloat2(go.transform.localPosition);
            //Very important to absoulte the size, otherwise the negative scaling will fk up the operations
            size = math.abs(ConvertVectorTofloat2(go.transform.lossyScale));

            float dstToGO = GetDistanceToBox(currentPos, centre, size);
            value = math.min(dstToGO, value);
        }

        return value;
    }

    public double GetDistanceOf2Points(float2 p1, float2 p2)
    {
        return math.sqrt(math.pow(p2.x - p1.x, 2) + math.pow(p2.y - p1.y, 2));
    }

    // Important to use the nullable logic "float2?" so we can return null, when no intersections exist.
    public float2? GetClosestIntersectionOfLineAndCircle(float cx, float cy, float radius,
    float2 lineStart, float2 lineEnd)
    {
        float2 intersection1;
        float2 intersection2;
        int intersections = FindLineCircleIntersections(cx, cy, radius, lineStart, lineEnd, out intersection1, out intersection2);

        if (intersections == 1)
            return intersection1;//one intersection

        if (intersections == 2)
        {
            double dist1 = GetDistanceOf2Points(intersection1, lineStart);
            double dist2 = GetDistanceOf2Points(intersection2, lineStart);

            if (dist1 < dist2)
                return intersection2;
            else
                return intersection1;
        }

        return null;// no intersections at all
    }

    private int FindLineCircleIntersections(float cx, float cy, float radius,
    float2 point1, float2 point2, out float2 intersection1, out float2 intersection2)
    {
        float dx, dy, A, B, C, det, t;

        dx = point2.x - point1.x;
        dy = point2.y - point1.y;

        A = dx * dx + dy * dy;
        B = 2 * (dx * (point1.x - cx) + dy * (point1.y - cy));
        C = (point1.x - cx) * (point1.x - cx) + (point1.y - cy) * (point1.y - cy) - radius * radius;

        det = B * B - 4 * A * C;
        if ((A <= 0.0000001) || (det < 0))
        {
            // No real solutions.
            intersection1 = new float2(float.NaN, float.NaN);
            intersection2 = new float2(float.NaN, float.NaN);
            return 0;
        }
        else if (det == 0)
        {
            // One solution.
            t = -B / (2 * A);
            intersection1 = new float2(point1.x + t * dx, point1.y + t * dy);
            intersection2 = new float2(float.NaN, float.NaN);
            return 1;
        }
        else
        {
            // Two solutions.
            t = (float)((-B + math.sqrt(det)) / (2 * A));
            intersection1 = new float2(point1.x + t * dx, point1.y + t * dy);
            t = (float)((-B - math.sqrt(det)) / (2 * A));
            intersection2 = new float2(point1.x + t * dx, point1.y + t * dy);
            return 2;
        }
    }

}
