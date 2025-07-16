using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    }
}
