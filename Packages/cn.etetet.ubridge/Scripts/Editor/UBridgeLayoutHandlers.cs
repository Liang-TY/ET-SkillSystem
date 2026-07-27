using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ET
{
    static class LayoutHelper
    {
        public static GameObject GetGameObject(int id)
        {
            return EditorUtility.InstanceIDToObject(id) as GameObject;
        }
    }

    public static class UBridgeLayoutGetHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<LayoutGetRequest>(p);
            var resp = LayoutGetResponse.Create();
            var go = LayoutHelper.GetGameObject(r?.InstanceId ?? 0);
            if (!go) { resp.Error = 3; resp.Message = "GameObject not found"; return UBridgeJsonHelper.ToJson(resp); }

            // Try each LayoutGroup type
            var hlg = go.GetComponent<HorizontalLayoutGroup>();
            var vlg = go.GetComponent<VerticalLayoutGroup>();
            var glg = go.GetComponent<GridLayoutGroup>();

            if (hlg != null)
            {
                resp.Type = "HorizontalLayoutGroup";
                resp.PaddingLeft = hlg.padding.left;
                resp.PaddingRight = hlg.padding.right;
                resp.PaddingTop = hlg.padding.top;
                resp.PaddingBottom = hlg.padding.bottom;
                resp.Spacing = hlg.spacing;
                resp.ChildAlignment = (int)hlg.childAlignment;
                resp.ReverseArrangement = hlg.reverseArrangement;
                resp.ControlChildWidth = hlg.childControlWidth;
                resp.ControlChildHeight = hlg.childControlHeight;
                resp.ChildForceExpandWidth = hlg.childForceExpandWidth;
                resp.ChildForceExpandHeight = hlg.childForceExpandHeight;
            }
            else if (vlg != null)
            {
                resp.Type = "VerticalLayoutGroup";
                resp.PaddingLeft = vlg.padding.left;
                resp.PaddingRight = vlg.padding.right;
                resp.PaddingTop = vlg.padding.top;
                resp.PaddingBottom = vlg.padding.bottom;
                resp.Spacing = vlg.spacing;
                resp.ChildAlignment = (int)vlg.childAlignment;
                resp.ReverseArrangement = vlg.reverseArrangement;
                resp.ControlChildWidth = vlg.childControlWidth;
                resp.ControlChildHeight = vlg.childControlHeight;
                resp.ChildForceExpandWidth = vlg.childForceExpandWidth;
                resp.ChildForceExpandHeight = vlg.childForceExpandHeight;
            }
            else if (glg != null)
            {
                resp.Type = "GridLayoutGroup";
                resp.PaddingLeft = glg.padding.left;
                resp.PaddingRight = glg.padding.right;
                resp.PaddingTop = glg.padding.top;
                resp.PaddingBottom = glg.padding.bottom;
                resp.SpacingX = glg.spacing.x;
                resp.SpacingY = glg.spacing.y;
                resp.ChildAlignment = (int)glg.childAlignment;
                resp.CellSizeX = glg.cellSize.x;
                resp.CellSizeY = glg.cellSize.y;
                resp.Constraint = (int)glg.constraint;
                resp.ConstraintCount = glg.constraintCount;
                resp.StartCorner = (int)glg.startCorner;
                resp.StartAxis = (int)glg.startAxis;
            }
            else
            {
                resp.Error = 3;
                resp.Message = "No LayoutGroup component found on this GameObject";
            }

            return UBridgeJsonHelper.ToJson(resp);
        }
    }

    public static class UBridgeLayoutSetHandler
    {
        public static string Handle(string p)
        {
            var r = UBridgeJsonHelper.FromJson<LayoutSetRequest>(p);
            var resp = LayoutSetResponse.Create();
            var go = LayoutHelper.GetGameObject(r?.InstanceId ?? 0);
            if (!go) { resp.Error = 3; resp.Message = "GameObject not found"; return UBridgeJsonHelper.ToJson(resp); }

            var hlg = go.GetComponent<HorizontalLayoutGroup>();
            var vlg = go.GetComponent<VerticalLayoutGroup>();
            var glg = go.GetComponent<GridLayoutGroup>();

            if (hlg != null)
            {
                hlg.padding = new RectOffset(r.PaddingLeft, r.PaddingRight, r.PaddingTop, r.PaddingBottom);
                hlg.spacing = (float)r.Spacing;
                hlg.childAlignment = (TextAnchor)r.ChildAlignment;
                hlg.reverseArrangement = r.ReverseArrangement;
                hlg.childControlWidth = r.ControlChildWidth;
                hlg.childControlHeight = r.ControlChildHeight;
                hlg.childForceExpandWidth = r.ChildForceExpandWidth;
                hlg.childForceExpandHeight = r.ChildForceExpandHeight;
                resp.Message = "HorizontalLayoutGroup updated";
            }
            else if (vlg != null)
            {
                vlg.padding = new RectOffset(r.PaddingLeft, r.PaddingRight, r.PaddingTop, r.PaddingBottom);
                vlg.spacing = (float)r.Spacing;
                vlg.childAlignment = (TextAnchor)r.ChildAlignment;
                vlg.reverseArrangement = r.ReverseArrangement;
                vlg.childControlWidth = r.ControlChildWidth;
                vlg.childControlHeight = r.ControlChildHeight;
                vlg.childForceExpandWidth = r.ChildForceExpandWidth;
                vlg.childForceExpandHeight = r.ChildForceExpandHeight;
                resp.Message = "VerticalLayoutGroup updated";
            }
            else if (glg != null)
            {
                glg.padding = new RectOffset(r.PaddingLeft, r.PaddingRight, r.PaddingTop, r.PaddingBottom);
                glg.spacing = new Vector2((float)r.SpacingX, (float)r.SpacingY);
                glg.childAlignment = (TextAnchor)r.ChildAlignment;
                glg.cellSize = new Vector2((float)r.CellSizeX, (float)r.CellSizeY);
                glg.constraint = (GridLayoutGroup.Constraint)r.Constraint;
                glg.constraintCount = r.ConstraintCount;
                glg.startCorner = (GridLayoutGroup.Corner)r.StartCorner;
                glg.startAxis = (GridLayoutGroup.Axis)r.StartAxis;
                resp.Message = "GridLayoutGroup updated";
            }
            else
            {
                resp.Error = 3;
                resp.Message = "No LayoutGroup component found on this GameObject";
            }

            return UBridgeJsonHelper.ToJson(resp);
        }
    }
}
