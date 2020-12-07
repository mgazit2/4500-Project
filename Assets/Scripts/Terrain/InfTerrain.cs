using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class InfTerrain : MonoBehaviour {

    public const float maxViewDist = 450;
    public detailInfo detailLevels;
    public Material mapMaterial;

    Transform viewer;
    static Vector2 viewerPosition;

    static MapGenerator mapGen;
    int chunkSize;
    int chunksVisible;

    Dictionary<Vector2, TerrainChunk> chunkDict = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> chunksUpdated = new List<TerrainChunk>();

    private void Start() {
        mapGen = FindObjectOfType<MapGenerator>();
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length > 0) {
            viewer = players[0].transform;
        } else {
            viewer = mapGen.transform;
        }
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunksVisible = Mathf.RoundToInt(maxViewDist / chunkSize);
    }

    private void Update() {
        // every frame, update which chunks are visible to the player
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length > 0) {
            viewer = players[0].transform;
        }
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunks();
    }

    void UpdateVisibleChunks() {
        // set all chunks from last update to not visible
        foreach (TerrainChunk t in chunksUpdated) {
            t.SetVisible(false);
        }
        // clear list for this frame
        chunksUpdated.Clear();

        // get x,y of current chunk on the chunk grid (steps of mapChunkSize-1)
        int currentChunkX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        // sweep a square around the player of chunks that should be currently loaded
        for (int chunkGridY = -chunksVisible; chunkGridY <= chunksVisible; chunkGridY++) {
            for (int chunkGridX = -chunksVisible; chunkGridX <= chunksVisible; chunkGridX++) {
                // chunk grid coords of selected chunk
                Vector2 viewedChunkVec = new Vector2(
                        currentChunkX + chunkGridX, 
                        currentChunkY + chunkGridY);

                if (chunkDict.ContainsKey(viewedChunkVec)) {
                    // the selected chunk has already been generated
                    chunkDict[viewedChunkVec].UpdateChunk();
                    // if the chunk became visible, add it to the list, so it can be set invisble 
                    // once it leaves the view distance bounds
                    if (chunkDict[viewedChunkVec].IsVisible()) {
                        chunksUpdated.Add(chunkDict[viewedChunkVec]);
                    }
                } else {
                    // the selected chunk is new and needs to be generated and added to the dictionary
                    chunkDict.Add(
                        viewedChunkVec, 
                        new TerrainChunk(viewedChunkVec, chunkSize, transform, mapMaterial));
                }
            }
        }
    }
    
    public class TerrainChunk {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;

        public TerrainChunk(Vector2 chunkGridVec, int size, Transform parent, Material mat) {
            // constructor
            position = chunkGridVec * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshRenderer.material = mat;
            meshFilter = meshObject.AddComponent<MeshFilter>();
            
            meshObject.transform.position = positionV3;
            meshObject.transform.parent = parent;

            SetVisible(false);

            mapGen.RequestMapData(OnMapDataReceived);
        }

        void OnMapDataReceived(MapData mapData) {
            // function called by thread when mapData has been retrieved for a chunk
            mapGen.RequestMeshData(mapData, 0, OnMeshDataReceived);
        }

        void OnMeshDataReceived(MeshData meshData) {
            // function called by thread when meshData has been generated for a chunk
            meshFilter.mesh = meshData.CreateMesh();
        }

        public void UpdateChunk() {
            // checks if the viewer's distance to the nearest edge of this chunk is within
            // view distance. If it is, set this chunk to visible
            float viewDistToEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = viewDistToEdge <= maxViewDist;
            SetVisible(visible);
        }

        public void SetVisible(bool visible) {
            // sets the visibility of the chunk
            meshObject.SetActive(visible);
        }

        public bool IsVisible() {
            // returns the visibility of the chunk
            return meshObject.activeSelf;
        }
    }

    class DetailMesh {
        public Mesh mesh;
        public bool requestedMesh;
        public bool hasMesh;
        int detailLevel;

        public DetailMesh(int detail) {
            detailLevel = detail;
        }

        public void RequestMesh(MapData mapData) {
            requestedMesh = true;
            mapGen.RequestMeshData(mapData, detailLevel, OnMeshDataReceived);
        }

        void OnMeshDataReceived(MeshData meshData) {
            mesh = meshData.CreateMesh();
            hasMesh = true;
        }
    }

    [System.Serializable]
    public struct detailInfo {
        public int detailLevel;
        public float visibleDistThreshold;
    }
}
