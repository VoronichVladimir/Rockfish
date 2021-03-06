﻿using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Input.Custom;
using RockfishCommon;

namespace RockfishClient.Commands
{
  /// <summary>
  /// RF_PolylineFromPoints command
  /// </summary>
  [System.Runtime.InteropServices.Guid("50076313-0259-464A-9BC7-5B4FBC26A764")]
  public class PolylineFromPointsCommand : Command
  {
    /// <summary>
    /// Gets the command name.
    /// </summary>
    public override string EnglishName => "RF_PolylineFromPoints";

    /// <summary>
    /// Called by Rhino when the user wants to run the command.
    /// </summary>
    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
      var rc = RockfishClientPlugIn.VerifyServerHostName();
      if (rc != Result.Success)
        return rc;

      var go = new GetObject();
      go.SetCommandPrompt("Select points for polyline creation");
      go.GeometryFilter = ObjectType.Point;
      go.SubObjectSelect = false;
      go.GetMultiple(2, 0);
      if (go.CommandResult() != Result.Success)
        return go.CommandResult();

      var in_points = new List<RockfishPoint>(go.ObjectCount);
      foreach (var obj_ref in go.Objects())
      {
        var point = obj_ref.Point();
        if (null != point)
          in_points.Add(new RockfishPoint(point.Location));
      }

      if (in_points.Count < 2)
        return Result.Cancel;

      RockfishGeometry out_curve;
      try
      {
        RockfishClientPlugIn.ServerHostName();
        using (var channel = new RockfishClientChannel())
        {
          channel.Create();
          out_curve = channel.PolylineFromPoints(in_points.ToArray(), doc.ModelAbsoluteTolerance);
        }
      }
      catch (Exception ex)
      {
        RhinoApp.WriteLine(ex.Message);
        return Result.Failure;
      }

      if (null != out_curve?.Curve)
      {
        doc.Objects.AddCurve(out_curve.Curve);
        doc.Views.Redraw();
      }

      return Result.Success;
    }
  }
}