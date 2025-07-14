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

namespace HTAddin
{
    [Transaction(TransactionMode.Manual)]
    public class RevitCommand : IExternalCommand
    {
        private Data Data => Data.Instance;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Data.Instance.InitData(commandData);

            var curveRow1 = new CurveModel();

            var viewModel = new FamilyArrayVM();
            var view = new MainWindow() { DataContext = viewModel };
            view.ShowDialog();

            return Result.Succeeded;
        }
    }
}
