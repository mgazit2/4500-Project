﻿using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

// TODO: Add spawn locations
// If not host, then put them in a box
// If host, spawn them on the planet area

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager RM;

    void Awake()
    {
        if (RM) // checks for another RM and destroys it
        {
            // there can only be one
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        RM = this;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if(scene.buildIndex == 1) // if we're in the game scene
        {
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerManager"), Vector3.zero, Quaternion.identity);
            PhotonNetwork.Instantiate(Path.Combine("Terrain", "MapGenerator"), Vector3.zero, Quaternion.identity);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
