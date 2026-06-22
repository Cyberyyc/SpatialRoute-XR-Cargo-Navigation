using UnityEngine;

namespace SpatialRoute
{
    [RequireComponent(typeof(LineRenderer))]
    public sealed class RouteGuidancePresenter : MonoBehaviour
    {
        [SerializeField] CargoNavigationSession session;
        [SerializeField] Transform directionArrow;
        [SerializeField] float floorOffset=0.025f;
        LineRenderer line;
        void Awake(){line=GetComponent<LineRenderer>();line.useWorldSpace=true;}
        void LateUpdate()
        {
            bool active=session!=null&&session.State==NavigationState.Navigating&&session.MapOrigin!=null;
            line.enabled=active;
            if(directionArrow!=null)directionArrow.gameObject.SetActive(active);
            if(!active)return;
            var route=session.LocalRoute;line.positionCount=route.Count;
            for(int i=0;i<route.Count;i++)line.SetPosition(i,session.MapOrigin.TransformPoint(route[i])+Vector3.up*floorOffset);
            if(directionArrow==null||session.UserHead==null||route.Count==0)return;
            var target=session.MapOrigin.TransformPoint(route[Mathf.Min(session.RouteCursor,route.Count-1)]);
            var heading=target-session.UserHead.position;heading.y=0;
            if(heading.sqrMagnitude>1e-6f)directionArrow.rotation=Quaternion.LookRotation(heading,Vector3.up);
        }
    }
}
