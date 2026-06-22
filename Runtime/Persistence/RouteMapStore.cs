using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SpatialRoute
{
    /// <summary>Versioned anchor-local map storage with validation before assignment.</summary>
    public static class RouteMapStore
    {
        [Serializable] sealed class Document
        {
            public int version=1;
            public List<RouteNode> nodes;
            public List<RouteEdge> edges;
            public List<CargoLocation> cargos;
        }
        public static void Save(string path,NavigationGraph graph)
        {
            graph.ValidateGraph();
            var document=new Document{nodes=graph.nodes,edges=graph.edges,cargos=graph.cargos};
            var full=Path.GetFullPath(path);Directory.CreateDirectory(Path.GetDirectoryName(full));
            string temp=full+"."+Guid.NewGuid().ToString("N")+".tmp";
            try
            {
                File.WriteAllText(temp,JsonUtility.ToJson(document,true));
                if(File.Exists(full))File.Replace(temp,full,full+".bak");else File.Move(temp,full);
            }
            finally {if(File.Exists(temp))File.Delete(temp);}
        }
        public static void LoadInto(string path,NavigationGraph graph)
        {
            var document=JsonUtility.FromJson<Document>(File.ReadAllText(path));
            if(document==null||document.version!=1)throw new InvalidDataException("Unsupported map version.");
            var candidate=ScriptableObject.CreateInstance<NavigationGraph>();
            try
            {
                candidate.nodes=document.nodes;candidate.edges=document.edges;candidate.cargos=document.cargos;
                candidate.ValidateGraph();
                graph.nodes=candidate.nodes;graph.edges=candidate.edges;graph.cargos=candidate.cargos;
            }
            finally {UnityEngine.Object.Destroy(candidate);}
        }
    }
}
