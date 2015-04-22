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

        public void enlargeMatrix(int x, int z)
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
        }

        //restituisce il numero di sovrapposizioni
        public bool addTile(int x, int z)
        {
            enlargeMatrix(x, z);
            if (m[z, x] == 0)
            {
                m[z, x] = 1; 
                return false;//non c'è stata sovrapposizione
            }
            else 
            {
                //m[z, x] = 2;
                return true;//c'è stata sovrapposizione
            }                       
        }

        public bool checkOverLap(IntVector2 origin, int width, int height)
        {
            string str = "";
            enlargeMatrix(origin.x + width, origin.z + height);//così sono certo di non incappare in qualchè outofbound
            for (int j = origin.x; j < (origin.x + width); j++)
            {
                for (int i = origin.z; i < (origin.z + height); i++)                   
                {
                    str += m[i, j] + ",";
                    if (m[i, j] == 1) return true;                    
                }
                str += "\n";
            }
            return false;
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
        DungeonRoom[] roomArray = new DungeonRoom[roomNum];
        int upShift = 0;
        int rightShift = 0;
        for (int i = 0; i < roomNum; i++)//questo array serve per memorizzare i dati delle stanze
        {            
            roomArray[i] = new DungeonRoom(minWidth, maxWidth, minHeight, maxHeight);
            roomArray[i].Data.Origin = new IntVector2(0, 0);
            Debug.Log("x: " + roomArray[i].Data.Origin.x + ", z:" + roomArray[i].Data.Origin.z);
            roomArray[i].Data.Name = "Room: " + i;
            while(tileMatrix.checkOverLap(roomArray[i].Data.Origin, roomArray[i].Data.Width, roomArray[i].Data.Height))
            {
                int actualX = roomArray[i].Data.Origin.x;
                int actualZ = roomArray[i].Data.Origin.z;
                int dir = Random.Range(0, 2);
                if (dir == 0)
                {
                    Debug.Log("sposto a dx");
                    roomArray[i].Data.Origin = new IntVector2(actualX + 5, actualZ);
                    rightShift++;
                }
                else if( dir == 1 )
                {
                    Debug.Log("sposto in alto");
                    roomArray[i].Data.Origin = new IntVector2(actualX, actualZ + 5);
                    upShift++;
                }
            }
            updateTileMatrix(roomArray[i]);            
        }        

        //comincia la creazione delle stanze nello spazio 3D
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

        Debug.Log("right: " + rightShift);
        Debug.Log("up: " + upShift);
  
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
