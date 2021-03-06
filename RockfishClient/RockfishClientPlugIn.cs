﻿using System;
using System.Net;
using Rhino;
using Rhino.Commands;

namespace RockfishClient
{
  /// <summary>
  /// RockfishClientPlugIn plug-in class
  /// </summary>
  public class RockfishClientPlugIn : Rhino.PlugIns.PlugIn
  {
    private static string g_server_host_name;

    /// <summary>
    /// Public constructor (called by Rhino)
    /// </summary>
    public RockfishClientPlugIn()
    {
      ThePlugIn = this;
    }

    /// <summary> 
    /// Gets the one and only instance of the RockfishClientPlugIn object.
    /// </summary>
    public static RockfishClientPlugIn ThePlugIn { get; private set; }

    /// <summary>
    /// Called by various commands to verify the server host name is set.
    /// </summary>
    public static Result VerifyServerHostName()
    {
      var rc = string.IsNullOrEmpty(ServerHostName());
      if (rc)
        RhinoApp.WriteLine("Run the \"RockfishSetServer\" command to set the server host name.");
      return rc ? Result.Cancel : Result.Success;
    }

    /// <summary>
    /// Gets the server host name.
    /// </summary>
    public static string ServerHostName()
    {
      if (!string.IsNullOrEmpty(g_server_host_name))
        return g_server_host_name;

      if (ThePlugIn.Settings.TryGetString("ServerHostName", out string host_name))
        g_server_host_name = LookupHostName(host_name);

      return g_server_host_name;
    }

    /// <summary>
    /// Sets the server host name.
    /// </summary>
    /// <param name="serverHostName">The server host name.</param>
    public void SetServerHostName(string serverHostName)
    {
      g_server_host_name = serverHostName;
      Settings.SetString("ServerHostName", g_server_host_name);
    }

    /// <summary>
    /// Resolves a host name to a canonical host name.
    /// </summary>
    public static string LookupHostName(string hostName)
    {
      if (string.IsNullOrEmpty(hostName))
        return null;

      // If the string parses as an IP address, return.
      if (IPAddress.TryParse(hostName, out IPAddress address))
        return hostName;

      try
      {
        var host_entry = Dns.GetHostEntry(hostName);
        return host_entry.HostName;
      }
      catch
      {
        // ignored
      }

      return null;
    }

    /// <summary>
    /// Returns an id that allows events to be aggregated by user. 
    /// There is no way to determine who the end user is based on this 
    /// id, unless the user tells you their id.
    /// </summary>
    public static string ClientId
    {
      get
      {
        var empty = Guid.Empty.ToString();
        var id = ThePlugIn.Settings.GetString("ClientId", empty);
        if (id.Equals(empty, StringComparison.OrdinalIgnoreCase))
        {
          id = Guid.NewGuid().ToString();
          ThePlugIn.Settings.SetString("ClientId", id);
        }
        return id;
      }
    }
  }
}