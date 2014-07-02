using System;
using System.Collections.Generic;
using System.Text;

namespace Car
{

    interface problem
    {
        state getCurrent();
        state getGoal();


        double cost(state s1, state s2); // cost of movement between 2 states.
        double heuristic(state s, state froms); // distance evaluation from froms to s
        List<state> successors(state s);
        List<state> predecessors(state s);
        List<state> changedCostSinceLastCall();
        bool equals(state s1, state s2);
    }
    class state
    {
        public state(int xx, int yy, double gg, double rhs_, orientation orient)
        {
            x = xx;
            y = yy;
            g = gg;
            rhs = rhs_;
            a = orient;
        }
        public int x, y;
        public orientation a;
        public double
            g,             //costsofar, inf
            rhs;           //one-step outlook, inf
        public int index = 0;

    }

    class comparer : IComparer<double[]>
    {// i compare priority tuples
        int IComparer<double[]>.Compare(double[] a, double[] b)
        {
            if (a[0] > b[0])
                return 1;
            if (a[0] < b[0])
                return -1;

            //only a[0]==b[0]
            if (a[1] > b[1])
                return 1;
            if (a[1] < b[1])
                return -1;
            //a[1] == b[1]

            if (a[2] > b[2])
                return 1;
            if (a[2] < b[2])
                return -1;
            return 0;
        }

    }
    class DStar
    {
        SortedDictionary<double[], state> U = new SortedDictionary<double[], state>(new comparer());
        Dictionary<state, double[]> Uinv = new Dictionary<state, double[]>();
        double Km = 0;
        state sGoal, sLast;
        public problem probl;


        public DStar(problem p)
        {
            probl = p;
            sLast = probl.getCurrent();
            sGoal = probl.getGoal();
            sGoal.rhs = 0;
            uAdd(sGoal);
            convergeToShortestPath();
        }
        void updateVertex(state u)
        {//"expand" function
            if (!probl.equals(u, sGoal))
            {//update rhs
                
                double min = double.PositiveInfinity;
                foreach (state s in probl.successors(u))
                {
                    double v = s.g + probl.cost(u, s);
                    if (v < min) min = v;
                }
                u.rhs = min;
            }
            double[] key;
            if (inU(u, out key))
            {//remove from U list if there

                uRemove(key);
            }
            if (u.g != u.rhs)
            {//if inconsistent, add to U list
                uAdd(u);
            }

        }
        double[] calculateKey(state s)
        {//compute a priority tuple for a given state. Used in U list.
            Double[] r = { Math.Min(s.g, s.rhs) + probl.heuristic(s, sLast) + Km, Math.Min(s.g, s.rhs), s.index };
            return r;
        }
        void convergeToShortestPath()
        {//iterates until an optimal path can be produced.
            //                  uTopKey < sLast key
            while ((compare(uTop().Key, calculateKey(sLast)) < 0) || (sLast.rhs != sLast.g))
            {
                
                double[] Kold = uTop().Key;
                state u = uPop();

        
                if (compare(Kold, calculateKey(u)) < 0) //key(u)>kOld
                    uAdd(u);
                else if (u.g > u.rhs)
                {
                    u.g = u.rhs;
                    foreach (state pred in probl.predecessors(u))
                        updateVertex(pred);
                }
                else
                {
                    u.g = double.PositiveInfinity;
                    foreach (state pred in probl.predecessors(u))
                        updateVertex(pred);
                    updateVertex(u);
                }
            }
        }

        public state decide()
        {//recommends where to go next
            if (sLast.g == double.PositiveInfinity) throw new Exception("No valid path");
            List<state> succ = probl.successors(sLast);
            double minV = double.PositiveInfinity;
            state minS = new state(0, 0, 0, 0, orientation.down);
            foreach (state s in succ)
            {
                double v = s.g + probl.cost(sLast, s);
                if (v < minV) { minV = v; minS = s; }
            }
            return minS;

        }
        public void apply(state newLocation = null)
        {
            if (newLocation == null) newLocation = probl.getCurrent();
            if (sLast != newLocation)
            {
                Km += probl.heuristic(newLocation, sLast);
                sLast = newLocation;
            }
            List<state> changes = probl.changedCostSinceLastCall();
            foreach (state s in changes)
                updateVertex(s);
            convergeToShortestPath();

        }
        public List<state> getCurrentPlan()
        {
            List<state> path = new List<state>();
            if (sLast.g == double.PositiveInfinity) throw new Exception("No valid path");
            state sEst = sLast;
            while (!probl.equals(sEst, sGoal))
            {
                double minV = double.PositiveInfinity;
                state minS = null;
                List<state> succ = probl.successors(sEst);
                foreach (state s in succ)
                {
                    double v = s.g +probl.cost(sEst, s);
                    if (v < minV) { minV = v; minS = s; }
                }
                if (path.Contains(minS))
                {
                    throw new Exception("Need to apply cost changes first");
                }
                path.Add(minS);
                sEst = minS;
            }
            return path;
        }
        //auxilary
        KeyValuePair<double[], state> uTop()
        {
            double[] prior = { double.PositiveInfinity, double.PositiveInfinity };
            if (U.Count == 0) return new KeyValuePair<double[], state>(prior, new state(-1, -1, double.PositiveInfinity, double.PositiveInfinity, orientation.down));
            SortedDictionary<double[], state>.Enumerator en = U.GetEnumerator();
            en.MoveNext();
            return en.Current;
        }
        state uPop()
        {
            KeyValuePair<double[], state> p = uTop();
            U.Remove(p.Key);
            Uinv.Remove(p.Value);
            return p.Value;
        }
        void uRemove(double[] key)
        {
            state u = U[key];
            U.Remove(key);
            Uinv.Remove(u);
        }

        bool inU(state s, out double[] key)
        {
            key = null;
            if (Uinv.ContainsKey(s))
            { key = Uinv[s]; return true; }
            return false;
        }
        void uAdd(state value)
        {

            if (Uinv.ContainsKey(value))
                return;//reason: states are unique and the main loop is effectively adapting the weights. (if changed continue)
            while (U.ContainsKey(calculateKey(value))) value.index++;
            Uinv.Add(value, calculateKey(value));
            U.Add(calculateKey(value), value);

        }
        int compare(double[] a, double[] b)
        {
            if (a[0] > b[0])
                return 1;
            if (a[0] < b[0])
                return -1;

            //only a[0]==b[0]
            if (a[1] > b[1])
                return 1;
            if (a[1] < b[1])
                return -1;
            //a[1] == b[1]
            return 0;
        }

    }

}
