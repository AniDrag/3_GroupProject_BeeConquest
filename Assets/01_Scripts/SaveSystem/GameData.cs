using UnityEngine;

public class GameData
{
    public int sceneIndex;
    public int storyChapter;
    public int chapterProgress;

    //add inventory data, npc relations, areas cleared, chosen stuff that effects the game, andy world edits that happened and so on.

    public GameData(GameData loadData/*Player player <-- example, insert draw from source where the data is going to be pulled from*/)
    {
        sceneIndex = loadData.sceneIndex;
        storyChapter = loadData.storyChapter;
        chapterProgress = loadData.chapterProgress;
    }
}
