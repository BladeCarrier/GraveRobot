using System;
using System.Collections.Generic;
using System.Text;
using Box2DX.Common;
using System.Drawing;

namespace Car
{
    public class Helper
    {
        
        public static float GetRelAngle(float a1, float a2, float turn)//относительный угол между двумя углами (решает проблему того что 2*pi и 0 разные углы) (a2-a1)
        {
            if (a1 < 0) a1 += turn;
            if (a2 < 0) a2 += turn;
            float res = a2 - a1;
            float abs_res = System.Math.Abs(res);
            if (abs_res > turn / 2) res = (abs_res - turn) * System.Math.Sign(res);
            return res;
        }

        public static float GetRelAngle(float x1, float y1, float x2, float y2, float turn)//относительный угол между двумя углами (решает проблему того что 2*pi и 0 разные углы) (a2-a1)
        {
            var a1 = Get_Point_Angle_Relative_to_X(x1, y1, turn);
            var a2 = Get_Point_Angle_Relative_to_X(x2, y2, turn);
            return GetRelAngle(a1, a2, turn);
        }

        public static float Get_Point_Angle_Relative_to_X(float x, float y, float turn)//азимут точки (отсчитывается от оси x в сторону оси y (-pi;pi)
        {
            var res1 = System.Math.Atan2(y, x)*turn/(float)(2*System.Math.PI);
            return (float)res1;
        }
        public static void LimitAngle(ref float a, float turn)
        {
            float hturn = turn / 2;
            a %= turn;
            if (a < -hturn) a += turn;
            if (a > hturn) a -= turn;
        }

        
        public const float angle_to_rad = (float)System.Math.PI / 180;
        public const float ms_to_kmh = 3.6f;        

    }
}
