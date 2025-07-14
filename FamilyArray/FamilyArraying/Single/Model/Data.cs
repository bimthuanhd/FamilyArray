using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using HTAddin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SingleData
{
    public class Data
    {
        private static Data instance;
        public static Data Instance
        {
            get => instance ?? (instance = new Data());
            set => instance = value;
        }

        public Document Doc { get; set; }
        public UIApplication UIApp { get; set; }
        public UIDocument UIDoc { get; set; }
        public Application App { get; set; }
        public Selection Sel { get; set; }

        public void InitData(ExternalCommandData commandData)
        {
            UIApp = commandData.Application;
            UIDoc = UIApp.ActiveUIDocument;
            App = UIApp.Application;
            Doc = UIDoc.Document;
            Sel = UIDoc.Selection;
        }
    }
}
