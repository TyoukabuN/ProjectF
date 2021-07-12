
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TMesh
{
    public static class TCameraUtility
    {
        public static bool TryGetCameraMesh<Mesh>(out Mesh tCameraMesh)where Mesh:TMeshBase
        {
            tCameraMesh = GameObject.FindObjectOfType<Mesh>();

            if (tCameraMesh == null)
            {
                GameObject gobj = new GameObject("TCameraMesh");
                gobj.transform.position = Vector3.zero;
                tCameraMesh = gobj.AddComponent<Mesh>();
            }

            //if (tCameraMesh != null)
            //{
            //    TCameraMesh.currentTCameraMesh = tCameraMesh;
            //}
            
            return tCameraMesh != null;
        }
        /// <summary>
        /// Is the vertices order of polygon clockwise
        /// </summary>
        /// <param name="vertices"></param>
        /// <returns>The sign of the result of the shoelace formula</returns>
        public static bool IsPositiveOrder(TVertex[] vertices)
        {
            return CalOrienttionSign(vertices) < 0;
        }

        public static bool IsPositiveOrder(TTrangle tTrangle)
        {
            return CalOrienttionSign(tTrangle.vertices) < 0;
        }

        /// <summary>
        /// Using Shoelace Formula
        /// </summary>
        /// <param name="vertices"></param>
        /// <returns>The result of the shoelace formula</returns>
        public static float CalOrienttionSign(TVertex[] vertices)
        {
            float positivePart = 0.0f;
            float negativePart = 0.0f;

            int n = vertices.Length - 1;

            for (int i = 0; i < n; i++)
            {
                positivePart += vertices[i].x * vertices[i + 1].z;
                negativePart += vertices[i + 1].x * vertices[i].z;
            }
            positivePart += vertices[n].x * vertices[0].z;
            negativePart += vertices[0].x * vertices[n].z;

            float area = (positivePart - negativePart) * 0.5f;

            return area;
        }
        public static Vector3[] GetVectices(TCameraVertex[] camVertices)
        {
            var m_Vertices = new List<Vector3>();
            for (int i = 0; i < camVertices.Length; i++)
            {
                if (camVertices[i] != null)
                {
                    m_Vertices.Add(camVertices[i].transform.position);
                }
            }
            return m_Vertices.ToArray();
        }
        public static Vector3 CalBaryCenter(Vector3[] trangle)
        {
            return CalCentroid(trangle);
        }
        public static Vector3 CalCentroid(Vector3[] trangle)
        {
            var A = trangle[0];
            var B = trangle[1];
            var C = trangle[2];

            float devTri = 0.3333333f;

            Vector3 baryCenter = (A + B + C) * devTri;

            return baryCenter;
        }
        public static bool IsInsideTrangle(Vector3[] trangle, Vector3 point, out float[] weight)
        {
            var A = trangle[0];
            var B = trangle[1];
            var C = trangle[2];
            var P = point;
            //Adjust all vertices's y to zero
            A.y = 0;
            B.y = 0;
            C.y = 0;
            P.y = 0;

            bool res = true;
            // Prepare our barycentric variables
            Vector3 u = B - A;
            Vector3 v = C - A;
            Vector3 w = P - A;

            Vector3 vCrossW = Vector3.Cross(v, w);
            Vector3 vCrossU = Vector3.Cross(v, u);
            // Test sign of r
            res = res && Vector3.Dot(vCrossW, vCrossU) >= 0;

            Vector3 uCrossW = Vector3.Cross(u, w);
            Vector3 uCrossV = Vector3.Cross(u, v);

            // Test sign of t
            res = res && Vector3.Dot(uCrossW, uCrossV) >= 0;

            // At this piont, we know that r and t and both > 0
            float denom = uCrossV.magnitude;
            float r = vCrossW.magnitude / denom;
            float t = uCrossW.magnitude / denom;

            weight = new float[] { 1.0f - (r + t), r, t };

            res = res && (r <= 1 && t <= 1 && r + t <= 1);

            return res;
        }

        #region FOR_GIZMOS

        /// <summary>
        /// use area of trangle to calculate
        /// </summary>
        /// <param name="trangle"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static bool IsInsideTrangleS(Vector3[] trangle, Vector3 point)
        {
            var A = trangle[0];
            var B = trangle[1];
            var C = trangle[2];
            var P = point;
            A.y = 0;
            B.y = 0;
            C.y = 0;
            P.y = 0;
            // Prepare our barycentric variables
            Vector3 u = B - A;
            Vector3 v = C - A;
            Vector3 w = P - A;

            Vector3 vCrossW = Vector3.Cross(v, w);
            Vector3 vCrossU = Vector3.Cross(v, u);
            // Test sign of r
            if (Vector3.Dot(vCrossW, vCrossU) < 0)
                return false;

            Vector3 uCrossW = Vector3.Cross(u, w);
            Vector3 uCrossV = Vector3.Cross(u, v);

            // Test sign of t
            if (Vector3.Dot(uCrossW, uCrossV) < 0)
                return false;

            // At this piont, we know that r and t and both > 0
            float denom = uCrossV.magnitude;
            float r = vCrossW.magnitude / denom;
            float t = uCrossW.magnitude / denom;

            return (r <= 1 && t <= 1 && r + t <= 1);
        }

        /// <summary>
        /// use barycentric technique to calculate
        /// </summary>
        /// <param name="trangle"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static bool IsInsideTrangleS2(Vector3[] trangle,Vector3 point)
        { 
            var A = trangle[0];
            var B = trangle[1];
            var C = trangle[2];
            var P = point;
            A.y = 0;
            B.y = 0;
            C.y = 0;
            P.y = 0;
            // Compute vectors        
            var v0 = C - A;
            var v1 = B - A;
            var v2 = P - A;

            // Compute dot products
            var dot00 = Vector3.Dot(v0, v0);
            var dot01 = Vector3.Dot(v0, v1);
            var dot02 = Vector3.Dot(v0, v2);
            var dot11 = Vector3.Dot(v1, v1);
            var dot12 = Vector3.Dot(v1, v2);

            // Compute barycentric coordinates
            var invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
            var u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            var v = (dot00 * dot12 - dot01 * dot02) * invDenom;

            // Check if point is in triangle
            return (u >= 0) && (v >= 0) && (u + v < 1);
        }
        #endregion
    }
}
