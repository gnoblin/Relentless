// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using UnityEngine;

namespace LoomNetwork.CZB.Helpers
{
    public struct IntVector2
    {
        public int x, y;

        public IntVector2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public override string ToString()
        {
            return string.Format("x:{0}; y:{1}", x, y);
        }
    }

    public struct FloatVector3
    {
        public static FloatVector3 one = new FloatVector3(1, 1, 1);

        public static FloatVector3 zero = new FloatVector3(0, 0, 0);

        public float x, y, z;

        public FloatVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public FloatVector3(float general)
        {
            x = general;
            y = general;
            z = general;
        }

        public override string ToString()
        {
            return string.Format("x:{0}; y:{1}; z:{2}", x, y, z);
        }
    }

    public class MathLib
    {
        public static Vector3 FloatVector3ToVector3(FloatVector3 vector)
        {
            return new Vector3(vector.x, vector.y, vector.z);
        }

        public static float AngleBetweenVector2(Vector2 vec1, Vector2 vec2)
        {
            Vector2 diference = vec2 - vec1;
            float sign = vec2.x > vec1.x?-1.0f:1.0f;
            return (180 + Vector2.Angle(Vector2.up, diference)) * sign;
        }
    }
}
