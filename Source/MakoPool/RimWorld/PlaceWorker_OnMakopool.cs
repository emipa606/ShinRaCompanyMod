using Verse;

namespace RimWorld;

public class PlaceWorker_OnMakopool : PlaceWorker_OnSteamGeyser
{
    public override bool ForceAllowPlaceOver(BuildableDef otherDef)
    {
        return otherDef == SRThingDefOf.Makopool;
    }
}