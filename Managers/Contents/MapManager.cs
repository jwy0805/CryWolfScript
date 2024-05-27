using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Numerics;
using Google.Protobuf.Protocol;
using ServerCore;
using Vector3 = UnityEngine.Vector3;


public struct Pos
{
    public Pos(int z, int x)
    {
        Z = z;
        X = x;
    }
    public int Z;
    public int X;
}

public struct PQNode : IComparable<PQNode>
{
    public float F;
    public float G;
    public float Z;
    public float X;

    public int CompareTo(PQNode other)
    {
        if (F < other.F) return -1;
        return F > other.F ? 1 : 0;
    }
}

public struct Region
{
    public int Id;
    public Pos CenterPos;
    public List<int> Neighbor;
    public List<Pos> Vertices;

    public Region(int id)
    {
        Id = id;
        CenterPos = new Pos();
        Neighbor = new List<int>();
        Vertices = new List<Pos>();
    }
}

public class MapManager
{
    public Grid CurrentGrid { get; private set; }
    public float MinX = -25f;
    public float MaxX = 25f;
    public float MinZ = -60f;
    public float MaxZ = 60f;
    public float GroundY = 6f;
    public float AirY = 8.5f;
    
    public int SizeX => (int)((MaxX - MinX) * 4 + 1); // grid = 0.25
    public int SizeZ => (int)((MaxZ - MinZ) * 4 + 1);
    
    private bool[,] _collisionGround;
    private bool[,] _collisionAir;

    private List<Region> _regionGraph = new();
    private int[,] _connectionMatrix;

    public bool CanGoGround(Vector3 cellPos)
    {
        if (cellPos.x < MinX || cellPos.x > MaxX) return false;
        if (cellPos.z < MinZ || cellPos.z > MaxZ) return false;

        int x = (int)((cellPos.x - MinX) * 4);
        int z = (int)(MaxZ - cellPos.z * 4);
        return !_collisionGround[z, x];
    }
    
    public bool CanGoAir(Vector3 cellPos)
    {
        if (cellPos.x < MinX || cellPos.x > MaxX) return false;
        if (cellPos.z < MinZ || cellPos.z > MaxZ) return false;

        int x = (int)((cellPos.x - MinX) * 4);
        int z = (int)(MaxZ - cellPos.z * 4);
        return !_collisionAir[z, x];
    }

    public void LoadMap(int mapId = 1)
    {
        DestroyMap();
        
        string mapName = "Map_" + mapId.ToString("000");
        GameObject go = Managers.Resource.Instantiate($"Map/{mapName}");
        go.name = "Map";

        CurrentGrid = go.GetComponent<Grid>();
        // PosInfo
        
        // Collision 관련 파일
        TextAsset txt = Managers.Resource.Load<TextAsset>($"Map/{mapName}");
        StringReader reader = new StringReader(txt.text);
        
        int xCount = (int)((MaxX - MinX) * 4 + 1);
        int zCount = (int)((MaxZ - MinZ) * 4 + 1);
        _collisionGround = new bool[zCount, xCount];
        _collisionAir = new bool[zCount, xCount];

        for (int z = 0; z < zCount; z++)
        {
            string line = reader.ReadLine();
            for (int x = 0; x < xCount; x++)
            {
                if (line != null)
                {
                    _collisionGround[z, x] = line[x] == '2' || line[x] == '4';
                    _collisionAir[z, x] = line[x] == '4';
                }
            }
        }
    }

    public void DestroyMap()
    {
        GameObject map = GameObject.Find("Map");
        if (map != null)
        {
            Managers.Resource.Destroy(map);
            CurrentGrid = null;
        }
    }

    private void SetMap()
    {
        
    }
    
    private void SortPointsCcw(List<Pos> points)
    {
        float sumX = 0;
        float sumZ = 0;

        for (int i = 0; i < points.Count; i++)
        {
            sumX += points[i].X;
            sumZ += points[i].Z;
        }
        
        float averageX = sumX / points.Count;
        float averageZ = sumZ / points.Count;
        
        points.Sort((lhs, rhs) =>
        {
            double lhsAngle = Math.Atan2(lhs.Z - averageZ, lhs.X - averageX);
            double rhsAngle = Math.Atan2(rhs.Z - averageZ, rhs.X - averageX);
            
            if (lhsAngle < rhsAngle) return -1;
            if (lhsAngle > rhsAngle) return 1;
            double lhsDist = Math.Sqrt(Math.Pow(lhs.Z - averageZ, 2) + Math.Pow(lhs.X - averageX, 2));
            double rhsDist = Math.Sqrt(Math.Pow(rhs.Z - averageZ, 2) + Math.Pow(rhs.X - averageX, 2));
            if (lhsDist < rhsDist) return 1;
            if (lhsDist > rhsDist) return -1;
            return 0;
        });
    }
    
    private Pos FindCenter(List<Pos> vertices)
    {
        int minZ = vertices.Select(v => v.Z).ToList().Min();
        int maxZ = vertices.Select(v => v.Z).ToList().Max();
        int minX = vertices.Select(v => v.X).ToList().Min();
        int maxX = vertices.Select(v => v.X).ToList().Max();
        int centerZ = (minZ + maxZ) / 2;
        int centerX = (minX + maxX) / 2;

        int size = 1;
        int startZ = centerZ;
        int startX = centerX;

        while (startZ >= minZ && startX >= minX && startZ < maxZ && startX < maxX)
        {
            for (int i = startZ - size; i <= startZ + size; i++)
            {
                for (int j = startX - size; j <= startX + size; j++)
                {
                    if (i >= 0 && i < maxZ && j >= 0 && j < maxX && _collisionGround[i, j] == false)
                    {
                        return new Pos { Z = i, X = j };
                    }
                }
            }

            size++;
            startZ = centerZ - size;
            startX = centerX - size;
        }

        return new Pos { Z = -1, X = -1 };
    }

    #region RegionDirection

    private List<Vector3> _west = new()
    {
        new Vector3(-62, 0, -16),
        new Vector3(-19, 0, -16),
        new Vector3(-19, 0, 20),
        new Vector3(-62, 0, 20)
    };

    private List<Vector3> _south = new()
    {
        new Vector3(-13, 0, -16),
        new Vector3(-13, 0, -21),
        new Vector3(13, 0, -21),
        new Vector3(13, 0, -16)
    };

    private List<Vector3> _east = new()
    {
        new Vector3(13, 0, 10),
        new Vector3(13, 0, -16),
        new Vector3(70, 0, -16),
        new Vector3(70, 0, 10),
    };

    private List<Vector3> _north = new()
    {
        new Vector3(-19, 0, 50),
        new Vector3(-19, 0, 20),
        new Vector3(15, 0, 20),
        new Vector3(15, 0, 50)
    };

    private List<Vector3> _mid = new()
    {
        new Vector3(-19, 0, 20),
        new Vector3(-19, 0, -16),
        new Vector3(19, 0, -16),
        new Vector3(19, 0, 20)
    };
    
    private int _idGenerator = 0;

    #endregion

    private int GetSideLen(float len, float minSide)
    {
        return len / 2 < minSide ? (int)len : GetSideLen(len / 2, minSide);
    }
    
    public void DivideRegion(List<Vector3> region, float lenSide)
    {
        float minX = region.Min(v => v.x);
        float maxX = region.Max(v => v.x);
        float minZ = region.Min(v => v.z);
        float maxZ = region.Max(v => v.z);

        int sideX = GetSideLen(maxX - minX, lenSide);
        int sideZ = GetSideLen(maxZ - minZ, lenSide);

        int remainX = (int)(maxX - minX) % sideX;
        int remainZ = (int)(maxZ - minZ) % sideZ;

        for (float i = minZ; i < maxZ; i += sideZ)
        {
            for (float j = minX; j < maxX; j += sideX)
            {
                if (i + 2 * sideZ > maxZ) i += remainZ;
                if (j + 2 * sideX > maxX) j += remainX;
                
                List<Pos> vertices = new List<Pos>
                {
                    Cell2Pos(new Vector3(j, 0, i)),
                    Cell2Pos(new Vector3(j, 0, i + sideZ)),
                    Cell2Pos(new Vector3(j + sideX, 0, i + sideZ)),
                    Cell2Pos(new Vector3(j + sideX, 0, i))
                };
                
                SortPointsCcw(vertices);
                
                Region newRegion = new()
                {
                    Id = _idGenerator,
                    CenterPos = FindCenter(vertices),
                    Vertices = vertices,
                };
                
                _regionGraph.Add(newRegion);
                _idGenerator++;
            }
        }
    }

    private int[,] RegionConnectivity()
    {
        int[,] connectionMatrix = new int[_regionGraph.Count, _regionGraph.Count];
        
        for (int i = 0; i < _regionGraph.Count; i++)
        {
            List<Pos> region = _regionGraph[i].Vertices;
            for (int j = 0; j < _regionGraph.Count; j++)
            {
                List<Pos> intersection = region.Intersect(_regionGraph[j].Vertices).ToList();
                if (region.SequenceEqual(intersection)) connectionMatrix[i, j] = 0;
                if (intersection.Count == 0)connectionMatrix[i, j] = -1;
                else connectionMatrix[i, j] = intersection.Count % 2 == 0 ? 10 : 14;
            }
        }

        return connectionMatrix;
    }

    public void Dijkstra(int start)
    {
        bool[] visited = new bool[_regionGraph.Count];
        int[] distance = new int[_regionGraph.Count];
        int[] parent = new int[_regionGraph.Count];
        Array.Fill(distance, Int32.MaxValue);

        distance[start] = 0;
        parent[start] = start;

        while (true)
        {
            int closest = Int32.MaxValue;
            int now = -1;
            for (int i = 0; i < _regionGraph.Count; i++)
            {
                if (visited[i]) continue;
                if (distance[i] == Int32.MaxValue || distance[i] >= closest) continue;
                
                closest = distance[i];
                now = i;
            }

            if (now == -1) break;
            visited[now] = true;

            for (int next = 0; next < _regionGraph.Count; next++)
            {
                if (_connectionMatrix[now, next] == -1) continue;
                if (visited[next]) continue;
                
                int nextDist = distance[now] + _connectionMatrix[now, next];
                if (nextDist < distance[next])
                {
                    distance[next] = nextDist;
                    parent[next] = now;
                }
            }
        }
    }
    
    Pos Cell2Pos(Vector3 cell)
    {
        // CellPos -> ArrayPos
        return new Pos((int)(MaxZ - cell.z), (int)(cell.x - MinX));
    }

    Vector3 Pos2Cell(Pos pos)
    {
        // ArrayPos -> CellPos
        return new Vector3(pos.X+ MinX, 0,  MaxZ - pos.Z);
    }
}