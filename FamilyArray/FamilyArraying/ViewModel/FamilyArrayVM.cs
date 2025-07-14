using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyArraying.ViewModel
{
    public class FamilyArrayVM : BaseViewModel
    {
        private List<CurveModel> curves;
        public List<CurveModel> Curves
        {
            get => curves;
            set
            {
                curves = value;
                OnPropertyChanged(nameof(curves));
            }
        }

    }
}
