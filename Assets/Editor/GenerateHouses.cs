using UnityEngine;
using UnityEditor;
using UnityEngine.ProBuilder;

public static class GenerateHousesEditor
{
    const float W = 10f, D = 10f, H = 5f, T = 0.3f;
    const float DoorW = 2.4f, DoorH = 3f;
    const float WinW = 1.5f, WinH = 1.5f, WinYBase = 2.5f;

    [MenuItem("VostokLike/Generate Houses")]
    static void Execute()
    {
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Generate Houses");

        Vector3 basePos = new Vector3(-480.8638f, 2221.213f, -1839.38f);

        for (int i = 0; i < 3; i++)
        {
            Vector3 pos = basePos + new Vector3(i * (W + 5f), 0f, 0f);
            BuildHouse(pos, "House_" + (i + 1));
        }

        Debug.Log("家3軒を生成しました。NavMeshを手動で再ベイクしてください。");
    }

    static void BuildHouse(Vector3 origin, string houseName)
    {
        GameObject root = new GameObject(houseName);
        root.transform.position = origin;
        Undo.RegisterCreatedObjectUndo(root, "Generate Houses");

        AddSection(root, "Floor",
            new Vector3(W / 2f, T / 2f, D / 2f),
            new Vector3(W, T, D));

        AddSection(root, "Roof",
            new Vector3(W / 2f, H + T / 2f, D / 2f),
            new Vector3(W + T * 2f, T, D + T * 2f));

        BuildDoorWall(root, "FrontWall", T / 2f);
        BuildDoorWall(root, "BackWall",  D - T / 2f);
        BuildWindowWall(root, "LeftWall",  T / 2f);
        BuildWindowWall(root, "RightWall", W - T / 2f);
    }

    static void BuildDoorWall(GameObject root, string wallName, float zCenter)
    {
        float dLeft  = (W - DoorW) / 2f;
        float dRight = dLeft + DoorW;
        float topH   = H - DoorH;

        AddSection(root, wallName + "_L",
            new Vector3(dLeft / 2f, H / 2f, zCenter),
            new Vector3(dLeft, H, T));

        AddSection(root, wallName + "_R",
            new Vector3((dRight + W) / 2f, H / 2f, zCenter),
            new Vector3(W - dRight, H, T));

        AddSection(root, wallName + "_T",
            new Vector3(W / 2f, DoorH + topH / 2f, zCenter),
            new Vector3(DoorW, topH, T));
    }

    static void BuildWindowWall(GameObject root, string wallName, float xCenter)
    {
        float win1Z = D / 3f;
        float win2Z = D * 2f / 3f;
        float w1L = win1Z - WinW / 2f;
        float w1R = win1Z + WinW / 2f;
        float w2L = win2Z - WinW / 2f;
        float w2R = win2Z + WinW / 2f;
        float winTop = WinYBase + WinH;
        float aboveH = H - winTop;

        AddSection(root, wallName + "_SL",
            new Vector3(xCenter, H / 2f, w1L / 2f),
            new Vector3(T, H, w1L));

        AddSection(root, wallName + "_SM",
            new Vector3(xCenter, H / 2f, (w1R + w2L) / 2f),
            new Vector3(T, H, w2L - w1R));

        AddSection(root, wallName + "_SR",
            new Vector3(xCenter, H / 2f, (w2R + D) / 2f),
            new Vector3(T, H, D - w2R));

        AddSection(root, wallName + "_B1",
            new Vector3(xCenter, WinYBase / 2f, win1Z),
            new Vector3(T, WinYBase, WinW));

        AddSection(root, wallName + "_A1",
            new Vector3(xCenter, winTop + aboveH / 2f, win1Z),
            new Vector3(T, aboveH, WinW));

        AddSection(root, wallName + "_B2",
            new Vector3(xCenter, WinYBase / 2f, win2Z),
            new Vector3(T, WinYBase, WinW));

        AddSection(root, wallName + "_A2",
            new Vector3(xCenter, winTop + aboveH / 2f, win2Z),
            new Vector3(T, aboveH, WinW));
    }

    static void AddSection(GameObject parent, string sectionName, Vector3 localCenter, Vector3 size)
    {
        ProBuilderMesh pb = MakeCube(size);
        pb.gameObject.name = sectionName;
        pb.transform.SetParent(parent.transform);
        pb.transform.localPosition = localCenter;
        pb.ToMesh();
        pb.Refresh();
        pb.gameObject.AddComponent<BoxCollider>();
        GameObjectUtility.SetStaticEditorFlags(pb.gameObject,
            StaticEditorFlags.ContributeGI |
            StaticEditorFlags.BatchingStatic);
        Undo.RegisterCreatedObjectUndo(pb.gameObject, "Generate Houses");
    }

    // PivotType を使わずに ProBuilderMesh.Create で中心基準のキューブを生成
    static ProBuilderMesh MakeCube(Vector3 size)
    {
        var t = new Vector3[]
        {
            // top
            new Vector3(-.5f, .5f, -.5f), new Vector3(.5f, .5f, -.5f),
            new Vector3(-.5f, .5f,  .5f), new Vector3(.5f, .5f,  .5f),
            // bottom
            new Vector3(-.5f, -.5f, -.5f), new Vector3(.5f, -.5f, -.5f),
            new Vector3(-.5f, -.5f,  .5f), new Vector3(.5f, -.5f,  .5f),
            // front
            new Vector3(-.5f,  .5f, -.5f), new Vector3(.5f,  .5f, -.5f),
            new Vector3(-.5f, -.5f, -.5f), new Vector3(.5f, -.5f, -.5f),
            // back
            new Vector3(-.5f,  .5f, .5f), new Vector3(.5f,  .5f, .5f),
            new Vector3(-.5f, -.5f, .5f), new Vector3(.5f, -.5f, .5f),
            // right
            new Vector3(.5f,  .5f, -.5f), new Vector3(.5f,  .5f, .5f),
            new Vector3(.5f, -.5f, -.5f), new Vector3(.5f, -.5f, .5f),
            // left
            new Vector3(-.5f,  .5f, -.5f), new Vector3(-.5f,  .5f, .5f),
            new Vector3(-.5f, -.5f, -.5f), new Vector3(-.5f, -.5f, .5f),
        };

        var positions = new Vector3[24];
        for (int i = 0; i < 24; i++)
            positions[i] = Vector3.Scale(t[i], size);

        return ProBuilderMesh.Create(positions, new Face[]
        {
            new Face(new[] {  0,  1,  2,  1,  3,  2 }),
            new Face(new[] {  4,  5,  6,  5,  7,  6 }),
            new Face(new[] {  8,  9, 10,  9, 11, 10 }),
            new Face(new[] { 12, 13, 14, 13, 15, 14 }),
            new Face(new[] { 16, 17, 18, 17, 19, 18 }),
            new Face(new[] { 20, 21, 22, 21, 23, 22 }),
        });
    }
}
