using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.UI;
using System.Windows;
using System.Windows.Input;
using SingleData;
using FamilyArraying.ViewModel;
using FamilyArraying.View;
using System.Windows.Controls;

namespace HTAddin
{
    [Transaction(TransactionMode.Manual)]
    public class RevitCommand : IExternalCommand
    {
        private Data Data => Data.Instance;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Data.Instance.InitData(commandData);

            try
            {
                IList<Reference> pickedRefs = Data.Sel.PickObjects(
                               ObjectType.Element,
                               new CurveSelectionFilter(),
                               "Select Curves (Line, Arc...)"
                           );

                List<Curve> curves = new List<Curve>();

                foreach (Reference reference in pickedRefs)
                {
                    Element elem = Data.Doc.GetElement(reference);

                    if (elem is CurveElement curveElem)
                    {
                        curves.Add(curveElem.GeometryCurve);
                    }
                }

                var viewModel = new FamilyArrayVM(curves);
                var view = new MainWindow() { DataContext = viewModel };
                view.ShowDialog();

            }
            catch (Exception)
            {

                throw;
            }
            return Result.Succeeded;
        }
    }
}
