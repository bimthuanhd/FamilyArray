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

namespace FamilyArraying.ViewModel
{
    public class FamilyArrayVM : BaseViewModel
    {
        private ObservableCollection<CurveModel> curves;
        public ObservableCollection<CurveModel> Curves
        {
            get => curves;
            set
            {
                curves = value;
                OnPropertyChanged(nameof(curves));
            }
        }

        private CurveModel selectedCurve;
        public CurveModel SelectedCurve
        {
            get => selectedCurve;
            set
            {
                selectedCurve = value;
                OnPropertyChanged(nameof(selectedCurve));
            }
        }

        #region Command
        public ICommand OkCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        #endregion

        private double GetParameterAtDistance(Curve curve, double distance)
        {
            double total = 0;
            double step = 0.001;
            for (double param = curve.GetEndParameter(0); param < curve.GetEndParameter(1); param += step)
            {
                XYZ p1 = curve.Evaluate(param, false);
                XYZ p2 = curve.Evaluate(param + step, false);
                total += p1.DistanceTo(p2);
                if (total >= distance)
                    return param;
            }
            return curve.GetEndParameter(1);
        }

        public void ArrayFamilyAlongArc(Document doc, Arc arc, FamilySymbol symbol, double stepLength, bool isFlip)
        {
            double curveLength = arc.Length;
            int count = (int)(curveLength / stepLength);
            List<XYZ> placementPoints = new List<XYZ>();
            placementPoints.Add(arc.Evaluate(0.0, true));

            for (int i = 1; i <= count; i++)
            {
                double currentLength = i * stepLength;
                if (currentLength > curveLength) break;


                double parameter = currentLength / curveLength;
                if (parameter > 1.0) parameter = 1.0;

                placementPoints.Add(arc.Evaluate(parameter, true));
            }

            List<FamilyInstance> createdInstances = new List<FamilyInstance>();
            foreach (XYZ pt in placementPoints)
            {
                FamilyInstance newInstance = Data.Instance.Doc.Create.NewFamilyInstance(pt, symbol, Data.Instance.Doc.ActiveView, StructuralType.NonStructural);

                Line axis = Line.CreateBound(pt, pt + XYZ.BasisZ);
                var centertoLocalVector = pt - arc.Center;
                var localFamilyVector = XYZ.BasisY;
                var rotateAngle = localFamilyVector.AngleTo(centertoLocalVector);
                double crossZ = localFamilyVector.CrossProduct(centertoLocalVector).Z;
                if (crossZ < 0) rotateAngle = -rotateAngle;
                rotateAngle = isFlip ? rotateAngle + Math.PI : rotateAngle;
                ElementTransformUtils.RotateElement(Data.Instance.Doc, newInstance.Id, axis, rotateAngle);

                createdInstances.Add(newInstance);
            }
        }

        public void ArrayFamilyAlongLine(Document doc, Line line, FamilySymbol symbol, double stepLength, bool isFlip)
        {
            double lineLength = line.Length;

            List<XYZ> placementPoints = new List<XYZ>();
            int numberOfInstances = (int)Math.Floor(lineLength / stepLength);

            XYZ lineDirection = (line.GetEndPoint(1) - line.GetEndPoint(0)).Normalize();

            var localFamilyVector = XYZ.BasisX;
            var rotateAngle = localFamilyVector.AngleTo(lineDirection);
            double crossZ = localFamilyVector.CrossProduct(lineDirection).Z;
            if (crossZ < 0) rotateAngle = -rotateAngle;
            rotateAngle = isFlip ? rotateAngle + Math.PI : rotateAngle;

            for (int i = 0; i <= numberOfInstances; i++)
            {
                double currentLength = i * stepLength;
                if (currentLength > lineLength + 0.0001)
                    break;

                XYZ pointOnLine = line.Evaluate(currentLength / lineLength, true);


                placementPoints.Add(pointOnLine);
            }

            List<FamilyInstance> createdInstances = new List<FamilyInstance>();
            foreach (var location in placementPoints)
            {
                FamilyInstance newInstance = doc.Create.NewFamilyInstance(location, symbol, Data.Instance.Doc.ActiveView, StructuralType.NonStructural);

                if (newInstance != null)
                {
                    Line axisLine = Line.CreateBound(location, location + XYZ.BasisZ);
                    ElementTransformUtils.RotateElement(doc, newInstance.Id, axisLine, rotateAngle);
                }
                createdInstances.Add(newInstance);
            }
        }

        public void BtnOkeCommand(object window)
        {
            (window as Window).Close();

            using (Transaction trans = new Transaction(Data.Instance.Doc, "Array Family Instances"))
            {
                trans.Start();

                try
                {
                    foreach (var curveItem in Curves)
                    {
                        var curve = curveItem.Curve;
                        var distance = curveItem.Spacing.MmToFeet();
                        var isFlip = (int)curveItem.SelectedFipDirection.FlipDirection == 1;

                        var symbol = curveItem.FamilyInfor.SelectedFamilySymbol;
                        if (!symbol.IsActive)
                            symbol.Activate();

                        if (curve is Arc arc)
                        {
                            ArrayFamilyAlongArc(Data.Instance.Doc, arc, symbol, distance, isFlip);
                        }
                        else if (curve is Line line)
                        {
                            ArrayFamilyAlongLine(Data.Instance.Doc, line, symbol, distance, isFlip);
                        }
                    }
                }
                catch (Exception e)
                {
                    throw;
                }

                trans.Commit();
            }
        }
        public void BtnCancelCommand(object window)
        {
            (window as Window).Close();
        }

        public FamilyArrayVM(List<Curve> listCurve)
        {
            Curves = new ObservableCollection<CurveModel>();
            foreach (var curve in listCurve)
            {
                Curves.Add(new CurveModel(curve));
            }

            OkCommand = new RelayCommand(BtnOkeCommand);
            CancelCommand = new RelayCommand(BtnCancelCommand);
        }

    }
}
