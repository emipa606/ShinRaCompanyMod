using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

public class GameCondition_MakoContamination : GameCondition
{
    private const int LerpTicks = 5000;

    private const float MaxSkyLerpFactor = 0.5f;

    private const float SkyGlow = 0.85f;

    private const int CheckInterval = 3451;

    private const float ToxicPerDay = 0.5f;

    private const float PlantKillChance = 0.0065f;

    private const float CorpseRotProgressAdd = 3000f;

    private readonly List<SkyOverlay> overlays = new List<SkyOverlay>
    {
        new WeatherOverlay_Fallout()
    };

    private readonly SkyColorSet ToxicFalloutColors = new SkyColorSet(new ColorInt(0, 255, 162).ToColor,
        new ColorInt(234, 200, 255).ToColor, new Color(0.6f, 0.8f, 0.5f), 0.85f);

    public override void Init()
    {
        LessonAutoActivator.TeachOpportunity(ConceptDefOf.ForbiddingDoors, OpportunityType.Critical);
        LessonAutoActivator.TeachOpportunity(ConceptDefOf.AllowedAreas, OpportunityType.Critical);
    }

    public override void GameConditionTick()
    {
        var affectedMaps = AffectedMaps;
        if (Find.TickManager.TicksGame % 3451 == 0)
        {
            foreach (var map in affectedMaps)
            {
                DoPawnsToxicDamage(map);
            }
        }

        foreach (var skyOverlay in overlays)
        {
            foreach (var map in affectedMaps)
            {
                skyOverlay.TickOverlay(map);
            }
        }
    }

    private void DoPawnsToxicDamage(Map map)
    {
        var allPawnsSpawned = map.mapPawns.AllPawnsSpawned;
        foreach (var pawn in allPawnsSpawned)
        {
            if (pawn.Position.Roofed(map) || !pawn.def.race.IsFlesh)
            {
                continue;
            }

            var num = 0.028758334f;
            var resistance = pawn.GetStatValue(StatDefOf.ToxicEnvironmentResistance);
            if (resistance != 0)
            {
                num /= resistance;
            }

            if (num == 0f)
            {
                continue;
            }

            var num2 = Mathf.Lerp(0.85f, 1.15f, Rand.ValueSeeded(pawn.thingIDNumber ^ 0x46EDC5D));
            num *= num2;
            HealthUtility.AdjustSeverity(pawn, HediffDefOf.ToxicBuildup, num);
        }
    }

    public override void DoCellSteadyEffects(IntVec3 c, Map map)
    {
        if (c.Roofed(map))
        {
            return;
        }

        var thingList = c.GetThingList(map);
        // Must be forloop as things can be removed in the iteration
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < thingList.Count; i++)
        {
            var thing = thingList[i];
            if (thing is Plant)
            {
                if (Rand.Value < 0.0065f)
                {
                    thing.Kill();
                }
            }
            else if (thing.def.category == ThingCategory.Item)
            {
                var compRottable = thing.TryGetComp<CompRottable>();
                if (compRottable != null && (int)compRottable.Stage < 2)
                {
                    compRottable.RotProgress += 3000f;
                }
            }
        }
    }

    public override void GameConditionDraw(Map map)
    {
        foreach (var skyOverlay in overlays)
        {
            skyOverlay.DrawOverlay(map);
        }
    }

    public override float SkyTargetLerpFactor(Map map)
    {
        return GameConditionUtility.LerpInOutValue(this, 5000f, 0.5f);
    }

    public override SkyTarget? SkyTarget(Map map)
    {
        return new SkyTarget(0.85f, ToxicFalloutColors, 1f, 1f);
    }

    public override float AnimalDensityFactor(Map map)
    {
        return 0f;
    }

    public override float PlantDensityFactor(Map map)
    {
        return 0f;
    }

    public override bool AllowEnjoyableOutsideNow(Map map)
    {
        return false;
    }

    public override List<SkyOverlay> SkyOverlays(Map map)
    {
        return overlays;
    }
}