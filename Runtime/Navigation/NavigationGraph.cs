using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialRoute
{
    [Serializable] public sealed class RouteNode
    {
        public string id;
        public Vector3 position;
    }
    [Serializable] public sealed class RouteEdge
    {
        public string from, to;
        public float multiplier=1f;
        public bool bidirectional=true;
        public bool enabled=true;
    }
    [Serializable] public sealed class CargoLocation
    {
        public string id, displayName, nodeId;
    }
    [CreateAssetMenu(menuName="SpatialRoute/Navigation graph")]
    public sealed class NavigationGraph : ScriptableObject
    {
        public List<RouteNode> nodes=new List<RouteNode>();
        public List<RouteEdge> edges=new List<RouteEdge>();
        public List<CargoLocation> cargos=new List<CargoLocation>();

        public void ValidateGraph()
        {
            if(nodes==null||edges==null||cargos==null) throw new ArgumentException("Graph lists cannot be null.");
            var ids=new HashSet<string>(StringComparer.Ordinal);
            foreach(var n in nodes)
                if(n==null||string.IsNullOrWhiteSpace(n.id)||!ids.Add(n.id)||!Finite(n.position))
                    throw new ArgumentException("Node identifiers must be unique; positions must be finite.");
            foreach(var e in edges)
                if(e==null||!ids.Contains(e.from)||!ids.Contains(e.to)||e.from==e.to||!Finite(e.multiplier)||e.multiplier<1)
                    throw new ArgumentException("Edges require valid distinct endpoints and a finite cost multiplier >= 1.");
            var cargoIds=new HashSet<string>(StringComparer.Ordinal);
            foreach(var c in cargos)
                if(c==null||string.IsNullOrWhiteSpace(c.id)||!cargoIds.Add(c.id)||!ids.Contains(c.nodeId))
                    throw new ArgumentException("Cargo identifiers must be unique and reference existing nodes.");
        }
        public RouteNode Node(string id) => nodes.Find(n=>n.id==id);
        public CargoLocation Cargo(string id) => cargos.Find(c=>c.id==id);
        public string Nearest(Vector3 localPosition)
        {
            string best=null; float distance=float.PositiveInfinity;
            foreach(var node in nodes)
            {
                float d=(node.position-localPosition).sqrMagnitude;
                if(d<distance) {distance=d;best=node.id;}
            }
            return best;
        }
        static bool Finite(float v) => !float.IsNaN(v)&&!float.IsInfinity(v);
        static bool Finite(Vector3 v) => Finite(v.x)&&Finite(v.y)&&Finite(v.z);
    }
}
