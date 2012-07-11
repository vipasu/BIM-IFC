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
using Autodesk.Revit.DB.IFC;
using BIM.IFC.Utility;

namespace BIM.IFC.Toolkit
{
    class IFCInstanceExporter
    {
        /////////////////////////////////////////////////////////////////
        // SetXXX method is to set base entities' attributes.
        // Every SetXXX method has a corresponding ValidateXXX method.
        // ValidateXXX is to validate the parameters for the attributes.
        // ValidateXXX should not be called in SetXXX. It must be called in CreateXXX method.
        // This is to make sure all arguments are valid BEFORE create an instance.
        // So we have below layout for these methods:
        //   ValidateABCBaseEntity(...) { ... }
        //   SetABCBaseEntity(...) { ... }
        //   CreateABCEntity(...)
        //      {
        //         //Code to validate ABC entity's own parameters goes here.
        //         ValidateABCBaseEntity(...);
        //         //Code to create ABC instance goes here.
        //         //Code to set ABC entity's own attributes goes here.
        //         SetABCBaseEntity(...);
        //      }
        ///////////////////////////////////////////////////////////////

        private static IFCAnyHandle CreateInstance(IFCFile file, IFCEntityType type)
        {
            IFCAnyHandle hnd = file.CreateInstance(type.ToString());
            ExporterCacheManager.HandleTypeCache[hnd] = type;
            return hnd;
        }

        #region private validation and set methods goes here

        /// <summary>
        /// Validates the values to be set to IfcRoot.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        private static void ValidateRoot(string guid, IFCAnyHandle ownerHistory, string name, string description)
        {
            if (String.IsNullOrEmpty(guid))
                throw new ArgumentException("Invalid guid.", "guid");

            IFCAnyHandleUtil.ValidateSubTypeOf(ownerHistory, false, IFCEntityType.IfcOwnerHistory);
        }

        /// <summary>
        /// Sets attributes to IfcRoot.
        /// </summary>
        /// <param name="root">The IfcRoot.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        private static void SetRoot(IFCAnyHandle root,
            string guid, IFCAnyHandle ownerHistory, string name, string description)
        {
            IFCAnyHandleUtil.SetAttribute(root, "GlobalId", guid);
            IFCAnyHandleUtil.SetAttribute(root, "OwnerHistory", ownerHistory);
            IFCAnyHandleUtil.SetAttribute(root, "Name", name);
            IFCAnyHandleUtil.SetAttribute(root, "Description", description);
        }

        /// <summary>
        /// Validates the values to be set to IfcObjectDefinition.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        private static void ValidateObjectDefinition(string guid, IFCAnyHandle ownerHistory, string name, string description)
        {
            ValidateRoot(guid, ownerHistory, name, description);
        }

        /// <summary>
        /// Sets attributes to IfcObjectDefinition.
        /// </summary>
        /// <param name="objectDefinition">The IfcObjectDefinition.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        private static void SetObjectDefinition(IFCAnyHandle objectDefinition,
            string guid, IFCAnyHandle ownerHistory, string name, string description)
        {
            SetRoot(objectDefinition, guid, ownerHistory, name, description);
        }

        /// <summary>
        /// Validates the values to be set to IfcTypeObject.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        private static void ValidateTypeObject(string guid, IFCAnyHandle ownerHistory, string name, string description,
            string applicableOccurrence, HashSet<IFCAnyHandle> propertySets)
        {
            //applicableOccurrence can be optional
            IFCAnyHandleUtil.ValidateSubTypeOf(propertySets, true, IFCEntityType.IfcPropertySetDefinition);

            if (ExporterCacheManager.ExportOptionsCache.ExportAs2x2)
                ValidatePropertyDefinition(guid, ownerHistory, name, description);
            else
                ValidateObjectDefinition(guid, ownerHistory, name, description);
        }

        /// <summary>
        /// Sets attributes to IfcTypeObject.
        /// </summary>
        /// <param name="typeObject">The IfcTypeObject.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        private static void SetTypeObject(IFCAnyHandle typeObject,
            string guid, IFCAnyHandle ownerHistory, string name, string description,
            string applicableOccurrence, HashSet<IFCAnyHandle> propertySets)
        {
            IFCAnyHandleUtil.SetAttribute(typeObject, "ApplicableOccurrence", applicableOccurrence);
            if (propertySets != null && propertySets.Count > 0)
                IFCAnyHandleUtil.SetAttribute(typeObject, "HasPropertySets", propertySets);

            if (ExporterCacheManager.ExportOptionsCache.ExportAs2x2)
                SetPropertyDefinition(typeObject, guid, ownerHistory, name, description);
            else
                SetObjectDefinition(typeObject, guid, ownerHistory, name, description);
        }

        /// <summary>
        /// Validates the values to be set to IfcTypeProduct.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        private static void ValidateTypeProduct(string guid, IFCAnyHandle ownerHistory, string name, string description,
            string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag)
        {
            //elementTag can be optional
            IFCAnyHandleUtil.ValidateSubTypeOf(representationMaps, true, IFCEntityType.IfcRepresentationMap);

            ValidateTypeObject(guid, ownerHistory, name, description, applicableOccurrence, propertySets);
        }

        /// <summary>
        /// Sets attributes to IfcTypeProduct.
        /// </summary>
        /// <param name="typeProduct">The IfcTypeProduct.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        private static void SetTypeProduct(IFCAnyHandle typeProduct,
            string guid, IFCAnyHandle ownerHistory, string name, string description,
            string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag)
        {
            IFCAnyHandleUtil.SetAttribute(typeProduct, "RepresentationMaps", representationMaps);
            IFCAnyHandleUtil.SetAttribute(typeProduct, "Tag", elementTag);

            SetTypeObject(typeProduct, guid, ownerHistory, name, description, applicableOccurrence, propertySets);
        }

        /// <summary>
        /// Validates the values to be set to IfcElementType.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        private static void ValidateElementType(string guid, IFCAnyHandle ownerHistory, string name, string description,
            string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType)
        {
            ValidateTypeProduct(guid, ownerHistory, name, description, applicableOccurrence,
                propertySets, representationMaps, elementTag);
        }

        /// <summary>
        /// Sets attributes to IfcElementType.
        /// </summary>
        /// <param name="elementType">The IfcElementType.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        private static void SetElementType(IFCAnyHandle elementType,
            string guid, IFCAnyHandle ownerHistory, string name, string description,
            string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string type)
        {
            IFCAnyHandleUtil.SetAttribute(elementType, "ElementType", type);

            SetTypeProduct(elementType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag);
        }

        /// <summary>
        /// Validates the values to be set to IfcPropertyDefinition.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        private static void ValidatePropertyDefinition(string guid, IFCAnyHandle ownerHistory, string name, string description)
        {
            ValidateRoot(guid, ownerHistory, name, description);
        }

        /// <summary>
        /// Sets attributes to IfcPropertyDefinition.
        /// </summary>
        /// <param name="propertyDefinition">The IfcPropertyDefinition.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        private static void SetPropertyDefinition(IFCAnyHandle propertyDefinition,
            string guid, IFCAnyHandle ownerHistory, string name, string description)
        {
            SetRoot(propertyDefinition, guid, ownerHistory, name, description);
        }

        /// <summary>
        /// Validates the values to be set to IfcRelationship.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        private static void ValidateRelationship(string guid, IFCAnyHandle ownerHistory, string name, string description)
        {
            ValidateRoot(guid, ownerHistory, name, description);
        }

        /// <summary>
        /// Sets attributes to IfcRelationship.
        /// </summary>
        /// <param name="relationship">The IfcRelationship.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        private static void SetRelationship(IFCAnyHandle relationship,
            string guid, IFCAnyHandle ownerHistory, string name, string description)
        {
            SetRoot(relationship, guid, ownerHistory, name, description);
        }

        /// <summary>
        /// Validates the values to be set to IfcPropertySetDefinition.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        private static void ValidatePropertySetDefinition(string guid, IFCAnyHandle ownerHistory, string name, string description)
        {
            ValidatePropertyDefinition(guid, ownerHistory, name, description);
        }

        /// <summary>
        /// Sets attributes to IfcPropertySetDefinition.
        /// </summary>
        /// <param name="propertySetDefinition">The IfcPropertySetDefinition.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        private static void SetPropertySetDefinition(IFCAnyHandle propertySetDefinition,
            string guid, IFCAnyHandle ownerHistory, string name, string description)
        {
            SetPropertyDefinition(propertySetDefinition, guid, ownerHistory, name, description);
        }

        /// <summary>
        /// Validates the values to be set to IfcRelAssociates.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="relatedObjects">The objects to be related to the material.</param>
        private static void ValidateRelAssociates(string guid, IFCAnyHandle ownerHistory, string name, string description, HashSet<IFCAnyHandle> relatedObjects)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(relatedObjects, false, IFCEntityType.IfcRoot);

            ValidateRelationship(guid, ownerHistory, name, description);
        }

        /// <summary>
        /// Sets attributes to IfcRelAssociates.
        /// </summary>
        /// <param name="relAssociates">The IfcRelAssociates.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="relatedObjects">The objects to be related to the material.</param>
        private static void SetRelAssociates(IFCAnyHandle relAssociates,
            string guid, IFCAnyHandle ownerHistory, string name, string description, HashSet<IFCAnyHandle> relatedObjects)
        {
            IFCAnyHandleUtil.SetAttribute(relAssociates, "RelatedObjects", relatedObjects);
            SetRelationship(relAssociates, guid, ownerHistory, name, description);
        }

        /// <summary>
        /// Validates the values to be set to IfcRelDefines.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="relatedObjects">The objects to be related to a type.</param>
        private static void ValidateRelDefines(string guid, IFCAnyHandle ownerHistory, string name, string description, HashSet<IFCAnyHandle> relatedObjects)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(relatedObjects, false, IFCEntityType.IfcObject);

            ValidateRelationship(guid, ownerHistory, name, description);
        }

        /// <summary>
        /// Sets attributes to IfcRelDefines.
        /// </summary>
        /// <param name="relDefines">The IfcRelDefines.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="relatedObjects">The objects to be related to a type.</param>
        private static void SetRelDefines(IFCAnyHandle relDefines,
            string guid, IFCAnyHandle ownerHistory, string name, string description, HashSet<IFCAnyHandle> relatedObjects)
        {
            IFCAnyHandleUtil.SetAttribute(relDefines, "RelatedObjects", relatedObjects);
            SetRelationship(relDefines, guid, ownerHistory, name, description);
        }

        /// <summary>
        /// Validates the values to be set to IfcRelDecomposes.
        /// </summary>
        /// <param name="guid">The GUID for the entity.</param>
        /// <param name="ownerHistory">The IfcOwnerHistory.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="relatingObject">The element to which the structure contributes.</param>
        /// <param name="relatedObjects">The elements that make up the structure.</param>
        private static void ValidateRelDecomposes(string guid, IFCAnyHandle ownerHistory, string name, string description,
            IFCAnyHandle relatingObject, HashSet<IFCAnyHandle> relatedObjects)
        {
            if (ExporterCacheManager.ExportOptionsCache.ExportAs2x2)
            {
                IFCAnyHandleUtil.ValidateSubTypeOf(relatingObject, false, IFCEntityType.IfcObject);

                IFCAnyHandleUtil.ValidateSubTypeOf(relatedObjects, false, IFCEntityType.IfcObject);
            }
            else
            {
                IFCAnyHandleUtil.ValidateSubTypeOf(relatingObject, false, IFCEntityType.IfcObjectDefinition);
                IFCAnyHandleUtil.ValidateSubTypeOf(relatedObjects, false, IFCEntityType.IfcObjectDefinition);
            }

            ValidateRelationship(guid, ownerHistory, name, description);
        }

        /// <summary>
        /// Sets attributes to IfcRelDecomposes.
        /// </summary>
        /// <param name="relDecomposes">The IfcRelDecomposes.</param>
        /// <param name="guid">The GUID for the entity.</param>
        /// <param name="ownerHistory">The IfcOwnerHistory.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="relatingObject">The element to which the structure contributes.</param>
        /// <param name="relatedObjects">The elements that make up the structure.</param>
        private static void SetRelDecomposes(IFCAnyHandle relDecomposes,
            string guid, IFCAnyHandle ownerHistory, string name, string description,
            IFCAnyHandle relatingObject, HashSet<IFCAnyHandle> relatedObjects)
        {
            IFCAnyHandleUtil.SetAttribute(relDecomposes, "RelatingObject", relatingObject);
            IFCAnyHandleUtil.SetAttribute(relDecomposes, "RelatedObjects", relatedObjects);
            SetRelationship(relDecomposes, guid, ownerHistory, name, description);
        }

        /// <summary>
        /// Validates the values to be set to IfcRelConnects.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        private static void ValidateRelConnects(string guid, IFCAnyHandle ownerHistory, string name, string description)
        {
            ValidateRelationship(guid, ownerHistory, name, description);
        }

        /// <summary>
        /// Sets attributes to IfcRelConnects.
        /// </summary>
        /// <param name="relConnects">The IfcRelConnects.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        private static void SetRelConnects(IFCAnyHandle relConnects,
            string guid, IFCAnyHandle ownerHistory, string name, string description)
        {
            SetRelationship(relConnects, guid, ownerHistory, name, description);
        }

        /// <summary>
        /// Validates the values to be set to IfcObject.
        /// </summary>
        /// <param name="guid">The GUID to use to label the wall.</param>
        /// <param name="ownerHistory">The IfcOwnerHistory.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        private static void ValidateObject(string guid, IFCAnyHandle ownerHistory, string name, string description, string objectType)
        {
            if (ExporterCacheManager.ExportOptionsCache.ExportAs2x2)
                ValidateRoot(guid, ownerHistory, name, description);
            else
                ValidateObjectDefinition(guid, ownerHistory, name, description);
        }

        /// <summary>
        /// Sets attributes to IfcObject.
        /// </summary>
        /// <param name="obj">The IfcObject.</param>
        /// <param name="guid">The GUID to use to label the wall.</param>
        /// <param name="ownerHistory">The IfcOwnerHistory.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        private static void SetObject(IFCAnyHandle obj,
            string guid, IFCAnyHandle ownerHistory, string name, string description, string objectType)
        {
            IFCAnyHandleUtil.SetAttribute(obj, "ObjectType", objectType);

            if (ExporterCacheManager.ExportOptionsCache.ExportAs2x2)
                SetRoot(obj, guid, ownerHistory, name, description);
            else
                SetObjectDefinition(obj, guid, ownerHistory, name, description);
        }

        /// <summary>
        /// Validates the values to be set to IfcProduct.
        /// </summary>
        /// <param name="guid">The GUID to use to label the wall.</param>
        /// <param name="ownerHistory">The IfcOwnerHistory.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The local placement.</param>
        /// <param name="representation">The representation object assigned to the wall.</param>
        private static void ValidateProduct(string guid, IFCAnyHandle ownerHistory, string name, string description, string objectType,
            IFCAnyHandle objectPlacement, IFCAnyHandle representation)
        {
            //objectPlacement can be optional
            IFCAnyHandleUtil.ValidateSubTypeOf(objectPlacement, true, IFCEntityType.IfcLocalPlacement);

            //representation can be optional
            IFCAnyHandleUtil.ValidateSubTypeOf(representation, true, IFCEntityType.IfcProductRepresentation);

            ValidateObject(guid, ownerHistory, name, description, objectType);
        }

        /// <summary>
        /// Sets attributes to IfcProduct.
        /// </summary>
        /// <param name="product">The IfcProduct.</param>
        /// <param name="guid">The GUID to use to label the wall.</param>
        /// <param name="ownerHistory">The IfcOwnerHistory.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The local placement.</param>
        /// <param name="representation">The representation object assigned to the wall.</param>
        private static void SetProduct(IFCAnyHandle product,
            string guid, IFCAnyHandle ownerHistory, string name, string description, string objectType,
            IFCAnyHandle objectPlacement, IFCAnyHandle representation)
        {
            IFCAnyHandleUtil.SetAttribute(product, "ObjectPlacement", objectPlacement);
            IFCAnyHandleUtil.SetAttribute(product, "Representation", representation);
            SetObject(product, guid, ownerHistory, name, description, objectType);
        }

        /// <summary>
        /// Validates the values to be set to IfcGroup.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        private static void ValidateGroup(string guid, IFCAnyHandle ownerHistory, string name, string description, string objectType)
        {
            ValidateObject(guid, ownerHistory, name, description, objectType);
        }

        /// <summary>
        /// Sets attributes to IfcGroup.
        /// </summary>
        /// <param name="group">The IfcGroup.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        private static void SetGroup(IFCAnyHandle group,
            string guid, IFCAnyHandle ownerHistory, string name, string description, string objectType)
        {
            SetObject(group, guid, ownerHistory, name, description, objectType);
        }

        /// <summary>
        /// Validates the values to be set to IfcElement.
        /// </summary>
        /// <param name="guid">The GUID to use to label the wall.</param>
        /// <param name="ownerHistory">The IfcOwnerHistory.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The local placement.</param>
        /// <param name="representation">The representation object assigned to the wall.</param>
        /// <param name="elementTag">The tag for the identifier of the element.</param>
        private static void ValidateElement(string guid, IFCAnyHandle ownerHistory, string name, string description, string objectType,
            IFCAnyHandle objectPlacement, IFCAnyHandle representation, string elementTag)
        {
            //Tag can be optional

            ValidateProduct(guid, ownerHistory, name, description, objectType, objectPlacement, representation);
        }

        /// <summary>
        /// Sets attributes to IfcElement.
        /// </summary>
        /// <param name="element">The IfcElement.</param>
        /// <param name="guid">The GUID to use to label the wall.</param>
        /// <param name="ownerHistory">The IfcOwnerHistory.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The local placement.</param>
        /// <param name="representation">The representation object assigned to the wall.</param>
        /// <param name="elementTag">The tag for the identifier of the element.</param>
        private static void SetElement(IFCAnyHandle element,
            string guid, IFCAnyHandle ownerHistory, string name, string description, string objectType,
            IFCAnyHandle objectPlacement, IFCAnyHandle representation, string elementTag)
        {
            IFCAnyHandleUtil.SetAttribute(element, "Tag", elementTag);
            SetProduct(element, guid, ownerHistory, name, description, objectType, objectPlacement, representation);
        }

        /// <summary>
        /// Validates the values to be set to IfcSpatialStructureElement.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The object placement.</param>
        /// <param name="representation">The representation object.</param>
        /// <param name="longName">The long name.</param>
        /// <param name="compositionType">The composition type.</param>
        private static void ValidateSpatialStructureElement(string guid, IFCAnyHandle ownerHistory, string name, string description, string objectType,
            IFCAnyHandle objectPlacement, IFCAnyHandle representation, string longName, IFCElementComposition compositionType)
        {
            //longName can be optional

            ValidateProduct(guid, ownerHistory, name, description, objectType, objectPlacement, representation);
        }

        /// <summary>
        /// Sets attributes to IfcSpatialStructureElement.
        /// </summary>
        /// <param name="spatialStructureElement">The IfcSpatialStructureElement.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The object placement.</param>
        /// <param name="representation">The representation object.</param>
        /// <param name="longName">The long name.</param>
        /// <param name="compositionType">The composition type.</param>
        private static void SetSpatialStructureElement(IFCAnyHandle spatialStructureElement,
            string guid, IFCAnyHandle ownerHistory, string name, string description, string objectType,
            IFCAnyHandle objectPlacement, IFCAnyHandle representation, string longName, IFCElementComposition compositionType)
        {
            IFCAnyHandleUtil.SetAttribute(spatialStructureElement, "LongName", longName);
            IFCAnyHandleUtil.SetAttribute(spatialStructureElement, "CompositionType", compositionType);
            SetProduct(spatialStructureElement, guid, ownerHistory, name, description, objectType, objectPlacement, representation);
        }

        /// <summary>
        /// Validates the values to be set to IfcRelConnectsElements.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="connectionGeometry">The geometric shape representation of the connection geometry.</param>
        /// <param name="relatingElement">Reference to a subtype of IfcElement that is connected by the connection relationship in the role of RelatingElement.</param>
        /// <param name="relatedElement">Reference to a subtype of IfcElement that is connected by the connection relationship in the role of RelatedElement.</param>
        private static void ValidateRelConnectsElements(string guid, IFCAnyHandle ownerHistory,
            string name, string description, IFCAnyHandle connectionGeometry, IFCAnyHandle relatingElement, IFCAnyHandle relatedElement)
        {
            //connectionGeometry can be optional
            IFCAnyHandleUtil.ValidateSubTypeOf(connectionGeometry, true, IFCEntityType.IfcConnectionGeometry);

            IFCAnyHandleUtil.ValidateSubTypeOf(relatingElement, false, IFCEntityType.IfcElement);

            IFCAnyHandleUtil.ValidateSubTypeOf(relatedElement, false, IFCEntityType.IfcElement);

            ValidateRelConnects(guid, ownerHistory, name, description);
        }

        /// <summary>
        /// Sets attributes to IfcRelConnectsElements.
        /// </summary>
        /// <param name="relConnectsElements">The IfcRelConnectsElements.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="connectionGeometry">The geometric shape representation of the connection geometry.</param>
        /// <param name="relatingElement">Reference to a subtype of IfcElement that is connected by the connection relationship in the role of RelatingElement.</param>
        /// <param name="relatedElement">Reference to a subtype of IfcElement that is connected by the connection relationship in the role of RelatedElement.</param>
        private static void SetRelConnectsElements(IFCAnyHandle relConnectsElements, string guid, IFCAnyHandle ownerHistory,
            string name, string description, IFCAnyHandle connectionGeometry, IFCAnyHandle relatingElement, IFCAnyHandle relatedElement)
        {
            IFCAnyHandleUtil.SetAttribute(relConnectsElements, "ConnectionGeometry", connectionGeometry);
            IFCAnyHandleUtil.SetAttribute(relConnectsElements, "RelatingElement", relatingElement);
            IFCAnyHandleUtil.SetAttribute(relConnectsElements, "RelatedElement", relatedElement);
            SetRelConnects(relConnectsElements, guid, ownerHistory, name, description);
        }

        /// <summary>
        /// Validates the values to be set to IfcRelAssigns.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="relatedObjects">Related objects, which are assigned to a single object.</param>
        /// <param name="relatedObjectsType">Particular type of the assignment relationship.</param>
        private static void ValidateRelAssigns(string guid, IFCAnyHandle ownerHistory,
            string name, string description, HashSet<IFCAnyHandle> relatedObjects, IFCObjectType? relatedObjectsType)
        {
            if (ExporterCacheManager.ExportOptionsCache.ExportAs2x2)
            {
                IFCAnyHandleUtil.ValidateSubTypeOf(relatedObjects, false, IFCEntityType.IfcObject);
            }
            else
            {
                IFCAnyHandleUtil.ValidateSubTypeOf(relatedObjects, false, IFCEntityType.IfcObjectDefinition);
            }

            ValidateRelationship(guid, ownerHistory, name, description);
        }

        /// <summary>
        /// Sets attributes to IfcRelAssigns.
        /// </summary>
        /// <param name="relAssigns">The IfcRelAssigns.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="relatedObjects">Related objects, which are assigned to a single object.</param>
        /// <param name="relatedObjectsType">Particular type of the assignment relationship.</param>
        private static void SetRelAssigns(IFCAnyHandle relAssigns, string guid, IFCAnyHandle ownerHistory,
            string name, string description, HashSet<IFCAnyHandle> relatedObjects, IFCObjectType? relatedObjectsType)
        {
            IFCAnyHandleUtil.SetAttribute(relAssigns, "RelatedObjects", relatedObjects);
            IFCAnyHandleUtil.SetAttribute(relAssigns, "RelatedObjectsType", relatedObjectsType);
            SetRelationship(relAssigns, guid, ownerHistory, name, description);
        }

        /// <summary>
        /// Validates the values to be set to IfcProductRepresentation.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="representations">The collection of representations assigned to the shape.</param>
        private static void ValidateProductRepresentation(string name, string description, IList<IFCAnyHandle> representations)
        {
            //name can be optional
            //description can be optional
            IFCAnyHandleUtil.ValidateSubTypeOf(representations, false, IFCEntityType.IfcRepresentation);
        }

        /// <summary>
        /// Sets attributes to IfcProductRepresentation.
        /// </summary>
        /// <param name="productDefinitionShape">The IfcProductRepresentation.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="representations">The collection of representations assigned to the shape.</param>
        private static void SetProductRepresentation(IFCAnyHandle productDefinitionShape,
            string name, string description, IList<IFCAnyHandle> representations)
        {
            IFCAnyHandleUtil.SetAttribute(productDefinitionShape, "Name", name);
            IFCAnyHandleUtil.SetAttribute(productDefinitionShape, "Description", description);
            IFCAnyHandleUtil.SetAttribute(productDefinitionShape, "Representations", representations);
        }

        /// <summary>
        /// Validates the values to be set to IfcProperty.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        private static void ValidateProperty(string name, string description)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            //description can be optional
        }

        /// <summary>
        /// Sets attributes to IfcProperty.
        /// </summary>
        /// <param name="property">The IfcProperty.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        private static void SetProperty(IFCAnyHandle property, string name, string description)
        {
            IFCAnyHandleUtil.SetAttribute(property, "Name", name);
            IFCAnyHandleUtil.SetAttribute(property, "Description", description);
        }

        /// <summary>
        /// Validates the values to be set to IfcRepresentationContext.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="type">The description of the type of a representation context.</param>
        private static void ValidateRepresentationContext(string identifier, string type)
        {
            //identifier can be optional
            //type can be optional
        }

        /// <summary>
        /// Sets attributes to IfcRepresentationContext.
        /// </summary>
        /// <param name="representationContext">The IfcRepresentationContext.</param>
        /// <param name="identifier">The identifier.</param>
        /// <param name="type">The description of the type of a representation context.</param>
        private static void SetRepresentationContext(IFCAnyHandle representationContext, string identifier, string type)
        {
            IFCAnyHandleUtil.SetAttribute(representationContext, "ContextIdentifier", identifier);
            IFCAnyHandleUtil.SetAttribute(representationContext, "ContextType", type);
        }

        /// <summary>
        /// Validates the values to be set to IfcConnectedFaceSet.
        /// </summary>
        /// <param name="faces">The collection of faces.</param>
        private static void ValidateConnectedFaceSet(HashSet<IFCAnyHandle> faces)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(faces, false, IFCEntityType.IfcFace);
        }

        /// <summary>
        /// Sets attributes to IfcConnectedFaceSet.
        /// </summary>
        /// <param name="connectedFaceSet">The IfcConnectedFaceSet.</param>
        /// <param name="faces">The collection of faces.</param>
        private static void SetConnectedFaceSet(IFCAnyHandle connectedFaceSet, HashSet<IFCAnyHandle> faces)
        {
            IFCAnyHandleUtil.SetAttribute(connectedFaceSet, "CfsFaces", faces);
        }

        /// <summary>
        /// Validates the values to be set to IfcGeometricSet.
        /// </summary>
        /// <param name="geometryElements">The collection of geometric elements.</param>
        private static void ValidateGeometricSet(HashSet<IFCAnyHandle> geometryElements)
        {
            if (geometryElements == null)
                throw new ArgumentNullException("geometryElements");
        }

        /// <summary>
        /// Sets attributes to IfcGeometricSet.
        /// </summary>
        /// <param name="geometricSet">The IfcGeometricSet.</param>
        /// <param name="geometryElements">The collection of geometric elements.</param>
        private static void SetGeometricSet(IFCAnyHandle geometricSet, HashSet<IFCAnyHandle> geometryElements)
        {
            IFCAnyHandleUtil.SetAttribute(geometricSet, "Elements", geometryElements);
        }

        /// <summary>
        /// Validates the values to be set to IfcAddress.
        /// </summary>
        /// <param name="purpose">Identifies the logical location of the address.</param>
        /// <param name="description">Text that relates the nature of the address.</param>
        /// <param name="userDefinedPurpose">Allows for specification of user specific purpose of the address.</param>
        private static void ValidateAddress(IFCAddressType? purpose, string description, string userDefinedPurpose)
        {
            //all can be optional
        }

        /// <summary>
        /// Sets attributes to IfcAddress.
        /// </summary>
        /// <param name="address">The IfcAddress.</param>
        /// <param name="purpose">Identifies the logical location of the address.</param>
        /// <param name="description">Text that relates the nature of the address.</param>
        /// <param name="userDefinedPurpose">Allows for specification of user specific purpose of the address.</param>
        private static void SetAddress(IFCAnyHandle address, IFCAddressType? purpose, string description, string userDefinedPurpose)
        {
            IFCAnyHandleUtil.SetAttribute(address, "Purpose", purpose);
            IFCAnyHandleUtil.SetAttribute(address, "Description", description);
            IFCAnyHandleUtil.SetAttribute(address, "UserDefinedPurpose", userDefinedPurpose);
        }

        /// <summary>
        /// Validates the values to be set to IfcNamedUnit.
        /// </summary>
        /// <param name="dimensions">The dimensions.</param>
        /// <param name="unitType">The type of the unit.</param>
        private static void ValidateNamedUnit(IFCAnyHandle dimensions, IFCUnit unitType)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(dimensions, false, IFCEntityType.IfcDimensionalExponents);
        }

        /// <summary>
        /// Sets attributes to IfcNamedUnit.
        /// </summary>
        /// <param name="namedUnit">The IfcNamedUnit.</param>
        /// <param name="dimensions">The dimensions.</param>
        /// <param name="unitType">The type of the unit.</param>
        private static void SetNamedUnit(IFCAnyHandle namedUnit, IFCAnyHandle dimensions, IFCUnit unitType)
        {
            IFCAnyHandleUtil.SetAttribute(namedUnit, "Dimensions", dimensions);
            IFCAnyHandleUtil.SetAttribute(namedUnit, "UnitType", unitType);
        }

        /// <summary>
        /// Validates the values to be set to IfcPlacement.
        /// </summary>
        /// <param name="location">The origin.</param>
        private static void ValidatePlacement(IFCAnyHandle location)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(location, false, IFCEntityType.IfcCartesianPoint);
        }

        /// <summary>
        /// Sets attributes to IfcPlacement.
        /// </summary>
        /// <param name="placement">The IfcPlacement.</param>
        /// <param name="location">The origin.</param>
        private static void SetPlacement(IFCAnyHandle placement, IFCAnyHandle location)
        {
            IFCAnyHandleUtil.SetAttribute(placement, "Location", location);
        }

        /// <summary>
        /// Validates the values to be set to IfcCartesianTransformationOperator.
        /// </summary>
        /// <param name="axis1">The X direction of the transformation coordinate system.</param>
        /// <param name="axis2">The Y direction of the transformation coordinate system.</param>
        /// <param name="localOrigin">The origin of the transformation coordinate system.</param>
        /// <param name="scale">The scale factor.</param>
        private static void ValidateCartesianTransformationOperator(IFCAnyHandle axis1, IFCAnyHandle axis2,
            IFCAnyHandle localOrigin, double? scale)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(axis1, true, IFCEntityType.IfcDirection);
            IFCAnyHandleUtil.ValidateSubTypeOf(axis2, true, IFCEntityType.IfcDirection);
            IFCAnyHandleUtil.ValidateSubTypeOf(localOrigin, false, IFCEntityType.IfcCartesianPoint);
        }

        /// <summary>
        /// Sets attributes to IfcCartesianTransformationOperator.
        /// </summary>
        /// <param name="cartesianTransformationOperator">The IfcCartesianTransformationOperator.</param>
        /// <param name="axis1">The X direction of the transformation coordinate system.</param>
        /// <param name="axis2">The Y direction of the transformation coordinate system.</param>
        /// <param name="localOrigin">The origin of the transformation coordinate system.</param>
        /// <param name="scale">The scale factor.</param>
        private static void SetCartesianTransformationOperator(IFCAnyHandle cartesianTransformationOperator, IFCAnyHandle axis1,
            IFCAnyHandle axis2, IFCAnyHandle localOrigin, double? scale)
        {
            IFCAnyHandleUtil.SetAttribute(cartesianTransformationOperator, "Axis1", axis1);
            IFCAnyHandleUtil.SetAttribute(cartesianTransformationOperator, "Axis2", axis2);
            IFCAnyHandleUtil.SetAttribute(cartesianTransformationOperator, "LocalOrigin", localOrigin);
            IFCAnyHandleUtil.SetAttribute(cartesianTransformationOperator, "Scale", scale);
        }

        /// <summary>
        /// Validates the values to be set to IfcManifoldSolidBrep.
        /// </summary>
        /// <param name="outer">The closed shell.</param>
        private static void ValidateManifoldSolidBrep(IFCAnyHandle outer)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(outer, false, IFCEntityType.IfcClosedShell);
        }

        /// <summary>
        /// Sets attributes to IfcManifoldSolidBrep.
        /// </summary>
        /// <param name="manifoldSolidBrep">The IfcManifoldSolidBrep.</param>
        /// <param name="outer">The closed shell.</param>
        private static void SetManifoldSolidBrep(IFCAnyHandle manifoldSolidBrep, IFCAnyHandle outer)
        {
            IFCAnyHandleUtil.SetAttribute(manifoldSolidBrep, "Outer", outer);
        }

        /// <summary>
        /// Validates the values to be set to IfcGeometricRepresentationContext.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="type">The description of the type of a representation context.</param>
        /// <param name="dimension">The integer dimension count of the coordinate space modeled in a geometric representation context.</param>
        /// <param name="precision">Value of the model precision for geometric models.</param>
        /// <param name="worldCoordinateSystem">Establishment of the engineering coordinate system (often referred to as the world coordinate system in CAD)
        /// for all representation contexts used by the project.</param>
        /// <param name="trueNorth">Direction of the true north relative to the underlying coordinate system.</param>
        private static void ValidateGeometricRepresentationContext(string identifier, string type, int dimension,
            double? precision, IFCAnyHandle worldCoordinateSystem, IFCAnyHandle trueNorth)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(worldCoordinateSystem, false, IFCEntityType.IfcAxis2Placement2D, IFCEntityType.IfcAxis2Placement3D);
            IFCAnyHandleUtil.ValidateSubTypeOf(trueNorth, true, IFCEntityType.IfcDirection);
            ValidateRepresentationContext(identifier, type);
        }

        /// <summary>
        /// Sets attributes to IfcGeometricRepresentationContext.
        /// </summary>
        /// <param name="geometricRepresentationContext">The IfcGeometricRepresentationContext.</param>
        /// <param name="identifier">The identifier.</param>
        /// <param name="type">The description of the type of a representation context.</param>
        /// <param name="dimension">The integer dimension count of the coordinate space modeled in a geometric representation context.</param>
        /// <param name="precision">Value of the model precision for geometric models.</param>
        /// <param name="worldCoordinateSystem">Establishment of the engineering coordinate system (often referred to as the world coordinate system in CAD)
        /// for all representation contexts used by the project.</param>
        /// <param name="trueNorth">Direction of the true north relative to the underlying coordinate system.</param>
        private static void SetGeometricRepresentationContext(IFCAnyHandle geometricRepresentationContext,
            string identifier, string type, int dimension, double? precision, IFCAnyHandle worldCoordinateSystem,
            IFCAnyHandle trueNorth)
        {
            IFCAnyHandleUtil.SetAttribute(geometricRepresentationContext, "CoordinateSpaceDimension", dimension);
            IFCAnyHandleUtil.SetAttribute(geometricRepresentationContext, "Precision", precision);
            IFCAnyHandleUtil.SetAttribute(geometricRepresentationContext, "WorldCoordinateSystem", worldCoordinateSystem);
            IFCAnyHandleUtil.SetAttribute(geometricRepresentationContext, "TrueNorth", trueNorth);
            SetRepresentationContext(geometricRepresentationContext, identifier, type);
        }

        /// <summary>
        /// Validates the values to be set to IfcPhysicalQuantity.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        private static void ValidatePhysicalQuantity(string name, string description)
        {
            if (name == null)
                throw new ArgumentNullException("name");
        }

        /// <summary>
        /// Sets attributes to IfcPhysicalQuantity.
        /// </summary>
        /// <param name="physicalQuantity">The IfcPhysicalQuantity.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        private static void SetPhysicalQuantity(IFCAnyHandle physicalQuantity, string name, string description)
        {
            IFCAnyHandleUtil.SetAttribute(physicalQuantity, "Name", name);
            IFCAnyHandleUtil.SetAttribute(physicalQuantity, "Description", description);
        }

        /// <summary>
        /// Validates the values to be set to IfcPhysicalSimpleQuantity.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="unit">The unit.</param>
        private static void ValidatePhysicalSimpleQuantity(string name, string description, IFCAnyHandle unit)
        {
            ValidatePhysicalQuantity(name, description);
            IFCAnyHandleUtil.ValidateSubTypeOf(unit, true, IFCEntityType.IfcNamedUnit);
        }

        /// <summary>
        /// Sets attributes to IfcPhysicalSimpleQuantity.
        /// </summary>
        /// <param name="physicalSimpleQuantity">The IfcPhysicalSimpleQuantity.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="unit">The unit.</param>
        private static void SetPhysicalSimpleQuantity(IFCAnyHandle physicalSimpleQuantity, string name, string description, IFCAnyHandle unit)
        {
            IFCAnyHandleUtil.SetAttribute(physicalSimpleQuantity, "Unit", unit);
            SetPhysicalQuantity(physicalSimpleQuantity, name, description);
        }

        /// <summary>
        /// Validates the values to be set to IfcRepresentation.
        /// </summary>
        /// <param name="contextOfItems">The context of the items.</param>
        /// <param name="identifier">The identifier.</param>
        /// <param name="type">The representation type.</param>
        /// <param name="items">The items that belong to the shape representation.</param>
        private static void ValidateRepresentation(IFCAnyHandle contextOfItems, string identifier, string type, HashSet<IFCAnyHandle> items)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(contextOfItems, false, IFCEntityType.IfcRepresentationContext);
            IFCAnyHandleUtil.ValidateSubTypeOf(items, false, IFCEntityType.IfcRepresentationItem);
        }

        /// <summary>
        /// Sets attributes to IfcRepresentation.
        /// </summary>
        /// <param name="representation">The IfcRepresentation.</param>
        /// <param name="contextOfItems">The context of the items.</param>
        /// <param name="identifier">The identifier.</param>
        /// <param name="type">The representation type.</param>
        /// <param name="items">The items that belong to the shape representation.</param>
        private static void SetRepresentation(IFCAnyHandle representation, IFCAnyHandle contextOfItems, string identifier, string type, HashSet<IFCAnyHandle> items)
        {
            IFCAnyHandleUtil.SetAttribute(representation, "ContextOfItems", contextOfItems);
            IFCAnyHandleUtil.SetAttribute(representation, "RepresentationIdentifier", identifier);
            IFCAnyHandleUtil.SetAttribute(representation, "RepresentationType", type);
            IFCAnyHandleUtil.SetAttribute(representation, "Items", items);
        }

        /// <summary>
        /// Validates the values to be set to IfcPresentationStyle.
        /// </summary>
        /// <param name="name">The name.</param>
        private static void ValidatePresentationStyle(string name)
        {
            //name can be optional
        }

        /// <summary>
        /// Sets attributes to IfcPresentationStyle.
        /// </summary>
        /// <param name="presentationStyle">The IfcPresentationStyle.</param>
        /// <param name="name">The name.</param>
        private static void SetPresentationStyle(IFCAnyHandle presentationStyle, string name)
        {
            IFCAnyHandleUtil.SetAttribute(presentationStyle, "Name", name);
        }

        /// <summary>
        /// Validates the values to be set to IfcPreDefinedItem.
        /// </summary>
        /// <param name="name">The name.</param>
        private static void ValidatePreDefinedItem(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
        }

        /// <summary>
        /// Sets attributes to IfcPreDefinedItem.
        /// </summary>
        /// <param name="preDefinedItem">The IfcPreDefinedItem.</param>
        /// <param name="name">The name.</param>
        private static void SetPreDefinedItem(IFCAnyHandle preDefinedItem, string name)
        {
            IFCAnyHandleUtil.SetAttribute(preDefinedItem, "Name", name);
        }

        /// <summary>
        /// Sets attributes to IfcExternalReference.
        /// </summary>
        /// <param name="externalReference">The IfcExternalReference.</param>
        /// <param name="location">Location of the reference (e.g. URL).</param>
        /// <param name="itemReference">Location of the item within the reference source.</param>
        /// <param name="name">Name of the reference.</param>
        private static void SetExternalReference(IFCAnyHandle externalReference,
           string location, string itemReference, string name)
        {
            IFCAnyHandleUtil.SetAttribute(externalReference, "Location", location);
            IFCAnyHandleUtil.SetAttribute(externalReference, "ItemReference", itemReference);
            IFCAnyHandleUtil.SetAttribute(externalReference, "Name", name);
        }

        /// <summary>
        /// Validates the values to be set to IfcActor.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="theActor">The actor.</param>
        private static void ValidateActor(string guid, IFCAnyHandle ownerHistory, string name, string description,
            string objectType, IFCAnyHandle theActor)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(theActor, false, IFCEntityType.IfcPerson, IFCEntityType.IfcPersonAndOrganization, IFCEntityType.IfcOrganization);
            ValidateObject(guid, ownerHistory, name, description, objectType);
        }

        /// <summary>
        /// Sets attributes to IfcActor.
        /// </summary>
        /// <param name="actor">The IfcActor.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="theActor">The actor.</param>
        private static void SetActor(IFCAnyHandle actor, string guid, IFCAnyHandle ownerHistory, string name, string description,
            string objectType, IFCAnyHandle theActor)
        {
            SetObject(actor, guid, ownerHistory, name, description, objectType);
            IFCAnyHandleUtil.SetAttribute(actor, "TheActor", theActor);
        }

        /// <summary>
        /// Validates the values to be set to IfcRelAssignsToActor.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="relatedObjects">Related objects, which are assigned to a single object.</param>
        /// <param name="relatedObjectsType">Particular type of the assignment relationship.</param>
        /// <param name="relatingActor">The actor.</param>
        /// <param name="actingRole">The role of the actor.</param>
        private static void ValidateRelAssignsToActor(string guid, IFCAnyHandle ownerHistory,
            string name, string description, HashSet<IFCAnyHandle> relatedObjects, IFCObjectType? relatedObjectsType,
            IFCAnyHandle relatingActor, IFCAnyHandle actingRole)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(relatingActor, false, IFCEntityType.IfcActor);
            IFCAnyHandleUtil.ValidateSubTypeOf(actingRole, true, IFCEntityType.IfcActorRole);

            ValidateRelAssigns(guid, ownerHistory, name, description, relatedObjects, relatedObjectsType);
        }

        /// <summary>
        /// Sets attributes to IfcRelAssignsToActor.
        /// </summary>
        /// <param name="relActor">The IfcRelAssignsToActor.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="relatedObjects">Related objects, which are assigned to a single object.</param>
        /// <param name="relatedObjectsType">Particular type of the assignment relationship.</param>
        /// <param name="relatingActor">The actor.</param>
        /// <param name="actingRole">The role of the actor.</param>
        private static void SetRelAssignsToActor(IFCAnyHandle relActor, string guid, IFCAnyHandle ownerHistory,
            string name, string description, HashSet<IFCAnyHandle> relatedObjects, IFCObjectType? relatedObjectsType,
            IFCAnyHandle relatingActor, IFCAnyHandle actingRole)
        {
            IFCAnyHandleUtil.SetAttribute(relActor, "RelatingActor", relatingActor);
            IFCAnyHandleUtil.SetAttribute(relActor, "ActingRole", actingRole);
            SetRelAssigns(relActor, guid, ownerHistory, name, description, relatedObjects, relatedObjectsType);
        }

        /// <summary>
        /// Sets attributes for IfcProfileDef.
        /// </summary>
        /// <param name="profileDef">The IfcProfileDef.</param>
        /// <param name="profileType">The profile type.</param>
        /// <param name="profileName">The profile name.</param>
        private static void SetProfileDef(IFCAnyHandle profileDef, IFCProfileType profileType, string profileName)
        {
            IFCAnyHandleUtil.SetAttribute(profileDef, "ProfileType", profileType);
            IFCAnyHandleUtil.SetAttribute(profileDef, "ProfileName", profileName);
        }

        /// <summary>
        /// Validates attributes for IfcParameterizedProfileDef.
        /// </summary>
        /// <param name="position">The profile position.</param>
        private static void ValidateParameterizedProfileDef(IFCAnyHandle position)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(position, false, IFCEntityType.IfcAxis2Placement2D);
        }

        /// <summary>
        /// Sets attributes for IfcParameterizedProfileDef.
        /// </summary>
        /// <param name="profileDef">The IfcProfileDef.</param>
        /// <param name="profileType">The profile type.</param>
        /// <param name="profileName">The profile name.</param>
        /// <param name="position">The profile position.</param>
        private static void SetParameterizedProfileDef(IFCAnyHandle profileDef, IFCProfileType profileType, string profileName, IFCAnyHandle position)
        {
            SetProfileDef(profileDef, profileType, profileName);
            IFCAnyHandleUtil.SetAttribute(profileDef, "Position", position);
        }

        /// <summary>
        /// Validates the attributes for IfcArbitraryClosedProfileDef.
        /// </summary>
        /// <param name="outerCurve">The outer curve, of type IfcCurve and non-null.</param>
        private static void ValidateArbitraryClosedProfileDef(IFCAnyHandle outerCurve)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(outerCurve, false, IFCEntityType.IfcCurve);
        }

        /// <summary>
        /// Sets attributes for IfcArbitraryClosedProfileDef.
        /// </summary>
        /// <param name="arbitraryClosedProfileDef">The IfcArbitraryClosedProfileDef.</param>
        /// <param name="profileType">The profile type.</param>
        /// <param name="profileName">The profile name.</param>
        /// <param name="outerCurve">The outer curve.</param>
        private static void SetArbitraryClosedProfileDef(IFCAnyHandle arbitraryClosedProfileDef, IFCProfileType profileType, string profileName,
            IFCAnyHandle outerCurve)
        {
            SetProfileDef(arbitraryClosedProfileDef, profileType, profileName);
            IFCAnyHandleUtil.SetAttribute(arbitraryClosedProfileDef, "OuterCurve", outerCurve);
        }

        /// <summary>
        /// Validates the attributes for IfcSweptAreaSolid.
        /// </summary>
        /// <param name="sweptArea">The profile.</param>
        /// <param name="sweptArea">The profile origin.</param>
        private static void ValidateSweptAreaSolid(IFCAnyHandle sweptArea, IFCAnyHandle position)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(sweptArea, false, IFCEntityType.IfcProfileDef);
            IFCAnyHandleUtil.ValidateSubTypeOf(position, false, IFCEntityType.IfcAxis2Placement3D);
        }

        /// <summary>
        /// Sets attributes for IfcSweptAreaSolid.
        /// </summary>
        /// <param name="SweptAreaSolid">The IfcSweptAreaSolid.</param>
        /// <param name="sweptArea">The profile.</param>
        /// <param name="position">The profile origin.</param>
        private static void SetSweptAreaSolid(IFCAnyHandle sweptAreaSolid, IFCAnyHandle sweptArea, IFCAnyHandle position)
        {
            IFCAnyHandleUtil.SetAttribute(sweptAreaSolid, "SweptArea", sweptArea);
            IFCAnyHandleUtil.SetAttribute(sweptAreaSolid, "Position", position);
        }

        #endregion

        #region public creation methods goes here

        /// <summary>
        /// Creates a handle representing an IfcWall and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID to use to label the wall.</param>
        /// <param name="ownerHistory">The IfcOwnerHistory.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The local placement.</param>
        /// <param name="representation">The representation object assigned to the wall.</param>
        /// <param name="elementTag">The tag for the identifier of the element.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateWall(IFCFile file, string guid, IFCAnyHandle ownerHistory,
            string name, string description, string objectType, IFCAnyHandle objectPlacement,
            IFCAnyHandle representation, string elementTag)
        {
            ValidateElement(guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);

            IFCAnyHandle wall = CreateInstance(file, IFCEntityType.IfcWall);
            SetElement(wall, guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);
            return wall;
        }

        /// <summary>
        /// Creates a handle representing an IfcWallStandardCase and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID to use to label the wall.</param>
        /// <param name="ownerHistory">The IfcOwnerHistory.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The local placement.</param>
        /// <param name="elementTag">The tag for the identifier of the element.</param>
        /// <param name="representation">The representation object assigned to the wall.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateWallStandardCase(IFCFile file, string guid, IFCAnyHandle ownerHistory,
            string name, string description, string objectType, IFCAnyHandle objectPlacement,
            IFCAnyHandle representation, string elementTag)
        {
            ValidateElement(guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);

            IFCAnyHandle wallStandardCase = CreateInstance(file, IFCEntityType.IfcWallStandardCase);
            SetElement(wallStandardCase, guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);
            return wallStandardCase;
        }

        /// <summary>
        /// Creates a handle representing an IfcProductDefinitionShape and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="representations">The collection of representations assigned to the shape.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateProductDefinitionShape(IFCFile file, string name, string description, IList<IFCAnyHandle> representations)
        {
            ValidateProductRepresentation(name, description, representations);

            IFCAnyHandle productDefinitionShape = CreateInstance(file, IFCEntityType.IfcProductDefinitionShape);
            SetProductRepresentation(productDefinitionShape, name, description, representations);
            return productDefinitionShape;
        }

        /// <summary>
        /// Creates a handle representing an IfcConnectedFaceSet and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="faces">The collection of faces.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateConnectedFaceSet(IFCFile file, HashSet<IFCAnyHandle> faces)
        {
            ValidateConnectedFaceSet(faces);

            IFCAnyHandle connectedFaceSet = CreateInstance(file, IFCEntityType.IfcConnectedFaceSet);
            SetConnectedFaceSet(connectedFaceSet, faces);
            return connectedFaceSet;
        }

        /// <summary>
        /// Creates a handle representing an IfcClosedShell and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="faces">The collection of faces.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateClosedShell(IFCFile file, HashSet<IFCAnyHandle> faces)
        {
            ValidateConnectedFaceSet(faces);

            IFCAnyHandle closedShell = CreateInstance(file, IFCEntityType.IfcClosedShell);
            SetConnectedFaceSet(closedShell, faces);
            return closedShell;
        }

        /// <summary>
        /// Creates a handle representing an IfcFaceBasedSurfaceModel and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="faces">The collection of faces.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateFaceBasedSurfaceModel(IFCFile file, HashSet<IFCAnyHandle> faces)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(faces, false, IFCEntityType.IfcConnectedFaceSet);

            IFCAnyHandle faceBasedSurfaceModel = CreateInstance(file, IFCEntityType.IfcFaceBasedSurfaceModel);
            IFCAnyHandleUtil.SetAttribute(faceBasedSurfaceModel, "FbsmFaces", faces);
            return faceBasedSurfaceModel;
        }

        /// <summary>
        /// Creates an IfcCovering, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID for the entity.</param>
        /// <param name="ownerHistory">The IfcOwnerHistory.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The local placement.</param>
        /// <param name="representation">The geometric representation of the entity, in the IfcProductRepresentation.</param>
        /// <param name="elementTag">The tag for the identifier of the element.</param>
        /// <param name="coveringType">The covering type.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateCovering(IFCFile file, string guid, IFCAnyHandle ownerHistory,
            string name, string description, string objectType, IFCAnyHandle objectPlacement,
            IFCAnyHandle representation, string elementTag, IFCCoveringType? coveringType)
        {
            //coveringType can be optional
            ValidateElement(guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);

            IFCAnyHandle covering = CreateInstance(file, IFCEntityType.IfcCovering);
            IFCAnyHandleUtil.SetAttribute(covering, "PredefinedType", coveringType);
            SetElement(covering, guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);
            return covering;
        }

        /// <summary>
        /// Creates an IfcFooting, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The object placement.</param>
        /// <param name="representation">The geometric representation of the entity, in the IfcProductRepresentation.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="predefinedType">The footing type.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateFooting(IFCFile file, string guid, IFCAnyHandle ownerHistory,
            string name, string description, string objectType, IFCAnyHandle objectPlacement,
            IFCAnyHandle representation, string elementTag, IFCFootingType predefinedType)
        {
            ValidateElement(guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);

            IFCAnyHandle footing = CreateInstance(file, IFCEntityType.IfcFooting);
            IFCAnyHandleUtil.SetAttribute(footing, "PredefinedType", predefinedType);
            SetElement(footing, guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);
            return footing;
        }

        /// <summary>
        /// Creates a handle representing an IfcSlab and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The geometric representation of the entity, in the IfcProductRepresentation.</param>
        /// <param name="representation"></param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="predefinedType">The slab type.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateSlab(IFCFile file,
            string guid, IFCAnyHandle ownerHistory, string name, string description, string objectType,
            IFCAnyHandle objectPlacement, IFCAnyHandle representation, string elementTag, IFCSlabType predefinedType)
        {
            ValidateElement(guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);

            IFCAnyHandle slab = CreateInstance(file, IFCEntityType.IfcSlab);
            IFCAnyHandleUtil.SetAttribute(slab, "PredefinedType", predefinedType);
            SetElement(slab, guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);
            return slab;
        }

        /// <summary>
        /// Creates a handle representing an IfcCurtainWall and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID for the entity.</param>
        /// <param name="ownerHistory">The IfcOwnerHistory.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The local placement.</param>
        /// <param name="representation">The geometric representation of the entity, in the IfcProductRepresentation.</param>
        /// <returns>The handle.</returns>
        /// <param name="elementTag">The tag for the identifier of the element.</param>
        public static IFCAnyHandle CreateCurtainWall(IFCFile file, string guid, IFCAnyHandle ownerHistory,
            string name, string description, string objectType, IFCAnyHandle objectPlacement,
            IFCAnyHandle representation, string elementTag)
        {
            ValidateElement(guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);

            IFCAnyHandle curtainWall = CreateInstance(file, IFCEntityType.IfcCurtainWall);
            SetElement(curtainWall, guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);
            return curtainWall;
        }

        /// <summary>
        /// Creates a handle representing an IfcRailing and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID for the entity.</param>
        /// <param name="ownerHistory">The IfcOwnerHistory.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The local placement.</param>
        /// <param name="representation">The geometric representation of the entity, in the IfcProductRepresentation.</param>
        /// <param name="elementTag">The tag for the identifier of the element.</param>
        /// <param name="predefinedType">The railing type.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateRailing(IFCFile file, string guid, IFCAnyHandle ownerHistory,
            string name, string description, string objectType, IFCAnyHandle objectPlacement,
            IFCAnyHandle representation, string elementTag, IFCRailingType predefinedType)
        {
            ValidateElement(guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);

            IFCAnyHandle railing = CreateInstance(file, IFCEntityType.IfcRailing);
            IFCAnyHandleUtil.SetAttribute(railing, "PredefinedType", predefinedType);
            SetElement(railing, guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);
            return railing;
        }

        /// <summary>
        /// Creates a handle representing an IfcRamp and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID for the entity.</param>
        /// <param name="ownerHistory">The IfcOwnerHistory.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The local placement.</param>
        /// <param name="representation">The geometric representation of the entity, in the IfcProductRepresentation.</param>
        /// <param name="elementTag">The tag for the identifier of the element.</param>
        /// <param name="shapeType">The ramp type.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateRamp(IFCFile file, string guid, IFCAnyHandle ownerHistory,
            string name, string description, string objectType, IFCAnyHandle objectPlacement,
            IFCAnyHandle representation, string elementTag, IFCRampType shapeType)
        {
            ValidateElement(guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);

            IFCAnyHandle ramp = CreateInstance(file, IFCEntityType.IfcRamp);
            IFCAnyHandleUtil.SetAttribute(ramp, "ShapeType", shapeType);
            SetElement(ramp, guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);
            return ramp;
        }

        /// <summary>
        /// Creates a handle representing an IfcRoof and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID for the entity.</param>
        /// <param name="ownerHistory">The IfcOwnerHistory.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The local placement.</param>
        /// <param name="representation">The geometric representation of the entity, in the IfcProductRepresentation.</param>
        /// <param name="elementTag">The tag for the identifier of the element.</param>
        /// <param name="shapeType">The roof type.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateRoof(IFCFile file, string guid, IFCAnyHandle ownerHistory,
            string name, string description, string objectType, IFCAnyHandle objectPlacement,
            IFCAnyHandle representation, string elementTag, IFCRoofType shapeType)
        {
            ValidateElement(guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);

            IFCAnyHandle roof = CreateInstance(file, IFCEntityType.IfcRoof);
            IFCAnyHandleUtil.SetAttribute(roof, "ShapeType", shapeType);
            SetElement(roof, guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);
            return roof;
        }

        /// <summary>
        /// Creates a handle representing an IfcStair and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID for the entity.</param>
        /// <param name="ownerHistory">The IfcOwnerHistory.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The local placement.</param>
        /// <param name="representation">The geometric representation of the entity, in the IfcProductRepresentation.</param>
        /// <param name="elementTag">The tag for the identifier of the element.</param>
        /// <param name="shapeType">The stair type.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateStair(IFCFile file, string guid, IFCAnyHandle ownerHistory,
            string name, string description, string objectType, IFCAnyHandle objectPlacement,
            IFCAnyHandle representation, string elementTag, IFCStairType shapeType)
        {
            ValidateElement(guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);

            IFCAnyHandle stair = CreateInstance(file, IFCEntityType.IfcStair);
            IFCAnyHandleUtil.SetAttribute(stair, "ShapeType", shapeType);
            SetElement(stair, guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);
            return stair;
        }

        /// <summary>
        /// Creates a handle representing an IfcStairFlight and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID for the entity.</param>
        /// <param name="ownerHistory">The IfcOwnerHistory.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The local placement.</param>
        /// <param name="representation">The geometric representation of the entity, in the IfcProductRepresentation.</param>
        /// <param name="elementTag">The tag for the identifier of the element.</param>
        /// <param name="shapeType">The stair type.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateStairFlight(IFCFile file, string guid, IFCAnyHandle ownerHistory,
            string name, string description, string objectType, IFCAnyHandle objectPlacement,
            IFCAnyHandle representation, string elementTag,
            int? numberOfRiser, int? numberOfTreads, double? riserHeight, double? treadLength)
        {
            ValidateElement(guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);

            IFCAnyHandle stairFlight = CreateInstance(file, IFCEntityType.IfcStairFlight);
            SetElement(stairFlight, guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);
            IFCAnyHandleUtil.SetAttribute(stairFlight, "NumberOfRiser", numberOfRiser);
            IFCAnyHandleUtil.SetAttribute(stairFlight, "NumberOfTreads", numberOfTreads);
            IFCAnyHandleUtil.SetAttribute(stairFlight, "RiserHeight", riserHeight);
            IFCAnyHandleUtil.SetAttribute(stairFlight, "TreadLength", treadLength);
            return stairFlight;
        }

        /// <summary>
        /// Creates a handle representing an IfcRampFlight and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID for the entity.</param>
        /// <param name="ownerHistory">The IfcOwnerHistory.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The local placement.</param>
        /// <param name="representation">The geometric representation of the entity, in the IfcProductRepresentation.</param>
        /// <param name="elementTag">The tag for the identifier of the element.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateRampFlight(IFCFile file, string guid, IFCAnyHandle ownerHistory,
            string name, string description, string objectType, IFCAnyHandle objectPlacement,
            IFCAnyHandle representation, string elementTag)
        {
            ValidateElement(guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);

            IFCAnyHandle rampFlight = CreateInstance(file, IFCEntityType.IfcRampFlight);
            SetElement(rampFlight, guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);
            return rampFlight;
        }

        private static void SetReinforcingElement(IFCAnyHandle reinforcingElement, string guid, IFCAnyHandle ownerHistory,
            string name, string description, string objectType, IFCAnyHandle objectPlacement,
            IFCAnyHandle representation, string elementTag, string steelGrade)
        {
            SetElement(reinforcingElement, guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);
            IFCAnyHandleUtil.SetAttribute(reinforcingElement, "SteelGrade", steelGrade);
        }

        private static void ValidSurfaceStyleShading(IFCAnyHandle surfaceColour)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(surfaceColour, true, IFCEntityType.IfcColourRgb);
        }

        private static void SetSurfaceStyleShading(IFCAnyHandle surfaceStyleRendering, IFCAnyHandle surfaceColour)
        {
            surfaceStyleRendering.SetAttribute("SurfaceColour", surfaceColour);
        }

        private static void ValidateBooleanResult(IFCAnyHandle firstOperand, IFCAnyHandle secondOperand)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(firstOperand, false, IFCEntityType.IfcSolidModel, IFCEntityType.IfcBooleanResult,
                IFCEntityType.IfcHalfSpaceSolid, IFCEntityType.IfcCsgPrimitive3D);
            IFCAnyHandleUtil.ValidateSubTypeOf(secondOperand, false, IFCEntityType.IfcHalfSpaceSolid, IFCEntityType.IfcSolidModel,
                IFCEntityType.IfcBooleanResult, IFCEntityType.IfcCsgPrimitive3D);
        }

        private static void SetBooleanResult(IFCAnyHandle booleanResultHnd, IFCBooleanOperator clipOperator,
            IFCAnyHandle firstOperand, IFCAnyHandle secondOperand)
        {
            IFCAnyHandleUtil.SetAttribute(booleanResultHnd, "Operator", clipOperator);
            IFCAnyHandleUtil.SetAttribute(booleanResultHnd, "FirstOperand", firstOperand);
            IFCAnyHandleUtil.SetAttribute(booleanResultHnd, "SecondOperand", secondOperand);
        }

        private static void ValidateElementarySurface(IFCAnyHandle position)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(position, false, IFCEntityType.IfcAxis2Placement3D);
        }

        private static void SetElementarySurface(IFCAnyHandle elementarySurfaceHnd, IFCAnyHandle position)
        {
            IFCAnyHandleUtil.SetAttribute(elementarySurfaceHnd, "Position", position);
        }

        private static void ValidateHalfSpaceSolid(IFCAnyHandle baseSurface)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(baseSurface, false, IFCEntityType.IfcSurface);
        }

        private static void SetHalfSpaceSolid(IFCAnyHandle halfSpaceSolidHnd, IFCAnyHandle baseSurface, bool agreementFlag)
        {
            IFCAnyHandleUtil.SetAttribute(halfSpaceSolidHnd, "BaseSurface", baseSurface);
            IFCAnyHandleUtil.SetAttribute(halfSpaceSolidHnd, "AgreementFlag", agreementFlag);
        }

        private static void ValidateConic(IFCAnyHandle position)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(position, false, IFCEntityType.IfcAxis2Placement2D, IFCEntityType.IfcAxis2Placement3D);
        }

        private static void SetConic(IFCAnyHandle conic, IFCAnyHandle position)
        {
            IFCAnyHandleUtil.SetAttribute(conic, "Position", position);
        }

        /// <summary>
        /// Creates a handle representing an IfcReinforcingBar and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID for the entity.</param>
        /// <param name="ownerHistory">The IfcOwnerHistory.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The local placement.</param>
        /// <param name="representation">The geometric representation of the entity, in the IfcProductRepresentation.</param>
        /// <param name="elementTag">The tag for the identifier of the element.</param>
        /// <param name="steelGrade">The steel grade.</param>
        /// <param name="longitudinalBarNominalDiameter">The nominal diameter.</param>
        /// <param name="longitudinalBarCrossSectionArea">The cross section area.</param>
        /// <param name="barLength">The bar length (optional).</param>
        /// <param name="role">The role.</param>
        /// <param name="surface">The surface (optional).</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateReinforcingBar(IFCFile file, string guid, IFCAnyHandle ownerHistory,
            string name, string description, string objectType, IFCAnyHandle objectPlacement,
            IFCAnyHandle representation, string elementTag, string steelGrade,
            double longitudinalBarNominalDiameter, double longitudinalBarCrossSectionArea,
            double? barLength, IFCReinforcingBarRole role, IFCReinforcingBarSurface? surface)
        {
            ValidateElement(guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);

            IFCAnyHandle reinforcingBar = CreateInstance(file, IFCEntityType.IfcReinforcingBar);
            SetReinforcingElement(reinforcingBar, guid, ownerHistory, name, description, objectType, objectPlacement,
                representation, elementTag, steelGrade);
            IFCAnyHandleUtil.SetAttribute(reinforcingBar, "NominalDiameter", longitudinalBarNominalDiameter);
            IFCAnyHandleUtil.SetAttribute(reinforcingBar, "CrossSectionArea", longitudinalBarCrossSectionArea);
            if (barLength != null)
                IFCAnyHandleUtil.SetAttribute(reinforcingBar, "BarLength", barLength);
            IFCAnyHandleUtil.SetAttribute(reinforcingBar, "BarRole", role);
            if (surface != null)
                IFCAnyHandleUtil.SetAttribute(reinforcingBar, "BarSurface", surface);

            return reinforcingBar;
        }

        /// <summary>
        /// Creates a handle representing an IfcReinforcingBar and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID for the entity.</param>
        /// <param name="ownerHistory">The IfcOwnerHistory.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The local placement.</param>
        /// <param name="representation">The geometric representation of the entity, in the IfcProductRepresentation.</param>
        /// <param name="elementTag">The tag for the identifier of the element.</param>
        /// <param name="steelGrade">The steel grade.</param>
        /// <param name="longitudinalBarNominalDiameter">The nominal diameter.</param>
        /// <param name="longitudinalBarCrossSectionArea">The cross section area.</param>
        /// <param name="barLength">The bar length (optional).</param>
        /// <param name="role">The role.</param>
        /// <param name="surface">The surface (optional).</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateReinforcingMesh(IFCFile file, string guid, IFCAnyHandle ownerHistory,
            string name, string description, string objectType, IFCAnyHandle objectPlacement,
            IFCAnyHandle representation, string elementTag, string steelGrade,
            double? meshLength, double? meshWidth,
            double longitudinalBarNominalDiameter, double transverseBarNominalDiameter,
            double longitudinalBarCrossSectionArea, double transverseBarCrossSectionArea,
            double longitudinalBarSpacing, double transverseBarSpacing)
        {
            ValidateElement(guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);

            IFCAnyHandle reinforcingMesh = CreateInstance(file, IFCEntityType.IfcReinforcingMesh);
            SetReinforcingElement(reinforcingMesh, guid, ownerHistory, name, description, objectType, objectPlacement,
                representation, elementTag, steelGrade);
            if (meshLength != null)
                IFCAnyHandleUtil.SetAttribute(reinforcingMesh, "MeshLength", meshLength);
            if (meshWidth != null)
                IFCAnyHandleUtil.SetAttribute(reinforcingMesh, "MeshWidth", meshWidth);

            IFCAnyHandleUtil.SetAttribute(reinforcingMesh, "LongitudinalBarNominalDiameter", longitudinalBarNominalDiameter);
            IFCAnyHandleUtil.SetAttribute(reinforcingMesh, "TransverseBarNominalDiameter", transverseBarNominalDiameter);
            IFCAnyHandleUtil.SetAttribute(reinforcingMesh, "LongitudinalBarCrossSectionArea", longitudinalBarCrossSectionArea);
            IFCAnyHandleUtil.SetAttribute(reinforcingMesh, "TransverseBarCrossSectionArea", transverseBarCrossSectionArea);
            IFCAnyHandleUtil.SetAttribute(reinforcingMesh, "LongitudinalBarSpacing", longitudinalBarSpacing);
            IFCAnyHandleUtil.SetAttribute(reinforcingMesh, "TransverseBarSpacing", transverseBarSpacing);

            return reinforcingMesh;
        }

        /// <summary>
        /// Creates an IfcRelContainedInSpatialStructure, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID for the entity.</param>
        /// <param name="ownerHistory">The IfcOwnerHistory.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="relatingObject">The element to which the structure contributes.</param>
        /// <param name="relatedObjects">The elements that make up the structure.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateRelAggregates(IFCFile file, string guid, IFCAnyHandle ownerHistory,
            string name, string description, IFCAnyHandle relatingObject, HashSet<IFCAnyHandle> relatedObjects)
        {
            ValidateRelDecomposes(guid, ownerHistory, name, description, relatingObject, relatedObjects);

            IFCAnyHandle relAggregates = CreateInstance(file, IFCEntityType.IfcRelAggregates);
            SetRelDecomposes(relAggregates, guid, ownerHistory, name, description, relatingObject, relatedObjects);
            return relAggregates;
        }

        /// <summary>
        /// Creates an IfcLocalPlacement, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="placementRelTo">The parent placement.</param>
        /// <param name="relativePlacement">The local offset to the parent placement.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateLocalPlacement(IFCFile file, IFCAnyHandle placementRelTo, IFCAnyHandle relativePlacement)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(placementRelTo, true, IFCEntityType.IfcObjectPlacement);

            IFCAnyHandleUtil.ValidateSubTypeOf(relativePlacement, false, IFCEntityType.IfcAxis2Placement2D, IFCEntityType.IfcAxis2Placement3D);

            IFCAnyHandle localPlacement = CreateInstance(file, IFCEntityType.IfcLocalPlacement);
            IFCAnyHandleUtil.SetAttribute(localPlacement, "PlacementRelTo", placementRelTo);
            IFCAnyHandleUtil.SetAttribute(localPlacement, "RelativePlacement", relativePlacement);
            return localPlacement;
        }

        /// <summary>
        /// Creates an IfcProject, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="longName">The long name.</param>
        /// <param name="phase">Current project phase, open to interpretation for all project partner.</param>
        /// <param name="representationContexts">Context of the representations used within the project.</param>
        /// <param name="units">Units globally assigned to measure types used within the context of this project.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateProject(IFCFile file, string guid, IFCAnyHandle ownerHistory,
            string name, string description, string objectType, string longName, string phase,
            HashSet<IFCAnyHandle> representationContexts, IFCAnyHandle units)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(representationContexts, false, IFCEntityType.IfcRepresentationContext);

            IFCAnyHandleUtil.ValidateSubTypeOf(units, false, IFCEntityType.IfcUnitAssignment);

            ValidateObject(guid, ownerHistory, name, description, objectType);

            IFCAnyHandle project = CreateInstance(file, IFCEntityType.IfcProject);
            IFCAnyHandleUtil.SetAttribute(project, "LongName", longName);
            IFCAnyHandleUtil.SetAttribute(project, "Phase", phase);
            IFCAnyHandleUtil.SetAttribute(project, "RepresentationContexts", representationContexts);
            IFCAnyHandleUtil.SetAttribute(project, "UnitsInContext", units);
            SetObject(project, guid, ownerHistory, name, description, objectType);
            return project;
        }

        /// <summary>
        /// Creates an IfcBuilding, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The object placement.</param>
        /// <param name="representation">The representation object.</param>
        /// <param name="longName">The long name.</param>
        /// <param name="compositionType">The composition type.</param>
        /// <param name="elevationOfRefHeight">Elevation above sea level of the reference height used for all storey elevation measures, equals to height 0.0.</param>
        /// <param name="elevationOfTerrain">Elevation above the minimal terrain level around the foot print of the building, given in elevation above sea level.</param>
        /// <param name="buildingAddress">Address given to the building for postal purposes.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateBuilding(IFCFile file, string guid, IFCAnyHandle ownerHistory,
            string name, string description, string objectType, IFCAnyHandle objectPlacement, IFCAnyHandle representation,
            string longName, IFCElementComposition compositionType, double? elevationOfRefHeight, double? elevationOfTerrain, IFCAnyHandle buildingAddress)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(buildingAddress, true, IFCEntityType.IfcPostalAddress);
            ValidateSpatialStructureElement(guid, ownerHistory, name, description, objectType, objectPlacement, representation, longName, compositionType);

            IFCAnyHandle building = CreateInstance(file, IFCEntityType.IfcBuilding);
            IFCAnyHandleUtil.SetAttribute(building, "ElevationOfRefHeight", elevationOfRefHeight);
            IFCAnyHandleUtil.SetAttribute(building, "ElevationOfTerrain", elevationOfTerrain);
            IFCAnyHandleUtil.SetAttribute(building, "BuildingAddress", buildingAddress);
            SetSpatialStructureElement(building, guid, ownerHistory, name, description, objectType, objectPlacement, representation, longName, compositionType);
            return building;
        }

        /// <summary>
        /// Creates a handle representing an IfcBuildingStorey and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The object placement.</param>
        /// <param name="representation">The representation object.</param>
        /// <param name="longName">The long name.</param>
        /// <param name="compositionType">The composition type.</param>
        /// <param name="elevation">The elevation with flooring measurement.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateBuildingStorey(IFCFile file, string guid, IFCAnyHandle ownerHistory,
            string name, string description, string objectType, IFCAnyHandle objectPlacement, IFCAnyHandle representation,
            string longName, IFCElementComposition compositionType, double elevation)
        {
            ValidateSpatialStructureElement(guid, ownerHistory, name, description, objectType, objectPlacement, representation, longName, compositionType);

            IFCAnyHandle buildingStorey = CreateInstance(file, IFCEntityType.IfcBuildingStorey);
            IFCAnyHandleUtil.SetAttribute(buildingStorey, "Elevation", elevation);
            SetSpatialStructureElement(buildingStorey, guid, ownerHistory, name, description, objectType, objectPlacement, representation, longName, compositionType);
            return buildingStorey;
        }

        /// <summary>
        /// Creates a handle representing an IfcSpace and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The object placement.</param>
        /// <param name="representation">The representation object.</param>
        /// <param name="longName">The long name.</param>
        /// <param name="compositionType">The composition type.</param>
        /// <param name="internalOrExternal">Specify if it is an exterior space (i.e. part of the outer space) or an interior space.</param>
        /// <param name="elevation">The elevation with flooring measurement.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateSpace(IFCFile file, string guid, IFCAnyHandle ownerHistory,
            string name, string description, string objectType, IFCAnyHandle objectPlacement, IFCAnyHandle representation,
            string longName, IFCElementComposition compositionType, IFCInternalOrExternal internalOrExternal, double? elevation)
        {
            ValidateSpatialStructureElement(guid, ownerHistory, name, description, objectType, objectPlacement, representation, longName, compositionType);

            IFCAnyHandle space = CreateInstance(file, IFCEntityType.IfcSpace);
            IFCAnyHandleUtil.SetAttribute(space, "InteriorOrExteriorSpace", internalOrExternal);
            IFCAnyHandleUtil.SetAttribute(space, "ElevationWithFlooring", elevation);
            SetSpatialStructureElement(space, guid, ownerHistory, name, description, objectType, objectPlacement, representation, longName, compositionType);
            return space;
        }

        /// <summary>
        /// Creates an IfcRelContainedInSpatialStructure, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="relatedElements">The elements that make up the structure.</param>
        /// <param name="relateingElement">The element to which the structure contributes.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateRelContainedInSpatialStructure(IFCFile file, string guid, IFCAnyHandle ownerHistory,
            string name, string description, HashSet<IFCAnyHandle> relatedElements, IFCAnyHandle relateingElement)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(relatedElements, false, IFCEntityType.IfcProduct);
            IFCAnyHandleUtil.ValidateSubTypeOf(relateingElement, false, IFCEntityType.IfcSpatialStructureElement);
            ValidateRelConnects(guid, ownerHistory, name, description);

            IFCAnyHandle relContainedInSpatialStructure = CreateInstance(file, IFCEntityType.IfcRelContainedInSpatialStructure);
            IFCAnyHandleUtil.SetAttribute(relContainedInSpatialStructure, "RelatedElements", relatedElements);
            IFCAnyHandleUtil.SetAttribute(relContainedInSpatialStructure, "RelatingStructure", relateingElement);
            SetRelConnects(relContainedInSpatialStructure, guid, ownerHistory, name, description);
            return relContainedInSpatialStructure;
        }

        /// <summary>
        /// Creates a handle representing a relationship (IfcRelAssociatesMaterial) between a material definition and elements 
        /// or element types to which this material definition applies.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="relatedObjects">The objects to be related to the material.</param>
        /// <param name="relatingMaterial">The material.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateRelAssociatesMaterial(IFCFile file, string guid, IFCAnyHandle ownerHistory,
            string name, string description, HashSet<IFCAnyHandle> relatedObjects, IFCAnyHandle relatingMaterial)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(relatingMaterial, false, IFCEntityType.IfcMaterial, IFCEntityType.IfcMaterialList, IFCEntityType.IfcMaterialLayerSetUsage
                , IFCEntityType.IfcMaterialLayerSet, IFCEntityType.IfcMaterialLayer);
            ValidateRelAssociates(guid, ownerHistory, name, description, relatedObjects);

            IFCAnyHandle relAssociatesMaterial = CreateInstance(file, IFCEntityType.IfcRelAssociatesMaterial);
            IFCAnyHandleUtil.SetAttribute(relAssociatesMaterial, "RelatingMaterial", relatingMaterial);
            SetRelAssociates(relAssociatesMaterial, guid, ownerHistory, name, description, relatedObjects);
            return relAssociatesMaterial;
        }

        /// <summary>
        /// Creates an IfcRelDefinesByType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="relatedObjects">The objects to be related to a type.</param>
        /// <param name="relatingType">The relating type.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateRelDefinesByType(IFCFile file, string guid, IFCAnyHandle ownerHistory,
            string name, string description, HashSet<IFCAnyHandle> relatedObjects, IFCAnyHandle relatingType)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(relatingType, false, IFCEntityType.IfcTypeObject);
            ValidateRelDefines(guid, ownerHistory, name, description, relatedObjects);

            IFCAnyHandle relDefinesByType = CreateInstance(file, IFCEntityType.IfcRelDefinesByType);
            IFCAnyHandleUtil.SetAttribute(relDefinesByType, "RelatingType", relatingType);
            SetRelDefines(relDefinesByType, guid, ownerHistory, name, description, relatedObjects);
            return relDefinesByType;
        }

        /// <summary>
        /// Creates a handle representing an IfcRelConnectsPathElements and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="connectionGeometry">The geometric shape representation of the connection geometry.</param>
        /// <param name="relatingElement">Reference to a subtype of IfcElement that is connected by the connection relationship in the role of RelatingElement.</param>
        /// <param name="relatedElement">Reference to a subtype of IfcElement that is connected by the connection relationship in the role of RelatedElement.</param>
        /// <param name="relatingPriorities">Priorities for connection.</param>
        /// <param name="relatedPriorities">Priorities for connection.</param>
        /// <param name="relatedConnectionType">The connection type in relation to the path of the RelatingObject.</param>
        /// <param name="relatingConnectionType">The connection type in relation to the path of the RelatingObject.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateRelConnectsPathElements(IFCFile file, string guid, IFCAnyHandle ownerHistory,
            string name, string description, IFCAnyHandle connectionGeometry, IFCAnyHandle relatingElement, IFCAnyHandle relatedElement,
            IList<int> relatingPriorities, IList<int> relatedPriorities, IFCConnectionType relatedConnectionType, IFCConnectionType relatingConnectionType)
        {
            if (relatingPriorities == null)
                throw new ArgumentNullException("relatingPriorities");
            if (relatedPriorities == null)
                throw new ArgumentNullException("relatedPriorities");

            ValidateRelConnectsElements(guid, ownerHistory, name, description, connectionGeometry, relatingElement, relatedElement);

            IFCAnyHandle relConnectsPathElements = CreateInstance(file, IFCEntityType.IfcRelConnectsPathElements);
            IFCAnyHandleUtil.SetAttribute(relConnectsPathElements, "RelatingPriorities", relatingPriorities);
            IFCAnyHandleUtil.SetAttribute(relConnectsPathElements, "RelatedPriorities", relatedPriorities);
            IFCAnyHandleUtil.SetAttribute(relConnectsPathElements, "RelatedConnectionType", relatedConnectionType);
            IFCAnyHandleUtil.SetAttribute(relConnectsPathElements, "RelatingConnectionType", relatingConnectionType);
            SetRelConnectsElements(relConnectsPathElements, guid, ownerHistory, name, description, connectionGeometry, relatingElement, relatedElement);
            return relConnectsPathElements;
        }

        /// <summary>
        /// Creates a handle representing an IfcZone and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateZone(IFCFile file, string guid, IFCAnyHandle ownerHistory,
            string name, string description, string objectType)
        {
            ValidateGroup(guid, ownerHistory, name, description, objectType);

            IFCAnyHandle zone = CreateInstance(file, IFCEntityType.IfcZone);
            SetGroup(zone, guid, ownerHistory, name, description, objectType);
            return zone;
        }

        /// <summary>
        /// Creates a handle representing an IfcOccupant and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="theActor">The actor.</param>
        /// <param name="predefinedType">Predefined occupant types.</param>
        /// <returns></returns>
        public static IFCAnyHandle CreateOccupant(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name, string description,
            string objectType, IFCAnyHandle theActor, IFCOccupantType predefinedType)
        {
            ValidateActor(guid, ownerHistory, name, description, objectType, theActor);

            IFCAnyHandle occupant = CreateInstance(file, IFCEntityType.IfcOccupant);
            SetActor(occupant, guid, ownerHistory, name, description, objectType, theActor);
            IFCAnyHandleUtil.SetAttribute(occupant, "PredefinedType", predefinedType);

            return occupant;
        }

        /// <summary>
        /// Creates a handle representing an IfcRelAssignsToGroup and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="relatedObjects">Related objects, which are assigned to a single object.</param>
        /// <param name="relatedObjectsType">Particular type of the assignment relationship.</param>
        /// <param name="relatingGroup">Reference to group that finally contains all assigned group members.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateRelAssignsToGroup(IFCFile file, string guid, IFCAnyHandle ownerHistory,
            string name, string description, HashSet<IFCAnyHandle> relatedObjects, IFCObjectType? relatedObjectsType, IFCAnyHandle relatingGroup)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(relatingGroup, false, IFCEntityType.IfcGroup);

            ValidateRelAssigns(guid, ownerHistory, name, description, relatedObjects, relatedObjectsType);

            IFCAnyHandle relAssignsToGroup = CreateInstance(file, IFCEntityType.IfcRelAssignsToGroup);
            IFCAnyHandleUtil.SetAttribute(relAssignsToGroup, "RelatingGroup", relatingGroup);
            SetRelAssigns(relAssignsToGroup, guid, ownerHistory, name, description, relatedObjects, relatedObjectsType);
            return relAssignsToGroup;
        }

        /// <summary>
        /// Creates a handle representing an IfcRelOccupiesSpaces and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="relatedObjects">Related objects, which are assigned to a single object.</param>
        /// <param name="relatedObjectsType">Particular type of the assignment relationship.</param>
        /// <param name="relatingActor">The actor.</param>
        /// <param name="actingRole">The role of the actor.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateRelOccupiesSpaces(IFCFile file, string guid, IFCAnyHandle ownerHistory,
            string name, string description, HashSet<IFCAnyHandle> relatedObjects, IFCObjectType? relatedObjectsType,
            IFCAnyHandle relatingActor, IFCAnyHandle actingRole)
        {
            ValidateRelAssignsToActor(guid, ownerHistory, name, description, relatedObjects, relatedObjectsType,
                relatingActor, actingRole);

            IFCAnyHandle relOccupiesSpaces = CreateInstance(file, IFCEntityType.IfcRelOccupiesSpaces);
            SetRelAssignsToActor(relOccupiesSpaces, guid, ownerHistory, name, description, relatedObjects, relatedObjectsType,
                relatingActor, actingRole);
            return relOccupiesSpaces;
        }

        /// <summary>
        /// Creates a handle representing an IfcPropertySet and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="hasProperties">The collection of properties.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreatePropertySet(IFCFile file, string guid, IFCAnyHandle ownerHistory,
            string name, string description, HashSet<IFCAnyHandle> hasProperties)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(hasProperties, false, IFCEntityType.IfcProperty);
            ValidatePropertySetDefinition(guid, ownerHistory, name, description);

            IFCAnyHandle propertySet = CreateInstance(file, IFCEntityType.IfcPropertySet);
            IFCAnyHandleUtil.SetAttribute(propertySet, "HasProperties", hasProperties);
            SetPropertySetDefinition(propertySet, guid, ownerHistory, name, description);
            return propertySet;
        }

        /// <summary>
        /// Creates a handle representing a IfcRelDefinesByProperties and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="relatedObjects">Related objects, which are assigned to a single object.</param>
        /// <param name="relatingPropertyDefinition">The relating proprety definition.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateRelDefinesByProperties(IFCFile file, string guid, IFCAnyHandle ownerHistory,
            string name, string description, HashSet<IFCAnyHandle> relatedObjects, IFCAnyHandle relatingPropertyDefinition)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(relatingPropertyDefinition, false, IFCEntityType.IfcPropertySetDefinition);
            ValidateRelDefines(guid, ownerHistory, name, description, relatedObjects);

            IFCAnyHandle relDefinesByProperties = CreateInstance(file, IFCEntityType.IfcRelDefinesByProperties);
            IFCAnyHandleUtil.SetAttribute(relDefinesByProperties, "RelatingPropertyDefinition", relatingPropertyDefinition);
            SetRelDefines(relDefinesByProperties, guid, ownerHistory, name, description, relatedObjects);
            return relDefinesByProperties;
        }

        /// <summary>
        /// Creates a handle representing an IfcComplexProperty and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="usageName">The name of the property.</param>
        /// <param name="hasProperties">The collection of the component properties.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateComplexProperty(IFCFile file, string name, string description, string usageName,
            HashSet<IFCAnyHandle> hasProperties)
        {
            if (usageName == null)
                throw new ArgumentNullException("usageName");

            IFCAnyHandleUtil.ValidateSubTypeOf(hasProperties, false, IFCEntityType.IfcProperty);
            ValidateProperty(name, description);

            IFCAnyHandle complexProperty = CreateInstance(file, IFCEntityType.IfcComplexProperty);
            IFCAnyHandleUtil.SetAttribute(complexProperty, "UsageName", usageName);
            IFCAnyHandleUtil.SetAttribute(complexProperty, "HasProperties", hasProperties);
            SetProperty(complexProperty, name, description);
            return complexProperty;
        }

        /// <summary>
        /// Creates a handle representing an IfcElementQuantity and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="methodOfMeasurement">Name of the method of measurement used to calculate the element quantity.</param>
        /// <param name="quantities">The individual quantities for the element.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateElementQuantity(IFCFile file, string guid, IFCAnyHandle ownerHistory,
            string name, string description, string methodOfMeasurement, HashSet<IFCAnyHandle> quantities)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(quantities, false, IFCEntityType.IfcPhysicalQuantity);
            ValidatePropertySetDefinition(guid, ownerHistory, name, description);

            IFCAnyHandle elementQuantity = CreateInstance(file, IFCEntityType.IfcElementQuantity);
            IFCAnyHandleUtil.SetAttribute(elementQuantity, "MethodOfMeasurement", methodOfMeasurement);
            IFCAnyHandleUtil.SetAttribute(elementQuantity, "Quantities", quantities);
            SetPropertySetDefinition(elementQuantity, guid, ownerHistory, name, description);
            return elementQuantity;
        }

        /// <summary>
        /// Creates an IfcOrganization and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="actorRoles">Roles played by the organization.</param>
        /// <param name="addresses">Postal and telecom addresses of an organization.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateOrganization(IFCFile file, string id, string name, string description,
            IList<IFCAnyHandle> actorRoles, IList<IFCAnyHandle> addresses)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            IFCAnyHandleUtil.ValidateSubTypeOf(actorRoles, true, IFCEntityType.IfcActorRole);
            IFCAnyHandleUtil.ValidateSubTypeOf(addresses, true, IFCEntityType.IfcAddress);

            IFCAnyHandle organization = CreateInstance(file, IFCEntityType.IfcOrganization);
            IFCAnyHandleUtil.SetAttribute(organization, "Id", id);
            IFCAnyHandleUtil.SetAttribute(organization, "Name", name);
            IFCAnyHandleUtil.SetAttribute(organization, "Description", description);
            IFCAnyHandleUtil.SetAttribute(organization, "Roles", actorRoles);
            IFCAnyHandleUtil.SetAttribute(organization, "Addresses", addresses);
            return organization;
        }

        /// <summary>
        /// Creates an IfcApplication and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="organization">The organization.</param>
        /// <param name="version">The version.</param>
        /// <param name="fullName">The full name.</param>
        /// <param name="identifier">The identifier.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateApplication(IFCFile file, IFCAnyHandle organization, string version, string fullName, string identifier)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(organization, false, IFCEntityType.IfcOrganization);
            if (version == null)
                throw new ArgumentNullException("version");
            if (fullName == null)
                throw new ArgumentNullException("fullName");
            if (identifier == null)
                throw new ArgumentNullException("identifier");

            IFCAnyHandle application = CreateInstance(file, IFCEntityType.IfcApplication);
            IFCAnyHandleUtil.SetAttribute(application, "ApplicationDeveloper", organization);
            IFCAnyHandleUtil.SetAttribute(application, "Version", version);
            IFCAnyHandleUtil.SetAttribute(application, "ApplicationFullName", fullName);
            IFCAnyHandleUtil.SetAttribute(application, "ApplicationIdentifier", identifier);
            return application;
        }

        /// <summary>
        /// Creates a handle representing an IfcDirection and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="directionRatios">The components in the direction of X axis, of Y axis and of Z axis.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateDirection(IFCFile file, IList<double> directionRatios)
        {
            IFCAnyHandle direction = CreateInstance(file, IFCEntityType.IfcDirection);
            IFCAnyHandleUtil.SetAttribute(direction, "DirectionRatios", directionRatios);
            return direction;
        }

        /// <summary>
        /// Creates an IfcGeometricRepresentationContext, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="identifier">The identifier.</param>
        /// <param name="type">The description of the type of a representation context.</param>
        /// <param name="dimension">The integer dimension count of the coordinate space modeled in a geometric representation context.</param>
        /// <param name="precision">Value of the model precision for geometric models.</param>
        /// <param name="worldCoordinateSystem">Establishment of the engineering coordinate system (often referred to as the world coordinate system in CAD)
        /// for all representation contexts used by the project.</param>
        /// <param name="trueNorth">Direction of the true north relative to the underlying coordinate system.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateGeometricRepresentationContext(IFCFile file, string identifier, string type, int dimension,
            double? precision, IFCAnyHandle worldCoordinateSystem, IFCAnyHandle trueNorth)
        {
            ValidateGeometricRepresentationContext(identifier, type, dimension, precision, worldCoordinateSystem, trueNorth);

            IFCAnyHandle geometricRepresentationContext = CreateInstance(file, IFCEntityType.IfcGeometricRepresentationContext);
            SetGeometricRepresentationContext(geometricRepresentationContext, identifier, type, dimension, precision, worldCoordinateSystem,
                trueNorth);
            return geometricRepresentationContext;
        }

        /// <summary>
        /// Creates an IfcGeometricRepresentationSubContext, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="identifier">The identifier.</param>
        /// <param name="type">The description of the type of a representation context.</param>
        /// <param name="parentContext">Parent context from which the sub context derives its world coordinate system, precision, space coordinate dimension and true north.</param>
        /// <param name="targetScale">The target plot scale of the representation to which this representation context applies.</param>
        /// <param name="targetView">Target view of the representation to which this representation context applies.</param>
        /// <param name="userDefinedTargetView">User defined target view, this value shall be given, if the targetView is set to UserDefined.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateGeometricRepresentationSubContext(IFCFile file,
            string identifier, string type, IFCAnyHandle parentContext, double? targetScale,
            IFCGeometricProjection targetView, string userDefinedTargetView)
        {
            ValidateRepresentationContext(identifier, type);
            IFCAnyHandleUtil.ValidateSubTypeOf(parentContext, false, IFCEntityType.IfcGeometricRepresentationContext);

            IFCAnyHandle geometricRepresentationSubContext = CreateInstance(file, IFCEntityType.IfcGeometricRepresentationSubContext);
            IFCAnyHandleUtil.SetAttribute(geometricRepresentationSubContext, "ParentContext", parentContext);
            IFCAnyHandleUtil.SetAttribute(geometricRepresentationSubContext, "TargetScale", targetScale);
            IFCAnyHandleUtil.SetAttribute(geometricRepresentationSubContext, "TargetView", targetView);
            IFCAnyHandleUtil.SetAttribute(geometricRepresentationSubContext, "UserDefinedTargetView", userDefinedTargetView);
            SetRepresentationContext(geometricRepresentationSubContext, identifier, type);
            return geometricRepresentationSubContext;
        }

        /// <summary>
        /// Creates a handle representing an IfcGeometricCurveSet and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="geometryElements">The collection of curve elements.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateGeometricCurveSet(IFCFile file, HashSet<IFCAnyHandle> geometryElements)
        {
            ValidateGeometricSet(geometryElements);
            IFCAnyHandle geometricCurveSet = CreateInstance(file, IFCEntityType.IfcGeometricCurveSet);
            SetGeometricSet(geometricCurveSet, geometryElements);
            return geometricCurveSet;
        }

        /// <summary>
        /// Creates a handle representing an IfcGeometricSet and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="geometryElements">The collection of curve elements.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateGeometricSet(IFCFile file, HashSet<IFCAnyHandle> geometryElements)
        {
            ValidateGeometricSet(geometryElements);
            IFCAnyHandle geometricSet = CreateInstance(file, IFCEntityType.IfcGeometricSet);
            SetGeometricSet(geometricSet, geometryElements);
            return geometricSet;
        }

        /// <summary>
        /// Creates an IfcPerson, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="identifier">Identification of the person.</param>
        /// <param name="familyName">The name by which the family identity of the person may be recognized.</param>
        /// <param name="givenName">The name by which a person is known within a family and by which he or she may be familiarly recognized.</param>
        /// <param name="middleNames">Additional names given to a person that enable their identification apart from others
        /// who may have the same or similar family and given names.</param>
        /// <param name="prefixTitles">The word, or group of words, which specify the person's social and/or professional standing and appear before his/her names.</param>
        /// <param name="suffixTitles">The word, or group of words, which specify the person's social and/or professional standing and appear after his/her names.</param>
        /// <param name="actorRoles">Roles played by the person.</param>
        /// <param name="addresses">Postal and telecommunication addresses of a person.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreatePerson(IFCFile file, string identifier, string familyName, string givenName,
            IList<string> middleNames, IList<string> prefixTitles, IList<string> suffixTitles,
            IList<IFCAnyHandle> actorRoles, IList<IFCAnyHandle> addresses)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(actorRoles, true, IFCEntityType.IfcActorRole);
            IFCAnyHandleUtil.ValidateSubTypeOf(addresses, true, IFCEntityType.IfcAddress);

            IFCAnyHandle person = CreateInstance(file, IFCEntityType.IfcPerson);
            IFCAnyHandleUtil.SetAttribute(person, "Id", identifier);
            IFCAnyHandleUtil.SetAttribute(person, "FamilyName", familyName);
            IFCAnyHandleUtil.SetAttribute(person, "GivenName", givenName);
            IFCAnyHandleUtil.SetAttribute(person, "MiddleNames", middleNames);
            IFCAnyHandleUtil.SetAttribute(person, "PrefixTitles", prefixTitles);
            IFCAnyHandleUtil.SetAttribute(person, "SuffixTitles", suffixTitles);
            IFCAnyHandleUtil.SetAttribute(person, "Roles", actorRoles);
            IFCAnyHandleUtil.SetAttribute(person, "Addresses", addresses);
            return person;
        }

        /// <summary>
        /// Creates an IfcPersonAndOrganization, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="person">The person who is related to the organization.</param>
        /// <param name="organization">The organization to which the person is related.</param>
        /// <param name="actorRoles">Roles played by the person within the context of an organization.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreatePersonAndOrganization(IFCFile file, IFCAnyHandle person, IFCAnyHandle organization,
            IList<IFCAnyHandle> actorRoles)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(person, false, IFCEntityType.IfcPerson);
            IFCAnyHandleUtil.ValidateSubTypeOf(organization, false, IFCEntityType.IfcOrganization);
            IFCAnyHandleUtil.ValidateSubTypeOf(actorRoles, true, IFCEntityType.IfcActorRole);

            IFCAnyHandle personAndOrganization = CreateInstance(file, IFCEntityType.IfcPersonAndOrganization);
            IFCAnyHandleUtil.SetAttribute(personAndOrganization, "ThePerson", person);
            IFCAnyHandleUtil.SetAttribute(personAndOrganization, "TheOrganization", organization);
            IFCAnyHandleUtil.SetAttribute(personAndOrganization, "Roles", actorRoles);
            return personAndOrganization;
        }

        /// <summary>
        /// Creates an IfcOwnerHistory, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="owningUser">Direct reference to the end user who currently "owns" this object.</param>
        /// <param name="owningApplication">Direct reference to the application which currently "Owns" this object on behalf of the owning user, who uses this application.</param>
        /// <param name="state">Enumeration that defines the current access state of the object.</param>
        /// <param name="changeAction">Enumeration that defines the actions associated with changes made to the object.</param>
        /// <param name="lastModifiedDate">Date and Time at which the last modification occurred.</param>
        /// <param name="lastModifyingUser">User who carried out the last modification.</param>
        /// <param name="lastModifyingApplication">Application used to carry out the last modification.</param>
        /// <param name="creationDate">Time and date of creation.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateOwnerHistory(IFCFile file, IFCAnyHandle owningUser, IFCAnyHandle owningApplication,
            IFCState? state, IFCChangeAction changeAction, int? lastModifiedDate, IFCAnyHandle lastModifyingUser,
            IFCAnyHandle lastModifyingApplication, int creationDate)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(owningUser, false, IFCEntityType.IfcPersonAndOrganization);
            IFCAnyHandleUtil.ValidateSubTypeOf(owningApplication, false, IFCEntityType.IfcApplication);
            IFCAnyHandleUtil.ValidateSubTypeOf(lastModifyingUser, true, IFCEntityType.IfcPersonAndOrganization);
            IFCAnyHandleUtil.ValidateSubTypeOf(lastModifyingApplication, true, IFCEntityType.IfcApplication);

            IFCAnyHandle ownerHistory = CreateInstance(file, IFCEntityType.IfcOwnerHistory);
            IFCAnyHandleUtil.SetAttribute(ownerHistory, "OwningUser", owningUser);
            IFCAnyHandleUtil.SetAttribute(ownerHistory, "OwningApplication", owningApplication);
            IFCAnyHandleUtil.SetAttribute(ownerHistory, "State", state);
            IFCAnyHandleUtil.SetAttribute(ownerHistory, "ChangeAction", changeAction);
            IFCAnyHandleUtil.SetAttribute(ownerHistory, "LastModifiedDate", lastModifiedDate);
            IFCAnyHandleUtil.SetAttribute(ownerHistory, "LastModifyingUser", lastModifyingUser);
            IFCAnyHandleUtil.SetAttribute(ownerHistory, "LastModifyingApplication", lastModifyingApplication);
            IFCAnyHandleUtil.SetAttribute(ownerHistory, "CreationDate", creationDate);
            return ownerHistory;
        }

        /// <summary>
        /// Creates an IfcPostalAddress, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="purpose">Identifies the logical location of the address.</param>
        /// <param name="description">Text that relates the nature of the address.</param>
        /// <param name="userDefinedPurpose">Allows for specification of user specific purpose of the address.</param>
        /// <param name="internalLocation">An organization defined address for internal mail delivery.</param>
        /// <param name="addressLines">The postal address.</param>
        /// <param name="postalBox">An address that is implied by an identifiable mail drop.</param>
        /// <param name="town">The name of a town.</param>
        /// <param name="region">The name of a region.</param>
        /// <param name="postalCode">The code that is used by the country's postal service.</param>
        /// <param name="country">The name of a country.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreatePostalAddress(IFCFile file, IFCAddressType? purpose, string description, string userDefinedPurpose,
            string internalLocation, IList<string> addressLines, string postalBox, string town, string region,
            string postalCode, string country)
        {
            ValidateAddress(purpose, description, userDefinedPurpose);

            IFCAnyHandle postalAddress = CreateInstance(file, IFCEntityType.IfcPostalAddress);
            IFCAnyHandleUtil.SetAttribute(postalAddress, "InternalLocation", internalLocation);
            IFCAnyHandleUtil.SetAttribute(postalAddress, "AddressLines", addressLines);
            IFCAnyHandleUtil.SetAttribute(postalAddress, "PostalBox", postalBox);
            IFCAnyHandleUtil.SetAttribute(postalAddress, "Town", town);
            IFCAnyHandleUtil.SetAttribute(postalAddress, "Region", region);
            IFCAnyHandleUtil.SetAttribute(postalAddress, "PostalCode", postalCode);
            IFCAnyHandleUtil.SetAttribute(postalAddress, "Country", country);
            SetAddress(postalAddress, purpose, description, userDefinedPurpose);
            return postalAddress;
        }

        /// <summary>
        /// Creates a handle representing an IfcSIUnit and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="unitType">The type of the unit.</param>
        /// <param name="prefix">The SI Prefix for defining decimal multiples and submultiples of the unit.</param>
        /// <param name="name">The word, or group of words, by which the SI unit is referred to.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateSIUnit(IFCFile file, /*IFCAnyHandle dimensions,*/ IFCUnit unitType, IFCSIPrefix? prefix,
            IFCSIUnitName name)
        {
            IFCAnyHandle siUnit = CreateInstance(file, IFCEntityType.IfcSIUnit);
            IFCAnyHandleUtil.SetAttribute(siUnit, "Prefix", prefix);
            IFCAnyHandleUtil.SetAttribute(siUnit, "Name", name);
            SetNamedUnit(siUnit, null, unitType);
            return siUnit;
        }

        /// <summary>
        /// Creates a handle representing an IfcDimensionalExponents and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="lengthExponent">The power of the length base quantity.</param>
        /// <param name="massExponent">The power of the mass base quantity.</param>
        /// <param name="timeExponent">The power of the time base quantity.</param>
        /// <param name="electricCurrentExponent">The power of the electric current base quantity.</param>
        /// <param name="thermodynamicTemperatureExponent">The power of the thermodynamic temperature base quantity.</param>
        /// <param name="amountOfSubstanceExponent">The power of the amount of substance base quantity.</param>
        /// <param name="luminousIntensityExponent">The power of the luminous intensity base quantity.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateDimensionalExponents(IFCFile file, int lengthExponent, int massExponent,
            int timeExponent, int electricCurrentExponent, int thermodynamicTemperatureExponent,
            int amountOfSubstanceExponent, int luminousIntensityExponent)
        {
            IFCAnyHandle dimensionalExponents = CreateInstance(file, IFCEntityType.IfcDimensionalExponents);
            IFCAnyHandleUtil.SetAttribute(dimensionalExponents, "LengthExponent", lengthExponent);
            IFCAnyHandleUtil.SetAttribute(dimensionalExponents, "MassExponent", massExponent);
            IFCAnyHandleUtil.SetAttribute(dimensionalExponents, "TimeExponent", timeExponent);
            IFCAnyHandleUtil.SetAttribute(dimensionalExponents, "ElectricCurrentExponent", electricCurrentExponent);
            IFCAnyHandleUtil.SetAttribute(dimensionalExponents, "ThermodynamicTemperatureExponent", thermodynamicTemperatureExponent);
            IFCAnyHandleUtil.SetAttribute(dimensionalExponents, "AmountOfSubstanceExponent", amountOfSubstanceExponent);
            IFCAnyHandleUtil.SetAttribute(dimensionalExponents, "LuminousIntensityExponent", luminousIntensityExponent);
            return dimensionalExponents;
        }

        /// <summary>
        /// Creates a handle representing an IfcMeasureWithUnit and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="valueComponent">The value of the physical quantity when expressed in the specified units.</param>
        /// <param name="unitComponent">The unit in which the physical quantity is expressed.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateMeasureWithUnit(IFCFile file, IFCData valueComponent, IFCAnyHandle unitComponent)
        {
            if (valueComponent == null)
                throw new ArgumentNullException("valueComponent");

            IFCAnyHandleUtil.ValidateSubTypeOf(unitComponent, false, IFCEntityType.IfcDerivedUnit,
                IFCEntityType.IfcNamedUnit, IFCEntityType.IfcMonetaryUnit);

            IFCAnyHandle measureWithUnit = CreateInstance(file, IFCEntityType.IfcMeasureWithUnit);
            IFCAnyHandleUtil.SetAttribute(measureWithUnit, "ValueComponent", valueComponent);
            IFCAnyHandleUtil.SetAttribute(measureWithUnit, "UnitComponent", unitComponent);
            return measureWithUnit;
        }

        /// <summary>
        /// Creates a handle representing an IfcConversionBasedUnit and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="dimensions">The dimensional exponents of the SI base units by which the named unit is defined.</param>
        /// <param name="unitType">The type of the unit.</param>
        /// <param name="name">The word, or group of words, by which the conversion based unit is referred to.</param>
        /// <param name="conversionFactor">The physical quantity from which the converted unit is derived.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateConversionBasedUnit(IFCFile file, IFCAnyHandle dimensions, IFCUnit unitType,
            string name, IFCAnyHandle conversionFactor)
        {
            ValidateNamedUnit(dimensions, unitType);

            IFCAnyHandleUtil.ValidateSubTypeOf(conversionFactor, false, IFCEntityType.IfcMeasureWithUnit);

            IFCAnyHandle conversionBasedUnit = CreateInstance(file, IFCEntityType.IfcConversionBasedUnit);
            IFCAnyHandleUtil.SetAttribute(conversionBasedUnit, "Name", name);
            IFCAnyHandleUtil.SetAttribute(conversionBasedUnit, "ConversionFactor", conversionFactor);
            SetNamedUnit(conversionBasedUnit, dimensions, unitType);
            return conversionBasedUnit;
        }

        /// <summary>
        /// Creates a handle representing an IfcUnitAssignment and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="units">Units to be included within a unit assignment.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateUnitAssignment(IFCFile file, HashSet<IFCAnyHandle> units)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(units, false, IFCEntityType.IfcDerivedUnit,
                IFCEntityType.IfcNamedUnit, IFCEntityType.IfcMonetaryUnit);

            IFCAnyHandle unitAssignment = CreateInstance(file, IFCEntityType.IfcUnitAssignment);
            IFCAnyHandleUtil.SetAttribute(unitAssignment, "Units", units);
            return unitAssignment;
        }

        /// <summary>
        /// Creates a handle representing an IfcCircle and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="position">The local coordinate system with the origin at the center of the circle.</param>
        /// <param name="radius">The radius of the circle.  Must be positive.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateCircle(IFCFile file, IFCAnyHandle position, double radius)
        {
            ValidateConic(position);

            if (radius < MathUtil.Eps())
                throw new ArgumentException("Radius is tiny, zero, or negative.");

            IFCAnyHandle circle = CreateInstance(file, IFCEntityType.IfcCircle);
            SetConic(circle, position);
            IFCAnyHandleUtil.SetAttribute(circle, "Radius", radius);
            return circle;
        }

        /// <summary>
        /// Creates a handle representing an IfcEllipse and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="position">The local coordinate system with the origin at the center of the circle.</param>
        /// <param name="semiAxis1">The radius in the direction of X in the local coordinate system.</param>
        /// <param name="semiAxis2">The radius in the direction of Y in the local coordinate system.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateEllipse(IFCFile file, IFCAnyHandle position, double semiAxis1, double semiAxis2)
        {
            ValidateConic(position);

            if (semiAxis1 < MathUtil.Eps())
                throw new ArgumentException("semiAxis1 is tiny, zero, or negative.");
            if (semiAxis2 < MathUtil.Eps())
                throw new ArgumentException("semiAxis2 is tiny, zero, or negative.");

            IFCAnyHandle ellipse = CreateInstance(file, IFCEntityType.IfcEllipse);
            SetConic(ellipse, position);
            IFCAnyHandleUtil.SetAttribute(ellipse, "SemiAxis1", semiAxis1);
            IFCAnyHandleUtil.SetAttribute(ellipse, "SemiAxis2", semiAxis2);
            return ellipse;
        }
        
        /// <summary>
        /// Creates a handle representing an IfcCartesianPoint and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="coordinates">The coordinates of the point locations.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateCartesianPoint(IFCFile file, IList<double> coordinates)
        {
            if (coordinates == null)
                throw new ArgumentNullException("coordinates");

            IFCAnyHandle cartesianPoint = CreateInstance(file, IFCEntityType.IfcCartesianPoint);
            IFCAnyHandleUtil.SetAttribute(cartesianPoint, "Coordinates", coordinates);
            return cartesianPoint;
        }

        /// <summary>
        /// Creates a handle representing an IfcPolyline and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="points">The coordinates of the vertices.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreatePolyline(IFCFile file, IList<IFCAnyHandle> points)
        {
            if (points == null)
                throw new ArgumentNullException("points");

            IFCAnyHandle polylineHnd = CreateInstance(file, IFCEntityType.IfcPolyline);
            IFCAnyHandleUtil.SetAttribute(polylineHnd, "Points", points);
            return polylineHnd;
        }

        /// <summary>
        /// Creates a handle representing an IfcTrimmedCurve and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="basisCurve">The base curve.</param>
        /// <param name="trim1">The cartesian point, parameter, or both of end 1.</param>
        /// <param name="trim2">The cartesian point, parameter, or both of end 2.</param>
        /// <param name="senseAgreement">True if the end points match the orientation of the curve.</param>
        /// <param name="masterRepresentation">An enum stating which trim parameters are available.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateTrimmedCurve(IFCFile file, IFCAnyHandle basisCurve,
            HashSet<IFCData> trim1, HashSet<IFCData> trim2, bool senseAgreement,
            IFCTrimmingPreference masterRepresentation)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(basisCurve, false, IFCEntityType.IfcCurve);

            IFCAnyHandle trimmedCurve = CreateInstance(file, IFCEntityType.IfcTrimmedCurve);
            IFCAnyHandleUtil.SetAttribute(trimmedCurve, "BasisCurve", basisCurve);
            IFCAnyHandleUtil.SetAttribute(trimmedCurve, "Trim1", trim1);
            IFCAnyHandleUtil.SetAttribute(trimmedCurve, "Trim2", trim2);
            IFCAnyHandleUtil.SetAttribute(trimmedCurve, "SenseAgreement", senseAgreement);
            IFCAnyHandleUtil.SetAttribute(trimmedCurve, "MasterRepresentation", masterRepresentation);
            return trimmedCurve;
        }
        
        /// <summary>
        /// Creates a handle representing an IfcPolyLoop and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="polygon">The coordinates of the vertices.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreatePolyLoop(IFCFile file, IList<IFCAnyHandle> polygon)
        {
            if (polygon == null)
                throw new ArgumentNullException("polygon");

            IFCAnyHandle polyLoop = CreateInstance(file, IFCEntityType.IfcPolyLoop);
            IFCAnyHandleUtil.SetAttribute(polyLoop, "Polygon", polygon);
            return polyLoop;
        }

        /// <summary>
        /// Creates a handle representing an IfcCompositeCurveSegment and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="transitionCode">TheThe continuity between curve segments.</param>
        /// <param name="sameSense">True if the segment has the same orientation as the IfcCompositeCurve.</param>
        /// <param name="parentCurve">The curve segment geometry.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateCompositeCurveSegment(IFCFile file, IFCTransitionCode transitionCode, bool sameSense,
            IFCAnyHandle parentCurve)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(parentCurve, false, IFCEntityType.IfcBoundedCurve);

            IFCAnyHandle compositeCurveSegment = CreateInstance(file, IFCEntityType.IfcCompositeCurveSegment);
            IFCAnyHandleUtil.SetAttribute(compositeCurveSegment, "Transition", transitionCode);
            IFCAnyHandleUtil.SetAttribute(compositeCurveSegment, "SameSense", sameSense);
            IFCAnyHandleUtil.SetAttribute(compositeCurveSegment, "ParentCurve", parentCurve);
            return compositeCurveSegment;
        }

        /// <summary>
        /// Creates a handle representing an IfcCompositeCurve and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="segments">The list of IfcCompositeCurveSegments.</param>
        /// <param name="selfIntersect">True if curve self-intersects, false if not, or unknown.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateCompositeCurve(IFCFile file, IList<IFCAnyHandle> segments, IFCLogical selfIntersect)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(segments, true, IFCEntityType.IfcCompositeCurveSegment);

            IFCAnyHandle compositeCurve = CreateInstance(file, IFCEntityType.IfcCompositeCurve);
            IFCAnyHandleUtil.SetAttribute(compositeCurve, "Segments", segments);
            IFCAnyHandleUtil.SetAttribute(compositeCurve, "SelfIntersect", selfIntersect);
            return compositeCurve;
        }

        private static void SetFaceBound(IFCAnyHandle faceBound, IFCAnyHandle bound, bool orientation)
        {
            IFCAnyHandleUtil.SetAttribute(faceBound, "Bound", bound);
            IFCAnyHandleUtil.SetAttribute(faceBound, "Orientation", orientation);
        }

        /// <summary>
        /// Creates a handle representing an IfcFaceBound and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="bound">The bounding loop.</param>
        /// <param name="orientation">The orientation of the face relative to the loop.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateFaceBound(IFCFile file, IFCAnyHandle bound, bool orientation)
        {
            if (bound == null)
                throw new ArgumentNullException("bound");

            IFCAnyHandle faceBound = CreateInstance(file, IFCEntityType.IfcFaceBound);
            SetFaceBound(faceBound, bound, orientation);
            return faceBound;
        }

        /// <summary>
        /// Creates a handle representing an IfcFaceOuterBound and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="bound">The bounding loop.</param>
        /// <param name="orientation">The orientation of the face relative to the loop.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateFaceOuterBound(IFCFile file, IFCAnyHandle bound, bool orientation)
        {
            if (bound == null)
                throw new ArgumentNullException("bound");

            IFCAnyHandle faceOuterBound = CreateInstance(file, IFCEntityType.IfcFaceOuterBound);
            SetFaceBound(faceOuterBound, bound, orientation);
            return faceOuterBound;
        }

        /// <summary>
        /// Creates a handle representing an IfcFace and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="bounds">The boundaries.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateFace(IFCFile file, HashSet<IFCAnyHandle> bounds)
        {
            if (bounds == null)
                throw new ArgumentNullException("bound");
            if (bounds.Count == 0)
                throw new ArgumentException("no bounds for Face.");

            IFCAnyHandle face = CreateInstance(file, IFCEntityType.IfcFace);
            IFCAnyHandleUtil.SetAttribute(face, "Bounds", bounds);
            return face;
        }

        /// <summary>
        /// Creates an IfcRepresentationMap, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="origin">The origin of the geometry.</param>
        /// <param name="representation">The geometry of the representation.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateRepresentationMap(IFCFile file, IFCAnyHandle origin, IFCAnyHandle representation)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(origin, false, IFCEntityType.IfcAxis2Placement2D, IFCEntityType.IfcAxis2Placement3D);
            IFCAnyHandleUtil.ValidateSubTypeOf(representation, false, IFCEntityType.IfcRepresentation);

            IFCAnyHandle representationMap = CreateInstance(file, IFCEntityType.IfcRepresentationMap);
            IFCAnyHandleUtil.SetAttribute(representationMap, "MappingOrigin", origin);
            IFCAnyHandleUtil.SetAttribute(representationMap, "MappedRepresentation", representation);
            return representationMap;
        }

        /// <summary>
        /// Creates a default IfcAxis2Placement2D, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="location">The origin.</param>
        /// <param name="axis">The Z direction.</param>
        /// <param name="refDirection">The X direction.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateAxis2Placement2D(IFCFile file, IFCAnyHandle location, IFCAnyHandle axis, IFCAnyHandle refDirection)
        {
            ValidatePlacement(location);
            IFCAnyHandleUtil.ValidateSubTypeOf(axis, true, IFCEntityType.IfcDirection);
            IFCAnyHandleUtil.ValidateSubTypeOf(refDirection, true, IFCEntityType.IfcDirection);

            IFCAnyHandle axis2Placement2D = CreateInstance(file, IFCEntityType.IfcAxis2Placement2D);
            IFCAnyHandleUtil.SetAttribute(axis2Placement2D, "Axis", axis);
            IFCAnyHandleUtil.SetAttribute(axis2Placement2D, "RefDirection", refDirection);
            SetPlacement(axis2Placement2D, location);
            return axis2Placement2D;
        }

        /// <summary>
        /// Creates a default IfcAxis2Placement3D, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="location">The origin.</param>
        /// <param name="axis">The Z direction.</param>
        /// <param name="refDirection">The X direction.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateAxis2Placement3D(IFCFile file, IFCAnyHandle location, IFCAnyHandle axis, IFCAnyHandle refDirection)
        {
            ValidatePlacement(location);
            IFCAnyHandleUtil.ValidateSubTypeOf(axis, true, IFCEntityType.IfcDirection);
            IFCAnyHandleUtil.ValidateSubTypeOf(refDirection, true, IFCEntityType.IfcDirection);

            IFCAnyHandle axis2Placement3D = CreateInstance(file, IFCEntityType.IfcAxis2Placement3D);
            IFCAnyHandleUtil.SetAttribute(axis2Placement3D, "Axis", axis);
            IFCAnyHandleUtil.SetAttribute(axis2Placement3D, "RefDirection", refDirection);
            SetPlacement(axis2Placement3D, location);
            return axis2Placement3D;
        }

        /// <summary>
        /// Creates an IfcBeam, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The object placement.</param>
        /// <param name="representation">The geometric representation of the entity.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateBeam(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string objectType, IFCAnyHandle objectPlacement, IFCAnyHandle representation,
            string elementTag)
        {
            ValidateElement(guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);

            IFCAnyHandle beam = CreateInstance(file, IFCEntityType.IfcBeam);
            SetElement(beam, guid, ownerHistory, name, description, objectType, objectPlacement, representation,
                elementTag);
            return beam;
        }

        /// <summary>
        /// Creates an IfcColumn, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The object placement.</param>
        /// <param name="representation">The geometric representation of the entity.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateColumn(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string objectType, IFCAnyHandle objectPlacement, IFCAnyHandle representation,
            string elementTag)
        {
            ValidateElement(guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);

            IFCAnyHandle column = CreateInstance(file, IFCEntityType.IfcColumn);
            SetElement(column, guid, ownerHistory, name, description, objectType, objectPlacement, representation,
                elementTag);
            return column;
        }

        /// <summary>
        /// Creates an IfcDistributionFlowElement, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The object placement.</param>
        /// <param name="representation">The geometric representation of the entity.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateDistributionFlowElement(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string objectType, IFCAnyHandle objectPlacement, IFCAnyHandle representation,
            string elementTag)
        {
            ValidateElement(guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);

            IFCAnyHandle distributionFlowElement = CreateInstance(file, IFCEntityType.IfcDistributionFlowElement);
            SetElement(distributionFlowElement, guid, ownerHistory, name, description, objectType, objectPlacement, representation,
                elementTag);
            return distributionFlowElement;
        }

        /// <summary>
        /// Creates an IfcEnergyConversionDevice, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The object placement.</param>
        /// <param name="representation">The geometric representation of the entity.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateEnergyConversionDevice(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string objectType, IFCAnyHandle objectPlacement, IFCAnyHandle representation,
            string elementTag)
        {
            ValidateElement(guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);

            IFCAnyHandle energyConversionDevice = CreateInstance(file, IFCEntityType.IfcEnergyConversionDevice);
            SetElement(energyConversionDevice, guid, ownerHistory, name, description, objectType, objectPlacement, representation,
                elementTag);
            return energyConversionDevice;
        }

        /// <summary>
        /// Creates an IfcFlowController, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The object placement.</param>
        /// <param name="representation">The geometric representation of the entity.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateFlowController(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string objectType, IFCAnyHandle objectPlacement, IFCAnyHandle representation,
            string elementTag)
        {
            ValidateElement(guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);

            IFCAnyHandle flowController = CreateInstance(file, IFCEntityType.IfcFlowController);
            SetElement(flowController, guid, ownerHistory, name, description, objectType, objectPlacement, representation,
                elementTag);
            return flowController;
        }

        /// <summary>
        /// Creates an IfcFlowFitting, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The object placement.</param>
        /// <param name="representation">The geometric representation of the entity.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateFlowFitting(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string objectType, IFCAnyHandle objectPlacement, IFCAnyHandle representation,
            string elementTag)
        {
            ValidateElement(guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);

            IFCAnyHandle flowFitting = CreateInstance(file, IFCEntityType.IfcFlowFitting);
            SetElement(flowFitting, guid, ownerHistory, name, description, objectType, objectPlacement, representation,
                elementTag);
            return flowFitting;
        }

        /// <summary>
        /// Creates an IfcFlowMovingDevice, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The object placement.</param>
        /// <param name="representation">The geometric representation of the entity.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateFlowMovingDevice(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string objectType, IFCAnyHandle objectPlacement, IFCAnyHandle representation,
            string elementTag)
        {
            ValidateElement(guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);

            IFCAnyHandle flowMovingDevice = CreateInstance(file, IFCEntityType.IfcFlowMovingDevice);
            SetElement(flowMovingDevice, guid, ownerHistory, name, description, objectType, objectPlacement, representation,
                elementTag);
            return flowMovingDevice;
        }

        /// <summary>
        /// Creates an IfcFlowSegment, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The object placement.</param>
        /// <param name="representation">The geometric representation of the entity.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateFlowSegment(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string objectType, IFCAnyHandle objectPlacement, IFCAnyHandle representation,
            string elementTag)
        {
            ValidateElement(guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);

            IFCAnyHandle flowSegment = CreateInstance(file, IFCEntityType.IfcFlowSegment);
            SetElement(flowSegment, guid, ownerHistory, name, description, objectType, objectPlacement, representation,
                elementTag);
            return flowSegment;
        }

        /// <summary>
        /// Creates an IfcFlowStorageDevice, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The object placement.</param>
        /// <param name="representation">The geometric representation of the entity.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateFlowStorageDevice(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string objectType, IFCAnyHandle objectPlacement, IFCAnyHandle representation,
            string elementTag)
        {
            ValidateElement(guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);

            IFCAnyHandle flowStorageDevice = CreateInstance(file, IFCEntityType.IfcFlowStorageDevice);
            SetElement(flowStorageDevice, guid, ownerHistory, name, description, objectType, objectPlacement, representation,
                elementTag);
            return flowStorageDevice;
        }

        /// <summary>
        /// Creates an IfcFlowTerminal, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The object placement.</param>
        /// <param name="representation">The geometric representation of the entity.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateFlowTerminal(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string objectType, IFCAnyHandle objectPlacement, IFCAnyHandle representation,
            string elementTag)
        {
            ValidateElement(guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);

            IFCAnyHandle flowTerminal = CreateInstance(file, IFCEntityType.IfcFlowTerminal);
            SetElement(flowTerminal, guid, ownerHistory, name, description, objectType, objectPlacement, representation,
                elementTag);
            return flowTerminal;
        }

        /// <summary>
        /// Creates an IfcFlowTreatmentDevice, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The object placement.</param>
        /// <param name="representation">The geometric representation of the entity.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateFlowTreatmentDevice(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string objectType, IFCAnyHandle objectPlacement, IFCAnyHandle representation,
            string elementTag)
        {
            ValidateElement(guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);

            IFCAnyHandle flowTreatmentDevice = CreateInstance(file, IFCEntityType.IfcFlowTreatmentDevice);
            SetElement(flowTreatmentDevice, guid, ownerHistory, name, description, objectType, objectPlacement, representation,
                elementTag);
            return flowTreatmentDevice;
        }

        /// <summary>
        /// Creates an IfcFurnishingElement, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The object placement.</param>
        /// <param name="representation">The geometric representation of the entity.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateFurnishingElement(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string objectType, IFCAnyHandle objectPlacement, IFCAnyHandle representation,
            string elementTag)
        {
            ValidateElement(guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);

            IFCAnyHandle furnishingElement = CreateInstance(file, IFCEntityType.IfcFurnishingElement);
            SetElement(furnishingElement, guid, ownerHistory, name, description, objectType, objectPlacement, representation,
                elementTag);
            return furnishingElement;
        }

        /// <summary>
        /// Creates an IfcMember, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The object placement.</param>
        /// <param name="representation">The geometric representation of the entity.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateMember(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string objectType, IFCAnyHandle objectPlacement, IFCAnyHandle representation,
            string elementTag)
        {
            ValidateElement(guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);

            IFCAnyHandle member = CreateInstance(file, IFCEntityType.IfcMember);
            SetElement(member, guid, ownerHistory, name, description, objectType, objectPlacement, representation,
                elementTag);
            return member;
        }

        /// <summary>
        /// Creates an IfcPlate, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The object placement.</param>
        /// <param name="representation">The geometric representation of the entity.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreatePlate(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string objectType, IFCAnyHandle objectPlacement, IFCAnyHandle representation,
            string elementTag)
        {
            ValidateElement(guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);

            IFCAnyHandle plate = CreateInstance(file, IFCEntityType.IfcPlate);
            SetElement(plate, guid, ownerHistory, name, description, objectType, objectPlacement, representation,
                elementTag);
            return plate;
        }

        /// <summary>
        /// Creates an IfcActuatorType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateActuatorType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCActuatorType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle actuatorType = CreateInstance(file, IFCEntityType.IfcActuatorType);
            IFCAnyHandleUtil.SetAttribute(actuatorType, "PredefinedType", predefinedType);
            SetElementType(actuatorType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return actuatorType;
        }

        /// <summary>
        /// Creates an IfcAirTerminalBoxType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateAirTerminalBoxType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCAirTerminalBoxType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle airTerminalBoxType = CreateInstance(file, IFCEntityType.IfcAirTerminalBoxType);
            IFCAnyHandleUtil.SetAttribute(airTerminalBoxType, "PredefinedType", predefinedType);
            SetElementType(airTerminalBoxType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return airTerminalBoxType;
        }

        /// <summary>
        /// Creates an IfcAirTerminalType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateAirTerminalType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCAirTerminalType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle airTerminalType = CreateInstance(file, IFCEntityType.IfcAirTerminalType);
            IFCAnyHandleUtil.SetAttribute(airTerminalType, "PredefinedType", predefinedType);
            SetElementType(airTerminalType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return airTerminalType;
        }

        /// <summary>
        /// Creates an IfcAirToAirHeatRecoveryType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateAirToAirHeatRecoveryType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCAirToAirHeatRecoveryType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle airToAirHeatRecoveryType = CreateInstance(file, IFCEntityType.IfcAirToAirHeatRecoveryType);
            IFCAnyHandleUtil.SetAttribute(airToAirHeatRecoveryType, "PredefinedType", predefinedType);
            SetElementType(airToAirHeatRecoveryType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return airToAirHeatRecoveryType;
        }

        /// <summary>
        /// Creates an IfcAlarmType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateAlarmType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCAlarmType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle alarmType = CreateInstance(file, IFCEntityType.IfcAlarmType);
            IFCAnyHandleUtil.SetAttribute(alarmType, "PredefinedType", predefinedType);
            SetElementType(alarmType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return alarmType;
        }

        /// <summary>
        /// Creates an IfcBoilerType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateBoilerType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCBoilerType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle boilerType = CreateInstance(file, IFCEntityType.IfcBoilerType);
            IFCAnyHandleUtil.SetAttribute(boilerType, "PredefinedType", predefinedType);
            SetElementType(boilerType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return boilerType;
        }

        /// <summary>
        /// Creates an IfcCableCarrierFittingType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateCableCarrierFittingType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCCableCarrierFittingType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle cableCarrierFittingType = CreateInstance(file, IFCEntityType.IfcCableCarrierFittingType);
            IFCAnyHandleUtil.SetAttribute(cableCarrierFittingType, "PredefinedType", predefinedType);
            SetElementType(cableCarrierFittingType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return cableCarrierFittingType;
        }

        /// <summary>
        /// Creates an IfcCableCarrierSegmentType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateCableCarrierSegmentType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCCableCarrierSegmentType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle cableCarrierSegmentType = CreateInstance(file, IFCEntityType.IfcCableCarrierSegmentType);
            IFCAnyHandleUtil.SetAttribute(cableCarrierSegmentType, "PredefinedType", predefinedType);
            SetElementType(cableCarrierSegmentType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return cableCarrierSegmentType;
        }

        /// <summary>
        /// Creates an IfcCableSegmentType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateCableSegmentType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCCableSegmentType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle cableSegmentType = CreateInstance(file, IFCEntityType.IfcCableSegmentType);
            IFCAnyHandleUtil.SetAttribute(cableSegmentType, "PredefinedType", predefinedType);
            SetElementType(cableSegmentType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return cableSegmentType;
        }

        /// <summary>
        /// Creates an IfcChillerType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateChillerType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCChillerType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle chillerType = CreateInstance(file, IFCEntityType.IfcChillerType);
            IFCAnyHandleUtil.SetAttribute(chillerType, "PredefinedType", predefinedType);
            SetElementType(chillerType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return chillerType;
        }

        /// <summary>
        /// Creates an IfcCoilType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateCoilType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCCoilType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle coilType = CreateInstance(file, IFCEntityType.IfcCoilType);
            IFCAnyHandleUtil.SetAttribute(coilType, "PredefinedType", predefinedType);
            SetElementType(coilType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return coilType;
        }

        /// <summary>
        /// Creates an IfcColumnType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateColumnType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCColumnType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle columnType = CreateInstance(file, IFCEntityType.IfcColumnType);
            IFCAnyHandleUtil.SetAttribute(columnType, "PredefinedType", predefinedType);
            SetElementType(columnType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return columnType;
        }

        /// <summary>
        /// Creates an IfcCompressorType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateCompressorType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCCompressorType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle compressorType = CreateInstance(file, IFCEntityType.IfcCompressorType);
            IFCAnyHandleUtil.SetAttribute(compressorType, "PredefinedType", predefinedType);
            SetElementType(compressorType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return compressorType;
        }

        /// <summary>
        /// Creates an IfcCondensorType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateCondenserType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCCondenserType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle condenserType = CreateInstance(file, IFCEntityType.IfcCondenserType);
            IFCAnyHandleUtil.SetAttribute(condenserType, "PredefinedType", predefinedType);
            SetElementType(condenserType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return condenserType;
        }

        /// <summary>
        /// Creates an IfcControllerType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateControllerType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCControllerType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle controllerType = CreateInstance(file, IFCEntityType.IfcControllerType);
            IFCAnyHandleUtil.SetAttribute(controllerType, "PredefinedType", predefinedType);
            SetElementType(controllerType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return controllerType;
        }

        /// <summary>
        /// Creates an IfcCooledBeamType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateCooledBeamType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCCooledBeamType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle cooledBeamType = CreateInstance(file, IFCEntityType.IfcCooledBeamType);
            IFCAnyHandleUtil.SetAttribute(cooledBeamType, "PredefinedType", predefinedType);
            SetElementType(cooledBeamType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return cooledBeamType;
        }

        /// <summary>
        /// Creates an IfcCoolingTowerType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateCoolingTowerType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCCoolingTowerType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle coolingTowerType = CreateInstance(file, IFCEntityType.IfcCoolingTowerType);
            IFCAnyHandleUtil.SetAttribute(coolingTowerType, "PredefinedType", predefinedType);
            SetElementType(coolingTowerType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return coolingTowerType;
        }

        /// <summary>
        /// Creates an IfcDamperType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateDamperType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCDamperType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle camperType = CreateInstance(file, IFCEntityType.IfcDamperType);
            IFCAnyHandleUtil.SetAttribute(camperType, "PredefinedType", predefinedType);
            SetElementType(camperType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return camperType;
        }

        /// <summary>
        /// Creates an IfcDistributionChamberElementType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateDistributionChamberElementType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCDistributionChamberElementType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle distributionChamberElementType = CreateInstance(file, IFCEntityType.IfcDistributionChamberElementType);
            IFCAnyHandleUtil.SetAttribute(distributionChamberElementType, "PredefinedType", predefinedType);
            SetElementType(distributionChamberElementType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return distributionChamberElementType;
        }

        /// <summary>
        /// Creates an IfcDuctFittingType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateDuctFittingType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCDuctFittingType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle ductFittingType = CreateInstance(file, IFCEntityType.IfcDuctFittingType);
            IFCAnyHandleUtil.SetAttribute(ductFittingType, "PredefinedType", predefinedType);
            SetElementType(ductFittingType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return ductFittingType;
        }

        /// <summary>
        /// Creates an IfcDuctSegmentType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateDuctSegmentType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCDuctSegmentType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle ductSegmentType = CreateInstance(file, IFCEntityType.IfcDuctSegmentType);
            IFCAnyHandleUtil.SetAttribute(ductSegmentType, "PredefinedType", predefinedType);
            SetElementType(ductSegmentType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return ductSegmentType;
        }

        /// <summary>
        /// Creates an IfcDuctSilencerType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateDuctSilencerType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCDuctSilencerType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle ductSilencerType = CreateInstance(file, IFCEntityType.IfcDuctSilencerType);
            IFCAnyHandleUtil.SetAttribute(ductSilencerType, "PredefinedType", predefinedType);
            SetElementType(ductSilencerType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return ductSilencerType;
        }

        /// <summary>
        /// Creates an IfcElectricApplianceType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateElectricApplianceType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCElectricApplianceType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle electricApplianceType = CreateInstance(file, IFCEntityType.IfcElectricApplianceType);
            IFCAnyHandleUtil.SetAttribute(electricApplianceType, "PredefinedType", predefinedType);
            SetElementType(electricApplianceType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return electricApplianceType;
        }

        /// <summary>
        /// Creates an IfcElectricFlowStorageDeviceType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateElectricFlowStorageDeviceType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCElectricFlowStorageDeviceType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle electricFlowStorageDeviceType = CreateInstance(file, IFCEntityType.IfcElectricFlowStorageDeviceType);
            IFCAnyHandleUtil.SetAttribute(electricFlowStorageDeviceType, "PredefinedType", predefinedType);
            SetElementType(electricFlowStorageDeviceType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return electricFlowStorageDeviceType;
        }

        /// <summary>
        /// Creates an IfcElectricGeneratorType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateElectricGeneratorType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCElectricGeneratorType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle electricGeneratorType = CreateInstance(file, IFCEntityType.IfcElectricGeneratorType);
            IFCAnyHandleUtil.SetAttribute(electricGeneratorType, "PredefinedType", predefinedType);
            SetElementType(electricGeneratorType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return electricGeneratorType;
        }

        /// <summary>
        /// Creates an IfcElectricHeaterType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateElectricHeaterType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCElectricHeaterType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle electricHeaterType = CreateInstance(file, IFCEntityType.IfcElectricHeaterType);
            IFCAnyHandleUtil.SetAttribute(electricHeaterType, "PredefinedType", predefinedType);
            SetElementType(electricHeaterType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return electricHeaterType;
        }

        /// <summary>
        /// Creates an IfcElectricMotorType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateElectricMotorType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCElectricMotorType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle electricMotorType = CreateInstance(file, IFCEntityType.IfcElectricMotorType);
            IFCAnyHandleUtil.SetAttribute(electricMotorType, "PredefinedType", predefinedType);
            SetElementType(electricMotorType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return electricMotorType;
        }

        /// <summary>
        /// Creates an IfcElectricTimeControlType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateElectricTimeControlType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCElectricTimeControlType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle electricTimeControlType = CreateInstance(file, IFCEntityType.IfcElectricTimeControlType);
            IFCAnyHandleUtil.SetAttribute(electricTimeControlType, "PredefinedType", predefinedType);
            SetElementType(electricTimeControlType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return electricTimeControlType;
        }

        /// <summary>
        /// Creates an IfcEvaporativeCoolerType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateEvaporativeCoolerType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCEvaporativeCoolerType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle evaporativeCoolerType = CreateInstance(file, IFCEntityType.IfcEvaporativeCoolerType);
            IFCAnyHandleUtil.SetAttribute(evaporativeCoolerType, "PredefinedType", predefinedType);
            SetElementType(evaporativeCoolerType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return evaporativeCoolerType;
        }

        /// <summary>
        /// Creates an IfcEvaporativeType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateEvaporatorType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCEvaporatorType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle evaporatorType = CreateInstance(file, IFCEntityType.IfcEvaporatorType);
            IFCAnyHandleUtil.SetAttribute(evaporatorType, "PredefinedType", predefinedType);
            SetElementType(evaporatorType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return evaporatorType;
        }

        /// <summary>
        /// Creates an IfcFanType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateFanType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCFanType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle fanType = CreateInstance(file, IFCEntityType.IfcFanType);
            IFCAnyHandleUtil.SetAttribute(fanType, "PredefinedType", predefinedType);
            SetElementType(fanType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return fanType;
        }

        /// <summary>
        /// Creates an IfcFilterType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateFilterType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCFilterType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle filterType = CreateInstance(file, IFCEntityType.IfcFilterType);
            IFCAnyHandleUtil.SetAttribute(filterType, "PredefinedType", predefinedType);
            SetElementType(filterType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return filterType;
        }

        /// <summary>
        /// Creates an IfcFireSuppressionTerminalType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateFireSuppressionTerminalType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCFireSuppressionTerminalType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle fireSuppressionTerminalType = CreateInstance(file, IFCEntityType.IfcFireSuppressionTerminalType);
            IFCAnyHandleUtil.SetAttribute(fireSuppressionTerminalType, "PredefinedType", predefinedType);
            SetElementType(fireSuppressionTerminalType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return fireSuppressionTerminalType;
        }

        /// <summary>
        /// Creates an IfcFlowInstrumentType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateFlowInstrumentType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCFlowInstrumentType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle flowInstrumentType = CreateInstance(file, IFCEntityType.IfcFlowInstrumentType);
            IFCAnyHandleUtil.SetAttribute(flowInstrumentType, "PredefinedType", predefinedType);
            SetElementType(flowInstrumentType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return flowInstrumentType;
        }

        /// <summary>
        /// Creates an IfcFlowMeterType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateFlowMeterType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCFlowMeterType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle flowMeterType = CreateInstance(file, IFCEntityType.IfcFlowMeterType);
            IFCAnyHandleUtil.SetAttribute(flowMeterType, "PredefinedType", predefinedType);
            SetElementType(flowMeterType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return flowMeterType;
        }

        /// <summary>
        /// Creates an IfcGasTerminalType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateGasTerminalType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCGasTerminalType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle gasTerminalType = CreateInstance(file, IFCEntityType.IfcGasTerminalType);
            IFCAnyHandleUtil.SetAttribute(gasTerminalType, "PredefinedType", predefinedType);
            SetElementType(gasTerminalType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return gasTerminalType;
        }

        /// <summary>
        /// Creates an IfcHeatExchangerType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateHeatExchangerType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCHeatExchangerType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle heatExchangerType = CreateInstance(file, IFCEntityType.IfcHeatExchangerType);
            IFCAnyHandleUtil.SetAttribute(heatExchangerType, "PredefinedType", predefinedType);
            SetElementType(heatExchangerType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return heatExchangerType;
        }

        /// <summary>
        /// Creates an IfcHumidifierType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateHumidifierType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCHumidifierType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle humidifierType = CreateInstance(file, IFCEntityType.IfcHumidifierType);
            IFCAnyHandleUtil.SetAttribute(humidifierType, "PredefinedType", predefinedType);
            SetElementType(humidifierType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return humidifierType;
        }

        /// <summary>
        /// Creates an IfcJunctionBoxType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateJunctionBoxType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCJunctionBoxType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle junctionBoxType = CreateInstance(file, IFCEntityType.IfcJunctionBoxType);
            IFCAnyHandleUtil.SetAttribute(junctionBoxType, "PredefinedType", predefinedType);
            SetElementType(junctionBoxType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return junctionBoxType;
        }

        /// <summary>
        /// Creates an IfcLampType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateLampType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCLampType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle lampType = CreateInstance(file, IFCEntityType.IfcLampType);
            IFCAnyHandleUtil.SetAttribute(lampType, "PredefinedType", predefinedType);
            SetElementType(lampType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return lampType;
        }

        /// <summary>
        /// Creates an IfcLightFixtureType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateLightFixtureType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCLightFixtureType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle lightFixtureType = CreateInstance(file, IFCEntityType.IfcLightFixtureType);
            IFCAnyHandleUtil.SetAttribute(lightFixtureType, "PredefinedType", predefinedType);
            SetElementType(lightFixtureType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return lightFixtureType;
        }

        /// <summary>
        /// Creates an IfcMemberType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateMemberType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCMemberType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle memberType = CreateInstance(file, IFCEntityType.IfcMemberType);
            IFCAnyHandleUtil.SetAttribute(memberType, "PredefinedType", predefinedType);
            SetElementType(memberType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return memberType;
        }

        /// <summary>
        /// Creates an IfcMotorConnectionType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateMotorConnectionType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCMotorConnectionType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle motorConnectionType = CreateInstance(file, IFCEntityType.IfcMotorConnectionType);
            IFCAnyHandleUtil.SetAttribute(motorConnectionType, "PredefinedType", predefinedType);
            SetElementType(motorConnectionType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return motorConnectionType;
        }

        /// <summary>
        /// Creates an IfcOutletType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateOutletType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCOutletType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle outletType = CreateInstance(file, IFCEntityType.IfcOutletType);
            IFCAnyHandleUtil.SetAttribute(outletType, "PredefinedType", predefinedType);
            SetElementType(outletType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return outletType;
        }

        /// <summary>
        /// Creates an IfcPlateType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreatePlateType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCPlateType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle plateType = CreateInstance(file, IFCEntityType.IfcPlateType);
            IFCAnyHandleUtil.SetAttribute(plateType, "PredefinedType", predefinedType);
            SetElementType(plateType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return plateType;
        }

        /// <summary>
        /// Creates an IfcPipeFittingType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreatePipeFittingType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCPipeFittingType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle pipeFittingType = CreateInstance(file, IFCEntityType.IfcPipeFittingType);
            IFCAnyHandleUtil.SetAttribute(pipeFittingType, "PredefinedType", predefinedType);
            SetElementType(pipeFittingType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return pipeFittingType;
        }

        /// <summary>
        /// Creates an IfcPipeSegmentType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreatePipeSegmentType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCPipeSegmentType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle pipeSegmentType = CreateInstance(file, IFCEntityType.IfcPipeSegmentType);
            IFCAnyHandleUtil.SetAttribute(pipeSegmentType, "PredefinedType", predefinedType);
            SetElementType(pipeSegmentType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return pipeSegmentType;
        }

        /// <summary>
        /// Creates an IfcProtectiveDeviceType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateProtectiveDeviceType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCProtectiveDeviceType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle protectiveDeviceType = CreateInstance(file, IFCEntityType.IfcProtectiveDeviceType);
            IFCAnyHandleUtil.SetAttribute(protectiveDeviceType, "PredefinedType", predefinedType);
            SetElementType(protectiveDeviceType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return protectiveDeviceType;
        }

        /// <summary>
        /// Creates an IfcPumpType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreatePumpType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCPumpType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle pumpType = CreateInstance(file, IFCEntityType.IfcPumpType);
            IFCAnyHandleUtil.SetAttribute(pumpType, "PredefinedType", predefinedType);
            SetElementType(pumpType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return pumpType;
        }

        /// <summary>
        /// Creates an IfcSanitaryTerminalType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateSanitaryTerminalType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCSanitaryTerminalType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle sanitaryTerminalType = CreateInstance(file, IFCEntityType.IfcSanitaryTerminalType);
            IFCAnyHandleUtil.SetAttribute(sanitaryTerminalType, "PredefinedType", predefinedType);
            SetElementType(sanitaryTerminalType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return sanitaryTerminalType;
        }

        /// <summary>
        /// Creates an IfcSensorType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateSensorType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCSensorType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle sensorType = CreateInstance(file, IFCEntityType.IfcSensorType);
            IFCAnyHandleUtil.SetAttribute(sensorType, "PredefinedType", predefinedType);
            SetElementType(sensorType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return sensorType;
        }

        /// <summary>
        /// Creates an IfcSpaceHeaterType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateSpaceHeaterType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCSpaceHeaterType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle spaceHeaterType = CreateInstance(file, IFCEntityType.IfcSpaceHeaterType);
            IFCAnyHandleUtil.SetAttribute(spaceHeaterType, "PredefinedType", predefinedType);
            SetElementType(spaceHeaterType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return spaceHeaterType;
        }

        /// <summary>
        /// Creates an IfcStackTerminalType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateStackTerminalType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCStackTerminalType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle stackTerminalType = CreateInstance(file, IFCEntityType.IfcStackTerminalType);
            IFCAnyHandleUtil.SetAttribute(stackTerminalType, "PredefinedType", predefinedType);
            SetElementType(stackTerminalType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return stackTerminalType;
        }

        /// <summary>
        /// Creates an IfcSwitchingDeviceType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateSwitchingDeviceType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCSwitchingDeviceType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle switchingDeviceType = CreateInstance(file, IFCEntityType.IfcSwitchingDeviceType);
            IFCAnyHandleUtil.SetAttribute(switchingDeviceType, "PredefinedType", predefinedType);
            SetElementType(switchingDeviceType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return switchingDeviceType;
        }

        /// <summary>
        /// Creates an IfcTankType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateTankType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCTankType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle tankType = CreateInstance(file, IFCEntityType.IfcTankType);
            IFCAnyHandleUtil.SetAttribute(tankType, "PredefinedType", predefinedType);
            SetElementType(tankType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return tankType;
        }

        /// <summary>
        /// Creates an IfcTransformerType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateTransformerType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCTransformerType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle transformerType = CreateInstance(file, IFCEntityType.IfcTransformerType);
            IFCAnyHandleUtil.SetAttribute(transformerType, "PredefinedType", predefinedType);
            SetElementType(transformerType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return transformerType;
        }

        /// <summary>
        /// Creates an IfcTransportElementType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateTransportElementType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCTransportElementType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle transportElementType = CreateInstance(file, IFCEntityType.IfcTransportElementType);
            IFCAnyHandleUtil.SetAttribute(transportElementType, "PredefinedType", predefinedType);
            SetElementType(transportElementType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return transportElementType;
        }

        /// <summary>
        /// Creates an IfcTubeBundleType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateTubeBundleType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCTubeBundleType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle tubeBundleType = CreateInstance(file, IFCEntityType.IfcTubeBundleType);
            IFCAnyHandleUtil.SetAttribute(tubeBundleType, "PredefinedType", predefinedType);
            SetElementType(tubeBundleType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return tubeBundleType;
        }

        /// <summary>
        /// Creates an IfcUnitaryEquipmentType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateUnitaryEquipmentType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCUnitaryEquipmentType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle unitaryEquipmentType = CreateInstance(file, IFCEntityType.IfcUnitaryEquipmentType);
            IFCAnyHandleUtil.SetAttribute(unitaryEquipmentType, "PredefinedType", predefinedType);
            SetElementType(unitaryEquipmentType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return unitaryEquipmentType;
        }

        /// <summary>
        /// Creates an IfcValveType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateValveType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCValveType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle valveType = CreateInstance(file, IFCEntityType.IfcValveType);
            IFCAnyHandleUtil.SetAttribute(valveType, "PredefinedType", predefinedType);
            SetElementType(valveType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return valveType;
        }

        /// <summary>
        /// Creates an IfcWasteTerminalType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateWasteTerminalType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCWasteTerminalType predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle wasteTerminalType = CreateInstance(file, IFCEntityType.IfcWasteTerminalType);
            IFCAnyHandleUtil.SetAttribute(wasteTerminalType, "PredefinedType", predefinedType);
            SetElementType(wasteTerminalType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return wasteTerminalType;
        }

        /// <summary>
        /// Creates an IfcFurnitureType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <param name="predefinedType">The predefined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateFurnitureType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType, IFCAssemblyPlace predefinedType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle furnitureType = CreateInstance(file, IFCEntityType.IfcFurnitureType);
            IFCAnyHandleUtil.SetAttribute(furnitureType, "AssemblyPlace", predefinedType);
            SetElementType(furnitureType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return furnitureType;
        }

        /// <summary>
        /// Creates an IfcSystemFurnitureElementType, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="elementType">The type name.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateSystemFurnitureElementType(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name,
            string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, string elementType)
        {
            ValidateElementType(guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);

            IFCAnyHandle systemFurnitureElementType = CreateInstance(file, IFCEntityType.IfcSystemFurnitureElementType);
            SetElementType(systemFurnitureElementType, guid, ownerHistory, name, description, applicableOccurrence, propertySets,
                representationMaps, elementTag, elementType);
            return systemFurnitureElementType;
        }

        /// <summary>
        /// Creates an IfcAnnotation and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The object placement.</param>
        /// <param name="representation">The geometric representation of the entity.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateAnnotation(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name, string description, string objectType,
            IFCAnyHandle objectPlacement, IFCAnyHandle representation)
        {
            ValidateProduct(guid, ownerHistory, name, description, objectType, objectPlacement, representation);

            IFCAnyHandle annotation = CreateInstance(file, IFCEntityType.IfcAnnotation);
            SetProduct(annotation, guid, ownerHistory, name, description, objectType, objectPlacement, representation);
            return annotation;
        }

        /// <summary>
        /// Creates an IfcBuildingElementProxy, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The object placement.</param>
        /// <param name="representation">The geometric representation of the entity.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="compositionType">The element composition of the proxy.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateBuildingElementProxy(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name, string description, string objectType,
            IFCAnyHandle objectPlacement, IFCAnyHandle representation, string elementTag, IFCElementComposition? compositionType)
        {
            ValidateElement(guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);

            IFCAnyHandle buildingElementProxy = CreateInstance(file, IFCEntityType.IfcBuildingElementProxy);
            IFCAnyHandleUtil.SetAttribute(buildingElementProxy, "CompositionType", compositionType);
            SetElement(buildingElementProxy, guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);
            return buildingElementProxy;
        }

        /// <summary>
        /// Creates an IfcCartesianTransformOperator3D, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="axis1">The X direction of the transformation coordinate system.</param>
        /// <param name="axis2">The Y direction of the transformation coordinate system.</param>
        /// <param name="localOrigin">The origin of the transformation coordinate system.</param>
        /// <param name="scale">The scale factor.</param>
        /// <param name="axis3">The Z direction of the transformation coordinate system.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateCartesianTransformationOperator3D(IFCFile file, IFCAnyHandle axis1, IFCAnyHandle axis2,
            IFCAnyHandle localOrigin, double? scale, IFCAnyHandle axis3)
        {
            ValidateCartesianTransformationOperator(axis1, axis2, localOrigin, scale);
            IFCAnyHandleUtil.ValidateSubTypeOf(axis3, true, IFCEntityType.IfcDirection);

            IFCAnyHandle cartesianTransformationOperator3D = CreateInstance(file, IFCEntityType.IfcCartesianTransformationOperator3D);
            IFCAnyHandleUtil.SetAttribute(cartesianTransformationOperator3D, "Axis3", axis3);
            SetCartesianTransformationOperator(cartesianTransformationOperator3D, axis1, axis2, localOrigin, scale);
            return cartesianTransformationOperator3D;
        }

        /// <summary>
        /// Creates an IfcColourRgb and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="name">The name.</param>
        /// <param name="red">The red colour component value.</param>
        /// <param name="green">The green colour component value.</param>
        /// <param name="blue">The blue colour component value.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateColourRgb(IFCFile file, string name, double red, double green, double blue)
        {
            IFCAnyHandle colourRgb = CreateInstance(file, IFCEntityType.IfcColourRgb);
            IFCAnyHandleUtil.SetAttribute(colourRgb, "Name", name);
            IFCAnyHandleUtil.SetAttribute(colourRgb, "Red", red);
            IFCAnyHandleUtil.SetAttribute(colourRgb, "Green", green);
            IFCAnyHandleUtil.SetAttribute(colourRgb, "Blue", blue);
            return colourRgb;
        }

        /// <summary>
        /// Creates an IfcConnectionSurfaceGeometry and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="surfaceOnRelatingElement">The handle to the surface on the relating element.  </param>
        /// <param name="surfaceOnRelatedElement">The handle to the surface on the related element.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateConnectionSurfaceGeometry(IFCFile file, IFCAnyHandle surfaceOnRelatingElement,
            IFCAnyHandle surfaceOnRelatedElement)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(surfaceOnRelatingElement, false, IFCEntityType.IfcSurface, IFCEntityType.IfcFaceSurface, IFCEntityType.IfcFaceBasedSurfaceModel);
            IFCAnyHandleUtil.ValidateSubTypeOf(surfaceOnRelatedElement, true, IFCEntityType.IfcSurface, IFCEntityType.IfcFaceSurface, IFCEntityType.IfcFaceBasedSurfaceModel);

            IFCAnyHandle connectionSurfaceGeometry = CreateInstance(file, IFCEntityType.IfcConnectionSurfaceGeometry);
            IFCAnyHandleUtil.SetAttribute(connectionSurfaceGeometry, "SurfaceOnRelatingElement", surfaceOnRelatingElement);
            IFCAnyHandleUtil.SetAttribute(connectionSurfaceGeometry, "SurfaceOnRelatedElement", surfaceOnRelatedElement);
            return connectionSurfaceGeometry;
        }

        /// <summary>
        /// Creates an IfcCurveBoundedPlane and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="basisSurface">The surface to be bound.</param>
        /// <param name="outerBoundary">The outer boundary of the surface.</param>
        /// <param name="innerBoundaries">An optional set of inner boundaries.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateCurveBoundedPlane(IFCFile file, IFCAnyHandle basisSurface, IFCAnyHandle outerBoundary,
            HashSet<IFCAnyHandle> innerBoundaries)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(basisSurface, false, IFCEntityType.IfcPlane);
            IFCAnyHandleUtil.ValidateSubTypeOf(outerBoundary, false, IFCEntityType.IfcCurve);
            IFCAnyHandleUtil.ValidateSubTypeOf(innerBoundaries, false, IFCEntityType.IfcCurve);

            IFCAnyHandle curveBoundedPlane = CreateInstance(file, IFCEntityType.IfcCurveBoundedPlane);
            IFCAnyHandleUtil.SetAttribute(curveBoundedPlane, "BasisSurface", basisSurface);
            IFCAnyHandleUtil.SetAttribute(curveBoundedPlane, "OuterBoundary", outerBoundary);
            IFCAnyHandleUtil.SetAttribute(curveBoundedPlane, "InnerBoundaries", innerBoundaries);
            return curveBoundedPlane;
        }

        /// <summary>
        /// Creates an IfcDistributionControlElement, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The object placement.</param>
        /// <param name="representation">The geometric representation of the entity.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="controlElementId">The ControlElement Point Identification assigned to this control element by the Building Automation System.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateDistributionControlElement(IFCFile file,
            string guid, IFCAnyHandle ownerHistory, string name, string description, string objectType,
            IFCAnyHandle objectPlacement, IFCAnyHandle representation, string elementTag, string controlElementId)
        {
            ValidateElement(guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);

            IFCAnyHandle distributionControlElement = CreateInstance(file, IFCEntityType.IfcDistributionControlElement);
            IFCAnyHandleUtil.SetAttribute(distributionControlElement, "ControlElementId", controlElementId);
            SetElement(distributionControlElement, guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);
            return distributionControlElement;
        }

        /// <summary>
        /// Creates an IfcDistributionElement, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The object placement.</param>
        /// <param name="representation">The geometric representation of the entity.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateDistributionElement(IFCFile file,
            string guid, IFCAnyHandle ownerHistory, string name, string description, string objectType,
            IFCAnyHandle objectPlacement, IFCAnyHandle representation, string elementTag)
        {
            ValidateElement(guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);

            IFCAnyHandle distributionElement = CreateInstance(file, IFCEntityType.IfcDistributionElement);
            SetElement(distributionElement, guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);
            return distributionElement;
        }

        /// <summary>
        /// Creates an IfcDistributionPort and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The object placement.</param>
        /// <param name="representation">The geometric representation of the entity.</param>
        /// <param name="flowDirection">The flow direction.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateDistributionPort(IFCFile file,
            string guid, IFCAnyHandle ownerHistory, string name, string description, string objectType,
            IFCAnyHandle objectPlacement, IFCAnyHandle representation, IFCFlowDirection? flowDirection)
        {
            ValidateProduct(guid, ownerHistory, name, description, objectType, objectPlacement, representation);

            IFCAnyHandle distributionPort = CreateInstance(file, IFCEntityType.IfcDistributionPort);
            IFCAnyHandleUtil.SetAttribute(distributionPort, "FlowDirection", flowDirection);
            SetProduct(distributionPort, guid, ownerHistory, name, description, objectType, objectPlacement, representation);
            return distributionPort;
        }

        /// <summary>
        /// Creates an IfcDoor, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The object placement.</param>
        /// <param name="representation">The geometric representation of the entity.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="overallHeight">The height of the door.</param>
        /// <param name="overallWidth">The width of the door.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateDoor(IFCFile file,
            string guid, IFCAnyHandle ownerHistory, string name, string description, string objectType,
            IFCAnyHandle objectPlacement, IFCAnyHandle representation, string elementTag,
            double? overallHeight, double? overallWidth)
        {
            ValidateElement(guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);

            IFCAnyHandle door = CreateInstance(file, IFCEntityType.IfcDoor);
            IFCAnyHandleUtil.SetAttribute(door, "OverallHeight", overallHeight);
            IFCAnyHandleUtil.SetAttribute(door, "OverallWidth", overallWidth);
            SetElement(door, guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);
            return door;
        }

        /// <summary>
        /// Creates an IfcDoorLiningProperties, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="liningDepth">The depth of the lining.</param>
        /// <param name="liningThickness">The thickness of the lining.</param>
        /// <param name="thresholdDepth">The depth of the threshold.</param>
        /// <param name="thresholdThickness">The thickness of the threshold.</param>
        /// <param name="transomThickness">The thickness of the transom.</param>
        /// <param name="transomOffset">The offset of the transom.</param>
        /// <param name="liningOffset">The offset of the lining.</param>
        /// <param name="thresholdOffset">The offset of the threshold.</param>
        /// <param name="casingThickness">The thickness of the casing.</param>
        /// <param name="casingDepth">The depth of the casing.</param>
        /// <param name="shapeAspectStyle">The shape aspect for the door style.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateDoorLiningProperties(IFCFile file,
            string guid, IFCAnyHandle ownerHistory, string name, string description, double? liningDepth,
            double? liningThickness, double? thresholdDepth, double? thresholdThickness, double? transomThickness,
            double? transomOffset, double? liningOffset, double? thresholdOffset, double? casingThickness,
            double? casingDepth, IFCAnyHandle shapeAspectStyle)
        {
            ValidatePropertySetDefinition(guid, ownerHistory, name, description);
            IFCAnyHandleUtil.ValidateSubTypeOf(shapeAspectStyle, true, IFCEntityType.IfcShapeAspect);

            IFCAnyHandle doorLiningProperties = CreateInstance(file, IFCEntityType.IfcDoorLiningProperties);
            IFCAnyHandleUtil.SetAttribute(doorLiningProperties, "LiningDepth", liningDepth);
            IFCAnyHandleUtil.SetAttribute(doorLiningProperties, "LiningThickness", liningThickness);
            IFCAnyHandleUtil.SetAttribute(doorLiningProperties, "ThresholdDepth", thresholdDepth);
            IFCAnyHandleUtil.SetAttribute(doorLiningProperties, "ThresholdThickness", thresholdThickness);
            IFCAnyHandleUtil.SetAttribute(doorLiningProperties, "TransomThickness", transomThickness);
            IFCAnyHandleUtil.SetAttribute(doorLiningProperties, "TransomOffset", transomOffset);
            IFCAnyHandleUtil.SetAttribute(doorLiningProperties, "LiningOffset", liningOffset);
            IFCAnyHandleUtil.SetAttribute(doorLiningProperties, "ThresholdOffset", thresholdOffset);
            IFCAnyHandleUtil.SetAttribute(doorLiningProperties, "CasingThickness", casingThickness);
            IFCAnyHandleUtil.SetAttribute(doorLiningProperties, "CasingDepth", casingDepth);
            IFCAnyHandleUtil.SetAttribute(doorLiningProperties, "ShapeAspectStyle", shapeAspectStyle);
            SetPropertySetDefinition(doorLiningProperties, guid, ownerHistory, name, description);
            return doorLiningProperties;
        }

        /// <summary>
        /// Creates an IfcWindowLiningProperties, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="liningDepth">The depth of the lining.</param>
        /// <param name="liningThickness">The thickness of the lining.</param>
        /// <param name="transomThickness">The thickness of the transom(s).</param>
        /// <param name="mullionThickness">The thickness of the mullion(s).</param>
        /// <param name="firstTransomOffset">The offset of the first transom.</param>
        /// <param name="secondTransomOffset">The offset of the second transom.</param>
        /// <param name="firstMullionOffset">The offset of the first mullion.</param>
        /// <param name="secondMullionOffset">The offset of the second mullion.</param>
        /// <param name="shapeAspectStyle">The shape aspect for the window style.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateWindowLiningProperties(IFCFile file,
            string guid, IFCAnyHandle ownerHistory, string name, string description,
            double? liningDepth, double? liningThickness, double? transomThickness,
            double? mullionThickness, double? firstTransomOffset, double? secondTransomOffset,
            double? firstMullionOffset, double? secondMullionOffset, IFCAnyHandle shapeAspectStyle)
        {
            ValidatePropertySetDefinition(guid, ownerHistory, name, description);
            IFCAnyHandleUtil.ValidateSubTypeOf(shapeAspectStyle, true, IFCEntityType.IfcShapeAspect);

            IFCAnyHandle windowLiningProperties = CreateInstance(file, IFCEntityType.IfcWindowLiningProperties);
            IFCAnyHandleUtil.SetAttribute(windowLiningProperties, "LiningDepth", liningDepth);
            IFCAnyHandleUtil.SetAttribute(windowLiningProperties, "LiningThickness", liningThickness);
            IFCAnyHandleUtil.SetAttribute(windowLiningProperties, "TransomThickness", transomThickness);
            IFCAnyHandleUtil.SetAttribute(windowLiningProperties, "MullionThickness", mullionThickness);
            IFCAnyHandleUtil.SetAttribute(windowLiningProperties, "FirstTransomOffset", firstTransomOffset);
            IFCAnyHandleUtil.SetAttribute(windowLiningProperties, "SecondTransomOffset", secondTransomOffset);
            IFCAnyHandleUtil.SetAttribute(windowLiningProperties, "FirstMullionOffset", firstMullionOffset);
            IFCAnyHandleUtil.SetAttribute(windowLiningProperties, "SecondMullionOffset", secondMullionOffset);
            IFCAnyHandleUtil.SetAttribute(windowLiningProperties, "ShapeAspectStyle", shapeAspectStyle);
            SetPropertySetDefinition(windowLiningProperties, guid, ownerHistory, name, description);
            return windowLiningProperties;
        }

        /// <summary>
        /// Creates an IfcDoorPanelProperties, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="panelDepth">The depth of the panel.</param>
        /// <param name="panelOperation">The panel operation.</param>
        /// <param name="panelWidth">The width of the panel.</param>
        /// <param name="panelPosition">The panel position.</param>
        /// <param name="shapeAspectStyle">The shape aspect for the door style.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateDoorPanelProperties(IFCFile file,
            string guid, IFCAnyHandle ownerHistory, string name, string description, double? panelDepth,
            IFCDoorPanelOperation panelOperation, double? panelWidth, IFCDoorPanelPosition panelPosition, IFCAnyHandle shapeAspectStyle)
        {
            ValidatePropertySetDefinition(guid, ownerHistory, name, description);
            IFCAnyHandleUtil.ValidateSubTypeOf(shapeAspectStyle, true, IFCEntityType.IfcShapeAspect);

            IFCAnyHandle doorPanelProperties = CreateInstance(file, IFCEntityType.IfcDoorPanelProperties);
            IFCAnyHandleUtil.SetAttribute(doorPanelProperties, "PanelDepth", panelDepth);
            IFCAnyHandleUtil.SetAttribute(doorPanelProperties, "PanelOperation", panelOperation);
            IFCAnyHandleUtil.SetAttribute(doorPanelProperties, "PanelWidth", panelWidth);
            IFCAnyHandleUtil.SetAttribute(doorPanelProperties, "PanelPosition", panelPosition);
            IFCAnyHandleUtil.SetAttribute(doorPanelProperties, "ShapeAspectStyle", shapeAspectStyle);
            SetPropertySetDefinition(doorPanelProperties, guid, ownerHistory, name, description);
            return doorPanelProperties;
        }

        /// <summary>
        /// Creates an IfcWindowPanelProperties, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="operationType">The panel operation.</param>
        /// <param name="positionType">The panel position.</param>
        /// <param name="frameDepth">The depth of the frame.</param>
        /// <param name="frameThickness">The thickness of the frame.</param>
        /// <param name="shapeAspectStyle">The shape aspect for the window style.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateWindowPanelProperties(IFCFile file,
            string guid, IFCAnyHandle ownerHistory, string name, string description,
            IFCWindowPanelOperation operationType, IFCWindowPanelPosition positionType,
            double? frameDepth, double? frameThickness, IFCAnyHandle shapeAspectStyle)
        {
            ValidatePropertySetDefinition(guid, ownerHistory, name, description);
            IFCAnyHandleUtil.ValidateSubTypeOf(shapeAspectStyle, true, IFCEntityType.IfcShapeAspect);

            IFCAnyHandle windowPanelProperties = CreateInstance(file, IFCEntityType.IfcWindowPanelProperties);
            IFCAnyHandleUtil.SetAttribute(windowPanelProperties, "OperationType", operationType);
            IFCAnyHandleUtil.SetAttribute(windowPanelProperties, "PanelPosition", positionType);
            IFCAnyHandleUtil.SetAttribute(windowPanelProperties, "FrameDepth", frameDepth);
            IFCAnyHandleUtil.SetAttribute(windowPanelProperties, "FrameThickness", frameThickness);
            IFCAnyHandleUtil.SetAttribute(windowPanelProperties, "ShapeAspectStyle", shapeAspectStyle);
            SetPropertySetDefinition(windowPanelProperties, guid, ownerHistory, name, description);
            return windowPanelProperties;
        }

        /// <summary>
        /// Creates an IfcDoorStyle, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="operationType">The operation type.</param>
        /// <param name="constructionType">The construction type.</param>
        /// <param name="parameterTakesPrecedence">True if the parameter given in the attached lining and panel properties exactly define the geometry,
        /// false if the attached style shape takes precedence.</param>
        /// <param name="sizeable">True if the attached IfcMappedRepresentation (if given) can be sized (using scale factor of transformation), false if not.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateDoorStyle(IFCFile file, string guid, IFCAnyHandle ownerHistory,
            string name, string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, IFCDoorStyleOperation operationType,
            IFCDoorStyleConstruction constructionType, bool parameterTakesPrecedence, bool sizeable)
        {
            ValidateTypeProduct(guid, ownerHistory, name, description, applicableOccurrence, propertySets, representationMaps, elementTag);

            IFCAnyHandle doorStyle = CreateInstance(file, IFCEntityType.IfcDoorStyle);
            IFCAnyHandleUtil.SetAttribute(doorStyle, "OperationType", operationType);
            IFCAnyHandleUtil.SetAttribute(doorStyle, "ConstructionType", constructionType);
            IFCAnyHandleUtil.SetAttribute(doorStyle, "ParameterTakesPrecedence", parameterTakesPrecedence);
            IFCAnyHandleUtil.SetAttribute(doorStyle, "Sizeable", sizeable);
            SetTypeProduct(doorStyle, guid, ownerHistory, name, description, applicableOccurrence, propertySets, representationMaps, elementTag);
            return doorStyle;
        }

        /// <summary>
        /// Creates an IfcWindowStyle, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="applicableOccurrence">The attribute optionally defines the data type of the occurrence object.</param>
        /// <param name="propertySets">The property set(s) associated with the type.</param>
        /// <param name="representationMaps">The mapped geometries associated with the type.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <param name="operationType">The operation type.</param>
        /// <param name="constructionType">The construction type.</param>
        /// <param name="paramTakesPrecedence"> True if the parameter given in the attached lining and panel properties exactly define the geometry,
        /// false if the attached style shape takes precedence.</param>
        /// <param name="sizeable">True if the attached IfcMappedRepresentation (if given) can be sized (using scale factor of transformation), false if not.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateWindowStyle(IFCFile file, string guid, IFCAnyHandle ownerHistory,
            string name, string description, string applicableOccurrence, HashSet<IFCAnyHandle> propertySets,
            IList<IFCAnyHandle> representationMaps, string elementTag, IFCWindowStyleConstruction constructionType,
            IFCWindowStyleOperation operationType, bool paramTakesPrecedence, bool sizeable)
        {
            ValidateTypeProduct(guid, ownerHistory, name, description, applicableOccurrence, propertySets, representationMaps, elementTag);

            IFCAnyHandle windowStyle = CreateInstance(file, IFCEntityType.IfcWindowStyle);
            IFCAnyHandleUtil.SetAttribute(windowStyle, "ConstructionType", constructionType);
            IFCAnyHandleUtil.SetAttribute(windowStyle, "OperationType", operationType);
            IFCAnyHandleUtil.SetAttribute(windowStyle, "ParameterTakesPrecedence", paramTakesPrecedence);
            IFCAnyHandleUtil.SetAttribute(windowStyle, "Sizeable", sizeable);
            SetTypeProduct(windowStyle, guid, ownerHistory, name, description, applicableOccurrence, propertySets, representationMaps, elementTag);
            return windowStyle;
        }

        /// <summary>
        /// Creates an IfcFacetedBrep and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="outer">The closed shell.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateFacetedBrep(IFCFile file, IFCAnyHandle outer)
        {
            ValidateManifoldSolidBrep(outer);

            IFCAnyHandle facetedBrep = CreateInstance(file, IFCEntityType.IfcFacetedBrep);
            SetManifoldSolidBrep(facetedBrep, outer);
            return facetedBrep;
        }

        /// <summary>
        /// Creates an IfcMappedItem, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="mappingSource">The mapped geometry.</param>
        /// <param name="mappingTarget">The transformation operator for this instance of the mapped geometry.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateMappedItem(IFCFile file, IFCAnyHandle mappingSource, IFCAnyHandle mappingTarget)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(mappingSource, false, IFCEntityType.IfcRepresentationMap);
            IFCAnyHandleUtil.ValidateSubTypeOf(mappingTarget, false, IFCEntityType.IfcCartesianTransformationOperator);

            IFCAnyHandle mappedItem = CreateInstance(file, IFCEntityType.IfcMappedItem);
            IFCAnyHandleUtil.SetAttribute(mappedItem, "MappingSource", mappingSource);
            IFCAnyHandleUtil.SetAttribute(mappedItem, "MappingTarget", mappingTarget);
            return mappedItem;
        }

        /// <summary>
        /// Creates an IfcMaterial and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="name">The name.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateMaterial(IFCFile file, string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            IFCAnyHandle material = CreateInstance(file, IFCEntityType.IfcMaterial);
            IFCAnyHandleUtil.SetAttribute(material, "Name", name);
            return material;
        }

        /// <summary>
        /// Creates an IfcMaterialList and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="materials">The list of materials.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateMaterialList(IFCFile file, IList<IFCAnyHandle> materials)
        {
            if (materials.Count == 0)
                throw new ArgumentNullException("materials");

            IFCAnyHandleUtil.ValidateSubTypeOf(materials, false, IFCEntityType.IfcMaterial);

            IFCAnyHandle materialList = CreateInstance(file, IFCEntityType.IfcMaterialList);
            IFCAnyHandleUtil.SetAttribute(materialList, "Materials", materials);
            return materialList;
        }

        /// <summary>
        /// Creates a handle representing an IfcMaterialDefinitionRepresentation and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="representations">The collection of representations assigned to the material.</param>
        /// <param name="representedMaterial">The material.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateMaterialDefinitionRepresentation(IFCFile file, string name, string description, IList<IFCAnyHandle> representations,
            IFCAnyHandle representedMaterial)
        {
            ValidateProductRepresentation(name, description, representations);
            IFCAnyHandleUtil.ValidateSubTypeOf(representedMaterial, false, IFCEntityType.IfcMaterial);

            IFCAnyHandle productDefinitionShape = CreateInstance(file, IFCEntityType.IfcMaterialDefinitionRepresentation);
            SetProductRepresentation(productDefinitionShape, name, description, representations);
            IFCAnyHandleUtil.SetAttribute(productDefinitionShape, "RepresentedMaterial", representedMaterial);

            return productDefinitionShape;
        }

        /// <summary>
        /// Creates an IfcMaterialLayer and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="material">The material.</param>
        /// <param name="layerThickness">The thickness of the layer.</param>
        /// <param name="isVentilated">  Indication of whether the material layer represents an air layer (or cavity).</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateMaterialLayer(IFCFile file, IFCAnyHandle material, double layerThickness, IFCLogical? isVentilated)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(material, true, IFCEntityType.IfcMaterial);

            IFCAnyHandle materialLayer = CreateInstance(file, IFCEntityType.IfcMaterialLayer);
            IFCAnyHandleUtil.SetAttribute(materialLayer, "Material", material);
            IFCAnyHandleUtil.SetAttribute(materialLayer, "LayerThickness", layerThickness);
            IFCAnyHandleUtil.SetAttribute(materialLayer, "IsVentilated", isVentilated);
            return materialLayer;
        }

        /// <summary>
        /// Creates an IfcMaterialLayerSet and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="materiallayers">The material layers.</param>
        /// <param name="name">The name.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateMaterialLayerSet(IFCFile file, IList<IFCAnyHandle> materiallayers, string name)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(materiallayers, false, IFCEntityType.IfcMaterialLayer);

            IFCAnyHandle materialLayerSet = CreateInstance(file, IFCEntityType.IfcMaterialLayerSet);
            IFCAnyHandleUtil.SetAttribute(materialLayerSet, "MaterialLayers", materiallayers);
            IFCAnyHandleUtil.SetAttribute(materialLayerSet, "LayerSetName", name);
            return materialLayerSet;
        }

        /// <summary>
        /// Creates an IfcMaterialLayerSetUsage and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="materialLayerSet">The material layer set handle.</param>
        /// <param name="direction">The direction of the material layer set.</param>
        /// <param name="directionSense">The direction sense.</param>
        /// <param name="offset">Offset of the material layer set base line (MlsBase) from reference geometry (line or plane).</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateMaterialLayerSetUsage(IFCFile file, IFCAnyHandle materialLayerSet, IFCLayerSetDirection direction,
            IFCDirectionSense directionSense, double offset)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(materialLayerSet, false, IFCEntityType.IfcMaterialLayerSet);

            IFCAnyHandle materialLayerSetUsage = CreateInstance(file, IFCEntityType.IfcMaterialLayerSetUsage);
            IFCAnyHandleUtil.SetAttribute(materialLayerSetUsage, "ForLayerSet", materialLayerSet);
            IFCAnyHandleUtil.SetAttribute(materialLayerSetUsage, "LayerSetDirection", direction);
            IFCAnyHandleUtil.SetAttribute(materialLayerSetUsage, "DirectionSense", directionSense);
            IFCAnyHandleUtil.SetAttribute(materialLayerSetUsage, "OffsetFromReferenceLine", offset);
            return materialLayerSetUsage;
        }

        /// <summary>
        /// Creates an IfcOpeningElement, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The object placement.</param>
        /// <param name="representation">The geometric representation of the entity.</param>
        /// <param name="elementTag">The tag that represents the entity.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateOpeningElement(IFCFile file, string guid,
            IFCAnyHandle ownerHistory, string name, string description, string objectType,
            IFCAnyHandle objectPlacement, IFCAnyHandle representation, string elementTag)
        {
            ValidateElement(guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);

            IFCAnyHandle openingElement = CreateInstance(file, IFCEntityType.IfcOpeningElement);
            SetElement(openingElement, guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);
            return openingElement;
        }

        /// <summary>
        /// Creates an IfcPlanarExtent and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="sizeInX">The extent in the direction of the x-axis.</param>
        /// <param name="sizeInY">The extent in the direction of the y-axis.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreatePlanarExtent(IFCFile file, double sizeInX, double sizeInY)
        {
            IFCAnyHandle planarExtent = CreateInstance(file, IFCEntityType.IfcPlanarExtent);
            IFCAnyHandleUtil.SetAttribute(planarExtent, "SizeInX", sizeInX);
            IFCAnyHandleUtil.SetAttribute(planarExtent, "SizeInY", sizeInY);
            return planarExtent;
        }

        /// <summary>
        /// Creates an IfcPresentationStyleAssignment and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="styles">A set of presentation styles that are assigned to styled items.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreatePresentationStyleAssignment(IFCFile file, ICollection<IFCAnyHandle> styles)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(styles, false, IFCEntityType.IfcCurveStyle, IFCEntityType.IfcSymbolStyle,
                IFCEntityType.IfcFillAreaStyle, IFCEntityType.IfcTextStyle, IFCEntityType.IfcSurfaceStyle);

            IFCAnyHandle presentationStyleAssignment = CreateInstance(file, IFCEntityType.IfcPresentationStyleAssignment);
            IFCAnyHandleUtil.SetAttribute(presentationStyleAssignment, "Styles", styles);
            return presentationStyleAssignment;
        }

        /// <summary>
        /// Creates an IfcQuantityArea and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="unit">The unit.</param>
        /// <param name="areaValue">The value of the quantity, in the appropriate units.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateQuantityArea(IFCFile file, string name, string description, IFCAnyHandle unit, double areaValue)
        {
            ValidatePhysicalSimpleQuantity(name, description, unit);

            IFCAnyHandle quantityArea = CreateInstance(file, IFCEntityType.IfcQuantityArea);
            IFCAnyHandleUtil.SetAttribute(quantityArea, "AreaValue", areaValue);
            SetPhysicalSimpleQuantity(quantityArea, name, description, unit);
            return quantityArea;
        }

        /// <summary>
        /// Creates an IfcQuantityLength and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="unit">The unit.</param>
        /// <param name="lengthValue">The value of the quantity, in the appropriate units.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateQuantityLength(IFCFile file, string name, string description, IFCAnyHandle unit, double lengthValue)
        {
            ValidatePhysicalSimpleQuantity(name, description, unit);

            IFCAnyHandle quantityLength = CreateInstance(file, IFCEntityType.IfcQuantityLength);
            IFCAnyHandleUtil.SetAttribute(quantityLength, "LengthValue", lengthValue);
            SetPhysicalSimpleQuantity(quantityLength, name, description, unit);
            return quantityLength;
        }

        /// <summary>
        /// Creates an IfcQuantityVolume and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="unit">The unit.</param>
        /// <param name="lengthValue">The value of the quantity, in the appropriate units.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateQuantityVolume(IFCFile file, string name, string description, IFCAnyHandle unit, double volumeValue)
        {
            ValidatePhysicalSimpleQuantity(name, description, unit);

            IFCAnyHandle quantityVolume = CreateInstance(file, IFCEntityType.IfcQuantityVolume);
            IFCAnyHandleUtil.SetAttribute(quantityVolume, "VolumeValue", volumeValue);
            SetPhysicalSimpleQuantity(quantityVolume, name, description, unit);
            return quantityVolume;
        }

        /// <summary>
        /// Creates an IfcRelConnectsPorts and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="relatingPort">The port handle.</param>
        /// <param name="relatedPort">The port handle.</param>
        /// <param name="realizingElement">The element handle.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateRelConnectsPorts(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name, string description,
            IFCAnyHandle relatingPort, IFCAnyHandle relatedPort, IFCAnyHandle realizingElement)
        {
            ValidateRelConnects(guid, ownerHistory, name, description);
            IFCAnyHandleUtil.ValidateSubTypeOf(relatingPort, false, IFCEntityType.IfcPort);
            IFCAnyHandleUtil.ValidateSubTypeOf(relatedPort, false, IFCEntityType.IfcPort);
            IFCAnyHandleUtil.ValidateSubTypeOf(realizingElement, true, IFCEntityType.IfcElement);

            IFCAnyHandle relConnectsPorts = CreateInstance(file, IFCEntityType.IfcRelConnectsPorts);
            IFCAnyHandleUtil.SetAttribute(relConnectsPorts, "RelatingPort", relatingPort);
            IFCAnyHandleUtil.SetAttribute(relConnectsPorts, "RelatedPort", relatedPort);
            IFCAnyHandleUtil.SetAttribute(relConnectsPorts, "RealizingElement", realizingElement);
            SetRelConnects(relConnectsPorts, guid, ownerHistory, name, description);
            return relConnectsPorts;
        }

        /// <summary>
        /// Creates an IfcRelConnectsPortToElement and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="relatingPort">The port handle.</param>
        /// <param name="relatedElement">The element handle.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateRelConnectsPortToElement(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name, string description,
            IFCAnyHandle relatingPort, IFCAnyHandle relatedElement)
        {
            ValidateRelConnects(guid, ownerHistory, name, description);
            IFCAnyHandleUtil.ValidateSubTypeOf(relatingPort, false, IFCEntityType.IfcPort);
            IFCAnyHandleUtil.ValidateSubTypeOf(relatedElement, false, IFCEntityType.IfcElement);

            IFCAnyHandle relConnectsPortToElement = CreateInstance(file, IFCEntityType.IfcRelConnectsPortToElement);
            IFCAnyHandleUtil.SetAttribute(relConnectsPortToElement, "RelatingPort", relatingPort);
            IFCAnyHandleUtil.SetAttribute(relConnectsPortToElement, "RelatedElement", relatedElement);
            SetRelConnects(relConnectsPortToElement, guid, ownerHistory, name, description);
            return relConnectsPortToElement;
        }

        /// <summary>
        /// Creates an IfcRelFillsElement, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="relatingOpeningElement">The opening element.</param>
        /// <param name="relatedBuildingElement">The building element that fills or partially filles the opening.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateRelFillsElement(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name, string description,
            IFCAnyHandle relatingOpeningElement, IFCAnyHandle relatedBuildingElement)
        {
            ValidateRelConnects(guid, ownerHistory, name, description);
            IFCAnyHandleUtil.ValidateSubTypeOf(relatingOpeningElement, false, IFCEntityType.IfcOpeningElement);
            IFCAnyHandleUtil.ValidateSubTypeOf(relatedBuildingElement, false, IFCEntityType.IfcElement);

            IFCAnyHandle relFillsElement = CreateInstance(file, IFCEntityType.IfcRelFillsElement);
            IFCAnyHandleUtil.SetAttribute(relFillsElement, "RelatingOpeningElement", relatingOpeningElement);
            IFCAnyHandleUtil.SetAttribute(relFillsElement, "RelatedBuildingElement", relatedBuildingElement);
            SetRelConnects(relFillsElement, guid, ownerHistory, name, description);
            return relFillsElement;
        }

        /// <summary>
        /// Creates an IfcRelSpaceBoundary and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="relatingSpace">The relating space handle.</param>
        /// <param name="relatedBuildingElement">The related building element.</param>
        /// <param name="connectionGeometry">The connection geometry.</param>
        /// <param name="physicalOrVirtual">The space boundary type, physical or virtual.</param>
        /// <param name="internalOrExternal">Internal or external.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateRelSpaceBoundary(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name, string description,
            IFCAnyHandle relatingSpace, IFCAnyHandle relatedBuildingElement, IFCAnyHandle connectionGeometry, IFCPhysicalOrVirtual physicalOrVirtual,
            IFCInternalOrExternal internalOrExternal)
        {
            ValidateRelConnects(guid, ownerHistory, name, description);
            IFCAnyHandleUtil.ValidateSubTypeOf(relatingSpace, false, IFCEntityType.IfcSpace);
            IFCAnyHandleUtil.ValidateSubTypeOf(relatedBuildingElement, true, IFCEntityType.IfcElement);
            IFCAnyHandleUtil.ValidateSubTypeOf(connectionGeometry, true, IFCEntityType.IfcConnectionGeometry);

            IFCAnyHandle relSpaceBoundary = CreateInstance(file, IFCEntityType.IfcRelSpaceBoundary);
            IFCAnyHandleUtil.SetAttribute(relSpaceBoundary, "RelatingSpace", relatingSpace);
            IFCAnyHandleUtil.SetAttribute(relSpaceBoundary, "RelatedBuildingElement", relatedBuildingElement);
            IFCAnyHandleUtil.SetAttribute(relSpaceBoundary, "ConnectionGeometry", connectionGeometry);
            IFCAnyHandleUtil.SetAttribute(relSpaceBoundary, "PhysicalOrVirtualBoundary", physicalOrVirtual);
            IFCAnyHandleUtil.SetAttribute(relSpaceBoundary, "InternalOrExternalBoundary", internalOrExternal);
            SetRelConnects(relSpaceBoundary, guid, ownerHistory, name, description);
            return relSpaceBoundary;
        }

        /// <summary>
        /// Creates an IfcRelVoidsElement, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="relatingBuildingElement">The building element.</param>
        /// <param name="relatedOpeningElement">The opening element that removes material from the building element.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateRelVoidsElement(IFCFile file, string guid, IFCAnyHandle ownerHistory, string name, string description,
            IFCAnyHandle relatingBuildingElement, IFCAnyHandle relatedOpeningElement)
        {
            ValidateRelConnects(guid, ownerHistory, name, description);
            IFCAnyHandleUtil.ValidateSubTypeOf(relatingBuildingElement, false, IFCEntityType.IfcElement);
            IFCAnyHandleUtil.ValidateSubTypeOf(relatedOpeningElement, false, IFCEntityType.IfcFeatureElementSubtraction);

            IFCAnyHandle relVoidsElement = CreateInstance(file, IFCEntityType.IfcRelVoidsElement);
            IFCAnyHandleUtil.SetAttribute(relVoidsElement, "RelatingBuildingElement", relatingBuildingElement);
            IFCAnyHandleUtil.SetAttribute(relVoidsElement, "RelatedOpeningElement", relatedOpeningElement);
            SetRelConnects(relVoidsElement, guid, ownerHistory, name, description);
            return relVoidsElement;
        }

        /// <summary>
        /// Creates an IfcShapeRepresentation and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="contextOfItems">The context of the items.</param>
        /// <param name="identifier">The identifier.</param>
        /// <param name="type">The representation type.</param>
        /// <param name="items">The items that belong to the shape representation.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateShapeRepresentation(IFCFile file,
            IFCAnyHandle contextOfItems, string identifier, string type, HashSet<IFCAnyHandle> items)
        {
            ValidateRepresentation(contextOfItems, identifier, type, items);

            IFCAnyHandle shapeRepresentation = CreateInstance(file, IFCEntityType.IfcShapeRepresentation);
            SetRepresentation(shapeRepresentation, contextOfItems, identifier, type, items);
            return shapeRepresentation;
        }

        /// <summary>
        /// Creates an IfcSite and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="ownerHistory">The owner history.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The object placement.</param>
        /// <param name="representation">The product representation.</param>
        /// <param name="longName">The long name.</param>
        /// <param name="compositionType">The composition type.</param>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="elevation">The elevation.</param>
        /// <param name="landTitleNumber">The title number.</param>
        /// <param name="address">The address.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateSite(IFCFile file,
            string guid, IFCAnyHandle ownerHistory, string name, string description, string objectType,
            IFCAnyHandle objectPlacement, IFCAnyHandle representation, string longName,
            IFCElementComposition compositionType, IList<int> latitude, IList<int> longitude,
            double? elevation, string landTitleNumber, IFCAnyHandle address)
        {
            ValidateSpatialStructureElement(guid, ownerHistory, name, description, objectType, objectPlacement, representation, longName, compositionType);
            IFCAnyHandleUtil.ValidateSubTypeOf(address, true, IFCEntityType.IfcPostalAddress);

            IFCAnyHandle site = CreateInstance(file, IFCEntityType.IfcSite);
            IFCAnyHandleUtil.SetAttribute(site, "RefLatitude", latitude);
            IFCAnyHandleUtil.SetAttribute(site, "RefLongitude", longitude);
            IFCAnyHandleUtil.SetAttribute(site, "RefElevation", elevation);
            IFCAnyHandleUtil.SetAttribute(site, "LandTitleNumber", landTitleNumber);
            IFCAnyHandleUtil.SetAttribute(site, "SiteAddress", address);
            SetSpatialStructureElement(site, guid, ownerHistory, name, description, objectType, objectPlacement, representation, longName, compositionType);
            return site;
        }

        /// <summary>
        /// Creates an IfcStyledItem and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="item">The geometric representation item to which the style is assigned.</param>
        /// <param name="styles">Representation style assignments which are assigned to an item.</param>
        /// <param name="name">The word, or group of words, by which the styled item is referred to.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateStyledItem(IFCFile file,
            IFCAnyHandle item, HashSet<IFCAnyHandle> styles, string name)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(item, true, IFCEntityType.IfcRepresentationItem);
            IFCAnyHandleUtil.ValidateSubTypeOf(styles, false, IFCEntityType.IfcPresentationStyleAssignment);

            IFCAnyHandle styledItem = CreateInstance(file, IFCEntityType.IfcStyledItem);
            IFCAnyHandleUtil.SetAttribute(styledItem, "Item", item);
            IFCAnyHandleUtil.SetAttribute(styledItem, "Styles", styles);
            IFCAnyHandleUtil.SetAttribute(styledItem, "Name", name);
            return styledItem;
        }

        /// <summary>
        /// Creates an IfcStyledRepresentation and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="representation">The IfcRepresentation.</param>
        /// <param name="contextOfItems">The context of the items.</param>
        /// <param name="identifier">The identifier.</param>
        /// <param name="type">The representation type.</param>
        /// <param name="items">The items that belong to the shape representation.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateStyledRepresentation(IFCFile file, IFCAnyHandle contextOfItems, string identifier, string type,
            HashSet<IFCAnyHandle> items)
        {
            ValidateRepresentation(contextOfItems, identifier, type, items);

            IFCAnyHandle styledRepresentation = CreateInstance(file, IFCEntityType.IfcStyledRepresentation);
            SetRepresentation(styledRepresentation, contextOfItems, identifier, type, items);
            return styledRepresentation;
        }

        /// <summary>
        /// Creates an IfcTextLiteralWithExtent and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="literal">The text literal to be presented.</param>
        /// <param name="placement">The IfcAxis2Placement that determines the placement and orientation of the presented string.</param>
        /// <param name="path">The writing direction of the text literal.</param>
        /// <param name="extent">The extent in the x and y direction of the text literal.</param>
        /// <param name="boxAlignment">The alignment of the text literal relative to its position.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateTextLiteralWithExtent(IFCFile file,
            string literal, IFCAnyHandle placement, IFCTextPath path, IFCAnyHandle extent, string boxAlignment)
        {
            if (literal == null)
                throw new ArgumentNullException("literal");
            if (boxAlignment == null)
                throw new ArgumentNullException("boxAlignment");
            IFCAnyHandleUtil.ValidateSubTypeOf(placement, false, IFCEntityType.IfcAxis2Placement2D, IFCEntityType.IfcAxis2Placement3D);
            IFCAnyHandleUtil.ValidateSubTypeOf(extent, false, IFCEntityType.IfcPlanarExtent);

            IFCAnyHandle textLiteralWithExtent = CreateInstance(file, IFCEntityType.IfcTextLiteralWithExtent);
            IFCAnyHandleUtil.SetAttribute(textLiteralWithExtent, "Literal", literal);
            IFCAnyHandleUtil.SetAttribute(textLiteralWithExtent, "Placement", placement);
            IFCAnyHandleUtil.SetAttribute(textLiteralWithExtent, "Path", path);
            IFCAnyHandleUtil.SetAttribute(textLiteralWithExtent, "Extent", extent);
            IFCAnyHandleUtil.SetAttribute(textLiteralWithExtent, "BoxAlignment", boxAlignment);
            return textLiteralWithExtent;
        }

        /// <summary>
        /// Creates an IfcTextStyle and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="name">The name.</param>
        /// <param name="characterAppearance">The character style to be used for presented text.</param>
        /// <param name="style">The style applied to the text block for its visual appearance.</param>
        /// <param name="fontStyle">The style applied to the text font for its visual appearance.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateTextStyle(IFCFile file,
            string name, IFCAnyHandle characterAppearance, IFCAnyHandle style, IFCAnyHandle fontStyle)
        {
            ValidatePresentationStyle(name);
            IFCAnyHandleUtil.ValidateSubTypeOf(characterAppearance, true, IFCEntityType.IfcTextStyleForDefinedFont);
            IFCAnyHandleUtil.ValidateSubTypeOf(style, true, IFCEntityType.IfcTextStyleWithBoxCharacteristics, IFCEntityType.IfcTextStyleTextModel);
            IFCAnyHandleUtil.ValidateSubTypeOf(fontStyle, false, IFCEntityType.IfcPreDefinedTextFont, IFCEntityType.IfcExternallyDefinedTextFont);

            IFCAnyHandle textStyle = CreateInstance(file, IFCEntityType.IfcTextStyle);
            IFCAnyHandleUtil.SetAttribute(textStyle, "TextCharacterAppearance", characterAppearance);
            IFCAnyHandleUtil.SetAttribute(textStyle, "TextStyle", style);
            IFCAnyHandleUtil.SetAttribute(textStyle, "TextFontStyle", fontStyle);
            SetPresentationStyle(textStyle, name);
            return textStyle;
        }

        /// <summary>
        /// Creates an IfcTextStyleFontModel and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="name">The name.</param>
        /// <param name="fontFamily">The font family.</param>
        /// <param name="fontStyle">The font style.</param>
        /// <param name="fontVariant">The font variant.</param>
        /// <param name="fontWeight">The font weight.</param>
        /// <param name="fontSize">The font size.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateTextStyleFontModel(IFCFile file,
            string name, IList<string> fontFamily, string fontStyle, string fontVariant,
            string fontWeight, IFCData fontSize)
        {
            ValidatePreDefinedItem(name);
            if (fontSize == null)
                throw new ArgumentNullException("fontSize");

            IFCAnyHandle textStyleFontModel = CreateInstance(file, IFCEntityType.IfcTextStyleFontModel);
            IFCAnyHandleUtil.SetAttribute(textStyleFontModel, "FontFamily", fontFamily);
            IFCAnyHandleUtil.SetAttribute(textStyleFontModel, "FontStyle", fontStyle);
            IFCAnyHandleUtil.SetAttribute(textStyleFontModel, "FontVariant", fontVariant);
            IFCAnyHandleUtil.SetAttribute(textStyleFontModel, "FontWeight", fontWeight);
            IFCAnyHandleUtil.SetAttribute(textStyleFontModel, "FontSize", fontSize);
            SetPreDefinedItem(textStyleFontModel, name);
            return textStyleFontModel;
        }

        /// <summary>
        /// Creates an IfcTextStyleForDefinedFont and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="color">The color.</param>
        /// <param name="backgroundColor">The background color.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateTextStyleForDefinedFont(IFCFile file,
            IFCAnyHandle color, IFCAnyHandle backgroundColor)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(color, false, IFCEntityType.IfcColourSpecification, IFCEntityType.IfcPreDefinedColour);
            IFCAnyHandleUtil.ValidateSubTypeOf(backgroundColor, true, IFCEntityType.IfcColourSpecification, IFCEntityType.IfcPreDefinedColour);

            IFCAnyHandle textStyleForDefinedFont = CreateInstance(file, IFCEntityType.IfcTextStyleForDefinedFont);
            IFCAnyHandleUtil.SetAttribute(textStyleForDefinedFont, "Colour", color);
            IFCAnyHandleUtil.SetAttribute(textStyleForDefinedFont, "BackgroundColour", backgroundColor);
            return textStyleForDefinedFont;
        }

        /// <summary>
        /// Creates an IfcTransportElement, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID to use to label the wall.</param>
        /// <param name="ownerHistory">The IfcOwnerHistory.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The local placement.</param>
        /// <param name="representation">The representation object assigned to the wall.</param>
        /// <param name="elementTag">The tag for the identifier of the element.</param>
        /// <param name="operationType">The transport operation type.</param>
        /// <param name="capacityByWeight">The capacity by weight.</param>
        /// <param name="capacityByNumber">The capacity by number.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateTransportElement(IFCFile file,
            string guid, IFCAnyHandle ownerHistory, string name, string description, string objectType,
            IFCAnyHandle objectPlacement, IFCAnyHandle representation, string elementTag,
            IFCTransportElementType? operationType, double? capacityByWeight, double? capacityByNumber)
        {
            ValidateElement(guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);

            IFCAnyHandle transportElement = CreateInstance(file, IFCEntityType.IfcTransportElement);
            IFCAnyHandleUtil.SetAttribute(transportElement, "OperationType", operationType);
            IFCAnyHandleUtil.SetAttribute(transportElement, "CapacityByWeight", capacityByWeight);
            IFCAnyHandleUtil.SetAttribute(transportElement, "CapacityByNumber", capacityByNumber);
            SetElement(transportElement, guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);
            return transportElement;
        }

        /// <summary>
        /// Creates an IfcWindow, and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID to use to label the wall.</param>
        /// <param name="ownerHistory">The IfcOwnerHistory.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="objectPlacement">The local placement.</param>
        /// <param name="representation">The representation object assigned to the wall.</param>
        /// <param name="elementTag">The tag for the identifier of the element.</param>
        /// <param name="height">The height of the window.</param>
        /// <param name="width">The width of the window.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateWindow(IFCFile file,
            string guid, IFCAnyHandle ownerHistory, string name, string description, string objectType,
            IFCAnyHandle objectPlacement, IFCAnyHandle representation, string elementTag,
            double? height, double? width)
        {
            ValidateElement(guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);

            IFCAnyHandle window = CreateInstance(file, IFCEntityType.IfcWindow);
            IFCAnyHandleUtil.SetAttribute(window, "OverallHeight", height);
            IFCAnyHandleUtil.SetAttribute(window, "OverallWidth", width);
            SetElement(window, guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);
            return window;
        }

        /// <summary>
        /// Creates an IfcPropertySingleValue and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="nominalValue">The value of the property.</param>
        /// <param name="unit">The unit.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreatePropertySingleValue(IFCFile file,
            string name, string description, IFCData nominalValue, IFCAnyHandle unit)
        {
            ValidateProperty(name, description);
            IFCAnyHandleUtil.ValidateSubTypeOf(unit, true, IFCEntityType.IfcDerivedUnit, IFCEntityType.IfcNamedUnit, IFCEntityType.IfcMonetaryUnit);

            IFCAnyHandle propertySingleValue = CreateInstance(file, IFCEntityType.IfcPropertySingleValue);
            IFCAnyHandleUtil.SetAttribute(propertySingleValue, "NominalValue", nominalValue);
            IFCAnyHandleUtil.SetAttribute(propertySingleValue, "Unit", unit);
            SetProperty(propertySingleValue, name, description);
            return propertySingleValue;
        }

        /// <summary>
        /// Creates an IfcPropertyEnumeratedValue and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="enumerationValues">The values of the property.</param>
        /// <param name="enumerationReference">The enumeration reference.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreatePropertyEnumeratedValue(IFCFile file,
            string name, string description, IList<IFCData> enumerationValues, IFCAnyHandle enumerationReference)
        {
            ValidateProperty(name, description);
            IFCAnyHandleUtil.ValidateSubTypeOf(enumerationReference, true, IFCEntityType.IfcPropertyEnumeration);

            IFCAnyHandle propertyEnumeratedValue = CreateInstance(file, IFCEntityType.IfcPropertyEnumeratedValue);
            IFCAnyHandleUtil.SetAttribute(propertyEnumeratedValue, "EnumerationValues", enumerationValues);
            IFCAnyHandleUtil.SetAttribute(propertyEnumeratedValue, "EnumerationReference", enumerationReference);
            SetProperty(propertyEnumeratedValue, name, description);
            return propertyEnumeratedValue;
        }

        /// <summary>
        /// Creates an IfcPropertyListValue and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="listValues">The values of the property.</param>
        /// <param name="unit">The unit.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreatePropertyListValue(IFCFile file,
            string name, string description, IList<IFCData> listValues, IFCAnyHandle unit)
        {
            ValidateProperty(name, description);
            IFCAnyHandleUtil.ValidateSubTypeOf(unit, true, IFCEntityType.IfcDerivedUnit, IFCEntityType.IfcNamedUnit, IFCEntityType.IfcMonetaryUnit);

            IFCAnyHandle propertyListValue = CreateInstance(file, IFCEntityType.IfcPropertyListValue);
            IFCAnyHandleUtil.SetAttribute(propertyListValue, "ListValues", listValues);
            IFCAnyHandleUtil.SetAttribute(propertyListValue, "Unit", unit);
            SetProperty(propertyListValue, name, description);
            return propertyListValue;
        }

        /// <summary>
        /// Creates a handle representing an IfcCalendarDate and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="day">The day of the month in the date.</param>
        /// <param name="month">The month in the date.</param>
        /// <param name="year">The year in the date.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateCalendarDate(IFCFile file, int day, int month, int year)
        {
            IFCAnyHandle calendarDate = CreateInstance(file, IFCEntityType.IfcCalendarDate);
            IFCAnyHandleUtil.SetAttribute(calendarDate, "DayComponent", day);
            IFCAnyHandleUtil.SetAttribute(calendarDate, "MonthComponent", month);
            IFCAnyHandleUtil.SetAttribute(calendarDate, "YearComponent", year);
            return calendarDate;
        }

        /// <summary>
        /// Creates a handle representing an IfcClassification and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="source">The source of the classification.</param>
        /// <param name="edition">The edition of the classification system.</param>
        /// <param name="editionDate">The date associated with this edition of the classification system.</param>
        /// <param name="name">The name of the classification.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateClassification(IFCFile file, string source, string edition, IFCAnyHandle editionDate,
           string name)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(editionDate, true, IFCEntityType.IfcCalendarDate);

            IFCAnyHandle classification = CreateInstance(file, IFCEntityType.IfcClassification);
            IFCAnyHandleUtil.SetAttribute(classification, "Source", source);
            IFCAnyHandleUtil.SetAttribute(classification, "Edition", edition);
            IFCAnyHandleUtil.SetAttribute(classification, "EditionDate", editionDate);
            IFCAnyHandleUtil.SetAttribute(classification, "Name", name);
            return classification;
        }

        /// <summary>
        /// Creates a handle representing an IfcClassificationReference and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="location">Location of the reference (e.g. URL).</param>
        /// <param name="itemReference">Location of the item within the reference source.</param>
        /// <param name="name">Name of the reference.</param>
        /// <param name="referencedSource">The referenced classification.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateClassificationReference(IFCFile file, string location,
           string itemReference, string name, IFCAnyHandle referencedSource)
        {
            // All IfcExternalReference arguments are optional.
            IFCAnyHandleUtil.ValidateSubTypeOf(referencedSource, true, IFCEntityType.IfcClassification);

            IFCAnyHandle classificationReference = CreateInstance(file, IFCEntityType.IfcClassificationReference);
            SetExternalReference(classificationReference, location, itemReference, name);
            IFCAnyHandleUtil.SetAttribute(classificationReference, "ReferencedSource", referencedSource);
            return classificationReference;
        }

        /// <summary>
        /// Creates a handle representing an IfcRelAssociatesClassification and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="globalId">The GUID of the IfcRelAssociatesClassification.</param>
        /// <param name="ownerHistory">The owner history of the IfcRelAssociatesClassification.</param>
        /// <param name="name">Name of the relation.</param>
        /// <param name="description">Description of the relation.</param>
        /// <param name="relatedObjects">The handles of the objects associated to the classification.</param>
        /// <param name="relatingClassification">The classification assigned to the objects.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateRelAssociatesClassification(IFCFile file, string globalId, IFCAnyHandle ownerHistory,
           string name, string description, HashSet<IFCAnyHandle> relatedObjects, IFCAnyHandle relatingClassification)
        {
            ValidateRelAssociates(globalId, ownerHistory, name, description, relatedObjects);

            IFCAnyHandleUtil.ValidateSubTypeOf(relatingClassification, false, IFCEntityType.IfcClassificationNotation, IFCEntityType.IfcClassificationReference);

            IFCAnyHandle relAssociatesClassification = CreateInstance(file, IFCEntityType.IfcRelAssociatesClassification);
            SetRelAssociates(relAssociatesClassification, globalId, ownerHistory, name, description, relatedObjects);
            IFCAnyHandleUtil.SetAttribute(relAssociatesClassification, "RelatingClassification", relatingClassification);
            return relAssociatesClassification;
        }

        /// <summary>
        /// Creates a handle representing an IfcCreateElementAssembly and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="globalId">The GUID of the IfcCreateElementAssembly.</param>
        /// <param name="ownerHistory">The owner history of the IfcCreateElementAssembly.</param>
        /// <param name="name">Name of the assembly.</param>
        /// <param name="description">Description of the assembly.</param>
        /// <param name="objectType">The object type of the assembly, usually the name of the Assembly type.</param>
        /// <param name="objectPlacement">The placement of the assembly.</param>
        /// <param name="representation">The representation of the assembly, usually empty.</param>
        /// <param name="tag">The tag of the assembly, usually represented by the Element ID.</param>
        /// <param name="assemblyPlace">The place where the assembly is made.</param>
        /// <param name="predefinedType">The type of the assembly, from a list of pre-defined types.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateElementAssembly(IFCFile file, string globalId, IFCAnyHandle ownerHistory,
            string name, string description, string objectType, IFCAnyHandle objectPlacement, IFCAnyHandle representation,
            string tag, IFCAssemblyPlace? assemblyPlace, IFCElementAssemblyType predefinedType)
        {
            ValidateElement(globalId, ownerHistory, name, description, objectType, objectPlacement, representation, tag);

            IFCAnyHandle elementAssembly = CreateInstance(file, IFCEntityType.IfcElementAssembly);
            SetElement(elementAssembly, globalId, ownerHistory, name, description, objectType, objectPlacement, representation, tag);
            IFCAnyHandleUtil.SetAttribute(elementAssembly, "AssemblyPlace", assemblyPlace);
            IFCAnyHandleUtil.SetAttribute(elementAssembly, "PredefinedType", predefinedType);
            return elementAssembly;
        }

        /// <summary>
        /// Creates a handle representing an IfcBuildingElementPart and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="guid">The GUID of the IfcBuildingElementPart.</param>
        /// <param name="ownerHistory">The owner history of the IfcBuildingElementPart.</param>
        /// <param name="name">Name of the entity.</param>
        /// <param name="description">Description of the entity.</param>
        /// <param name="objectType">Object type of the entity.</param>
        /// <param name="objectPlacement">Placement handle of the entity.</param>
        /// <param name="representation">Representation handle of the enti</param>
        /// <param name="elementTag">The element tag.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateBuildingElementPart(IFCFile file,
            string guid, IFCAnyHandle ownerHistory, string name, string description, string objectType,
            IFCAnyHandle objectPlacement, IFCAnyHandle representation, string elementTag)
        {
            ValidateElement(guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);

            IFCAnyHandle part = CreateInstance(file, IFCEntityType.IfcBuildingElementPart);
            SetElement(part, guid, ownerHistory, name, description, objectType, objectPlacement, representation, elementTag);
            return part;
        }

        /// <summary>
        /// Creates a handle representing an IfcAnnotationFillArea and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="outerBoundary">The outer boundary.</param>
        /// <param name="innerBoundaries">The inner boundaries.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateAnnotationFillArea(IFCFile file, IFCAnyHandle outerBoundary, HashSet<IFCAnyHandle> innerBoundaries)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(outerBoundary, false, IFCEntityType.IfcCurve);
            IFCAnyHandleUtil.ValidateSubTypeOf(innerBoundaries, true, IFCEntityType.IfcCurve);

            IFCAnyHandle annotationFillArea = CreateInstance(file, IFCEntityType.IfcAnnotationFillArea);
            IFCAnyHandleUtil.SetAttribute(annotationFillArea, "OuterBoundary", outerBoundary);
            IFCAnyHandleUtil.SetAttribute(annotationFillArea, "InnerBoundaries", innerBoundaries);
            return annotationFillArea;
        }

        /// <summary>
        /// Creates a handle representing an IfcArbitraryClosedProfileDef and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="profileType">The profile type.</param>
        /// <param name="profileName">The profile name.</param>
        /// <param name="positionHnd">The profile position.</param>
        /// <param name="radius">The profile radius.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateArbitraryClosedProfileDef(IFCFile file, IFCProfileType profileType, string profileName, IFCAnyHandle outerCurve)
        {
            ValidateArbitraryClosedProfileDef(outerCurve);

            IFCAnyHandle arbitraryClosedProfileDef = CreateInstance(file, IFCEntityType.IfcArbitraryClosedProfileDef);
            SetArbitraryClosedProfileDef(arbitraryClosedProfileDef, profileType, profileName, outerCurve);
            return arbitraryClosedProfileDef;
        }
        
        /// <summary>
        /// Creates a handle representing an IfcArbitraryProfileDefWithVoids and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="profileType">The profile type.</param>
        /// <param name="profileName">The profile name.</param>
        /// <param name="positionHnd">The profile position.</param>
        /// <param name="radius">The profile radius.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateArbitraryProfileDefWithVoids(IFCFile file, IFCProfileType profileType, string profileName, IFCAnyHandle outerCurve,
            HashSet<IFCAnyHandle> innerCurves)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(innerCurves, false, IFCEntityType.IfcCurve);

            ValidateArbitraryClosedProfileDef(outerCurve);

            IFCAnyHandle arbitraryProfileDefWithVoids = CreateInstance(file, IFCEntityType.IfcArbitraryProfileDefWithVoids);
            SetArbitraryClosedProfileDef(arbitraryProfileDefWithVoids, profileType, profileName, outerCurve);
            IFCAnyHandleUtil.SetAttribute(arbitraryProfileDefWithVoids, "InnerCurves", innerCurves);
            return arbitraryProfileDefWithVoids;
        }

        /// <summary>
        /// Creates a handle representing an IfcCircleProfileDef and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="profileType">The profile type.</param>
        /// <param name="profileName">The profile name.</param>
        /// <param name="positionHnd">The profile position.</param>
        /// <param name="radius">The profile radius.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateCircleProfileDef(IFCFile file, IFCProfileType profileType, string profileName, IFCAnyHandle positionHnd,
            double radius)
        {
            if (radius < MathUtil.Eps())
                throw new ArgumentException("Non-positive radius parameter.", "radius");

            ValidateParameterizedProfileDef(positionHnd);

            IFCAnyHandle circleProfileDef = CreateInstance(file, IFCEntityType.IfcCircleProfileDef);
            SetParameterizedProfileDef(circleProfileDef, profileType, profileName, positionHnd);
            IFCAnyHandleUtil.SetAttribute(circleProfileDef, "Radius", radius);
            return circleProfileDef;
        }
        
        /// <summary>
        /// Creates a handle representing an IfcRectangleProfileDef and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="profileType">The profile type.</param>
        /// <param name="profileName">The profile name.</param>
        /// <param name="positionHnd">The profile position.</param>
        /// <param name="xLen">The profile length.</param>
        /// <param name="yLen">The profile width.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateRectangleProfileDef(IFCFile file, IFCProfileType profileType, string profileName, IFCAnyHandle positionHnd, 
            double length, double width)
        {
            if (length < MathUtil.Eps())
                throw new ArgumentException("Non-positive length parameter.", "length");
            if (width < MathUtil.Eps())
                throw new ArgumentException("Non-positive width parameter.", "width");

            ValidateParameterizedProfileDef(positionHnd);

            IFCAnyHandle rectangleProfileDef = CreateInstance(file, IFCEntityType.IfcRectangleProfileDef);
            SetParameterizedProfileDef(rectangleProfileDef, profileType, profileName, positionHnd);
            IFCAnyHandleUtil.SetAttribute(rectangleProfileDef, "XDim", length);
            IFCAnyHandleUtil.SetAttribute(rectangleProfileDef, "YDim", width);
            return rectangleProfileDef;
        }

        /// <summary>
        /// Creates a handle representing an IfcIShapeProfileDef and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="profileType">The profile type.</param>
        /// <param name="profileName">The profile name.</param>
        /// <param name="positionHnd">The profile position.</param>
        /// <param name="xLen">The profile length.</param>
        /// <param name="yLen">The profile width.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateIShapeProfileDef(IFCFile file, IFCProfileType profileType, string profileName, IFCAnyHandle positionHnd,
            double overallWidth, double overallDepth, double webThickness, double flangeThickness, double? filletRadius)
        {
            if (overallWidth < MathUtil.Eps())
                throw new ArgumentException("Non-positive width parameter.", "overallWidth");
            if (overallDepth < MathUtil.Eps())
                throw new ArgumentException("Non-positive depth parameter.", "overallDepth");
            if (webThickness < MathUtil.Eps())
                throw new ArgumentException("Non-positive web thickness parameter.", "webThickness");
            if (flangeThickness < MathUtil.Eps())
                throw new ArgumentException("Non-positive flange thickness parameter.", "flangeThickness");
            if ((filletRadius != null) && filletRadius.GetValueOrDefault() < MathUtil.Eps())
                throw new ArgumentException("Non-positive fillet radius parameter.", "filletRadius");

            ValidateParameterizedProfileDef(positionHnd);

            IFCAnyHandle iShapeProfileDef = CreateInstance(file, IFCEntityType.IfcIShapeProfileDef);
            SetParameterizedProfileDef(iShapeProfileDef, profileType, profileName, positionHnd);
            IFCAnyHandleUtil.SetAttribute(iShapeProfileDef, "OverallWidth", overallWidth);
            IFCAnyHandleUtil.SetAttribute(iShapeProfileDef, "OverallDepth", overallDepth);
            IFCAnyHandleUtil.SetAttribute(iShapeProfileDef, "WebThickness", webThickness);
            IFCAnyHandleUtil.SetAttribute(iShapeProfileDef, "FlangeThickness", flangeThickness);
            IFCAnyHandleUtil.SetAttribute(iShapeProfileDef, "FilletRadius", filletRadius);
            return iShapeProfileDef;
        }
        
        /// <summary>
        /// Creates a handle representing an IfcExtrudedAreaSolid and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="sweptArea">The profile.</param>
        /// <param name="solidAxis">The plane of the profile.</param>
        /// <param name="extrudedDirection">The extrusion direction.</param>
        /// <param name="depth">The extrusion depth.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateExtrudedAreaSolid(IFCFile file, IFCAnyHandle sweptArea, IFCAnyHandle solidAxis, IFCAnyHandle extrudedDirection,
            double depth)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(extrudedDirection, false, IFCEntityType.IfcDirection);
            if (depth < MathUtil.Eps())
                throw new ArgumentException("Non-positive depth parameter.", "depth");

            ValidateSweptAreaSolid(sweptArea, solidAxis);

            IFCAnyHandle extrudedAreaSolid = CreateInstance(file, IFCEntityType.IfcExtrudedAreaSolid);
            SetSweptAreaSolid(extrudedAreaSolid, sweptArea, solidAxis);
            IFCAnyHandleUtil.SetAttribute(extrudedAreaSolid, "ExtrudedDirection", extrudedDirection);
            IFCAnyHandleUtil.SetAttribute(extrudedAreaSolid, "Depth", depth);
            return extrudedAreaSolid;
        }

        /// <summary>
        /// Creates a handle representing an IfcSurfaceStyleRendering and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="surfaceColour">The optional surface colour.</param>
        /// <param name="transparency">The optional transparency.</param>
        /// <param name="diffuseColour">The optional diffuse colour, as a handle or normalised ratio.</param>
        /// <param name="transmissionColour">The optional transmission colour, as a handle or normalised ratio.</param>
        /// <param name="diffuseTransmissionColour">The optional diffuse transmission colour, as a handle or normalised ratio.</param>
        /// <param name="reflectionColour">The optional reflection colour, as a handle or normalised ratio.</param>
        /// <param name="specularColour">The optional specular colour, as a handle or normalised ratio.</param>
        /// <param name="specularHighlight">The optional specular highlight, as a handle or normalised ratio.</param>
        /// <param name="method">The reflectance method.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateSurfaceStyleRendering(IFCFile file, IFCAnyHandle surfaceColour,
            double? transparency, IFCData diffuseColour, 
            IFCData transmissionColour, IFCData diffuseTransmissionColour, 
            IFCData reflectionColour, IFCData specularColour, IFCData specularHighlight, IFCReflectanceMethod method)
        {
            ValidSurfaceStyleShading(surfaceColour);
            
            IFCAnyHandle surfaceStyleRendering = CreateInstance(file, IFCEntityType.IfcSurfaceStyleRendering);
            SetSurfaceStyleShading(surfaceStyleRendering, surfaceColour);
            IFCAnyHandleUtil.SetAttribute(surfaceStyleRendering, "Transparency", transparency);
            IFCAnyHandleUtil.SetAttribute(surfaceStyleRendering, "DiffuseColour", diffuseColour);
            IFCAnyHandleUtil.SetAttribute(surfaceStyleRendering, "TransmissionColour", transmissionColour);
            IFCAnyHandleUtil.SetAttribute(surfaceStyleRendering, "DiffuseTransmissionColour", diffuseTransmissionColour);
            IFCAnyHandleUtil.SetAttribute(surfaceStyleRendering, "ReflectionColour", reflectionColour);
            IFCAnyHandleUtil.SetAttribute(surfaceStyleRendering, "SpecularColour", specularColour);
            IFCAnyHandleUtil.SetAttribute(surfaceStyleRendering, "SpecularHighlight", specularHighlight);
            IFCAnyHandleUtil.SetAttribute(surfaceStyleRendering, "ReflectanceMethod", method);
            
            return surfaceStyleRendering;
        }

        /// <summary>
        /// Creates a handle representing an IfcSurfaceStyle and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="side">The side of the surface being used.</param>
        /// <param name="styles">The styles associated with the surface.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateSurfaceStyle(IFCFile file, string name, IFCSurfaceSide side, ICollection<IFCAnyHandle> styles)
        {
            IFCAnyHandleUtil.ValidateSubTypeOf(styles, false, IFCEntityType.IfcSurfaceStyleShading, IFCEntityType.IfcSurfaceStyleLighting,
                IFCEntityType.IfcSurfaceStyleRefraction, IFCEntityType.IfcSurfaceStyleWithTextures, IFCEntityType.IfcExternallyDefinedSurfaceStyle);

            IFCAnyHandle surfaceStyle = CreateInstance(file, IFCEntityType.IfcSurfaceStyle);
            SetPresentationStyle(surfaceStyle, name);
            IFCAnyHandleUtil.SetAttribute(surfaceStyle, "Side", side);
            IFCAnyHandleUtil.SetAttribute(surfaceStyle, "Styles", styles);
            return surfaceStyle;
        }

        /// <summary>
        /// Creates a handle representing an IfcHalfSpaceSoild and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="baseSurface">The clipping surface.</param>
        /// <param name="agreementFlag">True if the normal of the half space solid points away from the base extrusion, false otherwise.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateHalfSpaceSolid(IFCFile file, IFCAnyHandle baseSurface, bool agreementFlag)
        {
            ValidateHalfSpaceSolid(baseSurface);

            IFCAnyHandle halfSpaceSolidHnd = CreateInstance(file, IFCEntityType.IfcHalfSpaceSolid);
            SetHalfSpaceSolid(halfSpaceSolidHnd, baseSurface, agreementFlag);
            return halfSpaceSolidHnd;
        }

        /// <summary>
        /// Creates a handle representing an IfcBooleanClippingResult and assigns it to the file.
        /// </summary>
        /// <param name="file">The IFC file.</param>
        /// <param name="clipOperator">The clipping operator.</param>
        /// <param name="firstOperand">The handle to be clipped.</param>
        /// <param name="secondOperand">The clipping handle.</param>
        /// <returns>The IfcBooleanClippingResult handle.</returns>
        public static IFCAnyHandle CreateBooleanClippingResult(IFCFile file, IFCBooleanOperator clipOperator,
            IFCAnyHandle firstOperand, IFCAnyHandle secondOperand)
        {
            ValidateBooleanResult(firstOperand, secondOperand);

            IFCAnyHandle booleanClippingResultHnd = CreateInstance(file, IFCEntityType.IfcBooleanClippingResult);
            SetBooleanResult(booleanClippingResultHnd, clipOperator, firstOperand, secondOperand);
            return booleanClippingResultHnd;
        }

        public static IFCAnyHandle CreatePolygonalBoundedHalfSpace(IFCFile file, IFCAnyHandle position, IFCAnyHandle polygonalBoundary,
            IFCAnyHandle baseSurface, bool agreementFlag)
        {
            ValidateHalfSpaceSolid(baseSurface);
            IFCAnyHandleUtil.ValidateSubTypeOf(position, false, IFCEntityType.IfcAxis2Placement3D);
            IFCAnyHandleUtil.ValidateSubTypeOf(polygonalBoundary, false, IFCEntityType.IfcBoundedCurve);

            IFCAnyHandle polygonalBoundedHalfSpaceHnd = CreateInstance(file, IFCEntityType.IfcPolygonalBoundedHalfSpace);
            SetHalfSpaceSolid(polygonalBoundedHalfSpaceHnd, baseSurface, agreementFlag);
            IFCAnyHandleUtil.SetAttribute(polygonalBoundedHalfSpaceHnd, "Position", position);
            IFCAnyHandleUtil.SetAttribute(polygonalBoundedHalfSpaceHnd, "PolygonalBoundary", polygonalBoundary);
            return polygonalBoundedHalfSpaceHnd;
        }

        /// <summary>
        /// Creates a handle representing an IfcPlane and assigns it to the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="position">The plane coordinate system.</param>
        /// <returns>The IfcPlane handle.</returns>
        public static IFCAnyHandle CreatePlane(IFCFile file, IFCAnyHandle position)
        {
            ValidateElementarySurface(position);

            IFCAnyHandle planeHnd = CreateInstance(file, IFCEntityType.IfcPlane);
            SetElementarySurface(planeHnd, position);
            return planeHnd;
        }

        #endregion

        #region public header creation methods

        /// <summary>
        /// Creates a handle representing file schema in the header.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateFileSchema(IFCFile file)
        {
            return file.CreateHeaderInstance("file_schema");
        }

        /// <summary>
        /// Creates a handle representing file description section in the header.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="descriptions">The description strings.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateFileDescription(IFCFile file, IList<string> descriptions)
        {
            IFCAnyHandle fileDescription = file.CreateHeaderInstance("file_description");
            IFCAnyHandleUtil.SetAttribute(fileDescription, "description", descriptions);
            return fileDescription;
        }

        /// <summary>
        /// Creates a handle representing file name section in the header.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="name">The name for the file.</param>
        /// <param name="author">The author list.</param>
        /// <param name="organization">The organization list.</param>
        /// <param name="preprocessorVersion">The preprocessor version.</param>
        /// <param name="originatingSystem">The orginating system.</param>
        /// <param name="authorisation">The authorisation.</param>
        /// <returns>The handle.</returns>
        public static IFCAnyHandle CreateFileName(IFCFile file, string name, IList<string> author, IList<string> organization, string preprocessorVersion,
            string originatingSystem, string authorisation)
        {
            IFCAnyHandle fileName = file.CreateHeaderInstance("file_name");
            IFCAnyHandleUtil.SetAttribute(fileName, "name", name);
            IFCAnyHandleUtil.SetAttribute(fileName, "author", author);
            IFCAnyHandleUtil.SetAttribute(fileName, "organisation", organization);
            IFCAnyHandleUtil.SetAttribute(fileName, "preprocessor_version", preprocessorVersion);
            IFCAnyHandleUtil.SetAttribute(fileName, "originating_system", originatingSystem);
            IFCAnyHandleUtil.SetAttribute(fileName, "authorisation", authorisation);
            return fileName;
        }

        #endregion
    }
}
