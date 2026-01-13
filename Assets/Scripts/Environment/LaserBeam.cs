using UnityEngine;
using System.Collections;

public class LaserBeam : MonoBehaviour
{
    [Header("Laser Settings")]
    public Transform startPoint;
    public Transform targetPoint; 
    public float travelTimeSeconds = 780f;  
    public float extendLengthPercent = 0.12f;  
    public float finalLengthPercent = 0.06f;   
    public float laserWidth = 3.0f;
    public Color laserOuterColor = new Color(1f, 0.1f, 0.1f);
    
    [Header("Animation")]
    public float extendDuration = 1.5f;   
    public float shrinkDuration = 0.8f;   
    
    [Header("Visual Settings")]
    public float flickerSpeed = 3f;
    public float flickerIntensity = 0.15f;
    
    
    private LineRenderer coreRenderer;
    private LineRenderer innerGlow;
    private LineRenderer outerGlow;
    
    
    private enum LaserPhase { Extending, Shrinking, Traveling }
    private LaserPhase currentPhase = LaserPhase.Extending;
    
    private float phaseProgress = 0f;
    private float currentProgress = 0f;
    private bool isFiring = false;
    private bool hasReachedTarget = false;
    
    private float extendLength;  
    private float finalLength;   
    private Vector3 laserStartPos;
    private Vector3 laserEndPos;
    private float totalDistance;
    
    
    private ParticleSystem sparkParticles;
    private GameObject impactGlow;
    
    void Start()
    {
        SetupMultiLayerLaser();
        SetupParticles();
    }
    
    void SetupMultiLayerLaser()
    {
        
        GameObject outerObj = new GameObject("LaserOuterGlow");
        outerObj.transform.SetParent(transform);
        outerGlow = outerObj.AddComponent<LineRenderer>();
        ConfigureLineRenderer(outerGlow, laserWidth * 2.5f, 
            new Color(laserOuterColor.r, laserOuterColor.g, laserOuterColor.b, 0.25f));
        
        
        GameObject innerObj = new GameObject("LaserInnerGlow");
        innerObj.transform.SetParent(transform);
        innerGlow = innerObj.AddComponent<LineRenderer>();
        ConfigureLineRenderer(innerGlow, laserWidth * 1.2f, 
            new Color(1f, 0.4f, 0.3f, 0.6f));
        
        
        GameObject coreObj = new GameObject("LaserCore");
        coreObj.transform.SetParent(transform);
        coreRenderer = coreObj.AddComponent<LineRenderer>();
        ConfigureLineRenderer(coreRenderer, laserWidth * 0.4f, 
            new Color(1f, 0.95f, 0.9f, 1f));
    }
    
    void ConfigureLineRenderer(LineRenderer lr, float width, Color color)
    {
        lr.positionCount = 2;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.useWorldSpace = true;
        
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = color;
        lr.material = mat;
        
        lr.numCapVertices = 8;
        lr.enabled = false;
    }
    
    void SetupParticles()
    {
        GameObject sparkObj = new GameObject("SparkParticles");
        sparkObj.transform.SetParent(transform);
        sparkParticles = sparkObj.AddComponent<ParticleSystem>();
        
        var main = sparkParticles.main;
        main.startColor = new Color(1f, 0.8f, 0.6f, 1f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        main.startSpeed = 1f;
        main.startLifetime = 1f;
        main.maxParticles = 20;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        var emission = sparkParticles.emission;
        emission.rateOverTime = 10f;
        
        var shape = sparkParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 2f;
        
        var renderer = sparkObj.GetComponent<ParticleSystemRenderer>();
        Material sparkMat = new Material(Shader.Find("Particles/Standard Unlit"));
        sparkMat.color = new Color(1f, 0.5f, 0.3f);
        renderer.material = sparkMat;
        
        sparkParticles.Stop();
    }
    
    public void FireLaser()
    {
        if (startPoint == null || targetPoint == null)
        {
            Debug.LogWarning("LaserBeam: Start or Target point not set!");
            return;
        }
        
        laserStartPos = startPoint.position;
        laserEndPos = targetPoint.position;
        totalDistance = Vector3.Distance(laserStartPos, laserEndPos);
        extendLength = totalDistance * extendLengthPercent;  
        finalLength = totalDistance * finalLengthPercent;    
        
        phaseProgress = 0f;
        currentProgress = 0f;
        currentPhase = LaserPhase.Extending;
        isFiring = true;
        hasReachedTarget = false;
        
        coreRenderer.enabled = true;
        innerGlow.enabled = true;
        outerGlow.enabled = true;
        
        if (sparkParticles != null) sparkParticles.Play();
    }
    
    void Update()
    {
        if (!isFiring || hasReachedTarget) return;
        
        switch (currentPhase)
        {
            case LaserPhase.Extending:
                UpdateExtendPhase();
                break;
            case LaserPhase.Shrinking:
                UpdateShrinkPhase();
                break;
            case LaserPhase.Traveling:
                UpdateTravelPhase();
                break;
        }
    }
    
    void UpdateExtendPhase()
    {
        
        phaseProgress += Time.deltaTime / extendDuration;
        phaseProgress = Mathf.Clamp01(phaseProgress);
        
        float easedProgress = Mathf.SmoothStep(0f, 1f, phaseProgress);
        
        Vector3 direction = (laserEndPos - laserStartPos).normalized;
        
        
        Vector3 laserBack = laserStartPos;
        Vector3 laserFront = laserStartPos + direction * (extendLength * easedProgress);
        
        UpdateLaserVisuals(laserBack, laserFront);
        
        if (phaseProgress >= 1f)
        {
            
            currentPhase = LaserPhase.Traveling;
            currentProgress = 0f; 
            
            
            if (GameTimer.Instance != null)
            {
                GameTimer.Instance.StartTimer();
                Debug.Log("Laser entered Travel Phase (Stage 2) - Starting 10 minute countdown.");
            }
        }
    }
    
    void UpdateShrinkPhase()
    {
        
        phaseProgress += Time.deltaTime / shrinkDuration;
        phaseProgress = Mathf.Clamp01(phaseProgress);
        
        float easedProgress = Mathf.SmoothStep(0f, 1f, phaseProgress);
        
        Vector3 direction = (laserEndPos - laserStartPos).normalized;
        
        
        float currentLength = Mathf.Lerp(extendLength, finalLength, easedProgress);
        Vector3 laserFront = laserStartPos + direction * extendLength;  
        Vector3 laserBack = laserFront - direction * currentLength;     
        
        UpdateLaserVisuals(laserBack, laserFront);
        
        if (phaseProgress >= 1f)
        {
            currentPhase = LaserPhase.Traveling;
            currentProgress = extendLengthPercent;  
        }
    }
    
    void UpdateTravelPhase()
    {
        
        float t = 0f;
        if (GameTimer.Instance != null && GameTimer.Instance.totalTime > 0)
        {
            t = 1f - (GameTimer.Instance.GetRemainingTime() / GameTimer.Instance.totalTime);
        }
        else
        {
            currentProgress += Time.deltaTime / travelTimeSeconds;
            t = Mathf.Clamp01(currentProgress);
        }
        
        Vector3 direction = (laserEndPos - laserStartPos).normalized;
        
        float travelProgress = Mathf.Lerp(extendLengthPercent, 1f, t);
        float currentDistance = travelProgress * totalDistance;
        
        Vector3 laserFront = laserStartPos + direction * currentDistance;
        Vector3 laserBack = laserFront - direction * extendLength; 
        
        if (currentDistance < extendLength)
        {
            laserBack = laserStartPos;
        }
        
        UpdateLaserVisuals(laserBack, laserFront);
        
        if (t >= 1f)
        {
            hasReachedTarget = true;
            OnLaserHitTarget();
        }
    }
    
    void UpdateLaserVisuals(Vector3 back, Vector3 front)
    {
        float flicker = 1f + Mathf.Sin(Time.time * flickerSpeed) * flickerIntensity;
        
        Vector3[] positions = new Vector3[] { back, front };
        
        coreRenderer.SetPositions(positions);
        innerGlow.SetPositions(positions);
        outerGlow.SetPositions(positions);
        
        coreRenderer.startWidth = laserWidth * 0.4f * flicker;
        coreRenderer.endWidth = laserWidth * 0.4f * flicker;
        innerGlow.startWidth = laserWidth * 1.2f * flicker;
        innerGlow.endWidth = laserWidth * 1.2f * flicker;
        outerGlow.startWidth = laserWidth * 2.5f * flicker;
        outerGlow.endWidth = laserWidth * 2.5f * flicker;
        
        if (sparkParticles != null)
        {
            sparkParticles.transform.position = front;
        }
    }
    
    void OnLaserHitTarget()
    {
        Debug.Log("Laser hit Earth!");
        CreateImpactEffect();
    }
    
    void CreateImpactEffect()
    {
        impactGlow = new GameObject("ImpactGlow");
        impactGlow.transform.position = laserEndPos;
        
        for (int i = 0; i < 3; i++)
        {
            GameObject glowSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            glowSphere.transform.SetParent(impactGlow.transform);
            glowSphere.transform.localPosition = Vector3.zero;
            glowSphere.transform.localScale = Vector3.one * (3f + i * 3f);
            
            Destroy(glowSphere.GetComponent<Collider>());
            
            Renderer rend = glowSphere.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Sprites/Default"));
            float alpha = 0.5f - i * 0.12f;
            mat.color = new Color(1f, 0.3f - i * 0.08f, 0.2f, alpha);
            rend.material = mat;
        }
        
        StartCoroutine(AnimateImpact());
    }
    
    System.Collections.IEnumerator AnimateImpact()
    {
        float duration = 3f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            float scale = Mathf.Lerp(1f, 10f, Mathf.Pow(t, 0.5f));
            impactGlow.transform.localScale = Vector3.one * scale;
            
            float fade = 1f - t;
            if (coreRenderer != null)
                coreRenderer.material.color = new Color(1f, 0.95f, 0.9f, fade);
            if (innerGlow != null)
                innerGlow.material.color = new Color(1f, 0.4f, 0.3f, fade * 0.6f);
            if (outerGlow != null)
                outerGlow.material.color = new Color(laserOuterColor.r, laserOuterColor.g, laserOuterColor.b, fade * 0.25f);
            
            yield return null;
        }
        
        coreRenderer.enabled = false;
        innerGlow.enabled = false;
        outerGlow.enabled = false;
        
        if (sparkParticles != null) sparkParticles.Stop();
        
        Destroy(impactGlow);
        isFiring = false;
    }
    
    public void ImpactImmediately()
    {
        if (targetPoint == null) return;
        
        isFiring = true; 
        hasReachedTarget = true;
        
        
        Vector3 direction = (targetPoint.position - startPoint.position).normalized;
        Vector3 laserFront = targetPoint.position;
        Vector3 laserBack = laserFront - direction * extendLength;
        
        UpdateLaserVisuals(laserBack, laserFront);
        
        
        ImpactEarth();
    }

    public void ImpactEarth()
    {
        hasReachedTarget = true;
        isFiring = false;
        
        Debug.Log("DÜNYA YOK EDİLDİ!");
        
        if (targetPoint != null)
        {
            GameObject explosion = new GameObject("EarthExplosion");
            explosion.transform.position = targetPoint.position;
            
            for (int i = 0; i < 5; i++)
            {
                GameObject blast = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                blast.transform.SetParent(explosion.transform);
                blast.transform.localPosition = Vector3.zero;
                blast.transform.localScale = Vector3.one * (3f + i * 2f);
                
                Destroy(blast.GetComponent<Collider>());
                
                Renderer rend = blast.GetComponent<Renderer>();
                Material mat = new Material(Shader.Find("Sprites/Default"));
                float alpha = 0.8f - i * 0.15f;
                mat.color = new Color(1f, 0.2f + i * 0.1f, 0f, alpha);
                rend.material = mat;
            }
            
            StartCoroutine(AnimateExplosion(explosion));
        }
        
        coreRenderer.enabled = false;
        innerGlow.enabled = false;
        outerGlow.enabled = false;
        if (sparkParticles != null) sparkParticles.Stop();
    }
    
    System.Collections.IEnumerator AnimateExplosion(GameObject explosion)
    {
        float duration = 3f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            float scale = Mathf.Lerp(1f, 25f, Mathf.Pow(t, 0.3f));
            explosion.transform.localScale = Vector3.one * scale;
            
            yield return null;
        }
        
        yield return new WaitForSeconds(2f);
        Destroy(explosion);
    }
    
    public void DisappearLaser()
    {
        isFiring = false;
        hasReachedTarget = true;
        
        Debug.Log("LAZER KAYBOLDU! Dünya kurtuldu!");
        
        StartCoroutine(FadeLaserOut());
    }
    
    System.Collections.IEnumerator FadeLaserOut()
    {
        float duration = 2f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float fade = 1f - t;
            
            if (coreRenderer != null)
            {
                coreRenderer.startWidth = laserWidth * 0.4f * fade;
                coreRenderer.endWidth = laserWidth * 0.4f * fade;
            }
            if (innerGlow != null)
            {
                innerGlow.startWidth = laserWidth * 1.2f * fade;
                innerGlow.endWidth = laserWidth * 1.2f * fade;
            }
            if (outerGlow != null)
            {
                outerGlow.startWidth = laserWidth * 2.5f * fade;
                outerGlow.endWidth = laserWidth * 2.5f * fade;
            }
            
            yield return null;
        }
        
        coreRenderer.enabled = false;
        innerGlow.enabled = false;
        outerGlow.enabled = false;
        if (sparkParticles != null) sparkParticles.Stop();
    }
}
