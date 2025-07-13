using UnityEngine;
using System.Collections;

public class Visibility : MonoBehaviour
{

    public GameObject view;

    ViewPoint viewScript;
    Renderer render;
    Bounds bounds;

    // Use this for initialization
    void Start()
    {

        render = GetComponent<Renderer>();
        bounds = render.bounds;

        if (view != null)
            viewScript = view.GetComponent<ViewPoint>();
        else
            Debug.LogWarning("No view assigned in visibility script.");

    }

    // Update is called once per frame
    void Update()
    {
        //Check it has material, shader is correct and view was assigned
        if (render != null && view != null)
        {
            //Each mesh material
            for (int i = 0; i < render.materials.Length; i++)
            {
                if (render.materials[i].shader.name == "Visibility/VisibilityTransparentStandard")
                {
                    //Pass view position and forward vectors
                    render.materials[i].SetVector("_ViewPosition", view.transform.position);
                    render.materials[i].SetVector("_Forward", view.transform.forward);

                    //Write view values
                    if (render.materials[i].GetFloat("_Override") == 0)
                    {
                        render.materials[i].SetFloat("_CenterAlpha", viewScript.CenterAlpha);
                        render.materials[i].SetFloat("_RangeAlpha", viewScript.RangeAlpha);
                        render.materials[i].SetFloat("_LimitAlpha", viewScript.LimitAlpha);
                        render.materials[i].SetFloat("_LimitDistance", viewScript.LimitDistance);
                        render.materials[i].SetFloat("_AngleAlpha", viewScript.AngleAlpha);
                        render.materials[i].SetFloat("_VisibleDistance", viewScript.VisibilityDistance);
                        render.materials[i].SetFloat("_VisibleAngle", viewScript.VisibilityAngle);
                        render.materials[i].SetFloat("_OutlineInternal", viewScript.OutlineInternal);
                        render.materials[i].SetFloat("_OutlineExternal", viewScript.OutlineExternal);
                        render.materials[i].SetColor("_OutlineColour", viewScript.OutlineColor);
                        render.materials[i].SetFloat("_OutlineAlpha", viewScript.OutlineAlpha);
                    }

                    //Get bounds again in case they were modified
                    bounds = render.bounds;

                    /*
                        Closest point from view to bounds of this object
                        Used for optimizations below
                    */
                    Vector3 pointInBounds = bounds.ClosestPoint(view.transform.position);

                    /*
                        Check bounds optimization
                        If closest point of bounds of an object are beyond the full distance (visible distance + limit distance)
                        then all the pixels of the object are beyond that limit and dont need to calculate an interpolation
                    */
                    float distanceToPoint = Vector3.Distance(pointInBounds, view.transform.position);
                    float fullDistance = render.materials[i].GetFloat("_VisibleDistance") + render.materials[i].GetFloat("_LimitDistance");
                    if (distanceToPoint > fullDistance)
                    {
                        render.materials[i].EnableKeyword("BEYOND_BOUNDS");
                    }
                    else
                    {
                        render.materials[i].DisableKeyword("BEYOND_BOUNDS");
                    }

                    /*
                        Check angle optimization
                        If the best possible ray towards the center of the bounds of the object doesn't intersect a sphere with bounds.extents radius
                        then all the pixels of the object are beyond the angle and don't need to check individually
                    */
                    float angle = render.materials[i].GetFloat("_VisibleAngle") / 2;
                    Vector3 toCenter = bounds.center - view.transform.position;
                    Vector3 coneVector = Vector3.RotateTowards(view.transform.forward, toCenter, Mathf.Deg2Rad * angle, 0);
                    if (RaySphereIntersection(view.transform.position, coneVector, bounds.center, bounds.extents.magnitude))
                    {
                        /*
                        Calculate cosine of the desired angle
                        The shader will compare the cosines of the angles instead of the angle itself to save using arc cosine
                        */
                        render.materials[i].SetFloat("_VisibleAngleCosine", Mathf.Cos(Mathf.Deg2Rad * render.materials[i].GetFloat("_VisibleAngle") / 2));

                        render.materials[i].DisableKeyword("BEYOND_ANGLE");
                    }
                    else
                    {
                        render.materials[i].EnableKeyword("BEYOND_ANGLE");
                    }
                }
            }
        }
    }

    //Checks intersection between a ray and a sphere
    bool RaySphereIntersection(Vector3 rayOrigin, Vector3 rayDirection, Vector3 sphereCenter, float sphereRadius)
    {

        //Check ray origin inside
        if (Vector3.Distance(rayOrigin, sphereCenter) < sphereRadius)
            return true;

        Vector3 m = rayOrigin - sphereCenter;
        float b = Vector3.Dot(m, rayDirection);
        float c = Vector3.Dot(m, m) - sphereRadius * sphereRadius;

        // Exit if r’s origin outside s (c > 0) and r pointing away from s (b > 0)
        if (c > 0.0f && b > 0.0f)
            return false;

        // A negative discriminant corresponds to ray missing sphere
        float discr = b * b - c;
        if (discr < 0.0f)
            return false;

        return true;
    }

}