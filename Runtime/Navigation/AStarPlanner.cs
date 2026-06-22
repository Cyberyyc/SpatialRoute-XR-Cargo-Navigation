using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialRoute
{
    /// <summary>A* with a Euclidean lower bound and explicit directed aisle connectivity.</summary>
    public static class AStarPlanner
    {
        struct Arc { public string to; public float cost; public Arc(string t,float c){to=t;cost=c;} }
        public static bool TryPlan(NavigationGraph graph,string start,string goal,out List<string> path)
        {
            graph.ValidateGraph(); path=new List<string>();
            if(graph.Node(start)==null||graph.Node(goal)==null) return false;
            var nodes=new Dictionary<string,RouteNode>();
            var adjacency=new Dictionary<string,List<Arc>>();
            foreach(var node in graph.nodes){nodes[node.id]=node;adjacency[node.id]=new List<Arc>();}
            foreach(var edge in graph.edges)
            {
                if(!edge.enabled) continue;
                float cost=Vector3.Distance(nodes[edge.from].position,nodes[edge.to].position)*edge.multiplier;
                adjacency[edge.from].Add(new Arc(edge.to,cost));
                if(edge.bidirectional) adjacency[edge.to].Add(new Arc(edge.from,cost));
            }
            var open=new List<string>{start}; var closed=new HashSet<string>();
            var costs=new Dictionary<string,float>{{start,0}}; var parents=new Dictionary<string,string>();
            while(open.Count>0)
            {
                int best=0; float bestScore=float.PositiveInfinity;
                for(int i=0;i<open.Count;i++)
                {
                    float score=costs[open[i]]+Vector3.Distance(nodes[open[i]].position,nodes[goal].position);
                    if(score<bestScore){best=i;bestScore=score;}
                }
                string current=open[best];open.RemoveAt(best);
                if(current==goal)
                {
                    path.Add(current);
                    while(parents.TryGetValue(current,out var previous)){path.Add(previous);current=previous;}
                    path.Reverse();return true;
                }
                closed.Add(current);
                foreach(var arc in adjacency[current])
                {
                    if(closed.Contains(arc.to))continue;
                    float proposed=costs[current]+arc.cost;
                    if(costs.TryGetValue(arc.to,out var old)&&proposed>=old)continue;
                    costs[arc.to]=proposed;parents[arc.to]=current;
                    if(!open.Contains(arc.to))open.Add(arc.to);
                }
            }
            return false;
        }
    }
}
