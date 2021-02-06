using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;



namespace AddInGeomInPoligono
{
    internal class Module1 : Module
    {
        private static Module1 _this = null;


        internal static DockpaneGeomInPoligonoView DockpaneGeomInPoligonoView { get; set; }

        internal static DockpaneGeomInPoligonoViewModel DockpaneGeomInPoligonoViewModel { get; set; }


        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current
        {
            get
            {
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("AddInGeomInPoligono_Module"));
            }
        }

        #region Overrides
        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload()
        {
            //TODO - add your business logic
            //return false to ~cancel~ Application close
            return true;
        }

        #endregion Overrides

    }
}
