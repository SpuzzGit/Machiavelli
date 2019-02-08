﻿using UnityEngine;
using UnityEngine.EventSystems;
using System.IO;
using UnityEngine.UI;
using System.Collections.Generic;

public class HexMapEditor : MonoBehaviour {

    enum OptionalToggle
    {
        Ignore, Yes, No
    }

    [SerializeField] Dropdown playerUnits;
    [SerializeField] Dropdown players;
    [SerializeField] Dropdown cityStateUnits;
    [SerializeField] Dropdown cityStates;

    public HexGrid hexGrid;
    public GameController gameController;
	public Material terrainMaterial;

	int activeElevation;
	int activeWaterLevel;

	int activeUrbanLevel, activeFarmLevel, activePlantLevel, activeSpecialIndex;

	int activeTerrainTypeIndex = -1;

	int brushSize;

	bool applyElevation = false;
	bool applyWaterLevel = false;

	bool applyUrbanLevel, applyFarmLevel, applyPlantLevel, applySpecialIndex;


	OptionalToggle riverMode, roadMode, walledMode;

	bool isDrag;
	HexDirection dragDirection;
	HexCell previousCell;

	public void SetTerrainTypeIndex (int index) {
		activeTerrainTypeIndex = index;
	}

	public void SetApplyElevation (bool toggle) {
		applyElevation = toggle;
	}

	public void SetElevation (float elevation) {
		activeElevation = (int)elevation;
	}

	public void SetApplyWaterLevel (bool toggle) {
		applyWaterLevel = toggle;
	}

	public void SetWaterLevel (float level) {
		activeWaterLevel = (int)level;
	}

	public void SetApplyUrbanLevel (bool toggle) {
		applyUrbanLevel = toggle;
	}

	public void SetUrbanLevel (float level) {
		activeUrbanLevel = (int)level;
	}

	public void SetApplyFarmLevel (bool toggle) {
		applyFarmLevel = toggle;
	}

	public void SetFarmLevel (float level) {
		activeFarmLevel = (int)level;
	}

	public void SetApplyPlantLevel (bool toggle) {
		applyPlantLevel = toggle;
	}

	public void SetPlantLevel (float level) {
		activePlantLevel = (int)level;
	}

	public void SetApplySpecialIndex (bool toggle) {
		applySpecialIndex = toggle;
	}

	public void SetSpecialIndex (float index) {
		activeSpecialIndex = (int)index;
	}

	public void SetBrushSize (float size) {
		brushSize = (int)size;
	}

	public void SetRiverMode (int mode) {
		riverMode = (OptionalToggle)mode;
	}

	public void SetRoadMode (int mode) {
		roadMode = (OptionalToggle)mode;
	}

	public void SetWalledMode (int mode) {
		walledMode = (OptionalToggle)mode;
	}

	public void SetEditMode (bool toggle) {
		enabled = toggle;
	}

    void Awake()
    {
        terrainMaterial.DisableKeyword("GRID_ON");
        Shader.EnableKeyword("HEX_MAP_EDIT_MODE");
        SetEditMode(true);

        string myPath = "Assets/Resources/";
        DirectoryInfo dir = new DirectoryInfo(myPath);
        FileInfo[] info = dir.GetFiles("*.prefab");
        List<string> files = new List<string>();
        foreach (FileInfo f in info)
        {
            files.Add(f.Name.Split('.')[0]);
        }
        playerUnits.ClearOptions();
        playerUnits.AddOptions(files);
        playerUnits.value = 0;
        cityStateUnits.ClearOptions();
        cityStateUnits.AddOptions(files);
        cityStateUnits.value = 0;

        cityStates.ClearOptions();
    }


	void Update () {
		if (!EventSystem.current.IsPointerOverGameObject()) {
			if (Input.GetMouseButton(0)) {
				HandleInput();
				return;
			}
			if (Input.GetKeyDown(KeyCode.U)) {
				if (Input.GetKey(KeyCode.LeftShift)) {
					DestroyUnit();
				}
				else {
					CreateUnit();
				}
				return;
			}
            if (Input.GetKeyDown(KeyCode.H))
            {
                CreateCityStateUnit();
                return;
            }
        }

        if(cityStates.options.Count != gameController.CityStateCount())
        {
            cityStates.ClearOptions();
            cityStates.AddOptions(gameController.CityStateNames());
        }

        if (players.options.Count != gameController.PlayerCount())
        {
            players.ClearOptions();
            players.AddOptions(gameController.PlayerNames());
        }

        previousCell = null;
	}
    public void ShowGrid(bool visible)
    {
        if (visible)
        {
            terrainMaterial.EnableKeyword("GRID_ON");
        }
        else
        {
            terrainMaterial.DisableKeyword("GRID_ON");
        }
    }

    HexCell GetCellUnderCursor () {
		return
			hexGrid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
	}

	void CreateUnit () {
		HexCell cell = GetCellUnderCursor();
		if (players.options.Count > 0 && cell && cell.CanUnitMoveToCell(HexUnit.UnitType.AGENT)) {
            string name = playerUnits.options[playerUnits.value].text;
            string playerName = players.options[players.value].text;
            Player player;
            if(playerName == "Human Player")
            {
                player = gameController.HumanPlayer;
            }
            else
            {
                int playerID = System.Convert.ToInt32(players.options[players.value].text);
                player = gameController.GetPlayer(playerID);
            }
            
            HexUnit hexUnit = Instantiate(Resources.Load(name) as GameObject).GetComponent<HexUnit>();
            hexUnit.UnitPrefabName = name;
            hexGrid.AddUnit(hexUnit, cell, Random.Range(0f, 360f));

            gameController.CreateAgent(hexUnit, player);


        }
    }

    public void CreateAIPlayer()
    {
        gameController.CreateAIPlayer();
    }
	void DestroyUnit () {
		HexCell cell = GetCellUnderCursor();
		if (cell && cell.GetTopUnit()) {
			hexGrid.RemoveUnit(cell.GetTopUnit());
		}
	}

    void CreateCityStateUnit()
    {
        HexCell cell = GetCellUnderCursor();
        if (cityStates.options.Count > 0 && cell && cell.CanUnitMoveToCell(HexUnit.UnitType.COMBAT))
        {
            string name = cityStateUnits.options[cityStateUnits.value].text;
            int cityStateID = System.Convert.ToInt32(cityStates.options[cityStates.value].text);
            HexUnit hexUnit = Instantiate(Resources.Load(name) as GameObject).GetComponent<HexUnit>();
            hexUnit.UnitPrefabName = name;
            hexGrid.AddUnit(hexUnit, cell, Random.Range(0f, 360f));
            gameController.CreateCityStateUnit(hexUnit, cityStateID);
        }
    }

    public void SetCityStatePlayer()
    {
        int cityStateID = System.Convert.ToInt32(cityStates.options[cityStates.value].text);
        string playerName = players.options[players.value].text;
        Player player;
        if (playerName == "Human Player")
        {
            player = gameController.HumanPlayer;
        }
        else
        {
            int playerID = System.Convert.ToInt32(players.options[players.value].text);
            player = gameController.GetPlayer(playerID);
        }


        if(player)
        {
            gameController.SetCityStatePlayer(player, cityStateID);
        }
    }

    void HandleInput () {
		HexCell currentCell = GetCellUnderCursor();
		if (currentCell) {
			if (previousCell && previousCell != currentCell) {
				ValidateDrag(currentCell);
			}
			else {
				isDrag = false;
			}
			EditCells(currentCell);
			previousCell = currentCell;
		}
		else {
			previousCell = null;
		}
	}

	void ValidateDrag (HexCell currentCell) {
		for (
			dragDirection = HexDirection.NE;
			dragDirection <= HexDirection.NW;
			dragDirection++
		) {
			if (previousCell.GetNeighbor(dragDirection) == currentCell) {
				isDrag = true;
				return;
			}
		}
		isDrag = false;
	}

	void EditCells (HexCell center) {
		int centerX = center.coordinates.X;
		int centerZ = center.coordinates.Z;

		for (int r = 0, z = centerZ - brushSize; z <= centerZ; z++, r++) {
			for (int x = centerX - r; x <= centerX + brushSize; x++) {
				EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
			}
		}
		for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++) {
			for (int x = centerX - brushSize; x <= centerX + r; x++) {
				EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
			}
		}
	}

	void EditCell (HexCell cell) {
		if (cell) {
			if (activeTerrainTypeIndex >= 0) {
				cell.TerrainTypeIndex = activeTerrainTypeIndex;
			}
			if (applyElevation) {
				cell.Elevation = activeElevation;
			}
			if (applyWaterLevel) {
				cell.WaterLevel = activeWaterLevel;
			}
			if (applySpecialIndex) {
                if(activeSpecialIndex == 2)
                {
                    hexGrid.RemoveCity(cell);
                    hexGrid.AddCity(cell);
                }
                else
                {
                    hexGrid.RemoveCity(cell);
                }
                if (activeSpecialIndex == 3)
                {
                    string playerName = players.options[players.value].text;
                    Player player;
                    if (playerName == "Human Player")
                    {
                        player = gameController.HumanPlayer;
                    }
                    else
                    {
                        int playerID = System.Convert.ToInt32(players.options[players.value].text);
                        player = gameController.GetPlayer(playerID);
                    }
                    hexGrid.RemoveOperationCentre(cell);
                    hexGrid.AddOperationCentre(cell, player);
                }
                else
                {
                    hexGrid.RemoveOperationCentre(cell);
                }

                cell.SpecialIndex = activeSpecialIndex;
			}
			if (applyUrbanLevel) {
				cell.UrbanLevel = activeUrbanLevel;
			}
			if (applyFarmLevel) {
				cell.FarmLevel = activeFarmLevel;
			}
			if (applyPlantLevel) {
				cell.PlantLevel = activePlantLevel;
			}
			if (riverMode == OptionalToggle.No) {
				cell.RemoveRiver();
			}
			if (roadMode == OptionalToggle.No) {
				cell.RemoveRoads();
			}
			if (walledMode != OptionalToggle.Ignore) {
				cell.Walled = walledMode == OptionalToggle.Yes;
			}
			if (isDrag) {
				HexCell otherCell = cell.GetNeighbor(dragDirection.Opposite());
				if (otherCell) {
					if (riverMode == OptionalToggle.Yes) {
						otherCell.SetOutgoingRiver(dragDirection);
					}
					if (roadMode == OptionalToggle.Yes) {
						otherCell.AddRoad(dragDirection);
					}
				}
			}
		}
	}
}