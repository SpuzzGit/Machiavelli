﻿
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DamageUnitBehaviour : AbilityBehaviour
{

    public override void Use(HexCell target = null)
    {
        HexUnit targetUnit = target.hexUnits.Find(C => C.HexUnitType == HexUnit.UnitType.COMBAT);
        if (targetUnit)
        {
            targetUnit.GetComponent<Unit>().HitPoints -= (config as DamageUnitConfig).GetDamage();
            targetUnit.GetComponent<Unit>().UpdateUI();
            if (targetUnit.GetComponent<Unit>().HitPoints <= 0)
            {
                targetUnit.DieAnimationAndRemove();
            }
        }
        PlayParticleEffect();
        PlayAbilitySound();
        PlayAnimation();
    }
    public override List<HexCell> IsValidTarget(HexCell target)
    {
        List<HexCell> cells = PathFindingUtilities.GetCellsInRange(target, (config as DamageUnitConfig).Range);
        List<HexCell> unitCells = cells.FindAll(c => c.hexUnits.FindAll(d => d.HexUnitType == HexUnit.UnitType.COMBAT).Count != 0);

        return unitCells;
    }



}

