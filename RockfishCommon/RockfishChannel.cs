﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace RockfishCommon
{
  public class RockfishChannel : IDisposable
  {
    private ChannelFactory<IRockfishService> m_factory;
    private IRockfishService m_channel;
    private NetNamedPipeBinding m_binding;
    private EndpointAddress m_endpoint;
    private readonly object m_locker;
    private bool m_disposed;

    /// <summary>
    /// Constructor
    /// </summary>
    public RockfishChannel()
    {
      m_locker = new object();
      m_disposed = false;
    }

    /// <summary>
    /// Public creator
    /// </summary>
    public bool Create()
    {
      var rc = false;
      try
      {
        m_binding = new NetNamedPipeBinding();
        m_endpoint = new EndpointAddress("net.pipe://localhost/mcneel/rockfishserver/1/server/pipe");
        m_factory = new ChannelFactory<IRockfishService>(m_binding, m_endpoint);
        m_channel = m_factory.CreateChannel();
        rc = true;
      }
      catch (Exception ex)
      {
        ThrowCreationException(ex);
        Dispose();
      }
      return rc;
    }

    /// <summary>
    /// Simple test to see if the Rockfish service is operational
    /// </summary>
    public string Echo(string str)
    {
      if (IsValid)
      {
        try
        {
          var result = m_channel.Echo(str);
          return result;
        }
        catch (Exception ex)
        {
          HandleException(ex);
          Dispose();
        }
      }
      return null;
    }

    /// <summary>
    /// 
    /// </summary>
    public Guid AddCurve(RockfishGeometry geometry)
    {
      if (IsValid)
      {
        try
        {
          var result = m_channel.AddCurve(geometry);
          return result;
        }
        catch (Exception ex)
        {
          HandleException(ex);
          Dispose();
        }
      }
      return Guid.Empty;
    }

    public RockfishGeometry[] IntersectBreps(RockfishGeometry brep0, RockfishGeometry brep1, double tolerance)
    {
      if (IsValid)
      {
        try
        {
          var result = m_channel.IntersectBreps(brep0, brep1, tolerance);
          return result;
        }
        catch (Exception ex)
        {
          HandleException(ex);
          Dispose();
        }
      }
      return new RockfishGeometry[0];

    }

    /// <summary>
    /// Object validator
    /// </summary>
    public bool IsValid => null != m_factory && null != m_channel && false == m_disposed;

    /// <summary>
    /// Exception handler
    /// </summary>
    private static void HandleException(Exception ex)
    {
      if (ex is FaultException)
      {
        ThrowFaultException((FaultException)ex);
      }
      else if (ex is CommunicationException)
      {
        ThrowCommunicationException((CommunicationException)ex);
      }
      else if (ex is TimeoutException)
      {
        ThrowTimeoutException((TimeoutException)ex);
      }
      else
      {
        ThrowGeneralException(ex);
      }
    }

    /// <summary>
    /// Handles creation exceptions
    /// </summary>
    private static void ThrowCreationException(Exception ex)
    {
      Debug.WriteLine(ex.Message + "\n" + ex.StackTrace);
      const string message = "There was a problem creating the communication channel.";
      throw new RockfishChannelException(message);
    }

    /// <summary>
    /// Handles fault exceptions
    /// </summary>
    private static void ThrowFaultException(FaultException ex)
    {
      Debug.WriteLine(ex.Message + "\n" + ex.StackTrace);
      throw new RockfishChannelException(ex.Message);
    }

    /// <summary>
    /// Handles communication exceptions
    /// </summary>
    private static void ThrowCommunicationException(CommunicationException ex)
    {
      Debug.WriteLine(ex.Message + "\n" + ex.StackTrace);
      const string message = "There was a problem communicating with the service.";
      throw new RockfishChannelException(message);
    }

    /// <summary>
    /// Handles timeout exceptions
    /// </summary>
    private static void ThrowTimeoutException(TimeoutException ex)
    {
      Debug.WriteLine(ex.Message + "\n" + ex.StackTrace);
      const string message = "The service operation has timed out.";
      throw new RockfishChannelException(message);
    }

    /// <summary>
    /// Handles generic exceptions
    /// </summary>
    private static void ThrowGeneralException(Exception ex)
    {
      Debug.WriteLine(ex.Message + "\n" + ex.StackTrace);
      const string message = "An unknown exception has occurred.";
      throw new RockfishChannelException(message);
    }

    /// <inheritdoc />
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose
    /// </summary>
    private void Dispose(bool disposing)
    {
      if (!m_disposed)
      {
        if (disposing)
        {
          lock (m_locker)
          {
            if (null != m_channel)
            {
              ((IClientChannel) m_channel).Abort();
              m_channel = null;
            }

            if (null != m_factory)
            {
              m_factory.Abort();
              m_factory = null;
            }

            m_endpoint = null;
            m_binding = null;
          }

          m_disposed = true;
        }
      }
    }
  }
}