using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using HTAddin;
using Microsoft.SqlServer.Server;
using SingleData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LinearArray = Autodesk.Revit.DB.LinearArray;
using RadialArray = Autodesk.Revit.DB.RadialArray;
using Frame = Autodesk.Revit.DB.Frame;
using static Autodesk.Revit.DB.SpecTypeId;
using Autodesk.Revit.UI;
using System.Reflection.Emit;
using FamilyArraying.Enum;
using Autodesk.Revit.UI.Selection;
using FamilyArraying.View;
using Reference = Autodesk.Revit.DB.Reference;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace FamilyArraying.ViewModel
{
    public class SetCoordinatesVM : BaseViewModel
    {
        private ObservableCollection<string> parameters;
        public ObservableCollection<string> Parameters
        {
            get => parameters;
            set
            {
                parameters = value;
                OnPropertyChanged(nameof(parameters));
            }
        }

        private string selectedParameter1;
        public string SelectedParameter1
        {
            get => selectedParameter1;
            set
            {
                selectedParameter1 = value;
                OnPropertyChanged(nameof(selectedParameter1));
            }
        }

        private string selectedParameter2;
        public string SelectedParameter2
        {
            get => selectedParameter2;
            set
            {
                selectedParameter2 = value;
                OnPropertyChanged(nameof(selectedParameter2));
            }
        }

        private List<Element> selectedElements;
        public List<Element> SelectedElements
        {
            get => selectedElements ?? (selectedElements = new List<Element>());
            set
            {
                selectedElements = value;
                OnPropertyChanged(nameof(selectedElements));
            }
        }

        #region Command
        public ICommand OkCmd { get; set; }
        public ICommand CancelCmd { get; set; }
        #endregion

        public void BtnOkeCommand(object window)
        {
            (window as Window).Close();

            try
            {
                // check 2 Cbbs must be different together
                if (SelectedParameter1 == SelectedParameter2)
                {
                    TaskDialog.Show("Revit", "2 parameters must be different!");
                    return;
                }

                //using (Transaction tx = new Transaction(Data.Instance.Doc, "Set Coordinates"))
                //{
                //    tx.Start();
                //    foreach (var element in selectedElements)
                //    {
                //        if (element.Location is LocationPoint locationPoint)
                //        {
                //            XYZ sharedPoint = Data.Instance.Doc.ActiveProjectLocation.GetTotalTransform().OfPoint(locationPoint.Point);
                //            Parameter paramX = element.LookupParameter(SelectedParameter1);
                //            Parameter paramY = element.LookupParameter(SelectedParameter2);

                //            double xInMeters = UnitUtils.ConvertFromInternalUnits(sharedPoint.X, UnitTypeId.Meters);
                //            double yInMeters = UnitUtils.ConvertFromInternalUnits(sharedPoint.Y, UnitTypeId.Meters);

                //            if (paramX != null && !paramX.IsReadOnly)
                //            {
                //                paramX.Set(xInMeters.ToString());
                //            }

                //            if (paramY != null && !paramY.IsReadOnly)
                //            {
                //                paramY.Set(yInMeters.ToString());
                //            }
                //        }
                //    }

                //    tx.Commit();
                //}

                using (var progressBarView = new ProgressBarView($"Set Coordinates {selectedElements.Count}"))
                {
                    using (TransactionGroup transactionGroup = new TransactionGroup(Data.Instance.Doc))
                    {
                        transactionGroup.Start("Group Set Coordinates");

                        progressBarView.Run(selectedElements, (element) =>
                        {
                            using (Transaction transaction = new Transaction(Data.Instance.Doc))
                            {
                                transaction.Start("Group Set Coordinates");
                                if (element.Location is LocationPoint locationPoint)
                                {
                                    XYZ sharedPoint = Data.Instance.Doc.ActiveProjectLocation.GetTotalTransform().OfPoint(locationPoint.Point);
                                    Parameter paramX = element.LookupParameter(SelectedParameter1);
                                    Parameter paramY = element.LookupParameter(SelectedParameter2);

                                    double xInMeters = UnitUtils.ConvertFromInternalUnits(sharedPoint.X, UnitTypeId.Meters);
                                    double yInMeters = UnitUtils.ConvertFromInternalUnits(sharedPoint.Y, UnitTypeId.Meters);

                                    if (paramX != null && !paramX.IsReadOnly)
                                    {
                                        paramX.Set(xInMeters.ToString());
                                    }

                                    if (paramY != null && !paramY.IsReadOnly)
                                    {
                                        paramY.Set(yInMeters.ToString());
                                    }
                                }
                                transaction.Commit();
                            }
                        });

                        if (progressBarView.IsClosed)
                            transactionGroup.RollBack();
                        else
                            transactionGroup.Assimilate();
                    }
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }
        public void BtnCancelCommand(object window)
        {
            (window as Window).Close();
        }

        public SetCoordinatesVM()
        {
            OkCmd = new RelayCommand(BtnOkeCommand);
            CancelCmd = new RelayCommand(BtnCancelCommand);
        }
    }
}
