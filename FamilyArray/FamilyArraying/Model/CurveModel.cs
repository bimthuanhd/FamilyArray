using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Lighting;
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

        private double distance = 2000;
        public double Distance
        {
            get => distance;
            set
            {
                distance = value;
                OnPropertyChanged(nameof(distance));
            }
        }

        public CurveModel()
        {
            FamilyInfor = new FamilyModel();

        }

    }
}
