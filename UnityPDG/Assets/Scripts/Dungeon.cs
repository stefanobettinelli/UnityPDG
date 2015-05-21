using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;

using System.Reflection;

[System.Serializable]
public class Dungeon : MonoBehaviour {

    public DungeonRoom dungeonRoomPrefab;
    public DungeonCell dungeonCellPrefab;
    public WallUnit wallPrefab;

	private int _minRoomWidth;
	private int _maxRoomWidth;
	private int _minRoomHeight;
	private int _maxRoomHeight;
    private string dungeonName;
    private bool showRNGonGizmos = true;

    private GameObject corridorsGO = null;

    public Dictionary<IntVector2, DungeonCell> activeDungeonCells = new Dictionary<IntVector2, DungeonCell>();    

    //struttura utilizzata per renderizzare le linee del grafo RNG nella funzione onDrawGizmos
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
    //lista di coppie di punti connessi secondo un grafo RNG utilizzati nella onDrawGizmos
    private List<centerPair> centerList = new List<centerPair>();    
    /*
     * Struttura dati specifica per memorizzare le singole mattonelle attive nello spazio,
     * se è 0 = vuota
     * se è 1 = mattonella di stanza
     * se è 2 = mettonella di corridoio
     * ho dovuto implementarla a mano in quanto in C# pare che non esistano strutture dati built in
     * che consentono di avere matrici che aumentano di dimensione in modo dinamico
     */
    private struct TileMatrix
    {
        public int _w;
        public int _h;
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
                return true;//c'è stata sovrapposizione
            }                       
        }

        //controlla se nella tilematrix si crea una sovrapposizione se si piazza la stanza nel punto origin di dimensioni width x height
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
    private TileMatrix tileMatrix = new TileMatrix(2,2);//inizialmente la tilematrix è 2x2

    //getter e setters
    public int MinRoomWidth { get { return _minRoomWidth; } set { _minRoomWidth = value; } }
    public int MaxRoomWidth { get { return _maxRoomWidth; } set { _maxRoomWidth = value; } }
    public int MinRoomHeight { get { return _minRoomHeight; } set { _minRoomHeight = value; } }
    public int MaxRoomHeight { get { return _maxRoomHeight; } set { _maxRoomHeight = value; } }
    public string DungeonName{ get { return dungeonName; } set { dungeonName = value; } }

    public void showCorridors(bool v)
    {       
        corridorsGO.SetActive(v);
    }

    //Genera l'intero dungeon, il paramentro dungeon container è un semplice empty game object di contenimento, minshiftValue è il valore di spostamento
    //utilizzato dall'algoritmo di piazzamento delle stanze
    public void Generate(int minWidth, int maxWidth, int minHeight, int maxHeight, int roomNum, Dungeon dungeonContainer, int minShitValue)
    {
        //questo array serve per memorizzare i dati delle stanze
        //le stanze non vengono fisicamente allocate ma servono solo per l'algoritmo di piazzamento
        DungeonRoom[] roomArray = new DungeonRoom[roomNum];
        corridorsGO = new GameObject("corridors");

        //algoritmo di separazione
        for (int i = 0; i < roomNum; i++)
        {            
            roomArray[i] = new DungeonRoom(minWidth, maxWidth, minHeight, maxHeight);
            roomArray[i].Data.Origin = new IntVector2(0, 0);//l'origine delle stanze è sempre dal punto 0,0      
            roomArray[i].Data.Name = "Room: " + i;
            while(tileMatrix.checkOverLap(roomArray[i].Data.Origin, roomArray[i].Data.Width, roomArray[i].Data.Height))
            {
                int dir = Random.Range(0, 2);
                if (dir == 0)//muovo in orizzontale
                {
                    roomArray[i].moveRoom(minShitValue,0);
                }
                else if( dir == 1 )//muovo in verticale
                {
                    roomArray[i].moveRoom(0,minShitValue);
                }
            }
            updateTileMatrix(roomArray[i]);
        }//fine algoritmo di separazione

        //comincia la creazione delle stanze nello spazio 3D
        DungeonRoom[] gameObjectRoomArray = new DungeonRoom[roomNum];//queste invece vengono effettivamente allocate
        for (int i = 0; i < roomNum; i++)
        {
            gameObjectRoomArray[i] = Instantiate(dungeonRoomPrefab) as DungeonRoom;
            gameObjectRoomArray[i].transform.parent = dungeonContainer.transform;
            gameObjectRoomArray[i].generateRoom(roomArray[i]);
            gameObjectRoomArray[i].Data.Name = roomArray[i].Data.Name;
            gameObjectRoomArray[i].name = gameObjectRoomArray[i].Data.Name;
            gameObjectRoomArray[i].transform.localPosition = new Vector3(roomArray[i].Data.Origin.x, 0, roomArray[i].Data.Origin.z);
            gameObjectRoomArray[i].AllocateRoomInSpace();
            updateDungeonActiveCells(roomArray[i].Data.Origin, gameObjectRoomArray[i]);//utilizzo un dizionario per mantenermi le mattonelle attive all'interno del dungeon corridoi + stanze
        }

        //algoritmo di connessione O(n^3), crea prima il gravo RNG e poi connette le stanze con percorsi rettangolari
        int ijDist, ikDist, jkDist;
        bool skip = false;        
        for (int i = 0; i < roomNum; i++)
        {
            for (int j = i + 1; j < roomNum; j++)
            {
                skip = false;
                ijDist = roomArray[i].distance(roomArray[j]);
                //per ogni coppia di stanze (i,j) controllo che non esista un terzo nodo k t.c Mathf.Max(ikDist, jkDist) < ijDist
                //se esiste break dal questo for e quindi significa che non esiste l'arco i,j
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
                {//se la prima coppia scelta i,j non è stata scartata vuol dire che il suo arco può essere aggiunto al grafo                    
                    Gizmos.color = Color.blue;
                    Vector3 c1 = new Vector3(roomArray[i].Data.Center.x, 3, roomArray[i].Data.Center.z);
                    Vector3 c2 = new Vector3(roomArray[j].Data.Center.x, 3, roomArray[j].Data.Center.z);
                    centerList.Add(new centerPair(c1, c2)); //serve per disegnare il grafo rosso di overlay

                    //ora quindi si crea il corridoio tra i e j                    
                    createCorridor(corridorsGO,gameObjectRoomArray[i], gameObjectRoomArray[j]); // va passato il gameobject istanziato nello spazio e non il roomArray[i]
                    corridorsGO.transform.parent = transform;
                    //print("ROOM " + gameObjectRoomArray[i] + " --- connected to ROOM --- " + gameObjectRoomArray[j]);
                }
            }
        }//fine algoritmo di connessione        
        //foreach (KeyValuePair<IntVector2, DungeonCell> entry in activeDungeonCells)
        //{
        //    print("key: " + entry.Key + ", value: " + entry.Value);
        //}
        //print(tileMatrix);
        print("current seed: " +Random.seed);
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
    private void createCorridor(GameObject corridors,DungeonRoom roomA, DungeonRoom roomB)
    {
        int dir = Random.Range(0,2);
        int dx = roomA.Data.Center.x - roomB.Data.Center.x;
        int dz = roomA.Data.Center.z - roomB.Data.Center.z;
        string nextDirX;
        string nextDirZ;

        //le stanze sono connesse da 2 segmenti, qui imposto la direzione del secondo segmenti di ogni connessione
        if (dz < 0) nextDirZ = "north";
        else nextDirZ = "south";
        if (dx < 0) nextDirX = "east";
        else nextDirX = "west";

        switch (dir)
        {
            case 0://prima corridoio orizzontale poi verticale
                {
                    //passo roomA per poi usare le sue celle controllare se ci sono eventuali muri da rimuovere
                    IntVector2 lastPosH = createHorizontalCorridor(corridors,roomA.Data.Center, dx,nextDirZ);//crea il pezzo di corridoio orizzontale partendo dalla stanza A                    
                    IntVector2 lastPosV = createVerticalCorridor(corridors,lastPosH, dz, "");////crea il pezzo di corridoio verticale partendo dalla stanza A
                    break;
                }
            case 1://viceversa
                {
                    IntVector2 lastPosV = createVerticalCorridor(corridors,roomA.Data.Center, dz, nextDirX);////crea il pezzo di corridoio verticale partendo dalla stanza A
                    IntVector2 lastPosH = createHorizontalCorridor(corridors,lastPosV, dx, "");//crea il pezzo di corridoio orizzontale partendo dalla stanza A
                    break;
                }
            default:
                break;
        }
    }


    //riceve in input la posizione di inizion del corridoio, il centro quindi e poi la lunghezza del segmento in questo caso
    //orizzontale e infine nextDirZ è la stringa che indica la direzione del segmento verticale successivo
    private IntVector2 createHorizontalCorridor(GameObject corridors, IntVector2 startPos, int lenght, string nextDirZ)
    {
        DungeonCell aCell = null;
        int direction = 0;
        bool lastSegmentTile = false;
        string nextSdir = "none";
        for (int i = 0; i < Mathf.Abs(lenght); i++)
        {
            if (i == (Mathf.Abs(lenght) - 1))
            {
                nextSdir = nextDirZ;
                lastSegmentTile = true;
            }
            if (lenght < 0)//cresce verso destra
            {
                startPos = new IntVector2(startPos.x + 1, startPos.z);                
                aCell = createCorridorTile(corridors, startPos, "east", nextSdir, lastSegmentTile);
                direction = 1;
            }
            else if (lenght > 0)//cresce verso sinistra
            {
                startPos = new IntVector2(startPos.x - 1, startPos.z);
                aCell = createCorridorTile(corridors ,startPos, "west", nextSdir, lastSegmentTile);
                direction = -1;
            }
            //tranne che sull'ultima tile metto le mura laterali
            //controllo che la cella sia stata effettivamente creata visto che la posizione potrebbere essere già occupata da 1 o 2 nella tilematrix
            if (i < (Mathf.Abs(lenght) - 1) && aCell !=null)
                CreateCorridorWalls(aCell, 0, aCell.transform);
        }

        //gestione delle mura nell'ultime tile con i 4 casi possibili, ma prima di tutto controllo che la cella sia stata veramente creata
        if (aCell != null)
        {
            if (direction == 1)
            {
                if (nextDirZ == "north")
                {
                    //se sotto l'ultima cella non ci sono celle allora creo il pezzo di muro finale
                    if (tileMatrix[aCell.Coordinates.z - 1, aCell.Coordinates.x] == 0 && (aCell.Coordinates.z - 1) >= 0)
                    {
                        createSingleWall(aCell, "south", aCell.transform);
                    }
                    //se a destra dell'ultima cella non c'è nulla creo il pezzo di muro finale
                    if (tileMatrix[aCell.Coordinates.z, aCell.Coordinates.x + 1] == 0 && (aCell.Coordinates.x + 1) <= tileMatrix._w )
                    {
                        createSingleWall(aCell, "east", aCell.transform);
                    }
                }
                if (nextDirZ == "south")
                {
                    if (tileMatrix[aCell.Coordinates.z + 1, aCell.Coordinates.x] == 0 && (aCell.Coordinates.z + 1) <= tileMatrix._h)
                    {
                        createSingleWall(aCell, "north", aCell.transform);
                    }
                    if (tileMatrix[aCell.Coordinates.z, aCell.Coordinates.x + 1] == 0 && (aCell.Coordinates.x + 1) <= tileMatrix._w)
                    {
                        createSingleWall(aCell, "east", aCell.transform);
                    }
                }
            }
            if (direction == -1)
            {
                if (nextDirZ == "north")
                {
                    if (tileMatrix[aCell.Coordinates.z - 1, aCell.Coordinates.x] == 0 && (aCell.Coordinates.z - 1) >= 0)
                    {
                        createSingleWall(aCell, "south", aCell.transform);
                    }
                    if (tileMatrix[aCell.Coordinates.z, aCell.Coordinates.x - 1] == 0 && (aCell.Coordinates.x - 1) >= 0)
                    {
                        createSingleWall(aCell, "west", aCell.transform);
                    }
                }
                if (nextDirZ == "south")
                {
                    if (tileMatrix[aCell.Coordinates.z + 1, aCell.Coordinates.x] == 0 && (aCell.Coordinates.z + 1) <= tileMatrix._h)
                    {
                        createSingleWall(aCell, "north", aCell.transform);
                    }
                    if (tileMatrix[aCell.Coordinates.z, aCell.Coordinates.x - 1] == 0 && (aCell.Coordinates.x - 1) >= 0)
                    {
                        createSingleWall(aCell, "west", aCell.transform);
                    }
                }
            }
        }
        return startPos;
    }

    private IntVector2 createVerticalCorridor(GameObject corridors, IntVector2 startPos, int lenght, string nextDirX)
    {
        DungeonCell aCell = null;
        int direction = 0;
        bool lastSegmentTile = false;
        string nextSdir = "none";
        for (int i = 0; i < Mathf.Abs(lenght); i++)
        {
            if (i == (Mathf.Abs(lenght) - 1))
            {
                nextSdir = nextDirX;
                lastSegmentTile = true;
            }
            if (lenght < 0)
            {
                startPos = new IntVector2(startPos.x, startPos.z + 1);
                aCell = createCorridorTile(corridors,startPos, "north", nextSdir, lastSegmentTile);                                          
                direction = 1;
            }
            else if (lenght > 0)
            {
                startPos = new IntVector2(startPos.x, startPos.z - 1);
                aCell = createCorridorTile(corridors,startPos, "south", nextSdir, lastSegmentTile);
                direction = -1;
            }            
            if (i < (Mathf.Abs(lenght) - 1) && aCell != null)
                CreateCorridorWalls(aCell, 1, aCell.transform);
        }

        if (aCell != null)
        {
            if (direction == 1)
            {
                if (nextDirX == "east")
                {
                    if (tileMatrix[aCell.Coordinates.z, aCell.Coordinates.x - 1] == 0 && (aCell.Coordinates.x - 1) >= 0)
                    {                        
                        createSingleWall(aCell, "west", aCell.transform);
                    }
                    if (tileMatrix[aCell.Coordinates.z + 1, aCell.Coordinates.x] == 0 && (aCell.Coordinates.z + 1) <= tileMatrix._h)
                    {
                        createSingleWall(aCell, "north", aCell.transform);
                    }
                }
                if (nextDirX == "west")
                {
                    if (tileMatrix[aCell.Coordinates.z + 1, aCell.Coordinates.x] == 0 && (aCell.Coordinates.z + 1) <= tileMatrix._h)
                    {
                        createSingleWall(aCell, "north", aCell.transform);
                    }
                    if (tileMatrix[aCell.Coordinates.z, aCell.Coordinates.x + 1] == 0 && (aCell.Coordinates.x + 1) <= tileMatrix._w )
                    {
                        createSingleWall(aCell, "east", aCell.transform);
                    }
                }
            }
            if (direction == -1)
            {
                if (nextDirX == "east")
                {
                    if (tileMatrix[aCell.Coordinates.z - 1, aCell.Coordinates.x] == 0 && (aCell.Coordinates.z - 1) >= 0)
                    {
                        createSingleWall(aCell, "south", aCell.transform);
                    }
                    if (tileMatrix[aCell.Coordinates.z, aCell.Coordinates.x-1] == 0)
                    {
                        createSingleWall(aCell, "west", aCell.transform);
                    }
                }
                if (nextDirX == "west")
                {
                    if (tileMatrix[aCell.Coordinates.z - 1, aCell.Coordinates.x] == 0 && (aCell.Coordinates.z - 1) >=0)
                    {
                        createSingleWall(aCell, "south", aCell.transform);
                    }
                    if (tileMatrix[aCell.Coordinates.z, aCell.Coordinates.x+1] == 0)
                    {
                        createSingleWall(aCell, "east", aCell.transform);
                    }
                }
            }
        }
        return startPos;
    }

    //il booleano doubleDirection=false lo uso quando devo distruggere un solo singolo muro nella direzione direction 
    public void destroyWall(IntVector2 coordinates, string direction, bool doubleDirection)
    {
        DungeonCell tmp;
        if (doubleDirection)
        {
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
        else
        {
            if (activeDungeonCells.TryGetValue(coordinates, out tmp))
                {
                    tmp.destroyWall(direction);
                }
        }
    }

    private DungeonCell createCorridorTile(GameObject corridors, IntVector2 c, string sDir, string nextSdir, bool lastSegmentTile)
    {
        DungeonCell aCell = null;
        if (tileMatrix[c.z, c.x] == 0)
        {//se non c'è sovrapposizione crea la mattonella del corridoio            
            tileMatrix[c.z, c.x] = 2;                 
            aCell = CreateCorridorCell(c, corridors);
            //print("Corridor tile created at " + c);
            try
            {
                activeDungeonCells.Add(c, aCell);
            }
            catch (System.ArgumentException)
            {
                print("An element with Key = \"txt\" already exists.");
            }
            return aCell;
        }
        //se sono all'interno di una stanza mentre costruisco il percorso di connessione distruggo le mura nord-sud oppure east-ovest
        //nella direzione di marcia solo se non è l'ultimo pezzo di segmento
        if (tileMatrix[c.z, c.x] == 1 && !lastSegmentTile)
        {
            destroyWall(c, sDir, true);
            //print("FOUND tile TYPE 1 i need to destroy wall at " + c + " direction " + sDir);
        }
        //se invece è l'ultimo segmento distruggo un solo muro nella direzione del prossimo segmento nextSDir
        else if (tileMatrix[c.z, c.x] == 1 && lastSegmentTile)
        {
            destroyWall(c, nextSdir, false);
            //libero anche eventuali mura nell'ultima cella del segmento che non ho distrutto
            // perchè passo false come paramentro, se avessi passato true avrei rimosso le mura nella direzione di percorrenza 
            //in questo caso però rimuovendo due mura si corre il rischio di eliminare mura perimetrali delle stanze infatti
            // l'ultimo tile di questo segmento potrebbe trovarsi all'interno di una stanza ma sul perimetro
            destroyWall(c, oppositeDir(sDir), false);
        }
        //gestisco il caso in cui sto creando tile di corridoio in posizioni già occupate da altri pezzi di corridoio
        if (tileMatrix[c.z, c.x] == 2)
        {
            if (lastSegmentTile)
            {
                //se sono l'ultimo segmento faccio attenzione a non rimuovere mura che non sono del segmento in questione
                destroyWall(c, nextSdir, false);//rimuovo le mura solo nella direzione del prossimo segmento
                destroyWall(c,oppositeDir(sDir),false);//rimuovo l'ultimo eventuale muro nella direzione attuale
                //print("FOUND last segment TYPE 2 i need to destroy wall at " + c + " direction " + nextSdir);
            }
            else
            {
                //se non sono l'ultima cella segmente distruggo tutto nella mia direzione quindi coppie di mura east,ovest oppure nord,sud
                destroyWall(c, sDir, true);
                //print("FOUND TYPE 2 i need to destroy wall at " + c + " direction " + sDir);            
            }            
        }
        return null;
    }

    private string oppositeDir(string dir)
    {
        string ret = "";
        if (dir == "north")
        {
            ret = "south";
        }
        if (dir == "south")
        {
            ret = "north";
        }
        if (dir == "east")
        {
            ret = "west";
        }
        if (dir == "west")
        {
            ret = "east";
        }
        return ret;
    }

    private void CreateCorridorWalls(DungeonCell cell, int dir, Transform parent)
    {            
        if( dir == 0 ){
            if (tileMatrix[cell.Coordinates.z + 1, cell.Coordinates.x] == 0)
            {
                WallUnit aWall = InstanciateWall(Directions.directionVectors[(int)Direction.North], Direction.North, cell, parent);
                cell.addWallRefenceToCell(aWall, "north");
            }
            if (tileMatrix[cell.Coordinates.z-1, cell.Coordinates.x] == 0)
            {
                WallUnit aWall = InstanciateWall(Directions.directionVectors[(int)Direction.South], Direction.South, cell, parent);
                cell.addWallRefenceToCell(aWall, "south");
            }
        }          
        if ( dir == 1 )
        {
            if (tileMatrix[cell.Coordinates.z, cell.Coordinates.x - 1] == 0)
            {
                WallUnit aWall = InstanciateWall(Directions.directionVectors[(int)Direction.West], Direction.West, cell, parent);
                cell.addWallRefenceToCell(aWall, "west");
            }
            if (tileMatrix[cell.Coordinates.z, cell.Coordinates.x + 1] == 0)
            {
                WallUnit aWall = InstanciateWall(Directions.directionVectors[(int)Direction.East], Direction.East, cell, parent);
                cell.addWallRefenceToCell(aWall, "east");
            }
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
    private DungeonCell CreateCorridorCell(IntVector2 coordinates, GameObject corridors)
    {
        DungeonCell newDungeonCell = Instantiate(dungeonCellPrefab) as DungeonCell;
        //cells[coordinates.x,coordinates.z] = newDungeonCell;
        newDungeonCell.name = "Dungeon Corridor Cell " + coordinates.x + ", " + coordinates.z;
        newDungeonCell.Coordinates = coordinates;        
        newDungeonCell.transform.localPosition = new Vector3(coordinates.x + 0.5f, 0f, coordinates.z + 0.5f);
        newDungeonCell.transform.parent = corridors.transform;
        return newDungeonCell;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (centerList != null)
        {
            foreach (var c in centerList)
            {
                Gizmos.DrawLine(c._c1, c._c2);
            }
        }
    }

    public void ToggleGizmos(bool gizmosOn)
    {
        int val = gizmosOn ? 1 : 0;
        Assembly asm = Assembly.GetAssembly(typeof(Editor));
        System.Type type = asm.GetType("UnityEditor.AnnotationUtility");
        if (type != null)
        {
            MethodInfo getAnnotations = type.GetMethod("GetAnnotations", BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo setGizmoEnabled = type.GetMethod("SetGizmoEnabled", BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo setIconEnabled = type.GetMethod("SetIconEnabled", BindingFlags.Static | BindingFlags.NonPublic);
            var annotations = getAnnotations.Invoke(null, null);
            foreach (object annotation in (IEnumerable)annotations)
            {
                System.Type annotationType = annotation.GetType();
                FieldInfo classIdField = annotationType.GetField("classID", BindingFlags.Public | BindingFlags.Instance);
                FieldInfo scriptClassField = annotationType.GetField("scriptClass", BindingFlags.Public | BindingFlags.Instance);
                if (classIdField != null && scriptClassField != null)
                {
                    int classId = (int)classIdField.GetValue(annotation);
                    string scriptClass = (string)scriptClassField.GetValue(annotation);
                    setGizmoEnabled.Invoke(null, new object[] { classId, scriptClass, val });
                    setIconEnabled.Invoke(null, new object[] { classId, scriptClass, val });
                }
            }
        }
    }


    public void showRNGGraph(bool g)
    {     
        showRNGonGizmos = g;       
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
