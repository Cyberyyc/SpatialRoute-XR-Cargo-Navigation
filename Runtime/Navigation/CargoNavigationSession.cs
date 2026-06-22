using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialRoute
{
    public enum NavigationState { Idle, Navigating, Paused, Arrived, Unreachable }
    public sealed class CargoNavigationSession : MonoBehaviour
    {
        [SerializeField] NavigationGraph graph;
        [SerializeField] Transform mapOrigin;
        [SerializeField] Transform userHead;
        [SerializeField,Min(0.05f)] float arrivalRadius=0.6f;
        [SerializeField,Min(0.05f)] float offRouteDistance=1.2f;
        [SerializeField,Min(0.1f)] float replanInterval=1f;
        [SerializeField,Min(0.05f)] float waypointRadius=0.5f;
        readonly List<Vector3> route=new List<Vector3>();
        int cursor;
        float nextReplan;
        string cargoId;
        public NavigationState State {get;private set;}
        public IReadOnlyList<Vector3> LocalRoute => route;
        public Transform MapOrigin => mapOrigin;
        public Transform UserHead => userHead;
        public int RouteCursor => cursor;
        public event Action<NavigationState> StateChanged;
        public event Action RouteChanged;

        public bool NavigateTo(string id)
        {
            if(graph==null||mapOrigin==null||userHead==null)throw new InvalidOperationException("Assign graph, map origin, and tracked headset.");
            graph.ValidateGraph();
            if(graph.Cargo(id)==null)throw new ArgumentException("Unknown cargo identifier.",nameof(id));
            cargoId=id;return Replan();
        }
        bool Replan()
        {
            route.Clear();cursor=0;nextReplan=Time.unscaledTime+replanInterval;
            var position=mapOrigin.InverseTransformPoint(userHead.position);
            bool found=AStarPlanner.TryPlan(graph,graph.Nearest(position),graph.Cargo(cargoId).nodeId,out var nodes);
            if(found)foreach(string id in nodes)route.Add(graph.Node(id).position);
            SetState(found?NavigationState.Navigating:NavigationState.Unreachable);
            RouteChanged?.Invoke();return found;
        }
        public void Pause(){if(State==NavigationState.Navigating)SetState(NavigationState.Paused);}
        public void Resume(){if(State==NavigationState.Paused)Replan();}
        public void Retry(){if(State==NavigationState.Unreachable&&!string.IsNullOrEmpty(cargoId))Replan();}
        public void Cancel(){route.Clear();cursor=0;cargoId=null;SetState(NavigationState.Idle);RouteChanged?.Invoke();}
        void SetState(NavigationState value){if(State==value)return;State=value;StateChanged?.Invoke(value);}
        void Update()
        {
            if(State!=NavigationState.Navigating||userHead==null||mapOrigin==null||route.Count==0)return;
            var user=userHead.position;
            if(PlanarDistance(user,mapOrigin.TransformPoint(route[route.Count-1]))<=arrivalRadius){SetState(NavigationState.Arrived);return;}
            while(cursor<route.Count-1&&PlanarDistance(user,mapOrigin.TransformPoint(route[cursor]))<=waypointRadius)cursor++;
            float distance=float.PositiveInfinity;
            for(int i=Math.Max(0,cursor-1);i<route.Count-1;i++)
                distance=Mathf.Min(distance,DistanceToSegment(user,mapOrigin.TransformPoint(route[i]),mapOrigin.TransformPoint(route[i+1])));
            if(route.Count==1)distance=PlanarDistance(user,mapOrigin.TransformPoint(route[0]));
            if(distance>offRouteDistance&&Time.unscaledTime>=nextReplan)Replan();
        }
        static float PlanarDistance(Vector3 a,Vector3 b){a.y=b.y=0;return Vector3.Distance(a,b);}
        static float DistanceToSegment(Vector3 p,Vector3 a,Vector3 b)
        {
            p.y=a.y=b.y=0;var ab=b-a;
            float t=ab.sqrMagnitude<1e-8f?0:Mathf.Clamp01(Vector3.Dot(p-a,ab)/ab.sqrMagnitude);
            return Vector3.Distance(p,a+t*ab);
        }
    }
}
