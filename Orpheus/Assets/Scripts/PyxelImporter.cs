#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

enum OPTIONS
{
    PrefabOnly = 0,
    LevelOnly = 1,
    PrefabAndLevel = 2
}

public class PyxelImporter : EditorWindow
{
    string levelFile = "Level001";

    public Texture2D TilesTexture;

    private int TILEWIDTH = 16;
    private int TILEHEIGHT = 16;

    private int tileCountX = 0;
    private int tileCountY = 0;

    private Sprite[] spriteList;

    private OPTIONS option;

    // Add menu item named "PyxelLoader" to the Window menu
    [MenuItem("Window/PyxelLoader")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindow(typeof(PyxelImporter));
    }

    void OnGUI()
    {
        GUILayout.Label("Pyxel Importer", EditorStyles.boldLabel);
        option = (OPTIONS)EditorGUILayout.EnumPopup("Option :", option);
        EditorGUILayout.Separator();

        switch (option)
        {
            case OPTIONS.PrefabOnly:
                TILEWIDTH = EditorGUILayout.IntField("Tile Width:", TILEWIDTH);
                TILEHEIGHT = EditorGUILayout.IntField("Tile Height:", TILEHEIGHT);
                TilesTexture = (Texture2D)EditorGUILayout.ObjectField("Texture", TilesTexture, typeof(Texture2D), false);
                break;
            case OPTIONS.LevelOnly:
                levelFile = EditorGUILayout.TextField("Level File:", levelFile);
                break;
            case OPTIONS.PrefabAndLevel:
                levelFile = EditorGUILayout.TextField("Level File:", levelFile);
                TilesTexture = (Texture2D)EditorGUILayout.ObjectField("Texture", TilesTexture, typeof(Texture2D), false);
                TILEWIDTH = EditorGUILayout.IntField("Tile Width:", TILEWIDTH);
                TILEHEIGHT = EditorGUILayout.IntField("Tile Height:", TILEHEIGHT);
                break;
            default:
                break;
        }

        // Generate Button
        Rect rect = EditorGUILayout.BeginHorizontal("Button");
        if (GUI.Button(rect, GUIContent.none))
        {
            switch (option)
            {
                case OPTIONS.PrefabOnly:
                    CreatePrefabs();
                    break;
                case OPTIONS.LevelOnly:
                    CreateLevel();
                    break;
                case OPTIONS.PrefabAndLevel:
                    CreatePrefabs();
                    CreateLevel();
                    break;
                default:
                    break;
            }
        }
        GUILayout.Label("Generate Prefabs");
        EditorGUILayout.EndHorizontal();
    }

    private void CreatePrefabs()
    {
        // Load Sprites
        spriteList = Resources.LoadAll<Sprite>(TilesTexture.name);
        tileCountX = TilesTexture.width / TILEWIDTH;
        tileCountY = TilesTexture.height / TILEHEIGHT;
        // Create Prefabs
        for (int y = 0; y < tileCountY; y++)
        {
            for (int x = 0; x < tileCountX; x++)
            {
                if (x + y * tileCountX >= spriteList.Length)
                {
                    break;
                }
                GameObject newTile = Resources.Load<GameObject>("Tiles/Tile" + (x + y * tileCountX).ToString("d4"));
                if (newTile == null)
                {
                       newTile = new GameObject();
                    SpriteRenderer spriteRenderer = (SpriteRenderer)newTile.AddComponent<SpriteRenderer>();
                    spriteRenderer.sprite = spriteList[x + y * tileCountX];
                    newTile.transform.position = new Vector3(x, -y, 0f);
                    newTile.name = "Tile" + (x + y * tileCountX).ToString("d4");
                    PrefabUtility.CreatePrefab("Assets/Resources/Tiles/" + newTile.gameObject.name + ".prefab", newTile, ReplacePrefabOptions.ConnectToPrefab);
                    GameObject.DestroyImmediate(newTile);
                }
            }
        }
    }

    private void CreateLevel()
    {
        // Create Parent GameObject
        GameObject parentObject = new GameObject();
        parentObject.name = levelFile;

        // Get listof tiles.
        List<GameObject> tiles = new List<GameObject>();
        int tileIndex = 0;

        // Get sprites from Resources/Tiles
        GameObject gameobject = Resources.Load<GameObject>("Tiles/Tile" + tileIndex.ToString("d4"));
        do
        {
            tiles.Add(gameobject);
            tileIndex++;
            gameobject = Resources.Load<GameObject>("Tiles/Tile" + tileIndex.ToString("d4"));
        } while (gameobject != null);

        // Generate level from level file
        try
        {
            TextAsset bindata = Resources.Load(levelFile) as TextAsset;
            string str = bindata.text;
            string[] lines = str.Split(new string[] { System.Environment.NewLine }, System.StringSplitOptions.None);
            int tileswide = int.Parse(lines[0].Split(' ')[1]);
            int tilesheight = int.Parse(lines[1].Split(' ')[1]);
            int lineIndex = 6;

            do
            {
                int[,] Layer = ReadLayer(tileswide, tilesheight, lines, lineIndex);

                string result = string.Empty;
                for (int y = 0; y < tilesheight; y++)
                {
                    for (int x = 0; x < tileswide; x++)
                    {
                        result += Layer[x, y].ToString() + ",";
                        if (Layer[x, y] != -1)
                        {
                            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(tiles[Layer[x, y]]);
                            go.transform.position = new Vector3(x, -y, 0f);
                            go.transform.parent = parentObject.transform;
                        }
                    }
                    result += System.Environment.NewLine;
                }
                lineIndex += tilesheight + 2;
            } while (lineIndex < lines.Length);
        }
        catch (System.Exception)
        {

        }
    }

    private int[,] ReadLayer(int tileswide, int tilesheight, string[] lines, int index)
    {
        int[,] result = new int[tileswide, tilesheight];

        for (int y = 0; y < tilesheight; y++)
        {
            string[] lineRow = lines[index++].Split(',');
            for (int x = 0; x < tileswide; x++)
            {
                result[x, y] = int.Parse(lineRow[x]);
            }
        }
        return result;
    }

}
#endif