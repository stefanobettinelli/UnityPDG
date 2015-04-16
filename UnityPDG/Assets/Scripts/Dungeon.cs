﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Dungeon : MonoBehaviour {

    public DungeonRoom dungeonRoomPrefab;

	private int _minRoomWidth;
	private int _maxRoomWidth;
	private int _minRoomHeight;
	private int _maxRoomHeight;
    private string dungeonName;
    private struct TileMatrix
    {
        private int _w;
        private int _h;
        private int[,] m;

        public TileMatrix(int w, int h)
        {
            _w = w;
            _h = h;
            m = new int[_h, _w];
        }

        public void addTile(int x, int z)
        {
            Debug.Log("Adding to x:" + x + ",z:" + z);
            if (x >= _w)
            {
                int[,] tmpMatrix = new int[_h, x + 1];
                //System.Array.Copy(m, tmpMatrix, _w * _h);
                for (int j = 0; j < _w; j++)
                {
                    for (int i = 0; i < _h; i++)
                    {
                        tmpMatrix[i, j] = m[i, j];
                    }                    
                }
                m = tmpMatrix;
                _w = x + 1;
                Debug.Log("allargato");
            }
            if (z >= _h)
            {
                int[,] tmpMatrix = new int[z + 1, _w]; 
                //System.Array.Copy(m, tmpMatrix, _w * _h);
                for (int j = 0; j < _w; j++)
                {
                    for (int i = 0; i < _h; i++)
                    {
                        tmpMatrix[i, j] = m[i, j];
                    }
                }
                m = tmpMatrix;
                _h = z + 1;
                Debug.Log("alzato");
            }
            Debug.Log("***Actal matrix***");
            Debug.Log(m);
            m[z, x] = 1; //imposto la mattonella come esistente
        }

        public override string ToString()
        {
            Debug.Log("tileMatrix wxh: " + _w + " x " + _h );
            string str = "TileMatrix width: " + _w + ",height: " + _h + "\n";
            for (int j = 0; j <_w; j++) 
            {
                for (int i = 0; i < _h; i++)
                {
                    str += m[i, j] + ",";
                }
                str += "\n";
            }
            return str;
        }
    };
    private TileMatrix tileMatrix = new TileMatrix(2,2);

    public int MinRoomWidth { get { return _minRoomWidth; } set { _minRoomWidth = value; } }
    public int MaxRoomWidth { get { return _maxRoomWidth; } set { _maxRoomWidth = value; } }
    public int MinRoomHeight { get { return _minRoomHeight; } set { _minRoomHeight = value; } }
    public int MaxRoomHeight { get { return _maxRoomHeight; } set { _maxRoomHeight = value; } }
    public string DungeonName{ get { return dungeonName; } set { dungeonName = value; } }

    //Genera l'intero dungeon
    public void Generate(int minWidth, int maxWidth, int minHeight, int maxHeight, int roomNum, Dungeon dungeonContainer)
    {
        int bbWidth=0;
        int bbHeight=0;

        //inizio creazione della stanza nel punto di origine 0,0,0
        Debug.Log("*** Creating room ORIGIN ***");
        DungeonRoom first = Instantiate(dungeonRoomPrefab) as DungeonRoom;
        first.transform.parent = dungeonContainer.transform;
        first.transform.localPosition = Vector3.zero;
        first.generateRoomSize(minWidth, maxWidth, minHeight, maxHeight);
        first.Data.Name = "Room: 0"; first.name = first.Data.Name;      
        foreach (DungeonCell cell in first.AllocateRoomInSpace())
        {
            Debug.Log("adding coordinate x: " + (first.Data.Origin.x + cell.Coordinates.x) + ", z: " + (first.Data.Origin.z + cell.Coordinates.z));
            tileMatrix.addTile(first.Data.Origin.x + cell.Coordinates.x, first.Data.Origin.z + cell.Coordinates.z);
        } 
        bbWidth = first.Data.Width;//larghezza del bounding box
        bbHeight = first.Data.Height;//altezza del bounding box
        Debug.Log("BB-width: " + bbWidth + " BB-height: " + bbHeight);
        Debug.Log("************************");
        //fine creazione prima stanza

        //creo le stanze successive alla prima
        for (int i = 1; i < roomNum; i++)
		{
            DungeonRoom aRoom = Instantiate(dungeonRoomPrefab) as DungeonRoom;
            aRoom.transform.parent = dungeonContainer.transform;
            aRoom.generateRoomSize(minWidth, maxWidth, minHeight, maxHeight);
            aRoom.Data.Name = "Room: " + i; aRoom.name = aRoom.Data.Name;
            int direction = Random.Range(0,2);//scelgo se creare la stanza a destra o sopra il bounding box attaule che racchiude il dungeon
            switch (direction)
	        {
                case 0:
                    Debug.Log("*** Creating room RIGHT ***");
                    //se si crea a destra l'origne può partire da metà della larghezza del BB fino a 10 unità oltre al BB
                    aRoom.Data.Origin = new IntVector2(Random.Range((int)Mathf.Ceil(bbWidth/2.0f), (int)Mathf.Ceil(bbWidth/2.0f)*2+5), Random.Range(0,bbHeight));
                    Debug.Log("origin x: " + aRoom.Data.Origin.x + " y: " + aRoom.Data.Origin.z);
                    aRoom.transform.localPosition = new Vector3(aRoom.Data.Origin.x, 0, aRoom.Data.Origin.z);
                    foreach (DungeonCell cell in aRoom.AllocateRoomInSpace())
	                {
                        Debug.Log("adding coordinate x: " + (aRoom.Data.Origin.x + cell.Coordinates.x) + ", z: " + (aRoom.Data.Origin.z + cell.Coordinates.z));
                        tileMatrix.addTile(aRoom.Data.Origin.x + cell.Coordinates.x,aRoom.Data.Origin.z + cell.Coordinates.z);
	                } 
                    //aggiorno le dimensioni del bounding box in modo che tengano conto della nuova stanza appena creata
                    bbWidth = aRoom.Data.Origin.x + aRoom.Data.Width;
                    bbHeight = Mathf.Max(bbHeight,aRoom.Data.Height + aRoom.Data.Origin.z);
                    Debug.Log("BB-width: " + bbWidth + " BB-height: " + bbHeight);
                    Debug.Log("************************");
                    break;
                case 1:
                    Debug.Log("*** Creating room UP ***");
                    //se si crea la stanza sopra il BB l'origne può partire da metà dell'altezza del BB fino a 10 unità oltre l'altezza del BB
                    aRoom.Data.Origin = new IntVector2(Random.Range(0, bbWidth), Random.Range((int)Mathf.Ceil(bbHeight / 2.0f), (int)Mathf.Ceil(bbHeight / 2.0f) * 2 + 5));
                    Debug.Log("origin x: " + aRoom.Data.Origin.x + " y: " + aRoom.Data.Origin.z);
                    aRoom.transform.localPosition = new Vector3(aRoom.Data.Origin.x, 0, aRoom.Data.Origin.z);
                    foreach (DungeonCell cell in aRoom.AllocateRoomInSpace())
                    {
                        Debug.Log("adding coordinate x: " + (aRoom.Data.Origin.x + cell.Coordinates.x) + ", z: " + (aRoom.Data.Origin.z + cell.Coordinates.z));
                        tileMatrix.addTile(aRoom.Data.Origin.x + cell.Coordinates.x, aRoom.Data.Origin.z + cell.Coordinates.z);
                    }
                    //aggiorno le dimensioni del bounding box in modo che tengano conto della nuova stanza appena creata
                    bbWidth = Mathf.Max(bbWidth, aRoom.Data.Width + aRoom.Data.Origin.x);
                    bbHeight = aRoom.Data.Origin.z + aRoom.Data.Height;
                    Debug.Log("BB-width: " + bbWidth + " BB-height: " + bbHeight);
                    Debug.Log("************************");
                    break;
	        }
		}
        Debug.Log(tileMatrix);
	}//fine generate



}
