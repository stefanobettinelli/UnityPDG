using UnityEngine;
using System.Collections;

public interface DungeonGeneratorInterface {
    Dungeon CreateDungeon(string dungeonName, int minWidth, int maxWidth, int minHeight, int maxHeight, int roomNum, int minShitValue);
}
