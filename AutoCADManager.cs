using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoCADNote
{
    public class AutoCADManager
    {
        public static event EventHandler DocumentChanged;
        public static string currentDocument;

        public static string GetDrawingName()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            return doc.Name.Split('\\').Last(); // Wire up renames
        }
        public static void WireUpDocumentReloadEvent()
        {
            Application.DocumentManager.DocumentBecameCurrent += DocumentManager_DocumentBecameCurrent;
        }

        public static void UnWireUpDocumentReloadEvent()
        {
            Application.DocumentManager.DocumentBecameCurrent -= DocumentManager_DocumentBecameCurrent;
        }

        static void DocumentManager_DocumentBecameCurrent(object sender, DocumentCollectionEventArgs e)
        {
            // DocumentBecameCurrent event fires multiple times for a document switch, it fires first with the new document with isActive = true,
            // then fires with the old document with isActive false and then once again with the new document with isActive = true.
            // Hence we put this double check below of processing only if active and different from the document stored in memory.
            // The world outside the AutoCADManager will only see one event bubbled correctly.
            if (e.Document != null)
            {
                var isActive = e.Document.IsActive;
                if (isActive)
                {
                    var documentThatJustBecameCurrent = e.Document == null ? null : e.Document.Name;

                    if (!String.Equals(documentThatJustBecameCurrent, currentDocument)) //currentDocument is saved from last time around.
                    {
                        currentDocument = documentThatJustBecameCurrent;
                        DocumentChanged(sender, e);
                    }
                }
            }          
        }
    }
}
