﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HexGameUI : MonoBehaviour {

    public HexGrid grid;

    HexCell currentCell;

    HexUnit selectedUnit;

    [SerializeField] GameController gameController;
    [SerializeField] HUD HUD;

    private bool editMode;

    private bool abilitySelection = false;
    private int abilityIndex;
    private List<HexCell> abilityTargetOptions;
    public void SetEditMode(bool toggle) {
        editMode = toggle;
        enabled = !toggle;
		grid.ShowUI(!toggle);
		grid.ClearPath();
        grid.EditMode = toggle;

        if (toggle) {
			Shader.EnableKeyword("HEX_MAP_EDIT_MODE");
		}
		else {
			Shader.DisableKeyword("HEX_MAP_EDIT_MODE");
		}
	}

	void Update () {
        if (abilitySelection == true)
        {
            DoAbilityInput();
        }
        else
        {
            DoSelectionInput();
        }


        if (grid.EditMode != editMode)
        {
            SetEditMode(grid.EditMode);
        }



    }

    private void DoSelectionInput()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButtonDown(0))
            {
                DoSelection();
            }
            else if (selectedUnit)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    DoMove();
                }
                else
                {
                    DoPathfinding();
                }
            }
        }
    }

    private void DoAbilityInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            FinishAbilitySelection();
        }
        else if(Input.GetKeyDown(KeyCode.Escape))
        {
            grid.ClearPath();
            abilitySelection = false;
            abilityTargetOptions.Clear();
            HUD.UpdateUI();
        }
    }

    public void ToggleEditMode()
    {
        SetEditMode(!editMode);
    }

	void DoSelection () {
		grid.ClearPath();
		UpdateCurrentCell();
		if (currentCell) {
            if(currentCell.GetTopUnit() && currentCell.GetTopUnit().Controllable)
            {
                selectedUnit = currentCell.GetTopUnit();
                HUD.Unit = selectedUnit.GetComponent<Unit>();
            }
            else if (currentCell.City)
            {
                selectedUnit = null;
                HUD.City  = currentCell.City;
            }
            else if(currentCell.OpCentre)
            {
                selectedUnit = null;
                HUD.OpCentre = currentCell.OpCentre;
            }
        }
	}

    public void SelectOpCentre(OperationCentre opCentre)
    {
        grid.ClearPath();
        selectedUnit = null;
        HUD.OpCentre = opCentre;
    }

    public void SelectCity(City city)
    {
        grid.ClearPath();
        selectedUnit = null;
        HUD.City = city;
    }

    public void SelectUnit(Unit unit)
    {
        grid.ClearPath();
        currentCell = unit.HexUnit.Location;
        selectedUnit = unit.HexUnit;
    }

    void DoPathfinding () {
		if (UpdateCurrentCell()) {
			if (currentCell && selectedUnit.IsValidDestination(currentCell)) {
				grid.FindPath(selectedUnit.Location, currentCell, selectedUnit);
			}
			else {
				grid.ClearPath();
			}
		}
	}

	void DoMove () {
		if (grid.HasPath) {
			selectedUnit.GetComponent<Unit>().SetPath(grid.GetPath());
			grid.ClearPath();
            HUD.UpdateUI();
		}
	}
    public void DoAbilitySelection(List<HexCell> cellOptions, int index)
    {
        abilityTargetOptions = cellOptions;
        abilitySelection = true;
        abilityIndex = index;
        grid.ClearPath();
        grid.HighlightCells(abilityTargetOptions);
       
    }

    void FinishAbilitySelection()
    {
        HexCell target = grid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
        if (selectedUnit && target && abilityTargetOptions.Contains(target))
        {
            selectedUnit.GetComponent<Unit>().UseAbility(abilityIndex, target);
        }
        grid.ClearHighlightedCells(abilityTargetOptions);
        abilitySelection = false;
        abilityTargetOptions.Clear();
        HUD.UpdateUI();
    }

    bool UpdateCurrentCell () {
		HexCell cell =
			grid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
		if (cell != currentCell) {
			currentCell = cell;
			return true;
		}
		return false;
	}
}