using UnityEngine;

public class SpaceEnvironment : MonoBehaviour
{
    public static SpaceEnvironment Instance;
    
    [Header("Settings")]
    public int starCount = 800;
    
    [Header("Earth Settings")]
    public float earthSize = 89f;
    public Vector3 earthPosition = new Vector3(-69.6f, 45.5f, 220f);
    public Texture2D earthTexture;  
    
    [Header("Laser Settings")]
    public float laserSpeed = 0.2f;
    public float laserDelay = 1f;
    public Vector3 laserSourcePosition = new Vector3(45f, 19.7f, 100f);  
    public Color laserColor = new Color(1f, 0.1f, 0.1f);
    
    private GameObject starsParent;
    private GameObject earth;
    private GameObject laserSource;
    private LaserBeam laserBeam;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SetupCamera();
        SetupLighting();
        CreateStarField();
        CreateEarth();
        CreateLaserSource();
    }
    
    void CreateEarth()
    {
        earth = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        earth.name = "Earth";
        earth.transform.SetParent(transform);
        earth.transform.position = earthPosition;
        earth.transform.localScale = Vector3.one * earthSize;
        
        
        Destroy(earth.GetComponent<Collider>());
        
        Renderer rend = earth.GetComponent<Renderer>();
        
        
        Material earthMat = Resources.Load<Material>("EarthMaterial");
        if (earthMat != null)
        {
            rend.material = earthMat;
        }
        else
        {
            
            Material fallback = new Material(Shader.Find("Unlit/Color"));
            fallback.color = new Color(0.2f, 0.4f, 0.8f);
            rend.material = fallback;
        }
        
        
    }
    
    Texture2D CreateEarthTexture(int size)
    {
        Texture2D tex = new Texture2D(size, size);
        
        
        Color oceanDeep = new Color(0.05f, 0.15f, 0.4f);
        Color oceanShallow = new Color(0.1f, 0.3f, 0.6f);
        Color landGreen = new Color(0.2f, 0.5f, 0.2f);
        Color landBrown = new Color(0.4f, 0.35f, 0.2f);
        Color ice = new Color(0.9f, 0.95f, 1f);
        Color desert = new Color(0.8f, 0.7f, 0.4f);
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (float)x / size;
                float ny = (float)y / size;
                
                
                float noise1 = Mathf.PerlinNoise(nx * 4f, ny * 4f);
                float noise2 = Mathf.PerlinNoise(nx * 8f + 100f, ny * 8f + 100f) * 0.5f;
                float noise3 = Mathf.PerlinNoise(nx * 16f + 200f, ny * 16f + 200f) * 0.25f;
                float continentNoise = noise1 + noise2 + noise3;
                
                Color pixelColor;
                
                
                float latitude = Mathf.Abs(ny - 0.5f) * 2f;
                if (latitude > 0.85f)
                {
                    pixelColor = ice;
                }
                else if (continentNoise > 0.55f) 
                {
                    if (latitude > 0.6f)
                        pixelColor = Color.Lerp(landGreen, ice, (latitude - 0.6f) * 4f);
                    else if (continentNoise > 0.75f)
                        pixelColor = desert;
                    else if (continentNoise > 0.65f)
                        pixelColor = landBrown;
                    else
                        pixelColor = landGreen;
                }
                else 
                {
                    pixelColor = Color.Lerp(oceanDeep, oceanShallow, continentNoise * 2f);
                }
                
                tex.SetPixel(x, y, pixelColor);
            }
        }
        
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        return tex;
    }
    
    void CreateAtmosphere()
    {
        GameObject atmosphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        atmosphere.name = "Atmosphere";
        atmosphere.transform.SetParent(earth.transform);
        atmosphere.transform.localPosition = Vector3.zero;
        atmosphere.transform.localScale = Vector3.one * 1.08f; 
        
        Destroy(atmosphere.GetComponent<Collider>());
        
        Renderer rend = atmosphere.GetComponent<Renderer>();
        Material atmoMat = new Material(Shader.Find("Unlit/Color"));
        atmoMat.color = new Color(0.4f, 0.6f, 1f, 0.15f); 
        rend.material = atmoMat;
    }
    
    void CreateLaserSource()
    {
        
        laserSource = new GameObject("LaserSource");
        laserSource.transform.SetParent(transform);
        laserSource.transform.position = laserSourcePosition; 
        
        
        GameObject sourceIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sourceIndicator.name = "SourceGlow";
        sourceIndicator.transform.SetParent(laserSource.transform);
        sourceIndicator.transform.localPosition = Vector3.zero;
        sourceIndicator.transform.localScale = Vector3.one * 1f;
        
        Destroy(sourceIndicator.GetComponent<Collider>());
        
        Renderer rend = sourceIndicator.GetComponent<Renderer>();
        Material glowMat = new Material(Shader.Find("Unlit/Color"));
        glowMat.color = laserColor * 1.5f;
        rend.material = glowMat;
        
        
        laserBeam = laserSource.AddComponent<LaserBeam>();
        laserBeam.startPoint = laserSource.transform;
        laserBeam.targetPoint = earth.transform;
        laserBeam.travelTimeSeconds = 780f;  
        laserBeam.laserOuterColor = laserColor;
    }
    
    void FireLaser()
    {
        if (laserBeam != null)
        {
            laserBeam.FireLaser();
        }
    }
    
    
    public void TriggerLaser()
    {
        FireLaser();
    }

    void SetupCamera()
    {
        Camera.main.clearFlags = CameraClearFlags.SolidColor;
        Camera.main.backgroundColor = new Color(0.005f, 0.005f, 0.015f);
    }

    void SetupLighting()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.25f, 0.25f, 0.35f);
        RenderSettings.fog = false;
        
        Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (var light in lights)
        {
            if (light.type == LightType.Directional)
            {
                light.color = new Color(0.95f, 0.95f, 1f);
                light.intensity = 1.2f;
            }
        }
    }

    void CreateStarField()
    {
        starsParent = new GameObject("Stars");
        starsParent.transform.SetParent(transform);
        
        for (int i = 0; i < starCount; i++)
        {
            GameObject star = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            star.name = "Star";
            star.transform.SetParent(starsParent.transform);
            
            Destroy(star.GetComponent<Collider>());
            
            float x = Random.Range(-120f, 120f);
            float y = Random.Range(-50f, 80f);
            float z = Random.Range(40f, 200f);
            
            star.transform.position = new Vector3(x, y, z);
            
            float sizeRandom = Random.value;
            float size;
            if (sizeRandom < 0.7f)
                size = Random.Range(0.03f, 0.08f);
            else if (sizeRandom < 0.9f)
                size = Random.Range(0.08f, 0.15f);
            else
                size = Random.Range(0.15f, 0.25f);
            
            star.transform.localScale = Vector3.one * size;
            
            Renderer rend = star.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Unlit/Color"));
            
            float colorVariation = Random.value;
            Color starColor;
            
            if (colorVariation < 0.6f)
                starColor = Color.white;
            else if (colorVariation < 0.75f)
                starColor = new Color(1f, 0.95f, 0.85f);
            else if (colorVariation < 0.88f)
                starColor = new Color(0.85f, 0.9f, 1f);
            else
                starColor = new Color(1f, 0.85f, 0.85f);
            
            mat.color = starColor;
            rend.material = mat;
        }
    }
}
