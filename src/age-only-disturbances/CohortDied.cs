//  Copyright 2007-2010 Portland State University, University of Wisconsin-Madison
//  Author: Robert Scheller, Ben Sulman

using Landis.Core;
using Landis.SpatialModeling;
using Edu.Wisc.Forest.Flel.Util;
using System.Collections.Generic;

using Landis.Library.LeafBiomassCohorts;  

namespace Landis.Extension.Succession.NetEcosystemCN.AgeOnlyDisturbances
{
    /// <summary>
    /// The handlers for various type of events related to age-only
    /// disturbances.
    /// </summary>
    public static CohortDeath
    {
        //---------------------------------------------------------------------

        public void CohortDied(object sender,
                               DeathEventArgs eventArgs)
        {

            //PlugIn.ModelCore.UI.WriteLine("Cohort Died! :-(");

            ExtensionType disturbanceType = eventArgs.DisturbanceType;
            ActiveSite site = eventArgs.Site;

            ICohort cohort = eventArgs.Cohort;
            double foliar = (double)cohort.LeafBiomass;

            double wood = (double)cohort.WoodBiomass;

            //PlugIn.ModelCore.UI.WriteLine("Cohort Died: species={0}, age={1}, biomass={2}, foliage={3}.", cohort.Species.Name, cohort.Age, cohort.Biomass, foliar);

            if (disturbanceType == null)
            {
                //PlugIn.ModelCore.UI.WriteLine("NO EVENT: Cohort Died: species={0}, age={1}, disturbance={2}.", cohort.Species.Name, cohort.Age, eventArgs.DisturbanceType);

                ForestFloor.AddWoodLitter(wood, cohort.Species, eventArgs.Site);
                ForestFloor.AddFoliageLitter(foliar, cohort.Species, eventArgs.Site);

                Roots.AddCoarseRootLitter(wood, cohort, cohort.Species, eventArgs.Site);
                Roots.AddFineRootLitter(foliar, cohort, cohort.Species, eventArgs.Site);
            }

            if (disturbanceType != null)
            {
                //PlugIn.ModelCore.UI.WriteLine("DISTURBANCE EVENT: Cohort Died: species={0}, age={1}, disturbance={2}.", cohort.Species.Name, cohort.Age, eventArgs.DisturbanceType);

                Disturbed[site] = true;
                if (disturbanceType.IsMemberOf("disturbance:fire"))
                    Landis.Library.Succession.Reproduction.CheckForPostFireRegen(eventArgs.Cohort, site);
                else
                    Landis.Library.Succession.Reproduction.CheckForResprouting(eventArgs.Cohort, site);
            }
        }
    }
}
