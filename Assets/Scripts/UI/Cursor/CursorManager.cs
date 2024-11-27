using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    [SerializeField] private CursorTexture[] cursors;

    CursorType currentCursor = CursorType.None;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        SwapCursor(CursorType.Normal);
    }
    private int GetCursorIndex(CursorType type)
    {
        return type switch
        {
            CursorType.Normal => 0,
            CursorType.Hand => 1,
            CursorType.Knife => 2,
            CursorType.Brush => 3,
            _ => -1,
        };
    }

    public void SwapCursor(CursorType cursorType)
    {
        int index = GetCursorIndex(cursorType);
        if (currentCursor == cursorType || index == -1) return;

        currentCursor = cursorType;
        Cursor.SetCursor(cursors[index].cursor, cursors[index].hotSpot, CursorMode.Auto);
    }

}

[System.Serializable]
public class CursorTexture
{
    public Texture2D cursor;
    public Vector2 hotSpot = Vector3.zero;
}

public enum CursorType
{
    None,
    Normal,
    Hand,
    Knife,
    Brush
}