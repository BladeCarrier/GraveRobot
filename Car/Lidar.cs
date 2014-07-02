using System;
using System.Collections.Generic;
using System.Text;
using Box2DX.Dynamics;
using Box2DX.Collision;
using Box2DX.Common;
using System.Drawing;

namespace Car
{

    // параметры лидара (LIDAR - LIght Detection and Ranging)
    public struct LidarParams
    {
        public int num_rays;
        public float fov_deg;
        public float dir_deg;
        public float d0, dist;
        public float x0, y0;

        public void InitDefault()
        {
            num_rays = 10; fov_deg = 60;
            dir_deg = 0; d0 = 1; dist = 5;
            x0 = 0; y0 = 0;
        }
    }

    // лидар - устройство определения дальности до препятствий при помощи лазерных лучей
    public class Lidar
    {
        public LidarParams p;
        Car parent_car;

        public float[] data;

        public Lidar(LidarParams p_, Car parent_car_)
        {
            p = p_;
            parent_car = parent_car_;
            data = new float[p.num_rays];
        }

        void GetSegment(int i, ref Box2DX.Collision.Segment s)
        {
            float a = (-p.fov_deg * (-0.5f + i / (p.num_rays - 1f)) + p.dir_deg) * Helper.angle_to_rad;
            float sin = (float)System.Math.Sin(a), cos = (float)System.Math.Cos(a);
            var v0 = new Vec2(p.d0 * sin, p.d0 * cos);
            var v = new Vec2(p.dist * sin, p.dist * cos);
            s.P1 = parent_car.body.GetWorldPoint(v0) + parent_car.body.GetWorldVector(new Vec2(p.x0, p.y0));
            s.P2 = s.P1 + parent_car.body.GetWorldVector(v);
        }
        public void Simulate()
        {
            var world = parent_car.body.GetWorld();
            var s = new Box2DX.Collision.Segment();
            for (int i = 0; i < p.num_rays; i++)
            {
                GetSegment(i, ref s);

                float lambda;
                Vec2 normal;
                var x = world.RaycastOne(s, out lambda, out normal, false, null);

                if (x == null) lambda = 1;
                data[i] = lambda * p.dist;
            }
        }
        public void Draw(GraphicsHelper gh)
        {
            var world = parent_car.body.GetWorld();
            var s = new Box2DX.Collision.Segment();
            var pen = new Pen(System.Drawing.Color.Gray, 1 / gh.Scale);
            for (int i = 0; i < p.num_rays; i++)
            {
                GetSegment(i, ref s);

                float lambda = data[i] / p.dist;

                float b0 = 0.3f, b = System.Math.Max(b0, 1 - lambda);
                byte b1 = (byte)((1 - b) * 255);
                Vec2 p1 = s.P1, p2 = (1 - lambda) * s.P1 + lambda * s.P2;

                pen.Color = System.Drawing.Color.FromArgb(b1, b1, b1);
                gh.G.DrawLine(pen, p1.X, p1.Y, p2.X, p2.Y);
            }
        }

        public bool is_calculated = false;
        float lidar_x; //степень смещения центра масс показаний лидара (к наибоолее свободной зоне) [-1...+1]
        float lidar_d; //среднее расстояние до препятствий [0...1]
        public void Calculate(out float lidar_x_, out float lidar_d_)
        {
            if (is_calculated)
            {
                lidar_x_ = lidar_x;
                lidar_d_ = lidar_d;
                return;
            }

            //Simulate();  //симулируется в машине
             
            float MX = 0, M = 0;
            for (int i = 0; i < data.Length; i++)
            {
                MX += data[i] * i;
                M += data[i];
            }
            float h = (data.Length - 1) / 2f;
            float x_ = (M == 0)?0:MX / M - h; lidar_x = lidar_x_ = x_ / h;
            float d_ = M / data.Length; lidar_d = lidar_d_ = d_ / p.dist;

            is_calculated = true;
        }

    }
}
