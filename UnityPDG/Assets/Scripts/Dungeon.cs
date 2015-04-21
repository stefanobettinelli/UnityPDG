using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Dungeon : MonoBehaviour {

    public DungeonRoom dungeonRoomPrefab;

	private int _minRoomWidth;
	private int _maxRoomWidth;
	private int _minRoomHeight;
	private int _maxRoomHeight;
    private string dungeonName;

    /*
     * Struttura dati specifica per memorizzare le singole mattonelle attivi nello spazio,
     * ho dovuto implementarla a mano in quanto in C# pare che non esistano strutture dati built in
     * che consentono di avere matrici che aumentano di dimensione in modo dinamico
     */
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

        public int this[int i, int j] { get { return m[i, j]; } }

        //restituisce il numero di sovrapposizioni
        public bool addTile(int x, int z)
        {
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
            }
            if (m[z, x] == 0)
            {
                m[z, x] = 1; 
                return false;//non c'è stata sovrapposizione
            }
            else 
            {
                m[z, x] = 2;
                return true;//c'è stata sovrapposizione
            }                       
        }

        public override string ToString()
        {
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
        int xMin = 0;
        int xMax = 0;
        int zMin = 0;
        int zMax = 0;
        DungeonRoom[] roomArray = new DungeonRoom[roomNum];
        for (int i = 0; i < roomNum; i++)//quest array serve per memorizzare i dati delle stanze
        {
            int x = 0;
            int z = 0;
            roomArray[i] = new DungeonRoom(minWidth, maxWidth, minHeight, maxHeight);
            if (i == 0)
            {
                roomArray[i].Data.Origin = new IntVector2(0, 0);
                roomArray[i].Data.Name = "Starting-Room: " + i;
                xMin = zMin = 0;
                xMax = roomArray[i].Data.Width;
                zMax = roomArray[i].Data.Height;
                //updateTileMatrix(roomArray[i]);
            }
            else
            {
                int direction = Random.Range(0,4);//scelgo se creare la stanza a destra o sopra il bounding box attaule che racchiude il dungeon
                if (direction == 0)
                {
                    x = Random.Range(xMax, xMax);
                    z = Random.Range(zMin, zMax - roomArray[i].Data.Height);
                    roomArray[i].Data.Origin = new IntVector2(x, z);
                    roomArray[i].Data.Name = "Room-right: " + i; 
                    xMax = (int) (1f * (roomArray[i].Data.Origin.x + roomArray[i].Data.Width));
                    zMax = (int) (1f * Mathf.Max(zMax, roomArray[i].Data.Height + roomArray[i].Data.Origin.z));
                    //updateTileMatrix(roomArray[i]);
                    //translateRoom("",roomArray[i]);
                }
                else if(direction == 1)
                {
                    x = Random.Range(xMin,xMax - roomArray[i].Data.Width);
                    z = Random.Range(zMax, zMax);
                    roomArray[i].Data.Origin = new IntVector2(x, z);
                    roomArray[i].Data.Name = "Room-up: " + i; 
                    xMax = (int) (1f * Mathf.Max(xMax, roomArray[i].Data.Width + roomArray[i].Data.Origin.x));
                    zMax = (int) (1f * (roomArray[i].Data.Origin.z + roomArray[i].Data.Height));
                    //updateTileMatrix(roomArray[i]);
                    //translateRoom("", roomArray[i]);
                }
                else if (direction == 2)
                {
                    x = Random.Range(xMin - 2*roomArray[i].Data.Width, xMin - roomArray[i].Data.Width);
                    z = Random.Range(zMin, zMax - roomArray[i].Data.Height);
                    roomArray[i].Data.Origin = new IntVector2(x, z);
                    roomArray[i].Data.Name = "Room-left: " + i;
                    xMin = (int)(1f * (roomArray[i].Data.Origin.x));
                    zMax = (int)(1f * Mathf.Max(zMax, roomArray[i].Data.Height + roomArray[i].Data.Origin.z));
                }
                else if (direction == 3)
                {
                    x = Random.Range(xMin, xMax - roomArray[i].Data.Width);
                    z = Random.Range(zMin - 2 * roomArray[i].Data.Height, zMin - roomArray[i].Data.Height);
                    roomArray[i].Data.Origin = new IntVector2(x, z);
                    roomArray[i].Data.Name = "Room-down: " + i;
                    xMax = (int)(1f * Mathf.Max(xMax, roomArray[i].Data.Width + roomArray[i].Data.Origin.x));
                    zMin = (int)(1f * (roomArray[i].Data.Origin.z));
                }                                     
            }
        }

        for (int i = 0; i < roomNum; i++)
        {
            for (int j = i+1; j < roomNum; j++)
            {
                if(roomArray[i].overLap(roomArray[j])){//se c'è sovrapposizione vanno staccati

                }
            }
        }

        DungeonRoom[] gameObjectRoomArray = new DungeonRoom[roomNum];

        for (int i = 0; i < roomNum; i++)
        {
            gameObjectRoomArray[i] = Instantiate(dungeonRoomPrefab) as DungeonRoom;
            gameObjectRoomArray[i].transform.parent = dungeonContainer.transform;
            gameObjectRoomArray[i].generateRoom(roomArray[i]);
            gameObjectRoomArray[i].Data.Name = roomArray[i].Data.Name;
            gameObjectRoomArray[i].name = gameObjectRoomArray[i].Data.Name;
            gameObjectRoomArray[i].transform.localPosition = new Vector3(roomArray[i].Data.Origin.x, 0, roomArray[i].Data.Origin.z);
            gameObjectRoomArray[i].AllocateRoomInSpace();            
        }

        /*
        //inizio creazione della stanza nel punto di origine 0,0,0
        //Debug.Log("*** Creating room ORIGIN ***");
        DungeonRoom first = Instantiate(dungeonRoomPrefab) as DungeonRoom;
        first.transform.parent = dungeonContainer.transform;
        first.transform.localPosition = Vector3.zero;
        first.generateRoomSize(minWidth, maxWidth, minHeight, maxHeight);
        first.Data.Name = "Room: 0"; first.name = first.Data.Name;
        foreach (DungeonCell cell in first.AllocateRoomInSpace())
        {
            tileMatrix.addTile(first.Data.Origin.x + cell.Coordinates.x, first.Data.Origin.z + cell.Coordinates.z);
        }
        bbWidth = first.Data.Width;//larghezza del bounding box
        bbHeight = first.Data.Height;//altezza del bounding box
        //Debug.Log("BB-width: " + bbWidth + " BB-height: " + bbHeight);
        //Debug.Log("************************");
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
                    {
                        //Debug.Log("*** Creating room RIGHT ***");
                        //se si crea a destra l'origne può partire da metà della larghezza del BB fino a 10 unità oltre al BB
                        aRoom.Data.Origin = new IntVector2(Random.Range((int)Mathf.Ceil(bbWidth / 2.0f), (int)Mathf.Ceil(bbWidth / 2.0f) * 2 + 5), Random.Range(0, bbHeight));
                        //aRoom.Data.Origin = new IntVector2(Random.Range(bbWidth, bbWidth + 2), Random.Range(0, bbHeight));
                        //Debug.Log("origin x: " + aRoom.Data.Origin.x + " y: " + aRoom.Data.Origin.z);
                        //aRoom.transform.localPosition = new Vector3(aRoom.Data.Origin.x, 0, aRoom.Data.Origin.z);                                              
                        aRoom.transform.localPosition = new Vector3(aRoom.Data.Origin.x, 0, aRoom.Data.Origin.z);
                        foreach (DungeonCell cell in aRoom.AllocateRoomInSpace())
                        {
                            tileMatrix.addTile(aRoom.Data.Origin.x + cell.Coordinates.x, aRoom.Data.Origin.z + cell.Coordinates.z);
                        }
                        translateRoom("right", aRoom);
                        //aggiorno le dimensioni del bounding box in modo che tengano conto della nuova stanza appena creata
                        bbWidth = aRoom.Data.Origin.x + aRoom.Data.Width;
                        bbHeight = Mathf.Max(bbHeight, aRoom.Data.Height + aRoom.Data.Origin.z);
                        //Debug.Log("BB-width: " + bbWidth + " BB-height: " + bbHeight);
                        //Debug.Log("************************");
                        break;
                    }
                case 1:
                    {
                        //Debug.Log("*** Creating room UP ***");
                        //se si crea la stanza sopra il BB l'origne può partire da metà dell'altezza del BB fino a 10 unità oltre l'altezza del BB
                        aRoom.Data.Origin = new IntVector2(Random.Range(0, bbWidth), Random.Range((int)Mathf.Ceil(bbHeight / 2.0f), (int)Mathf.Ceil(bbHeight / 2.0f) * 2 + 5));
                        //aRoom.Data.Origin = new IntVector2(Random.Range(0, bbWidth), Random.Range(bbHeight,bbHeight + 2));
                        //Debug.Log("origin x: " + aRoom.Data.Origin.x + " y: " + aRoom.Data.Origin.z);
                        aRoom.transform.localPosition = new Vector3(aRoom.Data.Origin.x, 0, aRoom.Data.Origin.z);                        
                        aRoom.transform.localPosition = new Vector3(aRoom.Data.Origin.x, 0, aRoom.Data.Origin.z);
                        foreach (DungeonCell cell in aRoom.AllocateRoomInSpace())
                        {
                            tileMatrix.addTile(aRoom.Data.Origin.x + cell.Coordinates.x, aRoom.Data.Origin.z + cell.Coordinates.z);
                        }
                        translateRoom("up", aRoom);
                        //aggiorno le dimensioni del bounding box in modo che tengano conto della nuova stanza appena creata
                        bbWidth = Mathf.Max(bbWidth, aRoom.Data.Width + aRoom.Data.Origin.x);
                        bbHeight = aRoom.Data.Origin.z + aRoom.Data.Height;
                        //Debug.Log("BB-width: " + bbWidth + " BB-height: " + bbHeight);
                        //Debug.Log("************************");
                        break;
                    }
	        }
		}*/
        //Debug.Log(tileMatrix);
	}//fine generate

    private void translateRoom(string direction, DungeonRoom aRoom)
    {
        int startX = aRoom.Data.Origin.x;
        int endX = aRoom.Data.Origin.x + aRoom.Data.Width;
        int startZ = aRoom.Data.Origin.z;
        int endZ = aRoom.Data.Origin.z + aRoom.Data.Height;
        int overLapX = 0;
        int overLapZ = 0;
        int startOverLapX = 0; bool startedX = false;
        int startOverLapZ = 0; bool startedZ = false;
        bool overLapFound = false;
        for (int i = startZ; i < endZ ; i++)
        {                       
            for (int j = startX; j < endX; j++)
            {
                if (tileMatrix[i, j] > 1 && !overLapFound)
                {
                    overLapFound = true;
                    if (!startedX)
                    {
                        startedX = true;
                        startOverLapX = j;
                    }
                    overLapX++;
                }                
            }
            if (overLapFound)
            {
                overLapFound = false;
                if(!startedZ){
                    startedZ = true;
                    startOverLapZ = i;
                }
                overLapZ++;
            }
        }
        //if( overLapZ > 0 )
        //    overLapX /= overLapZ;
        if (startedX || startedZ)
        {
            Debug.Log("Overlap starts at coordinates x: " + startOverLapX + ", z: " + startOverLapZ);
            Debug.Log("Width OverX " + overLapX + " Height OverZ " + overLapZ);
        }        
    }

    private void updateTileMatrix(DungeonRoom aRoom)
    {
        for (int i = 0; i < aRoom.Data.Width; i++)        
        {
            for (int j = 0; j < aRoom.Data.Height; j++)
            {
                tileMatrix.addTile(aRoom.Data.Origin.x + i, aRoom.Data.Origin.z + j);
            }   
        }            
    }

}
