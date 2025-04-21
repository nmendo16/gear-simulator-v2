using System;
using System.Collections.Generic;
using UnityEngine;

public class PlanetarySystem : GearSystem
{
    [Header("Planetary Gear References")]
    [SerializeField]
    private PlanetarySystemElement sunGear;
    [SerializeField]
    private List<PlanetarySystemElement> planetaryGears;
    [SerializeField]
    private RotatingElement planetaryAxis;
    [SerializeField]
    private PlanetarySystemElement ringGear;


    [Header("Settings")]
    [SerializeField]
    [Range(4, 40)]
    private int sunGearCogs;
    [SerializeField]
    [Range(4, 40)]
    private int planetGearCogs;
    [SerializeField]
    private bool isRingGearLocked = false;
    [Header("UI Reference")]
    [SerializeField]
    private UIManager ui_Manager;

    public bool IsRingGearLocked { get { return isRingGearLocked; } }

    public PlanetarySystemElement SunGear { get { return this.sunGear; } }
    public List<PlanetarySystemElement> PlanetGear { get { return this.planetaryGears; } }
    public PlanetarySystemElement RingGear { get { return this.ringGear; } }
    public List<PlanetarySystemElement> GearsReference
    {
        get
        {
            return new List<PlanetarySystemElement>()
        {
            sunGear,
            planetaryGears[0],
            ringGear
        };
        }
    }
    // ---- Event System ----
    public static event Action<bool> IsMovedByInputEvent;
    private void Start()
    {
        GenerateSystem();
        LockRingGear(isRingGearLocked);
        ui_Manager.InitializeVisuals(this);
    }
    void OnEnable()
    {
        MovementButtons.IsMovedByButtonEvent += BeingMovedByButton;
    }
    void OnDisable()
    {
        MovementButtons.IsMovedByButtonEvent -= BeingMovedByButton;
    }
    public void GenerateSystem()
    {
        sunGear.GenerateGear(sunGearCogs);
        foreach (Gear gear in planetaryGears)
        {
            gear.GenerateGear(planetGearCogs);
        }
        ringGear.GenerateGear(sunGearCogs + (planetGearCogs * 2));
        planetaryAxis.Cogs = ringGear.Cogs;

        sunGear.ResetMaterial();
        ringGear.ResetMaterial();
        foreach (Gear gear in planetaryGears)
        {
            gear.ResetMaterial();
        }

        RefreshGearPositions();
    }
    public void RefreshGearPositions()
    {
        float combinedRadius = sunGear.GetTotalRadius() + planetaryGears[0].GetBodyRadius();
        float hypothenuseMagnitude = Mathf.Sqrt(2 * (combinedRadius * combinedRadius)) / 2;
        Vector3 firstQuadrantGear = new(0, combinedRadius, 0);
        Vector3 thirdQuadrantGear = new(-hypothenuseMagnitude, -hypothenuseMagnitude, 0);
        Vector3 fourthQuadrantGear = new(hypothenuseMagnitude, -hypothenuseMagnitude, 0);
        planetaryGears[0].transform.localPosition = firstQuadrantGear;
        planetaryGears[1].transform.localPosition = thirdQuadrantGear;
        planetaryGears[2].transform.localPosition = fourthQuadrantGear;
    }
    public void LockRingGear(bool lockRingGear)
    {
        if (lockRingGear)
        {
            isRingGearLocked = true;
            this.ringGear.LockRotation = true;
            this.planetaryGears[0].AddNeighbor(planetaryAxis);
            this.ringGear.RemoveNeighbor(this.planetaryGears[0]);
        }
        else
        {
            if (isRingGearLocked == true)
            {
                isRingGearLocked = false;
                this.ringGear.LockRotation = false;
                this.planetaryGears[0].RemoveNeighbor(planetaryAxis);
                this.ringGear.AddNeighbor(this.planetaryGears[0]);
            }
        }
    }
    private bool _beingMovedByButton = false;
    protected override void Update()
    {
        if (!_beingMovedByButton)
        {
            float xAxis = Input.GetAxis("Horizontal");
            IsMovedByInputEvent?.Invoke(xAxis != 0);
            SetSystemSpeed(xAxis, false);
        }
    }
    public override void SetSystemSpeed(float normalizedValue, bool byButton)
    {
        if (!isRingGearLocked || this.drivingGear != ringGear)
        {
            base.SetSystemSpeed(normalizedValue, byButton);
        }
    }
    private void BeingMovedByButton(bool value)
    {
        _beingMovedByButton = value;
    }

    /***
    <summary>
        Changes the material of a gear to symbolize selection. 
    </summary>
    ***/
    public void VisuallySelectPlanetGroup(Material selectMaterial, bool deselect)
    {
        foreach (Gear planet in planetaryGears)
        {
            if (deselect)
            {
                planet.ResetMaterial();
            }
            else
            {
                planet.SetMaterial(selectMaterial);
            }
        }
    }
    public void VisuallySelectPlanetGroup(string layerName)
    {
        foreach (Gear planet in planetaryGears)
        {
            planet.SetLayerAll(layerName);
        }

    }
    public void RebuildSystem(PlanetarySystemElement changedGear, int newCogs)
    {
        if (changedGear.gearType == GearTypePlSystem.SunGear)
        {
            sunGear.DeleteGeneratedModel();
            this.sunGearCogs = newCogs;
            sunGear.GenerateGear(newCogs);
            sunGear.ResetMaterial();
        }
        else if (changedGear.gearType == GearTypePlSystem.PlanetaryGear)
        {
            foreach (var gear in this.planetaryGears)
            {
                gear.DeleteGeneratedModel();
                this.planetGearCogs = newCogs;
                gear.GenerateGear(newCogs);
                gear.ResetMaterial();
            }
        }
        ringGear.DeleteGeneratedModel();
        ringGear.GenerateGear(sunGearCogs + (planetGearCogs * 2));
        planetaryAxis.Cogs = ringGear.Cogs;
        ringGear.ResetMaterial();
        RefreshGearPositions();
        this.previousAxis = 0;

    }
    public RotatingElement GetPlanetaryAxis()
    {
        return planetaryAxis;
    }
}
