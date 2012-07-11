//
// BIM IFC library: this library works with Autodesk(R) Revit(R) to export IFC files containing model geometry.
// Copyright (C) 2012  Autodesk, Inc.
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.IFC;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit;

using BIM.IFC.Utility;
using BIM.IFC.Toolkit;
using BIM.IFC.Exporter.PropertySet;

namespace BIM.IFC.Exporter
{
    /// <summary>
    /// Provides methods to export MEP Connectors.
    /// </summary>
    class ConnectorExporter
    {
        /// <summary>
        /// Exports a connector instance. Almost verbatim exmaple from Revit 2012 API for Connector Class
        /// Works only for HVAC and Piping for now
        /// </summary>
        /// <param name="exporterIFC">The ExporterIFC object.</param>
        public static void Export(ExporterIFC exporterIFC)
        {
            foreach (ConnectorSet c in ExporterCacheManager.MEPCache.MEPConnectors)
            {
                Export(exporterIFC, c);
            }
            // clear local cache 
            ConnectorExporter.clearConnections();
        }

        /// <summary>
        /// Exports a connector instance. Almost verbatim exmaple from Revit 2012 API for Connector Class
        /// Works only for HVAC and Piping for now
        /// </summary>
        /// <param name="exporterIFC">The ExporterIFC object.</param>
        /// <param name="connectors">The ConnectorSet object.</param>
        private static void Export(ExporterIFC exporterIFC, ConnectorSet connectors)
        {
            IFCFile file = exporterIFC.GetFile();
            using (IFCTransaction tr = new IFCTransaction(file))
            {
                foreach (Connector connector in connectors)
                {
                    try
                    {
                        if (connector != null)
                        {
                            if (connector.Domain == Domain.DomainHvac || connector.Domain == Domain.DomainPiping)
                            {
                                if (connector.ConnectorType == ConnectorType.End ||
                                    connector.ConnectorType == ConnectorType.Curve ||
                                    connector.ConnectorType == ConnectorType.Physical)
                                {

                                    if (connector.IsConnected)
                                    {
                                        ConnectorSet connectorSet = connector.AllRefs;
                                        ConnectorSetIterator csi = connectorSet.ForwardIterator();

                                        while (csi.MoveNext())
                                        {
                                            Connector connected = csi.Current as Connector;
                                            if (connected != null && connected.Owner != null && connector.Owner != null)
                                            {
                                                if (connected.Owner.Id != connector.Owner.Id)
                                                {
                                                    // look for physical connections
                                                    if (connected.ConnectorType == ConnectorType.End ||
                                                        connected.ConnectorType == ConnectorType.Curve ||
                                                        connected.ConnectorType == ConnectorType.Physical)
                                                    {
                                                        if (connector.Direction == FlowDirectionType.Out)
                                                        {
                                                            AddConnection(file, exporterIFC, connected.Owner, connector.Owner, false);
                                                        }
                                                        else
                                                        {
                                                            bool isBiDirectional = (connector.Direction == FlowDirectionType.Bidirectional);
                                                            AddConnection(file, exporterIFC, connector.Owner, connected.Owner, isBiDirectional);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                            }
                        }
                    }
                    catch (System.Exception)
                    {
                        // Log an error here
                    }
                }
                tr.Commit();
            }
        }

        /// <summary>
        /// Add IFC Nodes
        /// </summary>
        /// <param name="ifcFile"></param>
        /// <param name="exporterIFC"></param>
        /// <param name="inElement"></param>
        /// <param name="outElement"></param>
        /// <param name="isBiDirectional"></param>
        static void AddConnection(IFCFile ifcFile, ExporterIFC exporterIFC, Element inElement, Element outElement, bool isBiDirectional)
        {
            // Check if the connection already exist
            if (connectionExists(inElement.Id, outElement.Id))
                return;

            if (isBiDirectional)
            {
                if (connectionExists(outElement.Id, inElement.Id))
                    return;
            }

            IFCAnyHandle inElementIFCHandle = ExporterCacheManager.MEPCache.Find(inElement.Id);
            IFCAnyHandle outElementIFCHandle = ExporterCacheManager.MEPCache.Find(outElement.Id);

            if (inElementIFCHandle == null || outElementIFCHandle == null ||
                !IFCAnyHandleUtil.IsSubTypeOf(inElementIFCHandle, IFCEntityType.IfcElement)
                || !IFCAnyHandleUtil.IsSubTypeOf(outElementIFCHandle, IFCEntityType.IfcElement))
                return;

            IFCAnyHandle ownerHistory = exporterIFC.GetOwnerHistoryHandle();
            IFCAnyHandle portOut = null;
            IFCAnyHandle portIn = null;
            // ----------------------- In Port ----------------------
            {
                // Make Source port
                IFCAnyHandle productRepresentation = null;
                string guid = ExporterIFCUtils.CreateGUID();
                string objType = IFCEntityType.IfcDistributionPort.ToString();
                IFCFlowDirection flowDir = (isBiDirectional) ? IFCFlowDirection.SourceAndSink : IFCFlowDirection.Sink;
                portIn = IFCInstanceExporter.CreateDistributionPort(ifcFile, guid, ownerHistory, null, null, objType, null, productRepresentation, flowDir);

                // Attach the port to the element
                guid = ExporterIFCUtils.CreateGUID();
                IFCAnyHandle connectorIn = IFCInstanceExporter.CreateRelConnectsPortToElement(ifcFile, guid, ownerHistory, null, null, portIn, inElementIFCHandle);
            }

            // ----------------------- Out Port----------------------
            {
                IFCAnyHandle productRepresentation = null;
                string guid = ExporterIFCUtils.CreateGUID();
                string objType = IFCEntityType.IfcDistributionPort.ToString();
                IFCFlowDirection flowDir = (isBiDirectional) ? IFCFlowDirection.SourceAndSink : IFCFlowDirection.Source;
                portOut = IFCInstanceExporter.CreateDistributionPort(ifcFile, guid, ownerHistory, null, null, objType, null, productRepresentation, flowDir);

                // Attach the port to the element
                guid = ExporterIFCUtils.CreateGUID();
                IFCAnyHandle connectorOut = IFCInstanceExporter.CreateRelConnectsPortToElement(ifcFile, guid, ownerHistory, null, null, portOut, outElementIFCHandle);
            }

            //  ----------------------- Out Port -> In Port ----------------------
            if (portOut != null && portIn != null)
            {
                string guid = ExporterIFCUtils.CreateGUID();
                IFCAnyHandle realizingElement = null;
                IFCInstanceExporter.CreateRelConnectsPorts(ifcFile, guid, ownerHistory, null, null, portIn, portOut, realizingElement);
                addConnection(inElement.Id, outElement.Id);
            }
        }

        /// <summary>
        /// Keeps track of created connection to prevent duplicate connections, 
        /// might not be necessary
        /// </summary>
        private static HashSet<string> m_connectionExists = new HashSet<string>();

        /// <summary>
        /// Checks existance of the connects
        /// </summary>
        /// <param name="inID">ElementId of the incoming Element</param>
        /// <param name="outID">ElementId of the outgoing Element</param>
        /// <returns>True if the connection exists already</returns>
        private static bool connectionExists(ElementId inID, ElementId outID)
        {
            string elementIdKey = inID.ToString() + "_" + outID.ToString();
            return m_connectionExists.Contains(elementIdKey);
        }

        /// <summary>
        /// Add new Connection
        /// </summary>
        /// <param name="inID"></param>
        /// <param name="outID"></param>
        private static void addConnection(ElementId inID, ElementId outID)
        {
            string elementIdKey = inID.ToString() + "_" + outID.ToString();
            m_connectionExists.Add(elementIdKey);
        }

        /// <summary>
        /// Clear the connection cache
        /// </summary>
        public static void clearConnections()
        {
            m_connectionExists.Clear();
        }
    }
}
