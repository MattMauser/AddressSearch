using System;
using System.Windows.Forms;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace AddressSearch
{
    public class AddressSearch : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        IFeatureClass addressFeature = null;

        public AddressSearch()
        {
        }

        public IFeatureClass GetSdeFeatureClass()
        {
            String sdeConnectionLocation = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),  "Esri\\Desktop10.7\\ArcCatalog\\rdcgis.sde");
            String featureClassName = "RDC_GIS_Database.DBO.Addresses_MunicipalAddress";
            IFeatureClass addressFeature = null;

            try
            {
                Type factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.SdeWorkspaceFactory");
                IWorkspaceFactory workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance(factoryType);
                IFeatureWorkspace destinationWorkspace = (IFeatureWorkspace)workspaceFactory.OpenFromFile(sdeConnectionLocation, 0);

                addressFeature = destinationWorkspace.OpenFeatureClass(featureClassName);

                return addressFeature;
            }
            catch (Exception exc)
            {
                throw exc;
            }

        }

        public AutoCompleteStringCollection GetUniqueAddresses()
        {
            AutoCompleteStringCollection addressCollection = new AutoCompleteStringCollection();
            ICursor cursor = (ICursor)addressFeature.Search(null, false);

            IDataStatistics dataStatistics = new DataStatistics();
            dataStatistics.Field = "Address";
            dataStatistics.Cursor = cursor;

            System.Collections.IEnumerator enumerator = dataStatistics.UniqueValues;
            enumerator.Reset();

            while (enumerator.MoveNext())
            {
                //object myObject = enumerator.Current;
                //Debug.WriteLine(myObject);
                //tempCollection.Add(myObject.ToString());
                addressCollection.Add(enumerator.Current.ToString());                
            }
            return addressCollection;
        }

        protected override void OnClick()
        {
            IMxDocument mxd = ArcMap.Application.Document as IMxDocument;
            IMap map = (IMap)mxd.FocusMap;
            IActiveView activeView = mxd.ActiveView;

            String AddressString = "";
 
            if (addressFeature is null)
            {
                addressFeature = GetSdeFeatureClass();
            }

            AutoCompleteStringCollection uniqueAddresses = GetUniqueAddresses();

            using (var form = new Form1(uniqueAddresses)) //Creating the form and returning the result
            {
                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    AddressString = form.ReturnValue1;
                }
            }


            IQueryFilter queryFilter = new QueryFilter();
            queryFilter.WhereClause = String.Format("Address = '{0}'", AddressString); //To be changed to UserInput

            IFeatureCursor addressCursor = addressFeature.Search(queryFilter, true);
            IFeature feature = addressCursor.NextFeature();
            IEnvelope tempEnvelope;

            while (feature != null)
            {
                tempEnvelope = activeView.Extent;
                IPoint point = (IPoint)feature.Shape;
                tempEnvelope.CenterAt(point);
                activeView.Extent = tempEnvelope;
                map.MapScale = 1000; //Could Change this to a User Variable

                activeView.Refresh();

                feature = addressCursor.NextFeature();
            }
            //ArcMap.Application.CurrentTool = null;
        }

        protected override void OnUpdate()
        {
            Enabled = ArcMap.Application != null;
        }
    }
}
