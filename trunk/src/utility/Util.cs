//  Copyright 2007-2016 Portland State University
//  Author: Robert Scheller

using Landis.Core;
using Landis.SpatialModeling;
using Landis.Library.LeafBiomassCohorts;
using Edu.Wisc.Forest.Flel.Util;
using System.IO;

namespace Landis.Extension.Succession.Century
{
    /// <summary>
    /// Utility methods.
    /// </summary>
    public static class Util
    {

        /// <summary>
        /// Converts a table indexed by species and ecoregion into a
        /// 2-dimensional array.
        /// </summary>
        public static T[,] ToArray<T>(Species.AuxParm<Ecoregions.AuxParm<T>> table)
        {
            T[,] array = new T[PlugIn.ModelCore.Ecoregions.Count, PlugIn.ModelCore.Species.Count];
            foreach (ISpecies species in PlugIn.ModelCore.Species) {
                foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions) {
                    array[ecoregion.Index, species.Index] = table[species][ecoregion];
                }
            }
            return array;
        }
        //---------------------------------------------------------------------

        public static void ReadSoilDepthMap(string path)
        {
            IInputRaster<IntPixel> map;

            try
            {
                map = PlugIn.ModelCore.OpenRaster<IntPixel>(path);
            }
            catch (FileNotFoundException)
            {
                string mesg = string.Format("Error: The file {0} does not exist", path);
                throw new System.ApplicationException(mesg);
            }

            if (map.Dimensions != PlugIn.ModelCore.Landscape.Dimensions)
            {
                string mesg = string.Format("Error: The input map {0} does not have the same dimension (row, column) as the ecoregions map", path);
                throw new System.ApplicationException(mesg);
            }

            using (map)
            {
                IntPixel pixel = map.BufferPixel;
                foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                {
                    map.ReadBufferPixel();
                    int mapValue = pixel.MapCode.Value;
                    if (site.IsActive)
                    {
                        //if (Dataset == null)
                        //    PlugIn.ModelCore.UI.WriteLine("FireRegion.Dataset not set correctly.");
                        //IFireRegion ecoregion = Find(mapCode);

                        //if (ecoregion == null)
                        //{
                        //    string mesg = string.Format("mapCode = {0}, dimensions.rows = {1}", mapCode, map.Dimensions.Rows);
                        //    throw new System.ApplicationException(mesg);
                        //}
                        //if (mapValue != null)
                        //{
                            if (mapValue < 1 || mapValue > 200)
                                throw new InputValueException(mapValue.ToString(),
                                                              "{0} is not between {1:0.0} and {2:0.0}",
                                                              mapValue, 1, 200);
                        //}

                        SiteVars.SoilDepth[site] = mapValue;
                    }
                }
            }
        }
    }
}
