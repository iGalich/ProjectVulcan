using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class Helpers
{
    #region GetWait

    private static readonly Dictionary<float, WaitForSeconds> WaitDictionary = new Dictionary<float, WaitForSeconds>();

    /// <summary>
    /// Use instaed of yield return waitforseconds in coroutines to allocate much less garbage
    /// </summary>
    /// <param name="time">How long to wait</param>
    /// <returns></returns>
    public static WaitForSeconds GetWait(float time)
    {
        if (WaitDictionary.TryGetValue(time, out var wait))
            return wait;

        WaitDictionary[time] = new WaitForSeconds(time);
        return WaitDictionary[time];
    }

    #endregion

    #region IsOverUI

    private static PointerEventData _eventDataCurrentPosition;
    private static List<RaycastResult> _results;

    /// <summary>
    /// Use to find out if mouse is over UI element
    /// </summary>
    /// <returns></returns>
    public static bool IsOverUI()
    {
        _eventDataCurrentPosition = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        _results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(_eventDataCurrentPosition, _results);
        return _results.Count > 0;
    }

    #endregion

    public static Vector2 GetWorldPositionOfCanvasElement(RectTransform element)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(element, element.position, Camera.main, out var result);
        return result;
    }

    public static void DeleteChildren(this Transform t)
    {
        foreach (Transform child in t)
            Object.Destroy(child.gameObject);
    }

    /// <summary>
    /// Converts the anchoredPosition of the first RectTransform to the second RectTransform,
    /// taking into consideration offset, anchors and pivot, and returns the new anchoredPosition
    /// </summary>
    /// <param name="from">from which recttransform to switch</param>
    /// <param name="to">to which recttransform to switch</param>
    /// <returns>returns a vector2 in the new recttranform</returns>
    public static Vector2 SwitchToRectTransform(RectTransform from, RectTransform to)
    {
        Vector2 localPoint;
        Vector2 fromPivotDerivedOffset = new Vector2(from.rect.width * 0.5f + from.rect.xMin, from.rect.height * 0.5f + from.rect.yMin);
        Vector2 screenP = RectTransformUtility.WorldToScreenPoint(null, from.position);
        screenP += fromPivotDerivedOffset;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(to, screenP, null, out localPoint);
        Vector2 pivotDerivedOffset = new Vector2(to.rect.width * 0.5f + to.rect.xMin, to.rect.height * 0.5f + to.rect.yMin);
        return to.anchoredPosition + localPoint - pivotDerivedOffset;
    }

    public static void SetCursor(Texture2D texture)
    {
        Cursor.SetCursor(texture, Vector2.zero, CursorMode.ForceSoftware);
    }

    /// <summary>
    ///     Calculus of the location of this object. Whether it is located at the top or bottom. -1 and 1 respectively.
    /// </summary>
    /// <returns></returns>
    public static int CloserEdge(this Transform transform, Camera camera, int width, int height)
    {
        //edge points according to the screen/camera
        var worldPointTop = camera.ScreenToWorldPoint(new Vector3(width / 2, height));
        var worldPointBot = camera.ScreenToWorldPoint(new Vector3(width / 2, 0));

        //distance from the pivot to the screen edge
        var deltaTop = Vector2.Distance(worldPointTop, transform.position);
        var deltaBottom = Vector2.Distance(worldPointBot, transform.position);

        return deltaBottom <= deltaTop ? 1 : -1;
    }

    private static bool IsObjectUnderMouse<T>() where T : Component
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.collider != null && hit.collider.GetComponent<T>() != null;
        }

        return false;
    }

    /// <summary>
    ///     Sets the x component of the transform's position.
    /// </summary>
    /// <param name="x">Value of x.</param>
    public static void SetX(this Transform transform, float x) =>
        transform.position = new Vector3(x, transform.position.y, transform.position.z);

    /// <summary>
    ///     Sets the y component of the transform's position.
    /// </summary>
    /// <param name="y">Value of y.</param>
    public static void SetY(this Transform transform, float y) =>
        transform.position = new Vector3(transform.position.x, y, transform.position.z);

    /// <summary>
    ///     Sets the z component of the transform's position.
    /// </summary>
    /// <param name="z">Value of z.</param>
    public static void SetZ(this Transform transform, float z) =>
        transform.position = new Vector3(transform.position.x, transform.position.y, z);
}