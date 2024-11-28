using UnityEngine;
using UnityEngine.EventSystems;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    [SerializeField] private CursorTexture[] cursors;

    private CursorType currentCursor = CursorType.None;
    private CursorType prevCursor = CursorType.None;
    private bool isCursorOverUI = false;

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

    private void Update()
    {
        if (!isCursorOverUI && EventSystem.current.IsPointerOverGameObject())
        {
            prevCursor = currentCursor;
            SwapCursor(CursorType.Normal);
            isCursorOverUI = true;
        }
        else if (isCursorOverUI && !EventSystem.current.IsPointerOverGameObject())
        {
            isCursorOverUI = false;
            SwapCursor(prevCursor);
        }
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
        if ((currentCursor == cursorType && !isCursorOverUI) || 
            (prevCursor == cursorType && isCursorOverUI) ||
            index == -1) return;

        if (isCursorOverUI)
        {
            prevCursor = cursorType;
        }
        else
        {
            currentCursor = cursorType;
            Cursor.SetCursor(cursors[index].cursor, cursors[index].hotSpot, CursorMode.Auto);
        }
    }

}
