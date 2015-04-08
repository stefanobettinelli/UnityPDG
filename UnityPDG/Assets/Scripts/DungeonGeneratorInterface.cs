using UnityEngine;
using System.Collections;

public interface DungeonGeneratorInterface {
    void CreateDungeon(string dungeonName, int minWidth, int maxWidth, int minHeight, int maxHeight);
}
