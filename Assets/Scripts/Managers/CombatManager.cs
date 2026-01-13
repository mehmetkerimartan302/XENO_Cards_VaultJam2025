using UnityEngine;
using System.Collections;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;
    
    [Header("Combat Settings")]
    public float attackDelay = 0.5f;
    public float clashDistance = 1.5f;
    public float clashSpeed = 1.5f;
    public float shakeDuration = 0.4f;
    public float shakeIntensity = 0.08f;

    void Awake()
    {
        Instance = this;
    }

    public void ResolveCombat()
    {
        StartCoroutine(CombatSequence());
    }

    IEnumerator CombatSequence()
    {
        var playerCells = BoardManager.Instance.GetPlayerCells();
        var enemyCells = BoardManager.Instance.GetEnemyCells();

        foreach (var cell in enemyCells)
        {
            if (cell.HasCharacter())
                CellVisualManager.Instance.RevealCard(cell);
        }
        
        yield return new WaitForSeconds(0.6f);
        yield return StartCoroutine(ConsolidateAllStats(playerCells, enemyCells));
        
        int playerSlotWins = 0;
        int enemySlotWins = 0;
        int totalPlayerDamage = 0;
        int totalEnemyDamage = 0;
        
        for (int col = 0; col < 3; col++)
        {
            BoardCell playerCell = playerCells[col];
            BoardCell enemyCell = enemyCells[col];
            
            if (playerCell.HasCharacter() && enemyCell.HasCharacter())
            {
                Transform playerCard = GetCardVisual(playerCell);
                Transform enemyCard = GetCardVisual(enemyCell);
                
                if (playerCard != null && enemyCard != null)
                    yield return StartCoroutine(ClashAnimation(playerCard, enemyCard));
                
                int playerDamage = playerCell.GetAttackPower();
                int enemyDamage = enemyCell.GetAttackPower();
                
                totalPlayerDamage += playerDamage;
                totalEnemyDamage += enemyDamage;
                
                enemyCell.TakeDamage(playerDamage);
                playerCell.TakeDamage(enemyDamage);
                
                bool playerDied = playerCell.currentHealth <= 0;
                bool enemyDied = enemyCell.currentHealth <= 0;
                
                if (enemyDied && !playerDied)
                    playerSlotWins++;
                else if (playerDied && !enemyDied)
                    enemySlotWins++;
                
                if (playerCard != null && enemyCard != null)
                {
                    if (enemyDamage > playerDamage || playerDied)
                        StartCoroutine(ShakeAnimation(playerCard));
                    if (playerDamage > enemyDamage || enemyDied)
                        StartCoroutine(ShakeAnimation(enemyCard));
                    if (enemyDamage == playerDamage && !playerDied && !enemyDied)
                    {
                        StartCoroutine(ShakeAnimation(playerCard));
                        StartCoroutine(ShakeAnimation(enemyCard));
                    }
                    
                    yield return new WaitForSeconds(shakeDuration);
                    
                    if (playerDied && playerCard != null)
                        StartCoroutine(DestroyCardAnimation(playerCard));
                    if (enemyDied && enemyCard != null)
                        StartCoroutine(DestroyCardAnimation(enemyCard));
                    
                    if (playerDied || enemyDied)
                        yield return new WaitForSeconds(0.6f);
                }
                
                yield return new WaitForSeconds(attackDelay);
            }
            else if (playerCell.HasCharacter() && !enemyCell.HasCharacter())
            {
                playerSlotWins++;
                totalPlayerDamage += playerCell.GetAttackPower();
            }
            else if (!playerCell.HasCharacter() && enemyCell.HasCharacter())
            {
                enemySlotWins++;
                totalEnemyDamage += enemyCell.GetAttackPower();
            }
        }
        
        DetermineRoundWinner(playerSlotWins, enemySlotWins, totalPlayerDamage, totalEnemyDamage);
        
        yield return new WaitForSeconds(0.5f);
        GameManager.Instance.OnCombatEnd();
    }
    
    void DetermineRoundWinner(int playerWins, int enemyWins, int playerDamage, int enemyDamage)
    {
        if (playerWins > enemyWins)
            GameManager.Instance.playerScore += 1;
        else if (enemyWins > playerWins)
            GameManager.Instance.enemyScore += 1;
        else
        {
            if (playerDamage > enemyDamage)
                GameManager.Instance.playerScore += 1;
            else if (enemyDamage > playerDamage)
                GameManager.Instance.enemyScore += 1;
            else
                GameManager.Instance.TriggerOvertime();
        }
    }

    Transform GetCardVisual(BoardCell cell)
    {
        foreach (Transform child in cell.transform)
        {
            if (child.name.StartsWith("Card_"))
                return child;
        }
        return null;
    }
    
    IEnumerator ConsolidateAllStats(BoardCell[] playerCells, BoardCell[] enemyCells)
    {
        var allCells = new System.Collections.Generic.List<BoardCell>();
        allCells.AddRange(playerCells);
        allCells.AddRange(enemyCells);
        
        foreach (var cell in allCells)
        {
            if (!cell.HasCharacter()) continue;
            
            foreach (Transform child in cell.transform)
            {
                if (child.name.StartsWith("StatsCanvas_"))
                {
                    var display = child.GetComponent<CardStatsDisplay>();
                    if (display != null)
                        StartCoroutine(display.ConsolidateStats());
                }
            }
        }
        
        yield return new WaitForSeconds(1.2f);
    }

    BoardCell FindOppositeCell(BoardCell cell)
    {
        if (BoardManager.Instance == null) return null;
        int oppositeRow = cell.isPlayerSide ? 1 : 0;
        return BoardManager.Instance.GetCell(oppositeRow, cell.column);
    }

    IEnumerator ClashAnimation(Transform card1, Transform card2)
    {
        Vector3 card1Start = card1.position;
        Vector3 card2Start = card2.position;
        Quaternion card1StartRot = card1.rotation;
        Quaternion card2StartRot = card2.rotation;
        Quaternion flatRotation = Quaternion.Euler(90, 0, 0);
        
        Vector3 difference = card2Start - card1Start;
        Vector3 direction = difference.normalized;
        float distance = difference.magnitude;
        
        float rotateDuration = 0.2f;
        float elapsed = 0f;
        
        while (elapsed < rotateDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / rotateDuration;
            
            if (card1 != null) card1.rotation = Quaternion.Slerp(card1StartRot, flatRotation, t);
            if (card2 != null) card2.rotation = Quaternion.Slerp(card2StartRot, flatRotation, t);
            yield return null;
        }
        
        float windUpDuration = 0.3f;
        float windUpDistance = 0.5f;
        elapsed = 0f;
        
        Vector3 card1Back = card1Start - direction * windUpDistance;
        Vector3 card2Back = card2Start + direction * windUpDistance;
        
        while (elapsed < windUpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / windUpDuration;
            float ease = t * (2 - t);
            
            if (card1 != null) card1.position = Vector3.Lerp(card1Start, card1Back, ease);
            if (card2 != null) card2.position = Vector3.Lerp(card2Start, card2Back, ease);
            yield return null;
        }

        float strikeDuration = 0.15f;
        elapsed = 0f;
        
        float strikeDist = (distance / 2f) + 0.2f; 
        Vector3 card1Hit = card1Start + direction * strikeDist;
        Vector3 card2Hit = card2Start - direction * strikeDist;
        
        while (elapsed < strikeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / strikeDuration;
            float ease = t == 0 ? 0 : Mathf.Pow(2, 10 * (t - 1));
            
            if (card1 != null) card1.position = Vector3.Lerp(card1Back, card1Hit, ease);
            if (card2 != null) card2.position = Vector3.Lerp(card2Back, card2Hit, ease);
            yield return null;
        }

        StartCoroutine(ShakeCamera(0.2f, 0.1f));
        yield return new WaitForSeconds(0.1f);

        float recoilDuration = 0.25f;
        elapsed = 0f;
        
        while (elapsed < recoilDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / recoilDuration;
            float c1 = 1.70158f;
            float c3 = c1 + 1;
            float ease = 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
            
            if (card1 != null) card1.position = Vector3.Lerp(card1Hit, card1Start, ease);
            if (card2 != null) card2.position = Vector3.Lerp(card2Hit, card2Start, ease);
            yield return null;
        }
        
        if (card1 != null) card1.position = card1Start;
        if (card2 != null) card2.position = card2Start;
        
        elapsed = 0f;
        while (elapsed < rotateDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / rotateDuration;
            
            if (card1 != null) card1.rotation = Quaternion.Slerp(flatRotation, card1StartRot, t);
            if (card2 != null) card2.rotation = Quaternion.Slerp(flatRotation, card2StartRot, t);
            yield return null;
        }
        
        if (card1 != null) card1.rotation = card1StartRot;
        if (card2 != null) card2.rotation = card2StartRot;
    }

    IEnumerator ShakeCamera(float duration, float magnitude)
    {
        Transform camTrans = Camera.main.transform;
        Vector3 originalPos = camTrans.position;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            camTrans.position = originalPos + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        camTrans.position = originalPos;
    }

    IEnumerator ShakeAnimation(Transform card)
    {
        if (card == null) yield break;
        
        if (GameManager.Instance != null)
            GameManager.Instance.PlaySFX(GameManager.Instance.collisionSfx);
            
        Vector3 originalPos = card.position;
        float elapsed = 0f;
        
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float x = originalPos.x + Random.Range(-shakeIntensity, shakeIntensity);
            float z = originalPos.z + Random.Range(-shakeIntensity, shakeIntensity);
            card.position = new Vector3(x, originalPos.y, z);
            yield return null;
        }
        
        card.position = originalPos;
    }

    IEnumerator DestroyCardAnimation(Transform card)
    {
        if (card == null) yield break;
        
        Vector3 originalScale = card.localScale;
        Quaternion originalRot = card.rotation;
        Quaternion flatRot = Quaternion.Euler(90, 0, 0);
        float duration = 0.5f;
        float elapsed = 0f;
        
        foreach (Transform child in card)
        {
            if (child.name.StartsWith("StatsCanvas_"))
                child.gameObject.SetActive(false);
        }
        
        SpriteRenderer spriteRenderer = card.GetComponentInChildren<SpriteRenderer>();
        Color originalColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
        
        Vector3 originalPos = card.position;
        Vector3 targetPos = originalPos + Vector3.down * 0.5f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float easeT = 1f - Mathf.Pow(1f - t, 3f);
            
            if (card != null)
            {
                card.localScale = Vector3.Lerp(originalScale, Vector3.zero, easeT);
                card.rotation = Quaternion.Slerp(originalRot, flatRot, easeT);
                card.position = Vector3.Lerp(originalPos, targetPos, easeT);
            }
            
            if (spriteRenderer != null)
            {
                Color newColor = originalColor;
                newColor.a = 1f - easeT;
                spriteRenderer.color = newColor;
            }
            
            yield return null;
        }
        
        if (card != null)
            Destroy(card.gameObject);
    }

    public void ApplySpell(SpellCard spell, BoardCell targetCell)
    {
        if (spell == null || targetCell == null) return;
        
        switch (spell.spellType)
        {
            case SpellType.Damage:
                if (targetCell.HasCharacter() && targetCell.isPlayerSide)
                {
                    targetCell.spellBonusDamage += spell.power;
                    Transform card = GetCardVisual(targetCell);
                    if (card != null)
                        StartCoroutine(BuffAnimation(card));
                }
                break;
                
            case SpellType.Heal:
                if (targetCell.HasCharacter() && targetCell.isPlayerSide)
                {
                    targetCell.currentHealth += 2;
                    Transform card = GetCardVisual(targetCell);
                    if (card != null)
                        StartCoroutine(BuffAnimation(card));
                }
                break;
                
            case SpellType.Buff:
                if (targetCell.HasCharacter() && targetCell.isPlayerSide)
                    targetCell.spellBonusDamage += spell.power;
                break;
                
            case SpellType.SoulSiphon:
                if (targetCell.HasCharacter() && targetCell.isPlayerSide)
                {
                    BoardCell enemyCell = FindOppositeCell(targetCell);
                    
                    if (enemyCell != null && enemyCell.HasCharacter())
                    {
                        enemyCell.spellBonusDamage -= spell.power;
                        targetCell.spellBonusDamage += spell.power;
                        
                        Transform playerCard = GetCardVisual(targetCell);
                        Transform enemyCardVisual = GetCardVisual(enemyCell);
                        
                        if (playerCard != null)
                            StartCoroutine(BuffAnimation(playerCard));
                        if (enemyCardVisual != null)
                            StartCoroutine(ShakeAnimation(enemyCardVisual));
                    }
                    else
                    {
                        targetCell.spellBonusDamage += spell.power;
                    }
                }
                break;
        }
    }

    IEnumerator BuffAnimation(Transform card)
    {
        if (card == null) yield break;
        
        if (GameManager.Instance != null)
            GameManager.Instance.PlaySFX(GameManager.Instance.augmentSfx);
            
        Vector3 originalScale = card.localScale;
        float duration = 0.3f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float scale = 1f + 0.2f * Mathf.Sin(t * Mathf.PI);
            card.localScale = originalScale * scale;
            yield return null;
        }
        
        card.localScale = originalScale;
    }
}

