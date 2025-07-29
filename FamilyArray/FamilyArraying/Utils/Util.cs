using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.IO;
using System.Windows;
using Line = Autodesk.Revit.DB.Line;
using System.Collections.ObjectModel;
using SingleData;

namespace HTAddin
{
    public static class Util
    {
        public static bool IsColumnSideFace(Face face, FamilyInstance column)
        {
            XYZ faceNormal = face.ComputeNormal(new UV());
            XYZ columnDirection = column.HandOrientation;
            double dotProduct = faceNormal.DotProduct(columnDirection);

            if (dotProduct > 0)
            {
                return true;
            }

            return false;
        }

        public static bool IsPointInsideGeometryElement(XYZ point, Element element)
        {
            Element columnElement = element;

            Options options = new Options();
            GeometryElement geometryElement = columnElement.get_Geometry(options);

            foreach (GeometryObject geometryObject in geometryElement)
            {
                Solid solid = geometryObject as Solid;
                if (solid != null && solid.Volume > 0)
                {
                    SolidCurveIntersectionOptions sco = new SolidCurveIntersectionOptions();
                    sco.ResultType = SolidCurveIntersectionMode.CurveSegmentsInside;

                    Line line = Line.CreateBound(point, point.Add(XYZ.BasisX));

                    double tolerance = 0.000001;

                    SolidCurveIntersection sci = solid.IntersectWithCurve(line, sco);

                    for (int i = 0; i < sci.SegmentCount; i++)
                    {
                        Curve c = sci.GetCurveSegment(i);

                        if (point.IsAlmostEqualTo(c.GetEndPoint(0), tolerance) || point.IsAlmostEqualTo(c.GetEndPoint(1), tolerance))
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }

            return false;
        }
        public static void SetLayoutRule_MaximumSpacing(Rebar rebar, double spacing)
        {
            spacing = MmToFeet(spacing);
            Parameter layoutRuleParameter = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_LAYOUT_RULE);
            if (layoutRuleParameter != null && layoutRuleParameter.IsReadOnly == false)
            {
                layoutRuleParameter.Set((int)RebarLayoutRule.MaximumSpacing);
            }

            Parameter spacingParameter = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_BAR_SPACING);
            if (spacingParameter != null && spacingParameter.IsReadOnly == false)
            {
                spacingParameter.Set(spacing);
            }
        }
        public static void SetLayoutRule_LengthwithSpacing(Rebar rebar, double length, double spacing)
        {
            length = MmToFeet(length);
            spacing = MmToFeet(spacing);
            int quantity = (int)(length / spacing) + 1;

            Parameter layoutRuleParameter = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_LAYOUT_RULE);
            if (layoutRuleParameter != null && layoutRuleParameter.IsReadOnly == false)
            {
                layoutRuleParameter.Set((int)RebarLayoutRule.NumberWithSpacing);
            }

            Parameter spacingParameter = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_BAR_SPACING);
            if (spacingParameter != null && spacingParameter.IsReadOnly == false)
            {
                spacingParameter.Set(spacing);
            }

            Parameter quantityParameter = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_QUANTITY_OF_BARS);
            if (quantityParameter != null && quantityParameter.IsReadOnly == false)
            {
                quantityParameter.Set(quantity);
            }
        }
        public static void SetLayoutRule_QuantitywithSpacing(Rebar rebar, int quantity, double spacing)
        {
            spacing = MmToFeet(spacing);

            Parameter layoutRuleParameter = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_LAYOUT_RULE);
            if (layoutRuleParameter != null && layoutRuleParameter.IsReadOnly == false)
            {
                layoutRuleParameter.Set((int)RebarLayoutRule.NumberWithSpacing);
            }

            Parameter spacingParameter = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_BAR_SPACING);
            if (spacingParameter != null && spacingParameter.IsReadOnly == false)
            {
                spacingParameter.Set(spacing);
            }

            Parameter quantityParameter = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_QUANTITY_OF_BARS);
            if (quantityParameter != null && quantityParameter.IsReadOnly == false)
            {
                quantityParameter.Set(quantity);
            }
        }
        public static void SetLayoutRule_FixedNumber(Rebar rebar, double fixedNumber)
        {
            Parameter layoutRuleParameter = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_LAYOUT_RULE);
            if (layoutRuleParameter != null && layoutRuleParameter.IsReadOnly == false)
            {
                layoutRuleParameter.Set((int)RebarLayoutRule.FixedNumber);
            }

            Parameter quantityParameter = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_QUANTITY_OF_BARS);

            if (quantityParameter != null && quantityParameter.IsReadOnly == false)
            {
                quantityParameter.Set(fixedNumber);
            }
        }
        public static void SetLayoutRule_Single(Rebar rebar)
        {

        }
        public static List<Rebar> CopyAndMoveRebar(Document doc, Rebar sourceRebar, double distance)
        {
            List<Rebar> copiedRebars = new List<Rebar>();

            // Sao chép đối tượng Rebar
            ICollection<ElementId> sourceRebarIds = new List<ElementId> { sourceRebar.Id };
            ICollection<ElementId> copiedRebarIds = ElementTransformUtils.CopyElements(
                doc.ActiveView,
                sourceRebarIds,
                doc.ActiveView,
                Transform.CreateTranslation(new XYZ(0, 0, distance)),
                new CopyPasteOptions()
            );

            // Lấy các đối tượng Rebar đã sao chép và di chuyển
            foreach (ElementId copiedRebarId in copiedRebarIds)
            {
                Rebar copiedRebar = doc.GetElement(copiedRebarId) as Rebar;
                if (copiedRebar != null)
                {
                    copiedRebars.Add(copiedRebar);
                }
            }

            return copiedRebars;
        }
        public static bool CheckPointInsideBoundingBox(BoundingBoxXYZ boundingBox, XYZ point)
        {
            bool isInside = false;
            if (boundingBox != null && boundingBox.Min.X < point.X && boundingBox.Min.Y < point.Y)
            {
                if (boundingBox.Max.X > point.X && boundingBox.Max.Y > point.Y)
                {
                    return isInside = true;
                }
            }
            return isInside;
        }
        public static bool CheckPointNotOutsideBoundingBox(BoundingBoxXYZ boundingBox, XYZ point)
        {
            bool isNotOutside = false;
            var tolerance = 0.000001;

            if (boundingBox != null
                && (boundingBox.Min.X <= point.X || CheckValuesSamebyTolerance(boundingBox.Min.X, point.X, tolerance))
                && (boundingBox.Min.Y <= point.Y || CheckValuesSamebyTolerance(boundingBox.Min.Y, point.Y, tolerance)))
            {
                if ((boundingBox.Max.X >= point.X || CheckValuesSamebyTolerance(boundingBox.Max.X, point.X, tolerance))
                    && (boundingBox.Max.Y >= point.Y || CheckValuesSamebyTolerance(boundingBox.Max.Y, point.Y, tolerance)))
                {
                    return isNotOutside = true;
                }
            }
            return isNotOutside;
        }
        public static bool CheckPointsSamebyTolerance(XYZ p0, XYZ p1, double tolerance)
        {
            bool isSame = Math.Abs(p0.X - p1.X) <= tolerance &&
                       Math.Abs(p0.Y - p1.Y) <= tolerance &&
                       Math.Abs(p0.Z - p1.Z) <= tolerance;

            return isSame;
        }
        public static XYZ FindNearestPoint(List<XYZ> points, XYZ targetPoint)
        {
            double minDistance = double.MaxValue;
            XYZ nearestPoint = null;

            foreach (XYZ point in points)
            {
                double distance = point.DistanceTo(targetPoint);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestPoint = point;
                }
            }

            return nearestPoint;
        }
        public static XYZ CreateNormalFromThreePoints(XYZ point1, XYZ point2, XYZ point3)
        {
            Plane plane = Plane.CreateByThreePoints(point1, point2, point3);

            XYZ normal = plane.Normal;

            return normal;
        }
        public static XYZ CalculatePlaneNormal(XYZ p1, XYZ p2, XYZ p3)
        {
            XYZ v1 = p2 - p1;
            XYZ v2 = p3 - p1;

            XYZ normal = v1.CrossProduct(v2);

            return normal.Normalize();
        }
        public static Line CreateModelLine(XYZ p1, XYZ p2)
        {
            Line line = Line.CreateBound(p1, p2);
            return line;
        }
        public static bool AreVectorsParallel(XYZ vector1, XYZ vector2)
        {
            double dotProduct = vector1.Normalize().DotProduct(vector2.Normalize());
            double angle = Math.Acos(dotProduct);

            // Kiểm tra xem góc giữa hai vector có gần bằng 0 hay không (tính chính xác đến một độ đo nhất định)
            const double tolerance = 0.001;
            return Math.Abs(angle) < tolerance || Math.Abs(angle - Math.PI) < tolerance;
        }
        public static bool AreParallel(XYZ vector1, XYZ vector2)
        {
            const double epsilon = 1e-9; // Giá trị ngưỡng nhỏ để so sánh gần đúng

            double dotProduct = vector1.DotProduct(vector2); // Không cần phải chuẩn hóa vectơ trước khi tính tích vô hướng.
            double magnitude1 = vector1.GetLength();
            double magnitude2 = vector2.GetLength();

            if (magnitude1 < epsilon || magnitude2 < epsilon)
            {
                // Tránh chia cho 0
                return false;
            }

            double cosTheta = dotProduct / (magnitude1 * magnitude2);

            if (cosTheta > 1.0)
            {
                cosTheta = 1.0; // Đảm bảo cosTheta nằm trong khoảng [-1, 1] để tránh lỗi trong Math.Acos.
            }

            double angle = Math.Acos(cosTheta);

            return Math.Abs(angle) < epsilon || Math.Abs(Math.PI - angle) < epsilon;
        }
        public static XYZ IncreaseVector(XYZ vector, double lenght)
        {
            var x = vector.X;
            var y = vector.Y;
            var z = vector.Z;
            var v = Math.Sqrt(x * x + y * y + z * z);
            var constValue = lenght / v;
            XYZ newVector = constValue * vector;

            return newVector;
        }
        // Transfer Unit
        public static double FeetToMm(this double feet)
        {
            double mm = feet * 304.8;
            return mm;
        }
        public static double MmToFeet(this double mm)
        {
            double feet = mm / 304.8;
            return feet;
        }
        public static double degreeToRadius(double degree)
        {
            double radius = degree * Math.PI / 180.0;
            return radius;
        }
        public static double radiusToDegree(double radius)
        {
            double degree = radius * 180.0 / Math.PI;
            return degree;
        }
        //public static void Show3DRebar_Actiview(Document doc, List<Rebar> rebars)
        //{
        //    if (doc.ActiveView is View3D)
        //    {
        //        View3D currentView3D = doc.ActiveView as View3D;
        //        bool viewSolidValue = true;
        //        bool viewUnobscuredValue = true;

        //        foreach (Rebar rebar in rebars)
        //        {
        //            rebar.SetSolidInView(currentView3D, viewSolidValue);
        //            rebar.SetUnobscuredInView(currentView3D, viewUnobscuredValue);
        //        }
        //    }
        //}
        public static List<Rebar> GetHostObjectRebar(Document doc, Element hostObject)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> allRebars = collector.OfClass(typeof(Rebar)).ToElements();

            List<Rebar> hostObject_Rebars = new List<Rebar>();

            foreach (Element rebar in allRebars)
            {
                Rebar currentRebar = rebar as Rebar;

                if (currentRebar != null && currentRebar.GetHostId().Equals(hostObject.Id))
                {
                    hostObject_Rebars.Add(currentRebar);
                }
            }
            return hostObject_Rebars;
        }
        public static List<RebarBarType> GetRebarBarTypes(Document doc)
        {
            List<RebarBarType> rebarBarTypes = new FilteredElementCollector(doc)
                .OfClass(typeof(RebarBarType))
                .Cast<RebarBarType>()
                .ToList();

            return rebarBarTypes;
        }
        public static RebarBarType GetRebarBarType(Document doc, double diameter)
        {
            List<RebarBarType> rebarBarTypes = GetRebarBarTypes(doc);
            RebarBarType rebarBarType = null;

            if (rebarBarTypes.Count > 0)
            {
                foreach (RebarBarType current_RebarBarType in rebarBarTypes)
                {
                    Parameter diameterParameter = current_RebarBarType.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER);
                    if (diameter == FeetToMm(diameterParameter.AsDouble()))
                    {
                        return rebarBarType = current_RebarBarType;
                    }
                }
            }

            return rebarBarType;
        }
        public static List<RebarHookType> GetAllRebarHookTypes(Document doc)
        {
            List<RebarHookType> rebarHookTypes = new List<RebarHookType>();

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> hookTypes = collector.OfClass(typeof(RebarHookType)).ToElements();

            foreach (Element hookTypeElement in hookTypes)
            {
                RebarHookType rebarHookType = hookTypeElement as RebarHookType;
                if (rebarHookType != null)
                {
                    rebarHookTypes.Add(rebarHookType);
                }
            }

            return rebarHookTypes;
        }
        public static List<Grid> GetAllGrids(Document doc)
        {

            List<Grid> grids = new List<Grid>();

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            List<Element> elements = collector.OfClass(typeof(Grid)).ToElements().ToList();
            foreach (var element in elements)
            {
                grids.Add(element as Grid);
            }

            return grids;
        }
        public static List<DimensionType> GetAllDimensionTypes(Document document)
        {
            FilteredElementCollector collector = new FilteredElementCollector(document);
            IList<Element> dimensions = collector.OfClass(typeof(DimensionType)).ToElements();

            List<DimensionType> dimensionTypes = new List<DimensionType>();

            foreach (Element element in dimensions)
            {
                DimensionType dimensionType = element as DimensionType;
                if (dimensionType != null)
                {
                    dimensionTypes.Add(dimensionType);
                }
            }

            return dimensionTypes;
        }
        //
        public static List<RebarBarType> GetAllRebarBarTypes(Document doc)
        {
            List<string> requiredTypeNames = new List<string> {
                "HT-D6",
                "HT-D8",
                "HT-D10",
                "HT-D12",
                "HT-D12",
                "HT-D14",
                "HT-D16",
                "HT-D18",
                "HT-D20",
                "HT-D22",
                "HT-D25",
                "HT-D28",
                "HT-D30",
                "HT-D32",
                "HT-D34",
                "HT-D36",
                "HT-D38",
                "HT-D40",
                "HT-D50",
                };
            List<RebarBarType> rebarBarTypes = new List<RebarBarType>();

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> elements = collector.OfClass(typeof(RebarBarType)).ToElements();

            foreach (Element element in elements)
            {
                RebarBarType rebarBarType = element as RebarBarType;
                if (rebarBarType != null && requiredTypeNames.Contains(rebarBarType.Name))
                {
                    rebarBarTypes.Add(rebarBarType);
                    requiredTypeNames.Remove(rebarBarType.Name);
                }
            }

            // Tạo mới các RebarBarType chưa tồn tại trong danh sách
            foreach (string requiredTypeName in requiredTypeNames)
            {
                double diameter = GetDiameterFromTypeName(requiredTypeName);
                CreateOrUpdateRebarBarType(doc, requiredTypeName, diameter);
                RebarBarType createdType = GetRebarBarTypeByName(doc, requiredTypeName);
                if (createdType != null)
                {
                    rebarBarTypes.Add(createdType);
                }
            }

            return rebarBarTypes;
        }
        public static double GetDiameterFromTypeName(string typeName)
        {
            string diameterString = typeName.Replace("HT-D", string.Empty);
            double diameter = double.Parse(diameterString);
            return diameter;
        }
        public static void CreateOrUpdateRebarBarType(Document doc, string typeName, double diameter)
        {
            RebarBarType rebarBarType = GetRebarBarTypeByName(doc, typeName);
            if (rebarBarType != null)
            {
                // Cập nhật giá trị đường kính
                Parameter diameterParameter = rebarBarType.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER);
                Parameter modelDiameterParameter = rebarBarType.get_Parameter(BuiltInParameter.REBAR_MODEL_BAR_DIAMETER);
                Parameter bParameter = rebarBarType.get_Parameter(BuiltInParameter.REBAR_STANDARD_BEND_DIAMETER);
                Parameter hookBParameter = rebarBarType.get_Parameter(BuiltInParameter.REBAR_STANDARD_HOOK_BEND_DIAMETER);
                Parameter stirBParameter = rebarBarType.get_Parameter(BuiltInParameter.REBAR_BAR_STIRRUP_BEND_DIAMETER);
                if (diameterParameter != null)
                {
                    diameterParameter.Set(Util.MmToFeet(diameter));
                    modelDiameterParameter.Set(Util.MmToFeet(diameter));
                    bParameter.Set(Util.MmToFeet(diameter * 3));
                    hookBParameter.Set(Util.MmToFeet(diameter * 3));
                    stirBParameter.Set(Util.MmToFeet(diameter * 3));
                }
            }
            else
            {
                // Tạo mới RebarBarType nếu chưa tồn tại
                rebarBarType = RebarBarType.Create(doc);
                rebarBarType.Name = typeName;

                // Thiết lập giá trị đường kính bằng cách sử dụng Parameter
                Parameter diameterParameter = rebarBarType.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER);
                Parameter modelDiameterParameter = rebarBarType.get_Parameter(BuiltInParameter.REBAR_MODEL_BAR_DIAMETER);
                Parameter bParameter = rebarBarType.get_Parameter(BuiltInParameter.REBAR_STANDARD_BEND_DIAMETER);
                Parameter hookBParameter = rebarBarType.get_Parameter(BuiltInParameter.REBAR_STANDARD_HOOK_BEND_DIAMETER);
                Parameter stirBParameter = rebarBarType.get_Parameter(BuiltInParameter.REBAR_BAR_STIRRUP_BEND_DIAMETER);
                if (diameterParameter != null)
                {
                    diameterParameter.Set(Util.MmToFeet(diameter));
                    modelDiameterParameter.Set(Util.MmToFeet(diameter));
                    bParameter.Set(Util.MmToFeet(diameter * 3));
                    hookBParameter.Set(Util.MmToFeet(diameter * 3));
                    stirBParameter.Set(Util.MmToFeet(diameter * 3));
                }
            }
        }
        public static RebarBarType GetRebarBarTypeByName(Document doc, string typeName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            Element element = collector.OfClass(typeof(RebarBarType)).FirstOrDefault(x => x.Name == typeName);
            return element as RebarBarType;
        }
        public static List<FamilyInstance> GetFamilyInstancesByBuiltInCategory(Document doc, BuiltInCategory builtInCategory)
        {
            List<FamilyInstance> familyInstances = new List<FamilyInstance>();

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> elements = collector
                .OfCategory(builtInCategory)
                .WhereElementIsNotElementType()
                .ToElements();

            foreach (Element element in elements)
            {
                if (element is FamilyInstance familyInstance)
                {
                    familyInstances.Add(familyInstance);
                }
            }

            return familyInstances;
        }
        // Select
        public static List<Element> SelectElementsbyTypes(UIDocument uiDoc, List<BuiltInCategory> builtInCategories)
        {

            Document doc = uiDoc.Document;
            Selection sel = uiDoc.Selection;

            List<Element> selectedElements = new List<Element>();
            CategoriesSelectionFilter categoryFilter = new CategoriesSelectionFilter(builtInCategories);

            try
            {
                IList<Reference> references = sel.PickObjects(ObjectType.Element, categoryFilter, "Select elements");

                if (references != null)
                {
                    foreach (Reference reference in references)
                    {
                        Element element = doc.GetElement(reference.ElementId) as Element;

                        if (element != null)
                        {
                            selectedElements.Add(element as Element);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }

            return selectedElements;
        }
        public static List<FamilyInstance> SelectElementsbyType(UIDocument uiDoc, BuiltInCategory builtInCategory)
        {
            Document doc = uiDoc.Document;
            Selection sel = uiDoc.Selection;

            List<FamilyInstance> selectedElements = new List<FamilyInstance>();
            CategorySelectionFilter categoryFilter = new CategorySelectionFilter(builtInCategory);

            try
            {
                IList<Reference> references = sel.PickObjects(ObjectType.Element, categoryFilter, "Select elements");

                if (references != null)
                {
                    foreach (Reference reference in references)
                    {
                        Element element = doc.GetElement(reference.ElementId) as FamilyInstance;

                        if (element != null)
                        {
                            selectedElements.Add(element as FamilyInstance);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }

            return selectedElements;
        }
        public static List<Grid> SelectedGrids(UIDocument uiDoc)
        {
            Document doc = uiDoc.Document;
            Selection sel = uiDoc.Selection;

            List<Grid> selectedElements = new List<Grid>();
            CategorySelectionFilter categoryFilter = new CategorySelectionFilter(BuiltInCategory.OST_Grids);

            try
            {
                IList<Reference> references = sel.PickObjects(ObjectType.Element, categoryFilter, "Select elements");

                if (references != null)
                {
                    foreach (Reference reference in references)
                    {
                        Element element = doc.GetElement(reference.ElementId) as Grid;

                        if (element != null)
                        {
                            selectedElements.Add(element as Grid);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }

            return selectedElements;
        }
        public static bool AreFamilyInstance_Adjacent(FamilyInstance familyInstance1, FamilyInstance familyInstance2)
        {
            //check 2 familyinstace kế nhau hoặc giao với nhau
            BoundingBoxXYZ boundingBox1 = familyInstance1.get_BoundingBox(null);
            BoundingBoxXYZ boundingBox2 = familyInstance2.get_BoundingBox(null);

            Outline outline1 = new Outline(boundingBox1.Min, boundingBox1.Max);
            Outline outline2 = new Outline(boundingBox2.Min, boundingBox2.Max);

            double tolerance = 0.0001;
            bool intersects = outline1.Intersects(outline2, tolerance);

            if (intersects)
            {
                return true;
            }

            return false;
        }
        public static List<FamilyInstance> FindBeamsIntersectingColumn(Document doc, FamilyInstance column)
        {
            List<FamilyInstance> intersectingBeams = new List<FamilyInstance>();

            BoundingBoxXYZ columnBoundingBox = column.get_BoundingBox(null);

            Outline columnOutline = new Outline(columnBoundingBox.Min, columnBoundingBox.Max);

            List<FamilyInstance> projectBeams = Util.GetFamilyInstancesByBuiltInCategory(doc, BuiltInCategory.OST_StructuralFraming);
            foreach (FamilyInstance projectBeam in projectBeams)
            {
                if (projectBeam.Id != column.Id)
                {
                    BoundingBoxXYZ beamBoundingBox = projectBeam.get_BoundingBox(null);

                    Outline beamOutline = new Outline(beamBoundingBox.Min, beamBoundingBox.Max);

                    double tolerance = 0.0001;

                    if (beamOutline.Intersects(columnOutline, tolerance))
                    {
                        intersectingBeams.Add(projectBeam);
                    }
                }
            }

            return intersectingBeams;
        }
        public static List<FamilyInstance> FindColumnsIntersectingBeam(Document doc, FamilyInstance beam)
        {
            List<FamilyInstance> intersectingColumns = new List<FamilyInstance>();

            BoundingBoxXYZ beamBoundingBox = beam.get_BoundingBox(null);

            Outline beamnOutline = new Outline(beamBoundingBox.Min, beamBoundingBox.Max);

            List<FamilyInstance> projectColumns = Util.GetFamilyInstancesByBuiltInCategory(doc, BuiltInCategory.OST_StructuralColumns);
            foreach (FamilyInstance projectColumn in projectColumns)
            {
                if (projectColumn.Id != beam.Id)
                {
                    BoundingBoxXYZ columnBoundingBox = projectColumn.get_BoundingBox(null);

                    Outline columnOutline = new Outline(columnBoundingBox.Min, columnBoundingBox.Max);

                    double tolerance = 0.0001;

                    if (columnOutline.Intersects(beamnOutline, tolerance))
                    {
                        intersectingColumns.Add(projectColumn);
                    }
                }
            }

            return intersectingColumns;
        }
        public static ModelLine CreateModelLine(Document doc, XYZ startPoint, XYZ endPoint)
        {
            try
            {
                XYZ normal = XYZ.BasisZ;
                if (startPoint.X == endPoint.X && startPoint.Y == endPoint.Y)
                {
                    normal = XYZ.BasisX;
                }
                XYZ origin = (startPoint + endPoint) / 2.0;

                Plane plane = Plane.CreateByNormalAndOrigin(normal, origin);
                SketchPlane sketchPlane = SketchPlane.Create(doc, plane);

                Line line = Line.CreateBound(startPoint, endPoint);

                ModelLine modelLine = doc.Create.NewModelCurve(line, sketchPlane) as ModelLine;

                return modelLine;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public static ModelCurve CreateModel(Document doc, Curve curve)
        {
            var dir = (curve as Line).Direction;
            var pnt = curve.GetEndPoint(0);
            XYZ normal = null;

            SketchPlane sp = null;
            if (dir.IsPerpendicular(XYZ.BasisZ))
            {
                normal = XYZ.BasisZ;
            }
            else if (dir.IsParallel(XYZ.BasisZ))
            {
                normal = XYZ.BasisX;
            }
            else
            {
                normal = dir.CrossProduct(XYZ.BasisZ);
            }

            sp = SketchPlane.Create(doc, Plane.CreateByNormalAndOrigin(normal, pnt));

            var ml = doc.Create.NewModelCurve(curve, sp);
            return ml;
        }
        public static bool IsPerpendicular(this Autodesk.Revit.DB.XYZ firstVec, Autodesk.Revit.DB.XYZ secondVec)
        {
            double dot = firstVec.DotProduct(secondVec);
            return dot.Equals(0);
        }
        public static bool IsParallel(this XYZ firstVec, XYZ secondVec)
        {
            Autodesk.Revit.DB.XYZ first = firstVec.Normalize();
            Autodesk.Revit.DB.XYZ second = secondVec.Normalize();

            // if the dot product of two unit vectors is equal to 1, return true
            double dot = first.DotProduct(second);
            return dot.Equals(1) || dot.Equals(-1);
        }
        public static XYZ[] GetRectangle_FamilyInstancePoints(FamilyInstance familyInstance)
        {
            BoundingBoxXYZ boundingBox = familyInstance.get_BoundingBox(null);

            XYZ minPoint = boundingBox.Min;
            XYZ maxPoint = boundingBox.Max;

            XYZ bottomLeftFront = minPoint;
            XYZ bottomRightFront = new XYZ(maxPoint.X, minPoint.Y, minPoint.Z);
            XYZ bottomRightBack = new XYZ(maxPoint.X, maxPoint.Y, minPoint.Z);
            XYZ bottomLeftBack = new XYZ(minPoint.X, maxPoint.Y, minPoint.Z);

            XYZ topLeftFront = new XYZ(minPoint.X, minPoint.Y, maxPoint.Z);
            XYZ topRightFront = new XYZ(maxPoint.X, minPoint.Y, maxPoint.Z);
            XYZ topRightBack = maxPoint;
            XYZ topLeftBack = new XYZ(minPoint.X, maxPoint.Y, maxPoint.Z);

            XYZ[] points = new XYZ[]
            {
                bottomLeftFront,
                bottomRightFront,
                bottomRightBack,
                bottomLeftBack,
                topLeftFront,
                topRightFront,
                topRightBack,
                topLeftBack
            };

            return points;
        }
        public static Transform CreateTransform(XYZ origin, XYZ newPoint)
        {
            // Phương thức dịch chuyển từ điểm thực sang điểm ảo
            // khi dùng: truyền vào điểm ảo, trả về điểm thực
            XYZ transformVector = newPoint - origin;

            Transform transform = Transform.CreateTranslation(transformVector);

            return transform;
        }
        public static Transform CreateTransform(XYZ Po, XYZ Px, XYZ Pz)
        {
            Transform transform = Transform.Identity;
            XYZ origin = new XYZ(Po.X, Po.Y, Po.Z);
            XYZ xVector = new XYZ(Px.X - Po.X, Px.Y - Po.Y, Px.Z - Po.Z).Normalize();
            XYZ zVector = new XYZ(Pz.X - Po.X, Pz.Y - Po.Y, Pz.Z - Po.Z).Normalize();
            XYZ yVector = zVector.CrossProduct(xVector).Normalize();

            try
            {
                transform.Origin = origin;
                transform.BasisX = xVector;
                transform.BasisY = yVector;
                transform.BasisZ = zVector;
            }
            catch (Exception ex)
            {
                throw;
            }

            return transform;
        }
        public static Transform CreateTransform(XYZ Po, XYZ Px, XYZ Py, XYZ Pz)
        {
            Transform transform = Transform.Identity;
            XYZ origin = new XYZ(Po.X, Po.Y, Po.Z);
            XYZ xVector = new XYZ(Util.MmToFeet(1), Px.Y - Po.Y, Px.Z - Po.Z);
            XYZ yVector = new XYZ(Py.X - Po.X, Util.MmToFeet(1), Py.Z - Po.Z);
            XYZ zVector = new XYZ(Pz.X - Po.X, Pz.Y - Po.Y, Util.MmToFeet(1));

            try
            {
                transform.Origin = origin;
                transform.BasisX = xVector;
                transform.BasisY = yVector;
                transform.BasisZ = zVector;

            }
            catch (Exception ex)
            {
                throw;
            }
            return transform;
        }
        // Get Distance
        public static double CalculateDistanceToLine(XYZ point, Line line)
        {
            XYZ lineStart = line.GetEndPoint(0);
            XYZ lineEnd = line.GetEndPoint(1);
            XYZ vector1 = point - lineStart;
            XYZ vector2 = lineEnd - lineStart;
            double distance = vector1.CrossProduct(vector2).GetLength() / vector2.GetLength();
            return distance;
        }
        public static bool IsPerpendicularToDirection(PlanarFace face, XYZ direction, double tolerance)
        {
            XYZ normal = face.ComputeNormal(new UV());
            double dotProduct = normal.DotProduct(direction);
            return Math.Abs(dotProduct) < tolerance;
        }
        public static bool ArePointsEqual(XYZ point1, XYZ point2)
        {
            return Math.Abs(point1.X - point2.X) < 0.000001 && Math.Abs(point1.Y - point2.Y) < 0.000001;
        }

        public static List<XYZ> ListPointsCheckedNotStraight(List<XYZ> pointsInput)
        {
            List<XYZ> points = pointsInput;

            for (int i = 1; i < pointsInput.Count - 1; i++)
            {
                var prePoint = pointsInput[i - 1];
                var currPoint = pointsInput[i];
                var nextPoint = pointsInput[i + 1];

                if (AreThreePointsCollinear(prePoint, currPoint, nextPoint))
                {
                    points.RemoveAt(i);
                }
            }

            return points;
        }
        public static bool CheckValuesSamebyTolerance(double v1, double v2, double tolerance = 0.000001)
        {
            bool isSame = (Math.Abs(v1 - v2) <= tolerance);
            return isSame;
        }
        public static bool ArePointsCollinear(List<XYZ> points)
        {
            if (points.Count < 3)
            {
                return false;
            }

            for (int i = 2; i < points.Count; i++)
            {
                XYZ p1 = points[i - 2];
                XYZ p2 = points[i - 1];
                XYZ p3 = points[i];

                XYZ v1 = p2 - p1;
                XYZ v2 = p3 - p1;

                double dotProduct = v1.DotProduct(v2);
                const double epsilon = 1e-6;

                if (dotProduct < 1 - epsilon || dotProduct > -1 + epsilon)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool AreThreePointsCollinear(XYZ point1, XYZ point2, XYZ point3)
        {
            XYZ vector1 = point2 - point1;
            XYZ vector2 = point3 - point1;

            return vector1.CrossProduct(vector2).IsAlmostEqualTo(XYZ.Zero);
        }
        public static bool IsValidPlane(XYZ point1, XYZ point2, XYZ point3)
        {
            double[] v1 = { point2.X - point1.X, point2.Y - point1.Y, point2.Z - point1.Z };
            double[] v2 = { point3.X - point1.X, point3.Y - point1.Y, point3.Z - point1.Z };

            double[] crossProduct = {
            v1[1] * v2[2] - v1[2] * v2[1],
            v1[2] * v2[0] - v1[0] * v2[2],
            v1[0] * v2[1] - v1[1] * v2[0]
        };

            double length = Math.Sqrt(
                crossProduct[0] * crossProduct[0] +
                crossProduct[1] * crossProduct[1] +
                crossProduct[2] * crossProduct[2]
            );

            return length > 0;
        }
        public static void ExecuteTransaction(Document doc, string transactionName, Action<Transaction> codeToExecute)
        {
            Transaction transaction = new Transaction(doc, transactionName);
            transaction.Start();

            try
            {
                codeToExecute(transaction);
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.RollBack();
                Console.WriteLine("Error occurred: " + ex.Message);
            }
        }

        public static XYZ EvaluatePointAtDistance(this Curve curve, double distance)
        {
            double totalLength = curve.Length;
            double paramStart = curve.GetEndParameter(0);
            double paramEnd = curve.GetEndParameter(1);

            // Nội suy nhị phân (binary search) để tìm parameter ứng với độ dài
            double low = paramStart;
            double high = paramEnd;
            double mid = 0;
            XYZ pointLow = curve.Evaluate(low, false);
            XYZ pointHigh = curve.Evaluate(high, false);

            const double tolerance = 0.001; // foot

            while ((high - low) > 1e-6)
            {
                mid = (low + high) / 2;
                XYZ pointMid = curve.Evaluate(mid, false);
                double currentDist = curve.ComputeDerivatives(mid, false).Origin.DistanceTo(curve.Evaluate(paramStart, false));

                if (currentDist < distance)
                    low = mid;
                else
                    high = mid;
            }

            return curve.Evaluate(mid, false);
        }

        public static double ComputeAngleFromTangent(Curve curve, XYZ point)
        {
            // Chiếu điểm vào curve để lấy parameter gần nhất
            var projResult = curve.Project(point);
            if (projResult == null) throw new InvalidOperationException("Cannot project point to curve.");

            double param = projResult.Parameter;

            // Di chuyển tiến lên 1mm dọc theo curve
            double offset = curve.ApproximateLength * 0.001; // tương đương 1mm
            double paramFwd = Math.Min(curve.GetEndParameter(1), param + offset);

            XYZ pFwd = curve.Evaluate(paramFwd, false);
            XYZ direction = (pFwd - point).Normalize();

            // So sánh với trục X
            XYZ yAxis = XYZ.BasisX;
            double angle = direction.AngleTo(yAxis);

            // Xác định chiều xoay
            XYZ cross = yAxis.CrossProduct(direction);
            if (cross.Z < 0)
                angle = -angle;

            return angle;
        }

        public static double FindParameterAtDistance(Curve curve, double targetDistance, double tolerance = 0.001)
        {
            double startParam = curve.GetEndParameter(0);
            double endParam = curve.GetEndParameter(1);
            double midParam = 0;

            double totalLength = curve.ApproximateLength;
            if (targetDistance >= totalLength)
                return endParam;

            while ((endParam - startParam) > 1e-6)
            {
                midParam = (startParam + endParam) / 2.0;

                XYZ pStart = curve.Evaluate(curve.GetEndParameter(0), false);
                XYZ pMid = curve.Evaluate(midParam, false);
                double len = pStart.DistanceTo(pMid);

                if (Math.Abs(len - targetDistance) < tolerance)
                    break;

                if (len < targetDistance)
                    startParam = midParam;
                else
                    endParam = midParam;
            }

            return midParam;
        }



    }
    public static class ListUtil
    {
        public static ObservableCollection<T> ToObservableCollection<T>
            (this IEnumerable<T> list)
        {
            return new ObservableCollection<T>(list);
        }
    }

    public class CategorySelectionFilter : ISelectionFilter
    {
        private BuiltInCategory targetCategory;

        public CategorySelectionFilter(BuiltInCategory category)
        {
            targetCategory = category;
        }

        public bool AllowElement(Element elem)
        {
            return elem.Category.Id.IntegerValue == (int)targetCategory;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
    public class CategoriesSelectionFilter : ISelectionFilter
    {
        private List<BuiltInCategory> targetCategories;

        public CategoriesSelectionFilter(List<BuiltInCategory> categories)
        {
            targetCategories = categories;
        }

        public bool AllowElement(Element elem)
        {
            return targetCategories.Contains((BuiltInCategory)elem.Category.Id.IntegerValue);
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }

    public class CurveSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return elem is CurveElement;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }

    public static class VectorUtils
    {
        /// <summary>
        /// Calculates the angle from a reference vector (typically X-axis) to a target vector on a given plane.
        /// The rotation direction is determined by the normal of the plane.
        /// </summary>
        /// <param name="fromVector">The starting vector (e.g., XYZ.BasisX, or XYZ.BasisY).</param>
        /// <param name="toVector">The target vector.</param>
        /// <param name="planeNormal">The normal vector of the plane on which the angle is measured (e.g., XYZ.BasisZ for XY plane).</param>
        /// <returns>The angle in radians.</returns>
        public static double AngleOnPlaneFromXAxis(XYZ fromVector, XYZ toVector, XYZ planeNormal)
        {
            // Project vectors onto the plane
            XYZ projectedFrom = fromVector - fromVector.DotProduct(planeNormal) * planeNormal;
            XYZ projectedTo = toVector - toVector.DotProduct(planeNormal) * planeNormal;

            // Normalize projected vectors
            projectedFrom = projectedFrom.Normalize();
            projectedTo = projectedTo.Normalize();

            // Calculate dot product for the angle magnitude
            double dot = projectedFrom.DotProduct(projectedTo);
            // Ensure dot product is within valid range for Acos due to floating point inaccuracies
            dot = Math.Max(-1.0, Math.Min(1.0, dot));
            double angle = Math.Acos(dot);

            // Determine the sign of the angle using cross product
            XYZ cross = projectedFrom.CrossProduct(projectedTo);
            if (cross.DotProduct(planeNormal) < 0)
            {
                angle = -angle;
            }

            return angle;
        }
    }

    public class GroupSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return elem is Group;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }

}
