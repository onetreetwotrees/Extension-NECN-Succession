//  Author: Robert Scheller

using Landis.Core;
using Landis.SpatialModeling;
using Landis.Library.LeafBiomassCohorts;
using System.Collections.Generic;
using Landis.Library.Climate;

namespace Landis.Extension.Succession.NetEcosystemCN
{
    /// <summary>
    /// Utility methods.
    /// </summary>
    public class Main
    {
        public static int Year;
        public static int Month;
        public static int MonthCnt;

        /// <summary>
        /// Grows all cohorts at a site for a specified number of years.
        /// Litter is decomposed following growth.
        /// </summary>
        public static ISiteCohorts Run(ActiveSite site,
                                       int         years,
                                       bool        isSuccessionTimeStep)
        {
            
            ISiteCohorts siteCohorts = SiteVars.Cohorts[site];
            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];

            for (int y = 0; y < years; ++y) {

                Year = y + 1;
                
                if (PlugIn.ModelCore.CurrentTime > 0 && Climate.Future_MonthlyData.ContainsKey(PlugIn.FutureClimateBaseYear + y + PlugIn.ModelCore.CurrentTime- years))
                    EcoregionData.AnnualWeather[ecoregion] = Climate.Future_MonthlyData[PlugIn.FutureClimateBaseYear + y - years + PlugIn.ModelCore.CurrentTime][ecoregion.Index];

                SiteVars.ResetAnnualValues(site);

                if(y == 0 && SiteVars.FireSeverity != null && SiteVars.FireSeverity[site] > 0)
                    FireEffects.ReduceLayers(SiteVars.FireSeverity[site], site);

                // Next, Grow and Decompose each month
                int[] months = new int[12]{6, 7, 8, 9, 10, 11, 0, 1, 2, 3, 4, 5};

                if(OtherData.CalibrateMode)
                    months = new int[12] { 6, 7, 8, 9, 10, 11, 0, 1, 2, 3, 4, 5 };

                for (MonthCnt = 0; MonthCnt < 12; MonthCnt++)
                {
                    // Calculate mineral N fractions based on coarse root biomass 
                    if (MonthCnt == 0)
                    {
                        AvailableN.CalculateMineralNfraction(site);
                    }
                    //PlugIn.ModelCore.UI.WriteLine("SiteVars.MineralN = {0:0.00}, month = {1}.", SiteVars.MineralN[site], i);

                    Month = months[MonthCnt];

                    SiteVars.MonthlyAGNPPcarbon[site][Month] = 0.0;
                    SiteVars.MonthlyBGNPPcarbon[site][Month] = 0.0;
                    SiteVars.MonthlyNEE[site][Month] = 0.0;
                    SiteVars.MonthlyResp[site][Month] = 0.0;
                    SiteVars.MonthlyStreamN[site][Month] = 0.0;
                    SiteVars.SourceSink[site].Carbon = 0.0;
                    SiteVars.TotalWoodBiomass[site] = Main.ComputeWoodBiomass((ActiveSite) site);
                    //SiteVars.LAI[site] = Main.ComputeLAI((ActiveSite)site);
                                   
                    double ppt = EcoregionData.AnnualWeather[ecoregion].MonthlyPrecip[Main.Month];

                    double monthlyNdeposition;
                    if  (EcoregionData.AtmosNintercept[ecoregion]!=-1 && EcoregionData.AtmosNslope[ecoregion] !=-1)
                        monthlyNdeposition = EcoregionData.AtmosNintercept[ecoregion] + (EcoregionData.AtmosNslope[ecoregion] * ppt);
                    else 
                    {
                        monthlyNdeposition = EcoregionData.AnnualWeather[ecoregion].MonthlyNDeposition[Main.Month];
                    }

                    if (monthlyNdeposition < 0)
                        throw new System.ApplicationException("Error: Nitrogen deposition input data are not present in climate library");

                    EcoregionData.MonthlyNDeposition[ecoregion][Month] = monthlyNdeposition;
                    EcoregionData.AnnualNDeposition[ecoregion] += monthlyNdeposition;
                    SiteVars.MineralN[site] += monthlyNdeposition;
                    //PlugIn.ModelCore.UI.WriteLine("Ndeposition={0},MineralN={1:0.00}.", monthlyNdeposition, SiteVars.MineralN[site]);

                    double liveBiomass = (double) ComputeLivingBiomass(siteCohorts);
                    double baseFlow, stormFlow;
                    SoilWater.Run(y, Month, liveBiomass, site, out baseFlow, out stormFlow);

                    // Calculate N allocation for each cohort
                    AvailableN.SetMineralNallocation(site);

                    if (MonthCnt==11)
                        siteCohorts.Grow(site, (y == years && isSuccessionTimeStep), true);
                    else
                        siteCohorts.Grow(site, (y == years && isSuccessionTimeStep), false);

                    WoodLayer.Decompose(site);
                    LitterLayer.Decompose(site);
                    SoilLayer.Decompose(site);
                   
                    //...Volatilization loss as a function of the mineral n which
                    //     remains after uptake by plants.  ML added a correction factor for wetlands since their denitrification rate is double that of wetlands
                    //based on a review paper by Seitziner 2006.

                    double volatilize = (SiteVars.MineralN[site] * EcoregionData.Denitrif[ecoregion]); // monthly value

                    //PlugIn.ModelCore.UI.WriteLine("BeforeVol.  MineralN={0:0.00}.", SiteVars.MineralN[site]);

                    SiteVars.MineralN[site] -= volatilize;
                    SiteVars.SourceSink[site].Nitrogen += volatilize;
                    SiteVars.Nvol[site] += volatilize;

                    SoilWater.Leach(site, baseFlow, stormFlow);

                    SiteVars.MonthlyNEE[site][Month] -= SiteVars.MonthlyAGNPPcarbon[site][Month];
                    SiteVars.MonthlyNEE[site][Month] -= SiteVars.MonthlyBGNPPcarbon[site][Month];
                    SiteVars.MonthlyNEE[site][Month] += SiteVars.SourceSink[site].Carbon;

                }
            }

            ComputeTotalCohortCN(site, siteCohorts);

            return siteCohorts;
        }

        //---------------------------------------------------------------------

        public static int ComputeLivingBiomass(ISiteCohorts cohorts)
        {
            int total = 0;
            if (cohorts != null)
                foreach (ISpeciesCohorts speciesCohorts in cohorts)
                    foreach (ICohort cohort in speciesCohorts)
                        total += (int) (cohort.WoodBiomass + cohort.LeafBiomass);
                    //total += ComputeBiomass(speciesCohorts);
            return total;
        }

        //---------------------------------------------------------------------

        public static double ComputeWoodBiomass(ActiveSite site)
        {
            double woodBiomass = 0;
            if (SiteVars.Cohorts[site] != null)
                foreach (ISpeciesCohorts speciesCohorts in SiteVars.Cohorts[site])
                    foreach (ICohort cohort in speciesCohorts)
                        woodBiomass += cohort.WoodBiomass;
            return woodBiomass;
        }

        //---------------------------------------------------------------------
        private static void ComputeTotalCohortCN(ActiveSite site, ISiteCohorts cohorts)
        {
            SiteVars.CohortLeafC[site] = 0;
            SiteVars.CohortFRootC[site] = 0;
            SiteVars.CohortLeafN[site] = 0;
            SiteVars.CohortFRootN[site] = 0;
            SiteVars.CohortWoodC[site] = 0;
            SiteVars.CohortCRootC[site] = 0;
            SiteVars.CohortWoodN[site] = 0;
            SiteVars.CohortCRootN[site] = 0;

            if (cohorts != null)
                foreach (ISpeciesCohorts speciesCohorts in cohorts)
                    foreach (ICohort cohort in speciesCohorts)
                        CalculateCohortCN(site, cohort);
            return;
        }

        /// <summary>
        /// Summarize cohort C&N for output.
        /// </summary>
        private static void CalculateCohortCN(ActiveSite site, ICohort cohort)
        {
            ISpecies species = cohort.Species;

            double leafC = cohort.LeafBiomass * 0.47;
            double woodC = cohort.WoodBiomass * 0.47;

            double fRootC = Roots.CalculateFineRoot(cohort, leafC);
            double cRootC = Roots.CalculateCoarseRoot(cohort, woodC);

            double totalC = leafC + woodC + fRootC + cRootC;

            double leafN  = leafC /  (double) SpeciesData.LeafCN[species];
            double woodN = woodC / (double) SpeciesData.WoodCN[species];
            double cRootN = cRootC / (double) SpeciesData.CoarseRootCN[species];
            double fRootN = fRootC / (double) SpeciesData.FineRootCN[species];

            //double totalN = woodN + cRootN + leafN + fRootN;

            //PlugIn.ModelCore.UI.WriteLine("month={0}, species={1}, leafB={2:0.0}, leafC={3:0.00}, leafN={4:0.0}, woodB={5:0.0}, woodC={6:0.000}, woodN={7:0.0}", Month, cohort.Species.Name, cohort.LeafBiomass, leafC, leafN, cohort.WoodBiomass, woodC, woodN);

            SiteVars.CohortLeafC[site] += leafC;
            SiteVars.CohortFRootC[site] += fRootC;
            SiteVars.CohortLeafN[site] += leafN;
            SiteVars.CohortFRootN[site] += fRootN;
            SiteVars.CohortWoodC[site] += woodC;
            SiteVars.CohortCRootC[site] += cRootC;
            SiteVars.CohortWoodN[site] += woodN ;
            SiteVars.CohortCRootN[site] += cRootN;

            return;

        }

    }
}
