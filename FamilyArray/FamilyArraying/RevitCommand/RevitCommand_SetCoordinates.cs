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
    [Regeneration(RegenerationOption.Manual)]
    public class RevitCommand_SetCoordinates : IExternalCommand
    {
        private Data Data => Data.Instance;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Data.Instance.InitData(commandData);

            try
            {
                IList<Reference> refs = Data.Sel.PickObjects(ObjectType.Element, "Chọn nhiều đối tượng");
                if (refs == null || refs.Count == 0)
                {
                    TaskDialog.Show("Thông báo", "Không có đối tượng nào được chọn.");
                    return Result.Cancelled;
                }

                var viewModel = new SetCoordinatesVM();

                List<HashSet<string>> allParamNames = new List<HashSet<string>>();
                foreach (var r in refs)
                {
                    HashSet<string> paramNames = new HashSet<string>();
                    Element e = Data.Instance.Doc.GetElement(r);
                    Group parentGroup = Data.Instance.Doc.GetElement(e.GroupId) as Group;
                    if (parentGroup != null)
                    {
                        continue;
                    }

                    foreach (Parameter param in e.Parameters)
                    {
                        if (!param.IsReadOnly)
                        {
                            paramNames.Add(param.Definition.Name);
                        }
                    }

                    if (paramNames.Count == 0) continue;

                    allParamNames.Add(paramNames);
                    viewModel.SelectedElements.Add(e);
                }

                HashSet<string> intersection = new HashSet<string>();
                intersection = allParamNames[0];
                for (int i = 1; i < allParamNames.Count; i++)
                {
                    intersection.IntersectWith(allParamNames[i]);
                }

                viewModel.Parameters = intersection.ToObservableCollection();
                if (viewModel.Parameters.Count < 2)
                {
                    TaskDialog.Show("Thông báo", "Có ít hơn 2 parameters");
                    return Result.Cancelled;
                }
                viewModel.SelectedParameter1 = viewModel.Parameters.FirstOrDefault(x => x == "Comments") ?? viewModel.Parameters.FirstOrDefault();
                viewModel.SelectedParameter2 = viewModel.Parameters.FirstOrDefault(x => x == "Position") ?? viewModel.Parameters[1];

                var view = new SetCoordinatesView() { DataContext = viewModel };
                view.ShowDialog();
            }
            catch (Exception)
            {
            }
            return Result.Succeeded;
        }
    }
}
