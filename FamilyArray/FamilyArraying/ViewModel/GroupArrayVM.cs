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

namespace FamilyArraying.ViewModel
{
    public class GroupArrayVM : BaseViewModel
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
        public ICommand OkCmd { get; set; }
        public ICommand CancelCmd { get; set; }
        public ICommand AddCmd { get; set; }
        public ICommand DeleteCmd { get; set; }
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

        public void ArrayGroupAlongArc(Document doc, Arc arc, Group group, double stepLength, bool isFlip, ArraySpreadDirection spreadType, Action onProgress)
        {
            double curveLength = arc.Length;
            int count = (int)Math.Ceiling(curveLength / stepLength);

            List<XYZ> placementPoints = new List<XYZ>();

            switch (spreadType)
            {
                case ArraySpreadDirection.StartToEnd:
                    placementPoints.Add(arc.Evaluate(0.0, true));

                    for (int i = 1; i <= count; i++)
                    {
                        double currentLength = i * stepLength;
                        if (currentLength > curveLength) break;


                        double parameter = currentLength / curveLength;
                        if (parameter > 1.0) parameter = 1.0;

                        placementPoints.Add(arc.Evaluate(parameter, true));
                    }
                    break;
                case ArraySpreadDirection.EndToStart:
                    placementPoints.Add(arc.Evaluate(1.0, true));

                    for (int i = 1; i < count; i++)
                    {
                        double currentLength = i * stepLength;
                        if (currentLength > curveLength) break;

                        double parameter = 1.0 - (currentLength / curveLength);
                        if (parameter < 0.0) parameter = 0.0;

                        placementPoints.Add(arc.Evaluate(parameter, true));
                    }
                    break;
                case ArraySpreadDirection.MiddleOutward:
                    double halfLength = curveLength / 2.0;
                    placementPoints.Add(arc.Evaluate(0.5, true)); // Điểm giữa cung

                    for (int i = 1; i <= count / 2; i++) // Cặp đối xứng
                    {
                        double offset = i * stepLength;
                        if (offset > halfLength) break;

                        double param1 = (halfLength - offset) / curveLength;
                        double param2 = (halfLength + offset) / curveLength;

                        if (param1 >= 0.0)
                            placementPoints.Add(arc.Evaluate(param1, true));
                        if (param2 <= 1.0)
                            placementPoints.Add(arc.Evaluate(param2, true));
                    }
                    break;
            }


            foreach (XYZ pt in placementPoints)
            {
                Group placedGroup = doc.Create.PlaceGroup(pt, group.GroupType);

                Line axis = Line.CreateBound(pt, pt + XYZ.BasisZ);
                var centertoLocalVector = pt - arc.Center;
                var localFamilyVector = XYZ.BasisY;
                var rotateAngle = localFamilyVector.AngleTo(centertoLocalVector);
                double crossZ = localFamilyVector.CrossProduct(centertoLocalVector).Z;
                if (crossZ < 0) rotateAngle = -rotateAngle;
                rotateAngle = isFlip ? rotateAngle + Math.PI : rotateAngle;
                ElementTransformUtils.RotateElement(Data.Instance.Doc, placedGroup.Id, axis, rotateAngle);
                onProgress?.Invoke();
            }
        }

        public void ArrayGroupAlongLine(Document doc, Line line, Group group, double stepLength, bool isFlip, ArraySpreadDirection spreadType, Action onProgress)
        {
            double lineLength = line.Length;

            List<XYZ> placementPoints = new List<XYZ>();
            int count = (int)Math.Ceiling(lineLength / stepLength);

            XYZ lineDirection = (line.GetEndPoint(1) - line.GetEndPoint(0)).Normalize();

            var localFamilyVector = XYZ.BasisX;
            var rotateAngle = localFamilyVector.AngleTo(lineDirection);
            double crossZ = localFamilyVector.CrossProduct(lineDirection).Z;
            if (crossZ < 0) rotateAngle = -rotateAngle;
            rotateAngle = isFlip ? rotateAngle + Math.PI : rotateAngle;

            switch (spreadType)
            {
                case ArraySpreadDirection.StartToEnd:
                    for (int i = 0; i < count; i++)
                    {
                        double currentLength = i * stepLength;
                        if (currentLength > lineLength + 0.0001)
                            break;

                        XYZ pointOnLine = line.Evaluate(currentLength / lineLength, true);
                        placementPoints.Add(pointOnLine);
                    }
                    break;

                case ArraySpreadDirection.EndToStart:
                    for (int i = 0; i < count; i++)
                    {
                        double currentLength = i * stepLength;
                        if (currentLength >= lineLength + 0.0001)
                            break;

                        double parameter = 1.0 - (currentLength / lineLength);
                        XYZ pointOnLine = line.Evaluate(parameter, true);
                        placementPoints.Add(pointOnLine);
                    }
                    break;

                case ArraySpreadDirection.MiddleOutward:
                    double halfLength = lineLength / 2.0;
                    placementPoints.Add(line.Evaluate(0.5, true)); // Điểm giữa

                    for (int i = 1; i <= count / 2; i++)
                    {
                        double offset = i * stepLength;
                        if (offset > halfLength + 0.0001)
                            break;

                        double param1 = (halfLength - offset) / lineLength;
                        double param2 = (halfLength + offset) / lineLength;

                        if (param1 >= 0.0)
                            placementPoints.Add(line.Evaluate(param1, true));
                        if (param2 <= 1.0)
                            placementPoints.Add(line.Evaluate(param2, true));
                    }
                    break;
            }

            foreach (var location in placementPoints)
            {
                Group placedGroup = doc.Create.PlaceGroup(location, group.GroupType);

                if (placedGroup != null)
                {
                    Line axisLine = Line.CreateBound(location, location + XYZ.BasisZ);
                    ElementTransformUtils.RotateElement(doc, placedGroup.Id, axisLine, rotateAngle);
                    onProgress?.Invoke();
                }
            }
        }

        public void ArrayGroupAlongSpline(Document doc, NurbSpline spline, Group group, double stepLength, bool isFlip, ArraySpreadDirection spreadType, Action onProgress)
        {
            double curveLength = spline.ApproximateLength;
            int count = (int)Math.Ceiling(curveLength / stepLength);
            List<XYZ> placementPoints = new List<XYZ>();

            switch (spreadType)
            {
                case ArraySpreadDirection.StartToEnd:

                    placementPoints = DivideCurveByFixedLength(spline, stepLength);

                    //for (double d = 0; d <= curveLength; d += stepLength)
                    //{
                    //    double param = spline.ComputeNormalizedParameter(d);
                    //    XYZ pt = spline.Evaluate(param, false);
                    //    placementPoints.Add(pt);
                    //}


                    //for (int i = 0; i < count; i++)
                    //{
                    //    double currentLength = i * stepLength;
                    //    if (currentLength > curveLength) break;

                    //    double param = currentLength / curveLength;
                    //    param = Math.Min(param, 1.0);
                    //    placementPoints.Add(spline.Evaluate(param, true));
                    //}
                    break;

                case ArraySpreadDirection.EndToStart:
                    placementPoints = DivideCurveByFixedLengthBackward(spline, stepLength);
                    break;

                case ArraySpreadDirection.MiddleOutward:
                    double halfLength = curveLength / 2.0;
                    placementPoints.Add(spline.Evaluate(0.5, true)); // chính giữa

                    for (int i = 1; i < count / 2 + 1; i++) // đối xứng
                    {
                        double offset = i * stepLength;
                        if (offset > halfLength) break;

                        double param1 = (halfLength - offset) / curveLength;
                        double param2 = (halfLength + offset) / curveLength;

                        if (param1 >= 0.0)
                            placementPoints.Add(spline.Evaluate(param1, true));
                        if (param2 <= 1.0)
                            placementPoints.Add(spline.Evaluate(param2, true));
                    }
                    break;
            }

            List<XYZ> points = DivideCurveByFixedLengthBackward(spline, stepLength);

            for (int i = 1; i < points.Count; i++)
            {
                if (points[i].DistanceTo(points[i - 1]) > 1)
                {
                    Util.CreateModelLine(Data.Instance.Doc, points[i - 1], points[i]);
                }
            }

            foreach (XYZ pt in placementPoints)
            {
                Group placedGroup = doc.Create.PlaceGroup(pt, group.GroupType);
                if (placedGroup == null) continue;

                var angle = Util.ComputeAngleFromTangent(spline, pt);

                //// Trục quay (dọc Z)
                Line axis = Line.CreateBound(pt, pt + XYZ.BasisZ);
                double rotateAngle = isFlip ? -angle : angle;

                ElementTransformUtils.RotateElement(doc, placedGroup.Id, axis, rotateAngle);
                onProgress?.Invoke();
            }
        }

        private List<XYZ> DivideCurveByFixedLength(Curve curve, double segmentLength)
        {
            int samples = 1000;
            List<double> cumulativeLengths = new List<double>();
            List<double> parameters = new List<double>();

            double totalLength = 0;
            XYZ prev = curve.Evaluate(0, true);
            cumulativeLengths.Add(0);
            parameters.Add(0);

            for (int i = 1; i <= samples; i++)
            {
                double t = (double)i / samples;
                XYZ pt = curve.Evaluate(t, true);
                totalLength += pt.DistanceTo(prev);

                cumulativeLengths.Add(totalLength);
                parameters.Add(t);
                prev = pt;
            }

            List<XYZ> result = new List<XYZ>();
            double currentLength = 0;

            while (currentLength <= totalLength)
            {
                // tìm parameter tương ứng với currentLength
                for (int j = 1; j < cumulativeLengths.Count; j++)
                {
                    if (cumulativeLengths[j] >= currentLength)
                    {
                        double len0 = cumulativeLengths[j - 1];
                        double len1 = cumulativeLengths[j];
                        double t0 = parameters[j - 1];
                        double t1 = parameters[j];

                        double ratio = (currentLength - len0) / (len1 - len0);
                        double t = t0 + ratio * (t1 - t0);

                        XYZ pt = curve.Evaluate(t, true);
                        result.Add(pt);
                        break;
                    }
                }

                currentLength += segmentLength;
            }

            return result;
        }

        private List<XYZ> DivideCurveByFixedLengthBackward(Curve curve, double segmentLength)
        {
            int samples = 1000;
            List<double> cumulativeLengths = new List<double>();
            List<double> parameters = new List<double>();

            double totalLength = 0;
            XYZ prev = curve.Evaluate(0, true);
            cumulativeLengths.Add(0);
            parameters.Add(0);

            for (int i = 1; i <= samples; i++)
            {
                double t = (double)i / samples;
                XYZ pt = curve.Evaluate(t, true);
                totalLength += pt.DistanceTo(prev);

                cumulativeLengths.Add(totalLength);
                parameters.Add(t);
                prev = pt;
            }

            List<XYZ> result = new List<XYZ>();
            double currentLength = totalLength;

            while (currentLength >= 0)
            {
                for (int j = 1; j < cumulativeLengths.Count; j++)
                {
                    if (cumulativeLengths[j] >= currentLength)
                    {
                        double len0 = cumulativeLengths[j - 1];
                        double len1 = cumulativeLengths[j];
                        double t0 = parameters[j - 1];
                        double t1 = parameters[j];

                        double ratio = (currentLength - len0) / (len1 - len0);
                        double t = t0 + ratio * (t1 - t0);

                        XYZ pt = curve.Evaluate(t, true);
                        result.Add(pt);
                        break;
                    }
                }

                currentLength -= segmentLength;
            }

            return result;
        }

        public void BtnOkeCommand(object window)
        {
            (window as Window).Close();

            try
            {
                int count = 0;
                foreach (var curveItem in Curves)
                {
                    count += (int)Math.Ceiling(curveItem.Curve.Length / curveItem.Spacing);
                }

                using (var progressBarView = new ProgressBarView($"Group Array {count}"))
                {
                    using (TransactionGroup transactionGroup = new TransactionGroup(Data.Instance.Doc))
                    {
                        transactionGroup.Start("Group Array");

                        progressBarView.Run(Curves, (curveItem) =>
                        {
                            using (Transaction transaction = new Transaction(Data.Instance.Doc))
                            {
                                transaction.Start("Group Array");
                                var curve = curveItem.Curve;
                                var distance = curveItem.Spacing.MmToFeet();
                                var isFlip = (int)curveItem.SelectedFipDirection.FlipDirection == 1;
                                var spreadType = curveItem.SelectedArraySpreadDirection.ArraySpreadDirection;

                                if (curve is Arc arc)
                                {
                                    ArrayGroupAlongArc(Data.Instance.Doc, arc, curveItem.Group, distance, isFlip, spreadType, () => progressBarView.Increase());
                                }
                                else if (curve is Line line)
                                {
                                    ArrayGroupAlongLine(Data.Instance.Doc, line, curveItem.Group, distance, isFlip, spreadType, () => progressBarView.Increase());
                                }
                                else if (curve is NurbSpline spline)
                                {
                                    ArrayGroupAlongSpline(Data.Instance.Doc, spline, curveItem.Group, distance, isFlip, spreadType, () => progressBarView.Increase());
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

                //foreach (var curveItem in Curves)
                //{
                //    var curve = curveItem.Curve;
                //    var distance = curveItem.Spacing.MmToFeet();
                //    var isFlip = (int)curveItem.SelectedFipDirection.FlipDirection == 1;
                //    var spreadType = curveItem.SelectedArraySpreadDirection.ArraySpreadDirection;

                //    var symbol = curveItem.FamilyInfor.SelectedFamilySymbol;
                //    if (!symbol.IsActive)
                //        symbol.Activate();

                //    if (curve is Arc arc)
                //    {
                //        ArrayFamilyAlongArc(Data.Instance.Doc, arc, symbol, distance, isFlip, spreadType);
                //    }
                //    else if (curve is Line line)
                //    {
                //        ArrayFamilyAlongLine(Data.Instance.Doc, line, symbol, distance, isFlip, spreadType);
                //    }
                //}
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

        public void AddCommand(object window)
        {
            try
            {
                (window as Window).Hide();
                IList<Reference> pickedRefs = Data.Instance.Sel.PickObjects(
                               ObjectType.Element,
                               new CurveSelectionFilter(),
                               "Select more Curves (Line, Arc...)"
                           );


                var existingCurves = new List<Curve>();
                foreach (Reference reference in pickedRefs)
                {
                    Element elem = Data.Instance.Doc.GetElement(reference);

                    if (elem is CurveElement curveElem)
                    {
                        if (curveElem.GeometryCurve is Arc newArc)
                        {
                            var arcList = Curves.Where(x => x.Curve is Arc).Select(x => x.Curve as Arc).ToList();
                            bool exists = arcList.Any(arc =>
                            arc.Center.IsAlmostEqualTo(newArc.Center)
                            && arc.Radius.Equals(newArc.Radius)
                            && arc.GetEndPoint(0).IsAlmostEqualTo(newArc.GetEndPoint(0))
                            && arc.GetEndPoint(1).IsAlmostEqualTo(newArc.GetEndPoint(1)));

                            if (exists)
                            {
                                //MessageBox.Show("This curve has been previously selected.", "Family Array", MessageBoxButton.OK, MessageBoxImage.Warning);
                                existingCurves.Add(newArc);
                                continue;
                            }
                            Curves.Add(new CurveModel(curveElem.GeometryCurve));
                        }
                        else if (curveElem.GeometryCurve is Line newLine)
                        {
                            var lineList = Curves.Where(x => x.Curve is Line).Select(x => x.Curve as Line).ToList();
                            bool exists = lineList.Any(line =>
                            (line.GetEndPoint(0).IsAlmostEqualTo(newLine.GetEndPoint(0)) &&
                            line.GetEndPoint(1).IsAlmostEqualTo(newLine.GetEndPoint(1)))
                            ||
                            (line.GetEndPoint(0).IsAlmostEqualTo(newLine.GetEndPoint(1)) &&
                            line.GetEndPoint(1).IsAlmostEqualTo(newLine.GetEndPoint(0))));

                            if (exists)
                            {
                                existingCurves.Add(newLine);
                                continue;
                            }
                            Curves.Add(new CurveModel(curveElem.GeometryCurve));
                        }
                    }
                }

                if (existingCurves.Count == 1)
                {
                    MessageBox.Show($"Has 1 curve previously selected!", "Family Array", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else if (existingCurves.Count > 1)
                {
                    MessageBox.Show($"Has {existingCurves.Count} curves previously selected!", "Family Array", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                (window as Window).ShowDialog();
            }
        }
        public void DeleteCommand(object window)
        {
            if (SelectedCurve != null)
            {
                Curves.Remove(SelectedCurve);
            }
            SelectedCurve = Curves.LastOrDefault();
        }

        public GroupArrayVM(List<Curve> listCurve, Group groupInput)
        {
            Curves = new ObservableCollection<CurveModel>();
            foreach (var curve in listCurve)
            {
                Curves.Add(new CurveModel(curve) { Group = groupInput });
            }

            OkCmd = new RelayCommand(BtnOkeCommand);
            CancelCmd = new RelayCommand(BtnCancelCommand);
            AddCmd = new RelayCommand(AddCommand);
            DeleteCmd = new RelayCommand(DeleteCommand);
        }
    }
}
