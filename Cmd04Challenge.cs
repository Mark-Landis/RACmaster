#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Electrical;


#endregion

namespace RACmaster
{
    [Transaction(TransactionMode.Manual)]
    public class Cmd04Challenge : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            IList<Element> pickList = uidoc.Selection.PickElementsByRectangle("Select some elements:");
            List<CurveElement> curveList = new List<CurveElement>();

            WallType curWallType1 = GetWallTypeByName(doc, "Storefront");
            WallType curWallType2 = GetWallTypeByName(doc, @"Generic - 8""");
            Level curLevel = GetLevelByName(doc, "Level 1");
            MEPSystemType curPipeSystemType = GetSystemTypeByName(doc, "Domestic Hot Water");
            MEPSystemType curDuctSystemType = GetSystemTypeByName(doc, "Supply Air");
            PipeType curPipeType = GetPipeTypeByName(doc, "Default");
            DuctType curDuctType = GetDuctTypeByName(doc, "Default");


            using (Transaction t = new Transaction(doc))
            {
                t.Start("Create Walls");

                foreach (Element element in pickList)
                {
                    if (element is CurveElement)
                    {
                        CurveElement curve = (CurveElement)element;


                        curveList.Add(curve);

                        GraphicsStyle curGS = curve.LineStyle as GraphicsStyle;


                        switch (curGS.Name)
                        {

                            case "A-GLAZ":
                                Curve curCurve = NewMethod(curve);
                                Wall newWall = Wall.Create(doc, curCurve, curWallType1.Id, curLevel.Id, 10, 0, false, false);
                                break;

                            case "A-WALL":
                                Curve curCurve1 = NewMethod(curve);
                                Wall newWall2 = Wall.Create(doc, curCurve1, curWallType2.Id, curLevel.Id, 10, 0, false, false);
                                break;

                            case "M-DUCT":
                                Curve curCurve2 = NewMethod(curve);
                                XYZ startPoint = curCurve2.GetEndPoint(0);
                                XYZ endPoint = curCurve2.GetEndPoint(1);
                                Duct newDuct = Duct.Create(doc, curDuctSystemType.Id, curDuctType.Id, curLevel.Id, startPoint, endPoint);
                                break;

                            case "P-PIPE":
                                Curve curCurve3 = NewMethod(curve);
                                XYZ startPoint2 = curCurve3.GetEndPoint(0);
                                XYZ endPoint2 = curCurve3.GetEndPoint(1);
                                Pipe newPipe = Pipe.Create(doc, curPipeSystemType.Id, curPipeType.Id, curLevel.Id, startPoint2, endPoint2);
                                break;

                            case "<Medium Lines>":
                                doc.Delete(curve.Id);
                                break;

                        }


                    }
                }

                t.Commit();

            }

            

            TaskDialog.Show("Complete", curveList.Count.ToString());





         


            return Result.Succeeded;
        }

        private Curve NewMethod(CurveElement curve)
        {
            Curve curCurve = curve.GeometryCurve;
            //XYZ startPoint = curCurve.GetEndPoint(0);
            //XYZ endPoint = curCurve.GetEndPoint(1);
            return curCurve;


        }

        private WallType GetWallTypeByName(Document doc, string wallTypeName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(WallType));

            foreach (Element curElem in collector)
            {
                WallType wallType = curElem as WallType;

                if (wallType.Name == wallTypeName)
                    return wallType;
            }
            return null;
        }

        private Level GetLevelByName(Document doc, string levelName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(Level));

            foreach (Element curElem in collector)
            {
                Level level = curElem as Level;

                if (level.Name == levelName)
                    return level;
            }
            return null;
        }

        private MEPSystemType GetSystemTypeByName(Document doc, string typeName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(MEPSystemType));

            foreach (Element curElem in collector)
            {
                MEPSystemType curType = curElem as MEPSystemType;

                if (curType.Name == typeName)
                    return curType;
            }
            return null;
        }

        private PipeType GetPipeTypeByName(Document doc, string typeName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(PipeType));

            foreach (Element curElem in collector)
            {
                PipeType curType = curElem as PipeType;

                if (curType.Name == typeName)
                    return curType;
            }
            return null;
        }

        private DuctType GetDuctTypeByName(Document doc, string typeName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(DuctType));

            foreach (Element curElem in collector)
            {
                DuctType curType = curElem as DuctType;

                if (curType.Name == typeName)
                    return curType;
            }
            return null;
        }
    }
}
