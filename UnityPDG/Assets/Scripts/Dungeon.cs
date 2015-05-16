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

    public Dictionary<IntVector2, DungeonCell> activeDungeonCells = new Dictionary<IntVector2, DungeonCell>();

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

    //struct di comodo usata per la rimozione dei muri di raccordo
    private struct coordinateAndCell
    {
        public IntVector2 pos;
        public DungeonCell cell;
        public int prevDirection;

        public coordinateAndCell(IntVector2 aCoordinate, DungeonCell aCell, int dir)
        {
            pos = aCoordinate;
            cell = aCell;
            prevDirection = dir;
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

        public int this[int i, int j] { get { return m[i, j]; } set { m[i, j] = value; } }


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
                    roomArray[i].moveRoom(minShitValue,0);
                }
                else if( dir == 1 )
                {
                    roomArray[i].moveRoom(0,minShitValue);
                }
            }
            updateTileMatrix(roomArray[i]);
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
            updateDungeonActiveCells(roomArray[i].Data.Origin, gameObjectRoomArray[i]);
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
                    Gizmos.color = Color.blue;
                    Vector3 c1 = new Vector3(roomArray[i].Data.Center.x, 3, roomArray[i].Data.Center.z);
                    Vector3 c2 = new Vector3(roomArray[j].Data.Center.x, 3, roomArray[j].Data.Center.z);
                    centerList.Add(new centerPair(c1, c2)); //serve per disegna il grafo rosso di overlay
                    createCorridor(gameObjectRoomArray[i], gameObjectRoomArray[j]); // va passato il gameobject istanziato nello spazio e non il roomArray[i]
                    print("ROOM " + gameObjectRoomArray[i] + " --- connected to ROOM --- " + gameObjectRoomArray[j]);
                    //createCorridor(gameObjectRoomArray[j], gameObjectRoomArray[i]);                    
                }
            }
        }//fine algoritmo di connessione        

        //foreach (KeyValuePair<IntVector2, DungeonCell> entry in activeDungeonCells)
        //{
        //    print("key: " + entry.Key + ", value: " + entry.Value);
        //}

        //print(tileMatrix);

	}//fine generate

    private void updateDungeonActiveCells(IntVector2 origin, DungeonRoom aRoom)
    {
        foreach (DungeonCell aCell in aRoom.activeCells)
        {            
            try
            {
                aCell.Coordinates = new IntVector2(aCell.Coordinates.x + origin.x, aCell.Coordinates.z + origin.z);
                activeDungeonCells.Add(new IntVector2(aCell.Coordinates.x,aCell.Coordinates.z),aCell);
            }
            catch (System.ArgumentException)
            {
                print("An element with Key = \"txt\" already exists.");
            }
        }
    }

    //crea il corridoio di connessione fatto da unità base di "tile" che connette la stanza A e B
    private void createCorridor(DungeonRoom roomA, DungeonRoom roomB)
    {
        //sukaaaaaaaaaa
        int dir = Random.Range(0,2);
        int dx = roomA.Data.Center.x - roomB.Data.Center.x;
        int dz = roomA.Data.Center.z - roomB.Data.Center.z;
        string nextDirX;
        string nextDirZ;

        if (dz < 0) nextDirZ = "north";
        else nextDirZ = "south";
        if (dx < 0) nextDirX = "east";
        else nextDirX = "west";

        switch (dir)
        {
            case 0:
                {
                    //passo roomA per poi usare le sue celle controllare se ci sono eventuali muri da rimuovere
                    coordinateAndCell lastCC = createHorizontalCorridor(roomA.Data.Center, dx, roomA, roomB, null,0,nextDirZ);//crea il pezzo di corridoio orizzontale partendo dalla stanza A
                    createVerticalCorridor(lastCC.pos, dz, roomA, roomB, lastCC.cell, lastCC.prevDirection,"");////crea il pezzo di corridoio verticale partendo dalla stanza A
                    //Debug.Log("----- Stanza " + roomA.Data.Name + " collegata con stanza " + roomB.Data.Name);
                    break;
                }
            case 1:
                {
                    coordinateAndCell lastCC = createVerticalCorridor(roomA.Data.Center, dz, roomA, roomB, null,0, nextDirX);////crea il pezzo di corridoio verticale partendo dalla stanza A
                    createHorizontalCorridor(lastCC.pos, dx, roomA, roomB, lastCC.cell, lastCC.prevDirection,"");//crea il pezzo di corridoio orizzontale partendo dalla stanza A
                    //Debug.Log("----- Stanza " + roomA.Data.Name + " collegata con stanza " + roomB.Data.Name);
                    break;
                }
            default:
                break;
        }
    }

    private coordinateAndCell createHorizontalCorridor(IntVector2 startPos, int lenght, DungeonRoom aRoom, DungeonRoom bRoom, DungeonCell lastCell, int preVDirection, string nextDirZ)
    {
        coordinateAndCell ret;
        DungeonCell aCell = null;
        int direction = 0;
        for (int i = 0; i < Mathf.Abs(lenght); i++)
        {
            if (lenght < 0)
            {
                print("DIO CANE " + startPos);
                //destroyWall(startPos, "east");
                startPos = new IntVector2(startPos.x + 1, startPos.z);                
                aCell = createCorridorTile(startPos, 0, "east");
                //destroyWall(startPos, "east");
                //destoyWall(aRoom, bRoom,"east wall", "west wall" , "Dungeon Cell " + startPos.x + ", " + startPos.z, startPos);                
                direction = 1;
            }
            else if (lenght > 0)
            {
                print("DIO CANE " + startPos);
                //destroyWall(startPos, "west");
                startPos = new IntVector2(startPos.x - 1, startPos.z);
                aCell = createCorridorTile(startPos, 0, "west");
                //destroyWall(startPos, "west");
                //destoyWall(aRoom, bRoom,"west wall", "east wall" ,"Dungeon Cell " + startPos.x + ", " + startPos.z, startPos);
                direction = -1;
            }
        }

        if (lenght < 0 && lastCell != null && tileMatrix[lastCell.Coordinates.z, lastCell.Coordinates.x] == 0 )
        {
            //lastCell.destroyWall("east");
            if (preVDirection == -1 && tileMatrix[lastCell.Coordinates.z-1, lastCell.Coordinates.x] == 0)
            {
                createSingleWall(lastCell, "south", lastCell.transform);
            }
            else if (preVDirection == 1 && tileMatrix[lastCell.Coordinates.z + 1, lastCell.Coordinates.x] == 0)
            {
                createSingleWall(lastCell, "north", lastCell.transform);
            }
        }
        if (lenght > 0 && lastCell != null && tileMatrix[lastCell.Coordinates.z, lastCell.Coordinates.x] == 0)
        {
            //lastCell.destroyWall("west");
            if (preVDirection == -1 && tileMatrix[lastCell.Coordinates.z - 1, lastCell.Coordinates.x] == 0)
            {
                createSingleWall(lastCell, "south", lastCell.transform);
            }
            else if (preVDirection == 1 && tileMatrix[lastCell.Coordinates.z + 1, lastCell.Coordinates.x] == 0)
            {
                createSingleWall(lastCell, "north", lastCell.transform);
            }
        }
        ret = new coordinateAndCell(startPos, aCell, direction);
        return ret;
    }

    private coordinateAndCell createVerticalCorridor(IntVector2 startPos, int lenght, DungeonRoom aRoom, DungeonRoom bRoom, DungeonCell lastCell, int preVDirection, string nextDirX)
    {
        coordinateAndCell ret;
        DungeonCell aCell = null;
        int direction = 0;        
        for (int i = 0; i < Mathf.Abs(lenght); i++)
        {
            if (lenght < 0)
            {
                print("DIO CANE " + startPos);
                //destroyWall(startPos, "north");                
                startPos = new IntVector2(startPos.x, startPos.z + 1);                
                aCell = createCorridorTile(startPos, 1, "north");
                //ripartire da qui domani 17/06, questo if mi consente di evitare che si formino buchi all'interno delle stanze
                //mentre si creano i corridoi, infatti se viene raggiunta la fine di un pezzo di segmento non si cerca di distruggere alcun muro se si è all'interno di una stanza
                // forse serve un controllo sul tile matrix == 1? ragionare su cosa succede se il corridoio viene creato fuori dalla stanza
                //if (i < (Mathf.Abs(lenght) - 1) )
                    //destroyWall(startPos, "north");  
                //destoyWall(aRoom, bRoom,"north wall", "south wall" ,"Dungeon Cell " + startPos.x + ", " + startPos.z, startPos);                                
                direction = 1;
            }
            else if (lenght > 0)
            {
                print("DIO CANE " + startPos);
                //destroyWall(startPos, "south");        
                startPos = new IntVector2(startPos.x, startPos.z - 1);
                aCell = createCorridorTile(startPos, 1, "south");
                //destroyWall(startPos, "south");        
                //destoyWall(aRoom, bRoom,"south wall", "north wall" ,"Dungeon Cell " + startPos.x + ", " + startPos.z, startPos);
                direction = -1;
            }
        }
        
        if (lenght < 0 && lastCell != null)
        {
            //lastCell.destroyWall("north");
            if (preVDirection == -1 && tileMatrix[lastCell.Coordinates.z, lastCell.Coordinates.x-1] == 0)
            {
                createSingleWall(lastCell, "west", lastCell.transform);
            }
            else if (preVDirection == 1 && tileMatrix[lastCell.Coordinates.z, lastCell.Coordinates.x+1] == 0)
            {
                createSingleWall(lastCell, "east", lastCell.transform);
            }
        }
        if (lenght > 0 && lastCell != null)
        {
            //lastCell.destroyWall("south");
            if (preVDirection == -1 && tileMatrix[lastCell.Coordinates.z, lastCell.Coordinates.x - 1] == 0)
            {
                createSingleWall(lastCell, "west", lastCell.transform);
            }
            else if (preVDirection == 1 && tileMatrix[lastCell.Coordinates.z, lastCell.Coordinates.x + 1] == 0)
            {
                createSingleWall(lastCell, "east", lastCell.transform);
            }
        }
        ret = new coordinateAndCell(startPos, aCell, direction);
        return ret;
    }

    public void destroyWall(IntVector2 coordinates, string direction)
    {
        DungeonCell tmp;
        if (direction == "north" || direction == "south")
        {
            //print("distruggo nord e sud su " + coordinates);
            if (activeDungeonCells.TryGetValue(coordinates, out tmp))
            {
                tmp.destroyWall("north");
                tmp.destroyWall("south");
            }
        }
        else if (direction == "east" || direction == "west")
        {
            if (activeDungeonCells.TryGetValue(coordinates, out tmp))
            {
                tmp.destroyWall("east");
                tmp.destroyWall("west");
            }
        }
    }

    public void destroySimpleWall(IntVector2 coordinates, string direction)
    {
        DungeonCell tmp;
        if (activeDungeonCells.TryGetValue(coordinates, out tmp))
        {
            tmp.destroyWall(direction);
        }
       
    }

    public void destoyWall(DungeonRoom aRoom, DungeonRoom bRoom, string wallName, string secondWallToDestroy,string cellName, IntVector2 pos)
    {
        if (pos.x >= aRoom.Data.Origin.x && pos.z >= aRoom.Data.Origin.z && pos.x <= (aRoom.Data.Width - 1 + aRoom.Data.Origin.x) && pos.z <= (aRoom.Data.Height - 1 + aRoom.Data.Origin.z))
        {
            foreach (Transform child in aRoom.activeCells[((pos.x - aRoom.Data.Origin.x) * aRoom.Data.Height) + (pos.z - aRoom.Data.Origin.z)].transform)
            {
                if (child.name == wallName)
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }
        else if (pos.x >= bRoom.Data.Origin.x && pos.z >= bRoom.Data.Origin.z && pos.x <= (bRoom.Data.Width - 1 + bRoom.Data.Origin.x) && pos.z <= (bRoom.Data.Height - 1 + bRoom.Data.Origin.z))
        {
            //la prima cella su cui approdo nella stanza b dovrebbe avre il muro di passaggio da eliminare
            foreach (Transform child in bRoom.activeCells[((pos.x - bRoom.Data.Origin.x) * bRoom.Data.Height) + (pos.z - bRoom.Data.Origin.z)].transform)
            {
                if (child.name == secondWallToDestroy)
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }
    }

    private DungeonCell createCorridorTile(IntVector2 c, int dir, string sDir)
    {
        DungeonCell aCell = null;
        if (tileMatrix[c.z, c.x] == 0)
        {//se non c'è sovrapposizione crea la mattonella del corridoio            
            tileMatrix[c.z, c.x] = 2;                 
            aCell = CreateCorridorCell(c);
            aCell.transform.parent = transform;
            try
            {
                activeDungeonCells.Add(c, aCell);
            }
            catch (System.ArgumentException)
            {
                print("An element with Key = \"txt\" already exists.");
            }
            CreateCorridorWalls(aCell, dir, aCell.transform);            
            return aCell;
        }
        else if (tileMatrix[c.z, c.x] == 2)
        {
            //destroySimpleWall(c, sDir);
            /*if( sDir == "north" || sDir == "south" ){
                destroyWall(c,"south");
                destroyWall(c,"north");
            }
            if (sDir == "west" || sDir == "east")
            {
                destroyWall(c, "west");
                destroyWall(c, "east");
            } */                   
        }
        return null;
    }

    private void CreateCorridorWalls(DungeonCell cell, int dir, Transform parent)
    {        
        if( dir == 0 ){
            WallUnit aWall = InstanciateWall(Directions.directionVectors[(int)Direction.North], Direction.North, cell, parent);
            cell.addWallRefenceToCell(aWall,"north");
            aWall = InstanciateWall(Directions.directionVectors[(int)Direction.South], Direction.South, cell, parent);            
            cell.addWallRefenceToCell(aWall,"south");
        }          
        if ( dir == 1)
        {
            WallUnit aWall = InstanciateWall(Directions.directionVectors[(int)Direction.West], Direction.West, cell, parent);
            cell.addWallRefenceToCell(aWall,"west");
            aWall = InstanciateWall(Directions.directionVectors[(int)Direction.East], Direction.East, cell, parent);            
            cell.addWallRefenceToCell(aWall,"east");
        }
    }

    private void createSingleWall(DungeonCell cell, string type, Transform parent)
    {
        if (type == "north")
        {
            WallUnit aWall = InstanciateWall(Directions.directionVectors[(int)Direction.North], Direction.North, cell, parent);
            cell.addWallRefenceToCell(aWall, "north");            
        }
        if (type == "south")
        {
            WallUnit aWall = InstanciateWall(Directions.directionVectors[(int)Direction.South], Direction.South, cell, parent);
            cell.addWallRefenceToCell(aWall, "south");
        }
        if (type == "east")
        {
            WallUnit aWall = InstanciateWall(Directions.directionVectors[(int)Direction.East], Direction.East, cell, parent);
            cell.addWallRefenceToCell(aWall, "east");
        }
        if (type == "west")
        {
            WallUnit aWall = InstanciateWall(Directions.directionVectors[(int)Direction.West], Direction.West, cell, parent);
            cell.addWallRefenceToCell(aWall, "west");
        }
    }

    private WallUnit InstanciateWall(IntVector2 wallDirection, Direction direction, DungeonCell cell, Transform parent)
    {
        WallUnit aWall = Instantiate(wallPrefab) as WallUnit;
        aWall.transform.parent = cell.transform;
        aWall.transform.localPosition = new Vector3(wallDirection.x, 0.5f, wallDirection.z);
        aWall.transform.localRotation = direction.ToRotation();
        aWall.transform.parent = parent;
        return aWall;

    }

    //crea una mattonella del pavimento nelle coordinate "coordinates"
    private DungeonCell CreateCorridorCell(IntVector2 coordinates)
    {
        DungeonCell newDungeonCell = Instantiate(dungeonCellPrefab) as DungeonCell;
        //cells[coordinates.x,coordinates.z] = newDungeonCell;
        newDungeonCell.name = "Dungeon Corrido Cell " + coordinates.x + ", " + coordinates.z;
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
