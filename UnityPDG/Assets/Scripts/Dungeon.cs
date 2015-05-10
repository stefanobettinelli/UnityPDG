using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;

public class Dungeon : MonoBehaviour {

    public DungeonRoom dungeonRoomPrefab;
    public DungeonCell dungeonCellPrefab;
    public WallUnit wallPrefab;

	private int _minRoomWidth;
	private int _maxRoomWidth;
	private int _minRoomHeight;
	private int _maxRoomHeight;
    private string dungeonName;

    private struct centerPair
    {
        public Vector3 _c1;
        public Vector3 _c2;
        public centerPair(Vector3 c1, Vector3 c2)
        {
            _c1 = c1;
            _c2 = c2;
        }
    };
    private List<centerPair> centerList = new List<centerPair>();

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


        //espande la tileMatrix
        public void enlargeMatrix(int x, int z)
        {
            if (x >= _w)
            {
                int[,] tmpMatrix = new int[_h, x + 1];
                //copio il vecchio contenuto nella matrice espansa
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
                //copio il vecchio contenuto nella matrice espansa
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

        //controlla se nella tilematrix si crea una sovrapposizione se si piazza la stanza in origin di dimensioni widthxheight
        public bool checkOverLap(IntVector2 origin, int width, int height)
        {
            string str = "";
            //espansione della tilematrix prima di tutto
            enlargeMatrix(origin.x + width, origin.z + height);//così sono certo di non incappare in qualchè outofbound
            for (int j = origin.x; j < (origin.x + width); j++)
            {
                for (int i = origin.z; i < (origin.z + height); i++)                   
                {
                    str += m[i, j] + ",";
                    if (m[i, j] == 1) return true; //esiste sovrapposizione                   
                }
                str += "\n";
            }
            return false; //non c'è sovrapposizione
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
    public void Generate(int minWidth, int maxWidth, int minHeight, int maxHeight, int roomNum, Dungeon dungeonContainer, int minShitValue)
    {
        DungeonRoom[] roomArray = new DungeonRoom[roomNum];
        //algoritmo di separazione
        for (int i = 0; i < roomNum; i++)//questo array serve per memorizzare i dati delle stanze
        {            
            roomArray[i] = new DungeonRoom(minWidth, maxWidth, minHeight, maxHeight);
            roomArray[i].Data.Origin = new IntVector2(0, 0);            
            roomArray[i].Data.Name = "Room: " + i;
            while(tileMatrix.checkOverLap(roomArray[i].Data.Origin, roomArray[i].Data.Width, roomArray[i].Data.Height))
            {
                int dir = Random.Range(0, 2);
                if (dir == 0)
                {
                    //Debug.Log("sposto a dx");
                    roomArray[i].moveRoom(minShitValue,0);
                }
                else if( dir == 1 )
                {
                    //Debug.Log("sposto in alto");
                    roomArray[i].moveRoom(0,minShitValue);
                }
            }
            updateTileMatrix(roomArray[i]);
            //Debug.Log(roomArray[i]);
        }//fine algoritmo di separazione

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

        //algoritmo di connessione O(n^3)
        int ijDist, ikDist, jkDist;
        bool skip = false;
        for (int i = 0; i < roomNum; i++)
        {
            for (int j = i + 1; j < roomNum; j++)
            {
                skip = false;
                ijDist = roomArray[i].distance(roomArray[j]);
                //Debug.Log("Distanza tra stanza :" + i + "e stanza:" + j + " = " + ijDist);
                //per ogni coppia di stanze (i,j) controllo che non esista un terzo nodo k t.c Dmax sia minore della d(i,j)
                for (int k = 0; k < roomNum; k++)
                {
                    if (k == i || k == j)
                        continue;
                    ikDist = roomArray[i].distance(roomArray[k]);
                    jkDist = roomArray[j].distance(roomArray[k]);
                    if (Mathf.Max(ikDist, jkDist) < ijDist)
                    {
                        skip = true;
                        break;
                    }
                }
                if (!skip)
                {//se la prima coppia scelta non è stata schippata vuol dire che il suo arco può essere aggiunto al grafo
                    //per ora lo disegno e basta
                    Gizmos.color = Color.blue;
                    //Debug.Log("linea tra " + roomArray[i].Data.Name + " e " + roomArray[j].Data.Name);
                    Vector3 c1 = new Vector3(roomArray[i].Data.Center.x, 3, roomArray[i].Data.Center.z);
                    Vector3 c2 = new Vector3(roomArray[j].Data.Center.x, 3, roomArray[j].Data.Center.z);
                    centerList.Add(new centerPair(c1, c2));
                    createCorridor(gameObjectRoomArray[i], gameObjectRoomArray[j]); // va passato il gameobject istanziato nello spazio e non il roomArray[i]
                }
            }
        }//fine algoritmo di connessione        
  
	}//fine generate

    //crea il corridoio di connessione fatto da unità base di "tile" che connette la stanza A e B
    private void createCorridor(DungeonRoom roomA, DungeonRoom roomB)
    {
        int dir = Random.Range(0,2);
        int dx = roomA.Data.Center.x - roomB.Data.Center.x;
        int dz = roomA.Data.Center.z - roomB.Data.Center.z;
        switch (dir)
        {
            case 0:
                {
                    //passo roomA per poi usare le sue celle controllare se ci sono eventuali muri da rimuovere
                    IntVector2 lastP = createHorizontalCorridor(roomA.Data.Center, dx, roomA);//crea il pezzo di corridoio orizzontale partendo dalla stanza A
                    createVerticalCorridor(lastP, dz, roomA);////crea il pezzo di corridoio verticale partendo dalla stanza A
                    //Debug.Log("----- Stanza " + roomA.Data.Name + " collegata con stanza " + roomB.Data.Name);
                    break;
                }
            case 1:
                {
                    IntVector2 lastP = createVerticalCorridor(roomA.Data.Center, dz, roomA);////crea il pezzo di corridoio verticale partendo dalla stanza A
                    createHorizontalCorridor(lastP, dx, roomA);//crea il pezzo di corridoio orizzontale partendo dalla stanza A
                    //Debug.Log("----- Stanza " + roomA.Data.Name + " collegata con stanza " + roomB.Data.Name);
                    break;
                }
            default:
                break;
        }
    }

    private IntVector2 createHorizontalCorridor(IntVector2 startPos, int lenght, DungeonRoom aRoom)
    {
        for (int i = 0; i < Mathf.Abs(lenght); i++)
        {
            if (lenght < 0)
            {
                //Debug.Log("tile corridoio in pos: (" + (startPos.x + 1) + ", " + (startPos.z) + ")");
                startPos = new IntVector2(startPos.x + 1, startPos.z);
                createCorridorTile(startPos,0,"east");
                foreach (DungeonCell aCell in aRoom.activeCells)
                {
                    Debug.Log(aCell);
                }
                //Debug.Log(startPos);
            }
            else if (lenght > 0)
            {
                //Debug.Log("tile corridoio in pos: (" + (startPos.x - 1) + ", " + (startPos.z) + ")");
                startPos = new IntVector2(startPos.x - 1, startPos.z);
                createCorridorTile(startPos,0,"west");
                foreach (DungeonCell aCell in aRoom.activeCells)
                {
                    Debug.Log(aCell);
                }
                //Debug.Log(startPos);
            }
        }
        return startPos;
    }

    private IntVector2 createVerticalCorridor(IntVector2 startPos, int lenght, DungeonRoom aRoom)
    {
        for (int i = 0; i < Mathf.Abs(lenght); i++)
        {
            if (lenght < 0)
            {
                //Debug.Log("tile corridoio in pos: (" + startPos.x + ", " + (startPos.z + 1) + ")");
                startPos = new IntVector2(startPos.x, startPos.z + 1);
                createCorridorTile(startPos,1,"north");
                foreach (DungeonCell aCell in aRoom.activeCells)
                {
                    Debug.Log(aCell);
                }
                //Debug.Log(startPos);
            }
            else if (lenght > 0)
            {
                //Debug.Log("tile corridoio in pos: (" + startPos.x + ", " + (startPos.z - 1) + ")");
                startPos = new IntVector2(startPos.x, startPos.z - 1);
                createCorridorTile(startPos,1,"south");
                foreach (DungeonCell aCell in aRoom.activeCells)
                {
                    Debug.Log(aCell);
                }
                //Debug.Log(startPos);
            }
        }
        return startPos;
    }

    private DungeonCell createCorridorTile(IntVector2 c, int dir, string sDir)
    {
        if (tileMatrix[c.z, c.x] == 0)
        {//se non c'è sovrapposizione crea la mattonella del corridoio
            //tileMatrix.addTile(c.z,c.x) == false
            DungeonCell aCell = CreateCorridorCell(c);
            CreateWall(aCell.Coordinates.x,aCell.Coordinates.z,1,1,aCell,dir);
        }
        return null;
    }

    private void CreateWall(int x, int z, int width, int height, DungeonCell cell, int dir)
    {        
        if( dir == 0 ){
            InstanciateWall(Directions.directionVectors[(int)Direction.North], Direction.North, cell);
            InstanciateWall(Directions.directionVectors[(int)Direction.South], Direction.South, cell);
        }
            
        if ( dir == 1)
        {
            InstanciateWall(Directions.directionVectors[(int)Direction.West], Direction.West, cell);
            InstanciateWall(Directions.directionVectors[(int)Direction.East], Direction.East, cell);
        }        
    }

    private void InstanciateWall(IntVector2 wallDirection, Direction direction, DungeonCell cell)
    {
        WallUnit aWall = Instantiate(wallPrefab) as WallUnit;
        aWall.transform.parent = cell.transform;
        aWall.transform.localPosition = new Vector3(wallDirection.x, 0.5f, wallDirection.z);
        aWall.transform.localRotation = direction.ToRotation();

    }

    //crea una mattonella del pavimento nelle coordinate "coordinates"
    private DungeonCell CreateCorridorCell(IntVector2 coordinates)
    {
        DungeonCell newDungeonCell = Instantiate(dungeonCellPrefab) as DungeonCell;
        //cells[coordinates.x,coordinates.z] = newDungeonCell;
        newDungeonCell.name = "Dungeon Cell " + coordinates.x + ", " + coordinates.z;
        newDungeonCell.Coordinates = coordinates;        
        newDungeonCell.transform.localPosition = new Vector3(coordinates.x + 0.5f, 0f, coordinates.z + 0.5f);
        return newDungeonCell;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (var c in centerList)
        {
            Gizmos.DrawLine(c._c1,c._c2);            
        }        
    }

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
            //Debug.Log("Overlap starts at coordinates x: " + startOverLapX + ", z: " + startOverLapZ);
            //Debug.Log("Width OverX " + overLapX + " Height OverZ " + overLapZ);
        }        
    }
    

    //aggiorna la matrice di 1 e 0 aggiungendo gli uni per la stanza aRoom
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
