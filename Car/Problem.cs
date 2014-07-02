using System;
using System.Collections.Generic;
using System.Text;

namespace Car
{
    class Prbl : problem
    {
        public Dictionary<orientation, state[,]> states = new Dictionary<orientation, state[,]>();
        public int dx, dy;
        public state current, goal;
        public List<state> changedCost = new List<state>();
        public double[,] costs;
        public void setCost(int x, int y, double cost)
        {
            setCost(new state(x,y,-1,-1,orientation.down), cost);
        }
        public bool isDeadEnd(state s)
        {
            double minCost = double.PositiveInfinity;
            foreach (state ss in successors(s))
            {
                if (cost(s,ss) <minCost) minCost = cost(s,ss);
            }
            return minCost >= maxCost;
        }
        public double maxCost = 1000;
        
        public state closestGoodPred(state s, bool updateCosts = true)
        {//'moves' robot's logical position to the closest NOT-dead-end node to s out of S's predecessors. Required to avoid massive recalculations on mapping own position's cost to infinity.
            List<state> preds = new List<state>();
            Dictionary<state, state> succ = new Dictionary<state, state>();
            preds.Add(s);
            state cur = s;
            while (isDeadEnd(cur))
            {
                foreach (state ss in predecessors(cur))
                {
                    preds.Add(ss);
                    if(!succ.ContainsKey(ss))succ.Add(ss, cur);
                }
                cur = preds[0];
                preds.Remove(cur);

            }
            state ini = cur;
            while (equals(ini , s) && false)
            {
                //setCost(ini, costs[ini.x,ini.y]);
                ini = succ[ini];
            }
            return cur;
        }
        public void setHeuristic(state sFrom, state sTo, double value)
        {
            if (!heuristics.ContainsKey(sFrom))
                heuristics.Add(sFrom, new Dictionary<state,double>());
            if (!heuristics[sFrom].ContainsKey(sTo)) heuristics[sFrom].Add(sTo, value);
            heuristics[sFrom][sTo] = value;
            
        }
        public void setCost( state s2,double cost)
        {
            if (costs[s2.x, s2.y] != cost)
            {

                costs[s2.x, s2.y] = Math.Min( cost,maxCost);
                List<state> temp = new List<state>();
                for(int i=-1; i<=1;i++)
                    for(int j=-1;j<=1;j++)
                        foreach (orientation o in orientor.movement.Keys)
                        {
                            if (!((s2.x + i -orientor.movement[o][0]< 0) || (s2.x + i -orientor.movement[o][0]> dx - 1) || (s2.y + j- orientor.movement[o][1] < 0) || (s2.y + j- orientor.movement[o][1] > dy - 1)))
                            {
                                state cand = states[o][s2.x + i - orientor.movement[o][0], s2.y + j - orientor.movement[o][1]];

                                if (!changedCost.Contains(cand)) changedCost.Add(cand);
                            }
                        }
            }
        }
        public Prbl(int dimx, int dimy, int startx = 5, int starty = 14, orientation starto = orientation.up, int goalx = 30, int goaly = 30)
        {
            dx = dimx;
            dy = dimy;
            costs = new double[dx , dy ];
            for (int i = 0; i < dx; i++)
                for (int j = 0; j < dy; j++)
                {
                    costs[i, j] = 1;
                }
            for (int o = 0; o < 8; o++)
            {
                orientation ori = (orientation)o;

                states[ori] = new state[dx, dy];
                for (int i = 0; i < dx; i++)
                    for (int j = 0; j < dy; j++)
                    {
                        states[ori][i, j] = new state(i, j, double.PositiveInfinity, double.PositiveInfinity, ori);// { x = i, y = j, a = ori };
                    }
            }
            current = states[starto][startx, starty];
            goal = states[orientation.down][goalx, goaly];
        }
        state problem.getCurrent()
        {
            return current;
        }   
        state problem.getGoal()
        {
            return goal;
        }
        public double cost(state s1, state s2)
        {
            if (s2.x == 0 || s2.y == 0 || s2.x >= costs.GetLength(0) || s2.y >= costs.GetLength(1))
                return maxCost * 100;

            double val = costs[s2.x, s2.y];
            double norm = 0;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (!((s2.x + i < 0) || (s2.x + i > dx - 1) || (s2.y + j < 0) || (s2.y + j > dy - 1) || (i == 0 && j == 0)))
                    {
                        val += costs[s2.x + i, s2.y + j] / (Math.Abs(i) + Math.Abs(j) + 1);
                        norm += 1f / (Math.Abs(i) + Math.Abs(j) + 1);
                    }
                }
            }
            val/=norm;

            if ((s1.x == s2.x) || (s1.y == s2.y))
            {
                val *= 1;//horizontal or vertical
            }
            else
            {
                val *= 1.4;//diagonal
            }
            if (s1.a != s2.a)
                val *= 1.1;//rotation
            int x = s1.x - orientor.movement[s1.a][0],
                y=s1.y - orientor.movement[s1.a][1];
            if (!((x < 0) || (x > dx - 1) || (y < 0) || (y > dy - 1)))
                if (equals(states[s2.a][x,y], s2))
                    val *= 5; //backwards


            return val;
        }
        double problem.heuristic(state s, state froms)
        {
            
            if (heuristics.ContainsKey(froms))
                if (heuristics[froms].ContainsKey(s))
                    return heuristics[froms][s];
            double distance = System.Math.Pow(System.Math.Pow((s.x - froms.x), 2) + System.Math.Pow((s.y - froms.y), 2), 0.5);
            return distance;
            /*the next 2 lines MIGHT turn heuristic into inconsistent to "H(a->c) <= H(a->b) + H(b->c)" condition. adding them made path convergence allow loops (i.e. fail to converge to any path time to time)
            double dOrient = (Math.Abs((int)s.a - (int)froms.a)) % orientor.countOri;
            return Math.Max(distance, dOrient);*/
        }
        Dictionary<state,Dictionary<state,double>> heuristics = new Dictionary<state,Dictionary<state,double>>();
        orient orientor = new orient();
        public List<state> successors(state s)
        {
            List<state> r = new List<state>();
            foreach (orientation o in orientor.adjacent(s.a))//этот цикл ещё в конце января занимал 50~100 строк
            {
                int x = s.x + orientor.movement[s.a][0],
                    y = s.y + orientor.movement[s.a][1];
                if (!((x < 0) || (x > dx - 1) || (y < 0) || (y > dy - 1))) r.Add(states[o][x, y]);
                x = s.x - orientor.movement[s.a][0];
                y = s.y - orientor.movement[s.a][1];
                if (!((x < 0) || (x > dx - 1) || (y < 0) || (y > dy - 1))) r.Add(states[o][x, y]);

            }
            
            bool flag = false;
            foreach (state st in r)
            {
                if ((st.x == goal.x) && (st.y == goal.y))
                {
                    flag = true;
                    break;
                }
            }
            if (flag) r.Add(goal);
            return r;


        }
        public List<state> predecessors(state s)
        {
            List<state> r = new List<state>();
            if (equals(s, goal))
            {
                foreach (orientation o in orientor.movement.Keys)
                {
                    r.Add(states[o][s.x - orientor.movement[o][0],s.y - orientor.movement[o][1]]);
                    r.Add(states[o][s.x + orientor.movement[o][0], s.y + orientor.movement[o][1]]);
                }
                return r;
            }

            foreach (orientation o in orientor.adjacent(s.a))//этот цикл ещё в конце января занимал 50~100 строк
            {

                int x = s.x - orientor.movement[o][0],
                    y = s.y - orientor.movement[o][1];
                if (!((x < 0) || (x > dx - 1) || (y < 0) || (y > dy - 1))) r.Add(states[o][x, y]);
                x = s.x + orientor.movement[o][0];
                y = s.y + orientor.movement[o][1];
                if (!((x < 0) || (x > dx - 1) || (y < 0) || (y > dy - 1))) r.Add(states[o][x, y]);
                
            }


            return r;
        }
        List<state> problem.changedCostSinceLastCall()
        {
            List<state> rr = changedCost;
            changedCost =new List<state>();
            return rr;
        }
        public bool equals(state s1, state s2)
        {
            return (s1.x == s2.x) && (s1.y == s2.y) && (s1.a == s2.a);
        }
    }
}
