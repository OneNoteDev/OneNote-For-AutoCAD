// (C) Copyright 2015 by  
//
using Autodesk.AutoCAD.Runtime;

// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(AutoCADNote.MyCommands))]

namespace AutoCADNote
{

    // This class is instantiated by AutoCAD for each document when
    // a command is called by the user the first time in the context
    // of a given document. In other words, non static data in this class
    // is implicitly per-document!
    public class MyCommands
    {
        // The CommandMethod attribute can be applied to any public  member 
        // function of any public class.
        // The function should take no arguments and return nothing.
        // If the method is an intance member then the enclosing class is 
        // intantiated for each document. If the member is a static member then
        // the enclosing class is NOT intantiated.
        //
        // NOTE: CommandMethod has overloads where you can provide helpid and
        // context menu.

        [CommandMethod("OneNoteGroup", "Show", "Show", CommandFlags.Modal)]
        public async void Show() // This method can have any name
        {
            await Manager.Init();
        }
    }
}
