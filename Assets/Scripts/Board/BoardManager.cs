using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;
    
    [Header("Board Settings")]
    public float cellWidth = 1.5f;
    public float cellLength = 1.5f;
    public float cellSpacing = 0.5f;
    public float rowSpacing = 1.8f;
    public float heightOffset = 0.0f;
    public float xOffset = 0f;
    public float zOffset = 0f;
    public Vector3 globalBiomeRotation = Vector3.zero;
    public GameObject cellPrefab;
    
    [Header("Board State")]
    public BoardCell[,] cells = new BoardCell[2, 3];
    
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        CreateBoard();
    }

    void CreateBoard()
    {
        float totalWidth = 3 * cellWidth + 2 * cellSpacing;
        float startX = -totalWidth / 2 + cellWidth / 2;
        
        for (int row = 0; row < 2; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                float zPos = (row == 0 ? -rowSpacing : rowSpacing) + zOffset;
                
                Vector3 position = new Vector3(
                    startX + col * (cellWidth + cellSpacing) + xOffset,
                    heightOffset,
                    zPos
                );
                
                GameObject cellObj = new GameObject($"Cell_{row}_{col}");
                cellObj.transform.SetParent(transform);
                cellObj.transform.localPosition = position;
                
                GameObject visualObj;
                if (cellPrefab != null)
                    visualObj = Instantiate(cellPrefab, cellObj.transform);
                else
                {
                    visualObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    visualObj.transform.SetParent(cellObj.transform);
                }
                
                visualObj.name = "Visual";
                visualObj.transform.localPosition = Vector3.zero;
                visualObj.transform.localRotation = Quaternion.identity;
                visualObj.transform.localScale = new Vector3(cellWidth, 0.1f, cellLength);

                BoardCell cell = cellObj.GetComponent<BoardCell>();
                if (cell == null)
                    cell = cellObj.AddComponent<BoardCell>();
                
                cell.row = row;
                cell.column = col;
                cell.isPlayerSide = (row == 0);
                
                if (cellObj.GetComponent<CardPickup>() == null)
                    cellObj.AddComponent<CardPickup>();
                
                Renderer rend = visualObj.GetComponent<Renderer>();
                if (rend != null)
                {
                    rend.material.color = cell.isPlayerSide ? 
                        new Color(0.3f, 0.5f, 0.3f) : new Color(0.5f, 0.3f, 0.3f);
                }
                
                cells[row, col] = cell;
            }
        }
    }

    void Update()
    {
        if (Application.isEditor)
            UpdateBoardPositions();
    }

    void UpdateBoardPositions()
    {
        float totalWidth = 3 * cellWidth + 2 * cellSpacing;
        float startX = -totalWidth / 2 + cellWidth / 2;
        
        for (int row = 0; row < 2; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                BoardCell cell = cells[row, col];
                if (cell != null)
                {
                    float zPos = (row == 0 ? -rowSpacing : rowSpacing) + zOffset;
                    
                    Vector3 position = new Vector3(
                        startX + col * (cellWidth + cellSpacing) + xOffset,
                        heightOffset,
                        zPos
                    );
                    
                    cell.transform.localPosition = position;
                    
                    Transform visual = cell.transform.Find("Visual");
                    if (visual == null && cell.transform.childCount > 0)
                    {
                        Transform child = cell.transform.GetChild(0);
                        if (!child.name.Contains("Card")) visual = child;
                    }

                    if (visual != null)
                        visual.localScale = new Vector3(cellWidth, 0.1f, cellLength);
                }
            }
        }
    }

    public BoardCell GetCell(int row, int col)
    {
        if (row >= 0 && row < 2 && col >= 0 && col < 3)
            return cells[row, col];
        return null;
    }

    public BoardCell[] GetPlayerCells()
    {
        return new BoardCell[] { cells[0, 0], cells[0, 1], cells[0, 2] };
    }

    public BoardCell[] GetEnemyCells()
    {
        return new BoardCell[] { cells[1, 0], cells[1, 1], cells[1, 2] };
    }

    public void HighlightPlayerCells(bool highlight)
    {
        foreach (var cell in GetPlayerCells())
            cell.SetHighlight(highlight);
    }

    public void ClearHighlights()
    {
        for (int row = 0; row < 2; row++)
        {
            for (int col = 0; col < 3; col++)
                cells[row, col].SetHighlight(false);
        }
    }
}

