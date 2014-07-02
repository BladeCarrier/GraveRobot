using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Box2DX.Dynamics;
using Box2DX.Common;
namespace Car
{
    abstract class Agent
    {//любое поведение реализуется через агента
        //обоснуй: давать агенту доступ к car или тем более environment было бы нечестно
        public Agent()
        {
        }
        //метод react вызывается на каждом тике симуляции
        public virtual CarControl react(PerceptParams pp, float t)
        {
            return new CarControl();
        }
    }   
    class reflexAgent:Agent
    {
        System.IO.TextWriter tw;

        public reflexAgent():base()

        {
            //создает логфайл с текущим временем
            string datePatt = @"dd_MM_yyyy_hh_mm_ss";
            int i = 0;
            while (true)
            {
                string dtString = "textlog\\" + DateTime.Now.ToString(datePatt) + "robot"+ i.ToString() + ".txt";
                if (!File.Exists(dtString))
                {
                    tw = File.CreateText(dtString);
                    break;
                }
                i++;
            }
        }

        /*ЗАПИСЬ ТЕКСТОВЫХ ЛОГФАЙЛОВ*/
        public void WriteLog(System.IO.TextWriter text, float t, CarControl carcar, PerceptParams parpar)//не уверена, что от меня требовалось именно это, но...
        {
            string time, gas, steer, pos;
            if (carcar.u_steer >= 0) { steer = "; Steer: +" + String.Format("{0:000.000}", System.Math.Round(carcar.u_steer, 3)); }
            else { steer = "; Steer: " + String.Format("{0:000.000}", System.Math.Round(carcar.u_steer, 3)); }
            pos = ": Pos: " + String.Format("{0:000.000} ", System.Math.Round(parpar.p.xc, 3)) + String.Format("{0:000.000}", System.Math.Round(parpar.p.yc, 3));
            time = String.Format("{0:000.000}", System.Math.Round(t, 3));
            gas = ": Gas: " + String.Format("{0:000.000}", System.Math.Round(carcar.u_gas, 3));
            //steer = "; Steer: " + String.Format("{0:0.000}", Math.Round(carcar.u_steer, 3));
            text.WriteLine(time + gas + steer + pos);
        }
        /*КОНЕЦ ЗАПСИИ ТЕКСТОВОГО ЛОГФАЙЛА*/

        public override CarControl react(PerceptParams pp, float t)
        {
            if (pp.dist < 2.5f) return new CarControl { u_brake = 1 };
            
            CarControl cc = new CarControl { u_gas = 0.5f };

            float cm = pp.lidar_cm, d = pp.lidar_d;
            float ang_target = pp.targ_angle;
            if (d < 0.9f) /*если подъехал к цели на расстояние меньше 0.9f, то останавливается*/
            {
                cc.u_steer = 2f * cm;
            }
            else
            {
                cc.u_steer = 2f * ang_target;
            }

            WriteLog(tw,t,cc,pp);
            return cc;
        }
        public void ClearResources()
        {
            tw.Close();
        }
    }

    class dreamerAgent : reflexAgent
    {
        public Prbl pr;
        public List<state> path = new List<state>();// drawn by environment.
        public List<state> pathGone = new List<state>();// same
        public List<state> infs = new List<state>();//same
        public Dictionary<state, System.Drawing.Color> obsts = new Dictionary<state, System.Drawing.Color>();//same
        public DStar ds;
        public double angle_last = 0, tLast = 0.0001, tBack = -10;
        public double ox = 80.0 / 80,
                      oy = 50.0 / 50,
                      xs = 1,
                      ys = 1;
        public double goBackOnCollisionTimer = 0.5;
        public orient orienter = new orient();
        public dreamerAgent(Pose initial, Vec2 target, int xtiles = 80, int ytiles = 50)
            : base()
        {
            int x = Convert.ToInt32(System.Math.Round((target.X - xs) / ox)),
                y = Convert.ToInt32(System.Math.Round((target.Y - ys) / oy));

            int xi = Convert.ToInt32(System.Math.Round((initial.xc - xs) / ox)),
                yi = Convert.ToInt32(System.Math.Round((initial.yc - ys) / oy));
            pr = new Prbl(xtiles, ytiles, xi, yi, orienter.radianToOri(3.14 + initial.angle_rad), goalx: x, goaly: y);
            ds = new DStar(pr);
            path = ds.getCurrentPlan();
        }
        public double P = 2, D = 0.3; //PD-регулятор угла
        public int maxforewatch = 3;// максимальное расстояние до опорного узла
        public double gasLast;


        //a PID of speed via gas controller with these
        public double Pvel = 0.02, Ivel = 0.01, Dvel = 0.001;
        public double lastdVel, sumdVel, maxVel = 50, targVel = 30;


        public bool goingBack = false;
        public int x = -1, y = -1;
        public orientation a;
        public bool isSekou = false;
        public int xn, yn;
        public virtual void intelligentSystemCalculateWeights(PerceptParams pp, float t) { }
        public double tstand = 0, limitstand = 1;
        public Dictionary<state, double> measures = new Dictionary<state,double>();//список рутовых измерений(за счёт kaif и ksurf)
        public override CarControl react(PerceptParams pp, float t)
        {

            base.react(pp, t);

            x = Convert.ToInt32(System.Math.Round((pp.p.xc - xs) / ox));
            if (x < 0) x = 0;
            if (x > pr.dx - 1) x = pr.dx - 1;
            y = Convert.ToInt32(System.Math.Round((pp.p.yc - ys) / oy));
            if (y < 0) y = 0;
            if (y > pr.dy - 1) y = pr.dy - 1;
            a = (orienter).radianToOri(pp.p.angle_rad);

            xn = Convert.ToInt32(System.Math.Round((pp.nose.X - xs) / ox));
            if (xn < 0) xn = 0;
            if (xn > pr.dx - 1) xn = pr.dx - 1;
            yn = Convert.ToInt32(System.Math.Round((pp.nose.Y - ys) / oy));
            if (yn < 0) yn = 0;
            if (yn > pr.dy - 1) yn = pr.dy - 1;

            if (double.IsPositiveInfinity(pp.kaif))
            {

                tBack = t;
                if (!infs.Contains(pr.states[orientation.down][xn, yn]))
                    infs.Add(pr.states[orientation.down][xn, yn]);
                if (obsts.ContainsKey(pr.states[orientation.down][xn, yn]))
                    obsts.Remove(pr.states[orientation.down][xn, yn]);
                pr.setCost(xn, yn, pp.kaif);
                measures[pr.states[orientation.down][xn, yn]] = pp.kaif;

            }
            if (!double.IsPositiveInfinity(pp.ksurf))
            {
                pr.setCost(x, y, pp.ksurf);
                measures[pr.states[orientation.down][x, y]] = pp.ksurf;

                if ((pp.ksurf > 1.1f))
                {
                    obsts[pr.states[orientation.down][x, y]] = System.Drawing.Color.DarkGray;//тут был "если нет, add, иначе присвоение". 14.03.2014 11.59. Если не вызовет проблем, удалите коммент
                    if (infs.Contains(pr.states[orientation.down][xn, yn]))
                        infs.Remove(pr.states[orientation.down][xn, yn]);


                }
                else
                {
                    if (infs.Contains(pr.states[orientation.down][x, y]))
                        infs.Remove(pr.states[orientation.down][x, y]);
                    if (obsts.ContainsKey(pr.states[orientation.down][x, y]))
                        obsts.Remove(pr.states[orientation.down][x, y]);

                }
            }




            

            intelligentSystemCalculateWeights(pp, t);

            if ( (!pr.equals(pr.current  , pr.states[a][x, y])) ||pr.changedCost.Count!=0)
            {
                tstand = t;
                pathGone.Add(pr.current);
                pr.current = pr.states[a][x, y];
                if (pr.isDeadEnd(pr.states[a][x, y]))
                {
                    pr.current = pr.closestGoodPred(pr.states[a][x, y]);
                    
                }

                ds.apply();
                //try{//as long as heuristic is INCONSISTENT, it is recommended to try-catch all the pathfinding OR make sure it won't throw an exception
                path = ds.getCurrentPlan(); 
                //}catch (Exception ee) {/* throw ee;*/ }

            }


            if (t - tBack < goBackOnCollisionTimer)
            {
                gasLast = -1;
                return new CarControl { u_gas = -1f };
            }

            if (path.Count < 2) return new CarControl { u_brake = float.PositiveInfinity };
            int forewatch = System.Math.Max(2, Convert.ToInt32(System.Math.Round((double)(maxforewatch) * gasLast)));

            Vec2 cubePos = new Vec2 { X = (float)(path[System.Math.Min(forewatch, path.Count - 1)].x * ox + xs), Y = (float)(path[System.Math.Min(forewatch + 1, path.Count - 1)].y * oy + ys) };
            double angleToNextCube = System.Math.Atan2(pp.p.yc - cubePos.Y, cubePos.X - pp.p.xc);
            double r_angle = pp.p.angle_rad - angleToNextCube;
            double flag = r_angle;
            while (r_angle > System.Math.PI + 0.01)
                r_angle -= 2 * System.Math.PI;
            while (r_angle < -System.Math.PI - 0.01)
                r_angle += 2 * System.Math.PI;
            if (System.Math.Abs(r_angle) > System.Math.PI * 3f / 5f && !goingBack)
                goingBack = true;
            if (System.Math.Abs(r_angle) < System.Math.PI * 2f / 5f && goingBack)
                goingBack = false;
            double dt = t - tLast;
            double angle_diff = (r_angle - angle_last) / dt;
            if (r_angle * angle_last < 0) angle_diff = 0;

            angle_last = r_angle;

            tLast = t;
            double steer, gas;


            if (!goingBack)
                steer = P * r_angle + D * angle_diff;
            else
            {
                //суть - малые отклонения дают большой стир, при приближении к pi/2 гасится

                double sigma = System.Math.PI / (1 + System.Math.Exp(-System.Math.Abs(System.Math.PI - r_angle))) - System.Math.PI / 2;
                double d_angle = sigma;
                if (r_angle > 0) d_angle *= -1;
                steer = P * d_angle - D * angle_diff;
            }

            //регулятор скорости
            targVel = 0.3; //% of max
            int rotationsInARow = 0;
            for (int i = 1; i < path.Count; i++)
            {
                //конец пути или поворот
                if (i > 5) break;
                if (path[i - 1].x - orienter.movement[path[i - 1].a][0] == path[i].x &&
                    path[i - 1].y - orienter.movement[path[i - 1].a][0] == path[i].y && !goingBack) break;
                if (path[i - 1].x + orienter.movement[path[i - 1].a][0] == path[i].x &&
                    path[i - 1].y + orienter.movement[path[i - 1].a][0] == path[i].y && goingBack) break;
                if (targVel >= 1) break;
                //сам путь
                if (path[i - 1].a != path[i - 1].a) rotationsInARow++;
                else if (rotationsInARow > 0) rotationsInARow--;
                if (rotationsInARow > 2) break;
                targVel += 0.1 / (rotationsInARow + 1);
            }
            if (goingBack) targVel = -0.5 * targVel - 0.3;
            targVel *= maxVel;

            double vel = pp.velocity;

            double dVel = targVel - vel;
            double dAcc = dVel - lastdVel;
            lastdVel = dVel;
            sumdVel += dVel * (t - tLast);
            tLast = t;
            double relGas = dVel * Pvel + dAcc * Dvel + sumdVel * Ivel;
            gas = relGas;

            //gas = (1 / (1 + P * System.Math.Abs(r_angle) + D * System.Math.Abs(angle_diff)));
            //if (goingBack) gas = -0.3;

            CarControl cc = new CarControl { u_steer = (float)steer, u_gas = (float)(gas*(1+t-tstand)) };
            gasLast = gas * (1 + t - tstand);
            return cc;
        }
    }
    class orient
    {
        public int countOri = 8; //supported: 8
        public Dictionary<orientation,int[]> movement = new Dictionary<orientation,int[]>();//where is it moving, being directed so
        public orient()
        {
            for (int i = 0; i < countOri; i++)
            {
                movement[(orientation)i] = new int[2]; 
            }
            movement[orientation.right][0]++;
            movement[orientation.upRight][0]++;
            movement[orientation.downRight][0]++;

            movement[orientation.left][0]--;
            movement[orientation.upLeft][0]--;
            movement[orientation.downLeft][0]--;

            movement[orientation.up][1]--;
            movement[orientation.upLeft][1]--;
            movement[orientation.upRight][1]--;

            movement[orientation.down][1]++;
            movement[orientation.downLeft][1]++;
            movement[orientation.downRight][1]++;



        }
        public orientation radianToOri(double angle)
        {//assuming 0rad is strait right
            return (orientation)(Convert.ToInt32(System.Math.Round(((angle / (2 * System.Math.PI)) * countOri) + countOri)) % countOri);
        }
        public double oriToRadian(orientation o)
        {//assuming 0rad is strait right
            return (((double)((int)o)) / (double)countOri) * (2 * System.Math.PI);
        }
        public orientation opposite(orientation o)
        {
            return (orientation)(((int)o + countOri / 2) % countOri);
        }
        public List<orientation> adjacent(orientation o)
        {
            List<orientation> adj = new List<orientation>();
            adj.Add(o);
            adj.Add((orientation)(((int)o + 1)%countOri));
            adj.Add((orientation)(((int)o - 1 + countOri) % countOri));

            return adj;
        }

    }
    enum orientation
    {
        right = 0, upRight, up, upLeft, left, downLeft, down, downRight
    }

}

