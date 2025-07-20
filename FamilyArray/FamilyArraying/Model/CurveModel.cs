using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Lighting;
using FamilyArraying.Enum;
using FamilyArraying.EnumModel;
using FamilyArraying.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyArraying.ViewModel
{
    public class CurveModel : BaseViewModel
    {
        private Curve curve;
        public Curve Curve
        {
            get => curve;
            set
            {
                curve = value;
                OnPropertyChanged(nameof(curve));
            }
        }

        public string CurveType
        {
            get => Curve.GetType().ToString().Split('.').Last() ?? string.Empty;
        }

        private FamilyModel familyInfor;
        public FamilyModel FamilyInfor
        {
            get => familyInfor;
            set
            {
                familyInfor = value;
                OnPropertyChanged(nameof(familyInfor));
            }
        }

        private double spacing = 2000;
        public double Spacing
        {
            get => spacing;
            set
            {
                spacing = value;
                OnPropertyChanged(nameof(spacing));
            }
        }

        private List<ArraySpreadDirectionModel> listArraySpreadDirection;
        public List<ArraySpreadDirectionModel> ListArraySpreadDirection
        {
            get => listArraySpreadDirection;
            set
            {
                listArraySpreadDirection = value;
                OnPropertyChanged(nameof(listArraySpreadDirection));
            }
        }

        private ArraySpreadDirectionModel selectedArraySpreadDirection;
        public ArraySpreadDirectionModel SelectedArraySpreadDirection
        {
            get => selectedArraySpreadDirection ?? (selectedArraySpreadDirection = ListArraySpreadDirection.FirstOrDefault());
            set
            {
                selectedArraySpreadDirection = value;
                OnPropertyChanged(nameof(selectedArraySpreadDirection));
            }
        }

        private List<FipDirectionModel> listFipDirection;
        public List<FipDirectionModel> ListFipDirection
        {
            get => listFipDirection;
            set
            {
                listFipDirection = value;
                OnPropertyChanged(nameof(listFipDirection));
            }
        }

        private FipDirectionModel selectedFipDirection;
        public FipDirectionModel SelectedFipDirection
        {
            get => selectedFipDirection ?? (selectedFipDirection = ListFipDirection.FirstOrDefault());
            set
            {
                selectedFipDirection = value;
                OnPropertyChanged(nameof(selectedFipDirection));
            }
        }

        public CurveModel(Curve curveInput)
        {
            Curve = curveInput;
            FamilyInfor = new FamilyModel();
            ListArraySpreadDirection = new List<ArraySpreadDirectionModel>()
            {
                new ArraySpreadDirectionModel(){Name = "Start To End", ArraySpreadDirection = ArraySpreadDirection.StartToEnd},
                new ArraySpreadDirectionModel(){Name = "End To Start", ArraySpreadDirection = ArraySpreadDirection.EndToStart},
                new ArraySpreadDirectionModel(){Name = "Middle Outward", ArraySpreadDirection = ArraySpreadDirection.MiddleOutward}
            };

            if (Curve is Line)
            {
                ListFipDirection = new List<FipDirectionModel>()
                {
                    new FipDirectionModel(){Name = "Left", FlipDirection = FlipDirection.Left_Outer},
                    new FipDirectionModel(){Name = "Right", FlipDirection = FlipDirection.Right_Inner},
                };
            }
            else
            {
                ListFipDirection = new List<FipDirectionModel>()
                {
                    new FipDirectionModel(){Name = "Outer", FlipDirection = FlipDirection.Left_Outer},
                    new FipDirectionModel(){Name = "Inner", FlipDirection = FlipDirection.Right_Inner},
                };
            }
        }

    }
}
