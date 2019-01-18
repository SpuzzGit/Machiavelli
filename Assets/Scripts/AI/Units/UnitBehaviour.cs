﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitBehaviour : MonoBehaviour
{
    
    Unit unit;

    public Unit ActiveUnit
    {
        get
        {
            return unit;
        }

        set
        {
            unit = value;
        }
    }

    HexGrid hexGrid;
    public HexGrid GameHexGrid
    {
        get
        {
            return hexGrid;
        }

        set
        {
            hexGrid = value;
        }
    }

    private void Awake()
    {
        GameHexGrid = FindObjectOfType<HexGrid>();
        ActiveUnit = GetComponent<Unit>();
    }

    public HexCell ExploreArea(HexCell centreCell, int distanceFromCentre)
    {
        List<HexCell> exploreCell = hexGrid.GetNearestUnexplored(centreCell, distanceFromCentre);
        return unit.HexUnit.Location;
    }

    public HexCell Explore()
    {
        return unit.HexUnit.Location;
    }
    public HexCell Idle()
    {
        return unit.HexUnit.Location;
    }

    public HexCell Attack(Unit target)
    {
        hexGrid.FindPath(unit.HexUnit.Location, target.HexUnit.Location, unit.HexUnit);
        return GetFirstCellFromPath();
        
        
    }

    private HexCell GetFirstCellFromPath()
    {
        List<HexCell> path = hexGrid.GetPath();
        if (path != null && path.Count > 1)
        {
            return path[1];
        }
        else
        {
            return null;
        }
    }

    public HexCell Attack(City city)
    {
        hexGrid.FindPath(unit.HexUnit.Location, city.GetHexCell(), unit.HexUnit,true,false);
        return GetFirstCellFromPath();
    }

    public HexCell Defend(City city)
    {
        hexGrid.FindPath(unit.HexUnit.Location, city.GetHexCell(), unit.HexUnit);
        return GetFirstCellFromPath();
    }

    public HexCell Patrol(HexCell hexCell, int radius)
    {
        if (hexCell.coordinates.DistanceTo(unit.HexUnit.Location.coordinates) > radius)
        {
            hexGrid.FindPath(unit.HexUnit.Location, hexCell, unit.HexUnit);
            return GetFirstCellFromPath();
        }
        HexCell target = null;

        List<HexDirection> directions = Enum.GetValues(typeof(HexDirection)).Cast<HexDirection>().ToList();
        while(directions.Count > 0 && !target)
        {
            HexDirection direction = directions[UnityEngine.Random.Range(0, directions.Count)];
            directions.Remove(direction);
            HexCell neighbour = unit.HexUnit.Location.GetNeighbor(direction);
            if(hexCell.coordinates.DistanceTo(neighbour.coordinates) <= radius && unit.HexUnit.IsValidDestination(neighbour,true))
            {
                target = neighbour;
            }
        }
        return target;
    }
}