//  Copyright 2007-2010 Portland State University, University of Wisconsin-Madison
//  Author: Robert Scheller, Melissa Lucash

using Landis.Core;
using Edu.Wisc.Forest.Flel.Util;
using Landis.Library.Succession;
using System.Collections.Generic;

namespace Landis.Extension.Succession.Century
{
    /// <summary>
    /// A parser that reads biomass succession parameters from text input.
    /// </summary>
    public class InputParametersParser//<TParseResult>
        : TextParser<IInputParameters>
    {
        public override string LandisDataValue
        {
            get
            {
                return PlugIn.ExtensionName;
            }
        }


        public static class Names
        {
            public const string Timestep = "Timestep";
            public const string SeedingAlgorithm = "SeedingAlgorithm";

            public const string ClimateConfigFile = "ClimateConfigFile";
            public const string CalibrateMode = "CalibrateMode";
            public const string SufficientLight = "SufficientLightTable";
            public const string SpeciesParameters = "SpeciesParameters";
            public const string FunctionalGroupParameters = "FunctionalGroupParameters";
            public const string EcoregionParameters = "EcoregionParameters";
            public const string FireReductionParameters = "FireReductionParameters";
            public const string HarvestReductionParameters = "HarvestReductionParameters";
            public const string AgeOnlyDisturbanceParms = "AgeOnlyDisturbances:BiomassParameters";
        }

        //---------------------------------------------------------------------

        private IEcoregionDataset ecoregionDataset;
        private ISpeciesDataset speciesDataset;
        private Dictionary<string, int> speciesLineNums;
        private InputVar<string> speciesName;

        //---------------------------------------------------------------------

        static InputParametersParser()
        {
            SeedingAlgorithmsUtil.RegisterForInputValues();
            RegisterForInputValues();
            Percentage dummy = new Percentage();

        }

        //---------------------------------------------------------------------

        public InputParametersParser()
        {
            this.ecoregionDataset = PlugIn.ModelCore.Ecoregions;
            this.speciesDataset = PlugIn.ModelCore.Species;
            this.speciesLineNums = new Dictionary<string, int>();
            this.speciesName = new InputVar<string>("Species");

            //Dynamic.InputValidation.Initialize();
        }

        //---------------------------------------------------------------------

        protected override IInputParameters Parse()
        {

            ReadLandisDataVar();

            int numLitterTypes = 4;
            int numFunctionalTypes = 25;

            InputParameters parameters = new InputParameters(ecoregionDataset, speciesDataset, numLitterTypes, numFunctionalTypes);

            InputVar<int> timestep = new InputVar<int>(Names.Timestep);
            ReadVar(timestep);
            parameters.Timestep = timestep.Value;

            InputVar<SeedingAlgorithms> seedAlg = new InputVar<SeedingAlgorithms>(Names.SeedingAlgorithm);
            ReadVar(seedAlg);
            parameters.SeedAlgorithm = seedAlg.Value;

            //---------------------------------------------------------------------------------

            InputVar<string> initCommunities = new InputVar<string>("InitialCommunities");
            ReadVar(initCommunities);
            parameters.InitialCommunities = initCommunities.Value;

            InputVar<string> communitiesMap = new InputVar<string>("InitialCommunitiesMap");
            ReadVar(communitiesMap);
            parameters.InitialCommunitiesMap = communitiesMap.Value;

            InputVar<string> climateConfigFile = new InputVar<string>(Names.ClimateConfigFile);
            ReadVar(climateConfigFile);
            parameters.ClimateConfigFile = climateConfigFile.Value;

            InputVar<string> ageOnlyDisturbanceParms = new InputVar<string>(Names.AgeOnlyDisturbanceParms);
            ReadVar(ageOnlyDisturbanceParms);
            parameters.AgeOnlyDisturbanceParms = ageOnlyDisturbanceParms.Value;

            InputVar<string> soilDepthMapName = new InputVar<string>("SoilDepthMapName");
            ReadVar(soilDepthMapName);
            parameters.SoilDepthMapName = soilDepthMapName.Value;

            InputVar<string> soilDrainMapName = new InputVar<string>("SoilDrainMapName");
            ReadVar(soilDrainMapName);
            parameters.SoilDrainMapName = soilDrainMapName.Value;

            InputVar<string> soilBaseFlowMapName = new InputVar<string>("SoilBaseFlowMapName");
            ReadVar(soilBaseFlowMapName);
            parameters.SoilBaseFlowMapName = soilBaseFlowMapName.Value;

            InputVar<string> soilStormFlowMapName = new InputVar<string>("SoilStormFlowMapName");
            ReadVar(soilStormFlowMapName);
            parameters.SoilStormFlowMapName = soilStormFlowMapName.Value;

            InputVar<string> soilFCMapName = new InputVar<string>("SoilFieldCapacityMapName");
            ReadVar(soilFCMapName);
            parameters.SoilFieldCapacityMapName = soilFCMapName.Value;

            InputVar<string> soilWPMapName = new InputVar<string>("SoilWiltingPointMapName");
            ReadVar(soilWPMapName);
            parameters.SoilWiltingPointMapName = soilWPMapName.Value;

            InputVar<string> soilSandMapName = new InputVar<string>("SoilPercentSandMapName");
            ReadVar(soilSandMapName);
            parameters.SoilPercentSandMapName = soilSandMapName.Value;

            InputVar<string> soilClayMapName = new InputVar<string>("SoilPercentClayMapName");
            ReadVar(soilClayMapName);
            parameters.SoilPercentClayMapName = soilClayMapName.Value;

            InputVar<string> som1CsurfMapName = new InputVar<string>("InitialSOM1CsurfMapName");
            ReadVar(som1CsurfMapName);
            parameters.InitialSOM1CSurfaceMapName = som1CsurfMapName.Value;

            InputVar<string> som1NsurfMapName = new InputVar<string>("InitialSOM1NsurfMapName");
            ReadVar(som1NsurfMapName);
            parameters.InitialSOM1NSurfaceMapName = som1NsurfMapName.Value;

            InputVar<bool> calimode = new InputVar<bool>(Names.CalibrateMode);
            if (ReadOptionalVar(calimode))
                parameters.CalibrateMode = calimode.Value;
            else
                parameters.CalibrateMode = false;

            //InputVar<double> spinMort = new InputVar<double>("SpinupMortalityFraction");
            //if (ReadOptionalVar(spinMort))
            //    parameters.SpinupMortalityFraction = spinMort.Value;
            //else
            //    parameters.SpinupMortalityFraction = 0.0;

            InputVar<string> wt = new InputVar<string>("WaterDecayFunction");
            ReadVar(wt);
            parameters.WType = WParse(wt.Value);

            InputVar<double> pea = new InputVar<double>("ProbabilityEstablishAdjust");
            ReadVar(pea);
            parameters.ProbEstablishAdjustment = pea.Value;

            InputVar<double> iMN = new InputVar<double>("InitialMineralN");
            ReadVar(iMN);
            parameters.SetInitMineralN(iMN.Value);

            InputVar<double> ans = new InputVar<double>("AtmosphericNSlope");
            ReadVar(ans);
            parameters.SetAtmosNslope(ans.Value);

            InputVar<double> ani = new InputVar<double>("AtmosphericNIntercept");
            ReadVar(ani);
            parameters.SetAtmosNintercept(ani.Value);

            InputVar<double> lat = new InputVar<double>("Latitude");
            ReadVar(lat);
            parameters.SetLatitude(lat.Value);

            InputVar<double> denits = new InputVar<double>("DenitrificationRate");
            ReadVar(denits);
            parameters.SetDenitrif(denits.Value);

            InputVar<double> drsoms = new InputVar<double>("DecayRateSurf");
            ReadVar(drsoms);
            parameters.SetDecayRateSurf(drsoms.Value);

            InputVar<double> drsom1 = new InputVar<double>("DecayRateSOM1");
            ReadVar(drsom1);
            parameters.SetDecayRateSOM1(drsom1.Value);

            InputVar<double> drsom2 = new InputVar<double>("DecayRateSOM2");
            ReadVar(drsom2);
            parameters.SetDecayRateSOM2(drsom2.Value);

            InputVar<double> drsom3 = new InputVar<double>("DecayRateSOM3");
            ReadVar(drsom3);
            parameters.SetDecayRateSOM3(drsom3.Value);

            //InputVar<string> soilCarbonMaps = new InputVar<string>("SoilCarbonMapNames");
            //if (ReadOptionalVar(soilCarbonMaps))
            //{
            //    PlugIn.SoilCarbonMapNames = soilCarbonMaps.Value;

            //    InputVar<int> soilCarbonMapFreq = new InputVar<int>("SoilCarbonMapFrequency");
            //    ReadVar(soilCarbonMapFreq);
            //    PlugIn.SoilCarbonMapFrequency = soilCarbonMapFreq.Value;

            //}

            //InputVar<string> soilNitrogenMaps = new InputVar<string>("SoilNitrogenMapNames");
            //if (ReadOptionalVar(soilNitrogenMaps))
            //{
            //    PlugIn.SoilNitrogenMapNames = soilNitrogenMaps.Value;

            //    InputVar<int> soilNitrogenMapFreq = new InputVar<int>("SoilNitrogenMapFrequency");
            //    ReadVar(soilNitrogenMapFreq);
            //    PlugIn.SoilNitrogenMapFrequency = soilNitrogenMapFreq.Value;

            //}

            InputVar<string> anppMaps = new InputVar<string>("ANPPMapNames");
            if (ReadOptionalVar(anppMaps))
            {
                PlugIn.ANPPMapNames = anppMaps.Value;

                InputVar<int> anppMapFreq = new InputVar<int>("ANPPMapFrequency");
                ReadVar(anppMapFreq);
                PlugIn.ANPPMapFrequency = anppMapFreq.Value;

            }

            InputVar<string> aneeMaps = new InputVar<string>("ANEEMapNames");
            if (ReadOptionalVar(aneeMaps))
            {
                PlugIn.ANEEMapNames = aneeMaps.Value;

                InputVar<int> aneeMapFreq = new InputVar<int>("ANEEMapFrequency");
                ReadVar(aneeMapFreq);
                PlugIn.ANEEMapFrequency = aneeMapFreq.Value;

            }

            InputVar<string> soilCarbonMaps = new InputVar<string>("SoilCarbonMapNames");
            if (ReadOptionalVar(soilCarbonMaps))
            {
                PlugIn.SoilCarbonMapNames = soilCarbonMaps.Value;

                InputVar<int> soilCarbonMapFreq = new InputVar<int>("SoilCarbonMapFrequency");
                ReadVar(soilCarbonMapFreq);
                PlugIn.SoilCarbonMapFrequency = soilCarbonMapFreq.Value;

            }

            InputVar<string> soilNitrogenMaps = new InputVar<string>("SoilNitrogenMapNames");
            if (ReadOptionalVar(soilNitrogenMaps))
            {
                PlugIn.SoilNitrogenMapNames = soilNitrogenMaps.Value;

                InputVar<int> soilNitrogenMapFreq = new InputVar<int>("SoilNitrogenMapFrequency");
                ReadVar(soilNitrogenMapFreq);
                PlugIn.SoilNitrogenMapFrequency = soilNitrogenMapFreq.Value;

            }

            InputVar<string> totalCMaps = new InputVar<string>("TotalCMapNames");
            if (ReadOptionalVar(totalCMaps))
            {
                PlugIn.TotalCMapNames = totalCMaps.Value;

                InputVar<int> totalCMapFreq = new InputVar<int>("TotalCMapFrequency");
                ReadVar(totalCMapFreq);
                PlugIn.TotalCMapFrequency = totalCMapFreq.Value;

            }
            //--------------------------
            //  MinRelativeBiomass table

            ReadName("AvailableLightBiomass");

            List<IEcoregion> ecoregions = ReadEcoregions();
            string lastEcoregion = ecoregions[ecoregions.Count-1].Name;

            InputVar<byte> shadeClassVar = new InputVar<byte>("Shade Class");
            for (byte shadeClass = 1; shadeClass <= 5; shadeClass++) {
                if (AtEndOfInput)
                    throw NewParseException("Expected a line with available light class {0}", shadeClass);

                StringReader currentLine = new StringReader(CurrentLine);
                ReadValue(shadeClassVar, currentLine);
                if (shadeClassVar.Value.Actual != shadeClass)
                    throw new InputValueException(shadeClassVar.Value.String,
                                                  "Expected the available light class {0}", shadeClass);

                foreach (IEcoregion ecoregion in ecoregions)
                {
                    InputVar<Percentage> MinRelativeBiomass = new InputVar<Percentage>("Ecoregion " + ecoregion.Name);
                    ReadValue(MinRelativeBiomass, currentLine);
                    parameters.SetMinRelativeBiomass(shadeClass, ecoregion, MinRelativeBiomass.Value);
                }

                CheckNoDataAfter("the Ecoregion " + lastEcoregion + " column",
                                 currentLine);
                GetNextLine();
            }

            //----------------------------------------------------------
            //  Read table of sufficient light probabilities.
            //  Available light classes are in increasing order.
            ReadName("LightEstablishmentTable");

            InputVar<byte> sc = new InputVar<byte>("Available Light Class");
            InputVar<double> pl0 = new InputVar<double>("Probability of Germination - Light Level 0");
            InputVar<double> pl1 = new InputVar<double>("Probability of Germination - Light Level 1");
            InputVar<double> pl2 = new InputVar<double>("Probability of Germination - Light Level 2");
            InputVar<double> pl3 = new InputVar<double>("Probability of Germination - Light Level 3");
            InputVar<double> pl4 = new InputVar<double>("Probability of Germination - Light Level 4");
            InputVar<double> pl5 = new InputVar<double>("Probability of Germination - Light Level 5");

            int previousNumber = 0;

            while (! AtEndOfInput && CurrentName != Names.SpeciesParameters
                                  && previousNumber != 6) {
                StringReader currentLine = new StringReader(CurrentLine);

                ISufficientLight suffLight = new SufficientLight();

                ReadValue(sc, currentLine);
                suffLight.ShadeClass = sc.Value;

                //  Check that the current shade class is 1 more than
                //  the previous number (numbers are must be in increasing order).
                if (sc.Value.Actual != (byte) previousNumber + 1)
                    throw new InputValueException(sc.Value.String,
                                                  "Expected the severity number {0}",
                                                  previousNumber + 1);
                previousNumber = (int) sc.Value.Actual;

                ReadValue(pl0, currentLine);
                suffLight.ProbabilityLight0 = pl0.Value;

                ReadValue(pl1, currentLine);
                suffLight.ProbabilityLight1 = pl1.Value;

                ReadValue(pl2, currentLine);
                suffLight.ProbabilityLight2 = pl2.Value;

                ReadValue(pl3, currentLine);
                suffLight.ProbabilityLight3 = pl3.Value;

                ReadValue(pl4, currentLine);
                suffLight.ProbabilityLight4 = pl4.Value;

                ReadValue(pl5, currentLine);
                suffLight.ProbabilityLight5 = pl5.Value;

                parameters.LightClassProbabilities.Add(suffLight);

                CheckNoDataAfter("the " + pl5.Name + " column",
                                 currentLine);
                GetNextLine();
            }
            if (parameters.LightClassProbabilities.Count == 0)
                throw NewParseException("No sufficient light probabilities defined.");
            if (previousNumber != 5)
                throw NewParseException("Expected shade class {0}", previousNumber + 1);

            //-------------------------
            //  Species Parameters table

            ReadName("SpeciesParameters");

            speciesLineNums.Clear();  //  If parser re-used (i.e., for testing purposes)

            InputVar<int> ft = new InputVar<int>("Functional Type");
            InputVar<bool> nt = new InputVar<bool>("Nitrogen Fixer");
            InputVar<int> gddmn = new InputVar<int>("Growing Degree Day Minimum");
            InputVar<int> gddmx = new InputVar<int>("Growing Degree Day Maximum");
            InputVar<int> mjt = new InputVar<int>("Minimum January Temperature");
            InputVar<double> maxd = new InputVar<double>("Maximum Allowable Drought");
            InputVar<double> leafLongevity = new InputVar<double>("Leaf Longevity");
            InputVar<bool> epicorm = new InputVar<bool>("Epicormic:  Y/N");
            InputVar<double> leafLignin = new InputVar<double>("Leaf Percent Lignin");
            InputVar<double> wLignin = new InputVar<double>("Wood Percent Lignin");
            InputVar<double> crLignin = new InputVar<double>("Coarse Root Percent Lignin");
            InputVar<double> frLignin = new InputVar<double>("Fine Root Percent Lignin");
            InputVar<double> leafCN = new InputVar<double>("Leaf CN Ratio");
            InputVar<double> woodCN = new InputVar<double>("Wood CN Ratio");
            InputVar<double> cRootCN = new InputVar<double>("Coarse Root CN Ratio");
            InputVar<double> foliarCN = new InputVar<double>("Foliage CN Ratio");
            InputVar<double> fRootCN = new InputVar<double>("Fine Root CN Ratio");
            InputVar<int> maxANPP = new InputVar<int>("Maximum ANPP");
            InputVar<int> maxBiomass = new InputVar<int>("Maximum Aboveground Biomass");
            string lastColumn = "the " + maxBiomass.Name + " column";

            while (! AtEndOfInput && CurrentName != Names.FunctionalGroupParameters) {
                StringReader currentLine = new StringReader(CurrentLine);
                ISpecies species = ReadSpecies(currentLine);

                ReadValue(ft, currentLine);
                parameters.SetFunctionalType(species, ft.Value);

                ReadValue(nt, currentLine);
                parameters.NFixer[species] = nt.Value;

                ReadValue(gddmn, currentLine);
                parameters.SetGDDmin(species, gddmn.Value);

                ReadValue(gddmx, currentLine);
                parameters.SetGDDmax(species, gddmx.Value);

                ReadValue(mjt, currentLine);
                parameters.SetMinJanTemp(species, mjt.Value);

                ReadValue(maxd, currentLine);
                parameters.SetMaxDrought(species, maxd.Value);

                ReadValue(leafLongevity, currentLine);
                parameters.SetLeafLongevity(species, leafLongevity.Value);

                ReadValue(epicorm, currentLine);
                parameters.Epicormic[species] = epicorm.Value;

                ReadValue(leafLignin, currentLine);
                parameters.SetLeafLignin(species, leafLignin.Value);

                ReadValue(frLignin, currentLine);
                parameters.SetFineRootLignin(species, frLignin.Value);

                ReadValue(wLignin, currentLine);
                parameters.SetWoodLignin(species, wLignin.Value);

                ReadValue(crLignin, currentLine);
                parameters.SetCoarseRootLignin(species, crLignin.Value);

                ReadValue(leafCN, currentLine);
                parameters.SetLeafCN(species, leafCN.Value);

                ReadValue(fRootCN, currentLine);
                parameters.SetFineRootCN(species, fRootCN.Value);

                ReadValue(woodCN, currentLine);
                parameters.SetWoodCN(species, woodCN.Value);

                ReadValue(cRootCN, currentLine);
                parameters.SetCoarseRootCN(species, cRootCN.Value);

                ReadValue(foliarCN, currentLine);
                parameters.SetFoliageLitterCN(species, foliarCN.Value);

                ReadValue(maxANPP, currentLine);
                parameters.SetMaxANPP(species, maxANPP.Value);

                ReadValue(maxBiomass, currentLine);
                parameters.SetMaxBiomass(species, maxANPP.Value);

                CheckNoDataAfter(lastColumn, currentLine);
                GetNextLine();
            }

            //--------- Read In Functional Group Table -------------------------------
            PlugIn.ModelCore.UI.WriteLine("   Begin parsing FUNCTIONAL GROUP table.");

            ReadName(Names.FunctionalGroupParameters);
            string InitialEcoregionParameters = "InitialEcoregionParameters";

            InputVar<string> ftname = new InputVar<string>("Name");
            InputVar<int> ftindex = new InputVar<int>("Index (< 25)");
            InputVar<double> ppdf1 = new InputVar<double>("PPDF(1)");
            InputVar<double> ppdf2 = new InputVar<double>("PPDF(2)");
            InputVar<double> ppdf3 = new InputVar<double>("PPDF(3)");
            InputVar<double> ppdf4 = new InputVar<double>("PPDF(4)");
            InputVar<double> fcfleaf = new InputVar<double>("FCFRAC: Leaf");
            InputVar<double> btolai = new InputVar<double>("BTOLAI");
            InputVar<double> klai = new InputVar<double>("KLAI");
            InputVar<double> maxlai = new InputVar<double>("MAXLAI");
            InputVar<double> mwm = new InputVar<double>("Monthly Wood Mortality");
            InputVar<double> wdr = new InputVar<double>("Wood Decay Rate");
            InputVar<double> mortCurveShapeParm = new InputVar<double>("Mortality Curve Shape Parameter");
            InputVar<int> leafNeedleDrop = new InputVar<int>("Leaf or Needle Drop Month");

            InputVar<double> ppr2 = new InputVar<double>("PPRPTS2");
            InputVar<double> ppr3 = new InputVar<double>("PPRPTS3");
            InputVar<double> coarseRootFraction = new InputVar<double>("CRootFrac");
            InputVar<double> fineRootFraction = new InputVar<double>("FRootFrac");

            while (! AtEndOfInput && CurrentName != InitialEcoregionParameters) {
                StringReader currentLine = new StringReader(CurrentLine);

                ReadValue(ftname , currentLine);

                ReadValue(ftindex , currentLine);
                int ln = (int) ftindex.Value.Actual;

                if(ln >= numFunctionalTypes)
                    throw new InputValueException(ftindex.Value.String,
                                              "The index:  {0} exceeds the allowable number of functional groups, {1}",
                                              ftindex.Value.String, numFunctionalTypes-1);


                //IEditableFunctionalType funcTParms = new EditableFunctionalType();
                FunctionalType funcTParms = new FunctionalType();
                parameters.FunctionalTypes[ln] = funcTParms;

                ReadValue(ppdf1, currentLine);
                funcTParms.PPDF1 = ppdf1.Value;

                ReadValue(ppdf2, currentLine);
                funcTParms.PPDF2 = ppdf2.Value;

                ReadValue(ppdf3, currentLine);
                funcTParms.PPDF3 = ppdf3.Value;

                ReadValue(ppdf4, currentLine);
                funcTParms.PPDF4 = ppdf4.Value;

                ReadValue(fcfleaf, currentLine);
                funcTParms.FCFRACleaf = fcfleaf.Value;

                ReadValue(btolai, currentLine);
                funcTParms.BTOLAI = btolai.Value;

                ReadValue(klai, currentLine);
                funcTParms.KLAI = klai.Value;

                ReadValue(maxlai, currentLine);
                funcTParms.MAXLAI = maxlai.Value;

                ReadValue(ppr2, currentLine);
                funcTParms.PPRPTS2 = ppr2.Value;

                ReadValue(ppr3, currentLine);
                funcTParms.PPRPTS3 = ppr3.Value;

                ReadValue(wdr, currentLine);
                funcTParms.WoodDecayRate = wdr.Value;

                ReadValue(mwm, currentLine);
                funcTParms.MonthlyWoodMortality = mwm.Value;

                ReadValue(mortCurveShapeParm, currentLine);
                funcTParms.MortCurveShape = mortCurveShapeParm.Value;

                ReadValue(leafNeedleDrop, currentLine);
                funcTParms.LeafNeedleDrop = leafNeedleDrop.Value;

                ReadValue(coarseRootFraction, currentLine);
                funcTParms.CoarseRootFraction = coarseRootFraction.Value;

                ReadValue(fineRootFraction, currentLine);
                funcTParms.FineRootFraction = fineRootFraction.Value;

                //PlugIn.ModelCore.UI.WriteLine("PPRPTS2={0}.", parameters.FunctionalTypeTable[ln].PPRPTS2);

                CheckNoDataAfter("the " + fineRootFraction.Name + " column", currentLine);
                GetNextLine();
            }


            //--------- Read In FIRST Ecoregion Table ---------------------------
            PlugIn.ModelCore.UI.WriteLine("   Begin reading INITIAL ECOREGION parameters.");
            ReadName(InitialEcoregionParameters);

            InputVar<string> ecoregionName = new InputVar<string>("Ecoregion");
            //InputVar<double> iS1surfC = new InputVar<double>("Initial SOM1 surface C");
            InputVar<double> iS1surfN = new InputVar<double>("Initial SOM1 surface N");
            InputVar<double> iS1soilC = new InputVar<double>("Initial SOM1 soil C");
            InputVar<double> iS1soilN = new InputVar<double>("Initial SOM1 soil N");
            InputVar<double> iS2C = new InputVar<double>("Initial SOM2 (intermediate turnover) C");
            InputVar<double> iS2N = new InputVar<double>("Initial SOM2 (intermediate turnover) N");
            InputVar<double> iS3C = new InputVar<double>("Initial SOM3 (slow turnover) C");
            InputVar<double> iS3N = new InputVar<double>("Initial SOM3 (slow turnover) N");
            Dictionary <string, int> lineNumbers2 = new Dictionary<string, int>();

            while (! AtEndOfInput && CurrentName != Names.FireReductionParameters){ //Names.EcoregionParameters ) {
                StringReader currentLine = new StringReader(CurrentLine);

                ReadValue(ecoregionName, currentLine);

                IEcoregion ecoregion = GetEcoregion(ecoregionName.Value,
                                                    lineNumbers2);

                //ReadValue(iS1surfC, currentLine);
                //parameters.SetInitSOM1surfC(ecoregion, iS1surfC.Value);

                //ReadValue(iS1surfN, currentLine);
                //parameters.SetInitSOM1surfN(ecoregion, iS1surfN.Value);

                ReadValue(iS1soilC, currentLine);
                parameters.SetInitSOM1soilC(ecoregion, iS1soilC.Value);

                ReadValue(iS1soilN, currentLine);
                parameters.SetInitSOM1soilN(ecoregion, iS1soilN.Value);

                ReadValue(iS2C, currentLine);
                parameters.SetInitSOM2C(ecoregion, iS2C.Value);

                ReadValue(iS2N, currentLine);
                parameters.SetInitSOM2N(ecoregion, iS2N.Value);

                ReadValue(iS3C, currentLine);
                parameters.SetInitSOM3C(ecoregion, iS3C.Value);

                ReadValue(iS3N, currentLine);
                parameters.SetInitSOM3N(ecoregion, iS3N.Value);

                CheckNoDataAfter("the " + iS3N.Name + " column", currentLine);

                GetNextLine();
            }

            //--------- Read In SECOND Ecoregion Table ---------------------------
            // First, read table of additional parameters for ecoregions
            //PlugIn.ModelCore.UI.WriteLine("   Begin reading FIXED ECOREGION parameters.");
            //ReadName(Names.EcoregionParameters);

            //InputVar<double> pclay = new InputVar<double>("Percent Clay");
            //InputVar<double> psand = new InputVar<double>("Percent Sand");
            //InputVar<int> sd = new InputVar<int>("Soil Depth");
            //InputVar<double> fc = new InputVar<double>("Field Capacity");
            //InputVar<double> wp = new InputVar<double>("Wilting Point");
            //InputVar<double> sff = new InputVar<double>("Storm Flow Fraction");
            //InputVar<double> bff = new InputVar<double>("Base Flow Fraction");
            //InputVar<double> drain = new InputVar<double>("Drain Fraction");
            //InputVar<double> lat = new InputVar<double>("Latitude");
            //InputVar<double> denits = new InputVar<double>("Denitrification");

            //Dictionary<string, int> lineNumbers = new Dictionary<string, int>();

            //while (! AtEndOfInput && CurrentName != Names.FireReductionParameters ) {
            //    StringReader currentLine = new StringReader(CurrentLine);

            //    ReadValue(ecoregionName, currentLine);

            //    IEcoregion ecoregion = GetEcoregion(ecoregionName.Value,
            //                                        lineNumbers);

            //    //ReadValue(sd, currentLine);
            //    //parameters.SetSoilDepth(ecoregion, sd.Value);

            //    ReadValue(pclay, currentLine);
            //    parameters.SetPercentClay(ecoregion, pclay.Value);

                //ReadValue(psand, currentLine);
                //parameters.SetPercentSand(ecoregion, psand.Value);

                //ReadValue(fc, currentLine);
                //parameters.SetFieldCapacity(ecoregion, fc.Value);

                //ReadValue(wp, currentLine);
                //parameters.SetWiltingPoint(ecoregion, wp.Value);

                //ReadValue(sff, currentLine);
                //parameters.SetStormFlowFraction(ecoregion, sff.Value);

                //ReadValue(bff, currentLine);
                //parameters.SetBaseFlowFraction(ecoregion, bff.Value);

                //ReadValue(drain, currentLine);
                //parameters.SetDrain(ecoregion, drain.Value);



            //    CheckNoDataAfter("the " + pclay.Name + " column", currentLine);

            //    GetNextLine();
            //}
            //--------- Read In Fire Reductions Table ---------------------------
            PlugIn.ModelCore.UI.WriteLine("   Begin reading FIRE REDUCTION parameters.");
            ReadName(Names.FireReductionParameters);

            InputVar<int> frindex = new InputVar<int>("Fire Severity Index MUST = 1-5");
            InputVar<double> wred = new InputVar<double>("Wood Reduction");
            InputVar<double> lred = new InputVar<double>("Litter Reduction");

            while (! AtEndOfInput && CurrentName != Names.HarvestReductionParameters && CurrentName != Names.AgeOnlyDisturbanceParms)
            {
                StringReader currentLine = new StringReader(CurrentLine);

                ReadValue(frindex , currentLine);
                int ln = (int) frindex.Value.Actual;

                if(ln < 1 || ln > 5)
                    throw new InputValueException(ftindex.Value.String,
                                              "The fire severity index:  {0} must be 1-5,",
                                              frindex.Value.String);


                FireReductions inputFireReduction = new FireReductions();  // ignoring severity = zero
                parameters.FireReductionsTable[ln] = inputFireReduction;

                ReadValue(wred, currentLine);
                inputFireReduction.WoodReduction = wred.Value;

                ReadValue(lred, currentLine);
                inputFireReduction.LitterReduction = lred.Value;

                CheckNoDataAfter("the " + lred.Name + " column", currentLine);

                GetNextLine();
            }

            //--------- Read In Harvest Reductions Table ---------------------------
            InputVar<string> hreds = new InputVar<string>("HarvestReductions");
            ReadOptionalName(Names.HarvestReductionParameters);
            {
                PlugIn.ModelCore.UI.WriteLine("   Begin reading HARVEST REDUCTION parameters.");
                InputVar<string> prescriptionName = new InputVar<string>("Prescription");
                InputVar<double> wred_pr = new InputVar<double>("Wood Reduction");
                InputVar<double> lred_pr = new InputVar<double>("Litter Reduction");

                List<string> prescriptionNames = new List<string>();

                while (!AtEndOfInput && CurrentName != Names.AgeOnlyDisturbanceParms)
                {
                    HarvestReductions harvReduction = new HarvestReductions();
                    parameters.HarvestReductionsTable.Add(harvReduction);

                    StringReader currentLine = new StringReader(CurrentLine);

                    ReadValue(prescriptionName, currentLine);
                    harvReduction.PrescriptionName = prescriptionName.Value;

                    ReadValue(wred_pr, currentLine);
                    harvReduction.WoodReduction = wred.Value;

                    ReadValue(lred_pr, currentLine);
                    harvReduction.LitterReduction = lred.Value;

                    GetNextLine();
                }
            }


            return parameters; 
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Registers the appropriate method for reading input values.
        /// </summary>
        public static void RegisterForInputValues()
        {
            //Type.SetDescription<LayerType>("Litter Types");
            //InputValues.Register<LayerType>(LTParse);
            Type.SetDescription<WaterType>("Water Effect on Decomposition");
            InputValues.Register<WaterType>(WParse);

        }
        //---------------------------------------------------------------------
/*        public static LayerType LTParse(string word)
        {
            if (word == "Surface")
                return LayerType.Surface;
            else if (word == "Soil")
                return LayerType.Soil;
            throw new System.FormatException("Valid names:  Surface, Soil");
        }*/
        //---------------------------------------------------------------------
        public static WaterType WParse(string word)
        {
            if (word == "Linear")
                return WaterType.Linear;
            else if (word == "Ratio")
                return WaterType.Ratio;
            throw new System.FormatException("Valid names:  Linear, Ratio");
        }
        //---------------------------------------------------------------------

        //protected void ReadDynamicTable(List<Dynamic.ParametersUpdate> parameterUpdates)
        //{
        //    int? prevYear = null;
        //    int prevYearLineNum = 0;
        //    InputVar<int> year = new InputVar<int>("Year", Dynamic.InputValidation.ReadYear);
        //    InputVar<string> file = new InputVar<string>("Parameter File");
        //    while (! AtEndOfInput) {
        //        StringReader currentLine = new StringReader(CurrentLine);

        //        ReadValue(year, currentLine);
        //        if (prevYear.HasValue) {
        //            if (year.Value.Actual < prevYear.Value)
        //                throw new InputValueException(year.Value.String,
        //                                              "Year {0} is before year {1} which was on line {2}",
        //                                              year.Value.Actual, prevYear.Value, prevYearLineNum);
        //            if (year.Value.Actual == prevYear.Value)
        //                throw new InputValueException(year.Value.String,
        //                                              "Year {0} was already used on line {1}",
        //                                              year.Value.Actual, prevYearLineNum);
        //        }
        //        prevYear = year.Value.Actual;
        //        prevYearLineNum = LineNumber;

        //        ReadValue(file, currentLine);
        //        Dynamic.InputValidation.CheckPath(file.Value);

        //        CheckNoDataAfter("the " + file + " column", currentLine);
        //        parameterUpdates.Add(new Dynamic.ParametersUpdate(year.Value.Actual,
        //                                                                file.Value.Actual));
        //        GetNextLine();
        //    }
        //}
        //---------------------------------------------------------------------

        /// <summary>
        /// Reads a species name from the current line, and verifies the name.
        /// </summary>
        private ISpecies ReadSpecies(StringReader currentLine)
        {
            ReadValue(speciesName, currentLine);
            ISpecies species = speciesDataset[speciesName.Value.Actual];
            if (species == null)
                throw new InputValueException(speciesName.Value.String,
                                              "{0} is not a species name.",
                                              speciesName.Value.String);
            int lineNumber;
            if (speciesLineNums.TryGetValue(species.Name, out lineNumber))
                throw new InputValueException(speciesName.Value.String,
                                              "The species {0} was previously used on line {1}",
                                              speciesName.Value.String, lineNumber);
            else
                speciesLineNums[species.Name] = LineNumber;
            return species;
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Reads ecoregion names as column headings
        /// </summary>
        private List<IEcoregion> ReadEcoregions()
        {
            if (AtEndOfInput)
                throw NewParseException("Expected a line with the names of 1 or more active ecoregions.");

            InputVar<string> ecoregionName = new InputVar<string>("Ecoregion");
            List<IEcoregion> ecoregions = new List<IEcoregion>();
            StringReader currentLine = new StringReader(CurrentLine);
            TextReader.SkipWhitespace(currentLine);
            while (currentLine.Peek() != -1) {
                ReadValue(ecoregionName, currentLine);
                IEcoregion ecoregion = ecoregionDataset[ecoregionName.Value.Actual];
                if (ecoregion == null)
                    throw new InputValueException(ecoregionName.Value.String,
                                                  "{0} is not an ecoregion name.",
                                                  ecoregionName.Value.String);
                if (! ecoregion.Active)
                    throw new InputValueException(ecoregionName.Value.String,
                                                  "{0} is not an active ecoregion",
                                                  ecoregionName.Value.String);
                if (ecoregions.Contains(ecoregion))
                    throw new InputValueException(ecoregionName.Value.String,
                                                  "The ecoregion {0} appears more than once.",
                                                  ecoregionName.Value.String);
                ecoregions.Add(ecoregion);
                TextReader.SkipWhitespace(currentLine);
            }
            GetNextLine();

            return ecoregions;
        }

        //---------------------------------------------------------------------

        private IEcoregion GetEcoregion(InputValue<string>      ecoregionName,
                                        Dictionary<string, int> lineNumbers)
        {
            IEcoregion ecoregion = ecoregionDataset[ecoregionName.Actual];
            if (ecoregion == null)
                throw new InputValueException(ecoregionName.String,
                                              "{0} is not an ecoregion name.",
                                              ecoregionName.String);
            int lineNumber;
            if (lineNumbers.TryGetValue(ecoregion.Name, out lineNumber))
                throw new InputValueException(ecoregionName.String,
                                              "The ecoregion {0} was previously used on line {1}",
                                              ecoregionName.String, lineNumber);
            else
                lineNumbers[ecoregion.Name] = LineNumber;

            return ecoregion;
        }
    }
}
