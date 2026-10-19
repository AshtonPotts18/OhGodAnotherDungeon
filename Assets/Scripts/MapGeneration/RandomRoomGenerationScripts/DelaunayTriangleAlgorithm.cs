using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


struct Edge
{
    public Vector2 a;
    public Vector2 b;
    public float weight;
    public Edge(Vector2 _a, Vector2 _b)
    {
        a = _a;
        b = _b;
        weight = Vector2.Distance(_a, _b);
    }
}
public struct Triangle
{
    public Vector2 a;
    public Vector2 b;
    public Vector2 c;

    public Triangle(Vector2 _a, Vector2 _b, Vector2 _c)
    {
        a = _a;
        b = _b;
        c = _c;
    }

    public Vector2 this[int i] // this is the bracket overload for the triangle struct. Will return one of three points or throw an error if out of range like usual.
    {
        get
        {
            switch (i)
            {
                case 0:
                    return a;
                case 1:
                    return b;
                case 2:
                    return c;
                default:
                    throw new System.ArgumentOutOfRangeException();
            }
        }
    }
}
public class DelaunayTriangleAlgorithm
{

    static bool IsEdgesEqual(Edge _edge1, Edge _edge2)
    {
        const float epsilon = 0.00001f; // near zero number

        bool sameDirection = (_edge1.a - _edge2.a).sqrMagnitude < epsilon * epsilon && (_edge1.b - _edge2.b).sqrMagnitude < epsilon * epsilon;

        bool reverseDirection = (_edge1.a - _edge2.b).sqrMagnitude < epsilon * epsilon && (_edge1.b - _edge2.a).sqrMagnitude < epsilon * epsilon;

        return sameDirection || reverseDirection;
    }



    static bool IsTrianglesEqual(Triangle _triangle1, Triangle _triangle2)
    {
        const float epsilon = 0.00001f;


        Vector2[] triangle1Points = { _triangle1.a, _triangle1.b, _triangle1.c };
        Vector2[] triangle2Points = { _triangle2.a, _triangle2.b, _triangle2.c };

        bool[] sharedPoints = new bool[3];

        foreach (Vector2 point in triangle1Points)
        {
            bool foundPoint = false;

            for (int triangle2Point = 0; triangle2Point < 3; triangle2Point++)
            {
                if (!sharedPoints[triangle2Point] && (point - triangle2Points[triangle2Point]).sqrMagnitude < epsilon * epsilon) //checks the distance between the two points and returns true if they are the same
                {
                    sharedPoints[triangle2Point] = true;
                    foundPoint = true;
                }
                if (!foundPoint)
                {
                    return false; // this means that one of the points is not shared
                }
            }


        }


        bool xCoords = Mathf.Abs(_triangle1.a.x * _triangle1.b.x * _triangle1.c.x - _triangle2.a.x * _triangle2.b.x * _triangle2.c.x) < epsilon; // if sum of triangle1 x coords == sum of triangle2 x coords, the result will be 0; this includes a minor margin for error
        bool yCoords = Mathf.Abs(_triangle1.a.y * _triangle1.b.y * _triangle1.c.y - _triangle2.a.y * _triangle2.b.y * _triangle2.c.y) < epsilon;
        return xCoords && yCoords;
    }

    static bool IsPointInCircumcircleOfTriangle(Vector2 _point, Triangle _triangle)
    {
        float lineABx_Difference = _triangle.b.x - _triangle.a.x;
        float lineABy_Difference = _triangle.b.y - _triangle.a.y;
        float lineACx_Difference = _triangle.c.x - _triangle.a.x;
        float lineACy_Difference = _triangle.c.y - _triangle.a.y;
        float lineAB_Determinate = (_triangle.b.x * _triangle.b.x) - (_triangle.a.x * _triangle.a.x) + (_triangle.b.y * _triangle.b.y) - (_triangle.a.y * _triangle.a.y);

        if (Mathf.Abs(lineAB_Determinate) < 0.000001f) return false; // prevents dividion by a near zero number
        float lineAC_Determinate = (_triangle.c.x * _triangle.c.x) - (_triangle.a.x * _triangle.a.x) + (_triangle.c.y * _triangle.c.y) - (_triangle.a.y * _triangle.a.y); // d^2 = (bx^2 + by^2) - (ax^2 + ay^2)
        if (Mathf.Abs(lineAC_Determinate) < 0.000001f) return false; // prevents dividion by a near zero number

        float scaling = 1.0f / (2.0f * (lineABx_Difference * lineACy_Difference - lineABy_Difference * lineACx_Difference));
        float xCircumCircleCenter = ((_triangle.c.y - _triangle.a.y) * lineAB_Determinate + (_triangle.a.y - _triangle.b.y) * lineAC_Determinate) * scaling;
        float yCircumCircleCenter = ((_triangle.a.x - _triangle.c.x) * lineAB_Determinate + (_triangle.b.x - _triangle.a.x) * lineAC_Determinate) * scaling;
        float xPointFromCircleCenter_Distance = _triangle.a.x - xCircumCircleCenter;
        float yPointFromCircleCenter_Distance = _triangle.a.y - yCircumCircleCenter;
        float radiusSquared = xPointFromCircleCenter_Distance * xPointFromCircleCenter_Distance + yPointFromCircleCenter_Distance * yPointFromCircleCenter_Distance;
        float distanceSquared = (xCircumCircleCenter - _point.x) * (xCircumCircleCenter - _point.x) + (yCircumCircleCenter - _point.y) * (yCircumCircleCenter - _point.y);
        return (distanceSquared - radiusSquared) <= 0.00001f;
    }

    static Triangle CreateSuperTriangle(List<Vector2> _points, out Vector4 _bounds)
    {
        float minXBounds = float.PositiveInfinity;
        float minYBounds = float.PositiveInfinity;
        float maxXBounds = float.NegativeInfinity;
        float maxYBounds = float.NegativeInfinity;

        for (int i = 0; i < _points.Count; i++)
        {
            Vector2 currPoint = _points[i];
            minXBounds = Mathf.Min(minXBounds, currPoint.x);
            maxXBounds = Mathf.Max(maxXBounds, currPoint.x);
            minYBounds = Mathf.Min(minYBounds, currPoint.y);
            maxYBounds = Mathf.Max(maxYBounds, currPoint.y);
        }

        _bounds = new Vector4(minXBounds, minYBounds, maxXBounds, maxYBounds);

        float maxDistance = Mathf.Max(maxXBounds - minXBounds, maxYBounds - minYBounds);
        float xMidPoint = (minXBounds + maxXBounds) * 0.5f;
        float yMidPoint = (minYBounds + maxYBounds) * 0.5f;

        Vector2 bottomLeft = new Vector2(xMidPoint - 20f * maxDistance, yMidPoint - maxDistance);
        Vector2 top = new Vector2(xMidPoint, yMidPoint + 20f * maxDistance);
        Vector2 bottomRight = new Vector2(xMidPoint + 20f * maxDistance, yMidPoint - maxDistance);
        return new Triangle(bottomLeft, top, bottomRight);
    }


    public static List<Triangle> DelaunayTriangulation(List<Vector2> _pointInput) // pointList is a set of coordinates defining the points to be triangulated
    {
        List<Triangle> triangulation_output = new List<Triangle>();
        Triangle superTriangle = CreateSuperTriangle(_pointInput, out Vector4 bounds);
        triangulation_output.Add(superTriangle);
        for (int i = 0; i < _pointInput.Count; i++)
        {
            Vector2 currPoint = _pointInput[i];
            List<Triangle> badTriangles = new List<Triangle>();

            for (int j = 0; j < triangulation_output.Count; j++) // checks the output for any bad triangles
            {
                Triangle currTriangle = triangulation_output[j];
                if (IsPointInCircumcircleOfTriangle(currPoint, currTriangle))
                {
                    badTriangles.Add(currTriangle);
                }
            }

            List<Edge> polygon = new List<Edge>();

            for (int currBadTriangle = 0; currBadTriangle < badTriangles.Count; currBadTriangle++) // checks ever bad triangle for boundry edges to add to the polygon
            {
                Triangle currTriangle = badTriangles[currBadTriangle];

                for (int currEdge = 0; currEdge < 3; currEdge++)
                {
                    Edge currTriangleEdge = new Edge(currTriangle[currEdge], currTriangle[(currEdge + 1) % 3]);
                    List<Edge> badEdgesList = new List<Edge>();

                    for (int triangleCheck = 0; triangleCheck < badTriangles.Count; triangleCheck++) // adds the edges from the bad triangles to a list
                    {
                        if (triangleCheck == currBadTriangle) continue;
                        Triangle checkedTriangle = badTriangles[triangleCheck];

                        for (int triangleCheckEdge = 0; triangleCheckEdge < 3; triangleCheckEdge++)
                            badEdgesList.Add(new Edge(checkedTriangle[triangleCheckEdge], checkedTriangle[(triangleCheckEdge + 1) % 3]));
                    }

                    bool containsEdge = false;
                    for (int edge = 0; edge < badEdgesList.Count; edge++)
                    {
                        if (IsEdgesEqual(badEdgesList[edge], currTriangleEdge)) containsEdge = true;
                    }
                    if (containsEdge == false)
                    {
                        polygon.Add(currTriangleEdge);
                    }
                }
            }
            for (int currBadTriangle = 0; currBadTriangle < badTriangles.Count; currBadTriangle++) // searches the polygon for bad triangles and removed them
            {
                for (int polygonTriangle = triangulation_output.Count - 1; polygonTriangle >= 0; polygonTriangle--)
                {
                    if (IsTrianglesEqual(triangulation_output[polygonTriangle], badTriangles[currBadTriangle]))
                    {
                        triangulation_output.RemoveAt(polygonTriangle);
                    }
                }
            }
            for (int polygonEdge = 0; polygonEdge < polygon.Count; polygonEdge++) // this for loop creates new triangles using the points
            {   // re-triangulate the polygonal hole
                Edge currPolygonEdge = polygon[polygonEdge];
                Triangle currTriangle = new Triangle(currPoint, currPolygonEdge.a, currPolygonEdge.b);
                triangulation_output.Add(currTriangle);
            }
        }
        for (int triangle = triangulation_output.Count - 1; triangle >= 0; triangle--) //out of bounds check
        {   // done inserting points, now clean up
            bool xPointA = triangulation_output[triangle].a.x < bounds.x || triangulation_output[triangle].a.x > bounds.z;
            bool yPointA = triangulation_output[triangle].a.y < bounds.y || triangulation_output[triangle].a.y > bounds.w;
            bool xPointB = triangulation_output[triangle].b.x < bounds.x || triangulation_output[triangle].b.x > bounds.z;
            bool yPointB = triangulation_output[triangle].b.y < bounds.y || triangulation_output[triangle].b.y > bounds.w;
            bool xPointC = triangulation_output[triangle].c.x < bounds.x || triangulation_output[triangle].c.x > bounds.z;
            bool yPointC = triangulation_output[triangle].c.y < bounds.y || triangulation_output[triangle].c.y > bounds.w;
            if (xPointA || yPointA || xPointB || yPointB || xPointC || yPointC) triangulation_output.RemoveAt(triangle); //if triangle is out of bounds remove it
        }
        return triangulation_output;
    }









}