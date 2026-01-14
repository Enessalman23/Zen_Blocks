using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class DraggableShape : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 originalPosition;
    private List<Transform> childSquares = new List<Transform>();
    private List<SpriteRenderer> childRenderers = new List<SpriteRenderer>();
    private Vector3 mouseOffset;

    void Start()
    {
        originalPosition = transform.position;
        RefreshChildSquares();

        transform.localScale = Vector3.zero;
        transform.DOScale(0.6f, 0.4f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    void RefreshChildSquares()
    {
        childSquares.Clear();
        childRenderers.Clear();
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf) 
            {
                childSquares.Add(child);
                SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
                if (sr != null) childRenderers.Add(sr);
            }
        }
    }

    void Update()
    {
        if (!isDragging) return;

        // GameManager kontrolü
        if (GameManager.instance != null && !GameManager.instance.isGameStarted)
        {
            CancelDragging();
            return;
        }

        transform.position = GetMouseWorldPos() + mouseOffset;

        // Önizleme Sistemi
        if (GridManager.instance != null)
        {
            GridManager.instance.ShowPreview(this.gameObject, transform.position);
        }

        if (Input.GetMouseButtonUp(0)) 
        {
            if (GridManager.instance != null) GridManager.instance.ClearPreviews();
            DropShape();
        }
    }

    void OnMouseDown()
    {
        if (GameManager.instance != null && !GameManager.instance.isGameStarted) return;
        if (!this.enabled || Time.timeScale == 0) return;
        
        isDragging = true;
        SetSortingOrder(100); 
        
        transform.DOKill(); 
        mouseOffset = transform.position - GetMouseWorldPos();
        transform.DOScale(1.0f, 0.15f).SetEase(Ease.OutQuad);
        
        if(AudioManager.instance != null) AudioManager.instance.PlaySound(AudioManager.instance.grabSound);
    }

    void DropShape()
    {
        isDragging = false;
        if (CanPlaceShape()) PlaceShape();
        else CancelDragging();
    }

    void CancelDragging()
    {
        isDragging = false;
        SetSortingOrder(2); 
        transform.DOMove(originalPosition, 0.3f).SetEase(Ease.OutQuint);
        transform.DOScale(0.6f, 0.3f).SetEase(Ease.OutQuad);
    }

    void PlaceShape()
    {
        if (GridManager.instance == null) return;

        foreach (Transform square in childSquares)
        {
            if (square == null) continue;
            Vector2Int gridPos = GridManager.instance.WorldToGridPos(square.position);
            GridManager.instance.SetCellStatus(gridPos.x, gridPos.y, true, square.gameObject);
        }

        SnapToGrid();
        SetSortingOrder(2);

        if(AudioManager.instance != null) AudioManager.instance.PlaySound(AudioManager.instance.placeSound);
        
        transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.2f);

        // Spawner bulma (En garanti yöntem)
        ShapeSpawner spawner = FindFirstObjectByType<ShapeSpawner>();
        if (spawner != null) spawner.ShapePlaced(this.gameObject);

        this.enabled = false;
        if (GetComponent<Collider2D>()) GetComponent<Collider2D>().enabled = false;
        
        foreach (Transform s in childSquares) 
        {
            if(s != null) s.gameObject.tag = "Block";
        }

        GridManager.instance.CheckForMatches();
    }

    void SetSortingOrder(int order)
    {
        foreach (var sr in childRenderers)
        {
            if (sr != null) sr.sortingOrder = order;
        }
    }

    void SnapToGrid()
    {
        if (GridManager.instance == null) return;

        Transform validSquare = childSquares.Find(s => s != null);
        if (validSquare == null) return;

        Vector2Int p = GridManager.instance.WorldToGridPos(validSquare.position);
        
        float startX = -(GridManager.instance.width - 1) * GridManager.instance.spacing / 2f;
        float startY = -(GridManager.instance.height - 1) * GridManager.instance.spacing / 2f;
        
        Vector3 targetWorldPos = new Vector3(startX + (p.x * GridManager.instance.spacing), startY + (p.y * GridManager.instance.spacing), 0);
        
        Vector3 shift = targetWorldPos - validSquare.position;
        transform.position += shift;
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 m = Input.mousePosition;
        m.z = 10f; 
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(m);
        worldPos.z = 0f; 
        return worldPos;
    }

    bool CanPlaceShape()
    {
        if (GridManager.instance == null) return false;

        foreach (Transform s in childSquares)
        {
            if (s == null) continue;
            Vector2Int p = GridManager.instance.WorldToGridPos(s.position);
            
            // Izgara sınırları ve doluluk kontrolü
            if (p.x < 0 || p.x >= GridManager.instance.width || p.y < 0 || p.y >= GridManager.instance.height || GridManager.instance.IsCellOccupied(p.x, p.y))
            {
                return false;
            }
        }
        return true;
    }

    private void OnDestroy()
    {foreach (Transform child in transform) {
            // Kareleri ana objeden ayır (veya zaten Grid'e yeni kareler mi spawn ediyorsun kontrol et)
        }
        Destroy(gameObject);
        transform.DOKill();
    }
}