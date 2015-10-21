using Autodesk.AutoCAD.Windows;
using System;
using System.Windows;

namespace AutoCADNote
{
    public class PaletteManager
    {
        private static readonly string paletteGuid = "4CF60816-6954-430B-A0EC-6A1EB23E2907";
        public static PaletteSet CreatePaletteSet()
        {
            var myPaletteSet = new PaletteSet("OneNote", new Guid(paletteGuid));
            
            myPaletteSet.Size = new System.Drawing.Size(600, 900);
            myPaletteSet.Dock = DockSides.Right;

            return myPaletteSet;
        }
    }
}
