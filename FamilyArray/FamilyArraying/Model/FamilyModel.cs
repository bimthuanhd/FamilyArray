using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Lighting;
using Autodesk.Revit.UI;
using SingleData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FamilyArraying.ViewModel
{
    public class FamilyModel : BaseViewModel
    {
        public static List<Family> Families;
        public static List<FamilySymbol> FamilySymbols;

        private Family selectedFamily;
        public Family SelectedFamily
        {
            get => selectedFamily;
            set
            {
                selectedFamily = value;
                OnPropertyChanged(nameof(selectedFamily));
                ResetFamilySymbol();
            }
        }

        private FamilySymbol selectedFamilySymbol;
        public FamilySymbol SelectedFamilySymbol
        {
            get => selectedFamilySymbol;
            set
            {
                selectedFamilySymbol = value;
                OnPropertyChanged(nameof(selectedFamilySymbol));
            }
        }

        private void InitFamilies()
        {
            var families = new FilteredElementCollector(Data.Instance.Doc)
                .OfClass(typeof(Family))
                .Cast<Family>()
                .Where(f => f.FamilyCategory != null && f.FamilyCategory.Id.IntegerValue == (int)BuiltInCategory.OST_GenericModel)
                .ToList();

            Families = new List<Family>();
            foreach (Family family in families)
            {
                Families.Add(family);
            }
            SelectedFamily = Families.FirstOrDefault();
        }

        private void ResetFamilySymbol()
        {
            if (SelectedFamily != null)
            {
                FamilySymbols = new List<FamilySymbol>();
                foreach (ElementId symbolId in SelectedFamily.GetFamilySymbolIds())
                {
                    FamilySymbol symbol = Data.Instance.Doc.GetElement(symbolId) as FamilySymbol;
                    if (symbol != null)
                    {
                        FamilySymbols.Add(symbol);
                    }
                }
                SelectedFamilySymbol = FamilySymbols.FirstOrDefault();
            }
        }

        public FamilyModel()
        {
            if (FamilySymbols == null || FamilySymbols == null)
            {
                InitFamilies();
            }
        }
    }
}
